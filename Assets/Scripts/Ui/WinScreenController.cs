#region Imports
using Kogetsu.Library.DesignPatternCore;
using NaughtyAttributes;
using UnityEngine;
#endregion
#region WinScreenController

public sealed class WinScreenController : MonoBehaviour
{
    #region Fields and configuration
    [SerializeField] private GameObject _winCanvas;
    [SerializeField] private Animator _animator;

    #endregion
    #region Unity lifecycle
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

    #endregion
    #region Event handlers
    private void OnPlayerWin(GameClearEvent _)
    {
        if (_winCanvas)
            _winCanvas.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (_animator)
        {
            _animator.speed = 1f;
            _animator.Play("Reveal", 0, 0f);
        }
    }

    #endregion
    #region Public API
    public void PublishWin()
    {
        PlayerRunSession.RecordWin();
        if (EventBus.Instance)
            EventBus.Instance.Publish(new GameClearEvent());
    }
    #endregion
}
#endregion
