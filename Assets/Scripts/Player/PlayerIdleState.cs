#region Idle state
public sealed class PlayerIdleState : PlayerMovementState
{
    public override void OnEnter() => Controller.AudioManager?.StopMovement();
    public override void OnUpdate() => TickAndTransition();
}
#endregion
