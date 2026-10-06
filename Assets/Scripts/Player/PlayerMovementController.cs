using UnityEngine;
using UnityEngine.InputSystem;
using Kogetsu.Library.DesignPatternCore;

[RequireComponent(typeof(CharacterController), typeof(PlayerStatus))]
public class PlayerMovementController : MonoBehaviour
{
    #region Constants

    private const float MoveThreshold = 0.0001f;
    private const float GroundedStickVelocity = -2f;

    #endregion

    #region Serialized Fields

    [Header("References")]
    [SerializeField] private PlayerStatus _playerStatus;
    [SerializeField] private CharacterController _characterController;
    [SerializeField] private Transform _cameraReference;
    [SerializeField] private Transform _modelTransform;

    [Header("Input Action")]
    [SerializeField] private InputActionReference _moveActionReference;
    [SerializeField] private InputActionReference _jumpActionReference;
    [SerializeField] private InputActionReference _runActionReference;
    [SerializeField] private InputActionReference _crouchActionReference;

    [Header("Crouch")]
    [SerializeField] private LayerMask _ceilingMask = ~0;

    [Header("Model Rotation")]
    [SerializeField, Min(0.1f)] private float _modelRotationSpeed = 10f;

    #endregion

    #region Private Fields

    private float _standingHeight;
    private Vector3 _standingCenter;
    private float _jumpPrepareTimer;
    private float _landingTimer;

    #endregion

    #region Setup

    private void Reset()
    {
        TryGetComponent(out _characterController);
        TryGetComponent(out _playerStatus);
        if (!_modelTransform) _modelTransform = transform;
    }

    private void Awake()
    {
        _standingHeight = _characterController.height;
        _standingCenter = _characterController.center;

        if (!_modelTransform) _modelTransform = transform;
    }

    #endregion

    #region Event Subscription

    private void OnEnable()
    {
        if (_moveActionReference != null)
        {
            _moveActionReference.action.Enable();
            _moveActionReference.action.performed += OnMovePerformed;
            _moveActionReference.action.canceled += OnMoveCanceled;
        }

        if (_jumpActionReference != null)
        {
            _jumpActionReference.action.Enable();
            _jumpActionReference.action.performed += OnJumpPerformed;
        }

        if (_runActionReference != null)
        {
            _runActionReference.action.Enable();
            _runActionReference.action.performed += OnRunPerformed;
            _runActionReference.action.canceled += OnRunCanceled;
        }

        if (_crouchActionReference != null)
        {
            _crouchActionReference.action.Enable();
            _crouchActionReference.action.performed += OnCrouchPerformed;
            _crouchActionReference.action.canceled += OnCrouchCanceled;
        }

        if (EventBus.Instance)
        {
            EventBus.Instance.Subscribe<GameoverEvent>(OnGameover);
        }
    }

    private void OnDisable()
    {
        if (_moveActionReference != null)
        {
            _moveActionReference.action.performed -= OnMovePerformed;
            _moveActionReference.action.canceled -= OnMoveCanceled;
            _moveActionReference.action.Disable();
        }

        if (_jumpActionReference != null)
        {
            _jumpActionReference.action.performed -= OnJumpPerformed;
            _jumpActionReference.action.Disable();
        }

        if (_runActionReference != null)
        {
            _runActionReference.action.performed -= OnRunPerformed;
            _runActionReference.action.canceled -= OnRunCanceled;
            _runActionReference.action.Disable();
        }

        if (_crouchActionReference != null)
        {
            _crouchActionReference.action.performed -= OnCrouchPerformed;
            _crouchActionReference.action.canceled -= OnCrouchCanceled;
            _crouchActionReference.action.Disable();
        }

        if (EventBus.Instance)
        {
            EventBus.Instance.Unsubscribe<GameoverEvent>(OnGameover);
        }
    }

    #endregion

    #region Main Loop

    private void Update()
    {
        if (!_playerStatus.GetIsAlive()) return;

        UpdateJumpPrepare();
        UpdateLanding();
        ApplyMovement();
    }

    #endregion

    #region Input Callbacks

    private void OnMovePerformed(InputAction.CallbackContext context)
    {
        _playerStatus.SetMoveInput(context.ReadValue<Vector2>());
    }

    private void OnMoveCanceled(InputAction.CallbackContext context)
    {
        _playerStatus.SetMoveInput(Vector2.zero);
    }

    private void OnJumpPerformed(InputAction.CallbackContext context)
    {
        if (!_playerStatus.GetCanJumpNow()) return;

        _jumpPrepareTimer = _playerStatus.GetJumpPrepareDuration();
        _playerStatus.SetIsPreparingJump(true);
    }

    private void OnRunPerformed(InputAction.CallbackContext context)
    {
        _playerStatus.SetIsRunInputHeld(true);
    }

    private void OnRunCanceled(InputAction.CallbackContext context)
    {
        _playerStatus.SetIsRunInputHeld(false);
    }

    private void OnCrouchPerformed(InputAction.CallbackContext context)
    {
        _playerStatus.SetIsCrouchInputHeld(true);
    }

    private void OnCrouchCanceled(InputAction.CallbackContext context)
    {
        _playerStatus.SetIsCrouchInputHeld(false);
    }

    #endregion

    #region Movement

    private void ApplyMovement()
    {
        bool isGrounded = _characterController.isGrounded;
        _playerStatus.SetIsGrounded(isGrounded);

        UpdateCrouchState();
        UpdateMoveStates();

        Vector3 velocity = _playerStatus.GetVelocity();

        if (isGrounded && velocity.y < 0f)
        {
            velocity.y = GroundedStickVelocity;

            if (_playerStatus.GetIsJumping()) StartLanding();

            _playerStatus.SetIsJumping(false);
        }

        Vector3 move = _playerStatus.GetIsMovementLocked() ? Vector3.zero : GetCameraRelativeMove();
        _characterController.Move(_playerStatus.GetCurrentMoveSpeed() * Time.deltaTime * move);

        RotateModelTowardsMovement(move);

        velocity.y += _playerStatus.GetGravity() * Time.deltaTime;
        _characterController.Move(velocity * Time.deltaTime);

        _playerStatus.SetVelocity(velocity);
    }

    private void UpdateMoveStates()
    {
        bool isMoving = !_playerStatus.GetIsMovementLocked() && _playerStatus.GetMoveInput().sqrMagnitude > MoveThreshold;
        bool isRunning = isMoving && _playerStatus.GetIsRunInputHeld();

        _playerStatus.SetIsMoving(isMoving);
        _playerStatus.SetIsRunning(isRunning);
    }

    private void  UpdateCrouchState()
    {
        bool wantsCrouch = _playerStatus.GetIsCrouchInputHeld();
        bool isCrouching = _playerStatus.GetIsCrouching();

        if (wantsCrouch && !isCrouching && !_playerStatus.GetIsPreparingJump() && !_playerStatus.GetIsLanding())
        {
            SetCrouch(true);
        }
        else if (!wantsCrouch && isCrouching && CanStandUp())
        {
            SetCrouch(false);
        }
    }

    private void SetCrouch(bool crouch)
    {
        float targetHeight = crouch ? _playerStatus.GetCrouchHeight() : _standingHeight;
        targetHeight = Mathf.Min(targetHeight, _standingHeight);

        float feetY = _standingCenter.y - _standingHeight * 0.5f;
        Vector3 center = _standingCenter;
        center.y = feetY + targetHeight * 0.5f;

        _characterController.height = targetHeight;
        _characterController.center = center;
        _playerStatus.SetIsCrouching(crouch);
    }

    private bool CanStandUp()
    {
        float radius = _characterController.radius;
        float crouchHeight = _characterController.height;
        float checkDistance = _standingHeight - crouchHeight;

        Vector3 feet = transform.position + _characterController.center
                       + Vector3.down * (crouchHeight * 0.5f);
        Vector3 origin = feet + Vector3.up * (crouchHeight - radius);

        return !Physics.SphereCast(
            origin,
            radius * 0.95f,
            Vector3.up,
            out _,
            checkDistance,
            _ceilingMask,
            QueryTriggerInteraction.Ignore);
    }

    private Vector3 GetCameraRelativeMove()
    {
        Vector2 input = _playerStatus.GetMoveInput();

        if (!_cameraReference) return new Vector3(input.x, 0f, input.y);

        Vector3 forward = _cameraReference.forward;
        forward.y = 0f;
        forward.Normalize();

        Vector3 right = _cameraReference.right;
        right.y = 0f;
        right.Normalize();

        Vector3 move = forward * input.y + right * input.x;

        return Vector3.ClampMagnitude(move, 1f);
    }
    private void UpdateJumpPrepare()
    {
        if (!_playerStatus.GetIsPreparingJump()) return;

        bool canceled = !_playerStatus.GetCanJump()
                        || !_playerStatus.GetIsGrounded()
                        || _playerStatus.GetIsCrouching();

        if (canceled)
        {
            _playerStatus.SetIsPreparingJump(false);
            return;
        }

        _jumpPrepareTimer -= Time.deltaTime;
        if (_jumpPrepareTimer > 0f) return;

        LaunchJump();
    }

    private void LaunchJump()
    {
        Vector3 velocity = _playerStatus.GetVelocity();
        velocity.y = Mathf.Sqrt(_playerStatus.GetJumpHeight() * -2f * _playerStatus.GetGravity());
        _playerStatus.SetVelocity(velocity);

        _playerStatus.SetIsPreparingJump(false);
        _playerStatus.SetIsJumping(true);
    }

    private void StartLanding()
    {
        _landingTimer = _playerStatus.GetJumpPrepareDuration();
        _playerStatus.SetIsLanding(true);
    }

    private void UpdateLanding()
    {
        if (!_playerStatus.GetIsLanding()) return;

        _landingTimer -= Time.deltaTime;
        if (_landingTimer > 0f) return;

        _playerStatus.SetIsLanding(false);
    }

    #endregion

    #region Model Rotation

    private void RotateModelTowardsMovement(Vector3 moveDirection)
    {
        if (!_modelTransform || moveDirection.sqrMagnitude < MoveThreshold) return;

        Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
        _modelTransform.rotation = Quaternion.Lerp(
            _modelTransform.rotation,
            targetRotation,
            _modelRotationSpeed * Time.deltaTime);
    }

    #endregion

    #region Event Handlers

    private void OnGameover(GameoverEvent gameoverEvent)
    {
        _playerStatus.ResetMovementState();
        enabled = false;
    }

    #endregion
}