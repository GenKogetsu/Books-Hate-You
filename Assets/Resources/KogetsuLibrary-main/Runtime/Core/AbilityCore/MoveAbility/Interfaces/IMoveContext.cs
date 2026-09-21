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

    /// <summary>true เมื่อยังกระโดดได้ (เช็คก่อน apply jump force เสมอ)</summary>
    bool CanJump { get; }

    /// <summary>เรียกหลัง apply jump force เพื่อนับจำนวนครั้ง</summary>
    void ConsumeJump();

    /// <summary>reset ตัวนับ — เรียกอัตโนมัติเมื่อ IsGrounded เป็น true</summary>
    void ResetJumps();

    Vector3 SnapDirection(Vector3 rawInput);

    /// <summary>
    /// Transform raw input before applying velocity. Used for perspective-mode diagonal walking.
    /// </summary>
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

/// <summary>
/// Implemented by controllers that can switch back to their default movement state.
/// Used by abilities such as RollMoveAbility2D to return control when finished.
/// </summary>
public interface IReturnableMovement
{
    void ReturnToNormalMovement();
}
