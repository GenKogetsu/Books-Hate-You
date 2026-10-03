using UnityEngine;

[CreateAssetMenu(fileName = "PlayerStats", menuName = "Data/PlayerStats")]
public class PlayerStats : ScriptableObject
{
    public float MoveSpeed = 5f;
    public float Gravity = -9.81f;
    public float JumpHeight = 1.2f;
}
