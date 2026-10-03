using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using NaughtyAttributes;

[ExecuteAlways]
public class CameraController : MonoBehaviour
{
    [Header("Target Settings")]
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

    [Header("Input Settings")]
    [SerializeField, Required] private InputActionReference _rotateClockwiseKey;
    [SerializeField, Required] private InputActionReference _rotateCounterClockwiseKey;

    private readonly Queue<float> _rotationQueue = new();
    private Coroutine _rotationRoutine;

    private enum CameraType
    {
        MainCamera,
        SecondaryCamera,
        ThirdCamera
    }

    public bool IsRotating => _rotationRoutine != null;

    private void Reset()
    {
        Setup();
    }

    private void OnEnable()
    {
        Setup();

        if (!Application.isPlaying) return;

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

        _rotationQueue.Clear();
        _rotationRoutine = null;
    }

    private void LateUpdate()
    {
        if (!_executeAlways && !Application.isPlaying) return;

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
            }
        }

        if (_resetRotationOnAwake)
        {
            transform.rotation = Quaternion.identity;
        }
    }

    private void UpdateCamera()
    {
        if (!_cameraBox || !_followTarget || !_isFollowing) return;

        _cameraBox.position = _followTarget.position;

        foreach (var camera in _cameras)
        {
            if (!camera.Key) continue;

            camera.Key.transform.LookAt(_followTarget);
        }
    }

    private void EnqueueRotation(float degrees)
    {
        Debug.Log($"Enqueueing rotation: {degrees} degrees");

        if (!_cameraBox) return;

        if (_maxQueuedRotations > 0 && _rotationQueue.Count >= _maxQueuedRotations) return;

        _rotationQueue.Enqueue(degrees);

        _rotationRoutine ??= StartCoroutine(ProcessRotationQueue());
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