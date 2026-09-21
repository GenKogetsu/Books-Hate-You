using System;
using UnityEngine;

namespace Kogetsu.Library.Core
{
    /// <summary>
    /// 2D Move Controller — รองรับทุก ability ที่ extend BaseMoveAbility2D
    ///
    /// Inspector Setup:
    ///   InputObserverChannel → BasicMovementInputObserverSO.asset
    ///   MoveAbility          → BasicMoveAbility2D  (top-down / platformer)
    ///   Roll Ability         → RollMoveAbility2D   (optional)
    ///   Ground Check         → Empty GO ปลายเท้า (ถ้าเปิด Jump)
    ///   Ground Layer         → Layer ของพื้น     (ถ้าเปิด Jump)
    ///   Rig Mode             → Spritesheet / Cutout2D
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class MoveController2D : BaseMoveController<IMoveContext2D, BaseMoveAbility2D>, IMoveContext2D, IReturnableMovement
    {
        [ReadOnly]
        [SerializeField] protected Rigidbody2D Rb2;

        [Tooltip("Perspective Mode — rotates input vector on all axes for movement.")]
        [SerializeField] protected bool PerspectiveMode;

        [MaxValue(90f)]
        [SerializeField] protected Vector2 PerspectiveQuaternion2D;

        // ─── Roll ──────────────────────────────────────────────────────────
        [SerializeReference]
        private RollMoveAbility2D _rollAbility;

        // ─── Jump ──────────────────────────────────────────────────────────
        [Tooltip("Disable for top-down / maps that don't need jumping.\n" +
                 "ค่านี้จะถูก push ไปยัง BasicMoveAbility2D._enableJump ก่อน OnEnter()")]
        [SerializeField] private bool _enableJump = true;

        [Tooltip("Empty GO ปลายเท้า — จุดกลาง OverlapCircle")]
        [SerializeField] private Transform _groundCheck;

        [Tooltip("รัศมี OverlapCircle (หน่วยเมตร)")]
        [SerializeField] private float _groundRadius = 0.12f;

        [Tooltip("Layer ของพื้น/platform ที่นับว่า grounded")]
        [SerializeField] private LayerMask _groundLayer;

        // ─── Flip ──────────────────────────────────────────────────────
        [Tooltip("พลิกตัวละครตามทิศที่เดิน โดย scale X = ±1")]
        [SerializeField] private bool _enableFlip = false;

        [Tooltip("Transform ที่จะ flip — ถ้าว่างใช้ transform ของ object นี้เอง")]
        [SerializeField] private Transform _flipTarget;

        // ─── Runtime ───────────────────────────────────────────────────────
        private BaseMoveAbility2D _normalAbility;

        // ─── Interface ─────────────────────────────────────────────────────
        protected override IMoveContext2D GetContext() => this;
        Rigidbody2D IMoveContext2D.Rb2 => Rb2;

        // ─── AbilitySetUp — push _enableJump before OnEnter() ─────────────
        protected override void AbilitySetUp()
        {
            if (MoveAbility is BasicMoveAbility2D basic)
                basic._enableJump = _enableJump;

            base.AbilitySetUp();
        }

        // ─── Lifecycle ─────────────────────────────────────────────────────
        protected override void Awake()
        {
            base.Awake();                           // calls AbilitySetUp() → OnEnter()
            _normalAbility = MoveAbility;

            _rollAbility?.Initialize(GetContext());

            if (InputObserverChannel != null)
                InputObserverChannel.OnRollChannel += TryRoll;
        }

        protected override void Update()
        {
            base.Update();
            _rollAbility?.TickCooldown(Time.deltaTime);
        }

        protected override void FixedUpdate()
        {
            UpdateGroundCheck();
            base.FixedUpdate();
            UpdateFlip();
        }

        // ─── Flip ──────────────────────────────────────────────────────
        private void UpdateFlip()
        {
            if (!_enableFlip) return;
            float dirX = LastFacingDirection.x;
            if (Mathf.Approximately(dirX, 0f)) return;

            var target = _flipTarget != null ? _flipTarget : transform;
            var scale  = target.localScale;
            scale.x    = Mathf.Abs(scale.x) * (dirX < 0f ? -1f : 1f);
            target.localScale = scale;
        }

        private void OnDestroy()
        {
            if (InputObserverChannel != null)
                InputObserverChannel.OnRollChannel -= TryRoll;
        }

        // ─── Ground Check ──────────────────────────────────────────────────
        private void UpdateGroundCheck()
        {
            if (!_enableJump || _groundCheck == null) return;
            IsGrounded = Physics2D.OverlapCircle(_groundCheck.position, _groundRadius, _groundLayer);
        }

        // ─── Roll ──────────────────────────────────────────────────────────
        private void TryRoll()
        {
            if (_rollAbility is null || _rollAbility.IsOnCooldown) return;
            StateMachine.ChangeState(_rollAbility);
        }

        public void ReturnToNormalMovement() => StateMachine.ChangeState(_normalAbility);

        // ─── TransformInput ────────────────────────────────────────────────
        public override float VerticalVelocity => Rb2 != null ? Rb2.linearVelocity.y : 0f;

        protected override Vector3 TransformInput(Vector3 input)
        {
            if (!PerspectiveMode || input == Vector3.zero) return input;

            var rotation = new Vector3(PerspectiveQuaternion2D.x, 0, PerspectiveQuaternion2D.y);
            Quaternion quaternion = Quaternion.Euler(rotation);
            return quaternion * input;
        }

        // ─── Helpers ───────────────────────────────────────────────────────
        public Rigidbody2D GetRigidbody2D() => Rb2;
        public bool        GetPerspectiveMode()           => PerspectiveMode;
        public Vector2     GetPerspectiveQuaternion2D()   => PerspectiveQuaternion2D;

#if UNITY_EDITOR
        protected virtual void OnDrawGizmosSelected()
        {
            if (MoveAbility is ClickMoveAbility2D clickAbility)
                clickAbility.DrawGizmos(transform);

            if (!_enableJump || _groundCheck == null) return;
            Gizmos.color = IsGrounded
                ? new Color(0.2f, 1f, 0.2f, 0.8f)
                : new Color(1f, 0.2f, 0.2f, 0.8f);
            Gizmos.DrawWireSphere(_groundCheck.position, _groundRadius);
        }

        protected override void OnValidate()
        {
            if (Application.isPlaying) return;

            if (!Rb2) TryGetComponent(out Rb2);

            if (PerspectiveQuaternion2D.x < 0) PerspectiveQuaternion2D.x *= -1;
            if (PerspectiveQuaternion2D.y < 0) PerspectiveQuaternion2D.y *= -1;
        }
#endif
    }
}
