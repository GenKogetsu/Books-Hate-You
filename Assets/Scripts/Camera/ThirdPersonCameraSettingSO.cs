#region Imports
using UnityEngine;
#endregion
#region ThirdPersonCameraSettingSO

[CreateAssetMenu(fileName = "TPP Camera Settings", menuName = "Game/Camera/TPP Settings")]
public sealed class ThirdPersonCameraSettingSO : CameraSettingSO
{
    [Header("Orbit")]
    [SerializeField, Range(0f, 89f)] private float _maxLookDown = 70f;
    [SerializeField, Range(0f, 89f)] private float _maxLookUp = 70f;

    [Header("Wall Collision")]
    [SerializeField] private LayerMask _wallMask;
    [SerializeField, Min(0.01f)] private float _collisionRadius = 0.25f;
    [SerializeField, Min(0f)] private float _wallClearance = 0.1f;
    [SerializeField, Min(0f)] private float _restoreSpeed = 8f;

    public LayerMask WallMask => _wallMask;
    public float CollisionRadius => _collisionRadius;
    public float WallClearance => _wallClearance;
    public float RestoreSpeed => _restoreSpeed;
    public float ClampPitch(float pitch) => Mathf.Clamp(pitch, -_maxLookUp, _maxLookDown);

    private void OnEnable()
    {
        if (_wallMask.value == 0) _wallMask = LayerMask.GetMask("Wall");
    }
}
#endregion
