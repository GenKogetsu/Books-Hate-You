#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using Kogetsu.Library.Core;

namespace Kogetsu.Library.Editor
{
    [CustomEditor(typeof(VNSceneSO))]
    public class VNSceneSOEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            SerializedProperty iterator = serializedObject.GetIterator();
            bool enterChildren = true;
            while (iterator.NextVisible(enterChildren))
            {
                using (new EditorGUI.DisabledScope("m_Script" == iterator.propertyPath))
                {
                    if (iterator.name == "Conversations")
                    {
                        continue;
                    }

                    EditorGUILayout.PropertyField(iterator, true);
                }
                enterChildren = false;
            }

            EditorGUILayout.Space(15);

            GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
            buttonStyle.fontStyle = FontStyle.Bold;
            buttonStyle.fontSize = 14;
            buttonStyle.fixedHeight = 40;

            GUI.backgroundColor = new Color(0.9f, 0.7f, 1f);

            if (GUILayout.Button("Open in VN Scene Editor", buttonStyle))
            {
                // [แก้ไข] ส่ง target (ข้อมูลปัจจุบันที่ Inspector กำลังดูอยู่) ไปบังคับเปิดเลย
                VNSceneSO chapter = (VNSceneSO)target;
                VNSceneEditorWindow.OpenVNSceneWindow(chapter);
            }

            GUI.backgroundColor = Color.white;

            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif