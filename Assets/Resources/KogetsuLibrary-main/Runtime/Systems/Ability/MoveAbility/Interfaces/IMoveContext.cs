using Kogetsu.Library.DesignPatternCore;

namespace Kogetsu.Library.Core;

public interface IMoveContext
{
    Transform Transform { get; }
    StatsController Stats { get; }
    BasicMovementInputObserverSO InputObserverChannel { get; }
    Vector3 CurrentDirection { get; set; }
    Vector3 LastFacingDirection { get; set; }
    bool IsGrounded { get; set; }
    float VerticalVelocity { get; }

    #region Documentation - XML
    /// <summary>
    /// <para> Summary : </para>
    /// <para> (TH) : true เมื่อยังกระโดดได้ (เช็คก่อน apply jump force เสมอ) </para>
    /// <para> (EN) : Indicates whether another jump is allowed; check before applying jump force. </para>
    /// </summary>
    #endregion
    bool CanJump { get; }

    #region Documentation - XML
    /// <summary>
    /// <para> Summary : </para>
    /// <para> (TH) : เรียกหลัง apply jump force เพื่อนับจำนวนครั้ง </para>
    /// <para> (EN) : Records a consumed jump after jump force is applied. </para>
    /// </summary>
    #endregion
    void ConsumeJump();

    #region Documentation - XML
    /// <summary>
    /// <para> Summary : </para>
    /// <para> (TH) : reset ตัวนับ — เรียกอัตโนมัติเมื่อ IsGrounded เป็น true </para>
    /// <para> (EN) : Resets the jump counter when the grounded state becomes true. </para>
    /// </summary>
    #endregion
    void ResetJumps();

    Vector3 SnapDirection(Vector3 rawInput);

    #region Documentation - XML
    /// <summary>
    /// <para> Summary : </para>
    /// <para> (TH) : แปลงทิศ input ดิบก่อนกำหนดความเร็ว เช่น การเดินทแยงในโหมด perspective </para>
    /// <para> (EN) : Transform raw input before applying velocity. Used for perspective-mode diagonal walking. </para>
    /// </summary>
    #endregion
    Vector3 TransformInput(Vector3 input);
}

public interface IMoveContext2D : IMoveContext
{
    Rigidbody2D Rb2 { get; }
}

public interface IMoveContext3D : IMoveContext
{
    Rigidbody Rb { get; }
}

#region Documentation - XML
/// <summary>
/// <para> Summary : </para>
/// <para> (TH) : สัญญาสำหรับ controller ที่กลับไป state เคลื่อนที่เริ่มต้นได้ เพื่อให้ ability เช่น Roll คืนการควบคุมเมื่อจบ </para>
/// <para> (EN) : Implemented by controllers that can switch back to their default movement state. Used by abilities such as RollMoveAbility2D to return control when finished. </para>
/// </summary>
#endregion
public interface IReturnableMovement
{
    void ReturnToNormalMovement();
}
