using UnityEngine;
using Kogetsu.Library.Core;
using Kogetsu.Library.DesignPatternCore;

public abstract class LivingMovementController : MonoBehaviour
{
    private bool _isLoadingScene;
    private bool _gameCleared;

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

    protected virtual bool CanMove() => true;

    protected abstract void UpdateMovement();

    public virtual void StopMovement() { }

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
}
