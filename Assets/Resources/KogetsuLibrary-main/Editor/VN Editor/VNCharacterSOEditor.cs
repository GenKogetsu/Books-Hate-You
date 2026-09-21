#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using Kogetsu.Library.Core;

namespace Kogetsu.Library.Editor
{
    [CustomEditor(typeof(VNCharacterSO))]
    public class VNCharacterSOEditor : UnityEditor.Editor
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
                    // ข้ามการวาดลิสต์ข้อมูลที่ยาวๆ เพื่อย้ายไปแก้ไขใน Window แทน
                    if (iterator.name == "Emotions" ||
                        iterator.name == "Animations" ||
                        iterator.name == "SoundEffects")
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

            // เปลี่ยนสีเป็นโทนฟ้าอ่อนๆ เพื่อแยกความแตกต่างจากปุ่ม Chapter (สีม่วง)
            GUI.backgroundColor = new Color(0.65f, 0.85f, 1f);

            if (GUILayout.Button("Open in VN Character Editor", buttonStyle))
            {
                VNCharacterSO character = (VNCharacterSO)target;
                VNCharacterEditorWindow.OpenCharacter(character);
            }

            GUI.backgroundColor = Color.white;

            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif