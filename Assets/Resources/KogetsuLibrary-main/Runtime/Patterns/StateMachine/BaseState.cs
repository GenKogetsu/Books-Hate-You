using System;

namespace Kogetsu.Library.DesignPatternCore
{
    [Serializable]
    public abstract class BaseState<TContext> : IState, IEnterState, IExitState, IUpdateState, IFixedUpdateState
    {
        protected TContext Context;
        public virtual void Initialize(TContext context) => Context = context;

        public virtual void OnEnter() { }
        public virtual void OnExit() { }
        public virtual void OnUpdate() { }
        public virtual void OnFixedUpdate() { }
    }
}
