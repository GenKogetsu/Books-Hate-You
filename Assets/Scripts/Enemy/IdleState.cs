public sealed class IdleState : EnemyState
{
    public IdleState(LibrarianMovementController controller) : base(controller) { }

    public override void Enter()
    {
        Controller.Agent.isStopped = true;
        Controller.Agent.ResetPath();
        Controller.Agent.speed = Controller.Stats.InitialChaseSpeed;
        Controller.PlayIdleAnimation();
    }

    public override void Update()
    {
        if (Controller.CanDetectPlayer())
            Controller.ChangeState(new ChaseState(Controller));
    }

    public override void Exit() { }
}
