using NaughtyAttributes;
using Kogetsu.Library.Core;
using UnityEngine.UI;

namespace Kogetsu.Library.DesignPatternCore
{
    #region Documentation - XML
    /// <summary>
    /// <para> Summary : </para>
    /// <para> (TH) : ตัวควบคุมการแสดงผลและแอนิเมชันของตัวละคร </para>
    /// <para> (EN) : Controller for character display and animation. </para>
    /// </summary>
    #endregion
    [RequireComponent(typeof(Animator))]
    public sealed class VNCharacterController : MonoBehaviour
    {
        [Header("AssignReferences")]

        [Required]
        [SerializeField] private Animator _characterBehaviorAniamtor;

        [Required]
        [SerializeField] private Animator _characterSpriteAnimator;

        [Required]
        [SerializeField] private Image _characterSprite;

        [Header("AssignData")]
        [Required]
        [SerializeField] private VNCharacterSO _characterData;

        [Header("Observer")]
        [Required]
        [SerializeField] private AudioObserverSO _audioObserver;

        #region Documentation - Notes
        // ── Character Actions ────────────────────────────────────────────────────
        #endregion

        private void OnEmotionSignel(string emotionName)
        {
            if (string.IsNullOrEmpty(emotionName)) return;

            var emotion = _characterData.GetEmotion(emotionName);
            if (emotion == null) return;

            _characterSprite.sprite = emotion.Value.EmoteSprite;
            _characterSpriteAnimator.Play(emotion.Value.EmoteClip.name);
        }

        private void OnAnimationSignel(string animationName)
        {
            if (string.IsNullOrEmpty(animationName)) return;

            var animation = _characterData.GetAnimation(animationName);
            if (animation == null) return;

            _characterBehaviorAniamtor.Play(animation.Value.BehaviorAnimationClip.name);
        }

        private void OnSoundEffectSignel(string soundEffectName)
        {
            if (string.IsNullOrEmpty(soundEffectName)) return;

            var soundEffect = _characterData.GetSoundEffect(soundEffectName);
            if (soundEffect == null) return;

            _audioObserver.SendSfxSignal(soundEffect.Value.SoundEffectClip);
        }

        #region Documentation - XML
        /// <summary>
        /// <para> Summary : </para>
        /// <para> (TH) : เรียกเมื่อผู้เล่น skip บทพูด — ให้ Animator ทั้งสองกระโดดไปจุดสิ้นสุดทันที และหยุด SFX </para>
        /// <para> (EN) : Called on player skip — jumps both Animators to the end of their current state and stops SFX. </para>
        /// </summary>
        #endregion
        private void OnSkipSignel()
        {
            JumpToEnd(_characterSpriteAnimator);
            JumpToEnd(_characterBehaviorAniamtor);
            _audioObserver?.SendSfxSignal(null); // หยุด SFX ที่กำลังเล่นอยู่
        }

        #region Documentation - XML
        /// <summary>
        /// <para> Summary : </para>
        /// <para> (TH) : กระโดด Animator ไปที่ normalized time = 1f (จุดสิ้นสุดของ state ปัจจุบัน) </para>
        /// <para> (EN) : Jumps the Animator to normalized time 1f — the end of its current state. </para>
        /// </summary>
        #endregion
        private static void JumpToEnd(Animator animator)
        {
            if (animator == null || !animator.gameObject.activeInHierarchy) return;
            var state = animator.GetCurrentAnimatorStateInfo(0);
            animator.Play(state.fullPathHash, 0, 1f);
        }

        #region Documentation - Notes
        // ── CutScene ─────────────────────────────────────────────────────────────
        #endregion

        private void OnCutScene(VNCutSceneEvent e)
        {
            _characterSprite.gameObject.SetActive(!e.IsCutScene);
        }

        #region Documentation - Notes
        // ── Lifecycle ────────────────────────────────────────────────────────────
        #endregion

        private void OnEnable()
        {
            _characterData.OnVNEmotionChannel     += OnEmotionSignel;
            _characterData.OnVNAnimationChannel   += OnAnimationSignel;
            _characterData.OnVNSoundEffectChannel += OnSoundEffectSignel;
            _characterData.OnVNSkipChannel        += OnSkipSignel;
            EventBus.Instance.Subscribe<VNCutSceneEvent>(OnCutScene);
        }

        private void OnDisable()
        {
            _characterData.OnVNEmotionChannel     -= OnEmotionSignel;
            _characterData.OnVNAnimationChannel   -= OnAnimationSignel;
            _characterData.OnVNSoundEffectChannel -= OnSoundEffectSignel;
            _characterData.OnVNSkipChannel        -= OnSkipSignel;
            if (EventBus.Instance != null) EventBus.Instance.Unsubscribe<VNCutSceneEvent>(OnCutScene);
        }
    }
}
