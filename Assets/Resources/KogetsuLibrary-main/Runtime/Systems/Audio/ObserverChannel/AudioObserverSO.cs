using System;

namespace Kogetsu.Library.DesignPatternCore
{
    [CreateAssetMenu(fileName = "AudioObserver", menuName = "KogetsuLibrary/DesignPattern/Observer/AudioObserver")]
    public class AudioObserverSO : ScriptableObject
    {
        public Action<AudioClip> OnSfxChannel;
        public Action<AudioClip> OnBmgChannel;
        public Action<AudioClip> OnVoiceoverChannel;
        public Action<AudioClip> OnTypingSfxChannel;
        public Action<AudioClip> OnClickSfxChannel;

        public void SendSfxSignal(AudioClip sound)       => OnSfxChannel?.Invoke(sound);
        public void SendBmgSignal(AudioClip sound)       => OnBmgChannel?.Invoke(sound);
        public void SendVoiceoverSignal(AudioClip sound) => OnVoiceoverChannel?.Invoke(sound);
        public void SendTypingSfxSignal(AudioClip sound) => OnTypingSfxChannel?.Invoke(sound);
        public void SendClickSfxSignal(AudioClip sound)  => OnClickSfxChannel?.Invoke(sound);
    }
}
