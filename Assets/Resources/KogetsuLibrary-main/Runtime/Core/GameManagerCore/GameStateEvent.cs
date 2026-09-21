using Kogetsu.Library.Core;

namespace Kogetsu.Library.DesignPatternCore;

public record struct GameStateEvent(GameState State) : IEvent;
