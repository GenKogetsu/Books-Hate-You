#region Imports
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
#endregion
#region MainMenuBuilder

public static class MainMenuBuilder
{
    #region Fields and configuration
    private const string ScenePath = "Assets/Scenes/MainMenu.unity";
    private const string SingletonPrefabPath = "Assets/Prefabs/System/Singleton group.prefab";

    #endregion
    #region Public API
    public static void Build()
    {
        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        var singletonPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(SingletonPrefabPath);
        if (singletonPrefab == null)
            throw new System.Exception("Singleton group prefab not found: " + SingletonPrefabPath);
        Object.Instantiate(singletonPrefab);

        var camera = GameObject.FindWithTag("MainCamera");
        if (camera == null)
            camera = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
        if (camera.GetComponent<Camera>() == null)
            camera.AddComponent<Camera>();
        if (camera.GetComponent<AudioListener>() == null)
            camera.AddComponent<AudioListener>();
        camera.tag = "MainCamera";
        camera.GetComponent<Camera>().clearFlags = CameraClearFlags.SolidColor;
        camera.GetComponent<Camera>().backgroundColor = new Color(0.025f, 0.02f, 0.04f, 1f);
        camera.GetComponent<Camera>().orthographic = true;
        camera.GetComponent<Camera>().orthographicSize = 5f;
        camera.transform.position = new Vector3(0, 0, -10);

        var canvasObject = new GameObject("Main Menu Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        var background = CreateImage("Background", canvasObject.transform);
        Stretch(background.rectTransform);
        background.color = new Color(0.025f, 0.02f, 0.04f, 1f);

        var logo = CreateText("Game Logo", canvasObject.transform, "BOOKS HATE YOU", 92, FontStyles.Bold);
        Center(logo.rectTransform, new Vector2(0, 135), new Vector2(1200, 150));
        logo.color = new Color(0.95f, 0.83f, 0.36f, 1f);

        var subtitle = CreateText("Subtitle", canvasObject.transform, "A LIBRARY OF NIGHTMARES", 25, FontStyles.Normal);
        Center(subtitle.rectTransform, new Vector2(0, 45), new Vector2(1000, 60));
        subtitle.color = new Color(0.8f, 0.78f, 0.86f, 1f);

        var prompt = CreateText("Any Key To Start", canvasObject.transform, "ANY KEY TO START", 38, FontStyles.Bold);
        Center(prompt.rectTransform, new Vector2(0, -170), new Vector2(1000, 90));
        prompt.color = Color.white;

        var controller = canvasObject.AddComponent<MainMenuController>();
        controller.name = "Main Menu Controller";

        var eventSystem = Object.FindAnyObjectByType<EventSystem>();
        if (eventSystem == null)
        {
            var eventSystemObject = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            eventSystem = eventSystemObject.GetComponent<EventSystem>();
        }
        else
        {
            var oldInputModule = eventSystem.GetComponent<StandaloneInputModule>();
            if (oldInputModule != null)
                Object.DestroyImmediate(oldInputModule);
            if (eventSystem.GetComponent<InputSystemUIInputModule>() == null)
                eventSystem.gameObject.AddComponent<InputSystemUIInputModule>();
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        Debug.Log("Main Menu built successfully with Canvas, TMP, Singleton group, and scene transition to build index 1.");
    }

    #endregion
    #region Internal helpers
    private static Image CreateImage(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        go.GetComponent<Image>().raycastTarget = false;
        return go.GetComponent<Image>();
    }

    private static TextMeshProUGUI CreateText(string name, Transform parent, string text, float size, FontStyles style)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        var label = go.GetComponent<TextMeshProUGUI>();
        label.text = text;
        label.fontSize = size;
        label.fontStyle = style;
        label.alignment = TextAlignmentOptions.Center;
        label.textWrappingMode = TextWrappingModes.NoWrap;
        label.raycastTarget = false;
        return label;
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private static void Center(RectTransform rect, Vector2 position, Vector2 size)
    {
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
    }
    #endregion
}
#endregion
