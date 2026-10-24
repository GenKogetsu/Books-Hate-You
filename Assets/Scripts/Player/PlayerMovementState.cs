using Kogetsu.Library.DesignPatternCore;

public abstract class PlayerMovementState : BaseState<PlayerMovementController>, IUpdateState
{
    protected PlayerMovementController Controller => Context;
    protected PlayerStatus Status => Controller.Status;

    public abstract override void OnUpdate();

    protected void TickAndTransition()
    {
        if (!Status.GetIsAlive())
        {
            Controller.AudioManager?.StopAll();
            Controller.ChangeState(new PlayerDeadState());
            return;
        }

        Controller.TickMovement();
        Controller.AudioManager?.Sync(Status);

        if (!Status.GetIsAlive())
        {
            Controller.AudioManager?.StopAll();
            Controller.ChangeState(new PlayerDeadState());
            return;
        }

        if (!Status.GetIsGrounded() || Status.GetIsJumping() ||
            Status.GetIsPreparingJump() || Status.GetIsLanding())
        {
            Controller.ChangeState(new PlayerAirborneState());
            return;
        }

        Controller.ChangeState(Status.GetIsMoving()
            ? new PlayerMoveState()
            : new PlayerIdleState());
    }
}
