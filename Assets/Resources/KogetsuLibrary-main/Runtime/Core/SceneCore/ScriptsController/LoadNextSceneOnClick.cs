using UnityEngine;
using Kogetsu.Library.Core;

public class LoadNextSceneOnClick : MonoBehaviour
{
    public void OnClick() => BasicSceneEffectController.Instance.LoadNextScene(1);
}
