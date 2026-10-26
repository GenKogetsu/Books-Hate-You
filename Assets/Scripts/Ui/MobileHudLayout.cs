using UnityEngine;

/// <summary>Applies a compact, safe-area-aware layout to the existing HUD on phones.</summary>
[DefaultExecutionOrder(-900)]
public sealed class MobileHudLayout : MonoBehaviour
{
    [SerializeField] private RectTransform _quest;
    [SerializeField] private RectTransform _key;
    [SerializeField] private RectTransform _death;
    [SerializeField] private RectTransform _deathBacking;
    [SerializeField] private RectTransform _stamina;
    [SerializeField] private RectTransform _toast;
    [SerializeField] private UnityEngine.UI.Image _questImage;
    [SerializeField] private UnityEngine.UI.Image _toastImage;
    [SerializeField] private UnityEngine.UI.Image _keyBackingImage;
    [SerializeField] private UnityEngine.UI.Image _deathBackingImage;
    [SerializeField] private UnityEngine.UI.Image _restartImage;
    [SerializeField] private UnityEngine.UI.Image _menuImage;
    [SerializeField] private Sprite _roundedSprite;

    private Canvas _canvas;
    private Rect _lastSafeArea;
    private Vector2Int _lastScreenSize;
    private bool _wasMobile;
    private RectState[] _desktopRects;
    private ImageState[] _desktopImages;

    private struct RectState
    {
        public RectTransform Rect;
        public Vector2 Min;
        public Vector2 Max;
        public Vector2 Pivot;
        public Vector2 Position;
        public Vector2 Size;
    }

    private struct ImageState
    {
        public UnityEngine.UI.Image Image;
        public Color Color;
        public Sprite Sprite;
        public UnityEngine.UI.Image.Type Type;
    }

    public void Configure(RectTransform quest, RectTransform key, RectTransform death,
        RectTransform deathBacking, RectTransform stamina, RectTransform toast,
        UnityEngine.UI.Image questImage, UnityEngine.UI.Image toastImage,
        UnityEngine.UI.Image keyBackingImage, UnityEngine.UI.Image deathBackingImage,
        UnityEngine.UI.Image restartImage, UnityEngine.UI.Image menuImage, Sprite roundedSprite)
    {
        _quest = quest;
        _key = key;
        _death = death;
        _deathBacking = deathBacking;
        _stamina = stamina;
        _toast = toast;
        _questImage = questImage;
        _toastImage = toastImage;
        _keyBackingImage = keyBackingImage;
        _deathBackingImage = deathBackingImage;
        _restartImage = restartImage;
        _menuImage = menuImage;
        _roundedSprite = roundedSprite;
    }

    public void SetEndScreenVisibility(bool hide)
    {
        bool showBackings = MobileControlsOverlay.IsMobileUiEnabled && !hide;
        if (_keyBackingImage && _keyBackingImage.gameObject.activeSelf != showBackings)
            _keyBackingImage.gameObject.SetActive(showBackings);
        if (_deathBackingImage && _deathBackingImage.gameObject.activeSelf != showBackings)
            _deathBackingImage.gameObject.SetActive(showBackings);
    }

    private void Awake()
    {
        _canvas = GetComponent<Canvas>();
        _desktopRects = new[]
        {
            Capture(_quest), Capture(_key), Capture(_death), Capture(_deathBacking),
            Capture(_stamina), Capture(_toast)
        };
        _desktopImages = new[]
        {
            Capture(_questImage), Capture(_toastImage), Capture(_restartImage), Capture(_menuImage)
        };
        Refresh();
    }

    private void Update()
    {
        bool mobile = MobileControlsOverlay.IsMobileUiEnabled;
        if (mobile != _wasMobile || (mobile &&
            (_lastSafeArea != Screen.safeArea || _lastScreenSize.x != Screen.width ||
             _lastScreenSize.y != Screen.height)))
            Refresh();

        if (mobile && _deathBackingImage && _death)
        {
            CanvasGroup source = _death.GetComponent<CanvasGroup>();
            CanvasGroup backing = _deathBackingImage.GetComponent<CanvasGroup>();
            if (source && backing) backing.alpha = source.alpha;
        }
    }

    private void Refresh()
    {
        bool mobile = MobileControlsOverlay.IsMobileUiEnabled;
        if (!mobile)
        {
            if (_wasMobile)
            {
                foreach (RectState state in _desktopRects) Restore(state);
                foreach (ImageState state in _desktopImages) Restore(state);
            }
            if (_keyBackingImage) _keyBackingImage.gameObject.SetActive(false);
            if (_deathBackingImage) _deathBackingImage.gameObject.SetActive(false);
            _wasMobile = false;
            return;
        }

        float scale = GetCanvasScale();
        Rect safe = Screen.safeArea;
        float left = safe.xMin / scale;
        float right = (Screen.width - safe.xMax) / scale;
        float top = (Screen.height - safe.yMax) / scale;
        float bottom = safe.yMin / scale;

        Place(_quest, Vector2.up, new Vector2(0f, 1f), new Vector2(left + 24f, -top - 20f), new Vector2(440f, 104f));
        Place(_key, Vector2.up, new Vector2(0f, 1f), new Vector2(left + 24f, -top - 136f), new Vector2(100f, 100f));
        Place(_death, Vector2.one, Vector2.one, new Vector2(-right - 24f, -top - 18f), new Vector2(240f, 64f));
        Place(_deathBacking, Vector2.one, Vector2.one, new Vector2(-right - 24f, -top - 18f), new Vector2(240f, 64f));
        Place(_stamina, new Vector2(0.5f, 0f), new Vector2(0.5f, 0.5f), new Vector2(0f, bottom + 240f), new Vector2(420f, 18f));
        Place(_toast, Vector2.one, Vector2.one, new Vector2(-right - 24f, -top - 100f), new Vector2(520f, 140f));

        Style(_questImage, new Color(0.035f, 0.055f, 0.09f, 0.67f));
        Style(_toastImage, new Color(0.045f, 0.06f, 0.1f, 0.78f));
        Style(_restartImage, new Color(0.72f, 0.85f, 1f, 0.4f));
        Style(_menuImage, new Color(0.72f, 0.85f, 1f, 0.4f));

        MobileControlsOverlay overlay = GetComponent<MobileControlsOverlay>();
        SetEndScreenVisibility(overlay && overlay.IsEnded);
        _wasMobile = true;
        _lastSafeArea = safe;
        _lastScreenSize = new Vector2Int(Screen.width, Screen.height);
    }

    private void Style(UnityEngine.UI.Image image, Color color)
    {
        if (!image) return;
        if (_roundedSprite)
        {
            image.sprite = _roundedSprite;
            image.type = UnityEngine.UI.Image.Type.Sliced;
        }
        image.color = color;
    }

    private float GetCanvasScale()
    {
        UnityEngine.UI.CanvasScaler scaler = GetComponent<UnityEngine.UI.CanvasScaler>();
        if (scaler && scaler.uiScaleMode == UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize &&
            scaler.screenMatchMode == UnityEngine.UI.CanvasScaler.ScreenMatchMode.MatchWidthOrHeight &&
            scaler.referenceResolution.x > 0f && scaler.referenceResolution.y > 0f &&
            Screen.width > 0 && Screen.height > 0)
        {
            float width = Mathf.Log(Screen.width / scaler.referenceResolution.x, 2f);
            float height = Mathf.Log(Screen.height / scaler.referenceResolution.y, 2f);
            return Mathf.Max(0.001f, Mathf.Pow(2f, Mathf.Lerp(width, height, scaler.matchWidthOrHeight)));
        }
        return _canvas ? Mathf.Max(0.001f, _canvas.scaleFactor) : 1f;
    }

    private static void Place(RectTransform rect, Vector2 anchor, Vector2 pivot, Vector2 position, Vector2 size)
    {
        if (!rect) return;
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = pivot;
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
    }

    private static RectState Capture(RectTransform rect) => new RectState
    {
        Rect = rect,
        Min = rect ? rect.anchorMin : Vector2.zero,
        Max = rect ? rect.anchorMax : Vector2.zero,
        Pivot = rect ? rect.pivot : Vector2.zero,
        Position = rect ? rect.anchoredPosition : Vector2.zero,
        Size = rect ? rect.sizeDelta : Vector2.zero
    };

    private static ImageState Capture(UnityEngine.UI.Image image) => new ImageState
    {
        Image = image,
        Color = image ? image.color : Color.white,
        Sprite = image ? image.sprite : null,
        Type = image ? image.type : UnityEngine.UI.Image.Type.Simple
    };

    private static void Restore(RectState state)
    {
        if (!state.Rect) return;
        state.Rect.anchorMin = state.Min;
        state.Rect.anchorMax = state.Max;
        state.Rect.pivot = state.Pivot;
        state.Rect.anchoredPosition = state.Position;
        state.Rect.sizeDelta = state.Size;
    }

    private static void Restore(ImageState state)
    {
        if (!state.Image) return;
        state.Image.color = state.Color;
        state.Image.sprite = state.Sprite;
        state.Image.type = state.Type;
    }
}
