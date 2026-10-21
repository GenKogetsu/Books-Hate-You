
using System.Collections;
using Kogetsu.Library.DesignPatternCore;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class KillPlayerOnHit : MonoBehaviour
{
    #region Serialized Fields

    [Header("Hit Effect")]
    [SerializeField] private ParticleSystem _effectOnHit;

    [Header("Model")]
    [SerializeField] private GameObject _model;

    [Header("Object Pool")]
    [Tooltip("เวลาก่อนส่ง Object กลับ Pool หลังชน หน่วยเป็นวินาที")]
    [SerializeField, Min(0f)] private float _returnToPoolDelay = 2f;

    [Header("Debug")]
    [SerializeField] private bool _debugLog = true;

    #endregion

    #region Private Fields

    private bool _hasHitPlayer;
    private Coroutine _returnCoroutine;

    #endregion

    #region Unity Callbacks

    private void Awake()
    {
        if (!_model)
            Debug.LogWarning(
                "[KillPlayerOnHit] ยังไม่ได้กำหนด _model",
                this);

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

        PlayerHpController playerHpController =
            playerRoot.GetComponent<PlayerHpController>();

        if (!playerHpController)
        {
            playerHpController =
                playerRoot.GetComponentInParent<PlayerHpController>();
        }

        if (!playerHpController)
        {
            playerHpController =
                other.GetComponentInParent<PlayerHpController>();
        }

        if (!playerHpController)
        {
            Debug.LogWarning(
                "[KillPlayerOnHit] พบ Player แต่ไม่พบ PlayerHpController",
                other);

            return;
        }

        // ป้องกันการเรียก KillPlayer ซ้ำจาก Collider หลายชิ้น
        _hasHitPlayer = true;

        if (_debugLog)
        {
            Debug.Log(
                $"[KillPlayerOnHit] {name} ชน {playerRoot.name}",
                this);
        }

        // ซ่อนโมเดลทันทีเมื่อชนผู้เล่น
        if (_model)
            _model.SetActive(false);

        // เรียกระบบตายของผู้เล่น
        playerHpController.KillPlayer();

        // เล่นเอฟเฟกต์ระเบิด
        PlayHitEffect();

        // รอแล้วส่งกลับ Pool
        _returnCoroutine = StartCoroutine(ReturnToPoolAfterDelay());
    }

    private Transform FindPlayerRoot(Transform candidate)
    {
        Transform current = candidate;

        while (current != null)
        {
            if (current.CompareTag("Player"))
                return current;

            current = current.parent;
        }

        return null;
    }

    #endregion

    #region Hit Effect

    private void PlayHitEffect()
    {
        if (!_effectOnHit)
            return;

        _effectOnHit.gameObject.SetActive(true);
        _effectOnHit.Play(true);
    }

    #endregion

    #region Object Pool

    private IEnumerator ReturnToPoolAfterDelay()
    {
        if (_returnToPoolDelay > 0f)
            yield return new WaitForSeconds(_returnToPoolDelay);

        string key = this.gameObject.name;

        if (ObjectPoolManager.Instance)
        {
            if (_debugLog)
            {
                Debug.Log(
                    $"[KillPlayerOnHit] คืน {name} เข้า Pool: {key}",
                    this);
            }

            ObjectPoolManager.Instance.Release(key, this);
        }
        else
        {
            Debug.LogError(
                "[KillPlayerOnHit] ไม่พบ ObjectPoolManager " +
                "จึงไม่สามารถคืน Object เข้า Pool ได้",
                this);
        }

        _returnCoroutine = null;
    }

    #endregion
}
