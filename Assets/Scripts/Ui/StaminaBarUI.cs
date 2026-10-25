using UnityEngine;

[RequireComponent(typeof(UnityEngine.UI.Image))]
public sealed class StaminaBarUI : MonoBehaviour
{
    [SerializeField, Min(1f)] private float _fullWidth = 420f;
    [SerializeField, Min(1f)] private float _barHeight = 18f;
    [SerializeField, Range(0f, 1f)] private float _lowThreshold = 0.25f;

    private static readonly Color Blue = new Color(0.06f, 0.83f, 1f, 1f);
    private static readonly Color Red = new Color(1f, 0.12f, 0.18f, 1f);
    private PlayerStatus _playerStatus;
    private UnityEngine.UI.Image _image;
    private RectTransform _rect;
    private Vector2 _restPosition;
    private int _lastDeniedVersion;
    private float _shakeRemaining;

    private void Awake()
    {
        _image = GetComponent<UnityEngine.UI.Image>();
        _rect = (RectTransform)transform;
        _restPosition = _rect.anchoredPosition;
        _image.raycastTarget = false;
        _image.enabled = false;
        _playerStatus = FindAnyObjectByType<PlayerStatus>();
        if (_playerStatus)
            _lastDeniedVersion = _playerStatus.GetDeniedStaminaFeedbackVersion();
    }

    private void Update()
    {
        if (!_playerStatus)
        {
            _playerStatus = FindAnyObjectByType<PlayerStatus>();
            if (!_playerStatus) return;
            _lastDeniedVersion = _playerStatus.GetDeniedStaminaFeedbackVersion();
        }

        int deniedVersion = _playerStatus.GetDeniedStaminaFeedbackVersion();
        if (deniedVersion != _lastDeniedVersion)
        {
            _lastDeniedVersion = deniedVersion;
            _shakeRemaining = 0.45f;
        }

        float stamina = Mathf.Clamp01(_playerStatus.GetStaminaFraction());
        bool running = _playerStatus.GetIsRunning();
        bool shaking = _shakeRemaining > 0f;
        _image.enabled = stamina < 0.999f || shaking;
        if (!_image.enabled)
        {
            _rect.anchoredPosition = _restPosition;
            return;
        }

        // A centered RectTransform shrinks its two outer edges toward the middle.
        _rect.sizeDelta = new Vector2(Mathf.Max(18f, _fullWidth * stamina), _barHeight);

        Color color = Color.Lerp(Red, Blue, Mathf.InverseLerp(0.08f, _lowThreshold + 0.1f, stamina));
        if (_playerStatus.GetIsSprintLocked() || shaking) color = Red;
        if (running && stamina <= _lowThreshold)
            color.a = Mathf.Lerp(0.35f, 1f, (Mathf.Sin(Time.unscaledTime * 18f) + 1f) * 0.5f);
        _image.color = color;

        if (shaking)
        {
            _shakeRemaining = Mathf.Max(0f, _shakeRemaining - Time.unscaledDeltaTime);
            float strength = _shakeRemaining / 0.45f;
            _rect.anchoredPosition = _restPosition + new Vector2(
                Mathf.Sin(Time.unscaledTime * 95f) * 9f * strength,
                Mathf.Cos(Time.unscaledTime * 73f) * 3f * strength);
        }
        else
        {
            _rect.anchoredPosition = _restPosition;
        }
    }

    private void OnDisable()
    {
        if (_rect) _rect.anchoredPosition = _restPosition;
    }
}
