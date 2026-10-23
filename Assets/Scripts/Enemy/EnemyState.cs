using Kogetsu.Library.DesignPatternCore;

public abstract class EnemyState : BaseState<LibrarianMovementController>
{
    protected LibrarianMovementController Controller => Context;
}
