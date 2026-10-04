using Kogetsu.Library.DesignPatternCore;
using UnityEngine;

public class LoadPublisher : MonoBehaviour
{
    public void PublishLoadSceneEvent(int sceneBuildIndex)
    {
        EventBus.Instance.Publish(new LoadSceneEvent(SceneIndex: sceneBuildIndex));
        DisableButton();
    }

    public void PublishLoadSceneEvent(string sceneName)
    {
        EventBus.Instance.Publish(new LoadSceneEvent(SceneName: sceneName));
        DisableButton();
    }

    public void ReloadSceneEvent()
    {
        EventBus.Instance.Publish(new LoadSceneEvent(SceneName: "Reload"));
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
