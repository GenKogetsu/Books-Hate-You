using UnityEngine.UI;
using Kogetsu.Library.DesignPatternCore;

namespace Kogetsu.Library.Core
{
    public class StatsController : MonoBehaviour
    {
        // ─── Stats Data ────────────────────────────────────────────────────
        [Header("Stats Data")]
        [Required]
        [SerializeField] protected BaseStatsDataSO StatsData;

        [ReadOnly][SerializeField] protected float CurrentMoveSpeed;
        [ReadOnly][SerializeField] protected float CurrectJumpForce;

        // ─── HP ────────────────────────────────────────────────────────────
        [Header("HP")]
        [ReadOnly][SerializeField] protected float CurrentHp;
        [SerializeField] protected float MinHp;

        // ─── HP UI ─────────────────────────────────────────────────────────
        [SerializeField] private StatsUIConfig _hpUIConfig = new();

        // ─── Stamina ───────────────────────────────────────────────────────
        [Header("Stamina")]
        [ReadOnly][SerializeField] protected float CurrentStamina;
        [SerializeField] protected float MinStamina;

        // ─── Stamina UI ────────────────────────────────────────────────────
        [SerializeField] private StatsUIConfig _staminaUIConfig = new();

        // ─── Color Lerp ────────────────────────────────────────────────────────
        [Header("Color Lerp (Hit Flash)")]
        [Tooltip("Root Transform ของตัวละคร — GetComponentsInChildren<SpriteRenderer> อัตโนมัติ\n" +
                 "รองรับทั้ง Spritesheet (1 renderer) และ Cutout2D Rig (หลาย renderer)")]
        [SerializeField] protected Transform colorLerpRoot;

        [SerializeField] protected Color startColor = Color.white;
        [SerializeField] protected Color endColor = Color.red;
        [SerializeField] protected float lerpSpeed = 10f;
        [SerializeField] protected int lerpCount = 3;

        // ─── Runtime ───────────────────────────────────────────────────────
        private Coroutine _lerpCoroutine;
        private Coroutine _hpBlinkCoroutine;
        private Coroutine _staminaBlinkCoroutine;
        private SpriteRenderer[] _cachedRenderers;

        private readonly List<Color> _hpBlockOrigColors = new();
        private readonly List<Color> _staminaBlockOrigColors = new();

        // ─── Properties ────────────────────────────────────────────────────
        public float MaxHp => StatsData != null ? StatsData.BaseHp : 0f;
        public float MaxStamina => StatsData != null ? StatsData.BaseStamina : 0f;

        // ─── Stats Accessors ───────────────────────────────────────────────
        public float GetBaseMoveSpeed() => StatsData != null ? StatsData.BaseMoveSpeed : 0f;
        public float GetBaseJumpForce() => StatsData != null ? StatsData.BaseJumpForce : 0f;
        public float GetMoveSpeed() => CurrentMoveSpeed;
        public float GetJumpForce() => CurrectJumpForce;
        public float GetMoveSpeedPercentage() =>
            GetBaseMoveSpeed() > 0f ? CurrentMoveSpeed / GetBaseMoveSpeed() : 0f;

        // ─── Lifecycle ─────────────────────────────────────────────────────
        protected virtual void Setup()
        {
            CurrentMoveSpeed = GetBaseMoveSpeed();
            CurrectJumpForce = GetBaseJumpForce();

            CurrentHp = MaxHp;
            CurrentStamina = MaxStamina;

            // Cache all SpriteRenderers under the root transform
            ResetSprite();

            CacheBlockColors(_hpUIConfig, _hpBlockOrigColors);
            CacheBlockColors(_staminaUIConfig, _staminaBlockOrigColors);

            RefreshBarUI(_hpUIConfig, CurrentHp, MaxHp, instant: true);
            RefreshBarUI(_staminaUIConfig, CurrentStamina, MaxStamina, instant: true);
            RefreshBlockUI(_hpUIConfig, CurrentHp, MaxHp, _hpBlockOrigColors, ref _hpBlinkCoroutine);
            RefreshBlockUI(_staminaUIConfig, CurrentStamina, MaxStamina, _staminaBlockOrigColors, ref _staminaBlinkCoroutine);
        }

        public void ResetSprite()
        {
            _cachedRenderers = colorLerpRoot != null
               ? colorLerpRoot.GetComponentsInChildren<SpriteRenderer>()
               : System.Array.Empty<SpriteRenderer>();
        }

        protected virtual void Start() => Setup();

#if UNITY_EDITOR
        /// <summary>
        /// Called by Unity when any serialized field changes in the Inspector (edit + play mode).
        /// In play mode: clamps current values so they stay within updated min/max bounds.
        /// </summary>
        protected virtual void OnValidate()
        {
            if (!Application.isPlaying) return;
            CurrentHp = Mathf.Clamp(CurrentHp, MinHp, MaxHp);
            CurrentStamina = Mathf.Clamp(CurrentStamina, MinStamina, MaxStamina);
        }
#endif

        protected void OnEnable()
        {
            if (!EventBus.Instance) return;
            EventBus.Instance.Subscribe<DealDamageEvent>(OnTakeDamage);
        }

        protected void OnDisable()
        {
            if (!EventBus.Instance) return;
            EventBus.Instance.Unsubscribe<DealDamageEvent>(OnTakeDamage);
        }

        protected virtual void Update()
        {
            // Bar back-layer lerps every frame
            RefreshBarUI(_hpUIConfig, CurrentHp, MaxHp, instant: false);
            RefreshBarUI(_staminaUIConfig, CurrentStamina, MaxStamina, instant: false);
        }

        // ─── HP ────────────────────────────────────────────────────────────
        public void TakeDamage(float damage)
        {
            CurrentHp = Mathf.Clamp(CurrentHp - damage, MinHp, MaxHp);
            TriggerColorLerp();
            RefreshBlockUI(_hpUIConfig, CurrentHp, MaxHp, _hpBlockOrigColors, ref _hpBlinkCoroutine);
        }

        public void Heal(float healAmount)
        {
            CurrentHp = Mathf.Clamp(CurrentHp + healAmount, MinHp, MaxHp);
            RefreshBlockUI(_hpUIConfig, CurrentHp, MaxHp, _hpBlockOrigColors, ref _hpBlinkCoroutine);
        }

        public void SetCurrentHp(float hp)
        {
            CurrentHp = Mathf.Clamp(hp, MinHp, MaxHp);
            RefreshBlockUI(_hpUIConfig, CurrentHp, MaxHp, _hpBlockOrigColors, ref _hpBlinkCoroutine);
        }

        public void ResetHp()
        {
            CurrentHp = MaxHp;
            RefreshBlockUI(_hpUIConfig, CurrentHp, MaxHp, _hpBlockOrigColors, ref _hpBlinkCoroutine);
        }

        public float GetCurrentHp() => CurrentHp;
        public float GetMaxHp() => MaxHp;
        public float GetMinHp() => MinHp;

        // ─── Stamina ───────────────────────────────────────────────────────
        public void UseStamina(float amount)
        {
            CurrentStamina = Mathf.Clamp(CurrentStamina - amount, MinStamina, MaxStamina);
            RefreshBlockUI(_staminaUIConfig, CurrentStamina, MaxStamina, _staminaBlockOrigColors, ref _staminaBlinkCoroutine);
        }

        public void RestoreStamina(float amount)
        {
            CurrentStamina = Mathf.Clamp(CurrentStamina + amount, MinStamina, MaxStamina);
            RefreshBlockUI(_staminaUIConfig, CurrentStamina, MaxStamina, _staminaBlockOrigColors, ref _staminaBlinkCoroutine);
        }

        public void ResetStamina()
        {
            CurrentStamina = MaxStamina;
            RefreshBlockUI(_staminaUIConfig, CurrentStamina, MaxStamina, _staminaBlockOrigColors, ref _staminaBlinkCoroutine);
        }

        public float GetCurrentStamina() => CurrentStamina;
        public float GetMaxStamina() => MaxStamina;

        // ─── EventBus Handler ──────────────────────────────────────────────
        protected void OnTakeDamage(DealDamageEvent data)
        {
            TakeDamage(data.Damage);
            if (EventBus.Instance)
                EventBus.Instance.Publish(new TakeDamageEvent(data.Damage, CurrentHp));
        }

        // ─── Color Lerp ────────────────────────────────────────────────────
        protected void TriggerColorLerp()
        {
            if (_cachedRenderers == null || _cachedRenderers.Length == 0) return;

            if (_lerpCoroutine != null) StopCoroutine(_lerpCoroutine);
            _lerpCoroutine = StartCoroutine(LerpColorRoutine());
        }

        private IEnumerator LerpColorRoutine()
        {
            for (int i = 0; i < lerpCount; i++)
            {
                for (float t = 0f; t < 1f; t += Time.deltaTime * lerpSpeed)
                {
                    SetAllColors(Color.Lerp(startColor, endColor, Mathf.Clamp01(t)));
                    yield return null;
                }
                for (float t = 0f; t < 1f; t += Time.deltaTime * lerpSpeed)
                {
                    SetAllColors(Color.Lerp(endColor, startColor, Mathf.Clamp01(t)));
                    yield return null;
                }
            }
            SetAllColors(startColor);
        }

        private void SetAllColors(Color color)
        {
            foreach (var sr in _cachedRenderers)
                if (sr != null) sr.color = color;
        }

        // ─── UI — Bar ──────────────────────────────────────────────────────
        private static void RefreshBarUI(StatsUIConfig cfg, float current, float max, bool instant)
        {
            if (cfg == null || !cfg.UseUI || cfg.Mode != StatsUIMode.Bar) return;
            float fill = max > 0f ? Mathf.Clamp01(current / max) : 0f;

            if (cfg.FrontBarImage != null)
                cfg.FrontBarImage.fillAmount = fill;

            if (cfg.BackBarImage != null)
                cfg.BackBarImage.fillAmount = instant
                    ? fill
                    : Mathf.Lerp(cfg.BackBarImage.fillAmount, fill, Time.deltaTime * cfg.BarLerpSpeed);
        }

        // ─── UI — Block ────────────────────────────────────────────────────
        private static void CacheBlockColors(StatsUIConfig cfg, List<Color> cache)
        {
            cache.Clear();
            if (cfg?.Blocks == null) return;
            foreach (var b in cfg.Blocks)
                cache.Add(b != null ? b.color : Color.white);
        }

        private void RefreshBlockUI(StatsUIConfig cfg, float current, float max,
                                     List<Color> origColors, ref Coroutine blinkRoutine)
        {
            if (cfg == null || !cfg.UseUI || cfg.Mode != StatsUIMode.Block || cfg.Blocks == null) return;

            if (blinkRoutine != null) { StopCoroutine(blinkRoutine); blinkRoutine = null; }

            float ratio = max > 0f ? current / max : 0f;
            bool critical = ratio <= 0.5f;
            int full = Mathf.FloorToInt(current);

            for (int i = 0; i < cfg.Blocks.Count; i++)
            {
                if (cfg.Blocks[i] == null) continue;
                Color baseCol = i < origColors.Count ? origColors[i] : Color.white;
                cfg.Blocks[i].color = i < full
                    ? Color.Lerp(baseCol, critical ? Color.black : Color.white, cfg.TintStrength)
                    : Color.clear;
            }

            // Blink the last active block
            int lastIdx = Mathf.CeilToInt(current) - 1;
            if (lastIdx < 0 || lastIdx >= cfg.Blocks.Count || cfg.Blocks[lastIdx] == null) return;

            Color baseColor = lastIdx < origColors.Count ? origColors[lastIdx] : Color.white;
            Color blinkColor = Color.Lerp(baseColor, critical ? Color.black : Color.white, cfg.TintStrength);
            float interval = critical ? cfg.BlinkFastInterval : cfg.BlinkSlowInterval;

            blinkRoutine = StartCoroutine(BlockBlinkRoutine(cfg.Blocks[lastIdx], blinkColor, interval));
        }

        private static IEnumerator BlockBlinkRoutine(Image block, Color onColor, float interval)
        {
            while (true)
            {
                block.color = onColor;
                yield return new WaitForSeconds(interval);
                block.color = Color.clear;
                yield return new WaitForSeconds(interval);
            }
        }
    }
}
