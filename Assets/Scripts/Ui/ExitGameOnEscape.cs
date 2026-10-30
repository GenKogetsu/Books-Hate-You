using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(LoadPublisher))]
public sealed class ExitGameOnEscape : MonoBehaviour
{
    private LoadPublisher publisher;

    private void Awake()
    {
        publisher = GetComponent<LoadPublisher>();
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            publisher.PublishOutGameEvent();
    }
}
