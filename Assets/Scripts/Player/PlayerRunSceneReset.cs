using Kogetsu.Library.DesignPatternCore;
using UnityEngine;

/// <summary>Resets run state when the existing scene reload action is invoked.</summary>
public sealed class PlayerRunSceneReset : MonoBehaviour
{
    private void OnEnable()
    {
        if (EventBus.Instance)
            EventBus.Instance.Subscribe<LoadSceneEvent>(OnLoadScene);
    }

    private void OnDisable()
    {
        if (EventBus.Instance)
            EventBus.Instance.Unsubscribe<LoadSceneEvent>(OnLoadScene);
    }

    private static void OnLoadScene(LoadSceneEvent sceneEvent)
    {
        if (sceneEvent.SceneName == "Reload")
            PlayerRunSession.ResetRun();
    }
}
