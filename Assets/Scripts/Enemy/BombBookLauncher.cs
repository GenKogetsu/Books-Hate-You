#region Imports
using Kogetsu.Library.DesignPatternCore;
using UnityEngine;
#endregion
#region BombBookLauncher

public sealed class BombBookLauncher : MonoBehaviour
{
    #region Launcher configuration and runtime state
    [Header("Launch Spread")]
    [SerializeField, Range(1f, 179f)] private float _viewAngle = 60f;
    [SerializeField, Min(0.1f)] private float _launchRange = 10f;

    [Header("Bomb Book")]
    [SerializeField] private Book _bombBookPrefab;
    [SerializeField] private Vector2Int _ammoRange = new(3, 8);
    [SerializeField] private Vector2 _cooldownRange = new(1f, 2f);
    [SerializeField, Min(0f)] private float _launchForce = 1.5f;

    [Header("Gizmo")]
    [SerializeField] private Color _launchConeColor = new(1f, 0.35f, 0f, 0.8f);

    private Collider[] _ownColliders;
    private int _ammoLeft;
    private float _nextShotTime;
    private Vector3 _nextLaunchDirection;

    public int AmmoLeft => _ammoLeft;

    #endregion
    #region Unity lifecycle
    private void Awake()
    {
        _ownColliders = GetComponentsInChildren<Collider>(true);
        _ammoLeft = Random.Range(
            Mathf.Min(_ammoRange.x, _ammoRange.y),
            Mathf.Max(_ammoRange.x, _ammoRange.y) + 1);
        _nextLaunchDirection = GetRandomDirectionInCone(transform.forward, _viewAngle);
    }

    private void Update()
    {
        if (_ammoLeft <= 0) return;
        if (Time.time >= _nextShotTime) Fire();
    }

    #endregion
    #region Launch and collision filtering
    private void Fire()
    {
        if (!_bombBookPrefab || !ObjectPoolManager.Instance) return;

        Vector3 origin = transform.position;
        Vector3 direction = _nextLaunchDirection;
        Book bombBook = ObjectPoolManager.Instance.Get<Book>(
            _bombBookPrefab.name, origin, Quaternion.LookRotation(direction));

        if (!bombBook) return;

        bombBook.Launch(direction, _launchForce, _ownColliders);
        _ammoLeft--;
        _nextLaunchDirection = GetRandomDirectionInCone(transform.forward, _viewAngle);
        _nextShotTime = Time.time + Random.Range(
            Mathf.Min(_cooldownRange.x, _cooldownRange.y),
            Mathf.Max(_cooldownRange.x, _cooldownRange.y));
    }

    private bool IsOwnCollider(Collider collider)
    {
        foreach (Collider own in _ownColliders)
            if (own == collider) return true;
        return false;
    }

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

    #endregion
    #region Event handlers
    private void OnDrawGizmosSelected()
    {
        const int sides = 8;
        Vector3 origin = transform.position;
        Vector3 axis = transform.forward;
        float halfAngle = _viewAngle * 0.5f * Mathf.Deg2Rad;
        float radius = _launchRange * Mathf.Tan(halfAngle);
        Vector3 center = origin + axis * _launchRange;
        Vector3 previous = center + transform.right * radius;

        Gizmos.color = _launchConeColor;

        for (int i = 1; i <= sides; i++)
        {
            float angle = i / (float)sides * Mathf.PI * 2f;
            Vector3 point = center +
                (transform.right * Mathf.Cos(angle) +
                 transform.up * Mathf.Sin(angle)) * radius;

            Gizmos.DrawLine(previous, point);
            Gizmos.DrawLine(origin, point);
            previous = point;
        }

        Gizmos.DrawRay(origin, axis * _launchRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(origin, _nextLaunchDirection * _launchRange);
    }

    #endregion
}
#endregion
