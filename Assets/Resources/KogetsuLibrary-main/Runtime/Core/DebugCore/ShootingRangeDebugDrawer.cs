#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Kogetsu.Library.Core;

/// <summary>
/// วาด debug range ซ้อนสองชั้นใน Scene view:
///   - กรวยในสีฟ้า  = ระยะ flashlight (สิ่งที่ผู้เล่นมองเห็น)
///   - กรวยนอกสีส้ม = ระยะยิง (ไกลกว่า)
///   - เส้นประเหลือง = แนวกระสุน
///
/// ใส่บน GunPivot หรือ MuzzlePoint
/// DrawAlways = true → แสดงตลอดเวลา, false → แสดงเฉพาะตอน select (default)
/// </summary>
public class ShootingRangeDebugDrawer : MonoBehaviour
{
    [Header("Flashlight Range (visibility)")]
    [SerializeField] private float FlashlightRange = 12f;
    [SerializeField] private float FlashlightAngle = 60f;
    [SerializeField] private Color FlashlightColor = new(0.2f, 0.8f, 1f, 0.18f);

    [Header("Shoot Range")]
    [SerializeField] private float ShootRange     = 28f;
    [SerializeField] private float ShootAngle     = 25f;
    [SerializeField] private Color ShootConeColor = new(1f, 0.55f, 0f, 0.22f);

    [Header("Center Dashed Line")]
    [SerializeField] private Color DashColor    = Color.yellow;
    [SerializeField] private int   DashSegments = 12;

    [Header("Options")]
    [SerializeField] private bool DrawAlways = false;

#if UNITY_EDITOR
    private void OnDrawGizmos()         { if (DrawAlways)  Draw(); }
    private void OnDrawGizmosSelected() { if (!DrawAlways) Draw(); }

    private void Draw()
    {
        Vector3 origin  = transform.position;
        Vector3 forward = transform.right;

        DrawCone(origin, forward, FlashlightRange, FlashlightAngle, FlashlightColor);
        DrawCone(origin, forward, ShootRange,      ShootAngle,      ShootConeColor);
        DrawDash(origin, forward, ShootRange, DashColor, DashSegments);
    }

    private static void DrawCone(Vector3 origin, Vector3 forward, float range, float angle, Color color)
    {
        Vector3 left  = Quaternion.Euler(0, 0,  angle * 0.5f) * forward;
        Vector3 right = Quaternion.Euler(0, 0, -angle * 0.5f) * forward;

        Handles.color = color;
        Handles.DrawLine(origin, origin + left  * range);
        Handles.DrawLine(origin, origin + right * range);
        Handles.DrawWireArc(origin, Vector3.back, left, angle, range);
        Handles.DrawSolidArc(origin, Vector3.back, left, angle, range);
    }

    private static void DrawDash(Vector3 origin, Vector3 forward, float range, Color color, int segs)
    {
        Gizmos.color = color;
        for (int i = 0; i < segs; i++)
        {
            float t0 = (i * 2f)      / (segs * 2f);
            float t1 = (i * 2f + 1f) / (segs * 2f);
            Gizmos.DrawLine(origin + forward * (t0 * range),
                            origin + forward * (t1 * range));
        }
    }
#endif
}
