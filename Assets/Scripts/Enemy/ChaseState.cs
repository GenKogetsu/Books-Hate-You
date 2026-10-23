using UnityEngine;

public sealed class ChaseState : EnemyState
{
    private enum ChasePhase
    {
        Chasing,
        InvestigatingLastSeenPosition,
        ReturningToChaseStart
    }

    private float _chaseDuration;
    private Vector3 _lastSeenPosition;
    private Vector3 _chaseStartPosition;
    private ChasePhase _phase;

    public ChaseState() { }

    public override void OnEnter()
    {
        _chaseDuration = 0f;
        _chaseStartPosition = Controller.transform.position;
        _lastSeenPosition = Controller.PlayerTransform.position;
        _phase = ChasePhase.Chasing;

        Controller.Agent.isStopped = false;
        Controller.Agent.speed = Controller.Stats.InitialChaseSpeed;
        Controller.NextDestinationUpdateTime = 0f;
        Controller.PlayRunAnimation();
        UpdateDestination(_lastSeenPosition);
    }

    public override void OnUpdate()
    {
        if (!Controller.HasLivingPlayer())
        {
            Controller.ChangeState(new IdleState());
            return;
        }

        if (Controller.CanDetectPlayer())
        {
            _phase = ChasePhase.Chasing;
            _lastSeenPosition = Controller.PlayerTransform.position;
            _chaseDuration += Time.deltaTime;

            Controller.Agent.speed = Mathf.Min(
                Controller.Stats.InitialChaseSpeed
                + _chaseDuration * Controller.Stats.SpeedIncreasePerSecond,
                Mathf.Max(
                    Controller.Stats.InitialChaseSpeed,
                    Controller.Stats.MaximumChaseSpeed));

            if (Time.time >= Controller.NextDestinationUpdateTime)
                UpdateDestination(_lastSeenPosition);

            return;
        }

        if (_phase == ChasePhase.Chasing)
        {
            _phase = ChasePhase.InvestigatingLastSeenPosition;
            Controller.Agent.speed = Controller.Stats.InitialChaseSpeed;
            UpdateDestination(_lastSeenPosition);
            return;
        }

        if (!HasReachedDestination())
            return;

        if (_phase == ChasePhase.InvestigatingLastSeenPosition)
        {
            _phase = ChasePhase.ReturningToChaseStart;
            UpdateDestination(_chaseStartPosition);
            return;
        }

        Controller.ChangeState(new IdleState());
    }

    private void UpdateDestination(Vector3 destination)
    {
        Controller.Agent.SetDestination(destination);
        Controller.NextDestinationUpdateTime = Time.time + Mathf.Max(
            0.02f,
            Controller.Stats.DestinationUpdateInterval);
    }

    private bool HasReachedDestination()
    {
        if (Controller.Agent.pathPending)
            return false;

        if (!Controller.Agent.hasPath)
            return true;

        float stoppingDistance = Mathf.Max(
            Controller.Agent.stoppingDistance,
            0.15f);

        return Controller.Agent.remainingDistance <= stoppingDistance;
    }

    public override void OnExit()
    {
        Controller.Agent.isStopped = true;
        Controller.Agent.ResetPath();
    }
}
