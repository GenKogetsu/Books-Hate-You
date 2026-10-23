using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

[RequireComponent(typeof(PlayerInventory))]
public sealed class InteractionManager : MonoBehaviour
{
    [Header("Detection")]
    [SerializeField, Min(0.1f)] private float _interactionRange = 2.5f;
    [SerializeField] private LayerMask _interactableMask = ~0;
    [SerializeField] private Transform _interactionOrigin;

    [Header("Input")]
    [SerializeField] private InputActionReference _interactActionReference;

    [Header("Prompt")]
    [SerializeField] private Vector3 _promptOffset = new(0f, 1.5f, 0f);
    [SerializeField, Min(0f)] private float _promptCameraOffset = 0.2f;
    [SerializeField] private float _promptCharacterSize = 0.12f;

    private PlayerInventory _inventory;
    private IInteractable _currentInteractable;
    private Component _currentComponent;
    private InteractionPromptView _currentPromptView;
    private float _currentDistance;
    private GameObject _promptObject;
    private Canvas _promptCanvas;
    private TextMeshProUGUI _promptText;

    private void Awake()
    {
        _inventory = GetComponent<PlayerInventory>();
        _interactionOrigin = _interactionOrigin ? _interactionOrigin : transform;
    }

    private void OnEnable()
    {
        if (_interactActionReference)
        {
            _interactActionReference.action.Enable();
            _interactActionReference.action.performed += OnInteractPerformed;
        }
    }

    private void OnDisable()
    {
        if (_interactActionReference)
        {
            _interactActionReference.action.performed -= OnInteractPerformed;
            _interactActionReference.action.Disable();
        }
    }

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

    private void CreatePrompt()
    {
        // Create once during Awake, then only toggle this existing Canvas.
        // It is never recreated per interaction target.
        _promptObject = new GameObject("InteractionPromptCanvas");
        _promptObject.transform.SetParent(transform);

        _promptCanvas = _promptObject.AddComponent<Canvas>();
        _promptCanvas.renderMode = RenderMode.WorldSpace;
        _promptCanvas.overrideSorting = true;
        _promptCanvas.sortingOrder = 1000;

        RectTransform canvasRect = _promptCanvas.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(2f, 1f);
        canvasRect.localScale = Vector3.one * 0.01f;

        GameObject textObject = new GameObject("PromptText");
        textObject.transform.SetParent(_promptCanvas.transform, false);

        RectTransform textRect = textObject.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        _promptText = textObject.AddComponent<TextMeshProUGUI>();
        _promptText.alignment = TextAlignmentOptions.Center;
        _promptText.fontSize = 72f;
        _promptText.characterSpacing = 0f;
        _promptText.color = Color.white;
        _promptText.textWrappingMode = TextWrappingModes.NoWrap;
        _promptText.raycastTarget = false;

        _promptObject.SetActive(false);
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
        string prompt = !inRange ? "O" : conditionPassed ? "F" : "X";
        view.SetState(prompt, true);
    }

    private void OnDrawGizmosSelected()
    {
        Transform origin = _interactionOrigin ? _interactionOrigin : transform;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(origin.position, _interactionRange);
    }
}
