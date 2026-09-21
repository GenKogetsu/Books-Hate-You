using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;
using Kogetsu.Library.DesignPatternCore;

namespace Kogetsu.Library.Core
{
    public class DirectionalAnimationState : BaseState<AnimationContext>, IFixedUpdateState
    {
        private readonly DirectionalAnimationData[] _idleData;
        private readonly DirectionalAnimationData[] _moveData;
        private readonly DirectionalAnimationData[] _jumpData;
        private readonly DirectionalAnimationData[] _fallData;
        private readonly bool _useAirborneStates;
        private readonly bool _showDebug;

        private string _currentDebugState = "";

        public DirectionalAnimationState(
            DirectionalAnimationData[] idleData,
            DirectionalAnimationData[] moveData,
            DirectionalAnimationData[] jumpData,
            DirectionalAnimationData[] fallData,
            bool useAirborneStates = false,
            bool showDebug = true)
        {
            _idleData = idleData;
            _moveData = moveData;
            _jumpData = jumpData;
            _fallData = fallData;
            _useAirborneStates = useAirborneStates;
            _showDebug = showDebug;
        }

        void IFixedUpdateState.OnFixedUpdate()
        {
            if (Context.MoveContext == null) return;

            Vector3 currentDir = Context.MoveContext.CurrentDirection;
            bool isMoving = currentDir.sqrMagnitude > 0.01f;

            if (isMoving) Context.MoveContext.LastFacingDirection = currentDir;

            DirectionalAnimationData[] dataArray;
            string stateName;

            if (_useAirborneStates && !Context.MoveContext.IsGrounded)
            {
                if (Context.MoveContext.VerticalVelocity > 0.05f) { dataArray = _jumpData; stateName = "Jump"; }
                else                                               { dataArray = _fallData; stateName = "Fall"; }
            }
            else
            {
                if (isMoving) { dataArray = _moveData; stateName = "Move"; }
                else          { dataArray = _idleData; stateName = "Idle"; }
            }

            if (_showDebug && _currentDebugState != stateName)
            {
                Debug.Log($"[Animation] State → {stateName}");
                _currentDebugState = stateName;
            }

            if (dataArray == null || dataArray.Length == 0) return;

            // Bias X slightly so left/right wins over up/down when walking diagonally
            Vector3 facing = Context.MoveContext.LastFacingDirection;
            Vector3 biased = new Vector3(facing.x * 1.1f, facing.y, facing.z).normalized;

            int bestMatch = 0;
            float maxDot = -1f;
            for (int i = 0; i < dataArray.Length; i++)
            {
                float dot = Vector3.Dot(biased, dataArray[i].Direction.normalized);
                if (dot > maxDot) { maxDot = dot; bestMatch = i; }
            }

            var best = dataArray[bestMatch];

            // Swap active model
            if (Context.CurrentActiveModel != best.Model)
            {
                if (Context.CurrentActiveModel) Context.CurrentActiveModel.SetActive(false);
                Context.CurrentActiveModel = best.Model;
                if (Context.CurrentActiveModel) Context.CurrentActiveModel.SetActive(true);
            }

            // Swap animation clip
            if (Context.MainAnimator != null && best.Clip != null && Context.CurrentClip != best.Clip)
            {
                Context.CurrentClip = best.Clip;

                if (Context.CurrentClipPlayable.IsValid())
                    Context.CurrentClipPlayable.Destroy();

                Context.CurrentClipPlayable = AnimationClipPlayable.Create(Context.AnimationGraph, best.Clip);
                Context.CurrentClipPlayable.SetSpeed(1f);
                Context.PlayableOutput.SetSourcePlayable(Context.CurrentClipPlayable);

                if (!Context.AnimationGraph.IsPlaying())
                    Context.AnimationGraph.Play();
            }
        }
    }
}
