#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;
using Kogetsu.Library.Core;

namespace Kogetsu.Library.Editor
{
    [CustomEditor(typeof(MoveController2D))]
    public class MoveController2DEditor : UnityEditor.Editor
    {
        #region Documentation - Notes
        // ─── Readonly ──────────────────────────────────────────────────
        #endregion
        private SerializedProperty _stats;
        private SerializedProperty _rb2;

        #region Documentation - Notes
        // ─── Core ──────────────────────────────────────────────────────
        #endregion
        private SerializedProperty _inputObserverChannel;
        private SerializedProperty _directionMode;
        private SerializedProperty _moveAbility;

        #region Documentation - Notes
        // ─── Perspective ───────────────────────────────────────────────
        #endregion
        private SerializedProperty _perspectiveMode;
        private SerializedProperty _perspectiveQuaternion2D;

        #region Documentation - Notes
        // ─── Roll ──────────────────────────────────────────────────────
        #endregion
        private SerializedProperty _rollAbility;

        #region Documentation - Notes
        // ─── Jump ──────────────────────────────────────────────────────
        #endregion
        private SerializedProperty _enableJump;
        private SerializedProperty _groundCheck;
        private SerializedProperty _groundRadius;
        private SerializedProperty _groundLayer;

        #region Documentation - Notes
        // ─── Flip ──────────────────────────────────────────────────────
        #endregion
        private SerializedProperty _enableFlip;
        private SerializedProperty _flipTarget;

        #region Documentation - Notes
        // ─── Debug ─────────────────────────────────────────────────────
        #endregion
        private SerializedProperty _currentDirection;
        private SerializedProperty _lastFacingDirection;
        private SerializedProperty _isGrounded;

        #region Documentation - Notes
        // ─── Foldout States ────────────────────────────────────────────
        #endregion
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

            #region Documentation - Notes
            // ── Script (readonly) ───────────────────────────────────────
            #endregion
            GUI.enabled = false;
            EditorGUILayout.ObjectField("Script", MonoScript.FromMonoBehaviour((MonoBehaviour)target), typeof(MonoScript), false);
            EditorGUILayout.PropertyField(_rb2);
            GUI.enabled = true;

            EditorGUILayout.PropertyField(_stats);

            EditorGUILayout.Space(4);

            #region Documentation - Notes
            // ── Core ───────────────────────────────────────────────────
            #endregion
            if (_inputObserverChannel.objectReferenceValue == null)
                EditorGUILayout.HelpBox("Input Observer Channel is required!", MessageType.Error);

            EditorGUILayout.PropertyField(_inputObserverChannel);
            EditorGUILayout.PropertyField(_directionMode);
            EditorGUILayout.Space(2);
            EditorGUILayout.PropertyField(_moveAbility);

            EditorGUILayout.Space(6);

            #region Documentation - Notes
            // ── Perspective ────────────────────────────────────────────
            #endregion
            DrawToggleFoldout(ref _showPerspective, _perspectiveMode, "Perspective Mode", () =>
            {
                EditorGUILayout.PropertyField(_perspectiveQuaternion2D);
            });

            EditorGUILayout.Space(4);

            #region Documentation - Notes
            // ── Jump ───────────────────────────────────────────────────
            #endregion
            DrawToggleFoldout(ref _showJump, _enableJump, "Jump", () =>
            {
                EditorGUILayout.PropertyField(_groundCheck);
                EditorGUILayout.PropertyField(_groundRadius);
                EditorGUILayout.PropertyField(_groundLayer);
            });

            EditorGUILayout.Space(4);

            #region Documentation - Notes
            // ── Flip ───────────────────────────────────────────────────
            #endregion
            DrawToggleFoldout(ref _showFlip, _enableFlip, "Flip (Scale X)", () =>
            {
                EditorGUILayout.PropertyField(_flipTarget, new GUIContent("Flip Target"));
            });

            EditorGUILayout.Space(4);

            #region Documentation - Notes
            // ── Roll / Dodge ───────────────────────────────────────────
            #endregion
            _showRoll = EditorGUILayout.Foldout(_showRoll, "Roll / Dodge", true, EditorStyles.foldoutHeader);
            if (_showRoll)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(_rollAbility);
                EditorGUI.indentLevel--;
            }


            EditorGUILayout.Space(4);

            #region Documentation - Notes
            // ── Debug ──────────────────────────────────────────────────
            #endregion
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

        #region Documentation - Notes
        // ── Helper: Toggle + Foldout (แบบเดียวกับ Perspective Mode) ────
        #endregion
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
            #region Documentation - Notes
            // Use Toggle directly — PropertyField renders a label even with GUIContent.none
            #endregion
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
