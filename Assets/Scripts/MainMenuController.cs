#region Imports
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using Kogetsu.Library.Core;
#endregion
#region MainMenuController

public sealed class MainMenuController : MonoBehaviour
{
    [SerializeField] private string targetSceneName = "Floor1";
    [SerializeField] private TMP_Text _anyKeyPrompt;
    private static MainMenuController activeMenuInstance;
    private static bool sceneTransitionStarted;
    private bool isStarting;
    private Coroutine _promptRoutine;

    private void Awake()
    {
        if (activeMenuInstance != null &&
            activeMenuInstance.gameObject.scene == gameObject.scene)
            return;

        activeMenuInstance = this;
        sceneTransitionStarted = false;
        if (!_anyKeyPrompt)
            _anyKeyPrompt = transform.Find("Any Key To Start")?.GetComponent<TMP_Text>();
    }

    private void OnEnable()
    {
        if (_anyKeyPrompt) _promptRoutine = StartCoroutine(PulsePrompt());
    }

    private void OnDisable()
    {
        if (_promptRoutine != null) StopCoroutine(_promptRoutine);
        _promptRoutine = null;
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
            targetSceneName, ResetForNewGame);
        if (!accepted) return;
        isStarting = true;
        sceneTransitionStarted = true;
        if (_promptRoutine != null) StopCoroutine(_promptRoutine);
        _promptRoutine = StartCoroutine(FadePrompt(0f, 0.5f));
    }

    private IEnumerator PulsePrompt()
    {
        while (true)
        {
            yield return FadePrompt(0f, 2f);
            yield return FadePrompt(1f, 2f);
        }
    }

    private IEnumerator FadePrompt(float target, float duration)
    {
        float start = _anyKeyPrompt.alpha;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            // Faster at both ends and slower around the middle.
            t += 0.65f / (2f * Mathf.PI) * Mathf.Sin(2f * Mathf.PI * t);
            _anyKeyPrompt.alpha = Mathf.Lerp(start, target, t);
            yield return null;
        }
        _anyKeyPrompt.alpha = target;
    }

    private static void ResetForNewGame()
    {
        PlayerRunSession.ResetForNewGame();
        foreach (PlayerSceneWarpState warp in Resources.FindObjectsOfTypeAll<PlayerSceneWarpState>())
            if (warp) warp.ClearPendingTransform();
        if (TimeManager.Instance) TimeManager.Instance.ResetSession();
    }
}
#endregion
