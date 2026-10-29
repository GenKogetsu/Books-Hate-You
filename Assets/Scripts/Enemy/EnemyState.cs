#region Imports
using Kogetsu.Library.DesignPatternCore;
#endregion
#region Shared librarian state

public abstract class EnemyState : BaseState<LibrarianMovementController>
{
    protected LibrarianMovementController Controller => Context;
}
#endregion
