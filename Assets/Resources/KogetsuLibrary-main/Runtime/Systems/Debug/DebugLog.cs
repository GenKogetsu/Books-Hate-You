using UnityEngine;

namespace Kogetsu.Library.Debugging
{
    public static class DebugLog
    {
        private const string ResourceName = "KogetsuDebugSettings";
        private static DebugSettingsSO _settings;

        public static bool IsEnabled(DebugCategory category, bool localDebug = false)
        {
            return localDebug || GetSettings()?.IsEnabled(category) == true;
        }

        public static void Log(DebugCategory category, object message, Object context = null, bool localDebug = false)
        {
            if (IsEnabled(category, localDebug))
                UnityEngine.Debug.Log($"[{category}] {message}", context);
        }

        public static void LogWarning(DebugCategory category, object message, Object context = null, bool localDebug = false)
        {
            if (IsEnabled(category, localDebug))
                UnityEngine.Debug.LogWarning($"[{category}] {message}", context);
        }

        private static DebugSettingsSO GetSettings()
        {
            if (!_settings)
                _settings = Resources.Load<DebugSettingsSO>(ResourceName);

            return _settings;
        }
    }
}
