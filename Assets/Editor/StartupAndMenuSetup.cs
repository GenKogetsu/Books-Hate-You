#region Imports
using System;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
#endregion
#region StartupAndMenuSetup

public static class StartupAndMenuSetup
{
    #region Setup assets and constants
    private const string Sheet = "Assets/Textures/UI/Ui.png";
    private const string CanvasPrefab = "Assets/Prefabs/Ui/Canvas.prefab";
    private const string AnimationFolder = "Assets/Object/UI/Startup";

    [MenuItem("Tools/Setup Startup And Menu UI")]
    #endregion
    #region Game and menu setup
    public static void Apply()
    {
        var sprites = AssetDatabase.LoadAllAssetsAtPath(Sheet).OfType<Sprite>().ToDictionary(x => x.name);
        SetupGameCanvas(sprites);
        SetupMainMenu(sprites);
        SetupStartup(sprites);
        SetupFloor2Canvas(sprites);
        AssetDatabase.SaveAssets();
    }

    private static void SetupFloor2Canvas(System.Collections.Generic.Dictionary<string, Sprite> sprites)
    {
        var scene = EditorSceneManager.OpenScene("Assets/Scenes/Floor2/Floor2.unity", OpenSceneMode.Single);
        var all = scene.GetRootGameObjects().SelectMany(x => x.GetComponentsInChildren<Transform>(true)).ToArray();
        foreach (var name in new[] { "DeadScreenPanel", "VictoryPanel" })
        {
            var rect = (RectTransform)all.First(x => x.name == name);
            rect.anchorMin = new Vector2(0f, 0.5f);
            rect.anchorMax = new Vector2(1f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(0f, 469f);
            rect.localScale = Vector3.one;
            var image = rect.Find("Content/Image")?.GetComponent<Image>();
            if (!image) continue;
            image.sprite = sprites[name == "DeadScreenPanel" ? "Gameover UI" : "Victory - UI"];
            image.preserveAspect = false;
            PrefabUtility.RecordPrefabInstancePropertyModifications(rect);
            PrefabUtility.RecordPrefabInstancePropertyModifications(image);
        }
        foreach (var name in new[] { "RestartButton", "Back To Main Menu" })
        {
            var rect = (RectTransform)all.First(x => x.name == name);
            rect.sizeDelta = new Vector2(378f, 106f);
            rect.localScale = Vector3.one;
            var image = rect.GetComponent<Image>();
            image.sprite = sprites[name == "RestartButton" ? "Restart Game - Ui" : "Back to Main Menu Ui"];
            image.preserveAspect = false;
            PrefabUtility.RecordPrefabInstancePropertyModifications(rect);
            PrefabUtility.RecordPrefabInstancePropertyModifications(image);
        }
        EditorSceneManager.SaveScene(scene);
    }

    private static void SetupGameCanvas(System.Collections.Generic.Dictionary<string, Sprite> sprites)
    {
        var root = PrefabUtility.LoadPrefabContents(CanvasPrefab);
        try
        {
            foreach (var name in new[] { "DeadScreenPanel", "VictoryPanel" })
            {
                var panel = root.GetComponentsInChildren<Transform>(true).First(x => x.name == name);
                var rect = (RectTransform)panel;
                rect.anchorMin = new Vector2(0f, 0.5f);
                rect.anchorMax = new Vector2(1f, 0.5f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.anchoredPosition = Vector2.zero;
                rect.sizeDelta = new Vector2(0f, 469f);
                var background = panel.GetComponent<Image>() ?? panel.gameObject.AddComponent<Image>();
                background.sprite = null;
                background.type = Image.Type.Simple;
                background.raycastTarget = true;
                background.color = new Color(0f, 0f, 0f, 0.89411765f);
                var content = panel.Find("Content") as RectTransform;
                if (!content)
                {
                    content = new GameObject("Content", typeof(RectTransform)).GetComponent<RectTransform>();
                    content.SetParent(panel, false);
                }
                content.anchorMin = content.anchorMax = content.pivot = new Vector2(0.5f, 0.5f);
                content.anchoredPosition = Vector2.zero;
                content.sizeDelta = new Vector2(760f, 550f);
                var artwork = content.Find("Image") as RectTransform;
                if (!artwork)
                {
                    artwork = new GameObject("Image", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image))
                        .GetComponent<RectTransform>();
                    artwork.SetParent(content, false);
                }
                artwork.anchorMin = artwork.anchorMax = artwork.pivot = new Vector2(0.5f, 0.5f);
                artwork.anchoredPosition = Vector2.zero;
                artwork.sizeDelta = new Vector2(760f, 550f);
                var image = artwork.GetComponent<Image>();
                image.sprite = sprites[name == "DeadScreenPanel" ? "Gameover UI" : "Victory - UI"];
                image.type = Image.Type.Simple;
                image.preserveAspect = false;
                image.raycastTarget = false;
                image.color = Color.white;
                foreach (Transform child in panel.Cast<Transform>().ToArray())
                    if (child != content) child.SetParent(content, false);
                artwork.SetAsFirstSibling();
                var animator = panel.GetComponent<Animator>() ?? panel.gameObject.AddComponent<Animator>();
                if (!panel.GetComponent<CanvasGroup>()) panel.gameObject.AddComponent<CanvasGroup>();
                animator.runtimeAnimatorController = MakePanelAnimator();
                foreach (var text in panel.GetComponentsInChildren<TMP_Text>(true))
                    text.gameObject.SetActive(false);
            }

            foreach (var name in new[] { "RestartButton", "Back To Main Menu" })
            {
                var button = root.GetComponentsInChildren<Transform>(true).First(x => x.name == name);
                var image = button.GetComponent<Image>();
                image.sprite = sprites[name == "RestartButton" ? "Restart Game - Ui" : "Back to Main Menu Ui"];
                image.type = Image.Type.Simple;
                image.preserveAspect = false;
                image.color = Color.white;
                ((RectTransform)button).sizeDelta = new Vector2(378f, 106f);
                foreach (var text in button.GetComponentsInChildren<TMP_Text>(true))
                    text.gameObject.SetActive(false);
            }

            PrefabUtility.SaveAsPrefabAsset(root, CanvasPrefab);
        }
        finally { PrefabUtility.UnloadPrefabContents(root); }
    }

    private static AnimatorController MakePanelAnimator()
    {
        System.IO.Directory.CreateDirectory(AnimationFolder);
        const string sharedName = "DeadScreenPanel";
        var path = AnimationFolder + "/" + sharedName + ".controller";
        var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(path);
        if (!controller) controller = AnimatorController.CreateAnimatorControllerAtPath(path);
        var clipPath = AnimationFolder + "/" + sharedName + "Reveal.anim";
        var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath);
        if (!clip)
        {
            clip = new AnimationClip { name = "Reveal" };
            AssetDatabase.CreateAsset(clip, clipPath);
        }
        AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve("", typeof(CanvasGroup), "m_Alpha"),
            new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1.32f, 1f)));
        AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve("", typeof(RectTransform), "m_AnchoredPosition.x"), null);
        AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve("", typeof(RectTransform), "m_AnchoredPosition.y"),
            new AnimationCurve(new Keyframe(0f, 900f), new Keyframe(1.42f, 0f)));
        var stateMachine = controller.layers[0].stateMachine;
        var revealState = stateMachine.states.Select(x => x.state).FirstOrDefault(x => x.name == "Reveal")
            ?? stateMachine.AddState("Reveal");
        revealState.motion = clip;
        var hiddenState = stateMachine.states.Select(x => x.state).FirstOrDefault(x => x.name == "Hidden")
            ?? stateMachine.AddState("Hidden");
        var hiddenPath = AnimationFolder + "/" + sharedName + "Hidden.anim";
        var hiddenClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(hiddenPath);
        if (!hiddenClip)
        {
            hiddenClip = new AnimationClip { name = "Hidden" };
            AssetDatabase.CreateAsset(hiddenClip, hiddenPath);
        }
        AnimationUtility.SetEditorCurve(hiddenClip, EditorCurveBinding.FloatCurve("", typeof(CanvasGroup), "m_Alpha"),
            AnimationCurve.Constant(0f, 0.02f, 0f));
        AnimationUtility.SetEditorCurve(hiddenClip, EditorCurveBinding.FloatCurve("", typeof(RectTransform), "m_AnchoredPosition.x"), null);
        AnimationUtility.SetEditorCurve(hiddenClip, EditorCurveBinding.FloatCurve("", typeof(RectTransform), "m_AnchoredPosition.y"),
            AnimationCurve.Constant(0f, 0.02f, 900f));
        hiddenState.motion = hiddenClip;
        stateMachine.defaultState = hiddenState;
        return controller;
    }

    private static void SetupMainMenu(System.Collections.Generic.Dictionary<string, Sprite> sprites)
    {
        var scene = EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity", OpenSceneMode.Single);
        var roots = scene.GetRootGameObjects();
        var menus = roots.SelectMany(x => x.GetComponentsInChildren<MainMenuController>(true)).ToArray();
        foreach (var menu in menus)
        {
            var logo = menu.transform.Find("Game Logo");
            if (logo)
            {
                var text = logo.GetComponent<TMP_Text>();
                if (text) text.enabled = false;
                var image = logo.GetComponent<Image>() ?? logo.Find("Logo Image")?.GetComponent<Image>();
                if (!image)
                {
                    var imageObject = new GameObject("Logo Image", typeof(RectTransform), typeof(Image));
                    imageObject.transform.SetParent(logo, false);
                    image = imageObject.GetComponent<Image>();
                    var imageRect = (RectTransform)imageObject.transform;
                    imageRect.anchorMin = Vector2.zero;
                    imageRect.anchorMax = Vector2.one;
                    imageRect.offsetMin = imageRect.offsetMax = Vector2.zero;
                }
                image.sprite = sprites["Logo - UI"];
                image.preserveAspect = true;
                image.raycastTarget = false;
                ((RectTransform)logo).sizeDelta = new Vector2(523f, 475f);
            }
            var prompt = menu.transform.Find("Any Key To Start")?.GetComponent<TMP_Text>();
            if (prompt) prompt.text = "PRESS ANY KEY TO START";
        }

        var clip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Sound/BMG/nojisuma-maze-117144.mp3");
        var bgm = roots.SelectMany(x => x.GetComponentsInChildren<Transform>(true)).FirstOrDefault(x => x.name == "Main Menu BGM");
        if (!bgm)
        {
            var obj = new GameObject("Main Menu BGM", typeof(AudioSource));
            SceneManager.MoveGameObjectToScene(obj, scene);
            bgm = obj.transform;
        }
        var audio = bgm.GetComponent<AudioSource>();
        if (!audio) audio = bgm.gameObject.AddComponent<AudioSource>();
        audio.clip = clip;
        audio.loop = true;
        audio.playOnAwake = true;
        audio.spatialBlend = 0f;
        EditorSceneManager.SaveScene(scene);
    }

    [MenuItem("Tools/Refresh Startup Logo Animation")]
    #endregion
    #region Startup setup
    public static void RefreshStartup()
    {
        var sprites = AssetDatabase.LoadAllAssetsAtPath(Sheet).OfType<Sprite>().ToDictionary(x => x.name);
        SetupStartup(sprites);
        AssetDatabase.SaveAssets();
    }

    private static void SetupStartup(System.Collections.Generic.Dictionary<string, Sprite> sprites)
    {
        var path = "Assets/Scenes/Startup.unity";
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var canvasObject = new GameObject("Startup Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        var background = new GameObject("Background", typeof(RectTransform), typeof(Image));
        background.transform.SetParent(canvasObject.transform, false);
        var bgRect = (RectTransform)background.transform;
        bgRect.anchorMin = Vector2.zero; bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = bgRect.offsetMax = Vector2.zero;
        background.GetComponent<Image>().color = Color.black;
        var logo = new GameObject("Game Logo", typeof(RectTransform), typeof(Image), typeof(CanvasGroup), typeof(Animator));
        logo.transform.SetParent(canvasObject.transform, false);
        var logoRect = (RectTransform)logo.transform;
        logoRect.sizeDelta = new Vector2(523f, 475f);
        var image = logo.GetComponent<Image>();
        image.sprite = sprites["Logo - UI"];
        image.preserveAspect = true;
        image.raycastTarget = false;
        var controllerPath = AnimationFolder + "/StartupLogo.controller";
        var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);
        if (!controller) controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
        var clipPath = AnimationFolder + "/LogoReveal.anim";
        var reveal = AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath);
        if (!reveal)
        {
            reveal = new AnimationClip { name = "LogoReveal" };
            AssetDatabase.CreateAsset(reveal, clipPath);
        }
        foreach (var axis in new[] { "x", "y", "z" })
            AnimationUtility.SetEditorCurve(reveal, EditorCurveBinding.FloatCurve("", typeof(RectTransform), "m_LocalScale." + axis),
                new AnimationCurve(new Keyframe(0f, 0.8f), new Keyframe(2f, 1f)));
        AnimationUtility.SetEditorCurve(reveal, EditorCurveBinding.FloatCurve("", typeof(CanvasGroup), "m_Alpha"),
            AnimationCurve.Constant(0f, 2f, 1f));
        AnimationUtility.SetAnimationEvents(reveal, new[] { new AnimationEvent { time = 2f, functionName = "OnClick" } });
        var logoStateMachine = controller.layers[0].stateMachine;
        var logoState = logoStateMachine.states.Select(x => x.state).FirstOrDefault(x => x.name == "LogoReveal")
            ?? logoStateMachine.AddState("LogoReveal");
        logoState.motion = reveal;
        logoStateMachine.defaultState = logoState;
        logo.GetComponent<Animator>().runtimeAnimatorController = controller;
        var loader = logo.AddComponent<LoadSceneOnClick>();
        var serialized = new SerializedObject(loader);
        serialized.FindProperty("_sceneIndex").intValue = 1;
        serialized.ApplyModifiedPropertiesWithoutUndo();
        var effect = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Resources/KogetsuLibrary-main/Runtime/Systems/Scene/SceneEffect/BasicSceneEffectController.prefab");
        if (!effect) throw new InvalidOperationException("BasicSceneEffectController prefab missing");
        PrefabUtility.InstantiatePrefab(effect, scene);
        EditorSceneManager.SaveScene(scene, path);
        var scenes = EditorBuildSettings.scenes.Where(x => x.path != path).ToList();
        scenes.Insert(0, new EditorBuildSettingsScene(path, true));
        EditorBuildSettings.scenes = scenes.ToArray();
    }
    #endregion
}
#endregion
