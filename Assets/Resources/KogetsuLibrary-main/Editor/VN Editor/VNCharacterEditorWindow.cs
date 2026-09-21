#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using Kogetsu.Library.Core;

namespace Kogetsu.Library.Editor
{
    public class VNCharacterEditorWindow : EditorWindow
    {
        private VNCharacterSO _currentCharacter;
        private SerializedObject _serializedCharacter;
        private SerializedProperty _emotionsProp;
        private SerializedProperty _animationsProp;
        private SerializedProperty _soundEffectsProp;

        private Vector2 _scrollPosition;

        [MenuItem("Window/KogetsuLibrary/VN Character Editor")]
        public static void ShowWindow()
        {
            var window = GetWindow<VNCharacterEditorWindow>("VN Character Editor");
            window.minSize = new Vector2(600, 400);
            window.CheckCurrentSelection();
            window.Show();
        }

        public static void OpenCharacter(VNCharacterSO character)
        {
            var window = GetWindow<VNCharacterEditorWindow>("VN Character Editor");
            window.minSize = new Vector2(600, 400);
            window.LoadCharacter(character);
            window.Show();
        }

        private void LoadCharacter(VNCharacterSO character)
        {
            if (character == null) return;

            _currentCharacter = character;
            _serializedCharacter = new SerializedObject(_currentCharacter);

            // ดึงข้อมูล List ทั้ง 3 ตัวมาเตรียมวาด (ชื่อตัวแปรต้องตรงกับใน VNCharacterSO.cs)
            _emotionsProp = _serializedCharacter.FindProperty("Emotions");
            _animationsProp = _serializedCharacter.FindProperty("Animations");
            _soundEffectsProp = _serializedCharacter.FindProperty("SoundEffects");
            Repaint();
        }

        private void OnEnable()
        {
            Selection.selectionChanged += CheckCurrentSelection;
            EditorApplication.update += ForceUpdateBackground;
            CheckCurrentSelection();
        }

        private void OnDisable()
        {
            Selection.selectionChanged -= CheckCurrentSelection;
            EditorApplication.update -= ForceUpdateBackground;
        }

        private void CheckCurrentSelection()
        {
            if (Selection.activeObject is VNCharacterSO character)
            {
                if (_currentCharacter != character)
                {
                    LoadCharacter(character);
                }
            }
        }

        private void ForceUpdateBackground()
        {
            if (_serializedCharacter != null)
            {
                _serializedCharacter.UpdateIfRequiredOrScript();
            }
            Repaint();
        }

        private void OnGUI()
        {
            if (_currentCharacter == null || _serializedCharacter == null)
            {
                EditorGUILayout.HelpBox("Please select a VNCharacterSO in the Project window.", MessageType.Info);
                return;
            }

            _serializedCharacter.Update();

            EditorGUILayout.BeginVertical(EditorStyles.toolbar);
            GUILayout.Label($"Editing Character: {_currentCharacter.name}", EditorStyles.boldLabel);
            EditorGUILayout.EndVertical();

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            // วาด List ทั้งหมด พร้อม Drawer สวยๆ ที่คุณเคยเขียนไว้
            if (_emotionsProp != null)
            {
                EditorGUILayout.PropertyField(_emotionsProp, new GUIContent("All Emotions"), true);
                EditorGUILayout.Space(10);
            }

            if (_animationsProp != null)
            {
                EditorGUILayout.PropertyField(_animationsProp, new GUIContent("All Animations"), true);
                EditorGUILayout.Space(10);
            }

            if (_soundEffectsProp != null)
            {
                EditorGUILayout.PropertyField(_soundEffectsProp, new GUIContent("All Sound Effects"), true);
                EditorGUILayout.Space(10);
            }

            EditorGUILayout.EndScrollView();

            _serializedCharacter.ApplyModifiedProperties();
        }
    }
}
#endif