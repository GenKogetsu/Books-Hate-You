using System;
using Kogetsu.Library.DesignPatternCore;

namespace Kogetsu.Library.Core
{
    [Serializable]
    public class ClickMoveAbility3D : BaseMoveAbility3D
    {
        [Header("Click Move Settings")]
        [SerializeField] protected LayerMask GroundLayer;
        [SerializeField] protected LayerMask SolidLayer;

        [Tooltip("ระยะเผื่อก่อนชนเป้าหมายหรือกำแพง")]
        [Range(0.01f, 0.5f)]
        [SerializeField] protected float StoppingDistance = 0.1f;

        [Range(15f, 100f)]
        [SerializeField] protected float RotationSpeed = 15f;

        [Header("Debug")]
        [SerializeField] protected bool ShowDebugRay = true;
        [Tooltip("ปรับสเกลของเส้น Debug ได้อิสระ (ปกติ = 1)")]
        [SerializeField] protected float DebugShapeScale = 1f;

        protected Vector3 TargetPosition;
        protected bool HasTarget;
        protected Camera MainCamera;
        protected RaycastHit Hit;
        protected Collider Collider;

        protected override void OnEnter()
        {
            MainCamera = Camera.main;
            if (Context?.Transform != null && Collider == null)
                Collider = Context.Transform.GetComponent<Collider>();

            if (Context.InputObserverChannel != null)
                Context.InputObserverChannel.OnLeftClickChannel += SetDestination;
        }

        protected override void OnExit()
        {
            if (Context.InputObserverChannel != null)
                Context.InputObserverChannel.OnLeftClickChannel -= SetDestination;
        }

        protected override void OnFixedUpdate()
        {
            if (!HasTarget || Context?.Stats == null || Context.Rb == null || Collider == null) return;

            Vector3 center = Collider.bounds.center;
            float radius = Mathf.Max(Collider.bounds.extents.x, Collider.bounds.extents.z);
            float totalStop = radius + StoppingDistance;

            Vector3 targetFlat = new(TargetPosition.x, center.y, TargetPosition.z);
            Vector3 dir = (targetFlat - center).normalized;
            float dist = Vector3.Distance(center, targetFlat);

            bool hitWall = Physics.SphereCast(center, radius, dir, out Hit, StoppingDistance, SolidLayer);

            if (dist <= totalStop || hitWall)
            {
                HasTarget = false;
                Context.Rb.linearVelocity = new(0, Context.Rb.linearVelocity.y, 0);
                return;
            }

            Vector3 vel = dir * Context.Stats.GetMoveSpeed();
            Context.Rb.linearVelocity = new(vel.x, Context.Rb.linearVelocity.y, vel.z);

            if (dir != Vector3.zero)
            {
                Quaternion targetRot = Quaternion.LookRotation(dir);
                Context.Rb.MoveRotation(Quaternion.Slerp(Context.Rb.rotation, targetRot, Time.fixedDeltaTime * RotationSpeed));
            }
        }

#if UNITY_EDITOR
        public void DrawGizmos(Transform transform)
        {
            if (Application.isPlaying || !ShowDebugRay) return;

            if (!transform.TryGetComponent(out Collider col)) return;

            Gizmos.color = Color.cyan;
            Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale * DebugShapeScale);

            switch (col)
            {
                case BoxCollider box:
                    Gizmos.DrawWireCube(box.center, box.size);
                    break;
                case SphereCollider sphere:
                    Gizmos.DrawWireSphere(sphere.center, sphere.radius);
                    break;
                case CapsuleCollider capsule:
                    Gizmos.DrawWireCube(capsule.center, new Vector3(capsule.radius * 2, capsule.height, capsule.radius * 2));
                    break;
                case MeshCollider mesh when mesh.sharedMesh != null:
                    Gizmos.DrawWireMesh(mesh.sharedMesh, Vector3.zero, Quaternion.identity, Vector3.one);
                    break;
                default:
                    Gizmos.matrix = Matrix4x4.identity;
                    Gizmos.DrawWireCube(col.bounds.center, col.bounds.size);
                    break;
            }

            Gizmos.matrix = Matrix4x4.identity;
            float r = Mathf.Max(col.bounds.extents.x, col.bounds.extents.z);
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(col.bounds.center, col.bounds.center + transform.forward * (r + StoppingDistance));
        }
#endif
        protected virtual void SetDestination(ClickData clickData)
        {
            if (MainCamera == null) return;

            Ray ray = MainCamera.ScreenPointToRay(clickData.ClickPosition);
            if (Physics.Raycast(ray, out Hit, 1000f, GroundLayer))
            {
                TargetPosition = Hit.point;
                HasTarget = true;
            }
        }
    }
}
