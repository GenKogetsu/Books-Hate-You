public sealed class PlayerDeadState : PlayerMovementState
{
    public override void OnUpdate()
    {
        Controller.StopMovement();
    }
}
