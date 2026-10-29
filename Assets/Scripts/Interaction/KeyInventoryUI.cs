#region Imports
using TMPro;
using UnityEngine;
#endregion
#region KeyInventoryUI

public sealed class KeyInventoryUI : MonoBehaviour
{
    #region Inventory display references
    [SerializeField] private PlayerInventoryData _inventoryData;
    [SerializeField] private TMPro.TMP_Text _amountText;
    [SerializeField] private CanvasGroup _visibilityGroup;
    [SerializeField] private InventoryItemType _item = InventoryItemType.Key;
    [SerializeField, Min(1)] private int _displayMaximum = 5;
    [SerializeField] private string _format = "{0}/{1}";

    #endregion
    #region Unity lifecycle
    private void Awake()
    {
        if (!_amountText)
            _amountText = GetComponentInChildren<TMP_Text>(true);

        if (!_visibilityGroup)
            _visibilityGroup = GetComponent<CanvasGroup>();

        if (!_visibilityGroup)
            _visibilityGroup = gameObject.AddComponent<CanvasGroup>();
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

    #endregion
    #region Inventory display updates
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
        bool visible = amount > 0;

        if (_amountText)
            _amountText.text = string.Format(_format, amount, _displayMaximum);

        if (_visibilityGroup)
        {
            _visibilityGroup.alpha = visible ? 1f : 0f;
            _visibilityGroup.interactable = visible;
            _visibilityGroup.blocksRaycasts = visible;
        }
    }
    #endregion
}
#endregion
