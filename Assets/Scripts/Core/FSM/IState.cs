#region IState
public interface IState<in TOwner>
{
    void Enter();
    void Update();
    void Exit();
}
#endregion
