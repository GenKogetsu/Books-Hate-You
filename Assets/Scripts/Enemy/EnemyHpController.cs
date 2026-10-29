#region Imports
using UnityEngine;
using NaughtyAttributes;
#endregion
#region EnemyHpController

public sealed class EnemyHpController : LivingHpController
{
    #region Health configuration and references

    [Header("References")]
    [SerializeField] private LivingAnimationController _animationController;
    [SerializeField] private LibrarianMovementController _movement;
    [SerializeField] private KillOnHit _killOnHit;
    [SerializeField] private Collider _collider;

    [Header("Health")]
    [SerializeField, Min(0f)] private float _startHp = 100f;
    [SerializeField, Min(0f)] private float _maxHp = 100f;
    [SerializeField, ReadOnly, Foldout("Runtime Debug")] private float _currentHp;

    public float CurrentHp => _currentHp;
    public bool IsAlive => _currentHp > 0f;
    public override bool IsDead => !IsAlive;

    #endregion
    #region Unity lifecycle
    private void Reset()
    {
        _animationController = GetComponent<LivingAnimationController>();
        _movement = GetComponent<LibrarianMovementController>();
        _killOnHit = GetComponent<KillOnHit>();
    }

    private void Awake()
    {
        _currentHp = Mathf.Clamp(_startHp, 0f, _maxHp);

        if (!_animationController)
            _animationController = GetComponent<LivingAnimationController>();

        if (!_movement)
            _movement = GetComponent<LibrarianMovementController>();

        if (!_killOnHit)
            _killOnHit = GetComponent<KillOnHit>();

        if (!_collider)
            _collider = GetComponent<Collider>();

    }

    private void OnEnable()
    {
        PlayerRunSession.LinkedLibrarianDefeated += OnLinkedLibrarianDefeated;
    }

    private void OnDisable()
    {
        PlayerRunSession.LinkedLibrarianDefeated -= OnLinkedLibrarianDefeated;
    }

    private void Start()
    {
        if (PlayerRunSession.ShouldLinkedLibrarianBeDead(this))
            Kill();
    }

    #endregion
    #region Event handlers
    private void OnLinkedLibrarianDefeated(EnemyHpController defeated)
    {
        if (defeated != this && PlayerRunSession.IsLinkedLibrarian(this))
            Kill();
    }

    #endregion
    #region Damage and death
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

        if (_killOnHit)
            _killOnHit.enabled = false;

        if (_collider)
            _collider.enabled = false;

        if (_movement)
        {
            _movement.HandleDeath();
            _movement.enabled = false;
        }

        if (_animationController)
            _animationController.PlayDeathAnimation();

        PlayDeathSound();
        PlayerRunSession.NotifyFloorEnemyDefeated(this);

    }
    #endregion
}
#endregion
