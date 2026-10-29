#region Imports
using TMPro;
using System.Collections.Generic;
using UnityEngine;
#endregion
#region InteractionPromptView

public sealed class InteractionPromptView : MonoBehaviour
{
    #region Prompt references and runtime state
    private static readonly HashSet<InteractionPromptView> ActiveViews = new();
    [SerializeField] private Vector3 _offset = new(0f, 1.5f, 0f);
    [SerializeField, Min(0f)] private float _cameraOffset = 0.2f;
    private static Sprite _mobileRoundedSprite;
    [SerializeField] private Canvas _canvas;
    [SerializeField] private UnityEngine.UI.Image _mobileBackground;
    [SerializeField] private UnityEngine.UI.Image _promptImage;
    [SerializeField] private TextMeshProUGUI _text;
    private RectTransform _canvasRect;
    private bool _mobileStyled;

    #endregion
    #region Unity lifecycle
    private void Awake()
    {
        if (_canvas != null) _canvasRect = _canvas.GetComponent<RectTransform>();
        SetVisible(false);
    }
    private void OnEnable() { ActiveViews.Add(this); }
    private void OnDisable() { ActiveViews.Remove(this); }
    private void LateUpdate() { if (_canvas != null && _canvas.gameObject.activeSelf) FaceCamera(); }

    public static IEnumerable<InteractionPromptView> AllActive => ActiveViews;

    #endregion
    #region Public API
    public void SetState(string value, bool visible, bool showOutOfRangeImage = false)
    {
        if (!visible) { SetVisible(false); return; }
        if (_canvas == null || _text == null) return;
        bool mobile = MobileControlsOverlay.IsMobileUiEnabled;
        ApplyStyle(mobile, value);
        bool showImage = showOutOfRangeImage && _promptImage != null && _promptImage.sprite != null;
        if (_promptImage != null) _promptImage.gameObject.SetActive(showImage);
        _text.gameObject.SetActive(!showImage);
        if (showImage)
        {
            if (_mobileBackground != null) _mobileBackground.gameObject.SetActive(false);
            if (_canvasRect != null) _canvasRect.sizeDelta = new Vector2(1.5f, 1.5f);
            SetVisible(true);
            FaceCamera();
            return;
        }
        if (!mobile && _canvasRect != null) _canvasRect.sizeDelta = new Vector2(2f, 1f);
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

    #endregion
    #region Prompt creation and presentation
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
            if (_mobileBackground != null) _mobileBackground.gameObject.SetActive(false);
            _mobileStyled = false;
            return;
        }

        if (_mobileBackground == null) return;
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
        _canvas.transform.position = position + towardCamera * _cameraOffset;
        _canvas.transform.rotation = Quaternion.LookRotation(_canvas.transform.position - camera.transform.position, Vector3.up);
    }

    private void SetVisible(bool visible) { if (_canvas != null) _canvas.gameObject.SetActive(visible); }
    #endregion
}
#endregion
