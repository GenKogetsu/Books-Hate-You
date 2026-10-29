#region Imports
using UnityEngine;
using Kogetsu.Library.Core;
using Kogetsu.Library.DesignPatternCore;
#endregion
#region LivingMovementController

public abstract class LivingMovementController : MonoBehaviour
{
    #region Fields and configuration
    private bool _isLoadingScene;
    private bool _gameCleared;

    #endregion
    #region Unity lifecycle
    protected virtual void OnEnable()
    {
        _isLoadingScene = BasicSceneEffectController.Instance &&
                          BasicSceneEffectController.Instance.IsTransitioning;
        if (EventBus.Instance)
        {
            EventBus.Instance.Subscribe<LoadSceneEvent>(OnLoadSceneEvent);
            EventBus.Instance.Subscribe<GameClearEvent>(OnGameClearEvent);
        }
    }

    protected virtual void OnDisable()
    {
        if (EventBus.Instance)
        {
            EventBus.Instance.Unsubscribe<LoadSceneEvent>(OnLoadSceneEvent);
            EventBus.Instance.Unsubscribe<GameClearEvent>(OnGameClearEvent);
        }
    }

    protected virtual void Update()
    {
        bool transitionActive = BasicSceneEffectController.Instance &&
                                BasicSceneEffectController.Instance.IsTransitioning;
        if (_isLoadingScene && !transitionActive && !_gameCleared)
            _isLoadingScene = false;
        if (!_isLoadingScene && !transitionActive && CanMove())
            UpdateMovement();
    }

    #endregion
    #region Internal helpers
    protected virtual bool CanMove() => true;

    protected abstract void UpdateMovement();

    #endregion
    #region Public API
    public virtual void StopMovement() { }

    #endregion
    #region Event handlers
    private void OnLoadSceneEvent(LoadSceneEvent _)
    {
        _isLoadingScene = true;
        StopMovement();
    }

    private void OnGameClearEvent(GameClearEvent _)
    {
        _gameCleared = true;
        _isLoadingScene = true;
        StopMovement();
    }
    #endregion
}
#endregion
