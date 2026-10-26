using UnityEngine;
using UnityEngine.InputSystem;
using Kogetsu.Library.Core;

public sealed class MainMenuController : MonoBehaviour
{
    [SerializeField] private int targetSceneBuildIndex = 1;
    private static MainMenuController activeMenuInstance;
    private static bool sceneTransitionStarted;
    private bool isStarting;

    private void Awake()
    {
        if (activeMenuInstance != null &&
            activeMenuInstance.gameObject.scene == gameObject.scene)
            return;

        activeMenuInstance = this;
        sceneTransitionStarted = false;
    }

    private void Update()
    {
#if UNITY_ANDROID || UNITY_IOS
        bool anyTouch = Touchscreen.current != null &&
                        Touchscreen.current.primaryTouch.press.wasPressedThisFrame;
#else
        const bool anyTouch = false;
#endif
        bool anyKeyboardKey = Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame;
        bool anyMouseButton = Mouse.current != null &&
                              (Mouse.current.leftButton.wasPressedThisFrame ||
                               Mouse.current.rightButton.wasPressedThisFrame ||
                               Mouse.current.middleButton.wasPressedThisFrame ||
                               Mouse.current.forwardButton.wasPressedThisFrame ||
                               Mouse.current.backButton.wasPressedThisFrame);
        bool anyGamepadButton = Gamepad.current != null && Gamepad.current.wasUpdatedThisFrame &&
                                (Gamepad.current.buttonSouth.wasPressedThisFrame ||
                                 Gamepad.current.buttonNorth.wasPressedThisFrame ||
                                 Gamepad.current.buttonEast.wasPressedThisFrame ||
                                 Gamepad.current.buttonWest.wasPressedThisFrame ||
                                 Gamepad.current.startButton.wasPressedThisFrame);

        if (!anyTouch && !anyKeyboardKey && !anyMouseButton && !anyGamepadButton)
            return;

        StartGame();
    }

    public void StartGame()
    {
        if (isStarting || sceneTransitionStarted) return;
        if (BasicSceneEffectController.Instance == null)
        {
            Debug.LogError("BasicSceneEffectController is missing from the Main Menu scene.");
            return;
        }

        bool accepted = BasicSceneEffectController.Instance.TryLoadSceneWithEvent(
            targetSceneBuildIndex, ResetForNewGame);
        if (accepted)
        {
            isStarting = true;
            sceneTransitionStarted = true;
        }
    }

    private static void ResetForNewGame()
    {
        PlayerRunSession.ResetForNewGame();
        foreach (PlayerSceneWarpState warp in Resources.FindObjectsOfTypeAll<PlayerSceneWarpState>())
            if (warp) warp.ClearPendingTransform();
        if (TimeManager.Instance) TimeManager.Instance.ResetSession();
    }
}
