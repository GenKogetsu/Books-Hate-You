public abstract class EnemyState
{
    protected readonly LibrarianController Controller;

    protected EnemyState(LibrarianController controller)
    {
        Controller = controller;
    }

    public abstract void Enter();
    public abstract void Update();
    public abstract void Exit();
}
