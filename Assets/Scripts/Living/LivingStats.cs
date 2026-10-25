using UnityEngine;

public abstract class LivingStats : ScriptableObject
{
    [Header("Base Movement")]
    [Min(0f)] public float MoveSpeed = 5f;
}
