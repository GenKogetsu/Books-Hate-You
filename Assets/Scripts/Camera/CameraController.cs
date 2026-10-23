using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using NaughtyAttributes;

[ExecuteAlways]
public class CameraController : MonoBehaviour
{
    [Header("Reference Settings")]
    [SerializeField] private PlayerStatus _playerStatus;
    [SerializeField] private Transform _cameraBox;
    [SerializeField] private Dictionary<Camera, CameraType> _cameras = new();
    [SerializeField] private Transform _followTarget;

    [Header("Camera Settings")]
    [SerializeField] private bool _executeAlways = true;
    [SerializeField] private bool _resetRotationOnAwake = true;
    [SerializeField] private bool _isFollowing = true;

    [Header("Rotation Settings")]
    [SerializeField, Min(0.01f)] private float _rotateDuration = 0.25f;
    [SerializeField] private AnimationCurve _rotateCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    [SerializeField, Min(0)] private int _maxQueuedRotations = 4;

    [Header("View Settings")]
    [SerializeField] private bool _startInFirstPerson = false;
    [SerializeField] private bool _startInTopView = false;
    [SerializeField, Min(0.1f)] private float _topViewHeight = 12f;
    [SerializeField] private Vector3 _firstPersonLocalOffset = new(0f, 1.6f, 0f);
    [SerializeField, Min(0.01f)] private float _viewTransitionDuration = 0.5f;
    [SerializeField] private AnimationCurve _viewCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("First Person Settings")]
    [SerializeField] private bool _lockCursorInFirstPerson = true;
    [SerializeField, Min(0f)] private float _lookSensitivity = 0.1f;
    [SerializeField, Range(0f, 89f)] private float _maxPitch = 80f;
    [SerializeField] private GameObject _playerModel;
    [SerializeField] private GameObject _firstPersonModel;

    [Header("Input Settings")]
    [SerializeField, Required] private InputActionReference _rotateClockwiseKey;
    [SerializeField, Required] private InputActionReference _rotateCounterClockwiseKey;
    [SerializeField, Required] private InputActionReference _toggleViewKey;
    [SerializeField] private InputActionReference _lookInput;

    private readonly Queue<float> _rotationQueue = new();
    private readonly Dictionary<Camera, Vector3> _baseLocalPositions = new();
    private Coroutine _rotationRoutine;
    private Coroutine _viewRoutine;
    private Quaternion _yawStartRotation;
    private Quaternion _yawTargetRotation;
    private bool _snapYawOnTransition;

    private bool _isFirstPerson;
    private bool _isTopView;

    private float _viewT = 1f;

    private float _lookYaw;
    private float _lookPitch;

    private enum CameraType
    {
        MainCamera,
        SecondaryCamera,
        ThirdCamera
    }

    public bool IsRotating => _rotationRoutine != null;
    public bool IsFirstPerson => _isFirstPerson;
    public bool IsTopView => _isTopView;
    public bool IsViewTransitioning => _viewRoutine != null;


    public Quaternion LookYawRotation
    {
        get
        {
            float mouseWeight = 1f - _viewT;
            return _cameraBox.rotation * Quaternion.Euler(0f, _lookYaw * mouseWeight, 0f);
        }
    }

    private void Reset()
    {
        Setup();
    }

    private void OnEnable()
    {
        Setup();

        if (!Application.isPlaying) return;

        _isFirstPerson = _startInFirstPerson;
        _isTopView = _startInTopView;
        if (_isTopView) _isFirstPerson = false;
        _viewT = _isFirstPerson ? 0f : 1f;
        _lookYaw = 0f;
        _lookPitch = 0f;

        if (_rotateClockwiseKey)
        {
            _rotateClockwiseKey.action.Enable();
            _rotateClockwiseKey.action.performed += RotateCameraBoxQ;
        }

        if (_rotateCounterClockwiseKey)
        {
            _rotateCounterClockwiseKey.action.Enable();
            _rotateCounterClockwiseKey.action.performed += RotateCameraBoxE;
        }

        if (_toggleViewKey)
        {
            _toggleViewKey.action.Enable();
            _toggleViewKey.action.performed += ToggleViewPerformed;
        }

        if (_lookInput)
        {
            _lookInput.action.Enable();
        }

        ApplyModelVisibility(_isFirstPerson);
        ApplyCursorState();
    }

    private void OnDisable()
    {
        if (!Application.isPlaying) return;

        if (_rotateClockwiseKey)
        {
            _rotateClockwiseKey.action.performed -= RotateCameraBoxQ;
            _rotateClockwiseKey.action.Disable();
        }

        if (_rotateCounterClockwiseKey)
        {
            _rotateCounterClockwiseKey.action.performed -= RotateCameraBoxE;
            _rotateCounterClockwiseKey.action.Disable();
        }

        if (_toggleViewKey)
        {
            _toggleViewKey.action.performed -= ToggleViewPerformed;
            _toggleViewKey.action.Disable();
        }

        if (_lookInput)
        {
            _lookInput.action.Disable();
        }

        _rotationQueue.Clear();
        _rotationRoutine = null;
        _viewRoutine = null;
    }

    private void LateUpdate()
    {
        if (!Application.isPlaying)
        {
            _isFirstPerson = false;
            _viewT = 1f;
            _viewRoutine = null;
            _lookYaw = 0f;
            _lookPitch = 0f;
        }

        if (!_executeAlways && !Application.isPlaying) return;

        ApplyLook();
        UpdateCamera();
    }

    private CameraType GetCameraType(Camera camera) => camera.name switch
    {
        "MainCamera" => CameraType.MainCamera,
        "SecondaryCamera" => CameraType.SecondaryCamera,
        "ThirdCamera" => CameraType.ThirdCamera,
        _ => CameraType.MainCamera,
    };

    private void Setup()
    {
        if (!_cameraBox) TryGetComponent(out _cameraBox);

        _cameras.Clear();

        if (_cameraBox != null)
        {
            Camera[] foundCameras = _cameraBox.GetComponentsInChildren<Camera>(true);

            foreach (var cam in foundCameras)
            {
                if (_cameras.ContainsKey(cam)) continue;

                _cameras.Add(cam, GetCameraType(cam));

                // Capture the original offset only once
                if (!_baseLocalPositions.ContainsKey(cam))
                {
                    _baseLocalPositions[cam] = cam.transform.localPosition;
                }
            }
        }

        if (_resetRotationOnAwake)
        {
            transform.rotation = Quaternion.identity;
        }
    }

    private void ApplyLook()
    {
        if (!Application.isPlaying || !_isFirstPerson || _isTopView || IsViewTransitioning || !_lookInput) return;

        Vector2 delta = _lookInput.action.ReadValue<Vector2>();

        _lookYaw += delta.x * _lookSensitivity;
        _lookPitch = Mathf.Clamp(_lookPitch - delta.y * _lookSensitivity, -_maxPitch, _maxPitch);
    }

    private void UpdateCamera()
    {
        if (!_cameraBox || !_followTarget || !_isFollowing) return;

        _cameraBox.position = _followTarget.position;

        if (_isTopView)
        {
            Vector3 topPosition = _followTarget.position + Vector3.up * _topViewHeight;
            Quaternion topRotation = Quaternion.Euler(90f, _cameraBox.eulerAngles.y, 0f);

            foreach (var pair in _cameras)
            {
                if (!pair.Key) continue;
                pair.Key.transform.position = topPosition;
                pair.Key.transform.rotation = topRotation;
            }

            return;
        }

        Quaternion firstPersonRotation = LookYawRotation * Quaternion.Euler(_lookPitch, 0f, 0f);

        foreach (var pair in _cameras)
        {
            Camera cam = pair.Key;
            if (!cam) continue;

            Transform camTransform = cam.transform;

            if (_baseLocalPositions.TryGetValue(cam, out Vector3 baseLocal))
            {
                camTransform.localPosition = Vector3.LerpUnclamped(_firstPersonLocalOffset, baseLocal, _viewT);
            }

            Vector3 toTarget = _followTarget.position - camTransform.position;
            Quaternion lookRotation = toTarget.sqrMagnitude > 1e-6f
                ? Quaternion.LookRotation(toTarget)
                : firstPersonRotation;

            camTransform.rotation = Quaternion.SlerpUnclamped(firstPersonRotation, lookRotation, _viewT);
        }
    }

    private void ToggleViewPerformed(InputAction.CallbackContext context) => ToggleView();

    public void ToggleView()
    {
        if (!Application.isPlaying) return;
        if (!CanToggleView()) return;

        if (_isTopView)
            SetTopView(false);

        _isFirstPerson = !_isFirstPerson;

        if (_viewRoutine != null) StopCoroutine(_viewRoutine);

        if (_isFirstPerson)
        {
            if (_rotationRoutine != null) StopCoroutine(_rotationRoutine);
            _rotationRoutine = null;
            _rotationQueue.Clear();

            _lookYaw = 0f;
            _lookPitch = 0f;
            _snapYawOnTransition = false;
        }
        else
        {
            Quaternion current = _cameraBox.rotation * Quaternion.Euler(0f, _lookYaw, 0f);
            float snappedYaw = Mathf.Round(current.eulerAngles.y / 90f) * 90f;

            _cameraBox.rotation = current;
            _lookYaw = 0f;

            _yawStartRotation = current;
            _yawTargetRotation = Quaternion.Euler(0f, snappedYaw, 0f);
            _snapYawOnTransition = true;

            ApplyModelVisibility(false);
        }

        _viewRoutine = StartCoroutine(ViewTransitionRoutine(_isFirstPerson ? 0f : 1f));
    }

    public void SetTopView(bool enabled)
    {
        if (!Application.isPlaying || !CanToggleView()) return;

        _isTopView = enabled;
        if (enabled)
        {
            _isFirstPerson = false;
            if (_viewRoutine != null) StopCoroutine(_viewRoutine);
            _viewRoutine = null;
            _viewT = 1f;
            ApplyModelVisibility(false);
        }
        else
        {
            ApplyModelVisibility(false);
        }
    }

    public void ToggleTopView()
    {
        SetTopView(!_isTopView);
    }

    public void SetTPP()
    {
        if (_isTopView)
        {
            _isTopView = false;
            _viewT = 1f;
            ApplyModelVisibility(false);
        }

        if (_isFirstPerson)
            ToggleView();
    }

    public void SetFPP()
    {
        if (_isTopView)
        {
            _isTopView = false;
            _viewT = 1f;
            ApplyModelVisibility(false);
        }

        if (!_isFirstPerson)
            ToggleView();
    }

    private void ApplyModelVisibility(bool firstPerson)
    {
        if (_playerModel) _playerModel.SetActive(!firstPerson);
        if (_firstPersonModel) _firstPersonModel.SetActive(firstPerson);
    }

    private void ApplyCursorState()
    {
        if (!Application.isPlaying) return;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private IEnumerator ViewTransitionRoutine(float target)
    {
        float start = _viewT;
        float elapsed = 0f;

        while (elapsed < _viewTransitionDuration)
        {
            elapsed += Time.deltaTime;
            float t = _viewCurve.Evaluate(Mathf.Clamp01(elapsed / _viewTransitionDuration));
            _viewT = Mathf.LerpUnclamped(start, target, t);

            if (_snapYawOnTransition)
            {
                _cameraBox.rotation = Quaternion.SlerpUnclamped(_yawStartRotation, _yawTargetRotation, t);
            }

            yield return null;
        }

        _viewT = target;
        _viewRoutine = null;

        if (_snapYawOnTransition)
        {
            _cameraBox.rotation = _yawTargetRotation;
            _snapYawOnTransition = false;
        }

        if (_isFirstPerson)
        {
            ApplyModelVisibility(true);
            ApplyCursorState();
        }
    }

    private void EnqueueRotation(float degrees)
    {
        if (_isFirstPerson || IsViewTransitioning || !_cameraBox) return;

        if (_maxQueuedRotations > 0 && _rotationQueue.Count >= _maxQueuedRotations) return;

        _rotationQueue.Enqueue(degrees);

        _rotationRoutine ??= StartCoroutine(ProcessRotationQueue());
    }

    private bool CanToggleView()
    {
        if (!_playerStatus) return true;

        return _playerStatus.GetIsIdle();
    }

    private IEnumerator ProcessRotationQueue()
    {
        while (_rotationQueue.Count > 0)
        {
            float degrees = _rotationQueue.Dequeue();
            yield return RotateRoutine(degrees);
        }

        _rotationRoutine = null;
    }

    private IEnumerator RotateRoutine(float degrees)
    {
        Quaternion start = _cameraBox.rotation;
        Quaternion target = Quaternion.Euler(0f, degrees, 0f) * start;

        float elapsed = 0f;

        while (elapsed < _rotateDuration)
        {
            elapsed += Time.deltaTime;
            float t = _rotateCurve.Evaluate(Mathf.Clamp01(elapsed / _rotateDuration));
            _cameraBox.rotation = Quaternion.SlerpUnclamped(start, target, t);
            yield return null;
        }

        _cameraBox.rotation = target;
    }

    private void RotateCameraBoxQ(InputAction.CallbackContext context) => EnqueueRotation(90f);

    private void RotateCameraBoxE(InputAction.CallbackContext context) => EnqueueRotation(-90f);

    public void SetCameraFollow(bool state) => _isFollowing = state;
}
