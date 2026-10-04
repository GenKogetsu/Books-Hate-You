using UnityEngine;
using NaughtyAttributes;
using Kogetsu.Library.Attribute;

public class PlayerStatus : MonoBehaviour
{
    #region Serialized Fields

    [Header("Config")]
    [SerializeField, EditOnInspector] private PlayerStats _playerStats;

    [Header("Movement Modifiers")]
    [SerializeField, Range(1.25f, 2.5f)] private float _runSpeedMultiplier = 1.6f;
    [SerializeField, Range(0.1f, 1f)] private float _crouchSpeedMultiplier = 0.5f;
    [SerializeField, Range(0.1f, 1f)] private float _crouchRunSpeedMultiplier = 0.8f;
    [SerializeField, Range(0.5f, 1.5f)] private float _crouchHeight = 1f;

    [Header("Health")]
    [SerializeField] private float _startHp = 100f;
    [SerializeField] private float _maxHp = 100f;
    [SerializeField, ReadOnly] private float _currentHp;

    [Header("Movement (Runtime)")]
    [SerializeField, ReadOnly] private Vector2 _moveInput;
    [SerializeField, ReadOnly] private Vector3 _velocity;
    [SerializeField, ReadOnly] private bool _isGrounded;

    [Header("Input Intent (Runtime)")]
    [SerializeField, ReadOnly] private bool _isRunInputHeld;
    [SerializeField, ReadOnly] private bool _isCrouchInputHeld;

    [Header("State (Runtime)")]
    [SerializeField, ReadOnly] private bool _isAlive;
    [SerializeField, ReadOnly] private bool _isMoving;
    [SerializeField, ReadOnly] private bool _isRunning;
    [SerializeField, ReadOnly] private bool _isCrouching;
    [SerializeField, ReadOnly] private bool _isJumping;

    #endregion

    #region Unity Callbacks

    private void Awake()
    {
        _currentHp = Mathf.Clamp(_startHp, 0f, _maxHp);
        _isAlive = _currentHp > 0f;
    }

    #endregion

    #region Health

    public float GetCurrentHp() => _currentHp;
    public float GetMaxHp() => _maxHp;
    public bool GetIsAlive() => _isAlive;
    public bool GetIsDead() => !_isAlive;

    public void SetCurrentHp(float value)
    {
        _currentHp = Mathf.Clamp(value, 0f, _maxHp);
        _isAlive = _currentHp > 0f;
    }

    #endregion

    #region Config (Read Only)

    public float GetMoveSpeed() => _playerStats.MoveSpeed;
    public float GetJumpHeight() => _playerStats.JumpHeight;
    public float GetGravity() => _playerStats.Gravity;
    public float GetRunSpeedMultiplier() => _runSpeedMultiplier;
    public float GetCrouchSpeedMultiplier() => _crouchSpeedMultiplier;
    public float GetCrouchRunSpeedMultiplier() => _crouchRunSpeedMultiplier;
    public float GetCrouchHeight() => _crouchHeight;

    public float GetCurrentMoveSpeed()
    {
        float speed = _playerStats.MoveSpeed;

        if (_isCrouching)
            speed *= _isRunning ? _crouchRunSpeedMultiplier : _crouchSpeedMultiplier;
        else if (_isRunning)
            speed *= _runSpeedMultiplier;

        return speed;
    }

    #endregion

    #region Movement Runtime

    public Vector2 GetMoveInput() => _moveInput;
    public void SetMoveInput(Vector2 value) => _moveInput = value;

    public Vector3 GetVelocity() => _velocity;
    public void SetVelocity(Vector3 value) => _velocity = value;

    public bool GetIsGrounded() => _isGrounded;
    public void SetIsGrounded(bool value) => _isGrounded = value;

    #endregion

    #region Input Intent

    public bool GetIsRunInputHeld() => _isRunInputHeld;
    public void SetIsRunInputHeld(bool value) => _isRunInputHeld = value;

    public bool GetIsCrouchInputHeld() => _isCrouchInputHeld;
    public void SetIsCrouchInputHeld(bool value) => _isCrouchInputHeld = value;

    #endregion

    #region State

    public bool GetIsMoving() => _isMoving;
    public void SetIsMoving(bool value) => _isMoving = value;

    public bool GetIsRunning() => _isRunning;
    public void SetIsRunning(bool value) => _isRunning = value;

    public bool GetIsCrouching() => _isCrouching;
    public void SetIsCrouching(bool value) => _isCrouching = value;

    public bool GetIsJumping() => _isJumping;
    public void SetIsJumping(bool value) => _isJumping = value;

    public bool GetIsCrouchRunning() => _isCrouching && _isRunning;

    #endregion

    #region Reset

    public void ResetMovementState()
    {
        _moveInput = Vector2.zero;
        _isRunInputHeld = false;
        _isCrouchInputHeld = false;
        _isMoving = false;
        _isRunning = false;
        _isJumping = false;
    }

    #endregion
}