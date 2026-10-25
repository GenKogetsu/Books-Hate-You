using System.Collections;
using Kogetsu.Library.DesignPatternCore;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public sealed class CameraController : MonoBehaviour
{
    public enum CameraAnimationMode { Animate, SkipAnimation }

    [Header("Player")]
    [SerializeField] private PlayerStatus _playerStatus;
    [SerializeField] private Transform _followTarget;
    [SerializeField] private Transform _lookAtTarget;
    [SerializeField] private GameObject _playerModel;
    [SerializeField] private GameObject _firstPersonModel;
    [SerializeField] private bool _isFollowing = true;

    [Header("Main Camera and Game View Anchors")]
    [SerializeField] private Camera _mainCamera;
    [SerializeField] private CameraViewAnchor _firstPersonViewCamera;
    [SerializeField] private CameraViewAnchor _topViewCamera;
    [SerializeField] private CameraViewAnchor _thirdPersonViewCamera;
    [SerializeField] private CameraViewMode _startView = CameraViewMode.TPP;
    [SerializeField] private CameraAnimationMode _animationMode = CameraAnimationMode.Animate;

    [Header("Mirror Cameras")]
    [SerializeField] private Camera _topMirrorViewCamera;
    [SerializeField] private Camera _backMirrorViewCamera;

    [Header("Skip Animation Fade")]
    [SerializeField] private Animator _fadeAnimator;
    [SerializeField] private Image _fadeImage;
    [SerializeField] private AnimationClip _fadeIn;
    [SerializeField] private AnimationClip _fadeOut;
    [SerializeField, Min(0.01f)] private float _fadeSpeed = 1f;

    [Header("Input Actions")]
    [SerializeField] private InputActionReference _firstPersonViewKey;
    [SerializeField] private InputActionReference _topViewKey;
    [SerializeField] private InputActionReference _thirdPersonViewKey;
    [SerializeField] private InputActionReference _toggleViewKey;
    [SerializeField] private InputActionReference _lookInput;
    [SerializeField] private InputActionReference _holdTopCameraKey;
    [SerializeField] private InputActionReference _holdBackCameraKey;

    private readonly Kogetsu.Library.DesignPatternCore.StateMachine<CameraController> _stateMachine = new();
    private GameViewState _gameViewState;
    private MirrorViewState _mirrorViewState;
    private CameraViewMode _currentMode;
    private Quaternion _initialBoxRotation;
    private Coroutine _transitionRoutine;
    private float _yaw;
    private float _pitch;
    private float _topZoom;
    private float _thirdPersonZoom;
    private bool _topHeld;
    private bool _backHeld;
    private bool _collisionCompressed;
    private Camera _activeMirrorCamera;
    private bool _isGameOver;
    private bool _isLoadingScene;

    private FirstPersonCameraSettingSO FppSettings =>
        _firstPersonViewCamera ? _firstPersonViewCamera.Settings as FirstPersonCameraSettingSO : null;
    private TopCameraSettingSO TopSettings =>
        _topViewCamera ? _topViewCamera.Settings as TopCameraSettingSO : null;
    private ThirdPersonCameraSettingSO TppSettings =>
        _thirdPersonViewCamera ? _thirdPersonViewCamera.Settings as ThirdPersonCameraSettingSO : null;

    public CameraViewMode CurrentViewMode => _currentMode;
    public bool IsFirstPerson => _currentMode == CameraViewMode.FPP;
    public bool IsTopView => _currentMode == CameraViewMode.TopView;
    public bool IsViewTransitioning => _transitionRoutine != null;
    public bool IsRotating => IsViewTransitioning;
    public Quaternion LookYawRotation => Quaternion.LookRotation(MovementForward, Vector3.up);

    public Vector3 MovementForward
    {
        get
        {
            if (!_mainCamera) return transform.forward;
            Vector3 axis = _currentMode == CameraViewMode.TopView
                ? _mainCamera.transform.up
                : _mainCamera.transform.forward;
            axis = Vector3.ProjectOnPlane(axis, Vector3.up);
            return axis.sqrMagnitude > 0.0001f ? axis.normalized : Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
        }
    }

    public Vector3 MovementRight
    {
        get
        {
            Vector3 forward = MovementForward;
            return forward.sqrMagnitude > 0.0001f ? Vector3.Cross(Vector3.up, forward).normalized : transform.right;
        }
    }

    private void Awake()
    {
        AssignCameras();
        _initialBoxRotation = transform.rotation;
        _currentMode = _startView;
        _gameViewState = new GameViewState();
        _mirrorViewState = new MirrorViewState();
        _gameViewState.Initialize(this);
        _mirrorViewState.Initialize(this);
        if (_fadeAnimator) _fadeAnimator.updateMode = AnimatorUpdateMode.UnscaledTime;
        SnapMainToCurrentView();
        ApplyModelVisibility();
    }

    private void OnEnable()
    {
        if (!Application.isPlaying) return;
        _isGameOver = false;
        _isLoadingScene = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        if (EventBus.Instance)
        {
            EventBus.Instance.Subscribe<GameoverEvent>(OnGameover);
            EventBus.Instance.Subscribe<GameClearEvent>(OnGameClear);
            EventBus.Instance.Subscribe<LoadSceneEvent>(OnLoadSceneEvent);
        }
        Subscribe(_firstPersonViewKey, SelectFpp);
        Subscribe(_topViewKey, SelectTop);
        Subscribe(_thirdPersonViewKey, SelectTpp);
        Subscribe(_toggleViewKey, CyclePerformed);
        Subscribe(_holdTopCameraKey, TopMirrorStarted, TopMirrorReleased);
        Subscribe(_holdBackCameraKey, BackMirrorStarted, BackMirrorReleased);
        if (_lookInput) _lookInput.action.Enable();
        _stateMachine.ChangeState(_gameViewState);
    }

    private void OnDisable()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        if (!Application.isPlaying) return;
        if (EventBus.Instance)
        {
            EventBus.Instance.Unsubscribe<GameoverEvent>(OnGameover);
            EventBus.Instance.Unsubscribe<GameClearEvent>(OnGameClear);
            EventBus.Instance.Unsubscribe<LoadSceneEvent>(OnLoadSceneEvent);
        }
        Unsubscribe(_firstPersonViewKey, SelectFpp);
        Unsubscribe(_topViewKey, SelectTop);
        Unsubscribe(_thirdPersonViewKey, SelectTpp);
        Unsubscribe(_toggleViewKey, CyclePerformed);
        Unsubscribe(_holdTopCameraKey, TopMirrorStarted, TopMirrorReleased);
        Unsubscribe(_holdBackCameraKey, BackMirrorStarted, BackMirrorReleased);
        if (_lookInput) _lookInput.action.Disable();
        CancelTransition();
        _stateMachine.ChangeState(null);
    }

    private static void Subscribe(InputActionReference reference, System.Action<InputAction.CallbackContext> performed,
        System.Action<InputAction.CallbackContext> canceled = null)
    {
        if (!reference) return;
        reference.action.Enable();
        reference.action.performed += performed;
        if (canceled != null) reference.action.canceled += canceled;
    }

    private static void Unsubscribe(InputActionReference reference, System.Action<InputAction.CallbackContext> performed,
        System.Action<InputAction.CallbackContext> canceled = null)
    {
        if (!reference) return;
        reference.action.performed -= performed;
        if (canceled != null) reference.action.canceled -= canceled;
        reference.action.Disable();
    }

    private void AssignCameras()
    {
        if (!_mainCamera) _mainCamera = transform.Find("Main Camera")?.GetComponent<Camera>();
        if (!_firstPersonViewCamera) _firstPersonViewCamera = transform.Find("First Person View Camera")?.GetComponent<CameraViewAnchor>();
        if (!_topViewCamera) _topViewCamera = transform.Find("Top View Camera")?.GetComponent<CameraViewAnchor>();
        if (!_thirdPersonViewCamera) _thirdPersonViewCamera = transform.Find("Third Person View Camera")?.GetComponent<CameraViewAnchor>();
        if (!_topMirrorViewCamera) _topMirrorViewCamera = transform.Find("Top Mirror View Camera")?.GetComponent<Camera>();
        if (!_backMirrorViewCamera) _backMirrorViewCamera = transform.Find("Back Mirror View Camera")?.GetComponent<Camera>();
    }

    private void LateUpdate()
    {
        if (!Application.isPlaying || _isGameOver || _isLoadingScene) return;
        _stateMachine.Update();
    }

    private void OnLoadSceneEvent(LoadSceneEvent _)
    {
        _isLoadingScene = true;
        DisableInputActions();
        _topHeld = false;
        _backHeld = false;
        _activeMirrorCamera = null;
        CancelTransition();
        _stateMachine.ChangeState(null);
    }

    private void DisableInputActions()
    {
        Unsubscribe(_firstPersonViewKey, SelectFpp);
        Unsubscribe(_topViewKey, SelectTop);
        Unsubscribe(_thirdPersonViewKey, SelectTpp);
        Unsubscribe(_toggleViewKey, CyclePerformed);
        Unsubscribe(_holdTopCameraKey, TopMirrorStarted, TopMirrorReleased);
        Unsubscribe(_holdBackCameraKey, BackMirrorStarted, BackMirrorReleased);
        if (_lookInput) _lookInput.action.Disable();
    }

    private void OnGameover(GameoverEvent _)
    {
        _isGameOver = true;
        CancelTransition();
        _stateMachine.ChangeState(null);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void OnGameClear(GameClearEvent _) => OnGameover(default);

    private void TickGameView()
    {
        if (_isFollowing && _followTarget) transform.position = _followTarget.position;
        UpdateLook();
        if (_transitionRoutine == null) SnapMainToCurrentView(true);
    }

    private void UpdateLook()
    {
        CameraSettingSO settings = SettingsFor(_currentMode);
        if (settings == null || !_lookInput) return;
        Vector2 delta = _lookInput.action.ReadValue<Vector2>();
        _yaw += settings.LookX(delta.x);
        float y = settings.LookY(delta.y);
        switch (_currentMode)
        {
            case CameraViewMode.FPP:
                _pitch = FppSettings.ClampPitch(_pitch - y);
                break;
            case CameraViewMode.TopView:
                // Top View keeps its Inspector pitch and rotates around Y only.
                break;
            case CameraViewMode.TPP:
                _pitch = TppSettings.ClampPitch(_pitch - y);
                break;
        }
        transform.rotation = _initialBoxRotation * Quaternion.Euler(0f, _yaw, 0f);

        if (Mouse.current == null) return;
        float scroll = Mouse.current.scroll.ReadValue().y;
        if (_currentMode == CameraViewMode.TopView && TopSettings)
            _topZoom = TopSettings.ClampZoom(_topZoom + scroll * TopSettings.ZoomSpeed);
        else if (_currentMode == CameraViewMode.TPP && TppSettings)
            _thirdPersonZoom = TppSettings.ClampZoom(_thirdPersonZoom + scroll * TppSettings.ZoomSpeed);
    }

    private CameraSettingSO SettingsFor(CameraViewMode mode) => mode switch
    {
        CameraViewMode.FPP => FppSettings,
        CameraViewMode.TopView => TopSettings,
        _ => TppSettings
    };

    private Transform AnchorFor(CameraViewMode mode) => mode switch
    {
        CameraViewMode.FPP => _firstPersonViewCamera ? _firstPersonViewCamera.transform : null,
        CameraViewMode.TopView => _topViewCamera ? _topViewCamera.transform : null,
        _ => _thirdPersonViewCamera ? _thirdPersonViewCamera.transform : null
    };

    private Vector3 LookPoint => _lookAtTarget ? _lookAtTarget.position
        : _followTarget ? _followTarget.position : transform.position;

    private static Quaternion LockTppPitch(Quaternion cameraRotation, Vector3 toPlayer)
    {
        if (toPlayer.sqrMagnitude <= 0.0001f)
            return cameraRotation;

        float lookAtPitch = Quaternion.LookRotation(toPlayer, Vector3.up).eulerAngles.x;
        Vector3 euler = cameraRotation.eulerAngles;
        euler.x = lookAtPitch;
        return Quaternion.Euler(euler);
    }

    private bool TryGetDesiredPose(CameraViewMode mode, out Vector3 position, out Quaternion rotation)
    {
        Transform anchor = AnchorFor(mode);
        position = default;
        rotation = default;
        if (!anchor) return false;

        position = anchor.position;
        rotation = anchor.rotation;
        if (mode == CameraViewMode.FPP)
        {
            rotation *= Quaternion.Euler(_pitch, 0f, 0f);
        }
        else if (mode == CameraViewMode.TopView)
        {
            position -= anchor.forward * _topZoom;
        }
        else
        {
            position -= anchor.forward * _thirdPersonZoom;
            Vector3 pivot = LookPoint;
            position = pivot + Quaternion.AngleAxis(_pitch, anchor.right) * (position - pivot);
            Vector3 toPlayer = pivot - position;
            rotation = LockTppPitch(rotation, toPlayer);
        }
        return true;
    }

    private Vector3 ResolveWallCollision(Vector3 desired, bool smoothReturn)
    {
        ThirdPersonCameraSettingSO settings = TppSettings;
        if (_currentMode != CameraViewMode.TPP || !settings || settings.WallMask.value == 0) return desired;
        Vector3 pivot = LookPoint;
        Vector3 offset = desired - pivot;
        float distance = offset.magnitude;
        if (distance < 0.0001f) return desired;

        if (Physics.SphereCast(pivot, settings.CollisionRadius, offset / distance,
            out RaycastHit hit, distance, settings.WallMask, QueryTriggerInteraction.Ignore))
        {
            _collisionCompressed = true;
            return pivot + offset / distance * Mathf.Max(0f, hit.distance - settings.WallClearance);
        }

        if (smoothReturn && _collisionCompressed && _mainCamera)
        {
            Vector3 restored = Vector3.Lerp(_mainCamera.transform.position, desired,
                1f - Mathf.Exp(-settings.RestoreSpeed * Time.deltaTime));
            if ((restored - desired).sqrMagnitude < 0.0001f) _collisionCompressed = false;
            return restored;
        }
        _collisionCompressed = false;
        return desired;
    }

    private void PlaceMain(Vector3 position, Quaternion rotation, bool smoothReturn = false)
    {
        if (!_mainCamera) return;
        Vector3 requestedPosition = position;
        position = ResolveWallCollision(position, smoothReturn);
        if (_currentMode == CameraViewMode.TPP && (position - requestedPosition).sqrMagnitude > 0.0001f)
        {
            Vector3 toPlayer = LookPoint - position;
            rotation = LockTppPitch(rotation, toPlayer);
        }
        _mainCamera.transform.SetPositionAndRotation(position, rotation);
    }

    private void SnapMainToCurrentView(bool smoothReturn = false)
    {
        if (TryGetDesiredPose(_currentMode, out Vector3 position, out Quaternion rotation))
            PlaceMain(position, rotation, smoothReturn);
    }

    private void SelectFpp(InputAction.CallbackContext _) => SetFPP();
    private void SelectTop(InputAction.CallbackContext _) => SetTopView(true);
    private void SelectTpp(InputAction.CallbackContext _) => SetTPP();
    private void CyclePerformed(InputAction.CallbackContext _) => CycleView();
    private void TopMirrorStarted(InputAction.CallbackContext _) { _topHeld = true; RefreshMirror(); }
    private void TopMirrorReleased(InputAction.CallbackContext _) { _topHeld = false; RefreshMirror(); }
    private void BackMirrorStarted(InputAction.CallbackContext _) { _backHeld = true; RefreshMirror(); }
    private void BackMirrorReleased(InputAction.CallbackContext _) { _backHeld = false; RefreshMirror(); }

    private void RefreshMirror()
    {
        if (_isLoadingScene) return;

        Camera desired = _topHeld ? _topMirrorViewCamera : _backHeld ? _backMirrorViewCamera : null;
        if (desired == _activeMirrorCamera) return;
        if (desired && _transitionRoutine != null && _animationMode == CameraAnimationMode.SkipAnimation)
            CancelTransition();
        _activeMirrorCamera = desired;
        _stateMachine.ChangeState(desired ? _mirrorViewState : _gameViewState);
    }

    private void SetOutputCamera(Camera active)
    {
        if (_mainCamera) _mainCamera.enabled = _mainCamera == active;
        if (_topMirrorViewCamera) _topMirrorViewCamera.enabled = _topMirrorViewCamera == active;
        if (_backMirrorViewCamera) _backMirrorViewCamera.enabled = _backMirrorViewCamera == active;
        DisableViewCamera(_firstPersonViewCamera);
        DisableViewCamera(_topViewCamera);
        DisableViewCamera(_thirdPersonViewCamera);
    }

    private static void DisableViewCamera(CameraViewAnchor anchor)
    {
        if (anchor && anchor.TryGetComponent(out Camera camera)) camera.enabled = false;
    }

    private bool CanChangeView() => Application.isPlaying && !_isLoadingScene
        && _activeMirrorCamera == null
        && (!_playerStatus || _playerStatus.GetIsIdle());

    private void RequestView(CameraViewMode mode)
    {
        if (!CanChangeView() || mode == _currentMode || !AnchorFor(mode)) return;
        CancelTransition();
        if (mode != CameraViewMode.FPP)
            SetModelVisibility(false);
        _transitionRoutine = StartCoroutine(_animationMode == CameraAnimationMode.Animate
            ? AnimateTo(mode) : FadeTo(mode));
    }

    private IEnumerator AnimateTo(CameraViewMode mode)
    {
        Vector3 startPosition = _mainCamera.transform.position;
        Quaternion startRotation = _mainCamera.transform.rotation;
        _currentMode = mode;
        CameraSettingSO settings = SettingsFor(mode);
        float duration = settings ? settings.TransitionDuration : 0.5f;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = settings ? settings.EvaluateTransition(elapsed / duration) : Mathf.Clamp01(elapsed / duration);
            if (TryGetDesiredPose(mode, out Vector3 targetPosition, out Quaternion targetRotation))
                PlaceMain(Vector3.LerpUnclamped(startPosition, targetPosition, progress),
                    Quaternion.SlerpUnclamped(startRotation, targetRotation, progress));
            yield return null;
        }
        SnapMainToCurrentView();
        ApplyModelVisibility();
        _transitionRoutine = null;
    }

    private IEnumerator FadeTo(CameraViewMode mode)
    {
        if (CanPlayFadeAnimation())
        {
            _fadeAnimator.speed = _fadeSpeed;
            _fadeAnimator.Play(_fadeIn.name, 0, 0f);
            yield return new WaitForSecondsRealtime(_fadeIn.length / _fadeSpeed);
        }
        _currentMode = mode;
        SnapMainToCurrentView();
        ApplyModelVisibility();
        if (CanPlayFadeAnimation())
        {
            _fadeAnimator.Play(_fadeOut.name, 0, 0f);
            yield return new WaitForSecondsRealtime(_fadeOut.length / _fadeSpeed);
        }
        ClearFade();
        _transitionRoutine = null;
    }

    private void CancelTransition()
    {
        if (_transitionRoutine != null) StopCoroutine(_transitionRoutine);
        _transitionRoutine = null;
        ClearFade();
    }

    private void ClearFade()
    {
        if (CanPlayFadeAnimation() && _fadeAnimator.isActiveAndEnabled && _fadeAnimator.gameObject.activeInHierarchy)
            _fadeAnimator.Play("Idel", 0, 0f);
        if (_fadeImage)
        {
            Color color = _fadeImage.color;
            color.a = 0f;
            _fadeImage.color = color;
        }
    }

    private bool CanPlayFadeAnimation()
    {
        return _fadeAnimator &&
            _fadeAnimator.runtimeAnimatorController &&
            _fadeIn &&
            _fadeOut;
    }

    private void ApplyModelVisibility()
    {
        SetModelVisibility(_currentMode == CameraViewMode.FPP);
    }

    private void SetModelVisibility(bool firstPerson)
    {
        if (_playerModel) _playerModel.SetActive(!firstPerson);
        if (_firstPersonModel) _firstPersonModel.SetActive(firstPerson);
    }

    public void CycleView() => RequestView(_currentMode switch
    {
        CameraViewMode.FPP => CameraViewMode.TopView,
        CameraViewMode.TopView => CameraViewMode.TPP,
        _ => CameraViewMode.FPP
    });

    public void SetFPP() => RequestView(CameraViewMode.FPP);
    public void SetTPP() => RequestView(CameraViewMode.TPP);
    public void SetTopView(bool enabled) => RequestView(enabled ? CameraViewMode.TopView : CameraViewMode.TPP);
    public void ToggleTopView() => SetTopView(_currentMode != CameraViewMode.TopView);
    public void ToggleView() => CycleView();
    public void SetCameraFollow(bool state) => _isFollowing = state;

    private sealed class GameViewState : BaseState<CameraController>
    {
        public override void OnEnter() => Context.SetOutputCamera(Context._mainCamera);
        public override void OnUpdate() => Context.TickGameView();
    }

    private sealed class MirrorViewState : BaseState<CameraController>
    {
        public override void OnEnter() => Context.SetOutputCamera(Context._activeMirrorCamera);
        public override void OnUpdate() => Context.TickGameView();
    }
}
