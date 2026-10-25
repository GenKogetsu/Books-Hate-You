using TMPro;
using UnityEngine;

public sealed class KeyInventoryUI : MonoBehaviour
{
    [SerializeField] private PlayerInventoryData _inventoryData;
    [SerializeField] private TMPro.TMP_Text _amountText;
    [SerializeField] private InventoryItemType _item = InventoryItemType.Key;
    [SerializeField] private string _format = "X {0}";

    private void Awake()
    {
        if (!_amountText)
            _amountText = GetComponentInChildren<TMP_Text>(true);
    }

    private void OnEnable()
    {
        if (_inventoryData)
            _inventoryData.AmountChanged += OnAmountChanged;

        Refresh();
    }

    private void OnDisable()
    {
        if (_inventoryData)
            _inventoryData.AmountChanged -= OnAmountChanged;
    }

    private void OnAmountChanged(InventoryItemType item, int amount)
    {
        if (item == _item)
            Refresh(amount);
    }

    private void Refresh()
    {
        if (_inventoryData)
            Refresh(_inventoryData.GetAmount(_item));
    }

    private void Refresh(int amount)
    {
        if (_amountText)
            _amountText.text = string.Format(_format, amount);
    }
}
