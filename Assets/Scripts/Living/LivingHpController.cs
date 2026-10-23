using UnityEngine;

public abstract class LivingHpController : MonoBehaviour
{
    public abstract bool IsDead { get; }
    public abstract void TakeDamage(float damage);
    public abstract void Kill();

    public virtual void Revive() { }
}
