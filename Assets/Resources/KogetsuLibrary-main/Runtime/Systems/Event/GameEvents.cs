using Kogetsu.Library.DesignPatternCore;

namespace Kogetsu.Library.Core;

public record struct PlayerRollEvent(Vector2 Direction)                        : IEvent;
public record struct PlayerShootEvent(Vector2 MuzzlePosition, int PelletCount) : IEvent;
public record struct FlashlightToggleEvent(bool IsOn)                          : IEvent;
public record struct PlayerJumpEvent()                                         : IEvent;
public record struct PlayerDeathEvent()                                        : IEvent;
public record struct PlayerLandEvent()                                         : IEvent;

#region Documentation - XML
/// <summary>
/// <para> Summary : </para>
/// <para> (TH) : broadcast ทุกครั้งที่ energy เปลี่ยน — ใช้อัปเดต HUD energy bar </para>
/// <para> (EN) : Broadcasts energy changes for consumers such as the energy HUD. </para>
/// </summary>
#endregion
public record struct EnergyChangedEvent(float Current, float Max)              : IEvent;
