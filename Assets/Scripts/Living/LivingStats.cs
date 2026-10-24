using UnityEngine;

/// <summary>
/// Shared immutable tuning data for every living character.
/// Keep only values that have the same meaning for both player and enemies here.
/// </summary>
public abstract class LivingStats : ScriptableObject
{
    [Header("Base Movement")]
    [Min(0f)] public float MoveSpeed = 5f;
}
