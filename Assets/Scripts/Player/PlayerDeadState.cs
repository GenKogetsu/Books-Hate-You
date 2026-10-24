public sealed class PlayerDeadState : PlayerMovementState
{
    public override void OnEnter() => Controller.AudioManager?.StopAll();

    public override void OnUpdate()
    {
        Controller.StopMovement();
    }
}
