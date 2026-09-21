#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace Kogetsu.Library.Assistant
{
    [InitializeOnLoad]
    public static class TagInitializer
    {
        private const string Suffix = ".KogetsuLibrary";

        static TagInitializer()
        {
            EditorApplication.delayCall += InitializeTags;
        }

        public static void InitializeTags()
        {
            string[] guids = AssetDatabase.FindAssets("t:TagSettingsSO");
            if (guids.Length == 0) return;

            var path = AssetDatabase.GUIDToAssetPath(guids[0]);
            var settings = AssetDatabase.LoadAssetAtPath<TagSettingsSO>(path);
            if (settings == null || settings.RequiredTags == null) return;

            var tagManagerPath = "ProjectSettings/TagManager.asset";
            Object[] assets = AssetDatabase.LoadAllAssetsAtPath(tagManagerPath);
            if (assets == null || assets.Length == 0) return;

            SerializedObject tagManager = new(assets[0]);
            SerializedProperty tagsProp = tagManager.FindProperty("tags");

            List<string> userCustomTags = new();
            for (int i = 0; i < tagsProp.arraySize; i++)
            {
                string existingTag = tagsProp.GetArrayElementAtIndex(i).stringValue;
                if (!existingTag.EndsWith(Suffix))
                {
                    userCustomTags.Add(existingTag);
                }
            }

            // ตรวจสอบก่อนว่าต้องอัปเดตไหม (เพื่อป้องกันการเขียนไฟล์ ProjectSettings บ่อยเกินจำเป็น)
            if (!NeedsUpdate(tagsProp, userCustomTags, settings.RequiredTags)) return;

            tagsProp.ClearArray();
            int index = 0;
            foreach (var utag in userCustomTags)
            {
                tagsProp.InsertArrayElementAtIndex(index);
                tagsProp.GetArrayElementAtIndex(index).stringValue = utag;
                index++;
            }

            foreach (var rawTag in settings.RequiredTags)
            {
                if (string.IsNullOrEmpty(rawTag)) continue;
                string finalTag = rawTag.EndsWith(Suffix) ? rawTag : rawTag + Suffix;
                tagsProp.InsertArrayElementAtIndex(index);
                tagsProp.GetArrayElementAtIndex(index).stringValue = finalTag;
                index++;
            }

            tagManager.ApplyModifiedProperties();
            Debug.Log($"<color=#87CEEB>[kogetsu]</color> Auto-Synced tags after Domain Reload.");
        }

        private static bool NeedsUpdate(SerializedProperty tagsProp, List<string> userTags, List<string> requiredTags)
        {
            int expectedTotal = userTags.Count + requiredTags.Count;
            if (tagsProp.arraySize != expectedTotal) return true;

            for (int i = 0; i < requiredTags.Count; i++)
            {
                string expected = requiredTags[i].EndsWith(Suffix) ? requiredTags[i] : requiredTags[i] + Suffix;
                string actual = tagsProp.GetArrayElementAtIndex(userTags.Count + i).stringValue;
                if (expected != actual) return true;
            }
            return false;
        }
    }
}
#endif