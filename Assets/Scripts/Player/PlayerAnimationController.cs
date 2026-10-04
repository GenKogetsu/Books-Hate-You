using UnityEngine;

public class PlayerAnimationController : MonoBehaviour
{
    private enum PlayerAnimState
    {
        None,
        Idle,
        Walk,
        Run,
        CrouchIdle,
        CrouchWalk,
        CrouchRun,
        Jump,
        Death
    }

    #region Serialized Fields

    [Header("References")]
    [SerializeField] private PlayerStatus _playerStatus;
    [SerializeField] private Animator _animator;

    [Header("Standing Clips")]
    [SerializeField] private AnimationClip _idleClip;
    [SerializeField] private AnimationClip _walkClip;
    [SerializeField] private AnimationClip _runClip;
    [SerializeField] private AnimationClip _jumpClip;

    [Header("Crouch Clips")]
    [SerializeField] private AnimationClip _crouchIdleClip;
    [SerializeField] private AnimationClip _crouchWalkClip;
    [SerializeField] private AnimationClip _crouchRunClip;

    [Header("Other Clips")]
    [SerializeField] private AnimationClip _deathClip;

    [Header("Settings")]
    [SerializeField, Min(0f)] private float _transitionDuration = 0.15f;

    #endregion

    #region Private Fields

    private PlayerAnimState _currentState = PlayerAnimState.None;

    #endregion

    #region Unity Callbacks

    private void Reset()
    {
        TryGetComponent(out _playerStatus);
        _animator = GetComponentInChildren<Animator>();
    }

    private void Update()
    {
        PlayerAnimState targetState = ResolveState();

        if (targetState == _currentState) return;

        PlayState(targetState);
    }

    #endregion

    #region State Logic

    private PlayerAnimState ResolveState()
    {
        if (_playerStatus.GetIsDead())
            return PlayerAnimState.Death;

        if (_playerStatus.GetIsJumping())
            return PlayerAnimState.Jump;

        if (_playerStatus.GetIsCrouching())
        {
            if (_playerStatus.GetIsCrouchRunning()) return PlayerAnimState.CrouchRun;
            if (_playerStatus.GetIsMoving()) return PlayerAnimState.CrouchWalk;
            return PlayerAnimState.CrouchIdle;
        }

        if (_playerStatus.GetIsRunning()) return PlayerAnimState.Run;
        if (_playerStatus.GetIsMoving()) return PlayerAnimState.Walk;

        return PlayerAnimState.Idle;
    }

    private AnimationClip GetClip(PlayerAnimState state) => state switch
    {
        PlayerAnimState.Idle => _idleClip,
        PlayerAnimState.Walk => _walkClip,
        PlayerAnimState.Run => _runClip,
        PlayerAnimState.CrouchIdle => _crouchIdleClip,
        PlayerAnimState.CrouchWalk => _crouchWalkClip,
        PlayerAnimState.CrouchRun => _crouchRunClip,
        PlayerAnimState.Jump => _jumpClip,
        PlayerAnimState.Death => _deathClip,
        _ => null
    };

    #endregion

    #region Animation

    private void PlayState(PlayerAnimState state)
    {
        if (!_animator) return;

        AnimationClip clip = GetClip(state);
        if (!clip) return;

        _animator.CrossFade(clip.name, _transitionDuration);
        _currentState = state;
    }

    #endregion
}