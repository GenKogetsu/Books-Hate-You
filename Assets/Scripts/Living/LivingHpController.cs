using UnityEngine;

public abstract class LivingHpController : MonoBehaviour
{
    [Header("Death Audio")]
    [SerializeField] private AudioSource _deathAudioSource;
    [SerializeField] private AudioClip _deathAudioClip;

    public abstract bool IsDead { get; }
    public abstract void TakeDamage(float damage);
    public abstract void Kill();

    public virtual void Revive() { }

    protected void PlayDeathSound()
    {
        if (!_deathAudioSource)
            _deathAudioSource = GetComponent<AudioSource>();

        if (_deathAudioSource && _deathAudioClip)
            _deathAudioSource.PlayOneShot(_deathAudioClip);
    }
}
