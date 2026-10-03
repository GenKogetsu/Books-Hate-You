using NaughtyAttributes;
using UnityEngine;
using UnityEngine.InputSystem;
using Kogetsu.Library.Attribute;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovementController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField, EditOnInspector] private PlayerStats _playerStats;

    [Header("Input Action")]
    [SerializeField] private InputActionReference _moveActionReference;
    [SerializeField] private InputActionReference _jumpActionReference;

    [SerializeField] private CharacterController _characterController;
    [SerializeField, ReadOnly] private Vector2 _moveInput;
    [SerializeField, ReadOnly] private Vector3 _velocity;
    [SerializeField, ReadOnly] private bool _isGrounded;

    private void Reset()
    {
        _characterController = GetComponent<CharacterController>();
    }

    private void OnEnable()
    {
        if (_moveActionReference != null)
        {
            _moveActionReference.action.Enable();
            _moveActionReference.action.performed += OnMovePerformed;
            _moveActionReference.action.canceled += OnMoveCanceled;
        }

        if (_jumpActionReference != null)
        {
            _jumpActionReference.action.Enable();
            _jumpActionReference.action.performed += OnJumpPerformed;
        }
    }

    private void OnDisable()
    {
        if (_moveActionReference != null)
        {
            _moveActionReference.action.performed -= OnMovePerformed;
            _moveActionReference.action.canceled -= OnMoveCanceled;
            _moveActionReference.action.Disable();
        }

        if (_jumpActionReference != null)
        {
            _jumpActionReference.action.performed -= OnJumpPerformed;
            _jumpActionReference.action.Disable();
        }
    }

    private void Update()
    {
        ApplyMovement();
    }

    private void OnMovePerformed(InputAction.CallbackContext context)
    {
        _moveInput = context.ReadValue<Vector2>();
    }

    private void OnMoveCanceled(InputAction.CallbackContext context)
    {
        _moveInput = Vector2.zero;
    }

    private void OnJumpPerformed(InputAction.CallbackContext context)
    {
        if (_isGrounded)
        {
            _velocity.y = Mathf.Sqrt(_playerStats.JumpHeight * -2f * _playerStats.Gravity);
        }
    }

    private void ApplyMovement()
    {
        _isGrounded = _characterController.isGrounded;
        if (_isGrounded && _velocity.y < 0)
        {
            _velocity.y = -2f;
        }

        Vector3 move = new(_moveInput.x, 0f, _moveInput.y);
        _characterController.Move(_playerStats.MoveSpeed * Time.deltaTime * move);

        _velocity.y += _playerStats.Gravity * Time.deltaTime;
        _characterController.Move(_velocity * Time.deltaTime);
    }
}