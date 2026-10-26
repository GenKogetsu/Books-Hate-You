using UnityEngine;

[RequireComponent(typeof(UnityEngine.UI.Button))]
public sealed class MobileZoomButton : MonoBehaviour
{
    [SerializeField] private float _direction = 1f;
    private UnityEngine.UI.Button _button;
    private CameraController _camera;

    public void Configure(float direction) => _direction = direction;

    private void Awake() => _button = GetComponent<UnityEngine.UI.Button>();
    private void OnEnable()
    {
        if (!_button) _button = GetComponent<UnityEngine.UI.Button>();
        _button.onClick.AddListener(Zoom);
    }

    private void OnDisable()
    {
        if (_button) _button.onClick.RemoveListener(Zoom);
    }

    private void Zoom()
    {
#if UNITY_ANDROID || UNITY_IOS || UNITY_EDITOR
        if (!_camera) _camera = FindAnyObjectByType<CameraController>();
        if (_camera) _camera.ZoomForMobile(_direction);
#endif
    }
}
