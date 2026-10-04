using UnityEngine;
using Kogetsu.Library.DesignPatternCore;

[RequireComponent(typeof(PlayerStatus))]
public class PlayerHpController : MonoBehaviour
{
    [SerializeField] private PlayerStatus _playerStatus;

    private void Reset()
    {
        TryGetComponent(out _playerStatus);
    }

    public void TakeDamage(float damage)
    {
        if (_playerStatus.GetIsDead()) return;

        _playerStatus.SetCurrentHp(_playerStatus.GetCurrentHp() - damage);

        if (_playerStatus.GetIsDead())
        {
            PublishGameover();
        }
    }

    public void KillPlayer()
    {
        if (_playerStatus.GetIsDead()) return;

        _playerStatus.SetCurrentHp(0f);
        PublishGameover();
    }

    private void PublishGameover()
    {
        if (!EventBus.Instance) return;

        EventBus.Instance.Publish(new GameoverEvent());
    }
}