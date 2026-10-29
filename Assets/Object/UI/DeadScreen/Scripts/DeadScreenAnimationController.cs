#region Imports
using UnityEngine;
using NaughtyAttributes;
using Kogetsu.Library.DesignPatternCore;
#endregion
#region DeadScreenAnimationController

public class DeadScreenAnimationController : MonoBehaviour
{
    #region Fields and configuration
    [SerializeField, Required] private Animator _animator;
    private CanvasGroup _canvasGroup;

    #endregion
    #region Unity lifecycle
    private void Awake()
    {
        if (!_animator)
            _animator = GetComponent<Animator>();
        _canvasGroup = GetComponent<CanvasGroup>();
    }

    private void OnEnable()
    {
        if (_animator)
            _animator.enabled = false;
        if (_canvasGroup)
        {
            _canvasGroup.alpha = 0f;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
        }

        if (EventBus.Instance)
        {
            EventBus.Instance.Subscribe<GameoverEvent>(OnGameoverEvent);
        }
    }

    private void OnDisable()
    {
        if (EventBus.Instance)
        {
            EventBus.Instance.Unsubscribe<GameoverEvent>(OnGameoverEvent);
        }
    }

    #endregion
    #region Event handlers
    private void OnGameoverEvent(GameoverEvent e)
    {
        if (!_animator) return;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        if (_canvasGroup)
        {
            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;
        }
        _animator.enabled = true;
        _animator.speed = 1f;
        _animator.Play("Reveal", 0, 0f);
    }
    #endregion
}
#endregion
