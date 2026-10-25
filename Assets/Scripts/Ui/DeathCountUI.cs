using TMPro;
using UnityEngine;

public sealed class DeathCountUI : MonoBehaviour
{
    [SerializeField] private TMP_Text _countText;
    [SerializeField] private string _format = "Deaths: {0}";
    [SerializeField, Min(0.1f)] private float _blinkCycleDuration = 1f;

    private CanvasGroup _canvasGroup;
    private Coroutine _blinkRoutine;

    private void Awake()
    {
        if (!_countText)
            _countText = GetComponent<TMP_Text>();

        _canvasGroup = GetComponent<CanvasGroup>();
        if (!_canvasGroup)
            _canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    private void OnEnable()
    {
        PlayerRunSession.DeathCountChanged += OnDeathCountChanged;
        Refresh(PlayerRunSession.DeathCount);
    }

    private void OnDisable()
    {
        PlayerRunSession.DeathCountChanged -= OnDeathCountChanged;
        if (_blinkRoutine != null)
            StopCoroutine(_blinkRoutine);
        _blinkRoutine = null;
    }

    private void Refresh(int deathCount)
    {
        if (_countText)
            _countText.text = string.Format(_format, deathCount);

        if (_canvasGroup)
            _canvasGroup.alpha = deathCount > 0 ? 1f : 0f;
    }

    private void OnDeathCountChanged(int deathCount)
    {
        Refresh(deathCount);

        if (deathCount <= 0 || !isActiveAndEnabled)
            return;

        if (_blinkRoutine != null)
            StopCoroutine(_blinkRoutine);
        _blinkRoutine = StartCoroutine(BlinkThreeTimes());
    }

    private System.Collections.IEnumerator BlinkThreeTimes()
    {
        float halfCycle = _blinkCycleDuration * 0.5f;
        for (int blink = 0; blink < 3; blink++)
        {
            yield return FadeTo(0f, halfCycle);
            yield return FadeTo(1f, halfCycle);
        }

        _canvasGroup.alpha = 1f;
        _blinkRoutine = null;
    }

    private System.Collections.IEnumerator FadeTo(float targetAlpha, float duration)
    {
        float startAlpha = _canvasGroup.alpha;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            _canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, Mathf.Clamp01(elapsed / duration));
            yield return null;
        }

        _canvasGroup.alpha = targetAlpha;
    }
}
