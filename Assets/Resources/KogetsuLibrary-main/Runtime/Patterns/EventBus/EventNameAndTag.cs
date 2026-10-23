namespace Kogetsu.Library.DesignPatternCore;


public record struct EventNameAndTag(string Name, string Tag) : IEvent;
