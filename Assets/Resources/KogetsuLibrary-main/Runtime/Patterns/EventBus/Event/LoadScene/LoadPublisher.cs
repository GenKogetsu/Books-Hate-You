using Kogetsu.Library.DesignPatternCore;
using Kogetsu.Library.Core;
using UnityEngine;

public class LoadPublisher : MonoBehaviour
{
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
