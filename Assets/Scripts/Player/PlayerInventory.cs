#region Imports
using UnityEngine;
#endregion
#region PlayerInventory

public sealed class PlayerInventory : MonoBehaviour
{
    [SerializeField] private PlayerInventoryData _data;

    public PlayerInventoryData Data => _data;

    public bool HasItem(InventoryItemType item, int amount = 1)
    {
        return _data && _data.HasItem(item, amount);
    }

    public int GetAmount(InventoryItemType item)
    {
        return _data ? _data.GetAmount(item) : 0;
    }

    public void AddItem(InventoryItemType item, int amount = 1)
    {
        if (_data)
            _data.AddItem(item, amount);
        else
            Debug.LogError("[PlayerInventory] ไม่ได้กำหนด PlayerInventoryData", this);
    }

    public bool RemoveItem(InventoryItemType item, int amount = 1)
    {
        return _data && _data.RemoveItem(item, amount);
    }
}
#endregion
