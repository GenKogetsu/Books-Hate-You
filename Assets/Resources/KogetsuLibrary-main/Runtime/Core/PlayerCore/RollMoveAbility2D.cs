using System;

namespace Kogetsu.Library.Core;

/// <summary>
/// Roll/Dodge ability สำหรับ 2D
/// - ดัน Rigidbody ทันทีที่ Enter แล้วรอจบ duration ใน FixedUpdate
/// - cooldown ถูก tick โดย MoveController2D.Update() ตลอดเวลา
/// - ใช้ IReturnableMovement เพื่อ decouple จาก MoveController2D โดยตรง
/// </summary>
[Serializable]
public class RollMoveAbility2D : BaseMoveAbility2D
{
    [Header("Roll Settings")]
    [SerializeField] private float _rollForce    = 14f;
    [SerializeField] private float _rollDuration = 0.28f;
    [SerializeField] private float _cooldown     = 1.5f;

    // ─── Runtime ───────────────────────────────────────────────────────────
    private float               _rollTimer;
    private float               _cooldownTimer;
    private IReturnableMovement _owner;

    // ─── Public State ──────────────────────────────────────────────────────
    public bool  IsOnCooldown  => _cooldownTimer > 0f;
    public float CooldownRatio => _cooldown > 0f ? Mathf.Clamp01(_cooldownTimer / _cooldown) : 0f;

    // ─── Initialize ────────────────────────────────────────────────────────
    public override void Initialize(IMoveContext2D context)
    {
        base.Initialize(context);
        // Cast to IReturnableMovement — decoupled from MoveController2D directly.
        // Any controller that implements the interface will work.
        _owner = context as IReturnableMovement;
    }

    // ─── State ─────────────────────────────────────────────────────────────
    protected override void OnEnter()
    {
        _rollTimer     = _rollDuration;
        _cooldownTimer = _cooldown;

        // Roll in the direction the player is currently moving; fall back to last facing.
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

    // ─── Cooldown ──────────────────────────────────────────────────────────
    /// <summary>Called every frame by MoveController2D.Update() regardless of active state.</summary>
    public void TickCooldown(float dt)
    {
        if (_cooldownTimer > 0f)
            _cooldownTimer -= dt;
    }
}
