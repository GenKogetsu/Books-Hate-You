#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using Kogetsu.Library.Core;

namespace Kogetsu.Library.Editor
{
    public class VNSceneEditorWindow : EditorWindow
    {
        private VNSceneSO _currentVNScene;
        private SerializedObject _serializedVNScene;
        private SerializedProperty _conversationsProp;

        private int _selectedIndex = -1;
        private Vector2 _leftScroll;
        private Vector2 _rightScroll;
        private const float LeftWidth = 210f;

        [MenuItem("Window/KogetsuLibrary/VN Scene Editor")]
        public static void ShowWindow()
        {
            var w = GetWindow<VNSceneEditorWindow>("VN Scene Editor");
            w.minSize = new Vector2(600, 400);
            w.CheckCurrentSelection();
            w.Show();
        }

        public static void OpenVNSceneWindow(VNSceneSO vnScene)
        {
            var w = GetWindow<VNSceneEditorWindow>("VN Scene Editor");
            w.minSize = new Vector2(600, 400);
            w.LoadVNScene(vnScene);
            w.Show();
        }

        private void LoadVNScene(VNSceneSO vnScene)
        {
            if (vnScene == null) return;
            _currentVNScene = vnScene;
            _serializedVNScene = new SerializedObject(_currentVNScene);
            _conversationsProp = _serializedVNScene.FindProperty("Conversations");
            SelectConversation(0);
            Repaint();
        }

        private void OnEnable()
        {
            Selection.selectionChanged += CheckCurrentSelection;
            EditorApplication.update += OnEditorUpdate;
            CheckCurrentSelection();
        }

        private void OnDisable()
        {
            Selection.selectionChanged -= CheckCurrentSelection;
            EditorApplication.update -= OnEditorUpdate;
        }

        private void CheckCurrentSelection()
        {
            if (Selection.activeObject is VNSceneSO vnScene && _currentVNScene != vnScene)
                LoadVNScene(vnScene);
        }

        private void OnEditorUpdate()
        {
            _serializedVNScene?.UpdateIfRequiredOrScript();
            Repaint();
        }

        private void SelectConversation(int index)
        {
            int count = _conversationsProp?.arraySize ?? 0;
            if (count == 0) { _selectedIndex = -1; return; }
            _selectedIndex = Mathf.Clamp(index, 0, count - 1);
            _rightScroll = Vector2.zero;
            _conversationsProp.GetArrayElementAtIndex(_selectedIndex).isExpanded = true;
        }

        private void OnGUI()
        {
            if (_currentVNScene == null || _serializedVNScene == null)
            {
                EditorGUILayout.HelpBox("Please select a VNSceneSO in the Project window.", MessageType.Info);
                return;
            }

            _serializedVNScene.Update();
            DrawToolbar();

            using (new EditorGUILayout.HorizontalScope())
            {
                DrawLeftPanel();
                var divRect = GUILayoutUtility.GetRect(2, 2, GUILayout.ExpandHeight(true), GUILayout.Width(2));
                EditorGUI.DrawRect(divRect, new Color(0.3f, 0.3f, 0.3f, 0.5f));
                DrawRightPanel();
            }

            _serializedVNScene.ApplyModifiedProperties();
        }

        private void DrawToolbar()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                GUILayout.Label($"Scene: {_currentVNScene.name}", EditorStyles.boldLabel, GUILayout.ExpandWidth(false));
                GUILayout.FlexibleSpace();
                VNConversationNodeDrawer.UseTint = GUILayout.Toggle(
                    VNConversationNodeDrawer.UseTint, "Tint", EditorStyles.toolbarButton);
                GUILayout.Space(8);
                GUILayout.Label($"{_conversationsProp?.arraySize ?? 0} conversations", EditorStyles.miniLabel);
            }
        }

        private void DrawLeftPanel()
        {
            using (new EditorGUILayout.VerticalScope(GUILayout.Width(LeftWidth)))
            {
                using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
                    GUILayout.Label("Conversations", EditorStyles.boldLabel);

                using (var sv = new EditorGUILayout.ScrollViewScope(_leftScroll, GUILayout.ExpandHeight(true)))
                {
                    _leftScroll = sv.scrollPosition;
                    int count = _conversationsProp?.arraySize ?? 0;
                    for (int i = 0; i < count; i++)
                        DrawConversationListItem(i);
                }

                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("+ Add", EditorStyles.miniButtonLeft))
                    {
                        _conversationsProp.arraySize++;
                        _serializedVNScene.ApplyModifiedProperties();
                        SelectConversation(_conversationsProp.arraySize - 1);
                    }

                    int count = _conversationsProp?.arraySize ?? 0;
                    GUI.enabled = _selectedIndex >= 0 && _selectedIndex < count;
                    if (GUILayout.Button("- Remove", EditorStyles.miniButtonRight))
                    {
                        _conversationsProp.DeleteArrayElementAtIndex(_selectedIndex);
                        _serializedVNScene.ApplyModifiedProperties();
                        SelectConversation(_selectedIndex);
                    }
                    GUI.enabled = true;
                }
            }
        }

        private void DrawConversationListItem(int i)
        {
            var convProp = _conversationsProp.GetArrayElementAtIndex(i);
            var modeProp = convProp.FindPropertyRelative("ConversationMode");
            bool isChoice = modeProp != null && modeProp.enumValueIndex == (int)VNConversationMode.ChoiceMode;

            Color tint = isChoice ? VNConversationNodeDrawer._exitColor : VNConversationNodeDrawer._mainColor;
            bool isSelected = i == _selectedIndex;

            var prevBg = GUI.backgroundColor;
            GUI.backgroundColor = isSelected ? tint : new Color(tint.r, tint.g, tint.b, 0.45f);

            if (GUILayout.Button($" {i + 1}. {(isChoice ? "[Choice]" : "[Dialogue]")}", EditorStyles.miniButton))
            {
                SelectConversation(i);
                GUI.FocusControl(null);
            }

            GUI.backgroundColor = prevBg;
        }

        private void DrawRightPanel()
        {
            using (new EditorGUILayout.VerticalScope(GUILayout.ExpandWidth(true)))
            {
                int count = _conversationsProp?.arraySize ?? 0;
                if (_selectedIndex < 0 || _selectedIndex >= count)
                {
                    EditorGUILayout.HelpBox("Select a conversation from the left panel, or click \"+ Add\".", MessageType.Info);
                    return;
                }

                using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
                {
                    GUI.enabled = _selectedIndex > 0;
                    if (GUILayout.Button("◀", EditorStyles.toolbarButton, GUILayout.Width(28)))
                        SelectConversation(_selectedIndex - 1);

                    GUI.enabled = _selectedIndex < count - 1;
                    if (GUILayout.Button("▶", EditorStyles.toolbarButton, GUILayout.Width(28)))
                        SelectConversation(_selectedIndex + 1);

                    GUI.enabled = true;
                    GUILayout.Label($"  {_selectedIndex + 1} / {count}", EditorStyles.miniLabel);
                    GUILayout.FlexibleSpace();
                }

                using (var sv = new EditorGUILayout.ScrollViewScope(_rightScroll, GUILayout.ExpandHeight(true)))
                {
                    _rightScroll = sv.scrollPosition;
                    var prop = _conversationsProp.GetArrayElementAtIndex(_selectedIndex);
                    EditorGUILayout.PropertyField(prop, new GUIContent($"Conversation {_selectedIndex + 1}"), true);
                }
            }
        }
    }
}
#endif
