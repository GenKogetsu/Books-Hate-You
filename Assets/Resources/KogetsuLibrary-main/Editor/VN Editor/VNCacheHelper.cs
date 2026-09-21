#if UNITY_EDITOR
using UnityEditor;
using System.Collections.Generic;

namespace Kogetsu.Library.Editor
{
    public static class VNCacheHelper
    {
        public static readonly Dictionary<string, string> NodeCache = new();
        public static readonly Dictionary<string, string> TextCache = new();
        public static readonly Dictionary<string, bool> UseDialogueTextCache = new();

        public struct SerializedElementData
        {
            public SerializedPropertyType type;
            public int intValue;
            public float floatValue;
            public string stringValue;
            public bool boolValue;
            public Color colorValue;
            public Object objectReferenceValue;
            public int enumValueIndex;
        }

        public static readonly Dictionary<string, List<Dictionary<string, SerializedElementData>>> ArrayCache = new();

        // NEW: ระบบ Cache สำหรับเก็บค่า Object ย่อยๆ (Action, Speaker, Phase, ฯลฯ)
        public static readonly Dictionary<string, Dictionary<string, SerializedElementData>> ElementCache = new();

        public static void CacheElement(SerializedProperty prop)
        {
            if (prop == null) return;
            string path = prop.propertyPath;
            var elementData = new Dictionary<string, SerializedElementData>();

            var end = prop.GetEndProperty();
            var child = prop.Copy();
            if (child.NextVisible(true))
            {
                do
                {
                    if (SerializedProperty.EqualContents(child, end)) break;
                    var data = new SerializedElementData { type = child.propertyType };
                    switch (child.propertyType)
                    {
                        case SerializedPropertyType.Integer: data.intValue = child.intValue; break;
                        case SerializedPropertyType.Float: data.floatValue = child.floatValue; break;
                        case SerializedPropertyType.String: data.stringValue = child.stringValue; break;
                        case SerializedPropertyType.Boolean: data.boolValue = child.boolValue; break;
                        case SerializedPropertyType.Color: data.colorValue = child.colorValue; break;
                        case SerializedPropertyType.ObjectReference: data.objectReferenceValue = child.objectReferenceValue; break;
                        case SerializedPropertyType.Enum: data.enumValueIndex = child.enumValueIndex; break;
                    }
                    elementData[child.propertyPath.Substring(prop.propertyPath.Length)] = data;
                } while (child.NextVisible(false));
            }
            ElementCache[path] = elementData;
        }

        public static void RecallElement(SerializedProperty prop)
        {
            if (prop == null) return;
            string path = prop.propertyPath;
            if (!ElementCache.TryGetValue(path, out var elementData)) return;

            foreach (var kvp in elementData)
            {
                var child = prop.serializedObject.FindProperty(prop.propertyPath + kvp.Key);
                if (child != null)
                {
                    var data = kvp.Value;
                    switch (data.type)
                    {
                        case SerializedPropertyType.Integer: child.intValue = data.intValue; break;
                        case SerializedPropertyType.Float: child.floatValue = data.floatValue; break;
                        case SerializedPropertyType.String: child.stringValue = data.stringValue; break;
                        case SerializedPropertyType.Boolean: child.boolValue = data.boolValue; break;
                        case SerializedPropertyType.Color: child.colorValue = data.colorValue; break;
                        case SerializedPropertyType.ObjectReference: child.objectReferenceValue = data.objectReferenceValue; break;
                        case SerializedPropertyType.Enum: child.enumValueIndex = data.enumValueIndex; break;
                    }
                }
            }
            prop.serializedObject.ApplyModifiedProperties();
        }

        public static void CacheArray(SerializedProperty arrayProp)
        {
            if (arrayProp == null || !arrayProp.isArray) return;
            string path = arrayProp.propertyPath;

            var cachedElements = new List<Dictionary<string, SerializedElementData>>();

            for (int i = 0; i < arrayProp.arraySize; i++)
            {
                var element = arrayProp.GetArrayElementAtIndex(i);
                var elementData = new Dictionary<string, SerializedElementData>();

                var end = element.GetEndProperty();
                var child = element.Copy();

                if (child.NextVisible(true))
                {
                    do
                    {
                        if (SerializedProperty.EqualContents(child, end)) break;

                        var data = new SerializedElementData { type = child.propertyType };

                        switch (child.propertyType)
                        {
                            case SerializedPropertyType.Integer: data.intValue = child.intValue; break;
                            case SerializedPropertyType.Float: data.floatValue = child.floatValue; break;
                            case SerializedPropertyType.String: data.stringValue = child.stringValue; break;
                            case SerializedPropertyType.Boolean: data.boolValue = child.boolValue; break;
                            case SerializedPropertyType.Color: data.colorValue = child.colorValue; break;
                            case SerializedPropertyType.ObjectReference: data.objectReferenceValue = child.objectReferenceValue; break;
                            case SerializedPropertyType.Enum: data.enumValueIndex = child.enumValueIndex; break;
                        }

                        elementData[child.propertyPath.Substring(element.propertyPath.Length)] = data;

                    } while (child.NextVisible(false));
                }

                cachedElements.Add(elementData);
            }

            ArrayCache[path] = cachedElements;
        }

        public static void RecallArray(SerializedProperty arrayProp)
        {
            if (arrayProp == null || !arrayProp.isArray) return;
            string path = arrayProp.propertyPath;

            if (!ArrayCache.TryGetValue(path, out var cachedElements)) return;

            arrayProp.ClearArray();
            arrayProp.arraySize = cachedElements.Count;

            for (int i = 0; i < cachedElements.Count; i++)
            {
                var element = arrayProp.GetArrayElementAtIndex(i);
                var elementData = cachedElements[i];

                foreach (var kvp in elementData)
                {
                    var child = element.serializedObject.FindProperty(element.propertyPath + kvp.Key);
                    if (child != null)
                    {
                        var data = kvp.Value;
                        switch (data.type)
                        {
                            case SerializedPropertyType.Integer: child.intValue = data.intValue; break;
                            case SerializedPropertyType.Float: child.floatValue = data.floatValue; break;
                            case SerializedPropertyType.String: child.stringValue = data.stringValue; break;
                            case SerializedPropertyType.Boolean: child.boolValue = data.boolValue; break;
                            case SerializedPropertyType.Color: child.colorValue = data.colorValue; break;
                            case SerializedPropertyType.ObjectReference: child.objectReferenceValue = data.objectReferenceValue; break;
                            case SerializedPropertyType.Enum: child.enumValueIndex = data.enumValueIndex; break;
                        }
                    }
                }
            }

            arrayProp.serializedObject.ApplyModifiedProperties();
        }

        public static void CacheNode(SerializedProperty nodeProp)
        {
            if (nodeProp == null) return;
            var targetObject = nodeProp.serializedObject.targetObject;
            string json = EditorJsonUtility.ToJson(targetObject);
            NodeCache[nodeProp.propertyPath] = json;
        }

        public static void RecallNode(SerializedProperty nodeProp)
        {
            if (nodeProp == null) return;

            if (NodeCache.TryGetValue(nodeProp.propertyPath, out string json))
            {
                var targetObject = nodeProp.serializedObject.targetObject;
                EditorJsonUtility.FromJsonOverwrite(json, targetObject);
                nodeProp.serializedObject.Update();
            }
        }
    }
}
#endif