#if UNITY_EDITOR
using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Kogetsu.Library.Attribute;

namespace Kogetsu.Library.Editor
{
    [CustomPropertyDrawer(typeof(MoveAbilitySelector))]
    public class MoveAbilityDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.ManagedReference)
            {
                EditorGUI.LabelField(position, label.text, "Use [MoveAbilitySelector] with [SerializeReference] only.");
                return;
            }

            EditorGUI.BeginProperty(position, label, property);

            var foldoutRect = new Rect(position.x, position.y, EditorGUIUtility.labelWidth, EditorGUIUtility.singleLineHeight);
            property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, label, true);

            var dropdownRect = new Rect(position.x + EditorGUIUtility.labelWidth, position.y,
                position.width - EditorGUIUtility.labelWidth, EditorGUIUtility.singleLineHeight);

            string fullTypeName = property.managedReferenceFullTypename;
            string displayName = string.IsNullOrEmpty(fullTypeName) ? "None (Null)" : fullTypeName.Split('.').Last();

            if (EditorGUI.DropdownButton(dropdownRect, new GUIContent(displayName), FocusType.Keyboard, EditorStyles.popup))
                ShowTypeMenu(property);

            if (property.isExpanded && !string.IsNullOrEmpty(fullTypeName))
            {
                EditorGUI.indentLevel++;
                float y = position.y + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

                SerializedProperty child = property.Copy();
                SerializedProperty end = property.GetEndProperty();
                bool hasNext = child.NextVisible(true);
                bool hasFields = false;

                while (hasNext && !SerializedProperty.EqualContents(child, end))
                {
                    hasFields = true;
                    float h = EditorGUI.GetPropertyHeight(child, true);
                    EditorGUI.PropertyField(new Rect(position.x, y, position.width, h), child, true);
                    y += h + EditorGUIUtility.standardVerticalSpacing;
                    hasNext = child.NextVisible(false);
                }

                if (!hasFields)
                    EditorGUI.LabelField(new Rect(position.x, y, position.width, EditorGUIUtility.singleLineHeight), "No Settings", EditorStyles.miniLabel);

                EditorGUI.indentLevel--;
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float total = EditorGUIUtility.singleLineHeight;

            if (!property.isExpanded || string.IsNullOrEmpty(property.managedReferenceFullTypename))
                return total;

            SerializedProperty child = property.Copy();
            SerializedProperty end = property.GetEndProperty();
            bool hasNext = child.NextVisible(true);
            bool hasFields = false;

            while (hasNext && !SerializedProperty.EqualContents(child, end))
            {
                hasFields = true;
                total += EditorGUI.GetPropertyHeight(child, true) + EditorGUIUtility.standardVerticalSpacing;
                hasNext = child.NextVisible(false);
            }

            if (!hasFields)
                total += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            return total;
        }

        private void ShowTypeMenu(SerializedProperty property)
        {
            var menu = new GenericMenu();
            Type baseType = GetBaseType(property);

            menu.AddItem(new GUIContent("None"), string.IsNullOrEmpty(property.managedReferenceFullTypename), () =>
            {
                property.managedReferenceValue = null;
                property.serializedObject.ApplyModifiedProperties();
            });

            if (baseType == null) { menu.ShowAsContext(); return; }

            menu.AddSeparator("");

            foreach (var type in TypeCache.GetTypesDerivedFrom(baseType).Where(t => !t.IsAbstract && !t.IsInterface))
            {
                var captured = type;
                menu.AddItem(new GUIContent(captured.Name), false, () =>
                {
                    property.managedReferenceValue = Activator.CreateInstance(captured);
                    property.isExpanded = true;
                    property.serializedObject.ApplyModifiedProperties();
                });
            }

            menu.ShowAsContext();
        }

        private static Type GetBaseType(SerializedProperty property)
        {
            string typeName = property.managedReferenceFieldTypename;
            if (string.IsNullOrEmpty(typeName)) return null;

            string[] parts = typeName.Split(' ');
            if (parts.Length != 2) return null;

            var assembly = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(a => a.GetName().Name == parts[0]);

            return assembly?.GetType(parts[1]);
        }
    }
}
#endif
