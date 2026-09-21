using UnityEngine;

namespace Kogetsu.Library.Core
{
    public class CameraController : MonoBehaviour
    {
        [SerializeField] private List<CameraSetting> _cameraList = new();
    }
}