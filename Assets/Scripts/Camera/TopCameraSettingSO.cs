using UnityEngine;

[CreateAssetMenu(fileName = "Top Camera Settings", menuName = "Game/Camera/Top Settings")]
public sealed class TopCameraSettingSO : CameraSettingSO
{
    [Header("Zoom Offset From View Camera")]
    [SerializeField, Min(0f)] private float _zoomInLimit = 4f;
    [SerializeField, Min(0f)] private float _zoomOutLimit = 8f;
    [SerializeField, Min(0f)] private float _zoomSpeed = 0.02f;

    [Header("Mouse Tilt")]
    [SerializeField, Range(0f, 89f)] private float _maxTilt = 60f;

    public float ZoomSpeed => _zoomSpeed;
    public float ClampZoom(float offset) => Mathf.Clamp(offset, -_zoomInLimit, _zoomOutLimit);
    public float ClampTilt(float tilt) => Mathf.Clamp(tilt, -_maxTilt, _maxTilt);
}
