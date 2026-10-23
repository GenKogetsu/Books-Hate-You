#if UNITY_EDITOR
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Kogetsu.Library.Core;

namespace Kogetsu.Library.Editor
{
    [CustomEditor(typeof(VNChoicePanel))]
    public class VNChoicePanelEditor : UnityEditor.Editor
    {
        #region Documentation - Notes
        // ────────────────────────────── Inspector ──────────────────────────────
        #endregion

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawDefaultInspector();

            EditorGUILayout.Space(12);
            DrawSectionLabel("Setup Status");
            DrawStatus("Button Container", serializedObject.FindProperty("_buttonContainer").objectReferenceValue != null);
            DrawStatus("Button Prefab",    serializedObject.FindProperty("_buttonPrefab").objectReferenceValue    != null);

            EditorGUILayout.Space(10);
            DrawSectionLabel("Quick Setup");

            var panel = (VNChoicePanel)target;

            GUI.backgroundColor = new Color(0.65f, 0.9f, 1f);
            if (GUILayout.Button("▶  Add ButtonContainer child", GUILayout.Height(30)))
                AddButtonContainer(panel);

            GUI.backgroundColor = new Color(0.75f, 1f, 0.7f);
            if (GUILayout.Button("▶  Generate VNChoiceButton in Scene  (then save as Prefab)", GUILayout.Height(30)))
                CreateChoiceButtonInScene();

            GUI.backgroundColor = Color.white;

            serializedObject.ApplyModifiedProperties();
        }

        #region Documentation - Notes
        // ─────────────────────────── Inspector helpers ───────────────────────────
        #endregion

        private static void DrawSectionLabel(string text)
        {
            EditorGUILayout.LabelField(text, EditorStyles.boldLabel);
        }

        private static void DrawStatus(string label, bool ok)
        {
            var style = new GUIStyle(EditorStyles.label)
            {
                normal  = { textColor = ok ? new Color(0.2f, 0.75f, 0.2f) : new Color(0.9f, 0.2f, 0.2f) },
                fontStyle = FontStyle.Bold
            };

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(ok ? "✓" : "✗", style, GUILayout.Width(20));
            EditorGUILayout.LabelField(label);
            EditorGUILayout.EndHorizontal();
        }

        #region Documentation - Notes
        // ─────────────────────────── Panel helper ──────────────────────────────
        #endregion

        private void AddButtonContainer(VNChoicePanel panel)
        {
            var go = new GameObject("ButtonContainer");
            Undo.RegisterCreatedObjectUndo(go, "Create ButtonContainer");
            go.transform.SetParent(panel.transform, false);

            var rt  = go.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(20, 20);
            rt.offsetMax = new Vector2(-20, -20);

            var vlg = go.AddComponent<VerticalLayoutGroup>();
            vlg.childForceExpandWidth  = true;
            vlg.childForceExpandHeight = false;
            vlg.spacing     = 10f;
            vlg.padding     = new RectOffset(0, 0, 0, 0);
            vlg.childAlignment = TextAnchor.UpperCenter;

            var csf = go.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var prop = serializedObject.FindProperty("_buttonContainer");
            prop.objectReferenceValue = rt;
            serializedObject.ApplyModifiedProperties();

            Selection.activeGameObject = go;
            Debug.Log("<b><color=#90CAF9>[VNChoicePanel]</color></b> ButtonContainer added. Now generate a VNChoiceButton, save it as a Prefab, then assign it to _buttonPrefab.");
        }

        #region Documentation - Notes
        // ──────────────────────── MenuItem : Button Prefab ────────────────────────
        #endregion

        #region Documentation - XML
        /// <summary>
        /// <para> Summary : </para>
        /// <para> (TH) : สร้าง VNChoiceButton ใน Scene (ผู้ใช้ save เป็น Prefab เอง) Structure : [Button + Image + VNChoiceButton] → Label [TMP] </para>
        /// <para> (EN) : Creates a VNChoiceButton scene object with Button, Image and TMP label; the user can save it as a prefab. </para>
        /// </summary>
        #endregion
        [MenuItem("GameObject/KogetsuLibrary/VN Choice Button (Prefab Source)", false, 12)]
        public static void CreateChoiceButtonInScene()
        {
            #region Documentation - Notes
            // ─── Root ───────────────────────────────────────────────────
            #endregion
            var root   = new GameObject("VNChoiceButton");
            var rootRT = root.AddComponent<RectTransform>();
            rootRT.sizeDelta = new Vector2(500f, 65f);

            #region Documentation - Notes
            // Image = พื้นหลัง
            #endregion
            var bg = root.AddComponent<Image>();
            bg.color = new Color(0.12f, 0.12f, 0.18f, 0.95f);

            #region Documentation - Notes
            // Button component
            #endregion
            var btn    = root.AddComponent<Button>();
            btn.targetGraphic = bg;
            var colors = btn.colors;
            colors.normalColor      = new Color(0.12f, 0.12f, 0.18f, 0.95f);
            colors.highlightedColor = new Color(0.28f, 0.48f, 0.78f, 1.00f);
            colors.pressedColor     = new Color(0.18f, 0.32f, 0.58f, 1.00f);
            colors.selectedColor    = new Color(0.22f, 0.42f, 0.70f, 1.00f);
            btn.colors = colors;

            #region Documentation - Notes
            // LayoutElement สำหรับ VerticalLayoutGroup
            #endregion
            var le = root.AddComponent<LayoutElement>();
            le.minHeight      = 65f;
            le.preferredHeight = 65f;

            #region Documentation - Notes
            // VNChoiceButton script
            #endregion
            var choiceBtn = root.AddComponent<VNChoiceButton>();

            #region Documentation - Notes
            // ─── Label ─────────────────────────────────────────────────
            #endregion
            var labelGo = new GameObject("Label");
            labelGo.transform.SetParent(root.transform, false);

            var labelRT = labelGo.AddComponent<RectTransform>();
            labelRT.anchorMin = Vector2.zero;
            labelRT.anchorMax = Vector2.one;
            labelRT.offsetMin = new Vector2(20f, 6f);
            labelRT.offsetMax = new Vector2(-20f, -6f);

            var tmp = labelGo.AddComponent<TextMeshProUGUI>();
            tmp.text      = "Choice Text";
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontSize  = 20;
            tmp.color     = Color.white;

            #region Documentation - Notes
            // ─── Wire references ───────────────────────────────────────
            #endregion
            var so = new SerializedObject(choiceBtn);
            so.FindProperty("_button").objectReferenceValue = btn;
            so.FindProperty("_label").objectReferenceValue  = tmp;
            so.ApplyModifiedProperties();

            #region Documentation - Notes
            // ─── Place under Canvas ────────────────────────────────────
            #endregion
#pragma warning disable CS0618
            var canvas = FindObjectOfType<Canvas>();
#pragma warning restore CS0618
            if (canvas != null) root.transform.SetParent(canvas.transform, false);

            Undo.RegisterCreatedObjectUndo(root, "Create VNChoiceButton");
            Selection.activeGameObject = root;

            Debug.Log("<b><color=#A5D6A7>[VNChoicePanel]</color></b> VNChoiceButton created. " +
                      "Drag it to the Project panel to save as a Prefab, " +
                      "then assign it to VNChoicePanel → _buttonPrefab.");
        }

        #region Documentation - Notes
        // ──────────────────────── MenuItem : Panel ────────────────────────────
        #endregion

        #region Documentation - XML
        /// <summary>
        /// <para> Summary : </para>
        /// <para> (TH) : สร้าง VNChoicePanel ใน Scene พร้อม ButtonContainer ลูก </para>
        /// <para> (EN) : Creates a VNChoicePanel scene object with a child ButtonContainer. </para>
        /// </summary>
        #endregion
        [MenuItem("GameObject/KogetsuLibrary/VN Choice Panel", false, 11)]
        public static void CreateChoicePanelInScene()
        {
#pragma warning disable CS0618
            var canvas = FindObjectOfType<Canvas>();
#pragma warning restore CS0618

            #region Documentation - Notes
            // ─── Panel root ────────────────────────────────────────────
            #endregion
            var panelGo = new GameObject("VNChoicePanel");
            if (canvas != null) panelGo.transform.SetParent(canvas.transform, false);

            var panelRT = panelGo.AddComponent<RectTransform>();
            panelRT.anchorMin = new Vector2(0.1f, 0.05f);
            panelRT.anchorMax = new Vector2(0.9f, 0.50f);
            panelRT.offsetMin = panelRT.offsetMax = Vector2.zero;

            var panelBg = panelGo.AddComponent<Image>();
            panelBg.color = new Color(0.05f, 0.05f, 0.10f, 0.88f);

            var choicePanel = panelGo.AddComponent<VNChoicePanel>();

            #region Documentation - Notes
            // ─── ButtonContainer child ─────────────────────────────────
            #endregion
            var containerGo = new GameObject("ButtonContainer");
            containerGo.transform.SetParent(panelGo.transform, false);

            var containerRT = containerGo.AddComponent<RectTransform>();
            containerRT.anchorMin = Vector2.zero;
            containerRT.anchorMax = Vector2.one;
            containerRT.offsetMin = new Vector2(30, 20);
            containerRT.offsetMax = new Vector2(-30, -20);

            var vlg = containerGo.AddComponent<VerticalLayoutGroup>();
            vlg.childForceExpandWidth  = true;
            vlg.childForceExpandHeight = false;
            vlg.spacing     = 10f;
            vlg.padding     = new RectOffset(0, 0, 0, 0);
            vlg.childAlignment = TextAnchor.UpperCenter;

            var csf = containerGo.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            #region Documentation - Notes
            // ─── Wire references ───────────────────────────────────────
            #endregion
            var so = new SerializedObject(choicePanel);
            so.FindProperty("_buttonContainer").objectReferenceValue = containerRT;
            so.ApplyModifiedProperties();

            #region Documentation - Notes
            // เริ่มปิดไว้ — VNChoicePanel จะ SetActive(true) เองตอน ShowChoices()
            #endregion
            panelGo.SetActive(false);

            Undo.RegisterCreatedObjectUndo(panelGo, "Create VNChoicePanel");
            Selection.activeGameObject = panelGo;

            Debug.Log("<b><color=#A5D6A7>[VNChoicePanel]</color></b> VNChoicePanel created (hidden by default). " +
                      "Assign a VNChoiceButton Prefab to _buttonPrefab, " +
                      "then drag this panel to VNSceneReader → _choicePanel.");
        }
    }
}
#endif
