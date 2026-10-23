using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class PlayerInventory : MonoBehaviour
{
    [Serializable]
    private class InventoryEntry
    {
        public InventoryItemType Item;
        [Min(0)] public int Amount;
    }

    [Header("Items")]
    [SerializeField] private List<InventoryEntry> _items = new();

    public bool HasItem(InventoryItemType item, int amount = 1)
    {
        return GetAmount(item) >= Mathf.Max(0, amount);
    }

    public int GetAmount(InventoryItemType item)
    {
        InventoryEntry entry = FindEntry(item);
        return entry?.Amount ?? 0;
    }

    public void AddItem(InventoryItemType item, int amount = 1)
    {
        if (amount <= 0)
            return;

        InventoryEntry entry = FindEntry(item);
        if (entry == null)
        {
            entry = new InventoryEntry { Item = item };
            _items.Add(entry);
        }

        entry.Amount += amount;
    }

    public bool RemoveItem(InventoryItemType item, int amount = 1)
    {
        if (!HasItem(item, amount) || amount <= 0)
            return false;

        InventoryEntry entry = FindEntry(item);
        entry.Amount -= amount;
        return true;
    }

    private InventoryEntry FindEntry(InventoryItemType item)
    {
        return _items.Find(entry => entry.Item == item);
    }
}
