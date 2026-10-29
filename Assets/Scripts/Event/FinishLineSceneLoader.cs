#region Imports
using Kogetsu.Library.Core;
using Kogetsu.Library.DesignPatternCore;
using UnityEngine;
#endregion
#region FinishLineSceneLoader

[RequireComponent(typeof(Collider))]
public sealed class FinishLineSceneLoader : MonoBehaviour
{
    #region Fields and configuration
    [Header("Scene")]
    #endregion
    #region Internal helpers
    [SerializeField, Min(0)] private int _sceneBuildIndex;
    [SerializeField] private bool _loadOnlyOnce = true;

    [Header("Player Spawn In Destination Scene")]
    [SerializeField] private PlayerSceneWarpState _playerSceneWarpState;
    [SerializeField] private PlayerTransformSnapshot _destinationPlayerTransform = PlayerTransformSnapshot.Identity;

    [Header("Target")]
    [SerializeField] private TargetTag _targetTags = TargetTag.Player;

    private bool _hasLoaded;

    #endregion
    #region Unity lifecycle
    private void Reset()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    #endregion
    #region Event handlers
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

        if (!EventBus.Instance)
        {
            Debug.LogError("[FinishLineSceneLoader] ไม่พบ EventBus ใน Scene", this);
            return;
        }

        _hasLoaded = BasicSceneEffectController.Instance.TryLoadSceneWithEvent(
            _sceneBuildIndex,
            () => _playerSceneWarpState.SetPendingTransform(_destinationPlayerTransform));
    }
    #endregion
}
#endregion
