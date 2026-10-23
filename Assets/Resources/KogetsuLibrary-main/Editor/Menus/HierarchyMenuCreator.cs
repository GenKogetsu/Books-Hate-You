#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Kogetsu.Library.Editor
{
    #region Documentation - XML
    /// <summary>
    /// <para> Summary : </para>
    /// <para> (TH) : ตัวช่วยสร้าง object ให้เมนู Hierarchy ที่ generate ไว้ โดยรวมขั้นตอนสร้างไว้จุดเดียว </para>
    /// <para> (EN) : Runtime helper for hierarchy menu entries produced by HierarchyMenuGenerator. The generated file (HierarchyMenuDefinitions.g.cs) delegates here so it stays thin and never needs to change when creation logic evolves. </para>
    /// </summary>
    #endregion
    public static class HierarchyMenuCreator
    {
        #region Documentation - Notes
        // ─── Public API called by generated code ───────────────────────────
        #endregion

        #region Documentation - XML
        /// <summary>
        /// <para> Summary : </para>
        /// <para> (TH) : ค้นหา prefab ตามชื่อ สร้างลง Scene และเลือก object ที่สร้าง; หากไม่พบจะสร้าง GameObject ว่างแทน </para>
        /// <para> (EN) : Finds the prefab named <paramref name="prefabName"/> anywhere in the project, instantiates it into the scene, and selects it. Falls back to an empty GameObject if no prefab is found. </para>
        /// </summary>
        #endregion
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

            #region Documentation - Notes
            // Parent to current selection (if any) and align transform
            #endregion
            var activeGO = Selection.activeGameObject;
            GameObjectUtility.SetParentAndAlign(instance, activeGO);

            Undo.RegisterCreatedObjectUndo(instance, "Create " + instance.name);
            Selection.activeObject = instance;

            if (instance.scene.IsValid())
                EditorSceneManager.MarkSceneDirty(instance.scene);
        }

        #region Documentation - Notes
        // ─── Private helpers ───────────────────────────────────────────────
        #endregion

        private static GameObject FindPrefabByName(string name)
        {
            #region Documentation - Notes
            // FindAssets is a fuzzy search — filter by exact file name afterwards.
            #endregion
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
