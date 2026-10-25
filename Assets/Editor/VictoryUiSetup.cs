using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class VictoryUiSetup
{
    private const string CanvasPath = "Assets/Prefabs/Ui/Canvas.prefab";
    private const string Floor2PrefabPath = "Assets/Prefabs/Map/Floor2.prefab";
    private const string VictoryClipGuid = "9933ebee89e8e1442b9612e2a34251aa";

    [MenuItem("Tools/Victory UI Setup")]
    public static void Apply()
    {
        ConfigureCanvasPrefab();
        ConfigureFinalDoor();
        LinkCanvasToGameplayScenes();
        AssetDatabase.SaveAssets();
        Debug.Log("Victory UI setup completed.");
    }

    private static void ConfigureCanvasPrefab()
    {
        GameObject root = PrefabUtility.LoadPrefabContents(CanvasPath);
        try
        {
            Transform victory = FindChild(root.transform, "VictoryPanel");
            if (!victory)
                throw new System.InvalidOperationException("VictoryPanel is missing in Canvas prefab.");

            DeadScreenAnimationController wrongDeathController =
                victory.GetComponent<DeadScreenAnimationController>();
            if (wrongDeathController)
                Object.DestroyImmediate(wrongDeathController, true);

            WinScreenController win = root.GetComponent<WinScreenController>();
            if (!win)
                win = root.AddComponent<WinScreenController>();

            AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(
                AssetDatabase.GUIDToAssetPath(VictoryClipGuid));
            SerializedObject serialized = new(win);
            serialized.FindProperty("_winCanvas").objectReferenceValue = victory.gameObject;
            serialized.FindProperty("_animator").objectReferenceValue = victory.GetComponent<Animator>();
            serialized.FindProperty("_winClip").objectReferenceValue = clip;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            victory.gameObject.SetActive(false);

            PrefabUtility.SaveAsPrefabAsset(root, CanvasPath);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    private static void ConfigureFinalDoor()
    {
        GameObject root = PrefabUtility.LoadPrefabContents(Floor2PrefabPath);
        try
        {
            DoorInteractable[] doors = root.GetComponentsInChildren<DoorInteractable>(true);
            if (doors.Length == 0)
                throw new System.InvalidOperationException("No DoorInteractable found in Floor2 prefab.");

            DoorInteractable door = doors[0];
            SerializedObject serialized = new(door);
            serialized.FindProperty("_publishesGameClearOnOpen").boolValue = true;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            PrefabUtility.RecordPrefabInstancePropertyModifications(door);
            PrefabUtility.SaveAsPrefabAsset(root, Floor2PrefabPath);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    private static void LinkCanvasToGameplayScenes()
    {
        string[] scenePaths =
        {
            "Assets/Scenes/Floor1/Floor1.unity",
            "Assets/Scenes/Stair/Stair.unity",
            "Assets/Scenes/Floor2/Floor2.unity"
        };
        GameObject canvasAsset = AssetDatabase.LoadAssetAtPath<GameObject>(CanvasPath);
        string canvasGuid = AssetDatabase.AssetPathToGUID(CanvasPath);

        foreach (string scenePath in scenePaths)
        {
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            var legacyUi = new System.Collections.Generic.HashSet<GameObject>();
            foreach (GameObject sceneRoot in scene.GetRootGameObjects())
            {
                foreach (Transform child in sceneRoot.GetComponentsInChildren<Transform>(true))
                {
                    GameObject candidate = child.gameObject;
                    if ((candidate.name == "StaminaHUD" || candidate.name == "DeadScreenPanel") &&
                        !BelongsToCanvasPrefab(candidate))
                        legacyUi.Add(candidate);
                }
            }

            foreach (GameObject legacy in legacyUi)
            {
                Object.DestroyImmediate(legacy);
            }

            bool hasLinkedCanvas = scene.GetRootGameObjects()
                .SelectMany(x => x.GetComponentsInChildren<Transform>(true))
                .Any(x => PrefabUtility.GetCorrespondingObjectFromSource(x.gameObject) is GameObject source &&
                          AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(source)) == canvasGuid);

            if (!hasLinkedCanvas)
                PrefabUtility.InstantiatePrefab(canvasAsset, scene);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log($"Linked Canvas prefab to {scenePath}");
        }
    }

    private static Transform FindChild(Transform root, string objectName)
    {
        foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
            if (child.name == objectName)
                return child;
        return null;
    }

    private static bool BelongsToCanvasPrefab(GameObject gameObject)
    {
        GameObject instanceRoot = PrefabUtility.GetNearestPrefabInstanceRoot(gameObject);
        if (!instanceRoot)
            return false;

        GameObject source = PrefabUtility.GetCorrespondingObjectFromSource(instanceRoot);
        return source && AssetDatabase.GetAssetPath(source) == CanvasPath;
    }
}
