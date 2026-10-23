using UnityEngine;
[RequireComponent(typeof(InteractionPromptView))]
public sealed class DoorInteractable : MonoBehaviour, IInteractable
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

    public string InteractionPrompt => _isOpen ? "" : "F";

    private void Reset()
    {
        _animator = GetComponentInChildren<Animator>();
    }

    public bool CanInteract(PlayerInventory inventory)
    {
        if (_isOpen)
            return false;

        return !_requiresKey ||
               inventory != null && inventory.HasItem(_requiredItem, _requiredAmount);
    }

    public void Interact(PlayerInventory inventory)
    {
        if (!CanInteract(inventory))
            return;

        if (_requiresKey && _consumeItem)
            inventory.RemoveItem(_requiredItem, _requiredAmount);

        _isOpen = true;
        if (_animator && _tiggerClip)
            _animator.CrossFade(_tiggerClip.name, 0.1f);
    }
}
