using UnityEngine;
using System.Collections.Generic;

public class CameraController : MonoBehaviour
{
    [Header("Target Settings")]
    [SerializeField] private Transform _cameraBox;
    [SerializeField] private Dictionary<Camera, CameraType> _cameras = new();
    [SerializeField] private Transform _followTarget;

    [Header("Camera Settings")]
    [SerializeField] private bool _isFollowing = true;

    private enum CameraType
    {
        MainCamera,
        SecondaryCamera,
        ThirdCamera
    }

    private void Reset()
    {
        Setup();
    }

    private void Awake()
    {
        Reset();
    }

    private void LateUpdate()
    {
        if (_followTarget && _isFollowing)
        {
            Vector3 targetPosition = Vector3.Lerp(_cameraBox.transform.position, _followTarget.position , Time.deltaTime);

            _cameraBox.transform.position = targetPosition;

            foreach (var camera in _cameras)
            {
                if (!camera.Key) continue;

                camera.Key.transform.LookAt(targetPosition);
            }
        }
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
        if (!_cameraBox) this.TryGetComponent(out _cameraBox);

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
    }

    public void SetCameraFollow(bool state)
    {
        _isFollowing = state;
    }
}
