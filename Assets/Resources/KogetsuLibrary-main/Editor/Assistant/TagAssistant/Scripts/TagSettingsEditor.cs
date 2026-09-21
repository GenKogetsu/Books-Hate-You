#if UNITY_EDITOR
using UnityEditor;
using Kogetsu.Library.Assistant;

namespace Kogetsu.Library.Editor
{
    [CustomEditor(typeof(TagSettingsSO))]
    public class TagSettingsEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUILayout.Space(15);
            GUI.backgroundColor = new Color(0.53f, 0.81f, 0.92f);

            if (GUILayout.Button("Update Tags Manually", GUILayout.Height(35)))
            {
                EditorUtility.SetDirty(target);
                AssetDatabase.SaveAssets();
                TagInitializer.InitializeTags();
            }

            GUI.backgroundColor = Color.white;
            EditorGUILayout.HelpBox("Tags auto-update on Domain Reload. Use the button to force sync now.", MessageType.Info);
        }
    }
}
#endif