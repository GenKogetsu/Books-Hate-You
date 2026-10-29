#region Player crouch idle state
public sealed class PlayerCrouchIdleState : PlayerMovementState
{
    public override void OnEnter() => Controller.AudioManager?.StopMovement();
    public override void OnUpdate() => TickAndTransition();
}
#endregion
