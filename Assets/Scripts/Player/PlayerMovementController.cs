using UnityEngine;
using UnityEngine.InputSystem;
using Kogetsu.Library.DesignPatternCore;

[DefaultExecutionOrder(-1000)]
[RequireComponent(typeof(CharacterController), typeof(PlayerStatus))]
public class PlayerMovementController : LivingMovementController
{
    #region Constants

    private const float MoveThreshold = 0.0001f;
    private const float GroundedStickVelocity = -2f;

    #endregion

    #region Serialized Fields

    [Header("References")]
    [SerializeField] private CameraController _cameraController;
    [SerializeField] private PlayerStatus _playerStatus;
    [SerializeField] private CharacterController _characterController;
    [SerializeField] private Transform _cameraReference;
    [SerializeField] private Transform _modelTransform;
    [SerializeField] private PlayerAudioManager _audioManager;
    [SerializeField] private PlayerAnimationController _animationController;
    [SerializeField] private PlayerSceneWarpState _playerSceneWarpState;

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
    private bool _isPlayerInputLocked;
    private Kogetsu.Library.DesignPatternCore.StateMachine<PlayerMovementController> _stateMachine;

    #endregion

    #region Setup

    private void Reset()
    {
        TryGetComponent(out _characterController);
        TryGetComponent(out _playerStatus);
        _audioManager = GetComponentInChildren<PlayerAudioManager>();
        _animationController = GetComponentInChildren<PlayerAnimationController>();
        if (!_modelTransform) _modelTransform = transform;
    }

    private void Awake()
    {
        if (!_characterController)
            _characterController = GetComponent<CharacterController>();

        if (!_playerStatus)
            _playerStatus = GetComponent<PlayerStatus>();

        if (!_audioManager)
            _audioManager = GetComponentInChildren<PlayerAudioManager>();

        if (!_animationController)
            _animationController = GetComponentInChildren<PlayerAnimationController>();

        if (!_audioManager)
            _audioManager = gameObject.AddComponent<PlayerAudioManager>();

        _standingHeight = _characterController.height;
        _standingCenter = _characterController.center;

        if (!_modelTransform) _modelTransform = transform;

        ApplyPendingSceneWarp();

        _stateMachine = new Kogetsu.Library.DesignPatternCore.StateMachine<PlayerMovementController>();
    }

    private void Start()
    {
        ChangeState(new PlayerIdleState());
    }

    #endregion

    #region Event Subscription

    private void OnEnable()
    {
        base.OnEnable();
        _isPlayerInputLocked = false;

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
            EventBus.Instance.Subscribe<LoadSceneEvent>(OnLoadSceneEvent);
        }
    }

    private void OnDisable()
    {
        base.OnDisable();

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
            EventBus.Instance.Unsubscribe<LoadSceneEvent>(OnLoadSceneEvent);
        }
    }

    #endregion

    #region Main Loop

    protected override bool CanMove()
    {
        return _playerStatus;
    }

    protected override void UpdateMovement()
    {
        _stateMachine?.Update();
    }

    internal PlayerStatus Status => _playerStatus;
    internal PlayerAudioManager AudioManager => _audioManager;

    internal void TickMovement()
    {
        UpdateJumpPrepare();
        UpdateLanding();
        ApplyMovement();
    }

    private void ApplyPendingSceneWarp()
    {
        if (!_playerSceneWarpState ||
            !_playerSceneWarpState.TryGetPendingPosition(out Vector3 destination))
            return;

        _characterController.enabled = false;
        transform.position = destination;
        _characterController.enabled = true;

        _playerSceneWarpState.ClearPendingPosition();
        _playerStatus.ResetMovementState();
        _animationController?.ForceIdle();
    }

    internal void ChangeState(PlayerMovementState nextState)
    {
        if (_stateMachine == null)
            _stateMachine = new Kogetsu.Library.DesignPatternCore.StateMachine<PlayerMovementController>();

        if (nextState != null && _stateMachine.CurrentState != null &&
            _stateMachine.CurrentState.GetType() == nextState.GetType())
            return;

        if (nextState != null)
            nextState.Initialize(this);

        _stateMachine.ChangeState(nextState);
    }

    #endregion

    #region Input Callbacks

    private void OnMovePerformed(InputAction.CallbackContext context)
    {
        if (_isPlayerInputLocked) return;
        _playerStatus.SetMoveInput(context.ReadValue<Vector2>());
    }

    private void OnMoveCanceled(InputAction.CallbackContext context)
    {
        if (_isPlayerInputLocked) return;
        _playerStatus.SetMoveInput(Vector2.zero);
    }

    private void OnJumpPerformed(InputAction.CallbackContext context)
    {
        if (_isPlayerInputLocked) return;
        if (!_playerStatus.GetCanJumpNow()) return;

        _jumpPrepareTimer = _playerStatus.GetJumpPrepareDuration();
        _playerStatus.SetIsPreparingJump(true);
    }

    private void OnRunPerformed(InputAction.CallbackContext context)
    {
        if (_isPlayerInputLocked) return;
        _playerStatus.SetIsRunInputHeld(true);
    }

    private void OnRunCanceled(InputAction.CallbackContext context)
    {
        if (_isPlayerInputLocked) return;
        _playerStatus.SetIsRunInputHeld(false);
    }

    private void OnCrouchPerformed(InputAction.CallbackContext context)
    {
        if (_isPlayerInputLocked) return;
        _playerStatus.SetIsCrouchInputHeld(true);
    }

    private void OnCrouchCanceled(InputAction.CallbackContext context)
    {
        if (_isPlayerInputLocked) return;
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

        Vector3 forward;
        Vector3 right;

        if (_cameraController)
        {
            Quaternion yaw = _cameraController.LookYawRotation;
            forward = yaw * Vector3.forward;
            right = yaw * Vector3.right;
        }
        else if (_cameraReference)
        {
            forward = _cameraReference.forward;
            right = _cameraReference.right;
        }
        else
        {
            return new Vector3(input.x, 0f, input.y);
        }

        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
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
        _audioManager?.StopAll();
        StopMovement();
        enabled = false;
    }

    private void OnLoadSceneEvent(LoadSceneEvent _)
    {
        _isPlayerInputLocked = true;
        DisableInputActions();
        _audioManager?.StopAll();
        StopMovement();
    }

    private void DisableInputActions()
    {
        _moveActionReference?.action.Disable();
        _jumpActionReference?.action.Disable();
        _runActionReference?.action.Disable();
        _crouchActionReference?.action.Disable();
    }

    public override void StopMovement()
    {
        if (!_playerStatus)
            return;

        _playerStatus.SetMoveInput(Vector2.zero);
        _playerStatus.SetIsRunInputHeld(false);
        _playerStatus.SetIsCrouchInputHeld(false);
        _playerStatus.ResetMovementState();
    }

    #endregion
}
