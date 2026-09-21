#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Kogetsu.Library.Editor
{
    /// <summary>
    /// Runtime helper for hierarchy menu entries produced by HierarchyMenuGenerator.
    /// The generated file (HierarchyMenuDefinitions.g.cs) delegates here so it
    /// stays thin and never needs to change when creation logic evolves.
    /// </summary>
    public static class HierarchyMenuCreator
    {
        // ─── Public API called by generated code ───────────────────────────

        /// <summary>
        /// Finds the prefab named <paramref name="prefabName"/> anywhere in the
        /// project, instantiates it into the scene, and selects it.
        /// Falls back to an empty GameObject if no prefab is found.
        /// </summary>
        public static void CreateFromPrefab(string prefabName)
        {
            var prefab = FindPrefabByName(prefabName);

            GameObject instance;
            if (prefab != null)
            {
                instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                instance.name = prefab.name;
            }
            else
            {
                instance = new GameObject(prefabName);
                Debug.LogWarning($"[HierarchyMenu] Prefab '{prefabName}' not found — created empty GameObject.");
            }

            // Parent to current selection (if any) and align transform
            var activeGO = Selection.activeGameObject;
            GameObjectUtility.SetParentAndAlign(instance, activeGO);

            Undo.RegisterCreatedObjectUndo(instance, "Create " + instance.name);
            Selection.activeObject = instance;

            if (instance.scene.IsValid())
                EditorSceneManager.MarkSceneDirty(instance.scene);
        }

        // ─── Private helpers ───────────────────────────────────────────────

        private static GameObject FindPrefabByName(string name)
        {
            // FindAssets is a fuzzy search — filter by exact file name afterwards.
            string[] guids = AssetDatabase.FindAssets($"{name} t:Prefab");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (Path.GetFileNameWithoutExtension(path) == name)
                {
                    var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                    if (prefab != null) return prefab;
                }
            }
            return null;
        }
    }
}
#endif
