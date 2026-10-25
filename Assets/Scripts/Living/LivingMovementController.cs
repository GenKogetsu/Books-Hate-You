using UnityEngine;
using Kogetsu.Library.DesignPatternCore;

public abstract class LivingMovementController : MonoBehaviour
{
    private bool _isLoadingScene;

    protected virtual void OnEnable()
    {
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
        if (!_isLoadingScene && CanMove())
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
        _isLoadingScene = true;
        StopMovement();
    }
}
