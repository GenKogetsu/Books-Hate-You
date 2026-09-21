namespace Kogetsu.Library.DesignPatternCore;


public record struct TakeDamageEvent(float Damage , float CurrentHp) : IEvent;
