#if UNITY_EDITOR
using UnityEditor;
using Kogetsu.Library.Core;
using System.Collections.Generic;

namespace Kogetsu.Library.Editor
{
    [CustomPropertyDrawer(typeof(VNCharacterSO.VNEmotionData))]
    public class VNEmotionDataDrawer : PropertyDrawer
    {
        private static readonly Dictionary<string, int> _arraySizes = new();
        private readonly Color _themeColor = new(0.6f, 0.85f, 0.65f); // เขียวพาสเทลนุ่มนวล

        private static bool _showSpritePreview = true;

        public override void OnGUI(Rect pos, SerializedProperty prop, GUIContent label)
        {
            CheckAndClearIfNewlyAdded(prop);

            EditorGUI.BeginProperty(pos, label, prop);
            var index = GetIndex(prop.propertyPath);

            if (index == 1)
            {
                var globalToggleRect = new Rect(pos.x, pos.y, pos.width, EditorGUIUtility.singleLineHeight);
                _showSpritePreview = EditorGUI.ToggleLeft(globalToggleRect, "Show All Sprite Previews", _showSpritePreview, EditorStyles.boldLabel);
                pos.y += EditorGUIUtility.singleLineHeight + 4f;
            }

            var foldoutRect = new Rect(pos.x + 4f, pos.y + 3f, pos.width - 54f, EditorGUIUtility.singleLineHeight);
            var clearBtnRect = new Rect(pos.x + pos.width - 45f, pos.y + 3f, 45f, EditorGUIUtility.singleLineHeight);

            // 🚀 ล็อคสี State ป้องกันบั๊กคลิกเลือกแล้วกลายเป็นสีขาว
            GUIStyle headerStyle = new(EditorStyles.foldout) { fontStyle = FontStyle.Bold, fontSize = 13 };
            headerStyle.normal.textColor = _themeColor;
            headerStyle.hover.textColor = _themeColor;
            headerStyle.focused.textColor = _themeColor;
            headerStyle.active.textColor = _themeColor;
            headerStyle.onNormal.textColor = _themeColor;
            headerStyle.onHover.textColor = _themeColor;
            headerStyle.onFocused.textColor = _themeColor;
            headerStyle.onActive.textColor = _themeColor;

            prop.isExpanded = EditorGUI.Foldout(foldoutRect, prop.isExpanded, $"Emotion {index}", true, headerStyle);

            if (GUI.Button(clearBtnRect, "Clear", EditorStyles.miniButton))
            {
                prop.FindPropertyRelative("EmoteSprite").objectReferenceValue = null;
                prop.FindPropertyRelative("EmoteClip").objectReferenceValue = null;
                prop.serializedObject.ApplyModifiedProperties();
                GUIUtility.keyboardControl = 0;
            }

            if (prop.isExpanded)
            {
                EditorGUI.indentLevel++;
                var y = pos.y + EditorGUIUtility.singleLineHeight + 7f;

                DrawRequiredProperty(ref y, pos, prop.FindPropertyRelative("EmoteSprite"));
                DrawRequiredProperty(ref y, pos, prop.FindPropertyRelative("EmoteClip"));

                // 🚀 วาด Preview ตามขนาดจริง ไม่บีบ ไม่ยืด
                if (_showSpritePreview)
                {
                    var spriteProp = prop.FindPropertyRelative("EmoteSprite");
                    if (spriteProp.objectReferenceValue is Sprite sprite && sprite.texture != null && sprite.textureRect.height > 0)
                    {
                        float aspect = sprite.textureRect.width / sprite.textureRect.height;
                        float maxWidth = pos.width - 15f;

                        // ใช้ขนาดจริงเป็นหลัก (nativeW) แต่ถ้าใหญ่ล้นจอ ให้หดลงมา (Mathf.Min)
                        float nativeW = sprite.textureRect.width;
                        float previewWidth = Mathf.Min(nativeW, maxWidth);
                        float previewHeight = previewWidth / aspect;

                        var boxRect = new Rect(pos.x + 15f, y, previewWidth, previewHeight + 4f);
                        GUI.Box(boxRect, GUIContent.none, EditorStyles.helpBox);

                        var texRect = new Rect(boxRect.x + 2f, boxRect.y + 2f, boxRect.width - 4f, boxRect.height - 4f);
                        Rect uvs = new(
                            sprite.textureRect.x / sprite.texture.width,
                            sprite.textureRect.y / sprite.texture.height,
                            sprite.textureRect.width / sprite.texture.width,
                            sprite.textureRect.height / sprite.texture.height
                        );

                        GUI.DrawTextureWithTexCoords(texRect, sprite.texture, uvs, true);
                        y += previewHeight + 8f;
                    }
                }

                EditorGUI.indentLevel--;
            }

            EditorGUI.EndProperty();
        }

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
            int index = GetIndex(prop.propertyPath);

            if (index == 1) h += EditorGUIUtility.singleLineHeight + 4f;

            if (prop.isExpanded)
            {
                h += 4f;
                h += GetRequiredPropertyHeight(prop.FindPropertyRelative("EmoteSprite"));
                h += GetRequiredPropertyHeight(prop.FindPropertyRelative("EmoteClip"));

                if (_showSpritePreview)
                {
                    var spriteProp = prop.FindPropertyRelative("EmoteSprite");
                    if (spriteProp.objectReferenceValue is Sprite sprite && sprite.texture != null && sprite.textureRect.height > 0)
                    {
                        float aspect = sprite.textureRect.width / sprite.textureRect.height;
                        float maxWidth = EditorGUIUtility.currentViewWidth - 85f;

                        // กะความสูงล่วงหน้าตามขนาดจริง
                        float nativeW = sprite.textureRect.width;
                        float previewWidth = Mathf.Min(nativeW, maxWidth);
                        float previewHeight = previewWidth / aspect;

                        h += previewHeight + 8f;
                    }
                }
            }
            return h;
        }

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
                        property.FindPropertyRelative("EmoteSprite").objectReferenceValue = null;
                        property.FindPropertyRelative("EmoteClip").objectReferenceValue = null;
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