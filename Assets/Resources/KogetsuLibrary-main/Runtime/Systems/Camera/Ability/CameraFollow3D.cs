using UnityEngine;
using UnityEngine.InputSystem;
using Kogetsu.Library.Core;

namespace Kogetsu.Library.DesignPatternCore
{
    [ExecuteAlways]
    public class CameraFollow3D : FollowAbility
    {
        [SerializeField] private bool _executeAlways = true;

        [Header("Transform Offset")]
        [SerializeField] private Vector3 _rotationOffset = Vector3.zero;

        [Header("Orbit Camera Settings")]
        [SerializeField] private bool  _enableMouseLook  = true;
        [SerializeField] private float _mouseSensitivity = 0.2f;
        [SerializeField] private float _smoothSpeed = 15f;

        [Header("Pitch Limits")]
        [SerializeField] private float _minPitch = -20f;
        [SerializeField] private float _maxPitch = 70f;

        [Header("Zoom Settings")]
        [SerializeField] private float _zoomSensitivity = 0.01f;
        [SerializeField] private float _minDistance = 0f;
        [SerializeField] private float _maxDistance = 15f;

        private float _yaw = 0f;
        private float _pitch = 0f;
        private float _currentDistance = 5f;


        public void ToggleMouseLook() => _enableMouseLook = !_enableMouseLook;
        public void SetMouseLook(bool enable) => _enableMouseLook = enable;

        private void Start()
        {
            _yaw   = 0f;
            _pitch = 0f;

            _currentDistance = Mathf.Abs(Offset.z);

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        protected override void LateUpdate()
        {
            if (!_executeAlways && !Application.isPlaying) return;

            if (FollowTarget == null) return;

            Vector2 mouseDelta = Vector2.zero;
            float scrollDelta = 0f;

            if (Mouse.current != null)
            {
                if (_enableMouseLook) mouseDelta = Mouse.current.delta.ReadValue();
                scrollDelta = Mouse.current.scroll.ReadValue().y;
            }

            if (_enableMouseLook)
            {
                _yaw   += mouseDelta.x * _mouseSensitivity;
                _pitch -= mouseDelta.y * _mouseSensitivity;
                _pitch  = Mathf.Clamp(_pitch, _minPitch, _maxPitch);
            }

            if (scrollDelta != 0f)
            {
                _currentDistance -= scrollDelta * _zoomSensitivity;
                _currentDistance = Mathf.Clamp(_currentDistance, _minDistance, _maxDistance);
                Offset.z = -_currentDistance;
            }

            Quaternion targetRotation = Quaternion.Euler(_pitch + _rotationOffset.x, _yaw + _rotationOffset.y, _rotationOffset.z);

            Vector3 lookTarget = FollowTarget.position + new Vector3(0, Offset.y, 0);

            Vector3 desiredPosition = lookTarget + (targetRotation * new Vector3(Offset.x, 0, Offset.z));

            transform.SetPositionAndRotation(Vector3.Lerp(transform.position, desiredPosition, _smoothSpeed * Time.deltaTime), Quaternion.Slerp(transform.rotation, targetRotation, _smoothSpeed * Time.deltaTime));
        }
    }
}