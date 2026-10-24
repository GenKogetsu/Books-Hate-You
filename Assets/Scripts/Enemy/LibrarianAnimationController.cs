using UnityEngine;
using System.Collections.Generic;

public sealed class LibrarianAnimationController : LivingAnimationController
{
    [Header("Librarian Clips")]
    [SerializeField] private AnimationClip _idleClip;
    [SerializeField] private AnimationClip _runClip;
    [SerializeField] private List<AnimationClip> _deathClips = new();

    public AnimationClip IdleClip => _idleClip;
    public AnimationClip RunClip => _runClip;

    public void PlayIdle() => PlayAnimation(_idleClip);
    public void PlayRun() => PlayAnimation(_runClip);
    public void PlayDeath()
    {
        if (_deathClips == null || _deathClips.Count == 0)
            return;

        AnimationClip deathClip = _deathClips[Random.Range(0, _deathClips.Count)];
        base.PlayAnimation(deathClip, 0f);
    }

    public override void PlayDeathAnimation() => PlayDeath();
}
