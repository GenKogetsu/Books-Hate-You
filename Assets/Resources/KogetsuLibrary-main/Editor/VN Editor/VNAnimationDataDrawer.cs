#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using Kogetsu.Library.Core;
using System.Collections.Generic;

namespace Kogetsu.Library.Editor
{
    [CustomPropertyDrawer(typeof(VNCharacterSO.VNBehaviorAnimationData))]
    public class VNAnimationDataDrawer : PropertyDrawer
    {
        private static readonly Dictionary<string, int> _arraySizes = new();
        private readonly Color _themeColor = new(0.55f, 0.75f, 0.95f); // ฟ้าพาสเทลนุ่มนวล

        public override void OnGUI(Rect pos, SerializedProperty prop, GUIContent label)
        {
            CheckAndClearIfNewlyAdded(prop);

            EditorGUI.BeginProperty(pos, label, prop);
            var index = GetIndex(prop.propertyPath);

            var foldoutRect = new Rect(pos.x + 4f, pos.y + 3f, pos.width - 54f, EditorGUIUtility.singleLineHeight);
            var clearBtnRect = new Rect(pos.x + pos.width - 45f, pos.y + 3f, 45f, EditorGUIUtility.singleLineHeight);

            GUIStyle headerStyle = new(EditorStyles.foldout)
            {
                fontStyle = FontStyle.Bold,
                fontSize = 13
            };
            headerStyle.normal.textColor = _themeColor;
            headerStyle.onNormal.textColor = _themeColor;
            headerStyle.focused.textColor = _themeColor;
            headerStyle.onFocused.textColor = _themeColor;

            prop.isExpanded = EditorGUI.Foldout(foldoutRect, prop.isExpanded, $"Animation {index}", true, headerStyle);

            if (GUI.Button(clearBtnRect, "Clear", EditorStyles.miniButton))
            {
                prop.FindPropertyRelative("BehaviorAnimationClip").objectReferenceValue = null;
                prop.serializedObject.ApplyModifiedProperties();
                GUIUtility.keyboardControl = 0;
            }

            if (prop.isExpanded)
            {
                EditorGUI.indentLevel++;
                var y = pos.y + EditorGUIUtility.singleLineHeight + 7f;

                // new : เปลี่ยนไปใช้ฟังก์ชันวาดแบบเช็ค Required
                DrawRequiredProperty(ref y, pos, prop.FindPropertyRelative("BehaviorAnimationClip"));

                EditorGUI.indentLevel--;
            }

            EditorGUI.EndProperty();
        }

        // new : ฟังก์ชันวาด Property พร้อมระบบแจ้งเตือน Error
        private void DrawRequiredProperty(ref float y, Rect pos, SerializedProperty prop)
        {
            if (prop.objectReferenceValue == null)
            {
                var helpBoxRect = new Rect(pos.x, y, pos.width, 30f);
                EditorGUI.HelpBox(helpBoxRect, $"{prop.name} is required", MessageType.Error);
                y += 32f;
            }

            EditorGUI.PropertyField(new Rect(pos.x, y, pos.width, EditorGUIUtility.singleLineHeight), prop);
            y += EditorGUIUtility.singleLineHeight + 2f;
        }

        public override float GetPropertyHeight(SerializedProperty prop, GUIContent label)
        {
            var h = EditorGUIUtility.singleLineHeight + 6f;
            if (prop.isExpanded)
            {
                h += 4f;
                // new : คำนวณความสูงโดยเช็คเผื่อกล่อง Error ไว้ด้วย
                h += GetRequiredPropertyHeight(prop.FindPropertyRelative("BehaviorAnimationClip"));
            }
            return h;
        }

        // new : ฟังก์ชันคำนวณความสูงของแต่ละตัวแปร (บวกเพิ่มถ้าเป็น Null)
        private float GetRequiredPropertyHeight(SerializedProperty prop)
        {
            float h = EditorGUIUtility.singleLineHeight + 2f;
            if (prop.objectReferenceValue == null) h += 32f;
            return h;
        }

        private void CheckAndClearIfNewlyAdded(SerializedProperty property)
        {
            string path = property.propertyPath;
            int lastBracket = path.LastIndexOf('[');
            if (lastBracket < 0) return;

            string arrayPath = path[..lastBracket];
            var arrayProp = property.serializedObject.FindProperty(arrayPath);

            if (arrayProp != null && arrayProp.isArray)
            {
                int currentSize = arrayProp.arraySize;
                int currentIndex = GetIndex(path) - 1;

                if (_arraySizes.TryGetValue(arrayPath, out int prevSize))
                {
                    if (currentSize > prevSize && currentIndex == currentSize - 1)
                    {
                        property.FindPropertyRelative("BehaviorAnimationClip").objectReferenceValue = null;
                        property.serializedObject.ApplyModifiedProperties();
                    }
                }
                if (currentIndex == currentSize - 1) _arraySizes[arrayPath] = currentSize;
            }
        }

        private int GetIndex(string path) { var s = path.LastIndexOf('[') + 1; var e = path.LastIndexOf(']'); return (s > 0 && e > s && int.TryParse(path[s..e], out var i)) ? i + 1 : 1; }
    }
}
#endif