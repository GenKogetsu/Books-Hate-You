using UnityEngine;

[CreateAssetMenu(fileName = "PlayerSceneWarpState", menuName = "Data/Player Scene Warp State")]
public sealed class PlayerSceneWarpState : ScriptableObject
{
    [SerializeField] private bool _hasPendingPosition;
    [SerializeField] private Vector3 _pendingPosition;

    public void SetPendingPosition(Vector3 position)
    {
        _pendingPosition = position;
        _hasPendingPosition = true;
    }

    public bool TryGetPendingPosition(out Vector3 position)
    {
        position = _pendingPosition;
        return _hasPendingPosition;
    }

    public void ClearPendingPosition()
    {
        _hasPendingPosition = false;
        _pendingPosition = default;
    }
}
