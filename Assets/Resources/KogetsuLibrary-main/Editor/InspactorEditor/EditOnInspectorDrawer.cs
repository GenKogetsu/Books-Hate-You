using Kogetsu.Library.Attribute;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Kogetsu.Library.Editor
{
    [CustomPropertyDrawer(typeof(EditOnInspectorAttribute))]
    public class EditOnInspectorDrawer : PropertyDrawer
    {
        static readonly Color _boxBg = new(0.5f, 0.5f, 0.5f, 0.2f);
        static readonly Color _boxBorder = new(0.5f, 0.5f, 0.5f, 0.5f);
        const float _pad = 15f;
        const float _boxLeftOffset = 3f;       // ระยะขอบซ้ายของกล่อง (px) — แก้ที่นี่ที่เดียวใช้ทั้ง UI Toolkit และ IMGUI
        const float _labelWidthPercent = 40f;  // ความกว้างฝั่งชื่อ field (%) ก่อนถึงช่องลาก SO (ใช้เฉพาะ UI Toolkit)

        // เก็บสถานะ toggle / foldout ไว้ใน SessionState (อยู่รอด Enter Play Mode / Domain Reload)
        static string KeyBase(SerializedProperty p) =>
            $"EditOnInspector_{p.serializedObject.targetObject.GetType().Name}_{p.propertyPath}";
        static string RunKey(SerializedProperty p) => KeyBase(p) + "_run";
        static string FoldKey(SerializedProperty p) => KeyBase(p) + "_fold";

        System.Type GetObjType() =>
            fieldInfo != null && typeof(ScriptableObject).IsAssignableFrom(fieldInfo.FieldType)
                ? fieldInfo.FieldType
                : typeof(ScriptableObject);

        // =====================================================================
        //  UI Toolkit — ฝัง Inspector จริงของ SO
        //  Null    : ช่องลากปกติ
        //  มี SO   : [▼ ชื่อ Field] [ช่องลาก SO]   ← แถวเดียว คลี่/หดได้ แล้วกล่องเทาอยู่ข้างล่าง
        // =====================================================================
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var prop = property.Copy();
            string runKey = RunKey(prop), foldKey = FoldKey(prop);

            var root = new VisualElement();

            // --- กรณี Null: ช่องลากปกติ ---
            var plainField = new ObjectField(prop.displayName) { objectType = GetObjType(), allowSceneObjects = false };
            plainField.BindProperty(prop);
            root.Add(plainField);

            // --- กรณีมี SO: แถวหัว [▼ ชื่อ Field] [ช่องลาก SO] ---
            var header = new VisualElement();
            header.style.flexDirection = FlexDirection.Row;
            header.style.alignItems = Align.Center;
            root.Add(header);

            var foldout = new Foldout { text = prop.displayName, value = SessionState.GetBool(foldKey, true) };
            foldout.style.width = new Length(_labelWidthPercent, LengthUnit.Percent);
            foldout.style.flexShrink = 0;
            header.Add(foldout);

            var headerField = new ObjectField { objectType = GetObjType(), allowSceneObjects = false };
            headerField.style.flexGrow = 1;
            headerField.BindProperty(prop);
            header.Add(headerField);

            // --- กล่องพื้นหลังเทา รอบพื้นที่ของ SO (แสดงเมื่อคลี่) ---
            var box = new VisualElement();
            box.style.marginTop = 2;
            box.style.marginLeft = _boxLeftOffset;
            box.style.paddingTop = box.style.paddingBottom = box.style.paddingRight = _pad;
            box.style.paddingLeft = _pad;
            box.style.backgroundColor = _boxBg;
            box.style.borderTopWidth = box.style.borderBottomWidth =
                box.style.borderLeftWidth = box.style.borderRightWidth = 1;
            box.style.borderTopColor = box.style.borderBottomColor =
                box.style.borderLeftColor = box.style.borderRightColor = _boxBorder;
            box.style.borderTopLeftRadius = box.style.borderTopRightRadius =
                box.style.borderBottomLeftRadius = box.style.borderBottomRightRadius = 3;
            root.Add(box);

            // Toggle อยู่บนสุดของกล่อง (เหนือ field แรกของ SO)
            var toggle = new Toggle("Edit On Runtime") { value = SessionState.GetBool(runKey, false) };
            box.Add(toggle);

            // ที่ใส่ Inspector ของ SO (ส่วนนี้ที่ถูกล็อกตอน Play Mode ถ้าไม่ติ๊ก toggle)
            var content = new VisualElement();
            box.Add(content);

            bool hasSO = false;

            void UpdateVisibility()
            {
                plainField.style.display = hasSO ? DisplayStyle.None : DisplayStyle.Flex;
                header.style.display = hasSO ? DisplayStyle.Flex : DisplayStyle.None;
                box.style.display = (hasSO && foldout.value) ? DisplayStyle.Flex : DisplayStyle.None;
            }

            void ApplyState()
            {
                bool playing = EditorApplication.isPlaying;
                content.SetEnabled(!playing || toggle.value); // Edit Mode แก้ได้เลย / Play Mode ต้องติ๊กก่อน
            }

            void Rebuild()
            {
                content.Clear();
                var so = prop.objectReferenceValue;
                hasSO = so != null;
                if (hasSO) content.Add(new InspectorElement(so)); // หน้าเดียวกับตอนคลิก SO ใน Project
                UpdateVisibility();
                ApplyState();
            }

            toggle.RegisterValueChangedCallback(e =>
            {
                if (e.target != toggle) return;
                SessionState.SetBool(runKey, e.newValue);
                ApplyState();
            });
            foldout.RegisterValueChangedCallback(e =>
            {
                if (e.target != foldout) return;
                SessionState.SetBool(foldKey, e.newValue);
                UpdateVisibility();
            });

            root.TrackPropertyValue(prop, _ => Rebuild());
            root.schedule.Execute(ApplyState).Every(200); // ตามสถานะ Play/Stop

            Rebuild();
            return root;
        }

        // =====================================================================
        //  IMGUI fallback (เมื่อ Inspector ของ host ถูกวาดด้วย IMGUI เช่น NaughtyAttributes)
        // =====================================================================
        SerializedObject _so;

        SerializedObject GetSO(Object target)
        {
            if (_so == null || _so.targetObject != target)
            {
                _so?.Dispose();
                _so = new SerializedObject(target);
            }
            _so.Update();
            return _so;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float line = EditorGUIUtility.singleLineHeight;
            float sp = EditorGUIUtility.standardVerticalSpacing;
            float h = line;

            var obj = property.objectReferenceValue;
            if (obj == null) return h;

            if (SessionState.GetBool(FoldKey(property), true))
            {
                h += sp + _pad * 2 + line; // กล่อง + toggle Edit On Runtime
                var it = GetSO(obj).GetIterator();
                bool enter = true;
                while (it.NextVisible(enter))
                {
                    enter = false;
                    if (it.propertyPath == "m_Script") continue;
                    h += sp + EditorGUI.GetPropertyHeight(it, true);
                }
            }
            return h;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            float line = EditorGUIUtility.singleLineHeight;
            float sp = EditorGUIUtility.standardVerticalSpacing;
            bool playing = EditorApplication.isPlaying;

            var r = new Rect(position.x, position.y, position.width, line);
            var obj = property.objectReferenceValue;

            // Null: ช่องลากปกติ
            if (obj == null)
            {
                EditorGUI.ObjectField(r, property, GetObjType(), label);
                return;
            }

            string runKey = RunKey(property), foldKey = FoldKey(property);

            // แถวหัว: [▼ ชื่อ Field] [ช่องลาก SO]
            float lw = EditorGUIUtility.labelWidth;
            bool fold = SessionState.GetBool(foldKey, true);
            bool newFold = EditorGUI.Foldout(new Rect(r.x, r.y, lw, line), fold, label, true);
            if (newFold != fold) SessionState.SetBool(foldKey, newFold);
            EditorGUI.ObjectField(new Rect(r.x + lw, r.y, r.width - lw, line), property, GetObjType(), GUIContent.none);
            if (!fold) return;

            // กล่องพื้นหลัง
            var boxRect = new Rect(position.x + _boxLeftOffset, r.yMax + sp,
                                   position.width - _boxLeftOffset, position.height - line - sp);
            EditorGUI.DrawRect(boxRect, _boxBg);
            EditorGUI.DrawRect(new Rect(boxRect.x, boxRect.y, boxRect.width, 1), _boxBorder);
            EditorGUI.DrawRect(new Rect(boxRect.x, boxRect.yMax - 1, boxRect.width, 1), _boxBorder);
            EditorGUI.DrawRect(new Rect(boxRect.x, boxRect.y, 1, boxRect.height), _boxBorder);
            EditorGUI.DrawRect(new Rect(boxRect.xMax - 1, boxRect.y, 1, boxRect.height), _boxBorder);

            float x = boxRect.x + _pad, w = boxRect.width - _pad * 2, y = boxRect.y + _pad;

            // Toggle บนสุดของ field แรก
            bool run = SessionState.GetBool(runKey, false);
            bool newRun = EditorGUI.ToggleLeft(new Rect(x, y, w, line), "Edit On Runtime", run);
            if (newRun != run) SessionState.SetBool(runKey, newRun);
            y += line;

            var so = GetSO(obj);
            using (new EditorGUI.DisabledScope(playing && !newRun))
            {
                EditorGUI.BeginChangeCheck();
                var it = so.GetIterator();
                bool enter = true;
                while (it.NextVisible(enter))
                {
                    enter = false;
                    if (it.propertyPath == "m_Script") continue;
                    y += sp;
                    float h = EditorGUI.GetPropertyHeight(it, true);
                    EditorGUI.PropertyField(new Rect(x, y, w, h), it, true);
                    y += h;
                }
                if (EditorGUI.EndChangeCheck()) so.ApplyModifiedProperties();
            }
        }
    }
}