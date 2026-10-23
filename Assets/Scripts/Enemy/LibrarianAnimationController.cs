using UnityEngine;

public sealed class LibrarianAnimationController : LivingAnimationController
{
    [Header("Librarian Clips")]
    [SerializeField] private AnimationClip _idleClip;
    [SerializeField] private AnimationClip _runClip;
    [SerializeField] private AnimationClip _deathClip;

    public AnimationClip IdleClip => _idleClip;
    public AnimationClip RunClip => _runClip;
    public AnimationClip DeathClip => _deathClip;

    public void PlayIdle() => PlayAnimation(_idleClip);
    public void PlayRun() => PlayAnimation(_runClip);
    public void PlayDeath() => base.PlayAnimation(_deathClip, 0f);

    public override void PlayDeathAnimation() => PlayDeath();
}
