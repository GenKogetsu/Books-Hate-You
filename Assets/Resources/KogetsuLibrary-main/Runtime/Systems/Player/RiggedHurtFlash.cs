using System.Collections;
using Kogetsu.Library.DesignPatternCore;

namespace Kogetsu.Library.Core;

#region Documentation - XML
/// <summary>
/// <para> Summary : </para>
/// <para> (TH) : ย้อมสี SpriteRenderer ทุกตัวใน rig เมื่อโดนโจมตี / ตาย รองรับ 2D Cutout rig ที่มี SpriteRenderer หลาย child: - Awake จะ GetComponentsInChildren&lt;SpriteRenderer&gt;() อัตโนมัติ - หรือลาก SpriteRenderer ที่ต้องการเข้า _renderers ด้วยตัวเอง Setup: 1. ใส่ Component นี้บน Player root (ตัวเดียวกับที่ Rig อยู่) 2. ตั้ง Max Hp ใน StatsController ให้ถูกต้อง 3. ใน StatsController ให้ target Renderer = null (ปล่อยว่าง) เพื่อให้ RiggedHurtFlash จัดการเอง </para>
/// <para> (EN) : Tints SpriteRenderers in a cutout rig for hurt/death effects; renderers can be discovered in children or assigned explicitly. </para>
/// </summary>
#endregion
public class RiggedHurtFlash : MonoBehaviour
{
    [Header("Renderers")]
    [Tooltip("ถ้าเว้นว่าง จะ auto-collect จาก children ทั้งหมด")]
    [SerializeField] private SpriteRenderer[] _renderers;

    [Header("Hurt Flash")]
    [SerializeField] private Color _hurtColor      = new(1f, 0.15f, 0.15f, 1f);
    [SerializeField] private float _flashDuration  = 0.07f;
    [SerializeField] private int   _flashCount     = 3;
    [SerializeField] private float _flashInterval  = 0.05f;

    [Header("Death Effect")]
    [SerializeField] private Color _deathColor     = new(1f, 1f, 1f, 1f);
    [Tooltip("fade ไปที่ deathColor แล้ว fade กลับ — ทำหลายรอบ จากนั้น fade ออก")]
    [SerializeField] private float _deathFadeSpeed = 6f;
    [SerializeField] private int   _deathFlashCount = 5;

    private Color[]   _originalColors;
    private Coroutine _current;

    #region Documentation - Notes
    // ─── Lifecycle ────────────────────────────────────────────────────
    #endregion
    private void Awake()
    {
        if (_renderers == null || _renderers.Length == 0)
            _renderers = GetComponentsInChildren<SpriteRenderer>(true);

        CacheColors();
    }

    private void OnEnable()
    {
        if (EventBus.Instance)
        {
            EventBus.Instance.Subscribe<TakeDamageEvent>(OnTakeDamage);
            EventBus.Instance.Subscribe<PlayerDeathEvent>(OnDeath);
        }
    }

    private void OnDisable()
    {
        if (EventBus.Instance)
        {
            EventBus.Instance.Unsubscribe<TakeDamageEvent>(OnTakeDamage);
            EventBus.Instance.Unsubscribe<PlayerDeathEvent>(OnDeath);
        }
    }

    #region Documentation - Notes
    // ─── Public API ───────────────────────────────────────────────────
    #endregion
    #region Documentation - XML
    /// <summary>
    /// <para> Summary : </para>
    /// <para> (TH) : เรียก hurt flash จากภายนอก (เช่น debug) </para>
    /// <para> (EN) : Triggers a hurt flash from an external caller, for example a debug action. </para>
    /// </summary>
    #endregion
    public void TriggerHurt() => Run(HurtRoutine());

    #region Documentation - XML
    /// <summary>
    /// <para> Summary : </para>
    /// <para> (TH) : เรียก death effect จากภายนอก </para>
    /// <para> (EN) : Triggers the death effect from an external caller. </para>
    /// </summary>
    #endregion
    public void TriggerDeath() => Run(DeathRoutine());

    #region Documentation - Notes
    // ─── Event handlers ───────────────────────────────────────────────
    #endregion
    private void OnTakeDamage(TakeDamageEvent _) => TriggerHurt();
    private void OnDeath(PlayerDeathEvent _)      => TriggerDeath();

    #region Documentation - Notes
    // ─── Coroutines ───────────────────────────────────────────────────
    #endregion
    private IEnumerator HurtRoutine()
    {
        for (int i = 0; i < _flashCount; i++)
        {
            SetAllColor(_hurtColor);
            yield return new WaitForSeconds(_flashDuration);
            RestoreAllColors();
            if (i < _flashCount - 1)
                yield return new WaitForSeconds(_flashInterval);
        }
    }

    private IEnumerator DeathRoutine()
    {
        #region Documentation - Notes
        // หลายรอบ flash ขาว → ปกติ
        #endregion
        for (int i = 0; i < _deathFlashCount; i++)
        {
            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime * _deathFadeSpeed;
                LerpAllColors(_originalColors, _deathColor, Mathf.Clamp01(t));
                yield return null;
            }
            t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime * _deathFadeSpeed;
                LerpAllColors(_deathColor, _originalColors, Mathf.Clamp01(t));
                yield return null;
            }
        }

        #region Documentation - Notes
        // Fade ออกหายไป (alpha → 0)
        #endregion
        Color[] transparent = new Color[_renderers.Length];
        for (int i = 0; i < _renderers.Length; i++)
        {
            transparent[i] = _originalColors[i];
            transparent[i].a = 0f;
        }

        float f = 0f;
        while (f < 1f)
        {
            f += Time.deltaTime * (_deathFadeSpeed * 0.5f);
            LerpAllColors(_originalColors, transparent, Mathf.Clamp01(f));
            yield return null;
        }

        gameObject.SetActive(false);
    }

    #region Documentation - Notes
    // ─── Helpers ──────────────────────────────────────────────────────
    #endregion
    private void Run(IEnumerator routine)
    {
        if (_current != null) StopCoroutine(_current);
        _current = StartCoroutine(routine);
    }

    private void CacheColors()
    {
        _originalColors = new Color[_renderers.Length];
        for (int i = 0; i < _renderers.Length; i++)
            _originalColors[i] = _renderers[i] != null ? _renderers[i].color : Color.white;
    }

    private void SetAllColor(Color c)
    {
        foreach (var sr in _renderers)
            if (sr != null) sr.color = c;
    }

    private void RestoreAllColors()
    {
        for (int i = 0; i < _renderers.Length; i++)
            if (_renderers[i] != null) _renderers[i].color = _originalColors[i];
    }

    private void LerpAllColors(Color from, Color to, float t)
    {
        foreach (var sr in _renderers)
            if (sr != null) sr.color = Color.Lerp(from, to, t);
    }

    private void LerpAllColors(Color[] from, Color to, float t)
    {
        for (int i = 0; i < _renderers.Length; i++)
            if (_renderers[i] != null)
                _renderers[i].color = Color.Lerp(from[i], to, t);
    }

    private void LerpAllColors(Color from, Color[] to, float t)
    {
        for (int i = 0; i < _renderers.Length; i++)
            if (_renderers[i] != null)
                _renderers[i].color = Color.Lerp(from, to[i], t);
    }

    private void LerpAllColors(Color[] from, Color[] to, float t)
    {
        for (int i = 0; i < _renderers.Length; i++)
            if (_renderers[i] != null)
                _renderers[i].color = Color.Lerp(from[i], to[i], t);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (_renderers == null || _renderers.Length == 0)
            _renderers = GetComponentsInChildren<SpriteRenderer>(true);
    }
#endif
}
