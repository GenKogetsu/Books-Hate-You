using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class AchievementToastController : MonoBehaviour
{
    [SerializeField] private AchievementDefinition[] _achievements;
    [SerializeField] private RectTransform _toast;
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private Image _icon;
    [SerializeField] private TMP_Text _fallbackIcon;
    [SerializeField] private TMP_Text _header;
    [SerializeField] private TMP_Text _title;
    [SerializeField] private TMP_Text _description;
    [SerializeField, Min(0.1f)] private float _slideDuration = 0.45f;
    [SerializeField, Min(0f)] private float _displayDuration = 5f;

    private readonly Queue<AchievementDefinition> _pending = new();
    private readonly HashSet<string> _queuedOrShown = new();
    private Vector2 _shownPosition;
    private Coroutine _playQueue;

    private void Awake()
    {
        if (!_toast)
            _toast = transform.Find("AchievementToast") as RectTransform;
        if (_toast && !_canvasGroup)
            _canvasGroup = _toast.GetComponent<CanvasGroup>();

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
            Show(achievement);

            float elapsed = 0f;
            Vector2 hiddenPosition = _shownPosition + Vector2.right * (_toast.rect.width + 48f);
            _toast.gameObject.SetActive(true);
            _toast.anchoredPosition = hiddenPosition;
            _canvasGroup.alpha = 0f;

            while (elapsed < _slideDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / _slideDuration);
                float eased = 1f - Mathf.Pow(1f - t, 3f);
                _toast.anchoredPosition = Vector2.LerpUnclamped(hiddenPosition, _shownPosition, eased);
                _canvasGroup.alpha = eased;
                yield return null;
            }

            _toast.anchoredPosition = _shownPosition;
            _canvasGroup.alpha = 1f;
            yield return new WaitForSecondsRealtime(_displayDuration);

            elapsed = 0f;
            while (elapsed < _slideDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / _slideDuration);
                float eased = t * t * (3f - 2f * t);
                _toast.anchoredPosition = Vector2.LerpUnclamped(_shownPosition, hiddenPosition, eased);
                _canvasGroup.alpha = 1f - eased;
                yield return null;
            }

            _toast.gameObject.SetActive(false);
        }

        _playQueue = null;
    }

    private void Show(AchievementDefinition achievement)
    {
        if (_header)
            _header.text = "ACHIEVEMENT UNLOCKED";
        if (_title)
            _title.text = achievement.Title;
        if (_description)
            _description.text = achievement.Description;
        if (_icon)
        {
            _icon.sprite = achievement.Icon ? achievement.Icon : _icon.sprite;
            _icon.enabled = _icon.sprite;
            _icon.color = achievement.Icon ? Color.white : new Color(1f, 0.77f, 0.25f, 1f);
        }
        if (_fallbackIcon)
        {
            _fallbackIcon.text = achievement.Icon ? string.Empty : achievement.IconGlyph;
            _fallbackIcon.gameObject.SetActive(!achievement.Icon);
        }
    }
}
