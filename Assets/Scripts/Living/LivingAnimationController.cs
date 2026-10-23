using UnityEngine;

public abstract class LivingAnimationController : MonoBehaviour
{
    [Header("Animation References")]
    [SerializeField] protected Animator _animator;

    [Header("Animation Settings")]
    [SerializeField, Min(0f)] protected float _transitionDuration = 0.15f;

    protected static readonly int SpeedMultiplierHash = Animator.StringToHash("SpeedMultiplier");

    protected virtual void Reset()
    {
        _animator = GetComponentInChildren<Animator>();
    }

    protected virtual void Awake()
    {
        if (!_animator)
            _animator = GetComponentInChildren<Animator>();
    }

    public virtual void PlayAnimation(AnimationClip clip, float blend = -1f)
    {
        if (!_animator || !clip)
            return;

        _animator.SetFloat(SpeedMultiplierHash, 1f);
        _animator.CrossFade(
            clip.name,
            blend >= 0f ? blend : _transitionDuration);
    }

    public virtual void PlayDeathAnimation() { }
}
