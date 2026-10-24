using UnityEngine;

public sealed class PlayerAudioManager : BaseAudioManager
{
    [SerializeField] private AudioSource _walkSource;
    [SerializeField] private AudioSource _runSource;
    [SerializeField] private AudioSource _takeoffSource;
    [SerializeField] private AudioSource _landingSource;

    private bool _wasJumping;
    private bool _wasLanding;

    public void PlayWalk()
    {
        Stop(_runSource);
        PlayLoop(_walkSource);
    }

    public void PlayRun()
    {
        Stop(_walkSource);
        PlayLoop(_runSource);
    }

    public void StopMovement()
    {
        Stop(_walkSource);
        Stop(_runSource);
    }

    public void PlayTakeoff()
    {
        PlayOneShot(_takeoffSource);
    }

    public void PlayLanding()
    {
        PlayOneShot(_landingSource);
    }

    /// <summary>Called by player movement states; this manager only reads runtime state from PlayerStatus.</summary>
    public void Sync(PlayerStatus status)
    {
        if (!status)
        {
            StopAll();
            return;
        }

        bool isJumping = status.GetIsJumping();
        bool isLanding = status.GetIsLanding();

        if (isJumping && !_wasJumping)
            PlayTakeoff();

        if (isLanding && !_wasLanding)
            PlayLanding();

        _wasJumping = isJumping;
        _wasLanding = isLanding;

        if (!status.GetIsAlive() || !status.GetIsMoving() || isJumping ||
            isLanding || status.GetIsPreparingJump())
        {
            StopMovement();
            return;
        }

        if (status.GetIsRunning())
            PlayRun();
        else
            PlayWalk();
    }

    public void StopAll()
    {
        StopMovement();
        Stop(_takeoffSource);
        Stop(_landingSource);
        _wasJumping = false;
        _wasLanding = false;
    }
}
