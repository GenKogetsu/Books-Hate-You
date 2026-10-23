using UnityEngine;

[RequireComponent(typeof(Collider))]
public sealed class CameraViewTriggerPublisher : MonoBehaviour
{
    [SerializeField] private CameraViewMode _cameraViewMode = CameraViewMode.TPP;
    [SerializeField] private CameraViewObserverSO _observer;
    [SerializeField] private TargetTag _targetTags = TargetTag.Player;

    private void Reset()
    {
        Collider trigger = GetComponent<Collider>();
        trigger.isTrigger = true;
    }

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
}
