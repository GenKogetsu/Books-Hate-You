#if UNITY_EDITOR
using UnityEditor;
using Kogetsu.Library.Core;

namespace Kogetsu.Library.Editor
{
    [CustomPropertyDrawer(typeof(VNChoiceAnswer))]
    public class VNChoiceAnswerDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            #region Documentation - Notes
            // สีพื้นหลังกล่อง ถ้าย้อมสี ให้เป็นสีฟ้าอ่อนๆ
            #endregion
            Color bgColor = VNConversationNodeDrawer.UseTint ? new Color(0.92f, 0.96f, 1f) : Color.white;
            GUI.color = bgColor;
            #region Documentation - Notes
            // ขยับกล่องไปทางซ้าย 5 หน่วย และชดเชยความกว้าง
            #endregion
            GUI.Box(new Rect(position.x - 5f, position.y, position.width + 5f, GetPropertyHeight(property, label)), "", EditorStyles.helpBox);
            GUI.color = Color.white;

            var index = GetIndex(property.propertyPath);

            #region Documentation - Notes
            // ขยับกรอบของ Foldout ไปทางขวา 20 หน่วย
            #endregion
            var foldoutRect = new Rect(position.x + 20f, position.y + 2f, position.width - 14f, EditorGUIUtility.singleLineHeight);

            #region Documentation - Notes
            // สร้าง Style พิเศษเพื่อเพิ่มช่องว่างระหว่างลูกศร (Foldout) กับตัวหนังสือ
            #endregion
            GUIStyle headerStyle = new GUIStyle(EditorStyles.foldout) { fontStyle = FontStyle.Bold };
            headerStyle.padding.right += 2;

            if (VNConversationNodeDrawer.UseTint) headerStyle.normal.textColor = new Color(0.2f, 0.4f, 0.6f);

            property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, $"Choice {index}", true, headerStyle);

            if (property.isExpanded)
            {
                EditorGUI.indentLevel++;
                float currentY = position.y + EditorGUIUtility.singleLineHeight + 6f;

                #region Documentation - Notes
                // Answer Number
                #endregion
                var numProp = property.FindPropertyRelative("AnswerNumber");
                EditorGUI.PropertyField(new Rect(position.x, currentY, position.width, EditorGUIUtility.singleLineHeight), numProp);
                currentY += EditorGUIUtility.singleLineHeight + 4f;

                #region Documentation - Notes
                // Answer Text Label
                #endregion
                EditorGUI.LabelField(new Rect(position.x, currentY, position.width, EditorGUIUtility.singleLineHeight), "Answer Text");
                currentY += EditorGUIUtility.singleLineHeight;

                #region Documentation - Notes
                // Answer Text Area
                #endregion
                var textProp = property.FindPropertyRelative("AnswerText");
                float textAreaHeight = 60f; // ความสูงสำหรับกล่องข้อความ
                textProp.stringValue = EditorGUI.TextArea(new Rect(position.x, currentY, position.width, textAreaHeight), textProp.stringValue, EditorStyles.textArea);

                EditorGUI.indentLevel--;
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (!property.isExpanded) return EditorGUIUtility.singleLineHeight + 4f;

            #region Documentation - Notes
            // Height: Header + Margin + Number Field + Label + Text Area + Margin
            #endregion
            return EditorGUIUtility.singleLineHeight + 6f + EditorGUIUtility.singleLineHeight + 4f + EditorGUIUtility.singleLineHeight + 60f + 4f;
        }

        private int GetIndex(string path)
        {
            var s = path.LastIndexOf('[') + 1;
            var e = path.LastIndexOf(']');
            return (s > 0 && e > s && int.TryParse(path[s..e], out var i)) ? i + 1 : 1;
        }
    }
}
#endif