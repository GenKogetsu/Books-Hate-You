
using System.Collections;
using Kogetsu.Library.DesignPatternCore;
using Kogetsu.Library.Debugging;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class KillOnHit : MonoBehaviour
{
    #region Serialized Fields

    [Header("Hit Effect")]
    [SerializeField] private ParticleSystem _effectOnHit;
    [SerializeField] private AudioSource _audioSourceOnHit;

    [Header("Model")]
    [SerializeField] private GameObject _model;

    [Header("Object Pool")]
    [SerializeField] private bool _returnToPoolOnHit = false;
    [SerializeField, Min(0f)] private float _returnToPoolDelay = 2f;

    [Header("Layer Detection")]
    [SerializeField] private LayerMask _livingMask;
    [SerializeField] private TargetTag _targetTags = TargetTag.Player;

    [Header("Debug")]
    [SerializeField] private bool _debugLog = false;

    #endregion

    #region Private Fields

    private bool _hasHitPlayer;
    private Coroutine _returnCoroutine;

    #endregion

    #region Unity Callbacks

    private void Awake()
    {
        if (_livingMask.value == 0)
            _livingMask = LayerMask.GetMask("Living");

        if (_effectOnHit)
        {
            _effectOnHit.Stop(
                true,
                ParticleSystemStopBehavior.StopEmittingAndClear);

            _effectOnHit.gameObject.SetActive(false);
        }
    }

    private void OnEnable()
    {
        // รีเซ็ตสถานะเมื่อ Object ถูกดึงออกจาก Pool
        _hasHitPlayer = false;
        _returnCoroutine = null;

        if (_model)
            _model.SetActive(true);

        if (_effectOnHit)
        {
            _effectOnHit.Stop(
                true,
                ParticleSystemStopBehavior.StopEmittingAndClear);

            _effectOnHit.gameObject.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        TryKillPlayer(other);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision != null)
            TryKillPlayer(collision.collider);
    }

    #endregion

    #region Hit Detection

    private void TryKillPlayer(Collider other)
    {
        if (_hasHitPlayer || !other)
            return;

        Transform playerRoot = FindPlayerRoot(other.transform);

        if (!playerRoot)
            return;

        LivingHpController livingHpController =
            playerRoot.GetComponent<LivingHpController>();

        if (!livingHpController)
        {
            livingHpController =
                playerRoot.GetComponentInParent<LivingHpController>();
        }

        if (!livingHpController)
        {
            livingHpController =
                other.GetComponentInParent<LivingHpController>();
        }

        if (!livingHpController)
        {
            Debug.LogWarning(
                "[KillOnHit] พบ Living แต่ไม่พบ LivingHpController",
                other);

            return;
        }

        _hasHitPlayer = true;

        if (_debugLog)
        {
            DebugLog.Log(DebugCategory.Combat,
                $"{name} ชน {playerRoot.name}", this, _debugLog);
        }

        if (_model)
            _model.SetActive(false);

        livingHpController.Kill();

        PlayHitEffect();

        if (_returnToPoolOnHit)
        {
            _returnCoroutine = StartCoroutine(ReturnToPoolAfterDelay());
        }
    }

    private Transform FindPlayerRoot(Transform candidate)
    {
        if (_targetTags == TargetTag.None)
            return null;

        Transform current = candidate;

        while (current != null)
        {
            if (_targetTags.MatchesUnityTag(current.tag))
                return current;

            current = current.parent;
        }

        return null;
    }

    private bool IsLivingLayer(Transform candidate)
    {
        while (candidate)
        {
            if ((_livingMask.value & (1 << candidate.gameObject.layer)) != 0)
                return true;

            candidate = candidate.parent;
        }

        return false;
    }

    #endregion

    #region Hit Effect

    private void PlayHitEffect()
    {
        if (!_effectOnHit)
            return;

        _effectOnHit.gameObject.SetActive(true);
        _effectOnHit.Play(true);

        if (_audioSourceOnHit)
            _audioSourceOnHit.Play();
    }

    #endregion

    #region Object Pool

    private IEnumerator ReturnToPoolAfterDelay()
    {
        if (_returnToPoolDelay > 0f)
            yield return new WaitForSeconds(_returnToPoolDelay);

        ObjectPoolManager poolManager = ObjectPoolManager.Instance;
        if (poolManager && poolManager.TryRelease(this))
        {
            if (_debugLog)
            {
                DebugLog.Log(DebugCategory.ObjectPool,
                    $"คืน {name} เข้า Pool", this, _debugLog);
            }
        }
        else
        {
            if (_debugLog)
            {
                DebugLog.Log(DebugCategory.ObjectPool,
                    $"{name} ไม่ได้มาจาก Pool จึงทำลาย Object", this, _debugLog);
            }

            Destroy(gameObject);
        }

        _returnCoroutine = null;
    }

    #endregion
}
