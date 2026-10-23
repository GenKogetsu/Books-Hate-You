#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using Kogetsu.Library.Debugging;

namespace Kogetsu.Library.EditorTools
{
    public static class DebugSettingsCreator
    {
        [MenuItem("Tools/KogetsuLibrary/Create Debug Settings")]
        private static void Create()
        {
            const string folder = "Assets/Resources";
            const string path = folder + "/KogetsuDebugSettings.asset";

            if (AssetDatabase.LoadAssetAtPath<DebugSettingsSO>(path))
            {
                Selection.activeObject = AssetDatabase.LoadAssetAtPath<DebugSettingsSO>(path);
                return;
            }

            var asset = ScriptableObject.CreateInstance<DebugSettingsSO>();
            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();
            Selection.activeObject = asset;
            EditorGUIUtility.PingObject(asset);
        }
    }
}
#endif
