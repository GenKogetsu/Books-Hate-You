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

        if (HasParameter(_animator, SpeedMultiplierHash, AnimatorControllerParameterType.Float))
            _animator.SetFloat(SpeedMultiplierHash, 1f);
        _animator.CrossFade(
            clip.name,
            blend >= 0f ? blend : _transitionDuration);
    }

    private static bool HasParameter(Animator animator, int hash, AnimatorControllerParameterType type)
    {
        foreach (AnimatorControllerParameter parameter in animator.parameters)
        {
            if (parameter.nameHash == hash && parameter.type == type)
                return true;
        }

        return false;
    }

    public virtual void PlayDeathAnimation() { }
}
