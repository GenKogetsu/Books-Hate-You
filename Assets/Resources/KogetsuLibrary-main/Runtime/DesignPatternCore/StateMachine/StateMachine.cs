using System;

namespace Kogetsu.Library.DesignPatternCore
{
    [Serializable]
    public class StateMachine<TContext>
    {
        public BaseState<TContext> CurrentState { get; private set; }

        public void ChangeState(BaseState<TContext> newState)
        {
            if (CurrentState is IExitState exitState) exitState.OnExit();
            CurrentState = newState;
            if (CurrentState is IEnterState enterState) enterState.OnEnter();
        }

        public void Update() 
        { 
            if (CurrentState is IUpdateState updateState) updateState.OnUpdate(); 
        }

        public void FixedUpdate() 
        { 
            if (CurrentState is IFixedUpdateState fixedUpdateState) fixedUpdateState.OnFixedUpdate(); 
        }
    }
}