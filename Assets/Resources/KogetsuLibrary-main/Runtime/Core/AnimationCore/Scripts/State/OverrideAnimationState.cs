using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;
using Kogetsu.Library.DesignPatternCore;

namespace Kogetsu.Library.Core
{
    /// <summary>
    /// <para>Summary :</para>
    /// <para>คลาส State สำหรับเล่น Animation ชั่วคราว และจะสลับกลับไปยัง State ก่อนหน้าเมื่อเล่นจบ</para>
    /// </summary>
    public class OverrideAnimationState : BaseState<AnimationContext>, IFixedUpdateState
    {
        private readonly AnimationClip _clip;
        private readonly BaseState<AnimationContext> _previousState;
        private readonly StateMachine<AnimationContext> _stateMachine;

        private float _timer;
        private bool _isInitialized;

        public OverrideAnimationState(AnimationClip clip, BaseState<AnimationContext> previousState, StateMachine<AnimationContext> stateMachine)
        {
            _clip = clip;
            _previousState = previousState;
            _stateMachine = stateMachine;
        }

        void IFixedUpdateState.OnFixedUpdate()
        {
            if (!_isInitialized)
            {
                PlayOverrideClip();
                _isInitialized = true;
            }

            _timer -= Time.fixedDeltaTime;
            if (_timer <= 0f)
            {
                _stateMachine.ChangeState(_previousState);
            }
        }

        private void PlayOverrideClip()
        {
            if (Context.MainAnimator == null || _clip == null) return;

            Context.CurrentClip = _clip;

            if (Context.CurrentClipPlayable.IsValid())
                Context.CurrentClipPlayable.Destroy();

            Context.CurrentClipPlayable = AnimationClipPlayable.Create(Context.AnimationGraph, _clip);
            Context.CurrentClipPlayable.SetSpeed(1f);
            Context.PlayableOutput.SetSourcePlayable(Context.CurrentClipPlayable);

            if (!Context.AnimationGraph.IsPlaying())
                Context.AnimationGraph.Play();

            _timer = _clip.length;
        }
    }
}