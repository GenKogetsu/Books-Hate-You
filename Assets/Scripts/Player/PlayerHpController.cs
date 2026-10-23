using UnityEngine;
using Kogetsu.Library.DesignPatternCore;

[RequireComponent(typeof(PlayerStatus))]
public class PlayerHpController : LivingHpController
{
    [SerializeField] private PlayerStatus _playerStatus;

    private void Reset()
    {
        TryGetComponent(out _playerStatus);
    }

    public override bool IsDead => _playerStatus.GetIsDead();

    public override void TakeDamage(float damage)
    {
        if (_playerStatus.GetIsDead()) return;

        _playerStatus.SetCurrentHp(_playerStatus.GetCurrentHp() - damage);

        if (_playerStatus.GetIsDead())
        {
            PublishGameover();
        }
    }

    public override void Kill()
    {
        if (_playerStatus.GetIsDead()) return;

        _playerStatus.SetCurrentHp(0f);
        PublishGameover();
    }

    public void KillPlayer() => Kill();

    private void PublishGameover()
    {
        if (!EventBus.Instance) return;

        EventBus.Instance.Publish(new GameoverEvent());
    }
}
