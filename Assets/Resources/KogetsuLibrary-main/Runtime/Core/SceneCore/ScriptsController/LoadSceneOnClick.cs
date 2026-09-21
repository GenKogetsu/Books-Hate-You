using UnityEngine;
using Kogetsu.Library.Core;

public class LoadSceneOnClick : MonoBehaviour
{
    [SerializeField] private int _sceneIndex;

    public void OnClick() => BasicSceneEffectController.Instance.LoadScene(_sceneIndex);
}
