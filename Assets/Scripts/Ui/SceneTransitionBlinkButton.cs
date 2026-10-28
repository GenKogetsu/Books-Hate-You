using System.Collections;
using Kogetsu.Library.Core;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button), typeof(CanvasGroup))]
public sealed class SceneTransitionBlinkButton : MonoBehaviour
{
    public enum Destination { Restart, MainMenu }

    [SerializeField] private Destination _destination;
    [SerializeField, Min(0.01f)] private float _blinkDuration = 0.45f;
    [SerializeField, Min(1)] private int _blinkCount = 3;

    private Button _button;
    private CanvasGroup _group;
    private bool _pressed;

    public void Configure(Destination destination) => _destination = destination;

    private void Awake()
    {
        _button = GetComponent<Button>();
        _group = GetComponent<CanvasGroup>();
    }

    public void Begin()
    {
        if (_pressed || !BasicSceneEffectController.Instance ||
            BasicSceneEffectController.Instance.IsTransitioning) return;
        LoadPublisher publisher = GetComponent<LoadPublisher>();
        if (!publisher) return;
        if (_destination == Destination.Restart)
            publisher.ReloadSceneEvent();
        else
            publisher.PublishLoadSceneEvent(0);
        if (!BasicSceneEffectController.Instance.IsTransitioning) return;

        _pressed = true;
        Canvas canvas = GetComponent<Canvas>();
        if (canvas)
        {
            canvas.overrideSorting = true;
            canvas.sortingOrder = 101;
        }
        _button.transition = Selectable.Transition.None;
        _button.interactable = false;
        if (_button.targetGraphic) _button.targetGraphic.color = Color.white;
        StartCoroutine(Blink());
    }

    private IEnumerator Blink()
    {
        float half = _blinkDuration * 0.5f;
        for (int i = 0; i < _blinkCount; i++)
        {
            yield return FadeTo(0f, half);
            yield return FadeTo(1f, half);
        }

    }

    private IEnumerator FadeTo(float target, float duration)
    {
        float start = _group.alpha;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            _group.alpha = Mathf.Lerp(start, target, Mathf.Clamp01(elapsed / duration));
            yield return null;
        }
        _group.alpha = target;
    }
}
