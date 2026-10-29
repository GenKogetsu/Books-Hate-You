#region Imports
using UnityEngine;
#endregion
#region Player transform snapshot

[System.Serializable]
public struct PlayerTransformSnapshot
{
    [SerializeField] private Vector3 _position;
    [SerializeField] private Quaternion _rotation;
    [SerializeField] private Vector3 _localScale;

    public static PlayerTransformSnapshot Identity => new()
    {
        _position = Vector3.zero,
        _rotation = Quaternion.identity,
        _localScale = Vector3.one
    };

    public void ApplyTo(Transform target)
    {
        target.SetPositionAndRotation(_position, _rotation);
        target.localScale = _localScale;
    }
}

#endregion

#region Player scene warp state
[CreateAssetMenu(fileName = "PlayerSceneWarpState", menuName = "Data/Player Scene Warp State")]
public sealed class PlayerSceneWarpState : ScriptableObject
{
    [SerializeField] private bool _hasPendingTransform;
    [SerializeField] private PlayerTransformSnapshot _pendingTransform;

    public void SetPendingTransform(PlayerTransformSnapshot transformSnapshot)
    {
        _pendingTransform = transformSnapshot;
        _hasPendingTransform = true;
    }

    public bool TryGetPendingTransform(out PlayerTransformSnapshot transformSnapshot)
    {
        transformSnapshot = _pendingTransform;
        return _hasPendingTransform;
    }

    public void ClearPendingTransform()
    {
        _hasPendingTransform = false;
        _pendingTransform = PlayerTransformSnapshot.Identity;
    }
}
#endregion
