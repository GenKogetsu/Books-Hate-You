using UnityEngine;

namespace Kogetsu.Library.Core
{
    [RequireComponent(typeof(Rigidbody))]
    public class MoveController3D : BaseMoveController<IMoveContext3D, BaseMoveAbility3D>, IMoveContext3D
    {
        [SerializeField] protected Rigidbody Rb;

        // ─── Jump ──────────────────────────────────────────────────────────
        [Tooltip("Disable for floating / maps that don't need jumping.")]
        [SerializeField] private bool _enableJump = true;

        [Tooltip("Empty GO ปลายเท้า — จุดกลาง CheckSphere")]
        [SerializeField] private Transform _groundCheck;

        [Tooltip("รัศมี CheckSphere (หน่วยเมตร)")]
        [SerializeField] private float _groundRadius = 0.12f;

        [Tooltip("Layer ของพื้น/platform ที่นับว่า grounded")]
        [SerializeField] private LayerMask _groundLayer;

        // ─── Interface ─────────────────────────────────────────────────────
        protected override IMoveContext3D GetContext() => this;
        Rigidbody IMoveContext3D.Rb => Rb;
        public override float VerticalVelocity => Rb != null ? Rb.linearVelocity.y : 0f;

        // ─── Lifecycle ─────────────────────────────────────────────────────
        protected override void Awake()
        {
            if (Rb == null) TryGetComponent(out Rb);
            base.Awake();
        }

        protected override void FixedUpdate()
        {
            UpdateGroundCheck();
            base.FixedUpdate();
        }

        // ─── Ground Check ──────────────────────────────────────────────────
        private void UpdateGroundCheck()
        {
            if (!_enableJump || _groundCheck == null) return;
            IsGrounded = Physics.CheckSphere(_groundCheck.position, _groundRadius, _groundLayer);
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            if (!Application.isPlaying) Awake();
        }

        protected virtual void OnDrawGizmosSelected()
        {
            if (MoveAbility is ClickMoveAbility3D clickAbility)
                clickAbility.DrawGizmos(transform);

            if (!_enableJump || _groundCheck == null) return;
            Gizmos.color = IsGrounded
                ? new Color(0.2f, 1f, 0.2f, 0.8f)
                : new Color(1f, 0.2f, 0.2f, 0.8f);
            Gizmos.DrawWireSphere(_groundCheck.position, _groundRadius);
        }
#endif
    }
}
