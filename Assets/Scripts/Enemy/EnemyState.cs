public abstract class EnemyState : IState<LibrarianMovementController>
{
    protected readonly LibrarianMovementController Controller;

    protected EnemyState(LibrarianMovementController controller)
    {
        Controller = controller;
    }

    public abstract void Enter();
    public abstract void Update();
    public abstract void Exit();
}
