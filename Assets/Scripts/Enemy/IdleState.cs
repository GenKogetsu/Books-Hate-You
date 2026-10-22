public sealed class IdleState : EnemyState
{
    public IdleState(LibrarianController controller) : base(controller) { }

    public override void Enter()
    {
        Controller.Agent.isStopped = true;
        Controller.Agent.ResetPath();
        Controller.Agent.speed = Controller.InitialChaseSpeed;
        Controller.PlayAnimation(Controller.IdleClip);
    }

    public override void Update()
    {
        if (Controller.CanDetectPlayer())
            Controller.ChangeState(new ChaseState(Controller));
    }

    public override void Exit() { }
}
