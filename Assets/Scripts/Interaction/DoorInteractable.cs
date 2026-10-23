using UnityEngine;
[RequireComponent(typeof(InteractionPromptView))]
public sealed class DoorInteractable : MonoBehaviour, IInteractable, IInteractionBlockReason
{
    [Header("Door")]
    [SerializeField] private Animator _animator;
    [SerializeField] private AnimationClip _tiggerClip;
    [SerializeField] private bool _isOpen;

    [Header("Key")]
    [SerializeField] private bool _requiresKey;
    [SerializeField] private InventoryItemType _requiredItem = InventoryItemType.Key;
    [SerializeField, Min(1)] private int _requiredAmount = 1;
    [SerializeField] private bool _consumeItem = true;

    [Header("Interaction Feedback")]
    [SerializeField, TextArea(1, 3)] private string _lockedMessage = "ต้องใช้กุญแจ";

    public DoorState CurrentState { get; private set; }

    public string InteractionPrompt => _isOpen ? "" : "F";
    public string InteractionBlockReason => _lockedMessage;

    private void Reset()
    {
        _animator = GetComponentInChildren<Animator>();
    }

    private void Awake()
    {
        CurrentState = _isOpen ? DoorState.Open : DoorState.Locked;
    }

    public bool CanInteract(PlayerInventory inventory)
    {
        if (_isOpen)
            return false;

        bool canOpen = !_requiresKey ||
                       inventory != null && inventory.HasItem(_requiredItem, _requiredAmount);
        CurrentState = canOpen ? DoorState.Ready : DoorState.Locked;
        return canOpen;
    }

    public void Interact(PlayerInventory inventory)
    {
        if (!CanInteract(inventory))
            return;

        if (_requiresKey && _consumeItem)
            inventory.RemoveItem(_requiredItem, _requiredAmount);

        _isOpen = true;
        CurrentState = DoorState.Opening;
        if (_animator && _tiggerClip)
            _animator.CrossFade(_tiggerClip.name, 0.1f);

        if (!_animator || !_tiggerClip)
            CurrentState = DoorState.Open;
    }
}
