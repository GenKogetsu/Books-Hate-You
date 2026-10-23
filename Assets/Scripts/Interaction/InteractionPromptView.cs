using TMPro;
using System.Collections.Generic;
using UnityEngine;

public sealed class InteractionPromptView : MonoBehaviour
{
    private static readonly HashSet<InteractionPromptView> ActiveViews = new();
    [SerializeField] private Vector3 _offset = new(0f, 1.5f, 0f);
    [SerializeField, Min(0f)] private float _cameraOffset = 0.2f;
    private GameObject _canvasObject;
    private TextMeshProUGUI _text;

    private void Awake() { CreateCanvas(); SetVisible(false); }
    private void OnEnable() { ActiveViews.Add(this); }
    private void OnDisable() { ActiveViews.Remove(this); }
    private void LateUpdate() { if (_canvasObject != null && _canvasObject.activeSelf) FaceCamera(); }

    public static IEnumerable<InteractionPromptView> AllActive => ActiveViews;

    public void SetState(string value, bool visible)
    {
        if (!visible) { SetVisible(false); return; }
        CreateCanvas(); _text.text = value; _text.color = Color.white; SetVisible(true); FaceCamera();
    }

    private void CreateCanvas()
    {
        if (_canvasObject != null) return;
        _canvasObject = new GameObject("InteractionPromptCanvas");
        _canvasObject.transform.SetParent(transform, false);
        Canvas canvas = _canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace; canvas.overrideSorting = true; canvas.sortingOrder = 1000;
        RectTransform canvasRect = canvas.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(2f, 1f); canvasRect.localScale = Vector3.one * 0.01f;
        GameObject textObject = new GameObject("PromptText");
        textObject.transform.SetParent(_canvasObject.transform, false);
        RectTransform textRect = textObject.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero; textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero; textRect.offsetMax = Vector2.zero;
        _text = textObject.AddComponent<TextMeshProUGUI>();
        _text.alignment = TextAlignmentOptions.Center; _text.fontSize = 72f;
        _text.color = Color.white; _text.textWrappingMode = TextWrappingModes.NoWrap; _text.raycastTarget = false;
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
