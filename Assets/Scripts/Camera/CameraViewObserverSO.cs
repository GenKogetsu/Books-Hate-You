#region Imports
using System;
using UnityEngine;
#endregion
#region CameraViewObserverSO

[CreateAssetMenu(fileName = "CameraViewObserver", menuName = "Game/Camera/Camera View Observer")]
public sealed class CameraViewObserverSO : ScriptableObject
{
    public event Action<CameraViewMode> OnCameraViewChanged;

    public void Publish(CameraViewMode mode)
    {
        OnCameraViewChanged?.Invoke(mode);
    }
}
#endregion
