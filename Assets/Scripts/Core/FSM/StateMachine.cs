#region StateMachine
public sealed class StateMachine<TOwner>
{
    private readonly TOwner _owner;

    public IState<TOwner> CurrentState { get; private set; }

    public StateMachine(TOwner owner)
    {
        _owner = owner;
    }

    public void ChangeState(IState<TOwner> nextState)
    {
        CurrentState?.Exit();
        CurrentState = nextState;
        CurrentState?.Enter();
    }

    public void Update()
    {
        CurrentState?.Update();
    }

    public void Stop()
    {
        CurrentState?.Exit();
        CurrentState = null;
    }
}
#endregion
