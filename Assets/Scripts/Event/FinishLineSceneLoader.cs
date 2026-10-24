using Kogetsu.Library.Core;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public sealed class FinishLineSceneLoader : MonoBehaviour
{
    [Header("Scene")]
    [SerializeField, Min(0)] private int _sceneBuildIndex;
    [SerializeField] private bool _loadOnlyOnce = true;

    [Header("Player Spawn In Destination Scene")]
    [SerializeField] private PlayerSceneWarpState _playerSceneWarpState;
    [SerializeField] private PlayerTransformSnapshot _destinationPlayerTransform = PlayerTransformSnapshot.Identity;

    [Header("Target")]
    [SerializeField] private TargetTag _targetTags = TargetTag.Player;

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
        if (!_targetTags.MatchesUnityTag(player.tag))
            return;

        if (!BasicSceneEffectController.Instance)
        {
            Debug.LogError(
                "[FinishLineSceneLoader] ไม่พบ BasicSceneEffectController ใน Scene",
                this);
            return;
        }

        if (!_playerSceneWarpState)
        {
            Debug.LogError(
                "[FinishLineSceneLoader] ไม่ได้กำหนด PlayerSceneWarpState",
                this);
            return;
        }

        _hasLoaded = true;
        _playerSceneWarpState.SetPendingTransform(_destinationPlayerTransform);
        BasicSceneEffectController.Instance.LoadScene(_sceneBuildIndex);
    }
}
