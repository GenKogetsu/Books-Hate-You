using System;
using TMPro;
using UnityEngine.UI;

namespace Kogetsu.Library.Core
{
    [Serializable]
    public class VNDialogueArea
    {
        public VNDialogueMode DialogueMode;

        [Required]
        public RectTransform DialogueBox;

        [Required]
        public Animator DialogueAnimator;

        public TextMeshProUGUI SpeakerNameTMP;

        public TextMeshProUGUI DialogueTMP;
    }
}
