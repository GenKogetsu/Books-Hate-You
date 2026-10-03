using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Kogetsu.Library.Editor
{
    /// <summary>
    /// ทุก field ที่เป็น ScriptableObject (ใน MonoBehaviour ของโปรเจค) จะฝัง Inspector ของ SO อัตโนมัติ
    /// มี toggle "Edit On Inspector" ถ้าไม่ติ๊กจะดูได้อย่างเดียว
    /// </summary>
    [CustomPropertyDrawer(typeof(ScriptableObject), true)]
    public class AutoInlineSODrawer : PropertyDrawer
    {
        const string MenuPath = "Tools/Kogetsu/Inline ScriptableObject Inspector";
        const string EnabledKey = "Kogetsu.InlineSO.Enabled";

        static bool Enabled => EditorPrefs.GetBool(EnabledKey, true);

        [MenuItem(MenuPath)]
        static void ToggleEnabled() => EditorPrefs.SetBool(EnabledKey, !Enabled); // เลือก object ใหม่เพื่อ refresh Inspector

        [MenuItem(MenuPath, true)]
        static bool ToggleEnabledValidate() { Menu.SetChecked(MenuPath, Enabled); return true; }

        bool ShouldInline(SerializedProperty p)
        {
            if (!Enabled) return false;
            if (p.propertyType != SerializedPropertyType.ObjectReference) return false;

            var host = p.serializedObject.targetObject;
            if (host == null) return false;
            if (host is ScriptableObject) return false;                          // SO ซ้อน SO: ไม่ฝัง (กัน reference วนลูป)
            if (host.GetType().Assembly.GetName().Name.StartsWith("Unity")) return false; // component ของ Unity/package เอง
            if (p.propertyPath.Contains(".Array.data[")) return false;           // สมาชิกใน Array/List: ไม่ฝัง
            if (fieldInfo != null && fieldInfo.IsDefined(typeof(NoInlineSOAttribute), true)) return false;
            return true;
        }

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var prop = property.Copy();
            var root = new VisualElement();

            var objType = fieldInfo != null && typeof(ScriptableObject).IsAssignableFrom(fieldInfo.FieldType)
                ? fieldInfo.FieldType
                : typeof(ScriptableObject);

            var objField = new ObjectField(prop.displayName) { objectType = objType, allowSceneObjects = false };
            objField.BindProperty(prop);
            root.Add(objField);

            if (!ShouldInline(prop)) return root;

            string key = $"InlineSO_{prop.serializedObject.targetObject.GetType().Name}_{prop.propertyPath}";
            var toggle = new Toggle("Edit On Inspector") { value = SessionState.GetBool(key, false) };
            root.Add(toggle);

            var box = new VisualElement();
            box.style.marginLeft = 15;
            box.style.paddingLeft = 4;
            box.style.borderLeftWidth = 2;
            box.style.borderLeftColor = new Color(0.4f, 0.4f, 0.4f, 0.8f);
            root.Add(box);

            void Rebuild()
            {
                box.Clear();
                var so = prop.objectReferenceValue;
                toggle.style.display = so ? DisplayStyle.Flex : DisplayStyle.None;
                if (so == null) return;
                box.Add(new InspectorElement(so));
                box.SetEnabled(toggle.value);
            }

            toggle.RegisterValueChangedCallback(e =>
            {
                SessionState.SetBool(key, e.newValue);
                box.SetEnabled(e.newValue);
            });

            root.TrackPropertyValue(prop, _ => Rebuild());
            Rebuild();
            return root;
        }
    }
}
