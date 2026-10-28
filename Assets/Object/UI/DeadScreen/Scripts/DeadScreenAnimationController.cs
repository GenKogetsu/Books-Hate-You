using UnityEngine;
using NaughtyAttributes;
using Kogetsu.Library.DesignPatternCore;

public class DeadScreenAnimationController : MonoBehaviour
{
    [SerializeField, Required] private Animator _animator;

    private void OnEnable()
    {
        if (EventBus.Instance)
        {
            EventBus.Instance.Subscribe<GameoverEvent>(OnGameoverEvent);
        }
    }

    private void OnDisable()
    {
        if (EventBus.Instance)
        {
            EventBus.Instance.Unsubscribe<GameoverEvent>(OnGameoverEvent);
        }
    }

    private void OnGameoverEvent(GameoverEvent e)
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        _animator.speed = 1f;
        _animator.Play("Reveal", 0, 0f);
    }
}
