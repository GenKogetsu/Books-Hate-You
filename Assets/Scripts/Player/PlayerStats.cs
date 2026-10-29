#region Imports
using UnityEngine;
#endregion
#region PlayerStats

[CreateAssetMenu(fileName = "PlayerStats", menuName = "Data/PlayerStats")]
public class PlayerStats : LivingStats
{
    public float Gravity = -9.81f;
    public float JumpHeight = 1.2f;
}
#endregion
