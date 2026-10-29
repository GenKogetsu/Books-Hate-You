#region Player crouch move state
public sealed class PlayerCrouchMoveState : PlayerMovementState
{
    public override void OnEnter() => Controller.AudioManager?.PlayWalk();
    public override void OnUpdate() => TickAndTransition();
}
#endregion
