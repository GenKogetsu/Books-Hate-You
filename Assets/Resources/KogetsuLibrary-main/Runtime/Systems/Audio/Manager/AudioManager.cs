using UnityEngine;
using Kogetsu.Library.DesignPatternCore;

namespace Kogetsu.Library.Core
{
    public class AudioManager : Singleton<AudioManager>
    {
        [Header("Obsever Channels")]
        [Required]
        [SerializeField] private AudioObserverSO _audioObserver;

        [Required]
        [SerializeField] private BasicMovementInputObserverSO _basicObserverChannel;

        [Header("Audio Source")]
        [Required]
        [SerializeField] private AudioSource _sfx;

        [Required]
        [SerializeField] private AudioSource _bmg;

        [Required]
        [SerializeField] private AudioSource _voiceover;

        [Required]
        [SerializeField] private AudioSource _typing;

        [Required]
        [SerializeField] private AudioSource _click;

        [Header("Global Click SFX")]
        [SerializeField] private AudioClip _clickSfx;

        private void OnEnable()
        {
            _audioObserver.OnSfxChannel                  += OnSfxSignal;
            _audioObserver.OnBmgChannel                  += OnBmgSignal;
            _audioObserver.OnVoiceoverChannel            += OnVoiceoverSignal;
            _audioObserver.OnTypingSfxChannel            += OnTypingSfxSignal;
            _basicObserverChannel.OnInteractionChannel   += OnInteraction;
        }

        private void OnDisable()
        {
            _audioObserver.OnSfxChannel                  -= OnSfxSignal;
            _audioObserver.OnBmgChannel                  -= OnBmgSignal;
            _audioObserver.OnVoiceoverChannel            -= OnVoiceoverSignal;
            _audioObserver.OnTypingSfxChannel            -= OnTypingSfxSignal;
            _basicObserverChannel.OnInteractionChannel   -= OnInteraction;
        }

        public void OnSfxSignal(AudioClip sound)
        {
            PlaySound(_sfx, sound, true);
        }

        public void OnBmgSignal(AudioClip sound)
        {
            PlaySound(_bmg, sound, false);
        }

        public void OnVoiceoverSignal(AudioClip sound)
        {
            PlaySound(_voiceover, sound, true);
        }

        public void OnTypingSfxSignal(AudioClip sound)
        {
            if (sound != null && _typing.isPlaying) return;
            PlaySound(_typing, sound, false);
        }

        #region Documentation - XML
        /// <summary>
        /// <para> Summary : </para>
        /// <para> (TH) : คำสั่งเสียงกลาง: clip เป็น null จะ Stop และล้าง clip; oneShot เลือก PlayOneShot หรือกำหนด clip แล้ว Play; ต้องมี AudioSource ที่กำหนดไว้ </para>
        /// <para> (EN) : Shared playback operation: null stops and clears the clip; oneShot selects PlayOneShot or assign-and-Play; an assigned AudioSource is required. </para>
        /// <para> Refactor : </para>
        /// <para> (TH) : ใช้ร่วมกันโดยช่อง SFX, BMG, Voiceover และ Typing; เงื่อนไขไม่ให้ typing ซ้อนยังตรวจที่ OnTypingSfxSignal </para>
        /// <para> (EN) : Shared by SFX, BMG, voiceover and typing; OnTypingSfxSignal still prevents overlapping typing playback. </para>
        /// </summary>
        #endregion
        private static void PlaySound(AudioSource source, AudioClip clip, bool oneShot)
        {
            if (clip == null) { source.Stop(); source.clip = null; return; }
            if (oneShot) source.PlayOneShot(clip);
            else { source.clip = clip; source.Play(); }
        }

        private void OnInteraction()
        {
            if (_clickSfx != null) _click.PlayOneShot(_clickSfx);
        }
    }
}
