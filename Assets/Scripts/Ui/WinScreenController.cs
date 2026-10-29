using Kogetsu.Library.DesignPatternCore;
using NaughtyAttributes;
using UnityEngine;

public sealed class WinScreenController : MonoBehaviour
{
    [SerializeField] private GameObject _winCanvas;
    [SerializeField] private Animator _animator;

    private void Awake()
    {
        if (_winCanvas)
            _winCanvas.SetActive(false);
    }

    private void OnEnable()
    {
        if (EventBus.Instance)
            EventBus.Instance.Subscribe<GameClearEvent>(OnPlayerWin);
    }

    private void OnDisable()
    {
        if (EventBus.Instance)
            EventBus.Instance.Unsubscribe<GameClearEvent>(OnPlayerWin);
    }

    private void OnPlayerWin(GameClearEvent _)
    {
        if (_winCanvas)
            _winCanvas.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (_animator)
        {
            _animator.speed = 1f;
            _animator.Play("Reveal", 0, 0f);
        }
    }

    public void PublishWin()
    {
        PlayerRunSession.RecordWin();
        if (EventBus.Instance)
            EventBus.Instance.Publish(new GameClearEvent());
    }
}
