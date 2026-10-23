using Kogetsu.Library.Core;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public sealed class FinishLineSceneLoader : MonoBehaviour
{
    [Header("Scene")]
    [SerializeField, Min(0)] private int _sceneBuildIndex;
    [SerializeField] private bool _loadOnlyOnce = true;

    [Header("Target")]
    [SerializeField] private string _playerTag = "Player";

    private bool _hasLoaded;

    private void Reset()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_loadOnlyOnce && _hasLoaded)
            return;

        Transform player = other.transform.root;
        if (!player.CompareTag(_playerTag))
            return;

        if (!BasicSceneEffectController.Instance)
        {
            Debug.LogError(
                "[FinishLineSceneLoader] ไม่พบ BasicSceneEffectController ใน Scene",
                this);
            return;
        }

        _hasLoaded = true;
        BasicSceneEffectController.Instance.LoadScene(_sceneBuildIndex);
    }
}
