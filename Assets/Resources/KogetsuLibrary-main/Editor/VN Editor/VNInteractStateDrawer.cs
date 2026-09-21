#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Kogetsu.Library.Core;

namespace Kogetsu.Library.Editor
{
    [CustomPropertyDrawer(typeof(VNInteractState))]
    public class VNInteractStateDrawer : PropertyDrawer
    {

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            CheckAndClearIfNewlyAdded(property);
            EditorGUI.BeginProperty(position, label, property);

            Color bgColor = VNConversationNodeDrawer.UseTint ? new Color(0.98f, 0.92f, 0.95f) : Color.white;
            GUI.color = bgColor;
            GUI.Box(new Rect(position.x - 5f, position.y, position.width + 5f, GetPropertyHeight(property, label)), "", EditorStyles.helpBox);
            GUI.color = Color.white;

            var index = GetIndex(property.propertyPath);
            var foldoutRect = new Rect(position.x + 20f, position.y + 2f, position.width - 130f, EditorGUIUtility.singleLineHeight);
            var clearBtnRect = new Rect(position.xMax - 105f, position.y + 2f, 50f, EditorGUIUtility.singleLineHeight);
            var recallBtnRect = new Rect(position.xMax - 50f, position.y + 2f, 50f, EditorGUIUtility.singleLineHeight);

            GUIStyle headerStyle = new GUIStyle(EditorStyles.foldout) { fontStyle = FontStyle.Bold };
            headerStyle.padding.right += 2;
            if (VNConversationNodeDrawer.UseTint) headerStyle.normal.textColor = new Color(0.6f, 0.2f, 0.4f);

            property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, $"Interact {index}", true, headerStyle);

            if (GUI.Button(clearBtnRect, "Clear", EditorStyles.miniButtonLeft))
            {
                VNCacheHelper.CacheElement(property);
                ResetInteract(property);
                GUIUtility.keyboardControl = 0;
            }

            GUI.enabled = VNCacheHelper.ElementCache.ContainsKey(property.propertyPath);
            if (GUI.Button(recallBtnRect, "Recall", EditorStyles.miniButtonRight))
            {
                VNCacheHelper.RecallElement(property);
                GUIUtility.keyboardControl = 0;
            }
            GUI.enabled = true;

            if (property.isExpanded)
            {
                EditorGUI.indentLevel++;
                var y = position.y + EditorGUIUtility.singleLineHeight + 6f;

                EditorGUI.PropertyField(new Rect(position.x, y, position.width, EditorGUIUtility.singleLineHeight), property.FindPropertyRelative("TargetAnswerNumber"));
                y += EditorGUIUtility.singleLineHeight + 2f;

                EditorGUI.PropertyField(new Rect(position.x, y, position.width, EditorGUIUtility.singleLineHeight), property.FindPropertyRelative("ReturnValue"));
                y += EditorGUIUtility.singleLineHeight + 2f;

                EditorGUI.PropertyField(new Rect(position.x, y, position.width, EditorGUIUtility.singleLineHeight), property.FindPropertyRelative("ReturnToChoicePhase"));
                y += EditorGUIUtility.singleLineHeight + 2f;

                var changeRelProp = property.FindPropertyRelative("ChangeRelationship");
                changeRelProp.boolValue = EditorGUI.ToggleLeft(
                    new Rect(position.x, y, position.width * 0.5f, EditorGUIUtility.singleLineHeight),
                    changeRelProp.displayName, changeRelProp.boolValue);
                y += EditorGUIUtility.singleLineHeight + 2f;

                if (changeRelProp.boolValue)
                {
                    EditorGUI.indentLevel++;
                    EditorGUI.PropertyField(new Rect(position.x, y, position.width, EditorGUIUtility.singleLineHeight), property.FindPropertyRelative("RelationshipCharacter"));
                    y += EditorGUIUtility.singleLineHeight + 2f;
                    EditorGUI.PropertyField(new Rect(position.x, y, position.width, EditorGUIUtility.singleLineHeight), property.FindPropertyRelative("RelationshipDelta"));
                    y += EditorGUIUtility.singleLineHeight + 2f;
                    EditorGUI.indentLevel--;
                }

                y += 4f;

                var subConvProp = property.FindPropertyRelative("SubConversation");

                //new: auto-init null managed reference elements (from [SerializeReference] + button)
                if (subConvProp != null && subConvProp.isArray)
                {
                    bool changed = false;
                    for (int i = 0; i < subConvProp.arraySize; i++)
                    {
                        var elem = subConvProp.GetArrayElementAtIndex(i);
                        if (elem.managedReferenceValue == null)
                        {
                            elem.managedReferenceValue = new VNConversationNode();
                            changed = true;
                        }
                    }
                    if (changed) property.serializedObject.ApplyModifiedProperties();
                }

                VNConversationNodeDrawer.DrawArrayWithCache(ref y, position, subConvProp, "Sub Conversation List", "Node");
                EditorGUI.indentLevel--;
            }
            EditorGUI.EndProperty();
        }

        private void ResetInteract(SerializedProperty prop)
        {
            prop.FindPropertyRelative("TargetAnswerNumber").intValue = 0;
            prop.FindPropertyRelative("ReturnValue").floatValue = 0f;
            prop.FindPropertyRelative("ReturnToChoicePhase").boolValue = false;
            prop.FindPropertyRelative("ChangeRelationship").boolValue = false;
            prop.FindPropertyRelative("RelationshipCharacter").objectReferenceValue = null;
            prop.FindPropertyRelative("RelationshipDelta").intValue = 0;

            var subConvProp = prop.FindPropertyRelative("SubConversation");
            if (subConvProp != null) { subConvProp.ClearArray(); subConvProp.arraySize = 0; }

            prop.serializedObject.ApplyModifiedProperties();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (!property.isExpanded) return EditorGUIUtility.singleLineHeight + 4f;

            float h = EditorGUIUtility.singleLineHeight + 6f;  // header
            h += EditorGUIUtility.singleLineHeight + 2f;       // TargetAnswerNumber
            h += EditorGUIUtility.singleLineHeight + 2f;       // ReturnValue
            h += EditorGUIUtility.singleLineHeight + 2f;       // ReturnToChoicePhase
            h += EditorGUIUtility.singleLineHeight + 2f;       // ChangeRelationship

            var changeRelProp = property.FindPropertyRelative("ChangeRelationship");
            if (changeRelProp != null && changeRelProp.boolValue)
            {
                h += EditorGUIUtility.singleLineHeight + 2f;   // RelationshipCharacter
                h += EditorGUIUtility.singleLineHeight + 2f;   // RelationshipDelta
            }

            h += 4f;                                            // spacing before SubConversation
            h += VNConversationNodeDrawer.GetArrayWithCacheHeight(property.FindPropertyRelative("SubConversation"));
            return h + 4f;
        }

        private void CheckAndClearIfNewlyAdded(SerializedProperty property)
        {
            // SubConversation ใช้ [SerializeReference] — เมื่อ Unity duplicate element
            // managed reference IDs จะถูก copy ทำให้ทุก element ชี้ object เดียวกัน
            // ตรวจโดยเปรียบ managedReferenceId กับ element ก่อนหน้า

            var subConv = property.FindPropertyRelative("SubConversation");
            if (subConv == null || !subConv.isArray || subConv.arraySize == 0) return;

            string path = property.propertyPath;
            int dotArrayData = path.LastIndexOf(".Array.data[");
            if (dotArrayData < 0) return;

            int lastBracket = path.LastIndexOf('[');
            int lastClose   = path.LastIndexOf(']');
            if (!int.TryParse(path[(lastBracket + 1)..lastClose], out int myIndex)) return;
            if (myIndex == 0) return;  // element แรกไม่มี predecessor

            // เก็บ IDs ของ SubConversation element นี้
            var myIds = new HashSet<long>();
            for (int i = 0; i < subConv.arraySize; i++)
            {
                long id = subConv.GetArrayElementAtIndex(i).managedReferenceId;
                if (id >= 0) myIds.Add(id);
            }
            if (myIds.Count == 0) return;

            // ดึง element ก่อนหน้าและเช็คว่า SubConversation มี ID ซ้ำกันไหม
            string prevPath = path[..dotArrayData] + ".Array.data[" + (myIndex - 1) + "]";
            var prevElem    = property.serializedObject.FindProperty(prevPath);
            if (prevElem == null) return;

            var prevSubConv = prevElem.FindPropertyRelative("SubConversation");
            if (prevSubConv == null || !prevSubConv.isArray) return;

            for (int i = 0; i < prevSubConv.arraySize; i++)
            {
                long id = prevSubConv.GetArrayElementAtIndex(i).managedReferenceId;
                if (id >= 0 && myIds.Contains(id))
                {
                    ResetInteract(property);
                    GUIUtility.ExitGUI();
                    return;
                }
            }
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
