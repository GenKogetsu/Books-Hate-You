using UnityEngine;

public sealed class CutsceneSignalAudioPlayer : MonoBehaviour
{
    [SerializeField] private AudioSource _bgmSource;
    [SerializeField] private AudioSource _sfxSource;

    public void SFXPlay()
    {
        if (_sfxSource)
            _sfxSource.Play();
    }

    public void BGMPlay()
    {
        if (_bgmSource)
            _bgmSource.Play();
    }
}
