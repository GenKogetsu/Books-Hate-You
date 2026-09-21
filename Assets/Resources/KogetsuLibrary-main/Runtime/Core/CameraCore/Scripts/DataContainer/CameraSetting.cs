using UnityEngine;

namespace Kogetsu.Library.Core
{
    public struct CameraSetting
    {
        [Required]
        public Camera CameraObject;

        public Vector3 Offset;
    }
}
