using UnityEngine;

[RequireComponent(typeof(Collider))]
public class KillPlayerOnHit : MonoBehaviour
{
    [SerializeField] private Animator _hitEffectAnimator;
    [SerializeField] private AnimationClip _hitEffectClip;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        
        Debug.Log($"Trigger with player detected: {other.name}");

        if (other.TryGetComponent<PlayerHpController>(out var playerHpController))
        {
            playerHpController.KillPlayer();
            //Debug.Log($"Player killed: {other.name}");
            PlayHitEffect();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!collision.collider.CompareTag("Player")) return;

        Debug.Log($"Collision with player detected: {collision.collider.name}");

        if (collision.collider.TryGetComponent<PlayerHpController>(out var playerHpController))
        {
            playerHpController.KillPlayer();
            //Debug.Log($"Player killed: {collision.collider.name}");
            PlayHitEffect();
        }
    }

    private void PlayHitEffect()
    {
        if (_hitEffectAnimator && _hitEffectClip)
        {
            _hitEffectAnimator.speed = 1f / _hitEffectClip.length;
            _hitEffectAnimator.CrossFade(_hitEffectClip.name, 0f);
        }
    }
}
