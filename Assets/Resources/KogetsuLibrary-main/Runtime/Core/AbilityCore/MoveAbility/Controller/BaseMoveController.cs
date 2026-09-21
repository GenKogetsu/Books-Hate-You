using System;
using Kogetsu.Library.Attribute;
using Kogetsu.Library.DesignPatternCore;
using Kogetsu.Library.Extension;
using UnityEngine;

namespace Kogetsu.Library.Core
{
    public abstract class BaseMoveController<TContext, TAbility> : MonoBehaviour, IMoveContext
        where TContext : class, IMoveContext
        where TAbility : BaseMoveAbility<TContext>
    {
        [SerializeField] protected StatsController Stats;

        [Header("Assign")]
        [Required]
        [SerializeField] protected BasicMovementInputObserverSO InputObserverChannel;

        [Space(10f)]
        [MoveAbilitySelector]
        [SerializeReference] protected TAbility MoveAbility;

        [SerializeField] protected DirectionMode DirectionMode;

        [Header("Debug")]
        [ReadOnly]
        [SerializeField] protected Vector3 CurrentDirection;

        [ReadOnly]
        [SerializeField] protected Vector3 LastFacingDirection = Vector3.down;

        [ReadOnly]
        [SerializeField] public bool IsGrounded = true;

        [Tooltip("จำนวนครั้งที่กระโดดได้ต่อเนื่อง (1 = กระโดดครั้งเดียว, 2 = double jump)")]
        [SerializeField] protected int MaxJumpCount = 1;

        int _jumpCount;

        protected StateMachine<TContext> StateMachine;

        public virtual float VerticalVelocity => 0f;

        #region implements interface
        Transform IMoveContext.Transform => this.transform;
        StatsController IMoveContext.Stats => this.Stats;
        BasicMovementInputObserverSO IMoveContext.InputObserverChannel => this.InputObserverChannel;
        Vector3 IMoveContext.CurrentDirection 
        { 
            get => this.CurrentDirection; 
            set => this.CurrentDirection = value; 
        }
        Vector3 IMoveContext.LastFacingDirection 
        { 
            get => this.LastFacingDirection; 
            set => this.LastFacingDirection = value; 
        }
        bool IMoveContext.IsGrounded
        {
            get => this.IsGrounded;
            set
            {
                // แตะพื้น → reset ตัวนับกระโดดทันที
                if (value && !this.IsGrounded) _jumpCount = 0;
                this.IsGrounded = value;
            }
        }

        bool IMoveContext.CanJump      => IsGrounded || _jumpCount < MaxJumpCount;
        void IMoveContext.ConsumeJump() => _jumpCount++;
        void IMoveContext.ResetJumps()  => _jumpCount = 0;

        Vector3 IMoveContext.SnapDirection(Vector3 rawInput) => this.SnapDirection(rawInput);

        Vector3 IMoveContext.TransformInput(Vector3 input) => this.TransformInput(input);

        #endregion //implements interface


#if UNITY_EDITOR
        protected virtual void OnValidate()
        {
            if (Application.isPlaying) return;

            AssignComponent();
        }
#endif

        protected virtual void Awake() => AbilitySetUp();

        protected virtual void Update() => StateMachine.Update();

        protected virtual void FixedUpdate() => StateMachine.FixedUpdate();

        protected virtual void AbilitySetUp()
        {
            AssignComponent();

            StateMachine = new StateMachine<TContext>();
            MoveAbility.Initialize(GetContext());
            StateMachine.ChangeState(MoveAbility);
        }

        protected virtual void AssignComponent()
        {
            if (!Stats) TryGetComponent(out Stats);
        }

        protected Vector3 SnapDirection(Vector3 rawInput)
        {
            if (rawInput == Vector3.zero || DirectionMode == DirectionMode.None) return rawInput;

            byte dirs = DirectionMode.ToByte();
            if (dirs <= 1) return rawInput;

            bool is3D = Mathf.Abs(rawInput.y) < 0.001f && Mathf.Abs(rawInput.z) > 0.001f;
            float h = rawInput.x;
            float v = is3D ? rawInput.z : rawInput.y;

            float step = 360f / dirs;
            float snapped = Mathf.Round(Mathf.Atan2(v, h) * Mathf.Rad2Deg / step) * step;

            float sh = Mathf.Cos(snapped * Mathf.Deg2Rad);
            float sv = Mathf.Sin(snapped * Mathf.Deg2Rad);
            float mag = rawInput.magnitude;

            return is3D
                ? new Vector3(sh, rawInput.y, sv).normalized * mag
                : new Vector3(sh, sv, rawInput.z).normalized * mag;
        }

        protected virtual Vector3 TransformInput(Vector3 input) => input;


        protected abstract TContext GetContext();

        public DirectionMode GetDirectionMode() => DirectionMode;

    }
}
