using UnityEngine;
using UnityEngine.InputSystem;
using Kogetsu.Library.Core;

public sealed class MainMenuController : MonoBehaviour
{
    [SerializeField] private int targetSceneBuildIndex = 1;
    private bool isStarting;

    private void Update()
    {
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

        if (isStarting || (!anyKeyboardKey && !anyMouseButton && !anyGamepadButton))
            return;

        isStarting = true;
        if (BasicSceneEffectController.Instance != null)
            BasicSceneEffectController.Instance.LoadScene(targetSceneBuildIndex);
        else
            Debug.LogError("BasicSceneEffectController is missing from the Main Menu scene.");
    }
}
