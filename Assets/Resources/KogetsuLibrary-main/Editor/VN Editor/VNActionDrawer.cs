#if UNITY_EDITOR
using System;
using System.Linq;
using UnityEditor;
using Kogetsu.Library.Core;

namespace Kogetsu.Library.Editor
{
    [CustomPropertyDrawer(typeof(VNAction))]
    public class VNActionDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect pos, SerializedProperty prop, GUIContent label)
        {
            Color bgColor = VNConversationNodeDrawer.UseTint ? new Color(0.95f, 0.95f, 0.98f) : Color.white;
            GUI.color = bgColor;
            // ดันพื้นหลังมาทางซ้าย 5 หน่วย และขยายความกว้างชดเชยเพื่อไม่ให้ขอบขวาหด
            GUI.Box(new Rect(pos.x - 5f, pos.y, pos.width + 5f, GetPropertyHeight(prop, label)), "", EditorStyles.helpBox);
            GUI.color = Color.white;

            var index = GetIndex(prop.propertyPath);

            // ขยับกรอบของ Foldout ไปทางขวา 10 หน่วย (pos.x + 14f) เพื่อให้มีระยะห่างจากขอบซ้าย/ปุ่ม =
            var foldoutRect = new Rect(pos.x + 13f, pos.y + 2f, pos.width - 14f, EditorGUIUtility.singleLineHeight);

            // สร้าง Style พิเศษเพื่อเพิ่มช่องว่างระหว่างลูกศร (Foldout) กับตัวหนังสือ
            GUIStyle actionLabelStyle = new GUIStyle(EditorStyles.foldout);
            actionLabelStyle.padding.right += 2; // ดันข้อความออกห่างจากลูกศร

            prop.isExpanded = EditorGUI.Foldout(foldoutRect, prop.isExpanded, $"Action {index}", true, actionLabelStyle);
            if (!prop.isExpanded) return;

            EditorGUI.indentLevel++;
            var y = pos.y + EditorGUIUtility.singleLineHeight + 6f;
            var charData = GetCharacterContext(prop);

            // ดึงข้อมูลให้ตรงกับฟิลด์ใหม่ใน VNCharacterSO
            var emoOptions = charData != null ? charData.Emotions.Where(e => e.EmoteClip != null).Select(e => e.EmoteClip.name).ToArray() : Array.Empty<string>();
            var animOptions = charData != null ? charData.BehaviorAnimations.Where(a => a.BehaviorAnimationClip != null).Select(a => a.BehaviorAnimationClip.name).ToArray() : Array.Empty<string>();
            var sfxOptions = charData != null ? charData.SoundEffects.Where(s => s.SoundEffectClip != null).Select(s => s.SoundEffectClip.name).ToArray() : Array.Empty<string>();

            var useEmo = prop.FindPropertyRelative("UseEmotion");
            var useAnim = prop.FindPropertyRelative("UseBehaviorAnimation");
            var useSfx = prop.FindPropertyRelative("UseSoundEffect");

            DrawActionRow(ref y, pos, useEmo, prop.FindPropertyRelative("EmotionName"), "Emotion", emoOptions);
            DrawActionRow(ref y, pos, useAnim, prop.FindPropertyRelative("BehaviorAnimationName"), "Animation", animOptions);
            DrawActionRow(ref y, pos, useSfx, prop.FindPropertyRelative("SoundEffectName"), "Sound Effect", sfxOptions);

            DrawNormalRow(ref y, pos, prop.FindPropertyRelative("UseDelay"), prop.FindPropertyRelative("DelayTime"), "Delay");

            EditorGUI.indentLevel--;
        }

        private void DrawActionRow(ref float y, Rect pos, SerializedProperty use, SerializedProperty val, string label, string[] opt)
        {
            use.boolValue = EditorGUI.ToggleLeft(new Rect(pos.x, y, 140, EditorGUIUtility.singleLineHeight), label, use.boolValue);

            if (use.boolValue)
            {
                var fieldRect = new Rect(pos.x + 140, y, pos.width - 150, EditorGUIUtility.singleLineHeight);
                if (opt == null || opt.Length == 0)
                {
                    EditorGUI.LabelField(fieldRect, "No Data", EditorStyles.helpBox);
                }
                else
                {
                    string current = val.stringValue;
                    if (string.IsNullOrEmpty(current) || Array.IndexOf(opt, current) < 0)
                    {
                        current = opt[0];
                        // new: write back to serialized property — previously only changed local var
                        // so val.stringValue stayed "" and other classes read null
                        val.stringValue = current;
                        val.serializedObject.ApplyModifiedPropertiesWithoutUndo();
                    }

                    if (GUI.Button(fieldRect, current, EditorStyles.popup))
                    {
                        var menu = new GenericMenu();
                        // new: capture path + serializedObject instead of the property itself
                        // SerializedProperty reference goes stale after a frame (GenericMenu runs next frame)
                        // → re-find from path inside callback to always get a fresh, valid reference
                        var capturedSO   = val.serializedObject;
                        var capturedPath = val.propertyPath;
                        foreach (string option in opt)
                        {
                            string o = option;
                            menu.AddItem(new GUIContent(o), current == o, () =>
                            {
                                capturedSO.Update();
                                var fresh = capturedSO.FindProperty(capturedPath);
                                if (fresh != null)
                                {
                                    fresh.stringValue = o;
                                    capturedSO.ApplyModifiedProperties();
                                }
                            });
                        }
                        menu.ShowAsContext();
                    }
                }
            }
            y += EditorGUIUtility.singleLineHeight + 4f;
        }

        private void DrawNormalRow(ref float y, Rect pos, SerializedProperty use, SerializedProperty val, string label)
        {
            use.boolValue = EditorGUI.ToggleLeft(new Rect(pos.x, y, 140, EditorGUIUtility.singleLineHeight), label, use.boolValue);
            if (use.boolValue)
            {
                EditorGUI.PropertyField(new Rect(pos.x + 140, y, pos.width - 150, EditorGUIUtility.singleLineHeight), val, GUIContent.none);
            }
            y += EditorGUIUtility.singleLineHeight + 4f;
        }

        public override float GetPropertyHeight(SerializedProperty prop, GUIContent label)
        {
            if (!prop.isExpanded) return EditorGUIUtility.singleLineHeight + 4f;
            return ((EditorGUIUtility.singleLineHeight + 4f) * 5) + 6f;
        }

        private VNCharacterSO GetCharacterContext(SerializedProperty property)
        {
            var path = property.propertyPath;
            var speakerIdx = path.IndexOf(".Speakers.Array.data[");
            if (speakerIdx == -1) return null;

            var endBracket = path.IndexOf("]", speakerIdx);
            var speakerProp = property.serializedObject.FindProperty(path[..(endBracket + 1)]);
            return (speakerProp != null) ? speakerProp.FindPropertyRelative("Character").objectReferenceValue as VNCharacterSO : null;
        }

        private int GetIndex(string path)
        {
            var s = path.LastIndexOf('[') + 1;
            var e = path.LastIndexOf(']');
            return (s > 0 && e > s && int.TryParse(path[s..e], out var i)) ? i + 1 : 1;
        }
    }
}
#endif
