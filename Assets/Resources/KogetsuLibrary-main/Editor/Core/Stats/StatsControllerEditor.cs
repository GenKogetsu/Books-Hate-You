#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;
using Kogetsu.Library.Core;

namespace Kogetsu.Library.Editor
{
    [CustomEditor(typeof(StatsController), true)]
    public class StatsControllerEditor : UnityEditor.Editor
    {
        // ─── Stats Data ────────────────────────────────────────────────
        private SerializedProperty _statsData;
        private SerializedProperty _currentMoveSpeed;
        private SerializedProperty _currectJumpForce;

        // ─── HP ────────────────────────────────────────────────────────
        private SerializedProperty _currentHp;
        private SerializedProperty _minHp;
        private SerializedProperty _hpUIConfig;

        // ─── Stamina ───────────────────────────────────────────────────
        private SerializedProperty _currentStamina;
        private SerializedProperty _minStamina;
        private SerializedProperty _staminaUIConfig;

        // ─── Color Lerp ────────────────────────────────────────────────
        private SerializedProperty _colorLerpRoot;
        private SerializedProperty _startColor;
        private SerializedProperty _endColor;
        private SerializedProperty _lerpSpeed;
        private SerializedProperty _lerpCount;

        // ─── Foldout States ────────────────────────────────────────────
        private bool _showHpUI      = true;
        private bool _showStaminaUI = true;
        private bool _showColorLerp = true;
        private bool _showDebug     = false;

        private void OnEnable()
        {
            _statsData        = serializedObject.FindProperty("StatsData");
            _currentMoveSpeed = serializedObject.FindProperty("CurrentMoveSpeed");
            _currectJumpForce = serializedObject.FindProperty("CurrectJumpForce");

            _currentHp  = serializedObject.FindProperty("CurrentHp");
            _minHp      = serializedObject.FindProperty("MinHp");
            _hpUIConfig = serializedObject.FindProperty("_hpUIConfig");

            _currentStamina  = serializedObject.FindProperty("CurrentStamina");
            _minStamina      = serializedObject.FindProperty("MinStamina");
            _staminaUIConfig = serializedObject.FindProperty("_staminaUIConfig");

            _colorLerpRoot = serializedObject.FindProperty("colorLerpRoot");
            _startColor    = serializedObject.FindProperty("startColor");
            _endColor      = serializedObject.FindProperty("endColor");
            _lerpSpeed     = serializedObject.FindProperty("lerpSpeed");
            _lerpCount     = serializedObject.FindProperty("lerpCount");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // ── Script (readonly) ────────────────────────────────────────
            GUI.enabled = false;
            EditorGUILayout.ObjectField("Script",
                MonoScript.FromMonoBehaviour((MonoBehaviour)target),
                typeof(MonoScript), false);
            GUI.enabled = true;

            EditorGUILayout.Space(4);

            // ── Stats Data ───────────────────────────────────────────────
            if (_statsData.objectReferenceValue == null)
                EditorGUILayout.HelpBox("Stats Data is required!", MessageType.Error);

            EditorGUILayout.PropertyField(_statsData);

            if (_statsData.objectReferenceValue != null)
            {
                EditorGUI.indentLevel++;
                GUI.enabled = false;
                EditorGUILayout.PropertyField(_currentMoveSpeed, new GUIContent("Move Speed (runtime)"));
                EditorGUILayout.PropertyField(_currectJumpForce, new GUIContent("Jump Force (runtime)"));
                GUI.enabled = true;
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space(6);

            // ── HP UI ────────────────────────────────────────────────────
            DrawUIConfigFoldout(ref _showHpUI, _hpUIConfig, "HP UI");

            EditorGUILayout.Space(4);

            // ── Stamina UI ───────────────────────────────────────────────
            DrawUIConfigFoldout(ref _showStaminaUI, _staminaUIConfig, "Stamina UI");

            EditorGUILayout.Space(6);

            // ── Color Lerp ───────────────────────────────────────────────
            _showColorLerp = EditorGUILayout.Foldout(_showColorLerp, "Color Lerp (Hit Flash)", true, EditorStyles.foldoutHeader);
            if (_showColorLerp)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(_colorLerpRoot, new GUIContent("Root Transform",
                    "GetComponentsInChildren<SpriteRenderer> อัตโนมัติ\nรองรับทั้ง Spritesheet และ Cutout2D Rig"));
                EditorGUILayout.Space(2);
                EditorGUILayout.PropertyField(_startColor);
                EditorGUILayout.PropertyField(_endColor);
                EditorGUILayout.PropertyField(_lerpSpeed, new GUIContent("Speed"));
                EditorGUILayout.PropertyField(_lerpCount, new GUIContent("Count"));
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space(4);

            // ── Debug ─────────────────────────────────────────────────────
            _showDebug = EditorGUILayout.Foldout(_showDebug, "Debug", true, EditorStyles.foldoutHeader);
            if (_showDebug)
            {
                var ctrl = (StatsController)target;
                EditorGUI.indentLevel++;

                // ── HP ───────────────────────────────────────────────────
                EditorGUILayout.LabelField("HP", EditorStyles.boldLabel);
                EditorGUI.indentLevel++;

                // Max — readonly, derived from StatsData.BaseHp
                GUI.enabled = false;
                EditorGUILayout.FloatField(new GUIContent("Max", "StatsData.BaseHp — แก้ได้ที่ ScriptableObject"), ctrl.MaxHp);
                GUI.enabled = true;

                // Min — editable; OnValidate() will clamp Current in play mode
                EditorGUILayout.PropertyField(_minHp, new GUIContent("Min"));

                // Current — readonly runtime value
                GUI.enabled = false;
                EditorGUILayout.PropertyField(_currentHp, new GUIContent("Current"));
                GUI.enabled = true;

                EditorGUI.indentLevel--;

                EditorGUILayout.Space(4);
                DrawSeparator();
                EditorGUILayout.Space(4);

                // ── Stamina ──────────────────────────────────────────────
                EditorGUILayout.LabelField("Stamina", EditorStyles.boldLabel);
                EditorGUI.indentLevel++;

                GUI.enabled = false;
                EditorGUILayout.FloatField(new GUIContent("Max", "StatsData.BaseStamina — แก้ได้ที่ ScriptableObject"), ctrl.MaxStamina);
                GUI.enabled = true;

                EditorGUILayout.PropertyField(_minStamina, new GUIContent("Min"));

                GUI.enabled = false;
                EditorGUILayout.PropertyField(_currentStamina, new GUIContent("Current"));
                GUI.enabled = true;

                EditorGUI.indentLevel--;
                EditorGUI.indentLevel--;
            }

            serializedObject.ApplyModifiedProperties();
        }

        // ── Separator line ─────────────────────────────────────────────────
        private static void DrawSeparator()
        {
            Rect rect = EditorGUILayout.GetControlRect(false, 1f);
            rect.x     += EditorGUI.indentLevel * 15f;
            rect.width -= EditorGUI.indentLevel * 15f;
            EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 0.4f));
        }

        // ── Draw StatsUIConfig toggle + foldout ────────────────────────────
        private void DrawUIConfigFoldout(ref bool foldout, SerializedProperty configProp, string label)
        {
            var useUIProp = configProp.FindPropertyRelative("UseUI");
            DrawToggleFoldout(ref foldout, useUIProp, label, () =>
            {
                var modeProp = configProp.FindPropertyRelative("Mode");
                EditorGUILayout.PropertyField(modeProp, new GUIContent("Mode"));

                EditorGUILayout.Space(2);

                bool isBar = modeProp.enumValueIndex == 0; // StatsUIMode.Bar = 0
                if (isBar)
                {
                    EditorGUILayout.PropertyField(
                        configProp.FindPropertyRelative("FrontBarImage"), new GUIContent("Front Bar"));
                    EditorGUILayout.PropertyField(
                        configProp.FindPropertyRelative("BackBarImage"),  new GUIContent("Back Bar"));
                    EditorGUILayout.PropertyField(
                        configProp.FindPropertyRelative("BarLerpSpeed"),  new GUIContent("Lerp Speed"));
                }
                else
                {
                    EditorGUILayout.PropertyField(
                        configProp.FindPropertyRelative("Blocks"));
                    EditorGUILayout.PropertyField(
                        configProp.FindPropertyRelative("BlinkSlowInterval"), new GUIContent("Slow Blink (>50 %)"));
                    EditorGUILayout.PropertyField(
                        configProp.FindPropertyRelative("BlinkFastInterval"), new GUIContent("Fast Blink (≤50 %)"));
                    EditorGUILayout.PropertyField(
                        configProp.FindPropertyRelative("TintStrength"),      new GUIContent("Tint Strength"));
                }
            });
        }

        // ── Helper: toggle inline + foldout ───────────────────────────────
        private void DrawToggleFoldout(ref bool foldout, SerializedProperty toggleProp,
                                       string label, System.Action drawContent)
        {
            if (!toggleProp.boolValue)
            {
                EditorGUILayout.PropertyField(toggleProp, new GUIContent(label));
                return;
            }

            Rect rect       = EditorGUILayout.GetControlRect();
            Rect toggleRect = new(rect.x + EditorGUIUtility.labelWidth, rect.y, 20f, rect.height);

            Event e = Event.current;
            if (e.type == EventType.MouseDown && e.button == 0 && toggleRect.Contains(e.mousePosition))
            {
                toggleProp.boolValue = false;
                e.Use();
            }

            foldout = EditorGUI.Foldout(rect, foldout, label, true, EditorStyles.foldoutHeader);
            EditorGUI.Toggle(toggleRect, toggleProp.boolValue);

            if (foldout)
            {
                EditorGUI.indentLevel++;
                drawContent();
                EditorGUI.indentLevel--;
            }
        }
    }
}

#endif
