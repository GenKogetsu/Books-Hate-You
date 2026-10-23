using System;

namespace Kogetsu.Library.Core;

#region Documentation - XML
/// <summary>
/// <para> Summary : </para>
/// <para> (TH) : Roll/Dodge ability สำหรับ 2D - ดัน Rigidbody ทันทีที่ Enter แล้วรอจบ duration ใน FixedUpdate - cooldown ถูก tick โดย MoveController2D.Update() ตลอดเวลา - ใช้ IReturnableMovement เพื่อ decouple จาก MoveController2D โดยตรง </para>
/// <para> (EN) : Implements a 2D roll with an entry impulse and fixed-update duration; cooldown ticks independently and IReturnableMovement restores default control. </para>
/// </summary>
#endregion
[Serializable]
public class RollMoveAbility2D : BaseMoveAbility2D
{
    [Header("Roll Settings")]
    [SerializeField] private float _rollForce    = 14f;
    [SerializeField] private float _rollDuration = 0.28f;
    [SerializeField] private float _cooldown     = 1.5f;

    #region Documentation - Notes
    // ─── Runtime ───────────────────────────────────────────────────────────
    #endregion
    private float               _rollTimer;
    private float               _cooldownTimer;
    private IReturnableMovement _owner;

    #region Documentation - Notes
    // ─── Public State ──────────────────────────────────────────────────────
    #endregion
    public bool  IsOnCooldown  => _cooldownTimer > 0f;
    public float CooldownRatio => _cooldown > 0f ? Mathf.Clamp01(_cooldownTimer / _cooldown) : 0f;

    #region Documentation - Notes
    // ─── Initialize ────────────────────────────────────────────────────────
    #endregion
    public override void Initialize(IMoveContext2D context)
    {
        base.Initialize(context);
        #region Documentation - Notes
        // Cast to IReturnableMovement — decoupled from MoveController2D directly.
        // Any controller that implements the interface will work.
        #endregion
        _owner = context as IReturnableMovement;
    }

    #region Documentation - Notes
    // ─── State ─────────────────────────────────────────────────────────────
    #endregion
    protected override void OnEnter()
    {
        _rollTimer     = _rollDuration;
        _cooldownTimer = _cooldown;

        #region Documentation - Notes
        // Roll in the direction the player is currently moving; fall back to last facing.
        #endregion
        Vector2 dir = Context.CurrentDirection != Vector3.zero
            ? (Vector2)Context.CurrentDirection
            : (Vector2)Context.LastFacingDirection;

        if (dir == Vector2.zero) dir = Vector2.right;

        SetVelocity(dir.normalized * _rollForce);
    }

    protected override void OnFixedUpdate()
    {
        _rollTimer -= Time.fixedDeltaTime;
        if (_rollTimer <= 0f)
            _owner?.ReturnToNormalMovement();
    }

    protected override void OnExit() { }

    #region Documentation - Notes
    // ─── Cooldown ──────────────────────────────────────────────────────────
    #endregion
    #region Documentation - XML
    /// <summary>
    /// <para> Summary : </para>
    /// <para> (TH) : ถูกเรียกทุกเฟรมโดย MoveController2D.Update ไม่ว่า ability ใดเป็น state ปัจจุบัน </para>
    /// <para> (EN) : Called every frame by MoveController2D.Update() regardless of active state. </para>
    /// </summary>
    #endregion
    public void TickCooldown(float dt)
    {
        if (_cooldownTimer > 0f)
            _cooldownTimer -= dt;
    }
}
