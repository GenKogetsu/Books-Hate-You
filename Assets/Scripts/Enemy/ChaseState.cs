using UnityEngine;

public sealed class ChaseState : EnemyState
{
    private float _chaseDuration;

    public ChaseState(LibrarianController controller) : base(controller) { }

    public override void Enter()
    {
        _chaseDuration = 0f;
        Controller.Agent.isStopped = false;
        Controller.Agent.speed = Controller.InitialChaseSpeed;
        Controller.NextDestinationUpdateTime = 0f;
        Controller.PlayAnimation(Controller.RunClip);
        UpdateDestination();
    }

    public override void Update()
    {
        if (!Controller.HasLivingPlayer())
        {
            Controller.ChangeState(new IdleState(Controller));
            return;
        }

        float distance = Vector3.Distance(
            Controller.transform.position,
            Controller.PlayerStatus.transform.position);

        if (distance > Controller.LoseTargetRange)
        {
            Controller.ChangeState(new IdleState(Controller));
            return;
        }

        _chaseDuration += Time.deltaTime;
        Controller.Agent.speed = Mathf.Min(
            Controller.InitialChaseSpeed
            + _chaseDuration * Controller.SpeedIncreasePerSecond,
            Controller.MaximumChaseSpeed);

        if (Time.time >= Controller.NextDestinationUpdateTime)
            UpdateDestination();
    }

    private void UpdateDestination()
    {
        if (!Controller.HasLivingPlayer())
            return;

        Controller.Agent.SetDestination(Controller.PlayerStatus.transform.position);
        Controller.NextDestinationUpdateTime = Time.time + Mathf.Max(
            0.02f,
            Controller.DestinationUpdateInterval);
    }

    public override void Exit()
    {
        Controller.Agent.isStopped = true;
        Controller.Agent.ResetPath();
    }
}
