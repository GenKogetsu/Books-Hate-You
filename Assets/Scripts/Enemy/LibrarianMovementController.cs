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
    [SerializeField] private Transform _eyePoint;
    [SerializeField] private AudioSource _chaseAudioSource;

    [Tooltip("ตรวจเฉพาะ Collider ใน Layer Living ที่มี Tag Player")]
    [SerializeField] private LayerMask _livingMask;
    [Tooltip("เลือก Layer ของกำแพง/สิ่งกีดขวางที่บังการมองเห็น เช่น Block")]
    [SerializeField] private LayerMask _visionBlockMask;
    [SerializeField, EditOnInspector] private LibrarianStats _librarianStats;

    [Header("State (Runtime)")]
    [SerializeField, ReadOnly] private string _currentStateName = "None";

    private Kogetsu.Library.DesignPatternCore.StateMachine<LibrarianMovementController> _stateMachine;
    private bool _isDead;
    private bool _isGameOver;
    private Transform _playerTransform;

    internal float NextDestinationUpdateTime { get; set; }
    internal NavMeshAgent Agent => _agent;
    internal LibrarianAnimationController AnimationController => _animationController;
    internal LibrarianStats Stats => _librarianStats;
    internal Transform PlayerTransform => _playerTransform;
    internal Transform EyePoint => _eyePoint;
    internal bool IsChasing => _stateMachine?.CurrentState is ChaseState;

    private void Reset()
    {
        TryGetComponent(out _agent);
        _animationController = GetComponent<LibrarianAnimationController>();

        if (_visionBlockMask.value == 0)
            _visionBlockMask = LayerMask.GetMask("Block");

        if (_livingMask.value == 0)
            _livingMask = LayerMask.GetMask("Living");
    }

    private void Start()
    {
        if (!_agent)
            _agent = GetComponent<NavMeshAgent>();

        if (!_animationController)
            _animationController = GetComponent<LibrarianAnimationController>();

        if (!_chaseAudioSource)
            _chaseAudioSource = GetComponentInChildren<AudioSource>();

        if (_livingMask.value == 0)
            _livingMask = LayerMask.GetMask("Living");

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
        _stateMachine = new Kogetsu.Library.DesignPatternCore.StateMachine<LibrarianMovementController>();
        ChangeState(new IdleState());
    }

    private void OnEnable()
    {
        base.OnEnable();

        if (EventBus.Instance)
            EventBus.Instance.Subscribe<GameoverEvent>(OnGameover);
    }

    private void OnDisable()
    {
        base.OnDisable();

        if (EventBus.Instance)
            EventBus.Instance.Unsubscribe<GameoverEvent>(OnGameover);
    }

    protected override bool CanMove()
    {
        return !_isDead && !_isGameOver && _stateMachine?.CurrentState != null;
    }

    protected override void UpdateMovement()
    {
        _stateMachine?.Update();
    }

    public override void StopMovement()
    {
        _stateMachine?.ChangeState(null);
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

        if (_stateMachine == null)
            _stateMachine = new Kogetsu.Library.DesignPatternCore.StateMachine<LibrarianMovementController>();

        if (nextState != null)
            nextState.Initialize(this);

        _stateMachine.ChangeState(nextState);
        _currentStateName = nextState?.GetType().Name ?? "None";
    }

    internal bool CanDetectPlayer()
    {
        Vector3 origin = GetVisionOrigin();
        Collider[] candidates = Physics.OverlapSphere(
            origin,
            _librarianStats.DetectionRange,
            _livingMask,
            QueryTriggerInteraction.Ignore);

        foreach (Collider candidate in candidates)
        {
            Transform player = FindPlayerRoot(candidate.transform);
            if (!player)
                continue;

            Vector3 toPlayer = (player.position + Vector3.up) - origin;

            if (Vector3.Angle(GetVisionForward(), toPlayer) >
                _librarianStats.ViewAngle * 0.5f)
                continue;

            if (_visionBlockMask.value != 0 && Physics.Raycast(
                origin,
                toPlayer.normalized,
                toPlayer.magnitude,
                _visionBlockMask,
                QueryTriggerInteraction.Ignore))
                continue;

            _playerTransform = player;
            return true;
        }

        return false;
    }

    internal bool HasLivingPlayer()
    {
        return _playerTransform
               && !_isGameOver
               && _playerTransform.gameObject.activeInHierarchy;
    }

    private Transform FindPlayerRoot(Transform candidate)
    {
        Transform current = candidate;

        while (current)
        {
            if (current.CompareTag("Player"))
                return current;

            current = current.parent;
        }

        return null;
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

    internal void StartChaseAudio()
    {
        if (_chaseAudioSource)
        {
            if (!_chaseAudioSource.isPlaying)
                _chaseAudioSource.Play();
        }
    }

    internal void StopChaseAudio()
    {
        if (_chaseAudioSource)
            _chaseAudioSource.Stop();

    }

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
        StopChaseAudio();
        StopMovement();
        _currentStateName = "Dead";
    }
}
