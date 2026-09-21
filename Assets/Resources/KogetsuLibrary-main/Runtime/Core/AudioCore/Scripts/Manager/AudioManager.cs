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
            if (sound == null) { _sfx.Stop(); _sfx.clip = null; return; }
            _sfx.PlayOneShot(sound);
        }

        public void OnBmgSignal(AudioClip sound)
        {
            if (sound == null) { _bmg.Stop(); _bmg.clip = null; return; }
            _bmg.clip = sound;
            _bmg.Play();
        }

        public void OnVoiceoverSignal(AudioClip sound)
        {
            if (sound == null) { _voiceover.Stop(); _voiceover.clip = null; return; }
            _voiceover.PlayOneShot(sound);
        }

        public void OnTypingSfxSignal(AudioClip sound)
        {
            if (sound == null) { _typing.Stop(); _typing.clip = null; return; }
            if (_typing.isPlaying) return;
            _typing.clip = sound;
            _typing.Play();
        }

        private void OnInteraction()
        {
            if (_clickSfx != null) _click.PlayOneShot(_clickSfx);
        }
    }
}
