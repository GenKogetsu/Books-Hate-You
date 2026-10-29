#region Imports
using System;
using System.Collections.Generic;
using UnityEngine;
#endregion
#region PlayerInventoryData

[CreateAssetMenu(fileName = "PlayerInventoryData", menuName = "Player/Inventory Data")]
public sealed class PlayerInventoryData : ScriptableObject
{
    [Serializable]
    private sealed class InventoryEntry
    {
        public InventoryItemType Item;
        [Min(0)] public int Amount;
    }

    [SerializeField] private List<InventoryEntry> _items = new();
    [NonSerialized] private Dictionary<InventoryItemType, int> _runtimeItems;

    public event Action<InventoryItemType, int> AmountChanged;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetAllRuntimeInventories()
    {
        foreach (PlayerInventoryData data in Resources.FindObjectsOfTypeAll<PlayerInventoryData>())
            data.ResetRuntimeItems();
    }

    private void OnEnable() => ResetRuntimeItems();

    public bool HasItem(InventoryItemType item, int amount = 1)
    {
        return GetAmount(item) >= Mathf.Max(0, amount);
    }

    public int GetAmount(InventoryItemType item)
    {
        EnsureRuntimeItems();
        return _runtimeItems.TryGetValue(item, out int amount) ? amount : 0;
    }

    public void AddItem(InventoryItemType item, int amount = 1)
    {
        if (amount <= 0)
            return;

        EnsureRuntimeItems();
        int newAmount = GetAmount(item) + amount;
        _runtimeItems[item] = newAmount;
        AmountChanged?.Invoke(item, newAmount);
    }

    public bool RemoveItem(InventoryItemType item, int amount = 1)
    {
        if (amount <= 0 || !HasItem(item, amount))
            return false;

        int newAmount = GetAmount(item) - amount;
        _runtimeItems[item] = newAmount;
        AmountChanged?.Invoke(item, newAmount);
        return true;
    }

    public void ClearRuntimeItems()
    {
        EnsureRuntimeItems();
        if (_runtimeItems.Count == 0)
            return;

        InventoryItemType[] items = new InventoryItemType[_runtimeItems.Count];
        _runtimeItems.Keys.CopyTo(items, 0);
        _runtimeItems.Clear();

        foreach (InventoryItemType item in items)
            AmountChanged?.Invoke(item, 0);
    }

    private void EnsureRuntimeItems()
    {
        if (_runtimeItems == null)
            ResetRuntimeItems();
    }

    private void ResetRuntimeItems()
    {
        _runtimeItems = new Dictionary<InventoryItemType, int>();
        foreach (InventoryEntry entry in _items)
        {
            if (entry != null && entry.Amount > 0)
                _runtimeItems[entry.Item] = entry.Amount;
        }
    }
}

#endregion
