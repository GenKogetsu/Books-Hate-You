public sealed class PlayerIdleState : PlayerMovementState
{
    public override void OnUpdate() => TickAndTransition();
}
