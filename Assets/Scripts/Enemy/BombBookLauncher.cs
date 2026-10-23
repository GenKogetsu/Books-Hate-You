using Kogetsu.Library.DesignPatternCore;
using UnityEngine;

public sealed class BombBookLauncher : MonoBehaviour
{
    [Header("Vision")]
    [SerializeField, Min(0.1f)] private float _range = 10f;
    [SerializeField, Range(1f, 179f)] private float _viewAngle = 60f;
    [SerializeField] private Vector3 _eyeOffset = new(0f, 1.5f, 0.5f);
    [SerializeField] private Vector3 _aimOffset = new(0f, 1f, 0f);
    [SerializeField] private LayerMask _livingMask;
    [SerializeField] private LayerMask _visionBlockMask;
    [SerializeField] private TargetTag _targetTags = TargetTag.Player;

    [Header("Bomb Book")]
    [SerializeField] private Book _bombBookPrefab;
    [SerializeField] private Vector2Int _ammoRange = new(3, 8);
    [SerializeField] private Vector2 _cooldownRange = new(1f, 2f);

    private Collider[] _ownColliders;
    private Transform _target;
    private int _ammoLeft;
    private float _nextShotTime;

    public int AmmoLeft => _ammoLeft;

    private void Awake()
    {
        _ownColliders = GetComponentsInChildren<Collider>(true);
        if (_livingMask.value == 0) _livingMask = LayerMask.GetMask("Living");
        _ammoLeft = Random.Range(
            Mathf.Min(_ammoRange.x, _ammoRange.y),
            Mathf.Max(_ammoRange.x, _ammoRange.y) + 1);
    }

    private void Update()
    {
        if (_ammoLeft <= 0) return;
        _target = FindVisibleTarget();
        if (_target && Time.time >= _nextShotTime) Fire();
    }

    private Transform FindVisibleTarget()
    {
        Vector3 origin = GetOrigin();
        Collider[] candidates = Physics.OverlapSphere(
            origin, _range, _livingMask, QueryTriggerInteraction.Ignore);

        Transform closest = null;
        float closestDistance = float.MaxValue;

        foreach (Collider candidate in candidates)
        {
            if (!candidate || IsOwnCollider(candidate)) continue;
            Transform target = FindTargetRoot(candidate.transform);
            if (!target) continue;

            Vector3 aimPoint = target.position + _aimOffset;
            Vector3 direction = aimPoint - origin;
            if (Vector3.Angle(transform.forward, direction) > _viewAngle * 0.5f) continue;
            if (!HasLineOfSight(origin, aimPoint, target)) continue;

            float distance = direction.sqrMagnitude;
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closest = target;
            }
        }

        return closest;
    }

    private void Fire()
    {
        if (!_bombBookPrefab || !ObjectPoolManager.Instance) return;

        Vector3 origin = GetOrigin();
        Vector3 targetDirection = ((_target.position + _aimOffset) - origin).normalized;
        Vector3 direction = GetRandomDirectionInCone(targetDirection, _viewAngle);
        Book bombBook = ObjectPoolManager.Instance.Get<Book>(
            _bombBookPrefab.name, origin, Quaternion.LookRotation(direction));

        if (!bombBook) return;

        // No throw force is applied. The pooled BombBook keeps Rigidbody gravity enabled.
        bombBook.Launch(direction, 0f, _ownColliders);
        _ammoLeft--;
        _nextShotTime = Time.time + Random.Range(
            Mathf.Min(_cooldownRange.x, _cooldownRange.y),
            Mathf.Max(_cooldownRange.x, _cooldownRange.y));
    }

    private Transform FindTargetRoot(Transform current)
    {
        while (current)
        {
            if (_targetTags.MatchesUnityTag(current.tag)) return current;
            current = current.parent;
        }
        return null;
    }

    private bool HasLineOfSight(Vector3 origin, Vector3 aimPoint, Transform target)
    {
        Vector3 direction = aimPoint - origin;
        RaycastHit[] hits = Physics.RaycastAll(
            origin, direction.normalized, direction.magnitude,
            Physics.AllLayers, QueryTriggerInteraction.Ignore);

        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
        foreach (RaycastHit hit in hits)
        {
            if (!hit.collider || IsOwnCollider(hit.collider)) continue;
            if (hit.transform == target || hit.transform.IsChildOf(target)) return true;
            if ((_visionBlockMask.value & (1 << hit.collider.gameObject.layer)) != 0) return false;
        }
        return true;
    }

    private bool IsOwnCollider(Collider collider)
    {
        foreach (Collider own in _ownColliders)
            if (own == collider) return true;
        return false;
    }

    private Vector3 GetOrigin() => transform.position + transform.TransformVector(_eyeOffset);

    private static Vector3 GetRandomDirectionInCone(Vector3 axis, float angle)
    {
        Quaternion axisRotation = Quaternion.LookRotation(axis);
        float cos = Mathf.Lerp(1f, Mathf.Cos(angle * 0.5f * Mathf.Deg2Rad), Random.value);
        float sin = Mathf.Sqrt(1f - cos * cos);
        float theta = Random.value * Mathf.PI * 2f;
        Vector3 localDirection = new(
            sin * Mathf.Cos(theta),
            sin * Mathf.Sin(theta),
            cos);
        return (axisRotation * localDirection).normalized;
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 origin = GetOrigin();
        Vector3 axis = transform.forward;
        float halfAngle = _viewAngle * 0.5f * Mathf.Deg2Rad;
        float radius = _range * Mathf.Tan(halfAngle);
        Vector3 center = origin + axis * _range;
        Vector3 right = transform.right;
        Vector3 up = transform.up;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(origin, _range);

        const int coneSegments = 8;
        Vector3 previous = center + right * radius;
        for (int i = 1; i <= coneSegments; i++)
        {
            float angle = i / (float)coneSegments * Mathf.PI * 2f;
            Vector3 point = center +
                (right * Mathf.Cos(angle) + up * Mathf.Sin(angle)) * radius;

            Gizmos.DrawLine(previous, point);
            Gizmos.DrawLine(origin, point);
            previous = point;
        }

        Gizmos.color = Color.red;
        Gizmos.DrawRay(origin, axis * _range);
    }
}
