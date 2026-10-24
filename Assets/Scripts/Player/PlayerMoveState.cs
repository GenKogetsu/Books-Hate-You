public sealed class PlayerMoveState : PlayerMovementState
{
    public override void OnEnter() => Controller.AudioManager?.Sync(Status);
    public override void OnUpdate() => TickAndTransition();
}
