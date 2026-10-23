using UnityEngine;

[CreateAssetMenu(fileName = "LibrarianStats", menuName = "Data/LibrarianStats")]
public class LibrarianStats : ScriptableObject
{
    [Header("Chase Movement")]
    [Min(0f)] public float InitialChaseSpeed = 3.5f;
    [Min(0f)] public float MaximumChaseSpeed = 7f;
    [Min(0f)] public float SpeedIncreasePerSecond = 0.25f;
    [Min(0f)] public float DestinationUpdateInterval = 0.15f;

    [Header("Vision")]
    [Min(0.1f)] public float DetectionRange = 10f;
    [Range(1f, 179f)] public float ViewAngle = 60f;
    [Min(0.1f)] public float LoseTargetRange = 15f;
    public Vector3 EyeOffset = new(0f, 1.5f, 0f);
}
