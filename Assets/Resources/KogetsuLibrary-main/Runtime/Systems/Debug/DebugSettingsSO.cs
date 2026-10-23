using UnityEngine;

namespace Kogetsu.Library.Debugging
{
    [CreateAssetMenu(fileName = "DebugSettings", menuName = "Kogetsu/Debug Settings")]
    public sealed class DebugSettingsSO : ScriptableObject
    {
        [Header("Master Switch")]
        [SerializeField] private bool _enableDebug;

        [Header("Categories")]
        [SerializeField] private bool _general;
        [SerializeField] private bool _player;
        [SerializeField] private bool _enemy;
        [SerializeField] private bool _interaction;
        [SerializeField] private bool _combat;
        [SerializeField] private bool _movement;
        [SerializeField] private bool _animation;
        [SerializeField] private bool _objectPool;
        [SerializeField] private bool _eventBus;
        [SerializeField] private bool _scene;
        [SerializeField] private bool _visualNovel;
        [SerializeField] private bool _audio;

        public bool IsEnabled(DebugCategory category)
        {
            if (!_enableDebug)
                return false;

            return category switch
            {
                DebugCategory.General => _general,
                DebugCategory.Player => _player,
                DebugCategory.Enemy => _enemy,
                DebugCategory.Interaction => _interaction,
                DebugCategory.Combat => _combat,
                DebugCategory.Movement => _movement,
                DebugCategory.Animation => _animation,
                DebugCategory.ObjectPool => _objectPool,
                DebugCategory.EventBus => _eventBus,
                DebugCategory.Scene => _scene,
                DebugCategory.VisualNovel => _visualNovel,
                DebugCategory.Audio => _audio,
                _ => false
            };
        }
    }
}
