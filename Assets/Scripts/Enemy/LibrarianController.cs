using UnityEngine;

[RequireComponent(typeof(LibrarianMovementController))]
[RequireComponent(typeof(EnemyHpController))]
[RequireComponent(typeof(LibrarianAnimationController))]
public sealed class LibrarianController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private LibrarianMovementController _movementController;
    [SerializeField] private EnemyHpController _enemyHpController;

    private void Reset()
    {
        _movementController = GetComponent<LibrarianMovementController>();
        _enemyHpController = GetComponent<EnemyHpController>();
    }

    private void Awake()
    {
        if (!_movementController)
            _movementController = GetComponent<LibrarianMovementController>();

        if (!_enemyHpController)
            _enemyHpController = GetComponent<EnemyHpController>();
    }

    private void OnDrawGizmos()
    {
        if (!_movementController || !_movementController.Stats)
            return;

        Vector3 origin = _movementController.GetVisionOrigin();
        Vector3 forward = _movementController.GetVisionForward();
        Transform eyePoint = _movementController.EyePoint;
        Vector3 right = eyePoint ? eyePoint.right : transform.right;
        Vector3 up = eyePoint ? eyePoint.up : transform.up;
        float halfAngle = _movementController.Stats.ViewAngle * 0.5f * Mathf.Deg2Rad;
        float radius = _movementController.Stats.DetectionRange * Mathf.Tan(halfAngle);
        Vector3 center = origin + forward * _movementController.Stats.DetectionRange;

        Gizmos.color = _movementController.IsChasing
            ? Color.red
            : Color.green;

        const int rimSegments = 24;
        Vector3 previous = center + right * radius;

        for (int i = 1; i <= rimSegments; i++)
        {
            float angle = i / (float)rimSegments * Mathf.PI * 2f;
            Vector3 point = center +
                (right * Mathf.Cos(angle) + up * Mathf.Sin(angle)) * radius;

            Gizmos.DrawLine(previous, point);
            previous = point;
        }

        for (int i = 0; i < rimSegments; i += 3)
        {
            float angle = i / (float)rimSegments * Mathf.PI * 2f;
            Vector3 point = center +
                (right * Mathf.Cos(angle) + up * Mathf.Sin(angle)) * radius;

            Gizmos.DrawLine(origin, point);
        }
    }

    public void HandleDeath()
    {
        _movementController?.HandleDeath();
    }
}
