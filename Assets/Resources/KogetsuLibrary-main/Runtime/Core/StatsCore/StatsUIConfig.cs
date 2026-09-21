using System;
using UnityEngine.UI;

namespace Kogetsu.Library.Core
{
    public enum StatsUIMode { Bar, Block }

    /// <summary>
    /// <para>(TH) : คอนฟิก UI สำหรับ HP / Stamina — ใช้งานกับ StatsController</para>
    /// <para>(EN) : UI display config for a stat (HP or Stamina) attached to StatsController.</para>
    /// </summary>
    [Serializable]
    public class StatsUIConfig
    {
        [Tooltip("เปิด/ปิดการแสดงผล UI สำหรับ stat นี้")]
        public bool UseUI;

        [Tooltip("Bar = fillAmount lerp  |  Block = individual image blocks")]
        public StatsUIMode Mode;

        // ─── Bar Mode ────────────────────────────────────────────────────
        [Tooltip("[Bar] อัปเดตทันทีตาม current value")]
        public Image FrontBarImage;

        [Tooltip("[Bar] ค่อยๆ lerp ตาม FrontBar — ให้ภาพ 'trailing damage'")]
        public Image BackBarImage;

        [Tooltip("[Bar] ความเร็ว lerp ของ BackBar")]
        public float BarLerpSpeed = 5f;

        // ─── Block Mode ──────────────────────────────────────────────────
        [Tooltip("[Block] Image หนึ่งใบ = 1 หน่วย stat  (index 0 = block แรก)")]
        public List<Image> Blocks;

        [Tooltip("[Block] interval กระพริบเมื่อ stat > 50 %")]
        public float BlinkSlowInterval = 0.5f;

        [Tooltip("[Block] interval กระพริบเมื่อ stat ≤ 50 %")]
        public float BlinkFastInterval = 0.15f;

        [Range(0f, 1f)]
        [Tooltip("[Block] 0–1 : ความแรงในการ tint ไปทาง white (>50 %) หรือ black (≤50 %)")]
        public float TintStrength = 0.3f;
    }
}
