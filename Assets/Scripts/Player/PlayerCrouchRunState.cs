#region Player crouch run state
public sealed class PlayerCrouchRunState : PlayerMovementState
{
    public override void OnEnter() => Controller.AudioManager?.PlayRun();
    public override void OnUpdate() => TickAndTransition();
}
#endregion
