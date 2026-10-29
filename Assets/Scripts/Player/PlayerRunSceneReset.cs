#region Imports
using Kogetsu.Library.DesignPatternCore;
using UnityEngine;
#endregion
#region PlayerRunSceneReset

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
#endregion
