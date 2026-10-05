using UnityEngine;
using Kogetsu.Library.Attribute;
using System.Collections.Generic;

public class PlayerAnimationController : MonoBehaviour
{
    #region Serialized Fields

    [Header("References")]
    [SerializeField] private PlayerStatus _playerStatus;
    [SerializeField] private Animator _animator;

    [Header("Standing Clips")]
    [SerializeField, KeyFromName] private Dictionary<string, AnimationClip> _idleClips = new();
    [SerializeField, KeyFromName] private Dictionary<string, AnimationClip> _walkClips = new();
    [SerializeField, KeyFromName] private Dictionary<string, AnimationClip> _runClips = new();
    [SerializeField, KeyFromName] private Dictionary<string, AnimationClip> _jumpClips = new();

    [Header("Crouch Clips")]
    [SerializeField, KeyFromName] private Dictionary<string, AnimationClip> _crouchIdleClips = new();
    [SerializeField, KeyFromName] private Dictionary<string, AnimationClip> _crouchWalkClips = new();

    [SerializeField] private bool _crouchFastWalkUsesCrouchWalk;
    [SerializeField, KeyFromName] private Dictionary<string, AnimationClip> _crouchFastWalkClips = new();

    [Header("Transition Clips")]
    [SerializeField, KeyFromName] private Dictionary<string, AnimationClip> _standingToCrouchClips = new();

    [SerializeField] private bool _crouchToStandingReversesStandingToCrouch;
    [SerializeField, KeyFromName] private Dictionary<string, AnimationClip> _crouchToStandingClips = new();

    [Header("Other Clips")]
    [SerializeField, KeyFromName] private Dictionary<string, AnimationClip> _deathClips = new();

    [Header("Settings")]
    [SerializeField, Min(0f)] private float _transitionDuration = 0.15f;

    #endregion

    #region Private Fields

    private LivingAnimState _currentState = LivingAnimState.None;
    private bool _wasCrouching;
    private float _transitionEndTime;

    #endregion

    #region Validation

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (Application.isPlaying) return;

        KeyFromNameSync.Sync(this);
    }
#endif

    #endregion

    #region Unity Callbacks

    private void Reset()
    {
        TryGetComponent(out _playerStatus);
        _animator = GetComponentInChildren<Animator>();
    }

    private void Start()
    {
        _wasCrouching = _playerStatus.GetIsCrouching();
    }

    private void Update()
    {
        LivingAnimState targetState = ResolveState();

        if (targetState == _currentState) return;

        PlayState(targetState);
    }

    #endregion

    #region State Logic

    private LivingAnimState ResolveState()
    {
        if (_playerStatus.GetIsDead()) return LivingAnimState.Death;

        if (_playerStatus.GetIsJumping()) return LivingAnimState.Jump;

        bool isCrouching = _playerStatus.GetIsCrouching();

        if (isCrouching != _wasCrouching)
        {
            _wasCrouching = isCrouching;

            LivingAnimState transition = isCrouching
                ? LivingAnimState.StandingToCrouch
                : LivingAnimState.CrouchToStanding;

            if (HasClip(transition)) return transition;
        }

        if (IsTransitionPlaying()) return _currentState;

        if (isCrouching)
        {
            if (_playerStatus.GetIsCrouchRunning()) return LivingAnimState.CrouchFastWalk;

            if (_playerStatus.GetIsMoving()) return LivingAnimState.CrouchWalk;

            return LivingAnimState.CrouchIdle;
        }

        if (_playerStatus.GetIsRunning()) return LivingAnimState.Run;

        if (_playerStatus.GetIsMoving()) return LivingAnimState.Walk;

        return LivingAnimState.Idle;
    }

    private bool IsTransitionPlaying()
    {
        return IsTransitionState(_currentState) && Time.time < _transitionEndTime;
    }

    private bool HasClip(LivingAnimState state)
    {
        Dictionary<string, AnimationClip> clips = GetClips(state);
        if (clips == null) return false;

        foreach (KeyValuePair<string, AnimationClip> pair in clips)
        {
            if (pair.Value) return true;
        }

        return false;
    }

    private Dictionary<string, AnimationClip> GetClips(LivingAnimState state) => state switch
    {
        LivingAnimState.Idle => _idleClips,
        LivingAnimState.Walk => _walkClips,
        LivingAnimState.Run => _runClips,
        LivingAnimState.CrouchIdle => _crouchIdleClips,
        LivingAnimState.CrouchWalk => _crouchWalkClips,
        LivingAnimState.CrouchFastWalk => _crouchFastWalkUsesCrouchWalk
            ? _crouchWalkClips
            : _crouchFastWalkClips,
        LivingAnimState.StandingToCrouch => _standingToCrouchClips,
        LivingAnimState.CrouchToStanding => _crouchToStandingReversesStandingToCrouch
            ? _standingToCrouchClips
            : _crouchToStandingClips,
        LivingAnimState.Jump => _jumpClips,
        LivingAnimState.Death => _deathClips,
        _ => null
    };

    private bool ShouldPlayReversed(LivingAnimState state)
    {
        return state == LivingAnimState.CrouchToStanding && _crouchToStandingReversesStandingToCrouch;
    }

    #endregion

    #region Animation

    private void PlayState(LivingAnimState state)
    {
        if (!_animator) return;

        AnimationClip clip = PickRandomClip(GetClips(state));
        if (!clip) return;

        if (IsTransitionState(state)) _transitionEndTime = Time.time + clip.length;

        float blend = _currentState == LivingAnimState.Jump ? 0f : _transitionDuration;

        if (ShouldPlayReversed(state))
        {
            _animator.speed = -1f;
            _animator.CrossFade(clip.name, blend, -1, 1f);
        }
        else
        {
            _animator.speed = 1f;
            _animator.CrossFade(clip.name, blend);
        }

        _currentState = state;
    }

    private static bool IsTransitionState(LivingAnimState state)
    {
        return state == LivingAnimState.StandingToCrouch
               || state == LivingAnimState.CrouchToStanding;
    }

    private AnimationClip PickRandomClip(Dictionary<string, AnimationClip> clips)
    {
        if (clips == null || clips.Count == 0) return null;

        int index = Random.Range(0, clips.Count);
        int current = 0;

        foreach (KeyValuePair<string, AnimationClip> pair in clips)
        {
            if (current == index) return pair.Value;
            current++;
        }

        return null;
    }

    public bool TryGetClip(LivingAnimState state, string clipName, out AnimationClip clip)
    {
        clip = null;

        Dictionary<string, AnimationClip> clips = GetClips(state);
        return clips != null && clips.TryGetValue(clipName, out clip) && clip;
    }

    #endregion
}

public enum LivingAnimState
{
    None,
    Idle,
    Walk,
    Run,
    CrouchIdle,
    CrouchWalk,
    CrouchFastWalk,
    StandingToCrouch,
    CrouchToStanding,
    Jump,
    Death
}