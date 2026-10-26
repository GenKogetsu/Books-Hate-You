using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public sealed class MobileSafeAreaFitter : MonoBehaviour
{
    private Rect _lastSafeArea;
    private Vector2Int _lastScreenSize;
    private RectTransform _rect;

    private void Awake() => _rect = (RectTransform)transform;
    private void OnEnable() => Apply();

    private void Update()
    {
        if (_lastSafeArea != Screen.safeArea ||
            _lastScreenSize.x != Screen.width || _lastScreenSize.y != Screen.height)
            Apply();
    }

    private void Apply()
    {
        if (!_rect) _rect = (RectTransform)transform;
        if (Screen.width <= 0 || Screen.height <= 0) return;
        Rect area = Screen.safeArea;
        _rect.anchorMin = new Vector2(area.xMin / Screen.width, area.yMin / Screen.height);
        _rect.anchorMax = new Vector2(area.xMax / Screen.width, area.yMax / Screen.height);
        _rect.offsetMin = Vector2.zero;
        _rect.offsetMax = Vector2.zero;
        _lastSafeArea = area;
        _lastScreenSize = new Vector2Int(Screen.width, Screen.height);
    }
}
