using Kogetsu.Library.Core;
using Kogetsu.Library.DesignPatternCore;


public record struct VNTypingSkipEvent(ushort CurrentConversationIndex , VNCurrentPhase CurrentPhase) : IEvent;
