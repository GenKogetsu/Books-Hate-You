using Kogetsu.Library.DesignPatternCore;
using Kogetsu.Library.Core;
using UnityEngine;

public class LoadPublisher : MonoBehaviour
{
    public void PublishOutGameEvent()
    {
        if (!EventBus.Instance || !BasicSceneEffectController.Instance ||
            BasicSceneEffectController.Instance.IsTransitioning)
            return;

        EventBus.Instance.Publish(new OutGameEvent());
        if (BasicSceneEffectController.Instance.IsTransitioning)
            DisableButton();
    }

    public void PublishLoadSceneEvent(int sceneBuildIndex)
    {
        if (BasicSceneEffectController.Instance &&
            BasicSceneEffectController.Instance.TryLoadSceneWithEvent(sceneBuildIndex))
            DisableButton();
    }

    public void PublishLoadSceneEvent(string sceneName)
    {
        if (BasicSceneEffectController.Instance &&
            BasicSceneEffectController.Instance.TryLoadSceneWithEvent(sceneName))
            DisableButton();
    }

    public void ReloadSceneEvent()
    {
        if (BasicSceneEffectController.Instance &&
            BasicSceneEffectController.Instance.TryReloadSceneWithEvent())
            DisableButton();
    }

    private void DisableButton()
    {
        if (TryGetComponent<UnityEngine.UI.Button>(out var button))
        {
            button.interactable = false;
        }
    }

}
