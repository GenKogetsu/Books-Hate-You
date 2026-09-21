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
    /// <summary>
    /// Scans every prefab in the project for components tagged with
    /// [CreateHierarchyMenu] and writes HierarchyMenuDefinitions.g.cs so
    /// Unity picks up the static [MenuItem] entries after the next domain reload.
    ///
    /// Prefab names are discovered automatically — no manual declaration needed.
    /// Multiple prefabs sharing the same component each get their own menu entry.
    ///
    /// Build-safety: the generated file is always wrapped in #if UNITY_EDITOR,
    /// and all code here lives inside the same guard, so player builds are
    /// completely unaffected regardless of the assembly definition settings.
    /// </summary>
    [InitializeOnLoad]
    public static class HierarchyMenuGenerator
    {
        private const string RelativeOutputPath =
            "Packages/com.Kogetsu.Library/Editor/Attribute Helper/HierarchyMenuDefinitions.g.cs";

        // ─── [InitializeOnLoad] entry point ───────────────────────────────

        static HierarchyMenuGenerator()
        {
            // Defer until the current domain-reload compile pass has settled.
            EditorApplication.delayCall += Generate;
        }

        // ─── Public ───────────────────────────────────────────────────────

        [MenuItem("Tools/KogetsuLibrary/Regenerate Hierarchy Menu", priority = 200)]
        public static void Generate()
        {
            var entries = CollectEntries();
            string code = BuildSourceFile(entries);
            string absPath = ToAbsolutePath(RelativeOutputPath);

            // Only write when content actually changed — prevents reload loops.
            if (File.Exists(absPath) && File.ReadAllText(absPath, Encoding.UTF8) == code)
                return;

            Directory.CreateDirectory(Path.GetDirectoryName(absPath)!);
            File.WriteAllText(absPath, code, Encoding.UTF8);
            AssetDatabase.ImportAsset(RelativeOutputPath, ImportAssetOptions.ForceUpdate);
            Debug.Log("[HierarchyMenuGenerator] HierarchyMenuDefinitions.g.cs updated.");
        }

        // ─── Entry collection ─────────────────────────────────────────────

        private static List<MenuEntry> CollectEntries()
        {
            // Build a map: script GUID → menu path, for every type with the attribute.
            var guidToMenuPath = BuildScriptGuidMap();
            if (guidToMenuPath.Count == 0) return new List<MenuEntry>();

            var entries   = new List<MenuEntry>();
            var seenPaths = new HashSet<string>(StringComparer.Ordinal);

            // Scan every prefab in the project for any of those script GUIDs.
            string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab");
            foreach (string prefabGuid in prefabGuids)
            {
                string prefabPath = AssetDatabase.GUIDToAssetPath(prefabGuid);
                string absPath    = ToAbsolutePath(prefabPath);

                if (!File.Exists(absPath)) continue;

                // Read prefab YAML as text — fast, no need to load the asset.
                string yaml = File.ReadAllText(absPath, Encoding.UTF8);

                foreach (var kv in guidToMenuPath)
                {
                    // YAML MonoBehaviour reference looks like:  guid: <scriptGuid>
                    if (!yaml.Contains(kv.Key)) continue;

                    string prefabName    = Path.GetFileNameWithoutExtension(prefabPath);
                    string fullMenuPath  = $"GameObject/{kv.Value.Trim('/')}/{prefabName}";

                    if (seenPaths.Add(fullMenuPath))
                        entries.Add(new MenuEntry(fullMenuPath, prefabName));
                }
            }

            // Stable alphabetical order for deterministic file content.
            entries.Sort((a, b) => string.Compare(
                a.FullMenuPath, b.FullMenuPath, StringComparison.Ordinal));

            return entries;
        }

        /// <summary>
        /// Returns a dictionary of  scriptAssetGUID → menuPath
        /// for every non-abstract MonoBehaviour type tagged with [CreateHierarchyMenu].
        /// </summary>
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

                // Find the MonoScript asset whose class matches this type.
                string scriptGuid = FindScriptGuid(type);
                if (string.IsNullOrEmpty(scriptGuid)) continue;

                // A type might have the same script GUID registered twice only if
                // someone copies the attribute — guard against that.
                if (!map.ContainsKey(scriptGuid))
                    map[scriptGuid] = attr.MenuPath;
            }

            return map;
        }

        private static string FindScriptGuid(Type type)
        {
            // FindAssets returns fuzzy matches — verify by loading the script.
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

        // ─── Code generation ──────────────────────────────────────────────

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

        // ─── Helpers ──────────────────────────────────────────────────────

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

        // ─── Data ─────────────────────────────────────────────────────────

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
