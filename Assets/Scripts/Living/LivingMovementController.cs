using UnityEngine;

public abstract class LivingMovementController : MonoBehaviour
{
    protected virtual void Update()
    {
        if (CanMove())
            UpdateMovement();
    }

    protected virtual bool CanMove() => true;

    protected abstract void UpdateMovement();

    public virtual void StopMovement() { }
}
