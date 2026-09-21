using System;

namespace Kogetsu.Library.Core
{
    /// <summary>
    /// <para> Summary : </para>
    /// <para> (TH) : โครงสร้างของคำสั่ง 1 รายการที่จะให้ตัวละครแสดงผล </para>
    /// <para> (EN) : Structure of a single action command for a character to execute. </para>
    /// </summary>
    [Serializable]
    public struct VNAction
    {
        public bool UseEmotion;
        public string EmotionName;

        public bool UseBehaviorAnimation;
        public string BehaviorAnimationName;

        public bool UseSoundEffect;
        public string SoundEffectName;

        public bool UseDelay;
        public float DelayTime;
    }
}