public sealed class IdleState : EnemyState
{
    public IdleState() { }

    public override void OnEnter()
    {
        Controller.Agent.isStopped = true;
        Controller.Agent.ResetPath();
        Controller.Agent.speed = Controller.Stats.InitialChaseSpeed;
        Controller.PlayIdleAnimation();
    }

    public override void OnUpdate()
    {
        if (Controller.CanDetectPlayer())
            Controller.ChangeState(new ChaseState());
    }

    public override void OnExit() { }
}
