using Kogetsu.Library.DesignPatternCore;
using NaughtyAttributes;
using UnityEngine;

public sealed class WinScreenController : MonoBehaviour
{
    [SerializeField] private GameObject _winCanvas;
    [SerializeField] private Animator _animator;
    [SerializeField, ShowIf(nameof(HasAnimator))] private AnimationClip _winClip;

    private bool HasAnimator => _animator != null;

    private void Awake()
    {
        if (_winCanvas)
            _winCanvas.SetActive(false);
    }

    private void OnEnable()
    {
        if (EventBus.Instance)
            EventBus.Instance.Subscribe<GameClearEvent>(OnPlayerWin);
    }

    private void OnDisable()
    {
        if (EventBus.Instance)
            EventBus.Instance.Unsubscribe<GameClearEvent>(OnPlayerWin);
    }

    private void OnPlayerWin(GameClearEvent _)
    {
        if (_winCanvas)
            _winCanvas.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (_animator && _winClip)
        {
            _animator.speed = 1f / Mathf.Max(0.01f, _winClip.length);
            _animator.CrossFade(_winClip.name, 0f);
        }
    }
}
