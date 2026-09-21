#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;
using Kogetsu.Library.Core;

namespace Kogetsu.Library.Editor
{
    [CustomEditor(typeof(MoveController2D))]
    public class MoveController2DEditor : UnityEditor.Editor
    {
        // ─── Readonly ──────────────────────────────────────────────────
        private SerializedProperty _stats;
        private SerializedProperty _rb2;

        // ─── Core ──────────────────────────────────────────────────────
        private SerializedProperty _inputObserverChannel;
        private SerializedProperty _directionMode;
        private SerializedProperty _moveAbility;

        // ─── Perspective ───────────────────────────────────────────────
        private SerializedProperty _perspectiveMode;
        private SerializedProperty _perspectiveQuaternion2D;

        // ─── Roll ──────────────────────────────────────────────────────
        private SerializedProperty _rollAbility;

        // ─── Jump ──────────────────────────────────────────────────────
        private SerializedProperty _enableJump;
        private SerializedProperty _groundCheck;
        private SerializedProperty _groundRadius;
        private SerializedProperty _groundLayer;

        // ─── Flip ──────────────────────────────────────────────────────
        private SerializedProperty _enableFlip;
        private SerializedProperty _flipTarget;

        // ─── Debug ─────────────────────────────────────────────────────
        private SerializedProperty _currentDirection;
        private SerializedProperty _lastFacingDirection;
        private SerializedProperty _isGrounded;

        // ─── Foldout States ────────────────────────────────────────────
        private bool _showPerspective = true;
        private bool _showRoll        = false;
        private bool _showJump        = true;
        private bool _showFlip        = true;
        private bool _showDebug       = false;

        private void OnEnable()
        {
            _stats                 = serializedObject.FindProperty("Stats");
            _rb2                   = serializedObject.FindProperty("Rb2");
            _inputObserverChannel  = serializedObject.FindProperty("InputObserverChannel");
            _directionMode         = serializedObject.FindProperty("DirectionMode");
            _moveAbility           = serializedObject.FindProperty("MoveAbility");
            _perspectiveMode       = serializedObject.FindProperty("PerspectiveMode");
            _perspectiveQuaternion2D = serializedObject.FindProperty("PerspectiveQuaternion2D");
            _rollAbility           = serializedObject.FindProperty("_rollAbility");
            _enableJump            = serializedObject.FindProperty("_enableJump");
            _groundCheck           = serializedObject.FindProperty("_groundCheck");
            _groundRadius          = serializedObject.FindProperty("_groundRadius");
            _groundLayer           = serializedObject.FindProperty("_groundLayer");
            _enableFlip            = serializedObject.FindProperty("_enableFlip");
            _flipTarget            = serializedObject.FindProperty("_flipTarget");
            _currentDirection      = serializedObject.FindProperty("CurrentDirection");
            _lastFacingDirection   = serializedObject.FindProperty("LastFacingDirection");
            _isGrounded            = serializedObject.FindProperty("IsGrounded");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // ── Script (readonly) ───────────────────────────────────────
            GUI.enabled = false;
            EditorGUILayout.ObjectField("Script", MonoScript.FromMonoBehaviour((MonoBehaviour)target), typeof(MonoScript), false);
            EditorGUILayout.PropertyField(_rb2);
            GUI.enabled = true;

            EditorGUILayout.PropertyField(_stats);

            EditorGUILayout.Space(4);

            // ── Core ───────────────────────────────────────────────────
            if (_inputObserverChannel.objectReferenceValue == null)
                EditorGUILayout.HelpBox("Input Observer Channel is required!", MessageType.Error);

            EditorGUILayout.PropertyField(_inputObserverChannel);
            EditorGUILayout.PropertyField(_directionMode);
            EditorGUILayout.Space(2);
            EditorGUILayout.PropertyField(_moveAbility);

            EditorGUILayout.Space(6);

            // ── Perspective ────────────────────────────────────────────
            DrawToggleFoldout(ref _showPerspective, _perspectiveMode, "Perspective Mode", () =>
            {
                EditorGUILayout.PropertyField(_perspectiveQuaternion2D);
            });

            EditorGUILayout.Space(4);

            // ── Jump ───────────────────────────────────────────────────
            DrawToggleFoldout(ref _showJump, _enableJump, "Jump", () =>
            {
                EditorGUILayout.PropertyField(_groundCheck);
                EditorGUILayout.PropertyField(_groundRadius);
                EditorGUILayout.PropertyField(_groundLayer);
            });

            EditorGUILayout.Space(4);

            // ── Flip ───────────────────────────────────────────────────
            DrawToggleFoldout(ref _showFlip, _enableFlip, "Flip (Scale X)", () =>
            {
                EditorGUILayout.PropertyField(_flipTarget, new GUIContent("Flip Target"));
            });

            EditorGUILayout.Space(4);

            // ── Roll / Dodge ───────────────────────────────────────────
            _showRoll = EditorGUILayout.Foldout(_showRoll, "Roll / Dodge", true, EditorStyles.foldoutHeader);
            if (_showRoll)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(_rollAbility);
                EditorGUI.indentLevel--;
            }


            EditorGUILayout.Space(4);

            // ── Debug ──────────────────────────────────────────────────
            _showDebug = EditorGUILayout.Foldout(_showDebug, "Debug", true, EditorStyles.foldoutHeader);
            if (_showDebug)
            {
                EditorGUI.indentLevel++;
                GUI.enabled = false;
                EditorGUILayout.PropertyField(_currentDirection);
                EditorGUILayout.PropertyField(_lastFacingDirection);
                EditorGUILayout.PropertyField(_isGrounded);
                GUI.enabled = true;
                EditorGUI.indentLevel--;
            }

            serializedObject.ApplyModifiedProperties();
        }

        // ── Helper: Toggle + Foldout (แบบเดียวกับ Perspective Mode) ────
        private void DrawToggleFoldout(ref bool foldout, SerializedProperty toggleProp, string label, System.Action drawContent)
        {
            if (!toggleProp.boolValue)
            {
                EditorGUILayout.PropertyField(toggleProp, new GUIContent(label));
                return;
            }

            Rect rect      = EditorGUILayout.GetControlRect();
            Rect toggleRect = new(rect.x + EditorGUIUtility.labelWidth, rect.y, 20f, rect.height);

            Event e = Event.current;
            if (e.type == EventType.MouseDown && e.button == 0 && toggleRect.Contains(e.mousePosition))
            {
                toggleProp.boolValue = false;
                e.Use();
            }

            foldout = EditorGUI.Foldout(rect, foldout, label, true, EditorStyles.foldoutHeader);
            // Use Toggle directly — PropertyField renders a label even with GUIContent.none
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
