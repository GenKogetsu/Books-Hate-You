using Kogetsu.Library.DesignPatternCore;


public record struct VNAmbientEvent(VNAmbientEventType AmbientEvent) : IEvent;
