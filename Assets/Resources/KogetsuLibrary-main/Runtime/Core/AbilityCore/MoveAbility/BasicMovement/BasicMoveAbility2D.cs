using System;
using Kogetsu.Library.DesignPatternCore;

namespace Kogetsu.Library.Core
{
    /// <summary>
    /// 2D move ability รองรับ 2 โหมด:
    ///
    ///  SideViewMode = false (default — top-down / free-move)
    ///    • ตั้ง linearVelocity เต็มทิศ (x + y)
    ///    • กระโดดแบบ AddForce ทันที ไม่มี coyote/buffer
    ///
    ///  SideViewMode = true (platformer)
    ///    • ตั้งเฉพาะ velocity.x ไม่แตะ y → gravity ทำงานตามปกติ
    ///    • ระบบกระโดดเต็ม: coyote time / jump buffer / variable height
    ///    • EnableJump ต้อง true จึงจะ subscribe jump channels
    ///    • EnableJump ถูก set โดย MoveController2D.AbilitySetUp() ก่อน OnEnter()
    /// </summary>
    [Serializable]
    public class BasicMoveAbility2D : BaseMoveAbility2D
    {
        // ─── Mode ──────────────────────────────────────────────────────────
        [Header("Mode")]
        [Tooltip("Platformer/Side-view: X-only velocity + full jump system.\n" +
                 "Off = top-down free-move (full velocity, instant jump).")]
        [SerializeField] private bool _sideViewMode = false;

        // ─── Jump ──────────────────────────────────────────────────────────
        [Header("Jump")]
        [Tooltip("Enable jump input. In top-down mode jump is instant AddForce.\n" +
                 "In side-view mode the full coyote/buffer/variable system is used.\n" +
                 "Set automatically by MoveController2D from its inspector toggle.")]
        [SerializeField] internal bool _enableJump = true;

        [Header("Jump Tuning (Side-View only)")]
        [Tooltip("Seconds after leaving a ledge that the player can still jump.")]
        [SerializeField] private float _coyoteTime = 0.12f;

        [Tooltip("Seconds in advance a jump input can be buffered.")]
        [SerializeField] private float _jumpBuffer = 0.12f;

        [Tooltip("Release jump mid-air to cut jump height.")]
        [SerializeField] private bool _variableJump = true;

        [Range(0f, 1f)]
        [Tooltip("velocity.y is multiplied by this when jump is released mid-air.")]
        [SerializeField] private float _jumpCutMultiplier = 0.45f;

        // ─── Runtime ───────────────────────────────────────────────────────
        private float _coyoteTimer;
        private float _jumpBufferTimer;
        private bool  _wasGrounded;
        private bool  _isJumping;
        private bool  _jumpHeld;

        // ─── Subscribe / Unsubscribe ───────────────────────────────────────
        protected override void OnEnter()
        {
            if (Context.InputObserverChannel == null) return;
            Context.InputObserverChannel.OnMoveChannel += SetInput;

            if (!_enableJump) return;

            if (_sideViewMode)
            {
                Context.InputObserverChannel.OnJumpChannel         += OnJumpPressed;
                Context.InputObserverChannel.OnJumpReleasedChannel += OnJumpReleased;
            }
            else
            {
                Context.InputObserverChannel.OnJumpChannel += ExecuteJumpInstant;
            }
        }

        protected override void OnExit()
        {
            if (Context.InputObserverChannel == null) return;
            Context.InputObserverChannel.OnMoveChannel -= SetInput;

            if (!_enableJump) return;

            if (_sideViewMode)
            {
                Context.InputObserverChannel.OnJumpChannel         -= OnJumpPressed;
                Context.InputObserverChannel.OnJumpReleasedChannel -= OnJumpReleased;
            }
            else
            {
                Context.InputObserverChannel.OnJumpChannel -= ExecuteJumpInstant;
            }
        }

        // ─── FixedUpdate ───────────────────────────────────────────────────
        protected override void OnFixedUpdate()
        {
            if (!HasStats) return;

            if (_sideViewMode)
            {
                if (_enableJump) TickJump();
                ApplySideViewMovement();
            }
            else
            {
                ApplyTopDownMovement();
            }
        }

        // ─── Movement ──────────────────────────────────────────────────────

        /// <summary>
        /// Top-down: velocity follows the raw (pre-snap) transformed direction
        /// so diagonal movement is smooth, while <see cref="ApplyDirection"/> uses
        /// the snapped value for display / flip purposes.
        /// </summary>
        private void ApplyTopDownMovement()
        {
            Vector3 rawDir  = TransformInput2D(CurrentInput);          // clamp + transform
            Vector3 snapped = Context.SnapDirection(rawDir);           // snap for display
            ApplyDirection(snapped);
            SetVelocity((Vector2)rawDir * MoveSpeed);
        }

        /// <summary>
        /// Side-view: X-only velocity keeps gravity intact.
        /// Snap is applied to both velocity and display direction.
        /// </summary>
        private void ApplySideViewMovement()
        {
            Vector3 snapped = ProcessInput2D(CurrentInput);            // clamp + transform + snap
            ApplyDirection(snapped);
            SetVelocityX(snapped.x * MoveSpeed);
        }

        // ─── Jump System (Side-View) ────────────────────────────────────────
        private void TickJump()
        {
            bool grounded = Context.IsGrounded;

            if (grounded)
            {
                _coyoteTimer = _coyoteTime;
                if (!_wasGrounded)
                {
                    _isJumping = false;
                    if (EventBus.Instance)
                        EventBus.Instance.Publish(new PlayerLandEvent());
                }
            }
            else if (_coyoteTimer > 0f)
            {
                _coyoteTimer -= Time.fixedDeltaTime;
            }

            _wasGrounded = grounded;

            if (_jumpBufferTimer > 0f)
                _jumpBufferTimer -= Time.fixedDeltaTime;

            // Execute jump when buffer + coyote both active
            if (_jumpBufferTimer > 0f && _coyoteTimer > 0f && Context.CanJump)
            {
                _jumpBufferTimer   = 0f;
                _coyoteTimer       = 0f;
                _isJumping         = true;
                Context.IsGrounded = false;
                Context.ConsumeJump();

                var vel = Context.Rb2.linearVelocity;
                SetVelocity(new Vector2(vel.x, 0f));
                Context.Rb2.AddForce(Vector2.up * JumpForce, ForceMode2D.Impulse);

                if (EventBus.Instance)
                    EventBus.Instance.Publish(new PlayerJumpEvent());
            }

            // Variable-height cut
            if (_variableJump && _isJumping && !_jumpHeld && Context.Rb2.linearVelocity.y > 0f)
            {
                var vel = Context.Rb2.linearVelocity;
                SetVelocity(new Vector2(vel.x, vel.y * _jumpCutMultiplier));
            }
        }

        // ─── Top-Down Jump (instant) ────────────────────────────────────────
        private void ExecuteJumpInstant()
        {
            if (!HasStats)          return;
            if (!Context.CanJump)   return;   // ป้องกันกระโดดซ้ำขณะลอยอยู่
            Context.ConsumeJump();
            Context.Rb2.AddForce(Vector2.up * JumpForce, ForceMode2D.Impulse);
        }

        // ─── Input Callbacks ───────────────────────────────────────────────
        private void OnJumpPressed()
        {
            _jumpBufferTimer = _jumpBuffer;
            _jumpHeld        = true;
        }

        private void OnJumpReleased() => _jumpHeld = false;
    }
}
