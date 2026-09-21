using System;

namespace Kogetsu.Library.DesignPatternCore
{
    [Serializable]
    public abstract class BaseState<TContext> : IState
    {
        protected TContext Context;
        public virtual void Initialize(TContext context) => Context = context;
    }
}
