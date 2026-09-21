using Kogetsu.Library.DesignPatternCore;

namespace Kogetsu.Library.Core;

public record struct PlayerRollEvent(Vector2 Direction)                        : IEvent;
public record struct PlayerShootEvent(Vector2 MuzzlePosition, int PelletCount) : IEvent;
public record struct FlashlightToggleEvent(bool IsOn)                          : IEvent;
public record struct PlayerJumpEvent()                                         : IEvent;
public record struct PlayerDeathEvent()                                        : IEvent;
public record struct PlayerLandEvent()                                         : IEvent;

/// <summary>broadcast ทุกครั้งที่ energy เปลี่ยน — ใช้อัปเดต HUD energy bar</summary>
public record struct EnergyChangedEvent(float Current, float Max)              : IEvent;
