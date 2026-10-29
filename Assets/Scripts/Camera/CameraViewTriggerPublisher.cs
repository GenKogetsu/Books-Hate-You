#region Imports
using UnityEngine;
#endregion
#region CameraViewTriggerPublisher

[RequireComponent(typeof(Collider))]
public sealed class CameraViewTriggerPublisher : MonoBehaviour
{
    #region Fields and configuration
    [SerializeField] private CameraViewMode _cameraViewMode = CameraViewMode.TPP;
    [SerializeField] private CameraViewObserverSO _observer;
    [SerializeField] private TargetTag _targetTags = TargetTag.Player;

    #endregion
    #region Unity lifecycle
    private void Reset()
    {
        Collider trigger = GetComponent<Collider>();
        trigger.isTrigger = true;
    }

    #endregion
    #region Event handlers
    private void OnTriggerEnter(Collider other)
    {
        Transform player = other.transform.root;
        if (!_targetTags.MatchesUnityTag(player.tag))
            return;

        CameraController cameraController =
            player.GetComponentInChildren<CameraController>();

        if (cameraController != null)
        {
            switch (_cameraViewMode)
            {
                case CameraViewMode.FPP:
                    cameraController.SetFPP();
                    break;
                case CameraViewMode.TopView:
                    cameraController.SetTopView(true);
                    break;
                default:
                    cameraController.SetTPP();
                    break;
            }
        }

        _observer?.Publish(_cameraViewMode);
    }
    #endregion
}
#endregion
