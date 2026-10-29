#region Imports
using TMPro;
using UnityEngine;
#endregion
#region MobileMenuStartButton

public sealed class MobileMenuStartButton : MonoBehaviour
{
    [SerializeField] private GameObject _buttonRoot;
    [SerializeField] private UnityEngine.UI.Button _button;
    [SerializeField] private MainMenuController _controller;
    [SerializeField] private TMP_Text _desktopPrompt;

    public void Configure(GameObject buttonRoot, UnityEngine.UI.Button button,
        MainMenuController controller, TMP_Text desktopPrompt)
    {
        _buttonRoot = buttonRoot;
        _button = button;
        _controller = controller;
        _desktopPrompt = desktopPrompt;
    }

    private void OnEnable()
    {
        if (_button) _button.onClick.AddListener(StartGame);
        Refresh();
    }

    private void OnDisable()
    {
        if (_button) _button.onClick.RemoveListener(StartGame);
    }

    private void Update() => Refresh();

    private void Refresh()
    {
        bool mobile = MobileControlsOverlay.IsMobileUiEnabled;
        if (_buttonRoot && _buttonRoot.activeSelf != mobile)
            _buttonRoot.SetActive(mobile);
        if (_desktopPrompt && _desktopPrompt.gameObject.activeSelf == mobile)
            _desktopPrompt.gameObject.SetActive(!mobile);
    }

    private void StartGame()
    {
#if UNITY_ANDROID || UNITY_IOS || UNITY_EDITOR
        if (_controller) _controller.StartGame();
#endif
    }
}
#endregion
