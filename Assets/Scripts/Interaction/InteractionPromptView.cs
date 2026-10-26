using TMPro;
using System.Collections.Generic;
using UnityEngine;

public sealed class InteractionPromptView : MonoBehaviour
{
    private static readonly HashSet<InteractionPromptView> ActiveViews = new();
    [SerializeField] private Vector3 _offset = new(0f, 1.5f, 0f);
    [SerializeField, Min(0f)] private float _cameraOffset = 0.2f;
    private static Sprite _mobileRoundedSprite;
    private GameObject _canvasObject;
    private RectTransform _canvasRect;
    private UnityEngine.UI.Image _mobileBackground;
    private TextMeshProUGUI _text;
    private bool _mobileStyled;

    private void Awake() { CreateCanvas(); SetVisible(false); }
    private void OnEnable() { ActiveViews.Add(this); }
    private void OnDisable() { ActiveViews.Remove(this); }
    private void LateUpdate() { if (_canvasObject != null && _canvasObject.activeSelf) FaceCamera(); }

    public static IEnumerable<InteractionPromptView> AllActive => ActiveViews;

    public void SetState(string value, bool visible)
    {
        if (!visible) { SetVisible(false); return; }
        CreateCanvas();
        bool mobile = MobileControlsOverlay.IsMobileUiEnabled;
        ApplyStyle(mobile, value);
        // The contextual touch button already says USE. Avoid repeating it in the world.
        if (mobile && (value == "USE" || value == "F"))
        {
            SetVisible(false);
            return;
        }
        _text.text = value;
        _text.color = Color.white;
        SetVisible(true);
        FaceCamera();
    }

    private void CreateCanvas()
    {
        if (_canvasObject != null) return;
        _canvasObject = new GameObject("InteractionPromptCanvas");
        _canvasObject.transform.SetParent(transform, false);
        Canvas canvas = _canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace; canvas.overrideSorting = true; canvas.sortingOrder = 1000;
        _canvasRect = canvas.GetComponent<RectTransform>();
        _canvasRect.sizeDelta = new Vector2(2f, 1f); _canvasRect.localScale = Vector3.one * 0.01f;
        GameObject backgroundObject = new GameObject("MobilePromptBacking");
        backgroundObject.transform.SetParent(_canvasObject.transform, false);
        RectTransform backgroundRect = backgroundObject.AddComponent<RectTransform>();
        backgroundRect.anchorMin = Vector2.zero; backgroundRect.anchorMax = Vector2.one;
        backgroundRect.offsetMin = Vector2.zero; backgroundRect.offsetMax = Vector2.zero;
        _mobileBackground = backgroundObject.AddComponent<UnityEngine.UI.Image>();
        _mobileBackground.color = new Color(0.035f, 0.055f, 0.09f, 0.68f);
        _mobileBackground.raycastTarget = false;
        backgroundObject.SetActive(false);
        GameObject textObject = new GameObject("PromptText");
        textObject.transform.SetParent(_canvasObject.transform, false);
        RectTransform textRect = textObject.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero; textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero; textRect.offsetMax = Vector2.zero;
        _text = textObject.AddComponent<TextMeshProUGUI>();
        _text.alignment = TextAlignmentOptions.Center; _text.fontSize = 72f;
        _text.color = Color.white; _text.textWrappingMode = TextWrappingModes.NoWrap; _text.raycastTarget = false;
    }

    private void ApplyStyle(bool mobile, string value)
    {
        if (!mobile)
        {
            if (!_mobileStyled) return;
            _canvasRect.sizeDelta = new Vector2(2f, 1f);
            _text.fontSize = 72f;
            _text.enableAutoSizing = false;
            _text.textWrappingMode = TextWrappingModes.NoWrap;
            _text.margin = Vector4.zero;
            _mobileBackground.gameObject.SetActive(false);
            _mobileStyled = false;
            return;
        }

        if (!_mobileRoundedSprite) _mobileRoundedSprite = CreateRoundedSprite();
        if (_mobileBackground.sprite != _mobileRoundedSprite)
        {
            _mobileBackground.sprite = _mobileRoundedSprite;
            _mobileBackground.type = UnityEngine.UI.Image.Type.Sliced;
        }

        bool hasReason = value.Contains("\n");
        _canvasRect.sizeDelta = hasReason ? new Vector2(360f, 128f) : new Vector2(164f, 86f);
        _text.fontSize = hasReason ? 42f : 52f;
        _text.enableAutoSizing = true;
        _text.fontSizeMin = 24f;
        _text.fontSizeMax = hasReason ? 42f : 52f;
        _text.textWrappingMode = TextWrappingModes.Normal;
        _text.margin = new Vector4(12f, 6f, 12f, 6f);
        _mobileBackground.gameObject.SetActive(true);
        _mobileStyled = true;
    }

    private static Sprite CreateRoundedSprite()
    {
        const int size = 64;
        const float radius = 17f;
        var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
        {
            hideFlags = HideFlags.DontSave
        };
        var pixels = new Color32[size * size];
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            float dx = Mathf.Max(Mathf.Abs(x + 0.5f - size * 0.5f) - (size * 0.5f - radius), 0f);
            float dy = Mathf.Max(Mathf.Abs(y + 0.5f - size * 0.5f) - (size * 0.5f - radius), 0f);
            float alpha = 1f - Mathf.Clamp01(Mathf.Sqrt(dx * dx + dy * dy) - radius + 0.5f);
            pixels[y * size + x] = new Color(1f, 1f, 1f, alpha);
        }
        texture.SetPixels32(pixels);
        texture.Apply();
        Sprite sprite = Sprite.Create(texture, new Rect(0f, 0f, size, size),
            new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect,
            new Vector4(20f, 20f, 20f, 20f));
        sprite.hideFlags = HideFlags.DontSave;
        return sprite;
    }

    private void FaceCamera()
    {
        Camera camera = Camera.main; if (camera == null) return;
        Vector3 position = transform.position + _offset;
        Vector3 towardCamera = (camera.transform.position - position).normalized;
        _canvasObject.transform.position = position + towardCamera * _cameraOffset;
        _canvasObject.transform.rotation = Quaternion.LookRotation(_canvasObject.transform.position - camera.transform.position, Vector3.up);
    }

    private void SetVisible(bool visible) { if (_canvasObject != null) _canvasObject.SetActive(visible); }
}
