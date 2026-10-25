
using Kogetsu.Library.DesignPatternCore;
using Kogetsu.Library.Debugging;
using UnityEngine;

public class BookShelf : MonoBehaviour
{
    #region Serialized Fields

    [Header("Vision")]
    [SerializeField, Min(0.1f)] private float _range = 10f;
    [SerializeField, Range(1f, 179f)] private float _viewAngle = 60f;
    [SerializeField] private Vector3 _eyeOffset = new(0f, 1.5f, 0.5f);

    [Tooltip("ตรวจเฉพาะ Object ใน Layer Living ที่มี Tag ตรงกับ Target Tags")]
    [SerializeField] private LayerMask _livingMask;

    [Tooltip("เลือก Tag ของเป้าหมายที่ชั้นหนังสือจะตรวจจับได้")]
    [SerializeField] private TargetTag _targetTags = TargetTag.Player;

    [Tooltip("เลือกเฉพาะ Layer ของกำแพงหรือสิ่งกีดขวาง")]
    [SerializeField] private LayerMask _visionBlockMask;

    [SerializeField] private Vector3 _aimOffset = new(0f, 1f, 0f);

    [Header("Debug")]
    [SerializeField] private bool _debugLog;
    [Header("Throw")]
    [Tooltip("ใช้ Prefab นี้อ้างอิงชื่อ Key ที่ลงทะเบียนใน Pool")]
    [SerializeField] private Book _bookPrefab;

    [SerializeField, Min(0f)] private float _throwForce = 15f;
    [SerializeField] private Vector2Int _ammoRange = new(3, 8);
    [SerializeField, Min(0f)] private float _cooldown = 1.5f;

    private Color _idleColor = Color.green;
    private Color _detectedColor = Color.red;
    private int _rimSegments = 8;

    #endregion

    #region Private Fields

    private Collider[] _shelfColliders;
    private Transform _targetRoot;
    private bool _playerDetected;
    private bool _isGameover;
    private int _ammoLeft;
    private float _nextShotTime;
    private Vector3 _debugRayFrom;
    private Vector3 _debugRayTo;
    private string _lastDebugReason;

    #endregion

    #region Unity Callbacks

    private void OnEnable()
    {
        if (EventBus.Instance)
        {
            EventBus.Instance.Subscribe<GameoverEvent>(OnGameoverEvent);
            EventBus.Instance.Subscribe<GameClearEvent>(OnGameClearEvent);
        }
    }

    private void OnDisable()
    {
        if (EventBus.Instance)
        {
            EventBus.Instance.Unsubscribe<GameoverEvent>(OnGameoverEvent);
            EventBus.Instance.Unsubscribe<GameClearEvent>(OnGameClearEvent);
        }
    }

    private void Awake()
    {
        _shelfColliders = GetComponentsInChildren<Collider>(true);

        if (_livingMask.value == 0)
            _livingMask = LayerMask.GetMask("Living");

        int min = Mathf.Max(0, Mathf.Min(_ammoRange.x, _ammoRange.y));
        int max = Mathf.Max(0, Mathf.Max(_ammoRange.x, _ammoRange.y));

        _ammoLeft = Random.Range(min, max + 1);

        if (!_bookPrefab)
        {
            Debug.LogWarning(
                "[BookShelf] ยังไม่ได้กำหนด _bookPrefab",
                this);
        }

        if (_livingMask.value == 0)
        {
            DebugLog.Log(DebugCategory.Enemy,
                "ไม่พบ Layer Living สำหรับตรวจจับผู้เล่น", this, _debugLog);
        }
    }

    private void Update()
    {
        if (_isGameover)
            return;

        FindVisiblePlayer();

        if (!_playerDetected)
            return;

        if (Time.time < _nextShotTime)
            return;

        Fire();
    }

    private void OnDrawGizmos()
    {
        Vector3 origin = GetOrigin();
        Vector3 forward = transform.forward;
        Vector3 right = transform.right;
        Vector3 up = transform.up;

        float halfAngle = _viewAngle * 0.5f * Mathf.Deg2Rad;
        float radius = _range * Mathf.Tan(halfAngle);
        Vector3 center = origin + forward * _range;

        Gizmos.color = _playerDetected
            ? _detectedColor
            : _idleColor;

        Vector3 previous = center + right * radius;

        for (int i = 1; i <= _rimSegments; i++)
        {
            float angle =
                (i / (float)_rimSegments) * Mathf.PI * 2f;

            Vector3 point = center +
                (right * Mathf.Cos(angle) +
                 up * Mathf.Sin(angle)) * radius;

            Gizmos.DrawLine(previous, point);
            previous = point;
        }

        int step = Mathf.Max(1, _rimSegments / 8);

        for (int i = 0; i < _rimSegments; i += step)
        {
            float angle =
                (i / (float)_rimSegments) * Mathf.PI * 2f;

            Vector3 point = center +
                (right * Mathf.Cos(angle) +
                 up * Mathf.Sin(angle)) * radius;

            Gizmos.DrawLine(origin, point);
        }

        const int dashCount = 12;

        for (int i = 0; i < dashCount; i += 2)
        {
            Vector3 start =
                origin + forward * (_range * i / dashCount);

            Vector3 end =
                origin + forward * (_range * (i + 1) / dashCount);

            Gizmos.DrawLine(start, end);
        }

        if (_playerDetected)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(_debugRayFrom, _debugRayTo);
            Gizmos.DrawWireSphere(_debugRayTo, 0.15f);
        }
    }

    #endregion

    #region Gameover Event

    private void OnGameoverEvent(GameoverEvent gameoverEvent)
    {
        if (_isGameover)
            return;

        _isGameover = true;
        _playerDetected = false;
        _targetRoot = null;

    }

    private void OnGameClearEvent(GameClearEvent _) => OnGameoverEvent(default);

    #endregion

    #region Detection

    private void FindVisiblePlayer()
    {
        _targetRoot = null;
        _playerDetected = false;

        Vector3 origin = GetOrigin();

        Collider[] candidates = Physics.OverlapSphere(
            origin,
            _range,
            _livingMask,
            QueryTriggerInteraction.Ignore);

        string lastReason =
            "ไม่พบ Tag ของ Target ใน Collider หรือ Parent";

        foreach (Collider candidate in candidates)
        {
            if (!candidate || IsOwnShelf(candidate))
                continue;

            Transform playerRoot =
                FindTargetRoot(candidate.transform);

            if (!playerRoot)
                continue;

            Vector3 aimPoint =
                playerRoot.position + _aimOffset;

            Vector3 toPlayer = aimPoint - origin;

            if (toPlayer.sqrMagnitude < 0.000001f)
                continue;

            float angle = Vector3.Angle(
                transform.forward,
                toPlayer);

            if (angle > _viewAngle * 0.5f)
            {
                lastReason =
                    $"ผู้เล่นอยู่นอกกรวย: {angle:F1} องศา";
                continue;
            }

            if (!HasLineOfSight(origin, aimPoint, playerRoot, out string blocker))
            {
                lastReason = $"มีสิ่งกีดขวาง: {blocker}";
                continue;
            }


            _targetRoot = playerRoot;
            _playerDetected = true;

            _debugRayFrom = origin;
            _debugRayTo = aimPoint;

            return;
        }

    }

    private Transform FindTargetRoot(Transform candidate)
    {
        Transform current = candidate;

        while (current != null)
        {
            if (_targetTags.MatchesUnityTag(current.tag))
                return current;

            current = current.parent;
        }

        return null;
    }

    private bool HasLineOfSight(
        Vector3 origin,
        Vector3 aimPoint,
        Transform playerRoot,
        out string blocker)
    {
        blocker = string.Empty;

        Vector3 direction = aimPoint - origin;
        float distance = direction.magnitude;

        if (distance <= 0.0001f)
            return true;

        RaycastHit[] hits = Physics.RaycastAll(
            origin,
            direction / distance,
            distance,
            Physics.AllLayers,
            QueryTriggerInteraction.Ignore);

        System.Array.Sort(
            hits,
            (a, b) => a.distance.CompareTo(b.distance));

        foreach (RaycastHit hit in hits)
        {
            Collider hitCollider = hit.collider;

            if (!hitCollider || IsOwnShelf(hitCollider))
                continue;

            Transform hitTransform = hitCollider.transform;

            if (hitTransform == playerRoot ||
                hitTransform.IsChildOf(playerRoot))
            {
                return true;
            }

            int layerBit = 1 << hitCollider.gameObject.layer;

            if ((_visionBlockMask.value & layerBit) != 0)
            {
                blocker = hitCollider.name;
                return false;
            }
        }

        return true;
    }

    private bool IsOwnShelf(Collider col)
    {
        return col &&
            (col.transform == transform ||
             col.transform.IsChildOf(transform));
    }

    private Vector3 GetOrigin()
    {
        return transform.position +
               transform.TransformVector(_eyeOffset);
    }

    #endregion

    #region Throw

    private void Fire()
    {
        if (_isGameover)
            return;

        if (!_bookPrefab || !_targetRoot)
        {
            DebugLog.Log(DebugCategory.Combat, "ยิงไม่ได้: ไม่มี Prefab หรือ Target", this, _debugLog);
            return;
        }

        Vector3 origin = GetOrigin();
        Vector3 aimPoint = _targetRoot.position + _aimOffset;
        Vector3 direction = (aimPoint - origin).normalized;

        if (direction.sqrMagnitude < 0.000001f)
            direction = transform.forward;

        Quaternion rotation = Quaternion.LookRotation(direction);

        if (!ObjectPoolManager.Instance)
        {
            DebugLog.Log(DebugCategory.ObjectPool, "ยิงไม่ได้: ไม่พบ ObjectPoolManager", this, _debugLog);
            return;
        }

        Book book = ObjectPoolManager.Instance.Get<Book>(
            _bookPrefab.name,
            origin,
            rotation);

        if (!book)
        {
            DebugLog.Log(DebugCategory.ObjectPool,
                $"ดึงหนังสือจาก Pool ไม่สำเร็จ: {_bookPrefab.name}", this, _debugLog);
            return;
        }

        book.Launch(
            direction,
            _throwForce,
            _shelfColliders,
            countsAsPlayerBook: true);

        _ammoLeft--;
        _nextShotTime = Time.time + _cooldown;

        DebugLog.Log(DebugCategory.Combat,
            $"ยิงหนังสือใส่ '{_targetRoot.name}' เหลือ {_ammoLeft} นัด", this, _debugLog);
    }

    #endregion

    #region Public

    public int AmmoLeft => _ammoLeft;
    public bool PlayerDetected => _playerDetected;

    #endregion
}
