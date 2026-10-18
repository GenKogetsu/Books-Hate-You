using UnityEngine;

[RequireComponent(typeof(Collider))]
public class KillPlayerOnHit : MonoBehaviour
{
    [SerializeField] private ParticleSystem _effectOnHit;

    private void Awake()
    {
        if (_effectOnHit)
        {
            _effectOnHit.gameObject.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        Debug.Log("OnTriggerEnter KillPlayerOnHit");

        var playerHpController = other.GetComponentInParent<PlayerHpController>();

        if (playerHpController)
        {
            playerHpController.KillPlayer();
            PlayHitEffect();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!collision.collider.CompareTag("Player")) return;

        Debug.Log("OnCollisionEnter KillPlayerOnHit");

        var playerHpController = collision.collider.GetComponentInParent<PlayerHpController>();

        if (playerHpController)
        {
            playerHpController.KillPlayer();
            PlayHitEffect();
        }
    }

    private void PlayHitEffect()
    {
        if (_effectOnHit)
        {
            _effectOnHit.gameObject.SetActive(true);
        }
    }
}

