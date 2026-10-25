using UnityEngine;

namespace Kogetsu.Library.Core
{
    [System.Serializable]
    public struct CameraSetting
    {
        [Required]
        public Camera CameraObject;

        public Vector3 Offset;
    }
}
