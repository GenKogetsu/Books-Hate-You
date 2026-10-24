using UnityEngine;
using NaughtyAttributes;

public sealed class EnemyHpController : LivingHpController
{
    [Header("Health")]
    [SerializeField, Min(0f)] private float _startHp = 100f;
    [SerializeField, Min(0f)] private float _maxHp = 100f;
    [SerializeField, ReadOnly] private float _currentHp;

    [Header("Death")]
    [SerializeField] private LivingAnimationController _animationController;

    public float CurrentHp => _currentHp;
    public bool IsAlive => _currentHp > 0f;
    public override bool IsDead => !IsAlive;

    private void Reset()
    {
        _animationController = GetComponent<LivingAnimationController>();
    }

    private void Awake()
    {
        _currentHp = Mathf.Clamp(_startHp, 0f, _maxHp);

        if (!_animationController)
            _animationController = GetComponent<LivingAnimationController>();

    }

    public override void TakeDamage(float damage)
    {
        if (IsDead)
            return;

        SetCurrentHp(_currentHp - damage);
    }

    public override void Kill()
    {
        if (IsDead)
            return;

        SetCurrentHp(0f);
    }

    private void SetCurrentHp(float value)
    {
        bool wasAlive = IsAlive;
        _currentHp = Mathf.Clamp(value, 0f, _maxHp);

        if (wasAlive && IsDead)
            OnDeath();
    }

    private void OnDeath()
    {
        PlayDeathSound();

        _animationController?.PlayDeathAnimation();

        LibrarianMovementController movement =
            GetComponent<LibrarianMovementController>();

        movement?.HandleDeath();
    }
}
