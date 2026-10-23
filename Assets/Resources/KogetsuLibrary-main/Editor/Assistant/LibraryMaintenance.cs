using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Kogetsu.Library.Editor
{
    #region Documentation - XML
    /// <summary>
    /// <para> Summary : </para>
    /// <para> (TH) : เครื่องมือจัดโฟลเดอร์และตรวจ reference ที่เรียกซ้ำได้ โดยไม่เปลี่ยนข้อมูล gameplay </para>
    /// <para> (EN) : Repeatable folder migration and reference audit without changing gameplay data. </para>
    /// </summary>
    #endregion
    public static class LibraryMaintenance
    {
        private const string Root = "Assets/Resources/KogetsuLibrary-main";

        #region Documentation - XML
        /// <summary>
        /// <para> Summary : </para>
        /// <para> (TH) : ย้ายโฟลเดอร์ด้วย AssetDatabase พร้อม meta เพื่อรักษา GUID และลบเฉพาะ placeholder/โฟลเดอร์ว่างที่ระบุ; ข้าม source ที่ไม่มีและหยุดเมื่อปลายทางชนกัน </para>
        /// <para> (EN) : Migrates folders with their meta files through AssetDatabase to preserve GUIDs and removes only designated empty placeholders/folders; absent sources are skipped and destination collisions stop the operation. </para>
        /// </summary>
        #endregion
        [MenuItem("Tools/KogetsuLibrary/Organize Library Folders")]
        public static void Organize()
        {
            AssetDatabase.StartAssetEditing();
            try
            {
                Move("Runtime/Core", "Runtime/Systems");
                Move("Runtime/DesignPatternCore", "Runtime/Patterns");
                Move("Runtime/Attribute", "Runtime/Attributes");
                Move("Runtime/Extension", "Runtime/Extensions");
                foreach (string folder in Directory.GetDirectories(Root + "/Runtime/Systems"))
                {
                    string name = Path.GetFileName(folder);
                    string target = name == "VN_Core" ? "VisualNovel" : name.EndsWith("Core") ? name.Substring(0, name.Length - 4) : name;
                    if (target != name) Move("Runtime/Systems/" + name, "Runtime/Systems/" + target);
                }
                Move("Editor/Attribute Helper", "Editor/Menus");
                Move("Editor/InspactorEditor", "Editor/Inspectors");
                Move("Editor/Move Ability Editor", "Editor/Movement");
                Move("Editor/VN Editor", "Editor/VisualNovel");
                foreach (string scripts in Directory.GetDirectories(Root + "/Runtime", "Scripts", SearchOption.AllDirectories))
                {
                    string parent = Path.GetDirectoryName(scripts).Replace('\\', '/');
                    foreach (string entry in Directory.GetFileSystemEntries(scripts))
                    {
                        if (entry.EndsWith(".meta")) continue;
                        string target = parent + "/" + Path.GetFileName(entry);
                        if (File.Exists(target) || Directory.Exists(target)) continue;
                        string error = AssetDatabase.MoveAsset(entry.Replace('\\', '/'), target);
                        if (!string.IsNullOrEmpty(error)) throw new InvalidOperationException(error);
                    }
                    if (Directory.GetFileSystemEntries(scripts).Length == 0) AssetDatabase.DeleteAsset(scripts.Replace('\\', '/'));
                }
                RenameScript("Runtime/Systems/Audio/ObserverChannel/AudioObserver.cs", "AudioObserverSO.cs");
                RenameScript("Runtime/Systems/Skill/CooldownController.cs", "CooldowmContoller.cs");
                string emptyScript = Root + "/Runtime/Systems/Input/ObserverChannel/PlayerInputObserverSO.cs";
                if (File.Exists(emptyScript) && File.ReadAllText(emptyScript).TrimStart().StartsWith("// File intentionally empty."))
                    AssetDatabase.DeleteAsset(emptyScript);
                string emptyFolder = Root + "/Runtime/Systems/InputCore 1";
                if (Directory.Exists(emptyFolder) && Directory.GetFileSystemEntries(emptyFolder).Length == 0)
                    AssetDatabase.DeleteAsset(emptyFolder);
            }
            finally { AssetDatabase.StopAssetEditing(); }
            AssetDatabase.SaveAssets();
            Debug.Log("[KogetsuLibrary] Folder migration complete; existing asset GUIDs preserved.");
        }

        private static void Move(string from, string to)
        {
            string source = Root + "/" + from, target = Root + "/" + to;
            if (!Directory.Exists(source)) return;
            if (Directory.Exists(target)) throw new InvalidOperationException(target + " already exists");
            string error = AssetDatabase.MoveAsset(source, target);
            if (!string.IsNullOrEmpty(error)) throw new InvalidOperationException(error);
        }

        private static void RenameScript(string relativePath, string fileName)
        {
            string path = Root + "/" + relativePath;
            if (!File.Exists(path)) return;
            string error = AssetDatabase.RenameAsset(path, fileName);
            if (!string.IsNullOrEmpty(error)) throw new InvalidOperationException(error);
        }

        #region Documentation - XML
        /// <summary>
        /// <para> Summary : </para>
        /// <para> (TH) : ตรวจ prefab ของไลบรารี/เกมและ Scene ที่โหลดอยู่; เขียน Logs/KogetsuReferenceAudit.txt โดยรายงาน Missing Script และ reference ที่มี ID แต่ resolve ไม่ได้ ไม่รวม null ที่ยังไม่เคยกำหนด </para>
        /// <para> (EN) : Audits library/game prefabs and loaded scenes into Logs/KogetsuReferenceAudit.txt, reporting missing scripts and unresolved retained IDs while excluding unassigned nulls. </para>
        /// </summary>
        #endregion
        [MenuItem("Tools/KogetsuLibrary/Audit References")]
        public static void Audit()
        {
            var issues = new List<string>();
            int count = 0;
            foreach (string guid in AssetDatabase.FindAssets("t:Prefab", new[] { Root, "Assets/Prefabs" }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (path.Contains("/Third Party Library/")) continue;
                var root = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (root) Scan(root, path, issues, ref count);
            }
            for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCount; i++)
            {
                var scene = UnityEngine.SceneManagement.SceneManager.GetSceneAt(i);
                if (!scene.isLoaded) continue;
                foreach (var root in scene.GetRootGameObjects()) Scan(root, scene.path, issues, ref count);
            }
            Directory.CreateDirectory("Logs");
            File.WriteAllText("Logs/KogetsuReferenceAudit.txt", $"Components checked: {count}\nIssues: {issues.Count}\n" + string.Join("\n", issues));
            Debug.Log($"[KogetsuLibrary] Audit: {count} components, {issues.Count} broken references. See Logs/KogetsuReferenceAudit.txt");
        }

        private static void Scan(GameObject root, string location, List<string> issues, ref int count)
        {
            foreach (var transform in root.GetComponentsInChildren<Transform>(true))
            {
                int missing = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(transform.gameObject);
                if (missing > 0) issues.Add($"MISSING SCRIPT {location}/{transform.name}: {missing}");
                foreach (var component in transform.GetComponents<MonoBehaviour>())
                {
                    if (!component) continue;
                    count++;
                    var so = new SerializedObject(component);
                    var property = so.GetIterator();
                    while (property.Next(true))
                        if (property.propertyType == SerializedPropertyType.ObjectReference &&
                            !property.propertyPath.StartsWith("m_") &&
                            property.objectReferenceValue == null && !property.objectReferenceEntityIdValue.Equals(default(EntityId)))
                            issues.Add($"BROKEN REF {location}/{transform.name}/{component.GetType().Name}.{property.propertyPath}");
                }
            }
        }

        #region Documentation - XML
        /// <summary>
        /// <para> Summary : </para>
        /// <para> (TH) : ตรวจการ dispatch, subscribe ซ้ำ, unsubscribe ทีละครั้ง และ Clear ด้วย dispatcher แยก; เขียนผลผ่านลง Logs/KogetsuContractChecks.txt หรือ throw เมื่อเงื่อนไขผิด </para>
        /// <para> (EN) : Checks dispatch, duplicate registrations, balanced removal and Clear on an isolated dispatcher; writes success to Logs/KogetsuContractChecks.txt or throws on failure. </para>
        /// </summary>
        #endregion
        [MenuItem("Tools/KogetsuLibrary/Verify Event Dispatcher")]
        public static void VerifyEventDispatcher()
        {
            var bus = new DesignPatternCore.EventDispatcher();
            int calls = 0;
            Action<DesignPatternCore.GameClearEvent> handler = _ => calls++;
            bus.Subscribe(handler); bus.Subscribe(handler);
            bus.Publish(new DesignPatternCore.GameClearEvent());
            if (calls != 2) throw new Exception("Dispatch failed");
            bus.Unsubscribe(handler); bus.Publish(new DesignPatternCore.GameClearEvent());
            if (calls != 3) throw new Exception("Unsubscribe failed");
            bus.Unsubscribe(handler); bus.Publish(new DesignPatternCore.GameClearEvent());
            if (calls != 3) throw new Exception("Removal failed");
            bus.Subscribe(handler); bus.Clear(); bus.Publish(new DesignPatternCore.GameClearEvent());
            if (calls != 3) throw new Exception("Clear failed");
            Directory.CreateDirectory("Logs");
            File.WriteAllText("Logs/KogetsuContractChecks.txt", "PASS: typed dispatch, duplicate subscriptions, balanced unsubscribe, removal and clear.\n");
            Debug.Log("[KogetsuLibrary] EventDispatcher contract checks passed.");
        }
    }
}
