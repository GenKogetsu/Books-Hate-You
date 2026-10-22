using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class LibrarianController : MonoBehaviour
{
    #region Serialized Fields

    [Header("References")]
    [SerializeField] private NavMeshAgent _agent;
    [SerializeField] private Animator _animator;
    [SerializeField] private PlayerStatus _playerStatus;

    [Header("Animation Clips")]
    [SerializeField] private AnimationClip _idleClip;
    [SerializeField] private AnimationClip _runClip;
    [SerializeField, Min(0f)] private float _crossFadeDuration = 0.2f;

    [Header("Detection")]
    [SerializeField, Min(0f)] private float _detectionRange = 10f;
    [SerializeField, Min(0f)] private float _loseTargetRange = 15f;

    [Header("Chase Movement")]
    [SerializeField, Min(0f)] private float _initialChaseSpeed = 3.5f;
    [SerializeField, Min(0f)] private float _maximumChaseSpeed = 7f;
    [SerializeField, Min(0f)] private float _speedIncreasePerSecond = 0.25f;
    [SerializeField, Min(0f)] private float _destinationUpdateInterval = 0.15f;

    #endregion

    #region Private Fields

    private EnemyState _currentState;
    internal float NextDestinationUpdateTime { get; set; }

    internal NavMeshAgent Agent => _agent;
    internal PlayerStatus PlayerStatus => _playerStatus;
    internal AnimationClip IdleClip => _idleClip;
    internal AnimationClip RunClip => _runClip;
    internal float InitialChaseSpeed => _initialChaseSpeed;
    internal float MaximumChaseSpeed => _maximumChaseSpeed;
    internal float SpeedIncreasePerSecond => _speedIncreasePerSecond;
    internal float DestinationUpdateInterval => _destinationUpdateInterval;
    internal float LoseTargetRange => _loseTargetRange;

    #endregion

    #region Unity Callbacks

    private void Reset()
    {
        TryGetComponent(out _agent);
        _animator = GetComponentInChildren<Animator>();

        if (!_playerStatus)
            _playerStatus = FindAnyObjectByType<PlayerStatus>();
    }

    private void Start()
    {
        if (!_agent || !_animator)
        {
            Debug.LogError(
                "LibrarianController requires a NavMeshAgent and an Animator.",
                this);

            enabled = false;
            return;
        }

        if (!_idleClip || !_runClip)
        {
            Debug.LogError(
                "Please assign both Idle and Run Animation Clips.",
                this);

            enabled = false;
            return;
        }

        if (_maximumChaseSpeed < _initialChaseSpeed)
            _maximumChaseSpeed = _initialChaseSpeed;

        _agent.speed = _initialChaseSpeed;
        ChangeState(new IdleState(this));
    }

    private void Update()
    {
        if (_currentState == null)
            return;

        _currentState.Update();
    }

    private void OnDisable()
    {
        _currentState?.Exit();
        _currentState = null;
    }

    #endregion

    #region State Machine

    internal void ChangeState(EnemyState nextState)
    {
        _currentState?.Exit();

        _currentState = nextState;
        _currentState.Enter();
    }

    #endregion

    #region Detection

    internal bool HasLivingPlayer()
    {
        return _playerStatus != null
               && _playerStatus.GetIsAlive()
               && _playerStatus.gameObject.activeInHierarchy;
    }

    internal bool CanDetectPlayer()
    {
        if (!HasLivingPlayer())
            return false;

        float distanceSqr =
            (_playerStatus.transform.position - transform.position)
            .sqrMagnitude;

        return distanceSqr <= _detectionRange * _detectionRange;
    }

    #endregion

    #region Animation

    internal void PlayAnimation(AnimationClip clip)
    {
        if (!_animator || !clip)
            return;

        _animator.CrossFade(
            clip.name,
            _crossFadeDuration,
            0);
    }

    #endregion
}
