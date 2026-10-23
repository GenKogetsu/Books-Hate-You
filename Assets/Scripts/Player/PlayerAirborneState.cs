public sealed class PlayerAirborneState : PlayerMovementState
{
    public override void OnUpdate() => TickAndTransition();
}
