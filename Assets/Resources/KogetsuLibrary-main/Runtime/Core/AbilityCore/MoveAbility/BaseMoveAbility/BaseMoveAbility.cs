using System;
using Kogetsu.Library.DesignPatternCore;

namespace Kogetsu.Library.Core;

[Serializable]
public abstract class BaseMoveAbility<TContext> : BaseState<TContext> , IEnterState, IFixedUpdateState, IExitState, IAbility
    where TContext : class, IMoveContext
{
    [Header("Input Debug")]
    [ReadOnly]
    [SerializeField] protected Vector3 CurrentInput;

    //public bool IsJumping { get; protected set; }
    //public bool IsCrouching { get; protected set; }

    void IEnterState.OnEnter() => OnEnter();
    void IFixedUpdateState.OnFixedUpdate() => OnFixedUpdate();
    void IExitState.OnExit() => OnExit();

    protected abstract void OnEnter();
    protected abstract void OnFixedUpdate();

    protected abstract void OnExit();

    protected virtual void SetInput(Vector3 input) => CurrentInput = input;

}
