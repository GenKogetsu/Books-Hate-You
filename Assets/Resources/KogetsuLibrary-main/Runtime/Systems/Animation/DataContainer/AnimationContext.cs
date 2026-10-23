using System;
using UnityEngine.Playables;
using UnityEngine.Animations;

namespace Kogetsu.Library.Core
{
    [Serializable]
    public class AnimationContext
    {
        public IMoveContext MoveContext { get; set; }
        public DirectionMode Mode { get; set; }

        public GameObject CurrentActiveModel { get; set; }

        public Animator MainAnimator { get; set; }
        public PlayableGraph AnimationGraph { get; set; }
        public AnimationPlayableOutput PlayableOutput { get; set; }
        public AnimationClip CurrentClip { get; set; }
        public AnimationClipPlayable CurrentClipPlayable { get; set; }
    }
}
