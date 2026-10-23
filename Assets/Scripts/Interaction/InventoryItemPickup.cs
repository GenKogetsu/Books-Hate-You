using UnityEngine;

/// <summary>
/// Collectible item that adds an amount to the player's inventory.
/// </summary>
[RequireComponent(typeof(InteractionPromptView))]
public sealed class InventoryItemPickup : MonoBehaviour, IInteractable
{
    [SerializeField] private InventoryItemType _item = InventoryItemType.Key;
    [SerializeField, Min(1)] private int _amount = 1;
    [SerializeField] private string _interactionPrompt = "F";

    public string InteractionPrompt => _interactionPrompt;

    public bool CanInteract(PlayerInventory inventory)
    {
        return inventory != null && _amount > 0;
    }

    public void Interact(PlayerInventory inventory)
    {
        if (!CanInteract(inventory))
            return;

        inventory.AddItem(_item, _amount);
        gameObject.SetActive(false);
    }
}
