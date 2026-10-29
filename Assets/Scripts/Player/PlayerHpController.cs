#region Imports
using UnityEngine;
using Kogetsu.Library.DesignPatternCore;
#endregion
#region PlayerHpController

[RequireComponent(typeof(PlayerStatus))]
public class PlayerHpController : LivingHpController
{
    #region Fields and configuration
    [SerializeField] private PlayerStatus _playerStatus;

    #endregion
    #region Unity lifecycle
    private void Reset()
    {
        TryGetComponent(out _playerStatus);
    }

    public override bool IsDead => _playerStatus.GetIsDead();

    private void Awake()
    {
        if (!_playerStatus)
            _playerStatus = GetComponent<PlayerStatus>();

    }

    #endregion
    #region Public API
    public override void TakeDamage(float damage)
    {
        if (_playerStatus.GetIsDead()) return;

        _playerStatus.SetCurrentHp(_playerStatus.GetCurrentHp() - damage);

        if (_playerStatus.GetIsDead())
        {
            PlayDeathSound();
            PublishGameover();
        }
    }

    public override void Kill()
    {
        if (_playerStatus.GetIsDead()) return;

        _playerStatus.SetCurrentHp(0f);
        PlayDeathSound();
        PublishGameover();
    }

    public void KillPlayer() => Kill();

    #endregion
    #region Internal helpers
    private void PublishGameover()
    {
        PlayerRunSession.RecordDeath();
        PlayerRunSession.ResetRun();

        if (!EventBus.Instance) return;

        EventBus.Instance.Publish(new GameoverEvent());
    }
    #endregion
}
#endregion
