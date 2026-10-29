#region Move state
public sealed class PlayerMoveState : PlayerMovementState
{
    public override void OnEnter() => Controller.AudioManager?.PlayWalk();
    public override void OnUpdate() => TickAndTransition();
}
#endregion
