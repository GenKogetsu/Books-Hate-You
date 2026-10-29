#region Imports
using Kogetsu.Library.DesignPatternCore;
#endregion
#region Shared player state

public abstract class PlayerMovementState : BaseState<PlayerMovementController>, IUpdateState
{
    #region State context
    protected PlayerMovementController Controller => Context;
    protected PlayerStatus Status => Controller.Status;

    public abstract override void OnUpdate();

    #endregion
    #region Shared state update
    protected void TickAndTransition()
    {
        if (TransitionToDeadIfNeeded())
            return;

        Controller.TickMovement();
        Controller.AudioManager?.Sync(Status);

        if (TransitionToDeadIfNeeded() || TransitionToAirborneIfNeeded())
            return;

        if (Status.GetIsCrouching())
            TransitionToCrouchState();
        else
            TransitionToGroundedState();
    }
    #endregion

    #region Dead state transition
    private bool TransitionToDeadIfNeeded()
    {
        if (Status.GetIsAlive())
            return false;

        Controller.AudioManager?.StopAll();
        Controller.ChangeState<PlayerDeadState>();
        return true;
    }
    #endregion

    #region Airborne state transition
    private bool TransitionToAirborneIfNeeded()
    {
        if (!Status.GetIsGrounded() || Status.GetIsJumping() ||
            Status.GetIsPreparingJump() || Status.GetIsLanding())
        {
            Controller.ChangeState<PlayerAirborneState>();
            return true;
        }

        return false;
    }
    #endregion

    #region Crouch state transitions
    private void TransitionToCrouchState()
    {
        if (Status.GetIsRunning())
            Controller.ChangeState<PlayerCrouchRunState>();
        else if (Status.GetIsMoving())
            Controller.ChangeState<PlayerCrouchMoveState>();
        else
            Controller.ChangeState<PlayerCrouchIdleState>();
    }
    #endregion

    #region Grounded state transitions
    private void TransitionToGroundedState()
    {
        if (Status.GetIsRunning())
            Controller.ChangeState<PlayerRunState>();
        else if (Status.GetIsMoving())
            Controller.ChangeState<PlayerMoveState>();
        else
            Controller.ChangeState<PlayerIdleState>();
    }
    #endregion
}
#endregion
