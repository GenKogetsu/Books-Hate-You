#region Imports
using UnityEngine;
#endregion
#region LivingStats

public abstract class LivingStats : ScriptableObject
{
    [Header("Base Movement")]
    [Min(0f)] public float MoveSpeed = 5f;
}
#endregion
