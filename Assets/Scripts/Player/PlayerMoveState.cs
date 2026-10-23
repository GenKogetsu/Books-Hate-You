public sealed class PlayerMoveState : PlayerMovementState
{
    public override void OnUpdate() => TickAndTransition();
}
