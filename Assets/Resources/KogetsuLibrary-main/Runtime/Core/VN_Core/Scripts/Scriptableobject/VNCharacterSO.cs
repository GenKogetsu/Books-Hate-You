using System;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

namespace Kogetsu.Library.Core
{
    /// <summary>
    /// <para> Summary : </para>
    /// <para> (TH) : ไฟล์ข้อมูลตัวละคร แยกการจัดการระหว่าง Emotion (หน้าตา) และ Animation (การเคลื่อนที่) </para>
    /// <para> (EN) : Character data file, separating Emotion (face) and Animation (movement) management. </para>
    /// </summary>
    [CreateAssetMenu(fileName = "NewVNCharacter", menuName = "KogetsuLibrary/Core/VN/Character")]
    public class VNCharacterSO : ScriptableObject
    {
        [Serializable]
        public struct VNEmotionData
        {
            [Required]
            public Sprite EmoteSprite;

            [Required]
            public Sprite EmoteIcon;

            [Required]
            public AnimationClip EmoteClip;
        }

        [Serializable]
        public struct VNBehaviorAnimationData
        {
            [Required]
            public AnimationClip BehaviorAnimationClip;
        }

        [Serializable]
        public struct VNSoundEffectData
        {
            [Required]
            public AudioClip SoundEffectClip;
        }

        public string CharacterName;
        public ushort CharacterID;
        public Sprite NameIcon; //new: displayed when VNNameDisplayMode.Icon is set on a speaker

        [Header("Relationship")]
        [SerializeField] private int _relationshipValue = 0;
        public int RelationshipValue => _relationshipValue;
        public Action<int> OnRelationshipChanged;

        public void AddRelationship(int delta)
        {
            _relationshipValue = Mathf.Clamp(_relationshipValue + delta, 0, 10);
            OnRelationshipChanged?.Invoke(_relationshipValue);
        }

        public void ResetRelationship()
        {
            _relationshipValue = 0;
            OnRelationshipChanged?.Invoke(_relationshipValue);
        }

        public void SetRelationship(int value)
        {
            _relationshipValue = Mathf.Clamp(value, 0, 10);
            OnRelationshipChanged?.Invoke(_relationshipValue);
        }

        [Header("Character Assets")]
        public List<VNEmotionData> Emotions = new();
        public List<VNBehaviorAnimationData> BehaviorAnimations = new();
        public List<VNSoundEffectData> SoundEffects = new();

        private Dictionary<string, VNEmotionData> _dicEmotions;
        private Dictionary<string, VNBehaviorAnimationData> _dicAnimations;
        private Dictionary<string, VNSoundEffectData> _dicSoundEffects;

        public Action<string> OnVNEmotionChannel;
        public Action<string> OnVNAnimationChannel;
        public Action<string> OnVNSoundEffectChannel;
        public Action        OnVNSkipChannel; // fired เมื่อผู้เล่น skip → ให้ Animator/SFX ไปจุดสิ้นสุดทันที

        public void SendVNEmotionSignel(string emotionName) => OnVNEmotionChannel?.Invoke(emotionName);

        public void SendVNAnimationSignal(string animationName) => OnVNAnimationChannel?.Invoke(animationName);

        public void SendVNSoundEffectSignel(string soundEffectName) => OnVNSoundEffectChannel?.Invoke(soundEffectName);

        public void SendVNSkipSignel() => OnVNSkipChannel?.Invoke();

        /// <summary>
        /// <para> Summary : </para>
        /// <para> (TH) : ค้นหาข้อมูล Emotion จากชื่อที่ระบุ </para>
        /// <para> (EN) : Finds the emotion data by the specified name. </para>
        /// <para> Return : </para>
        /// <para> (TH) : ข้อมูล Emotion ที่พบ หรือ null หากไม่พบ </para>
        /// </summary>
        public VNEmotionData? GetEmotion(string name)
        {
            if (_dicEmotions == null) InitializeDictionaries();
            return _dicEmotions.TryGetValue(name, out var data) ? data : null;
        }

        /// <summary>
        /// <para> Summary : </para>
        /// <para> (TH) : ค้นหาข้อมูล Animation จากชื่อที่ระบุ </para>
        /// <para> (EN) : Finds the animation data by the specified name. </para>
        /// <para> Return : </para>
        /// <para> (TH) : ข้อมูล Animation ที่พบ หรือ null หากไม่พบ </para>
        /// </summary>
        public VNBehaviorAnimationData? GetAnimation(string name)
        {
            if (_dicAnimations == null) InitializeDictionaries();
            return _dicAnimations.TryGetValue(name, out var data) ? data : null;
        }

        public VNSoundEffectData? GetSoundEffect(string name)
        {
            if (_dicSoundEffects == null) InitializeDictionaries();
            return _dicSoundEffects.TryGetValue(name, out var data) ? data : null;
        }

        /// <summary>
        /// <para> (TH) : คืนชื่อ Emotion แรกสุดที่มี EmoteClip — ใช้ตั้ง default ให้ action ที่ว่างเปล่า </para>
        /// <para> (EN) : Returns the name of the first valid Emotion clip, used to default-fill empty action fields. </para>
        /// </summary>
        public string GetFirstEmotionName()
        {
            foreach (var e in Emotions)
                if (e.EmoteClip != null) return e.EmoteClip.name;
            return null;
        }

        /// <summary>
        /// <para> (TH) : คืนชื่อ Animation แรกสุดที่มี BehaviorAnimationClip — ใช้ตั้ง default ให้ action ที่ว่างเปล่า </para>
        /// <para> (EN) : Returns the name of the first valid BehaviorAnimation clip, used to default-fill empty action fields. </para>
        /// </summary>
        public string GetFirstAnimationName()
        {
            foreach (var a in BehaviorAnimations)
                if (a.BehaviorAnimationClip != null) return a.BehaviorAnimationClip.name;
            return null;
        }

        /// <summary>
        /// <para> (TH) : คืนชื่อ SoundEffect แรกสุดที่มี SoundEffectClip — ใช้ตั้ง default ให้ action ที่ว่างเปล่า </para>
        /// <para> (EN) : Returns the name of the first valid SoundEffect clip, used to default-fill empty action fields. </para>
        /// </summary>
        public string GetFirstSoundEffectName()
        {
            foreach (var s in SoundEffects)
                if (s.SoundEffectClip != null) return s.SoundEffectClip.name;
            return null;
        }

        private void OnEnable()
        {
            InitializeDictionaries();
        }

        private void InitializeDictionaries()
        {
            // ป้องกันการสร้างซ้ำซ้อนถ้า Dictionary มีการ Initialize ไว้แล้ว
            if (_dicEmotions != null && _dicAnimations != null && _dicSoundEffects != null)
                return;

            _dicEmotions = InitializeDictionary(Emotions, emote => emote.EmoteClip, "Emotion");
            _dicAnimations = InitializeDictionary(BehaviorAnimations, anime => anime.BehaviorAnimationClip, "Animation");
            _dicSoundEffects = InitializeDictionary(SoundEffects, effect => effect.SoundEffectClip, "SoundEffect");
        }

        private Dictionary<string, T> InitializeDictionary<T>(List<T> list, Func<T, UnityEngine.Object> getAssetFunc, string typeName)
        {
            var dictionary = new Dictionary<string, T>();

            foreach (var item in list)
            {
                var asset = getAssetFunc(item);

                if (asset == null)
                {
#if UNITY_EDITOR
                    Debug.LogWarning($"<b><color=red>[Skip {typeName}]</color></b>" + Environment.NewLine
                        + $"<b><u><color=cyan>because</color></u></b> Asset Clip is <b><color=yellow><i>Null!!</i></color></b>");
#endif
                    continue;
                }

                var assetName = asset.name;

                if (dictionary.ContainsKey(assetName))
                {
#if UNITY_EDITOR
                    Debug.LogWarning($"<b><color=red>[Skip {assetName}]</color></b>" + Environment.NewLine
                       + $"<b><u><color=cyan>because</color></u></b> {typeName} is already available.");
#endif
                    continue;
                }

                dictionary.Add(assetName, item);
            }

            return dictionary;
        }
    }
}