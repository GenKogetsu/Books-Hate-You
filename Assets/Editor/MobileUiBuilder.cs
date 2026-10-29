#region Imports
using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem.OnScreen;
using UnityEngine.SceneManagement;
#endregion
#region MobileUiBuilder

/// <summary>Creates the shared phone controls and menu start button without replacing existing UI.</summary>
public static class MobileUiBuilder
{
    #region Fields and configuration
    private const string CanvasPath = "Assets/Prefabs/Ui/Canvas.prefab";
    private const string MainMenuPath = "Assets/Scenes/MainMenu.unity";
    private const string RoundedSpritePath = "Assets/Art/UI/MobileRoundedPanel.png";
    private static readonly Color Outline = new(0.65f, 0.84f, 1f, 0.36f);
    private static readonly Color Face = new(0.025f, 0.055f, 0.09f, 0.42f);

    [MenuItem("Tools/Mobile/Build Touch UI")]
    #endregion
    #region Public API
    public static void Apply()
    {
        Sprite rounded = EnsureRoundedSprite();
        BuildGameplayCanvas(rounded);
        BuildMainMenu(rounded);
        AssetDatabase.SaveAssets();
        Debug.Log("Mobile touch UI built for shared gameplay Canvas and MainMenu.");
    }

    #endregion
    #region Internal helpers
    private static Sprite EnsureRoundedSprite()
    {
        string diskPath = Path.Combine(Application.dataPath, "Art", "UI", "MobileRoundedPanel.png");
        if (!File.Exists(diskPath))
            throw new InvalidOperationException("The existing rounded UI sprite is missing.");

        AssetDatabase.ImportAsset(RoundedSpritePath);
        if (AssetImporter.GetAtPath(RoundedSpritePath) is TextureImporter importer)
        {
            if (importer.textureType != TextureImporterType.Sprite || importer.spriteBorder != new Vector4(20f, 20f, 20f, 20f))
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.spriteBorder = new Vector4(20f, 20f, 20f, 20f);
                importer.alphaIsTransparency = true;
                importer.mipmapEnabled = false;
                importer.SaveAndReimport();
            }
        }
        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(RoundedSpritePath);
        if (!sprite) throw new InvalidOperationException("Unable to import mobile rounded UI sprite.");
        return sprite;
    }

    private static void BuildGameplayCanvas(Sprite rounded)
    {
        GameObject root = PrefabUtility.LoadPrefabContents(CanvasPath);
        try
        {
            Transform canvas = root.transform;
            RectTransform quest = RequireRect(canvas, "MainQuest");
            RectTransform key = RequireRect(canvas, "KeyUI");
            RectTransform death = RequireRect(canvas, "DeathCount");
            RectTransform stamina = RequireRect(canvas, "StaminaHUD");
            RectTransform toast = RequireRect(canvas, "AchievementToast");
            RectTransform restart = RequireRect(canvas, "DeadScreenPanel/Content/RestartButton");
            RectTransform menu = RequireRect(canvas, "VictoryPanel/Content/Back To Main Menu");

            UnityEngine.UI.Image keyBacking = EnsureImage(EnsureRect(key, "MobileKeyBacking"), rounded,
                new Color(0.035f, 0.055f, 0.09f, 0.62f), false);
            Stretch((RectTransform)keyBacking.transform);
            keyBacking.transform.SetAsFirstSibling();
            keyBacking.gameObject.SetActive(false);

            RectTransform deathBackingRect = EnsureRect(canvas, "MobileDeathBacking");
            UnityEngine.UI.Image deathBacking = EnsureImage(deathBackingRect, rounded,
                new Color(0.035f, 0.055f, 0.09f, 0.6f), false);
            EnsureComponent<CanvasGroup>(deathBacking.gameObject);
            deathBackingRect.SetSiblingIndex(death.GetSiblingIndex());
            deathBacking.gameObject.SetActive(false);

            RectTransform controls = EnsureRect(canvas, "MobileControls");
            Stretch(controls);
            controls.SetAsLastSibling();
            RectTransform safeArea = EnsureRect(controls, "SafeArea");
            Stretch(safeArea);
            EnsureComponent<MobileSafeAreaFitter>(safeArea.gameObject);

            CreateStick(safeArea, "Move", new Vector2(166f, 162f), "<Gamepad>/leftStick", rounded);
            CreateStick(safeArea, "Look", new Vector2(-328f, 162f), "<Gamepad>/rightStick", rounded, true);

            CreateOnScreenButton(safeArea, "Jump", "JUMP", "<Gamepad>/buttonSouth",
                new Vector2(-102f, 194f), new Vector2(150f, 88f), rounded, true);
            CreateOnScreenButton(safeArea, "Run", "RUN", "<Gamepad>/leftStickPress",
                new Vector2(-102f, 94f), new Vector2(150f, 88f), rounded, true);
            CreateOnScreenButton(safeArea, "Crouch", "CROUCH", "<Gamepad>/buttonEast",
                new Vector2(-102f, 294f), new Vector2(150f, 88f), rounded, true);
            GameObject interact = CreateOnScreenButton(safeArea, "Interact", "USE", "<Gamepad>/buttonNorth",
                new Vector2(-102f, 394f), new Vector2(150f, 88f), rounded, true);

            RectTransform cameraRow = EnsureRect(safeArea, "CameraRow");
            Anchor(cameraRow, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -54f), new Vector2(660f, 76f));
            CreateOnScreenButton(cameraRow, "CycleCamera", "VIEW", "<Gamepad>/rightShoulder",
                new Vector2(-228f, 0f), new Vector2(112f, 72f), rounded, whitePlaceholder: true);
            CreateOnScreenButton(cameraRow, "HoldTop", "TOP", "<Gamepad>/leftShoulder",
                new Vector2(-106f, 0f), new Vector2(112f, 72f), rounded, whitePlaceholder: true);
            CreateOnScreenButton(cameraRow, "HoldBack", "BACK", "<Gamepad>/rightTrigger",
                new Vector2(16f, 0f), new Vector2(112f, 72f), rounded, whitePlaceholder: true);
            Transform oldZoomButtons = cameraRow.Find("ZoomButtons");
            if (oldZoomButtons) UnityEngine.Object.DestroyImmediate(oldZoomButtons.gameObject);
            RectTransform desktopGuide = CreateDesktopGuide(canvas, rounded);

            MobileControlsOverlay overlay = EnsureComponent<MobileControlsOverlay>(root);
            overlay.Configure(controls.gameObject, interact, desktopGuide.gameObject,
                new[] { quest.gameObject, key.gameObject, death.gameObject,
                    stamina.gameObject, toast.gameObject });

            MobileHudLayout hud = EnsureComponent<MobileHudLayout>(root);
            hud.Configure(quest, key, death, deathBackingRect, stamina, toast,
                quest.GetComponent<UnityEngine.UI.Image>(), toast.GetComponent<UnityEngine.UI.Image>(),
                keyBacking, deathBacking, restart.GetComponent<UnityEngine.UI.Image>(),
                menu.GetComponent<UnityEngine.UI.Image>(), rounded);

            PrefabUtility.SaveAsPrefabAsset(root, CanvasPath);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    private static void BuildMainMenu(Sprite rounded)
    {
        Scene scene = EditorSceneManager.OpenScene(MainMenuPath, OpenSceneMode.Single);
        var menus = new List<(MainMenuController controller, Canvas canvas, int rootOrder)>();
        int rootOrder = 0;
        foreach (GameObject sceneRoot in scene.GetRootGameObjects())
        {
            foreach (MainMenuController controller in sceneRoot.GetComponentsInChildren<MainMenuController>(true))
            {
                Canvas canvas = controller.GetComponent<Canvas>();
                if (controller.gameObject.activeInHierarchy && canvas && canvas.isActiveAndEnabled)
                    menus.Add((controller, canvas, rootOrder));
            }
            ++rootOrder;
        }

        if (menus.Count == 0)
            throw new InvalidOperationException("No active MainMenuController Canvas was found in MainMenu.unity.");

        // Screen Space Overlay canvases with the same sorting order render in scene order.
        // Keep one touch button on the visible top canvas; duplicate buttons compound alpha.
        var topMenu = menus[0];
        foreach (var candidate in menus)
            if (candidate.canvas.sortingOrder > topMenu.canvas.sortingOrder ||
                (candidate.canvas.sortingOrder == topMenu.canvas.sortingOrder &&
                 candidate.rootOrder > topMenu.rootOrder))
                topMenu = candidate;

        foreach (var entry in menus)
        {
            MainMenuController controller = entry.controller;
            RectTransform menu = controller.transform as RectTransform;
            if (!menu) continue;

            if (controller != topMenu.controller)
            {
                Transform oldSafeArea = menu.Find("MobileMenuSafeArea");
                if (oldSafeArea) UnityEngine.Object.DestroyImmediate(oldSafeArea.gameObject);
                MobileMenuStartButton oldStarter = controller.GetComponent<MobileMenuStartButton>();
                if (oldStarter) UnityEngine.Object.DestroyImmediate(oldStarter);
                continue;
            }

            RectTransform safe = EnsureRect(menu, "MobileMenuSafeArea");
            Stretch(safe);
            safe.SetAsLastSibling();
            EnsureComponent<MobileSafeAreaFitter>(safe.gameObject);

            RectTransform buttonRect = EnsureRect(safe, "MobileTouchStart");
            Anchor(buttonRect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, -170f), new Vector2(420f, 96f));
            UnityEngine.UI.Image image = EnsureImage(buttonRect, rounded, Outline, true);
            UnityEngine.UI.Button button = EnsureComponent<UnityEngine.UI.Button>(image.gameObject);
            button.transition = UnityEngine.UI.Selectable.Transition.ColorTint;
            UnityEngine.UI.ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1f, 1f, 1f, 1f);
            colors.pressedColor = new Color(0.7f, 0.83f, 1f, 1f);
            button.colors = colors;
            AddFaceAndLabel(buttonRect, "TAP TO START", rounded, 36f);

            TMP_Text desktopPrompt = menu.Find("Any Key To Start")?.GetComponent<TMP_Text>();
            MobileMenuStartButton starter = EnsureComponent<MobileMenuStartButton>(controller.gameObject);
            starter.Configure(safe.gameObject, button, controller, desktopPrompt);
        }
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }

    private static void CreateStick(Transform parent, string name, Vector2 position,
        string path, Sprite rounded, bool fromRight = false)
    {
        Vector2 anchor = fromRight ? new Vector2(1f, 0f) : Vector2.zero;
        RectTransform baseRect = EnsureRect(parent, name + "Pad");
        Anchor(baseRect, anchor, new Vector2(0.5f, 0.5f), position, new Vector2(226f, 226f));
        EnsureImage(baseRect, rounded, new Color(0.58f, 0.78f, 1f, 0.2f), false);

        RectTransform title = EnsureRect(baseRect, "Title");
        Anchor(title, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0f, -15f), new Vector2(160f, 32f));
        ConfigureText(title, name.ToUpperInvariant(), 24f);

        RectTransform stickRect = EnsureRect(baseRect, "Stick");
        Anchor(stickRect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            Vector2.zero, new Vector2(170f, 170f));
        EnsureImage(stickRect, rounded, new Color(1f, 1f, 1f, 0.01f), true);
        OnScreenStick stick = EnsureComponent<OnScreenStick>(stickRect.gameObject);
        stick.controlPath = path;
        stick.movementRange = 63f;
        stick.useIsolatedInputActions = true;

        RectTransform thumb = EnsureRect(stickRect, "Thumb");
        Anchor(thumb, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            Vector2.zero, new Vector2(82f, 82f));
        EnsureImage(thumb, rounded, new Color(0.8f, 0.91f, 1f, 0.68f), false);
    }

    private static GameObject CreateOnScreenButton(Transform parent, string name, string label,
        string path, Vector2 position, Vector2 size, Sprite rounded, bool fromRight = false,
        bool whitePlaceholder = false)
    {
        RectTransform rect = EnsureRect(parent, name);
        Vector2 anchor = fromRight ? new Vector2(1f, 0f) : new Vector2(0.5f, 0.5f);
        Anchor(rect, anchor, new Vector2(0.5f, 0.5f), position, size);
        UnityEngine.UI.Image image = whitePlaceholder
            ? EnsureWhitePlaceholder(rect, true)
            : EnsureImage(rect, rounded, Outline, true);
        OnScreenButton onScreen = EnsureComponent<OnScreenButton>(image.gameObject);
        onScreen.controlPath = path;
        if (whitePlaceholder)
            AddPlaceholderLabel(rect, label, size.x > 120f ? 28f : 24f);
        else
            AddFaceAndLabel(rect, label, rounded, size.x > 120f ? 28f : 24f);
        return image.gameObject;
    }

    private static RectTransform CreateDesktopGuide(Transform canvas, Sprite rounded)
    {
        RectTransform guide = EnsureRect(canvas, "DesktopNavigationBar");
        Anchor(guide, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(0f, 18f), new Vector2(1280f, 100f));
        EnsureImage(guide, rounded, new Color(0.02f, 0.04f, 0.07f, 0.65f), false);
        guide.SetAsLastSibling();
        (string key, string purpose)[] entries =
        {
            ("WASD", "Move"), ("MOUSE", "Look"), ("SHIFT", "Run"),
            ("C", "Crouch"), ("SPACE", "Jump"), ("F", "Use"),
            ("1 2 3", "Views"), ("TAB", "Next view"), ("Q", "Top peek"),
            ("E", "Back peek")
        };
        Transform oldZoomControl = guide.Find("ZoomControl");
        if (oldZoomControl) UnityEngine.Object.DestroyImmediate(oldZoomControl.gameObject);
        for (int i = 0; i < entries.Length; i++)
        {
            int row = i / 6;
            int column = i % 6;
            RectTransform cell = EnsureRect(guide, "Guide" + i);
            Anchor(cell, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(-535f + column * 214f, 24f - row * 49f), new Vector2(204f, 42f));
            RectTransform key = EnsureRect(cell, "Key");
            Anchor(key, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
                Vector2.zero, new Vector2(82f, 36f));
            EnsureWhitePlaceholder(key, false);
            AddPlaceholderLabel(key, entries[i].key, 20f);
            RectTransform purpose = EnsureRect(cell, "Purpose");
            Anchor(purpose, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
                new Vector2(88f, 0f), new Vector2(116f, 38f));
            ConfigureText(purpose, entries[i].purpose, 18f);
            purpose.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.MidlineLeft;
        }
        return guide;
    }

    private static UnityEngine.UI.Image EnsureWhitePlaceholder(RectTransform rect, bool raycast)
    {
        UnityEngine.UI.Image image = EnsureComponent<UnityEngine.UI.Image>(rect.gameObject);
        image.sprite = null;
        image.type = UnityEngine.UI.Image.Type.Simple;
        image.color = Color.white;
        image.raycastTarget = raycast;
        return image;
    }

    private static void AddPlaceholderLabel(RectTransform parent, string caption, float fontSize)
    {
        Transform oldFace = parent.Find("Face");
        if (oldFace) UnityEngine.Object.DestroyImmediate(oldFace.gameObject);
        RectTransform label = EnsureRect(parent, "Label");
        Stretch(label, 2f);
        ConfigureText(label, caption, fontSize);
        label.GetComponent<TextMeshProUGUI>().color = new Color(0.05f, 0.08f, 0.12f);
    }

    private static void AddFaceAndLabel(RectTransform parent, string caption, Sprite rounded, float fontSize)
    {
        RectTransform faceRect = EnsureRect(parent, "Face");
        Stretch(faceRect, 4f);
        EnsureImage(faceRect, rounded, Face, false);

        RectTransform labelRect = EnsureRect(parent, "Label");
        Stretch(labelRect, 4f);
        ConfigureText(labelRect, caption, fontSize);
    }

    private static void ConfigureText(RectTransform rect, string value, float size)
    {
        TextMeshProUGUI label = EnsureComponent<TextMeshProUGUI>(rect.gameObject);
        label.text = value;
        label.font = TMP_Settings.defaultFontAsset;
        label.fontSize = size;
        label.fontStyle = FontStyles.Bold;
        label.alignment = TextAlignmentOptions.Center;
        label.color = Color.white;
        label.enableAutoSizing = true;
        label.fontSizeMin = 16f;
        label.fontSizeMax = size;
        label.raycastTarget = false;
    }

    private static UnityEngine.UI.Image EnsureImage(RectTransform rect, Sprite sprite, Color color, bool raycast)
    {
        UnityEngine.UI.Image image = EnsureComponent<UnityEngine.UI.Image>(rect.gameObject);
        image.sprite = sprite;
        image.type = UnityEngine.UI.Image.Type.Sliced;
        image.color = color;
        image.raycastTarget = raycast;
        return image;
    }

    private static RectTransform RequireRect(Transform root, string path)
    {
        Transform found = root.Find(path);
        if (!found || found is not RectTransform rect)
            throw new InvalidOperationException($"Missing UI RectTransform: {path}");
        return rect;
    }

    private static RectTransform EnsureRect(Transform parent, string name)
    {
        Transform child = parent.Find(name);
        if (child)
        {
            if (child is RectTransform existing) return existing;
            throw new InvalidOperationException($"UI object {name} exists without RectTransform.");
        }
        var go = new GameObject(name, typeof(RectTransform));
        go.layer = LayerMask.NameToLayer("UI");
        go.transform.SetParent(parent, false);
        return (RectTransform)go.transform;
    }

    private static T EnsureComponent<T>(GameObject target) where T : Component
    {
        T component = target.GetComponent<T>();
        return component ? component : target.AddComponent<T>();
    }

    private static void Stretch(RectTransform rect, float inset = 0f)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.offsetMin = new Vector2(inset, inset);
        rect.offsetMax = new Vector2(-inset, -inset);
    }

    private static void Anchor(RectTransform rect, Vector2 anchor, Vector2 pivot,
        Vector2 position, Vector2 size)
    {
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = pivot;
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
    }
    #endregion
}
#endregion
