#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using Kogetsu.Library.Attribute;

namespace Kogetsu.Library.Editor
{
    #region Documentation - XML
    /// <summary>
    /// <para> Summary : </para>
    /// <para> (TH) : ตรวจ prefab ที่ใช้ CreateHierarchyMenu แล้ว generate เมนูจากชื่อ prefab อัตโนมัติ; โค้ดเมนูอยู่ภายใต้ UNITY_EDITOR </para>
    /// <para> (EN) : Scans every prefab in the project for components tagged with [CreateHierarchyMenu] and writes HierarchyMenuDefinitions.g.cs so Unity picks up the static [MenuItem] entries after the next domain reload. Prefab names are discovered automatically — no manual declaration needed. Multiple prefabs sharing the same component each get their own menu entry. Build-safety: the generated file is always wrapped in #if UNITY_EDITOR, and all code here lives inside the same guard, so player builds are completely unaffected regardless of the assembly definition settings. </para>
    /// </summary>
    #endregion
    [InitializeOnLoad]
    public static class HierarchyMenuGenerator
    {
        private static string RelativeOutputPath =>
            LibraryAssetLocator.Find("HierarchyMenuDefinitions.g.cs");

        #region Documentation - Notes
        // ─── [InitializeOnLoad] entry point ───────────────────────────────
        #endregion

        static HierarchyMenuGenerator()
        {
            #region Documentation - Notes
            // Defer until the current domain-reload compile pass has settled.
            #endregion
            EditorApplication.delayCall += Generate;
        }

        #region Documentation - Notes
        // ─── Public ───────────────────────────────────────────────────────
        #endregion

        [MenuItem("Tools/KogetsuLibrary/Regenerate Hierarchy Menu", priority = 200)]
        public static void Generate()
        {
            var entries = CollectEntries();
            string code = BuildSourceFile(entries);
            string absPath = ToAbsolutePath(RelativeOutputPath);

            #region Documentation - Notes
            // Only write when content actually changed — prevents reload loops.
            #endregion
            if (File.Exists(absPath) && File.ReadAllText(absPath, Encoding.UTF8) == code)
                return;

            Directory.CreateDirectory(Path.GetDirectoryName(absPath)!);
            File.WriteAllText(absPath, code, Encoding.UTF8);
            AssetDatabase.ImportAsset(RelativeOutputPath, ImportAssetOptions.ForceUpdate);
            Debug.Log("[HierarchyMenuGenerator] HierarchyMenuDefinitions.g.cs updated.");
        }

        #region Documentation - Notes
        // ─── Entry collection ─────────────────────────────────────────────
        #endregion

        private static List<MenuEntry> CollectEntries()
        {
            #region Documentation - Notes
            // Build a map: script GUID → menu path, for every type with the attribute.
            #endregion
            var guidToMenuPath = BuildScriptGuidMap();
            if (guidToMenuPath.Count == 0) return new List<MenuEntry>();

            var entries   = new List<MenuEntry>();
            var seenPaths = new HashSet<string>(StringComparer.Ordinal);

            #region Documentation - Notes
            // Scan every prefab in the project for any of those script GUIDs.
            #endregion
            string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab");
            foreach (string prefabGuid in prefabGuids)
            {
                string prefabPath = AssetDatabase.GUIDToAssetPath(prefabGuid);
                string absPath    = ToAbsolutePath(prefabPath);

                if (!File.Exists(absPath)) continue;

                #region Documentation - Notes
                // Read prefab YAML as text — fast, no need to load the asset.
                #endregion
                string yaml = File.ReadAllText(absPath, Encoding.UTF8);

                foreach (var kv in guidToMenuPath)
                {
                    #region Documentation - Notes
                    // YAML MonoBehaviour reference looks like:  guid: <scriptGuid>
                    #endregion
                    if (!yaml.Contains(kv.Key)) continue;

                    string prefabName    = Path.GetFileNameWithoutExtension(prefabPath);
                    string fullMenuPath  = $"GameObject/{kv.Value.Trim('/')}/{prefabName}";

                    if (seenPaths.Add(fullMenuPath))
                        entries.Add(new MenuEntry(fullMenuPath, prefabName));
                }
            }

            #region Documentation - Notes
            // Stable alphabetical order for deterministic file content.
            #endregion
            entries.Sort((a, b) => string.Compare(
                a.FullMenuPath, b.FullMenuPath, StringComparison.Ordinal));

            return entries;
        }

        #region Documentation - XML
        /// <summary>
        /// <para> Summary : </para>
        /// <para> (TH) : สร้างตาราง GUID ของสคริปต์ไปยัง menu path สำหรับ MonoBehaviour ที่ไม่เป็น abstract และมี CreateHierarchyMenu </para>
        /// <para> (EN) : Returns a dictionary of scriptAssetGUID → menuPath for every non-abstract MonoBehaviour type tagged with [CreateHierarchyMenu]. </para>
        /// </summary>
        #endregion
        private static Dictionary<string, string> BuildScriptGuidMap()
        {
            var map = new Dictionary<string, string>(StringComparer.Ordinal);

            foreach (Type type in TypeCache.GetTypesWithAttribute<CreateHierarchyMenu>())
            {
                if (type.IsAbstract) continue;

                var attr = (CreateHierarchyMenu)
                    type.GetCustomAttributes(typeof(CreateHierarchyMenu), false)
                        .FirstOrDefault();
                if (attr == null) continue;

                #region Documentation - Notes
                // Find the MonoScript asset whose class matches this type.
                #endregion
                string scriptGuid = FindScriptGuid(type);
                if (string.IsNullOrEmpty(scriptGuid)) continue;

                #region Documentation - Notes
                // A type might have the same script GUID registered twice only if
                // someone copies the attribute — guard against that.
                #endregion
                if (!map.ContainsKey(scriptGuid))
                    map[scriptGuid] = attr.MenuPath;
            }

            return map;
        }

        private static string FindScriptGuid(Type type)
        {
            #region Documentation - Notes
            // FindAssets returns fuzzy matches — verify by loading the script.
            #endregion
            string[] guids = AssetDatabase.FindAssets($"{type.Name} t:MonoScript");
            foreach (string g in guids)
            {
                string path   = AssetDatabase.GUIDToAssetPath(g);
                var    script = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
                if (script != null && script.GetClass() == type)
                    return g;
            }
            return null;
        }

        #region Documentation - Notes
        // ─── Code generation ──────────────────────────────────────────────
        #endregion

        private static string BuildSourceFile(IReadOnlyList<MenuEntry> entries)
        {
            var sb = new StringBuilder(2048);

            sb.AppendLine("// <auto-generated>");
            sb.AppendLine("// Generated by HierarchyMenuGenerator — DO NOT EDIT MANUALLY.");
            sb.AppendLine("// Regenerate via:  Tools ▸ KogetsuLibrary ▸ Regenerate Hierarchy Menu");
            sb.AppendLine("// </auto-generated>");
            sb.AppendLine();
            sb.AppendLine("#if UNITY_EDITOR");
            sb.AppendLine("using UnityEditor;");
            sb.AppendLine();
            sb.AppendLine("namespace Kogetsu.Library.Editor");
            sb.AppendLine("{");
            sb.AppendLine("    // ReSharper disable UnusedMember.Local");
            sb.AppendLine("    internal static class HierarchyMenuDefinitions");
            sb.AppendLine("    {");

            if (entries.Count == 0)
            {
                sb.AppendLine("        // No prefabs with [CreateHierarchyMenu] components found.");
            }
            else
            {
                var usedNames = new HashSet<string>(StringComparer.Ordinal);

                foreach (MenuEntry e in entries)
                {
                    string methodName = UniqueMethodName(e.FullMenuPath, usedNames);

                    sb.AppendLine($"        [MenuItem(\"{e.FullMenuPath}\", false, 10)]");
                    sb.AppendLine($"        static void {methodName}()");
                    sb.AppendLine($"            => HierarchyMenuCreator.CreateFromPrefab(\"{e.PrefabName}\");");
                    sb.AppendLine();
                }
            }

            sb.AppendLine("    }");
            sb.AppendLine("    // ReSharper restore UnusedMember.Local");
            sb.AppendLine("}");
            sb.AppendLine("#endif");

            return sb.ToString();
        }

        #region Documentation - Notes
        // ─── Helpers ──────────────────────────────────────────────────────
        #endregion

        private static string ToAbsolutePath(string projectRelativePath)
        {
            string projectRoot = Path.GetDirectoryName(Application.dataPath)!;
            return Path.GetFullPath(Path.Combine(projectRoot, projectRelativePath));
        }

        private static string UniqueMethodName(string fullMenuPath, HashSet<string> used)
        {
            string sanitized = "Create_" + new string(
                fullMenuPath
                    .Replace("GameObject/", "")
                    .Select(c => char.IsLetterOrDigit(c) ? c : '_')
                    .ToArray());

            if (used.Add(sanitized)) return sanitized;

            for (int i = 2; ; i++)
            {
                string candidate = $"{sanitized}_{i}";
                if (used.Add(candidate)) return candidate;
            }
        }

        #region Documentation - Notes
        // ─── Data ─────────────────────────────────────────────────────────
        #endregion

        private readonly struct MenuEntry
        {
            public readonly string FullMenuPath;
            public readonly string PrefabName;

            public MenuEntry(string fullMenuPath, string prefabName)
            {
                FullMenuPath = fullMenuPath;
                PrefabName   = prefabName;
            }
        }
    }
}
#endif
