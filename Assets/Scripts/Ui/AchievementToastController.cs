using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class AchievementToastController : MonoBehaviour
{
    [SerializeField] private AchievementDefinition[] _achievements;
    [SerializeField] private RectTransform _toast;
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private Animator _animator;
    [SerializeField, Min(0.1f)] private float _slideDuration = 0.45f;
    [SerializeField, Min(0f)] private float _displayDuration = 5f;

    private readonly Queue<AchievementDefinition> _pending = new();
    private readonly HashSet<string> _queuedOrShown = new();
    private Vector2 _shownPosition;
    private Coroutine _playQueue;

    private const string SlideInState = "SlideIn";
    private const string VisibleState = "Visible";
    private const string SlideOutState = "SlideOut";

    private void Awake()
    {
        if (!_toast)
            _toast = transform.Find("AchievementToast") as RectTransform;
        if (_toast && !_canvasGroup)
            _canvasGroup = _toast.GetComponent<CanvasGroup>();
        if (_toast && !_animator)
            _animator = _toast.GetComponent<Animator>();

        if (_toast)
        {
            _shownPosition = _toast.anchoredPosition;
            _toast.gameObject.SetActive(false);
        }
    }

    private void OnEnable()
    {
        PlayerRunSession.AchievementUnlocked += EnqueueAchievement;
    }

    private void OnDisable()
    {
        PlayerRunSession.AchievementUnlocked -= EnqueueAchievement;
        if (_playQueue != null)
            StopCoroutine(_playQueue);
        _playQueue = null;
    }

    private void EnqueueAchievement(string id)
    {
        if (!_toast || _queuedOrShown.Contains(id))
            return;

        AchievementDefinition definition = FindDefinition(id);
        if (!definition)
        {
            Debug.LogWarning($"[AchievementToast] No AchievementDefinition found for '{id}'.", this);
            return;
        }

        _queuedOrShown.Add(id);
        _pending.Enqueue(definition);
        if (_playQueue == null)
            _playQueue = StartCoroutine(PlayQueue());
    }

    private AchievementDefinition FindDefinition(string id)
    {
        if (_achievements == null)
            return null;

        foreach (AchievementDefinition achievement in _achievements)
        {
            if (achievement && achievement.Id == id)
                return achievement;
        }

        return null;
    }

    private IEnumerator PlayQueue()
    {
        while (_pending.Count > 0)
        {
            AchievementDefinition achievement = _pending.Dequeue();
            PlayState(achievement, "Hidden");

            _toast.gameObject.SetActive(true);
            PlayState(achievement, SlideInState);
            yield return new WaitForSecondsRealtime(_slideDuration);
            PlayState(achievement, VisibleState);
            yield return new WaitForSecondsRealtime(_displayDuration);
            PlayState(achievement, SlideOutState);
            yield return new WaitForSecondsRealtime(_slideDuration);
            _toast.gameObject.SetActive(false);
        }

        _playQueue = null;
    }

    private void PlayState(AchievementDefinition achievement, string state)
    {
        if (_animator && _animator.runtimeAnimatorController)
            _animator.Play($"{achievement.Id}.{state}", 0, 0f);
    }
}
