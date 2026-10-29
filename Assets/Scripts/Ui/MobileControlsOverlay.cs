#region Imports
using Kogetsu.Library.DesignPatternCore;
using Kogetsu.Library.Core;
using UnityEngine;
#endregion
#region MobileControlsOverlay

/// <summary>Shows the touch controls only while a mobile run is playable.</summary>
public sealed class MobileControlsOverlay : MonoBehaviour
{
    [SerializeField] private GameObject _controlsRoot;
    [SerializeField] private GameObject _interactButton;
    [SerializeField] private GameObject _desktopGuideRoot;
    [SerializeField] private GameObject[] _hudRoots;

    private InteractionManager _interaction;
    private bool _ended;
    private bool _subscribed;
    private bool _hudHidden;
    private bool[] _hudWasActive;

    public bool IsEnded => _ended;

#if UNITY_EDITOR
    // Set this during Play mode to inspect the phone layout in the Editor.
    public static bool ForcePreview { get; set; }
#endif

    public static bool IsMobileUiEnabled
    {
        get
        {
#if UNITY_ANDROID || UNITY_IOS
            return true;
#elif UNITY_EDITOR
            return ForcePreview;
#else
            return false;
#endif
        }
    }

    public void Configure(GameObject controlsRoot, GameObject interactButton,
        GameObject desktopGuideRoot, GameObject[] hudRoots)
    {
        _controlsRoot = controlsRoot;
        _interactButton = interactButton;
        _desktopGuideRoot = desktopGuideRoot;
        _hudRoots = hudRoots;
    }

    private void Awake()
    {
#if UNITY_ANDROID || UNITY_IOS
        Application.targetFrameRate = 60;
#endif
        Refresh();
    }

    private void OnEnable()
    {
        _ended = false;
        TrySubscribe();
        Refresh();
    }

    private void OnDisable()
    {
        if (_subscribed && EventBus.Instance)
        {
            EventBus.Instance.Unsubscribe<GameoverEvent>(OnGameOver);
            EventBus.Instance.Unsubscribe<GameClearEvent>(OnGameClear);
            EventBus.Instance.Unsubscribe<LoadSceneEvent>(OnLoadScene);
        }
        _subscribed = false;
    }

    private void Update()
    {
        TrySubscribe();
        Refresh();
    }

    private void TrySubscribe()
    {
        if (_subscribed || !EventBus.Instance) return;
        EventBus.Instance.Subscribe<GameoverEvent>(OnGameOver);
        EventBus.Instance.Subscribe<GameClearEvent>(OnGameClear);
        EventBus.Instance.Subscribe<LoadSceneEvent>(OnLoadScene);
        _subscribed = true;
    }

    private void OnGameOver(GameoverEvent _) { _ended = true; Refresh(); }
    private void OnGameClear(GameClearEvent _) { _ended = true; Refresh(); }
    private void OnLoadScene(LoadSceneEvent _) { _ended = true; Refresh(); }

    private void Refresh()
    {
        bool mobile = IsMobileUiEnabled;
        RefreshHud(mobile);
        MobileHudLayout hud = GetComponent<MobileHudLayout>();
        if (hud) hud.SetEndScreenVisibility(mobile && _ended);
        bool transitioning = BasicSceneEffectController.Instance &&
                             BasicSceneEffectController.Instance.IsTransitioning;
        bool show = mobile && !_ended && !transitioning;
        bool showGuide = !mobile && !_ended && !transitioning;
        if (_desktopGuideRoot && _desktopGuideRoot.activeSelf != showGuide)
            _desktopGuideRoot.SetActive(showGuide);
        if (_controlsRoot && _controlsRoot.activeSelf != show)
            _controlsRoot.SetActive(show);
        if (!show) return;

        if (!_interaction)
            _interaction = FindAnyObjectByType<InteractionManager>();
        bool canInteract = _interaction && _interaction.isActiveAndEnabled &&
                           _interaction.CurrentState == InteractionState.InRange;
        if (_interactButton && _interactButton.activeSelf != canInteract)
            _interactButton.SetActive(canInteract);

    }

    private void RefreshHud(bool mobile)
    {
        if (_hudRoots == null) return;
        if (mobile && _ended)
        {
            if (!_hudHidden)
            {
                _hudWasActive = new bool[_hudRoots.Length];
                for (int i = 0; i < _hudRoots.Length; i++)
                    _hudWasActive[i] = _hudRoots[i] && _hudRoots[i].activeSelf;
                _hudHidden = true;
            }
            // Toast animations may activate their own GameObject after the end event.
            foreach (GameObject root in _hudRoots)
                if (root && root.activeSelf) root.SetActive(false);
        }
        else if (_hudHidden)
        {
            for (int i = 0; i < _hudRoots.Length; i++)
                if (_hudRoots[i]) _hudRoots[i].SetActive(_hudWasActive[i]);
            _hudWasActive = null;
            _hudHidden = false;
        }
    }
}
#endregion
