using UnityEngine;
using NaughtyAttributes;
using Kogetsu.Library.DesignPatternCore;

public class PlayerHpController : MonoBehaviour
{
    [SerializeField] private float _startHp = 100f;
    [SerializeField] private float _maxHp = 100f;
    [SerializeField, ReadOnly] private float _currentHp;

    private void Awake()
    {
        _currentHp = _startHp;
    }

    public float GetPlayerHp() => _currentHp;

    public void TakeDamage(float damage)
    {
        _currentHp -= damage;
        _currentHp = Mathf.Clamp(_currentHp, 0f, _maxHp);

        if (_currentHp > 0f && !EventBus.Instance) return;

        EventBus.Instance.Publish(new GameoverEvent());
    }

    public void KillPlayer()
    {
        _currentHp = 0f;

        if (!EventBus.Instance) return;

        EventBus.Instance.Publish(new GameoverEvent());
        //Debug.Log("Player has been killed.");
    }
}
