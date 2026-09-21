using UnityEngine.Playables;
using UnityEngine.Animations;
using Kogetsu.Library.DesignPatternCore;
using UnityEngine.Accessibility;
using Kogetsu.Library.Extension;

namespace Kogetsu.Library.Core
{
    [RequireComponent(typeof(Animator))]
    public class AnimationController2D : BaseAnimationController
    {
        [SerializeField] protected MoveController2D MoveController;
        [SerializeField] protected Animator AnimatorControl;

        [Header("Settings")]
        [SerializeField] protected Animation2DMode AnimationMode;
        [SerializeField] protected DirectionMode DirectionMode;

        [Space(10)]
        [SerializeField] protected List<Transform> VanishingPoints;
        [SerializeField] protected float MinScale = 0.5f;
        [SerializeField] protected float MaxScale = 1.0f;
        [SerializeField] protected float MaxDistance = 10f;


        protected DirectionalAnimationState DirectionalState;

#if UNITY_EDITOR
        protected virtual void OnValidate()
        {
            if (Application.isPlaying) return;

            AssignComponent();
            ResizeArray(DirectionMode.ToByte());
        }
#endif

        protected override void Awake()
        {
            base.Awake();
            AssignComponent();
        }

        protected virtual void Start()
        {
            SetupAnimator();
            SetupStateMachine();
        }

        protected override void FixedUpdate()
        {
            base.FixedUpdate();
            UpdatePerspectiveScale();
        }

        protected override void AssignComponent()
        {
            if (!MoveController)
            {
                if (this.TryGetComponent(out MoveController)) return;
                MoveController = this.GetComponentInParent<MoveController2D>();
            }

            if (!AnimatorControl) this.TryGetComponent(out AnimatorControl);
        }

        protected virtual void UpdatePerspectiveScale()
        {
            if (VanishingPoints == null || VanishingPoints.Count == 0) return;

            float minDistance = float.MaxValue;

            foreach (Transform point in VanishingPoints)
            {
                if (!point) continue;

                float dist = Vector3.Distance(transform.position, point.position);

                if (dist >= minDistance) continue;
                minDistance = dist;
            }

            if (minDistance != float.MaxValue)
            {
                float t = Mathf.Clamp01(minDistance / MaxDistance);
                float currentScale = Mathf.Lerp(MinScale, MaxScale, t);

                MoveController.transform.localScale = new(currentScale, currentScale, currentScale);
            }
        }

        protected virtual void SetupAnimator()
        {
            Context.MoveContext = MoveController;
            Context.Mode = MoveController.GetDirectionMode();

            Context.MainAnimator = AnimatorControl;
            Context.AnimationGraph = PlayableGraph.Create(gameObject.name + "_AnimGraph");
            Context.AnimationGraph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);
            Context.PlayableOutput = AnimationPlayableOutput.Create(Context.AnimationGraph, "Animation", AnimatorControl);
        }

        protected virtual void SetupStateMachine()
        {
            DirectionalState = new DirectionalAnimationState(IdleAnimationArray, MoveAnimationArray, JumpAnimationArray, FallAnimationArray);
            DirectionalState.Initialize(Context);
            StateMachine.ChangeState(DirectionalState);
        }

        protected override void PlayOverrideAnimation(AnimationClip clip)
        {
            if (!clip) return;

            var overrideState = new OverrideAnimationState(clip, DirectionalState, StateMachine);
            overrideState.Initialize(Context);
            StateMachine.ChangeState(overrideState);
        }
    }
}