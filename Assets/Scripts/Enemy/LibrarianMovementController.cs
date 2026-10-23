using UnityEngine;
using UnityEngine.AI;
using NaughtyAttributes;
using Kogetsu.Library.Attribute;
using Kogetsu.Library.DesignPatternCore;

[RequireComponent(typeof(NavMeshAgent))]
public sealed class LibrarianMovementController : LivingMovementController
{
    [Header("References")]
    [SerializeField] private NavMeshAgent _agent;
    [SerializeField] private LibrarianAnimationController _animationController;
    [SerializeField] private PlayerStatus _playerStatus;
    [SerializeField] private Transform _eyePoint;

    [Tooltip("เลือก Layer ของกำแพง/สิ่งกีดขวางที่บังการมองเห็น เช่น Block")]
    [SerializeField] private LayerMask _visionBlockMask;
    [SerializeField, EditOnInspector] private LibrarianStats _librarianStats;

    [Header("State (Runtime)")]
    [SerializeField, ReadOnly] private string _currentStateName = "None";

    private EnemyState _currentState;
    private bool _isDead;
    private bool _isGameOver;

    internal float NextDestinationUpdateTime { get; set; }
    internal NavMeshAgent Agent => _agent;
    internal PlayerStatus PlayerStatus => _playerStatus;
    internal LibrarianAnimationController AnimationController => _animationController;
    internal LibrarianStats Stats => _librarianStats;
    internal Transform EyePoint => _eyePoint;
    internal bool IsChasing => _currentState is ChaseState;

    private void Reset()
    {
        TryGetComponent(out _agent);
        _animationController = GetComponent<LibrarianAnimationController>();

        if (_visionBlockMask.value == 0)
            _visionBlockMask = LayerMask.GetMask("Block");

        if (!_playerStatus)
            _playerStatus = FindAnyObjectByType<PlayerStatus>();
    }

    private void Start()
    {
        if (_visionBlockMask.value == 0)
            _visionBlockMask = LayerMask.GetMask("Block");

        if (!_agent || !_animationController || !_librarianStats)
        {
            Debug.LogError(
                "LibrarianMovementController requires NavMeshAgent, LibrarianAnimationController, and LibrarianStats.",
                this);
            enabled = false;
            return;
        }

        if (!_animationController.IdleClip || !_animationController.RunClip)
        {
            Debug.LogError(
                "Please assign both Idle and Run Animation Clips to LibrarianAnimationController.",
                this);
            enabled = false;
            return;
        }

        _agent.speed = _librarianStats.InitialChaseSpeed;
        ChangeState(new IdleState(this));
    }

    private void OnEnable()
    {
        if (EventBus.Instance)
            EventBus.Instance.Subscribe<GameoverEvent>(OnGameover);
    }

    private void OnDisable()
    {
        if (EventBus.Instance)
            EventBus.Instance.Unsubscribe<GameoverEvent>(OnGameover);
    }

    protected override bool CanMove()
    {
        return !_isDead && !_isGameOver && _currentState != null;
    }

    protected override void UpdateMovement()
    {
        _currentState.Update();
    }

    public override void StopMovement()
    {
        _currentState?.Exit();
        _currentState = null;
        _currentStateName = "None";

        if (_agent)
        {
            _agent.isStopped = true;
            _agent.ResetPath();
        }
    }

    internal void ChangeState(EnemyState nextState)
    {
        if (_isDead)
            return;

        _currentState?.Exit();
        _currentState = nextState;
        _currentStateName = nextState?.GetType().Name ?? "None";
        _currentState.Enter();
    }

    internal bool HasLivingPlayer()
    {
        return _playerStatus != null
               && !_isGameOver
               && _playerStatus.gameObject.activeInHierarchy;
    }

    internal bool CanDetectPlayer()
    {
        if (!HasLivingPlayer())
            return false;

        Vector3 origin = GetVisionOrigin();
        Vector3 toPlayer = (_playerStatus.transform.position + Vector3.up) - origin;

        if (toPlayer.sqrMagnitude > _librarianStats.DetectionRange * _librarianStats.DetectionRange)
            return false;

        if (Vector3.Angle(GetVisionForward(), toPlayer) >
            _librarianStats.ViewAngle * 0.5f)
            return false;

        return _visionBlockMask.value == 0 || !Physics.Raycast(
            origin,
            toPlayer.normalized,
            toPlayer.magnitude,
            _visionBlockMask,
            QueryTriggerInteraction.Ignore);
    }

    internal Vector3 GetVisionOrigin()
    {
        return _eyePoint
            ? _eyePoint.position + _eyePoint.TransformVector(_librarianStats.EyeOffset)
            : transform.position + transform.TransformVector(_librarianStats.EyeOffset);
    }

    internal Vector3 GetVisionForward()
    {
        return _eyePoint ? _eyePoint.forward : transform.forward;
    }

    internal void PlayIdleAnimation() => _animationController?.PlayIdle();
    internal void PlayRunAnimation() => _animationController?.PlayRun();

    private void OnGameover(GameoverEvent _)
    {
        _isGameOver = true;
        StopMovement();
        _currentStateName = "GameOver";
    }

    public void HandleDeath()
    {
        if (_isDead)
            return;

        _isDead = true;
        StopMovement();
        _currentStateName = "Dead";
    }
}
