using UnityEngine;

public sealed class LibrarianAudioManager : BaseAudioManager
{
    [SerializeField] private AudioSource _chaseSource;

    private void Awake()
    {
        if (!_chaseSource)
            _chaseSource = GetComponentInChildren<AudioSource>();
    }

    public void PlayChase() => PlayLoop(_chaseSource);
    public void StopChase() => Stop(_chaseSource);
}
