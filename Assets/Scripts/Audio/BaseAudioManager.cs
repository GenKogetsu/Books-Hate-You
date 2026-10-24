using UnityEngine;

public abstract class BaseAudioManager : MonoBehaviour
{
    protected static void PlayLoop(AudioSource source)
    {
        if (!source || !source.clip)
            return;

        source.loop = true;
        if (!source.isPlaying)
            source.Play();
    }

    protected static void Stop(AudioSource source)
    {
        if (source)
            source.Stop();
    }

    protected static void PlayOneShot(AudioSource source)
    {
        if (source && source.clip)
            source.PlayOneShot(source.clip);
    }
}
