using Kogetsu.Library.Core;
using UnityEngine;

public class LoadNextSceneAffter : MonoBehaviour
{
    [SerializeReference] private float LoadSceneAffter = 2f;

    private void Start()
    {
        StartCoroutine(LoadNextScene());
    }

    private IEnumerator LoadNextScene()
    {
        yield return new WaitForSeconds(LoadSceneAffter);
        BasicSceneEffectController.Instance.LoadNextScene(1);
    }
}
