using System;
using Kogetsu.Library.DesignPatternCore;
using UnityEngine;

namespace Kogetsu.Library.Core;

[Serializable]
public abstract class BaseMoveAbility2D : BaseMoveAbility<IMoveContext2D>
{
    #region Documentation - Notes
    // ─── Context Shortcuts ────────────────────────────────────────────────
    #endregion
    #region Documentation - XML
    /// <summary>
    /// <para> Summary : </para>
    /// <para> (TH) : เป็น true เมื่อ Context และ Stats ไม่เป็น null จึงอ่านข้อมูลสถิติผ่าน context ได้ </para>
    /// <para> (EN) : True when both Context and Stats are non-null and safe to use. </para>
    /// </summary>
    #endregion
    protected bool HasStats => Context?.Stats != null;

    #region Documentation - XML
    /// <summary>
    /// <para> Summary : </para>
    /// <para> (TH) : ความเร็วเคลื่อนที่ปัจจุบันที่อ่านจาก StatsController ผ่าน context </para>
    /// <para> (EN) : Current move speed from StatsController. </para>
    /// </summary>
    #endregion
    protected float MoveSpeed => Context.Stats.GetMoveSpeed();

    #region Documentation - XML
    /// <summary>
    /// <para> Summary : </para>
    /// <para> (TH) : แรงกระโดดที่อ่านจาก StatsController ผ่าน context </para>
    /// <para> (EN) : Jump force from StatsController. </para>
    /// </summary>
    #endregion
    protected float JumpForce => Context.Stats.GetJumpForce();

    #region Documentation - Notes
    // ─── Direction ────────────────────────────────────────────────────────
    #endregion
    #region Documentation - XML
    /// <summary>
    /// <para> Summary : </para>
    /// <para> (TH) : ตั้ง CurrentDirection และจำทิศล่าสุดที่ไม่เป็นศูนย์ใน LastFacingDirection เพื่อให้การหันภาพยังอ้างอิงทิศเดิมเมื่อหยุด </para>
    /// <para> (EN) : Sets <see cref="IMoveContext.CurrentDirection"/> to <paramref name="direction"/>. Also updates <see cref="IMoveContext.LastFacingDirection"/> when direction is non-zero, so the flip system always knows the last facing axis. </para>
    /// </summary>
    #endregion
    protected void ApplyDirection(Vector3 direction)
    {
        Context.CurrentDirection = direction;
        if (direction != Vector3.zero)
            Context.LastFacingDirection = direction;
    }

    #region Documentation - Notes
    // ─── Velocity ─────────────────────────────────────────────────────────
    #endregion
    #region Documentation - XML
    /// <summary>
    /// <para> Summary : </para>
    /// <para> (TH) : กำหนดความเร็ว Rigidbody2D ทั้งแกน X และ Y </para>
    /// <para> (EN) : Set full 2D velocity (X and Y). </para>
    /// </summary>
    #endregion
    protected void SetVelocity(Vector2 velocity) =>
        Context.Rb2.linearVelocity = velocity;

    #region Documentation - XML
    /// <summary>
    /// <para> Summary : </para>
    /// <para> (TH) : กำหนดเฉพาะความเร็วแกน X โดยเก็บค่า Y เดิมไว้เพื่อคงแรงโน้มถ่วงและการกระโดด </para>
    /// <para> (EN) : Set X-axis velocity only — preserves Y so gravity and jump are unaffected. </para>
    /// </summary>
    #endregion
    protected void SetVelocityX(float x) =>
        Context.Rb2.linearVelocity = new Vector2(x, Context.Rb2.linearVelocity.y);

    #region Documentation - XML
    /// <summary>
    /// <para> Summary : </para>
    /// <para> (TH) : ตั้งความเร็ว Rigidbody2D เป็นศูนย์ทุกแกน </para>
    /// <para> (EN) : Zero out Rigidbody2D velocity. </para>
    /// </summary>
    #endregion
    protected void StopVelocity() =>
        Context.Rb2.linearVelocity = Vector2.zero;

    #region Documentation - Notes
    // ─── Input Pipeline ───────────────────────────────────────────────────
    #endregion
    #region Documentation - XML
    /// <summary>
    /// <para> Summary : </para>
    /// <para> (TH) : ประมวลผล input ตามลำดับ ClampMagnitude(1), TransformInput และ SnapDirection แล้วคืนทิศสุดท้ายให้การเคลื่อนที่และการหันภาพ </para>
    /// <para> (EN) : Full keyboard/gamepad input pipeline: ClampMagnitude(1) → TransformInput → SnapDirection. Returns the final snapped direction used for both velocity and <see cref="ApplyDirection"/>. </para>
    /// </summary>
    /// <returns>
    /// <para> Return : </para>
    /// <para> (TH) : ทิศหลัง ClampMagnitude, TransformInput และ SnapDirection </para>
    /// <para> (EN) : Direction after ClampMagnitude, TransformInput and SnapDirection. </para>
    /// </returns>
    #endregion
    protected Vector3 ProcessInput2D(Vector2 rawInput)
    {
        return Context.SnapDirection(TransformInput2D(rawInput));
    }

    #region Documentation - XML
    /// <summary>
    /// <para> Summary : </para>
    /// <para> (TH) : จำกัดขนาด input แล้ว TransformInput โดยไม่ snap เพื่อใช้ทิศดิบกับความเร็วและ snap แยกเฉพาะทิศภาพ </para>
    /// <para> (EN) : Partial input pipeline: ClampMagnitude(1) → TransformInput only (no snap). Use when you need the raw transformed direction for velocity while snapping only for the display direction. </para>
    /// </summary>
    /// <returns>
    /// <para> Return : </para>
    /// <para> (TH) : ทิศหลัง ClampMagnitude และ TransformInput โดยยังไม่ snap </para>
    /// <para> (EN) : Direction after ClampMagnitude and TransformInput, without snapping. </para>
    /// </returns>
    #endregion
    protected Vector3 TransformInput2D(Vector2 rawInput)
    {
        Vector2 clamped = Vector2.ClampMagnitude(rawInput, 1f);
        return Context.TransformInput(new Vector3(clamped.x, clamped.y, 0f));
    }
}
