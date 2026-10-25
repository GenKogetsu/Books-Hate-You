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
    private TransformSnapshot _chaseStartTransform;
    private ChasePhase _phase;

    private readonly struct TransformSnapshot
    {
        public readonly Vector3 Position;
        public readonly Quaternion Rotation;
        public readonly Vector3 LocalScale;

        public TransformSnapshot(Transform source)
        {
            Position = source.position;
            Rotation = source.rotation;
            LocalScale = source.localScale;
        }

        public void Restore(Transform target)
        {
            target.SetPositionAndRotation(Position, Rotation);
            target.localScale = LocalScale;
        }
    }

    public ChaseState() { }

    public override void OnEnter()
    {
        _chaseDuration = 0f;
        _chaseStartTransform = new TransformSnapshot(Controller.transform);
        _lastSeenPosition = Controller.PlayerTransform.position;
        _phase = ChasePhase.Chasing;

        Controller.Agent.isStopped = false;
        Controller.Agent.speed = Controller.Stats.MoveSpeed;
        Controller.NextDestinationUpdateTime = 0f;
        Controller.PlayRunAnimation();
        Controller.AudioManager?.PlayRunSfx();
        Controller.AudioManager?.PlayChaseBgm();
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
                Controller.Stats.MoveSpeed
                + _chaseDuration * Controller.Stats.SpeedIncreasePerSecond,
                Mathf.Max(
                    Controller.Stats.MoveSpeed,
                    Controller.Stats.MaximumChaseSpeed));

            if (Time.time >= Controller.NextDestinationUpdateTime)
                UpdateDestination(_lastSeenPosition);

            return;
        }

        if (_phase == ChasePhase.Chasing)
        {
            _phase = ChasePhase.InvestigatingLastSeenPosition;
            Controller.Agent.speed = Controller.Stats.MoveSpeed;
            UpdateDestination(_lastSeenPosition);
            return;
        }

        if (!HasReachedDestination())
            return;

        if (_phase == ChasePhase.InvestigatingLastSeenPosition)
        {
            _phase = ChasePhase.ReturningToChaseStart;
            UpdateDestination(_chaseStartTransform.Position);
            return;
        }

        RestoreChaseStartTransform();
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

    private void RestoreChaseStartTransform()
    {
        Controller.Agent.Warp(_chaseStartTransform.Position);
        _chaseStartTransform.Restore(Controller.transform);
    }

    public override void OnExit()
    {
        Controller.AudioManager?.StopAll();
        Controller.Agent.isStopped = true;
        Controller.Agent.ResetPath();
    }
}
