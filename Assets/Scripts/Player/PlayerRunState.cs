#region Player run state
public sealed class PlayerRunState : PlayerMovementState
{
    public override void OnEnter() => Controller.AudioManager?.PlayRun();
    public override void OnUpdate() => TickAndTransition();
}
#endregion
