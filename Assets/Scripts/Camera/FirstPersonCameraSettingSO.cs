#region Imports
using UnityEngine;
#endregion
#region FirstPersonCameraSettingSO

[CreateAssetMenu(fileName = "FPP Camera Settings", menuName = "Game/Camera/FPP Settings")]
public sealed class FirstPersonCameraSettingSO : CameraSettingSO
{
    [SerializeField, Range(0f, 89f)] private float _maxLookDown = 80f;
    [SerializeField, Range(0f, 89f)] private float _maxLookUp = 80f;

    public float ClampPitch(float pitch) => Mathf.Clamp(pitch, -_maxLookUp, _maxLookDown);
}
#endregion
