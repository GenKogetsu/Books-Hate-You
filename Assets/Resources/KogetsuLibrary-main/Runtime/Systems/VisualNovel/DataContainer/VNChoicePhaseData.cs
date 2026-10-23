using System;

namespace Kogetsu.Library.Core
{
    [Serializable]
    public class VNChoicePhaseData
    {

        public VNDialogueMode DialogueMode;

        public bool ChangeBackground;
        public Sprite BackgroundSprite;

        public bool OverrideBmgClip;
        public AudioClip BmgClip;

        public bool UseEnterName; // เปิด panel ให้ผู้เล่นกรอกชื่อก่อน phase นี้จะดำเนินต่อ

        public bool UseCutScene;
        public VNCutSceneMode CutSceneMode;

        public bool UseAmbientEvent;
        public VNAmbientEventType AmbientEventType;

        public bool UseVoiceoverClip;
        public AudioClip VoiceoverClip;

        public bool OverrideDialogueBoxAnimation;
        public AnimationClip DialogueBoxAnimation;

        public List<VNSpeakerData> Speakers = new();
    }
}
