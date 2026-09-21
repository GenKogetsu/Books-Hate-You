using System;
using Kogetsu.Library.DesignPatternCore;

namespace Kogetsu.Library.Core
{
    /// <summary>
    /// Click-to-move ability สำหรับ 2D
    /// - ผู้ใช้คลิกจุดหมายบนพื้น → ตัวละครวิ่งไปยังจุดนั้น
    /// - หยุดเมื่อถึงระยะ StoppingDistance หรือชนกับ SolidLayer
    /// - อัปเดต CurrentDirection / LastFacingDirection ระหว่างเดินให้ flip system ทำงานถูก
    /// </summary>
    [Serializable]
    public class ClickMoveAbility2D : BaseMoveAbility2D
    {
        [Header("Click Move Settings")]
        [SerializeField] protected LayerMask GroundLayer;
        [SerializeField] protected LayerMask SolidLayer;

        [Tooltip("ระยะเผื่อก่อนชนเป้าหมายหรือกำแพง")]
        [Range(0.05f, 0.5f)]
        [SerializeField] protected float StoppingDistance = 0.1f;

        [Header("Debug")]
        [SerializeField] protected bool ShowDebugRay = true;
        [Tooltip("ปรับสเกลของเส้น Debug ได้อิสระ (ปกติ = 1)")]
        [SerializeField] protected float DebugShapeScale = 1f;

        // ─── Runtime ───────────────────────────────────────────────────────
        protected Vector3    TargetPosition;
        protected bool       HasTarget;
        protected Camera     MainCamera;
        protected RaycastHit2D Hit;
        protected Collider2D Collider;

        // ─── State ─────────────────────────────────────────────────────────
        protected override void OnEnter()
        {
            MainCamera = Camera.main;
            if (Context?.Transform != null && Collider == null)
                Collider = Context.Transform.GetComponent<Collider2D>();

            if (Context.InputObserverChannel != null)
                Context.InputObserverChannel.OnLeftClickChannel += SetDestination;
        }

        protected override void OnExit()
        {
            if (Context.InputObserverChannel != null)
                Context.InputObserverChannel.OnLeftClickChannel -= SetDestination;

            // Clear movement state on exit so other abilities start fresh.
            HasTarget = false;
            ApplyDirection(Vector3.zero);
            StopVelocity();
        }

        // ─── Input ─────────────────────────────────────────────────────────
        protected virtual void SetDestination(ClickData clickData)
        {
            if (MainCamera == null) return;

            Vector3 worldPos = MainCamera.ScreenToWorldPoint(clickData.ClickPosition);
            worldPos.z = 0;

            RaycastHit2D hitInfo = Physics2D.Raycast(worldPos, Vector2.zero, Mathf.Infinity, GroundLayer);
            TargetPosition = hitInfo.collider != null ? hitInfo.point : (Vector2)worldPos;
            HasTarget = true;
        }

        // ─── FixedUpdate ───────────────────────────────────────────────────
        protected override void OnFixedUpdate()
        {
            if (!HasTarget || !HasStats || Context.Rb2 == null || Collider == null) return;

            Vector2 center   = Collider.bounds.center;
            float   radius   = Mathf.Max(Collider.bounds.extents.x, Collider.bounds.extents.y);
            float   totalStop = radius + StoppingDistance;

            Vector2 target2D = TargetPosition;
            Vector2 toTarget = target2D - center;
            Vector2 dir      = toTarget.normalized;
            float   dist     = toTarget.magnitude;

            Hit = Physics2D.CircleCast(center, radius, dir, StoppingDistance, SolidLayer);

            // Arrived or blocked → stop
            if (dist <= totalStop || Hit.collider != null)
            {
                HasTarget = false;
                StopVelocity();
                ApplyDirection(Vector3.zero);   // clear CurrentDirection; LastFacingDirection kept
                return;
            }

            // Moving → apply velocity and update direction for flip / facing systems
            SetVelocity(dir * MoveSpeed);
            ApplyDirection(new Vector3(dir.x, dir.y, 0f));
        }

#if UNITY_EDITOR
        public void DrawGizmos(Transform transform)
        {
            if (Application.isPlaying || !ShowDebugRay) return;

            if (!transform.TryGetComponent(out Collider2D col)) return;

            Gizmos.color  = Color.cyan;
            Gizmos.matrix = Matrix4x4.TRS(
                transform.position, transform.rotation, transform.lossyScale * DebugShapeScale);

            switch (col)
            {
                case BoxCollider2D    box:     Gizmos.DrawWireCube(box.offset, box.size);          break;
                case CircleCollider2D circle:  Gizmos.DrawWireSphere(circle.offset, circle.radius); break;
                case CapsuleCollider2D capsule: Gizmos.DrawWireCube(capsule.offset, capsule.size); break;
                default:
                    Gizmos.matrix = Matrix4x4.identity;
                    Gizmos.DrawWireCube(col.bounds.center, col.bounds.size);
                    break;
            }

            Gizmos.matrix = Matrix4x4.identity;
            float r = Mathf.Max(col.bounds.extents.x, col.bounds.extents.y);
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(col.bounds.center,
                col.bounds.center + transform.up * (r + StoppingDistance));
        }
#endif
    }
}
