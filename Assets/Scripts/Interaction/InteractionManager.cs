using UnityEngine;
using UnityEngine.InputSystem;
using Kogetsu.Library.DesignPatternCore;

[RequireComponent(typeof(PlayerInventory))]
public sealed class InteractionManager : MonoBehaviour
{
    [Header("Detection")]
    [SerializeField, Min(0.1f)] private float _interactionRange = 2.5f;
    [SerializeField] private LayerMask _interactableMask = ~0;
    [SerializeField] private Transform _interactionOrigin;

    [Header("Input")]
    [SerializeField] private InputActionReference _interactActionReference;

    private PlayerInventory _inventory;
    private IInteractable _currentInteractable;
    private Component _currentComponent;
    private InteractionPromptView _currentPromptView;
    private float _currentDistance;
    public InteractionState CurrentState { get; private set; }

    private void Awake()
    {
        _inventory = GetComponent<PlayerInventory>();
        _interactionOrigin = _interactionOrigin ? _interactionOrigin : transform;
    }

    private void OnEnable()
    {
        if (EventBus.Instance)
            EventBus.Instance.Subscribe<GameClearEvent>(OnGameClear);

        if (_interactActionReference)
        {
            _interactActionReference.action.Enable();
            _interactActionReference.action.performed += OnInteractPerformed;
        }
    }

    private void OnDisable()
    {
        if (EventBus.Instance)
            EventBus.Instance.Unsubscribe<GameClearEvent>(OnGameClear);

        if (_interactActionReference)
        {
            _interactActionReference.action.performed -= OnInteractPerformed;
            _interactActionReference.action.Disable();
        }
    }

    private void OnGameClear(GameClearEvent _) => enabled = false;

    private void Update()
    {
        FindInteractable();
        UpdatePrompt();

        // Keeps the system usable even when the InputActionReference has not
        // been assigned yet. The reference remains the preferred input path.
        if (Keyboard.current != null &&
            Keyboard.current.fKey.wasPressedThisFrame)
        {
            TryInteract();
        }
    }

    private void OnInteractPerformed(InputAction.CallbackContext _)
    {
        TryInteract();
    }

    private void TryInteract()
    {
        if (_currentInteractable != null &&
            _currentDistance <= _interactionRange * _interactionRange &&
            _currentInteractable.CanInteract(_inventory))
        {
            _currentInteractable.Interact(_inventory);
        }
    }

    private void FindInteractable()
    {
        if (_currentPromptView != null)
            _currentPromptView.SetState(string.Empty, false);

        _currentPromptView = null;
        _currentInteractable = null;
        _currentComponent = null;
        _currentDistance = float.MaxValue;
        CurrentState = InteractionState.None;

        float closestSqrDistance = float.MaxValue;

        foreach (InteractionPromptView view in InteractionPromptView.AllActive)
        {
            if (!view || !view.isActiveAndEnabled)
                continue;

            IInteractable interactable =
                view.GetComponent(typeof(IInteractable)) as IInteractable;

            if (interactable == null ||
                string.IsNullOrEmpty(interactable.InteractionPrompt))
                continue;

            float distance =
                (view.transform.position - _interactionOrigin.position).sqrMagnitude;

            if (distance < closestSqrDistance)
            {
                closestSqrDistance = distance;
                _currentInteractable = interactable;
                _currentComponent = interactable as Component;
                _currentDistance = distance;
            }
        }
    }

    private void UpdatePrompt()
    {
        bool hasTarget = _currentInteractable != null;
        if (!hasTarget || _currentComponent == null)
            return;

        InteractionPromptView view =
            _currentComponent.GetComponent<InteractionPromptView>();

        if (view == null)
            view = _currentComponent.gameObject.AddComponent<InteractionPromptView>();

        _currentPromptView = view;

        bool inRange = _currentDistance <= _interactionRange * _interactionRange;
        bool conditionPassed = _currentInteractable.CanInteract(_inventory);
        string prompt = !inRange
            ? "O"
            : conditionPassed
                ? "F"
                : GetBlockedPrompt(_currentInteractable);
        CurrentState = !inRange
            ? InteractionState.Detected
            : conditionPassed ? InteractionState.InRange : InteractionState.Blocked;
        view.SetState(prompt, true);
    }

    private static string GetBlockedPrompt(IInteractable interactable)
    {
        IInteractionBlockReason reasonProvider = interactable as IInteractionBlockReason;
        if (reasonProvider == null ||
            string.IsNullOrWhiteSpace(reasonProvider.InteractionBlockReason))
            return "X";

        return $"X\n{reasonProvider.InteractionBlockReason}";
    }

    private void OnDrawGizmosSelected()
    {
        Transform origin = _interactionOrigin ? _interactionOrigin : transform;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(origin.position, _interactionRange);
    }
}
