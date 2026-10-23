namespace Kogetsu.Library.Core
{
    using UnityEngine;

    public class FollowAbility : MonoBehaviour
    {
        [SerializeField] protected Transform FollowTarget;

        [SerializeField] protected Camera CameraTarget;

        [SerializeField] protected bool CameraLookat;

        public Vector3 Offset = new(0, 0, 0);

        protected virtual void LateUpdate()
        {
            if (FollowTarget != null)
            {
                transform.position = FollowTarget.transform.position + Offset;
            }

            if (CameraLookat)
            {
                transform.LookAt(transform.position + CameraTarget.transform.rotation * Vector3.forward,
                    CameraTarget.transform.rotation * Vector3.up);
            }
        }
    }
}