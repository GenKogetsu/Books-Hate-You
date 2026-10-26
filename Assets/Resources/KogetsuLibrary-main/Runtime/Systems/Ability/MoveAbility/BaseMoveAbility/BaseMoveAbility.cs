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

    #region Documentation - Notes
    //public bool IsJumping { get; protected set; }
    //public bool IsCrouching { get; protected set; }
    #endregion

    void IEnterState.OnEnter() => OnEnter();
    void IFixedUpdateState.OnFixedUpdate() => OnFixedUpdate();
    void IExitState.OnExit() => OnExit();

    protected new abstract void OnEnter();
    protected new abstract void OnFixedUpdate();

    protected new abstract void OnExit();

    protected virtual void SetInput(Vector3 input) => CurrentInput = input;

}
