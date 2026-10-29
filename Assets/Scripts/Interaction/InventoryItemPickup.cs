#region Imports
using UnityEngine;
#endregion
#region InventoryItemPickup

[RequireComponent(typeof(InteractionPromptView))]
public sealed class InventoryItemPickup : MonoBehaviour, IInteractable
{
    #region Pickup configuration and runtime state
    [SerializeField] private InventoryItemType _item = InventoryItemType.Key;
    [SerializeField, Min(1)] private int _amount = 1;
    [SerializeField] private string _interactionPrompt = "F";

    private string _pickupId;

    #endregion
    #region Unity lifecycle
    private void Awake()
    {
        _pickupId = BuildPickupId();
        if (PlayerRunSession.IsPickupCollected(_pickupId))
            gameObject.SetActive(false);
    }

    public string InteractionPrompt => _interactionPrompt;

    #endregion
    #region Public API
    public bool CanInteract(PlayerInventory inventory)
    {
        return inventory != null && _amount > 0;
    }

    public void Interact(PlayerInventory inventory)
    {
        if (!CanInteract(inventory))
            return;

        inventory.AddItem(_item, _amount);
        if (_item == InventoryItemType.Key)
            PlayerRunSession.RecordKeyCollected(_amount);
        PlayerRunSession.MarkPickupCollected(_pickupId);
        gameObject.SetActive(false);
    }

    #endregion
    #region Pickup identity
    private string BuildPickupId()
    {
        string id = gameObject.scene.path;
        Transform current = transform;

        while (current)
        {
            id = $"{current.GetSiblingIndex()}:{current.name}/{id}";
            current = current.parent;
        }

        return id;
    }
    #endregion
}
#endregion
