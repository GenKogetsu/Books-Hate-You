using Kogetsu.Library.DesignPatternCore;


public record struct LoadSceneEvent(int SceneIndex = -1, string SceneName = "") : IEvent;
