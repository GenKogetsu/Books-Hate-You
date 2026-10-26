using Kogetsu.Library.DesignPatternCore;
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
    [SerializeField] private bool _publishesGameClearOnOpen;

    [Header("Interaction Feedback")]
    [SerializeField, TextArea(1, 3)] private string _lockedMessage = "A key is required";

    public DoorState CurrentState { get; private set; }

    public string InteractionPrompt => _isOpen ? "" : "F";
    public string GetInteractionBlockReason(PlayerInventory inventory)
    {
        if (!_requiresKey)
            return _lockedMessage;

        int amount = inventory ? inventory.GetAmount(_requiredItem) : 0;
        int missing = Mathf.Max(0, _requiredAmount - amount);
        return $"Need {missing} more {(missing == 1 ? "key" : "keys")} ({amount}/{_requiredAmount})";
    }

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

        if (_publishesGameClearOnOpen)
        {
            PlayerRunSession.RecordWin();
            if (EventBus.Instance)
                EventBus.Instance.Publish(new GameClearEvent());
        }
    }
}
