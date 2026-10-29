#region Airborne state
public sealed class PlayerAirborneState : PlayerMovementState
{
    public override void OnEnter() => Controller.AudioManager?.StopMovement();
    public override void OnUpdate() => TickAndTransition();
}
#endregion
