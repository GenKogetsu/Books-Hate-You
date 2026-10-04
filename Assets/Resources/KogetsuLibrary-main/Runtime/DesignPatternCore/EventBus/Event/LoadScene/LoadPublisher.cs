using Kogetsu.Library.DesignPatternCore;
using UnityEngine;

public class LoadPublisher : MonoBehaviour
{
    public void PublishLoadSceneEvent(int sceneBuildIndex)
    {
        EventBus.Instance.Publish(new LoadSceneEvent(SceneIndex: sceneBuildIndex));
    }

    public void PublishLoadSceneEvent(string sceneName)
    {
        EventBus.Instance.Publish(new LoadSceneEvent(SceneName: sceneName));
    }

    public void ReloadSceneEvent()
    {
        EventBus.Instance.Publish(new LoadSceneEvent(SceneName: "Reload"));
    }


}
