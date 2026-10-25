using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Book : MonoBehaviour
{
    [SerializeField] private Rigidbody _rigidbody;
    [SerializeField] private LayerMask _bounceLayers;
    [SerializeField, Range(0.01f, 1f)] private float _bounceSpeedMultiplier = 0.25f;
    [SerializeField, Min(0f)] private float _stopSpeedThreshold = 0.2f;

    private bool _hasLaunched;
    private Vector3 _velocityBeforePhysicsStep;
    private float _lastBounceFixedTime = float.NegativeInfinity;
    private void Reset()
    {
        TryGetComponent(out _rigidbody);
    }

    private void Awake()
    {
        if (!_rigidbody)
            TryGetComponent(out _rigidbody);

        if (_bounceLayers.value == 0)
            _bounceLayers = LayerMask.GetMask("Wall", "Obstacle", "Marker", "Ground");
    }

    private bool _countsAsPlayerBook;
    private bool _hasKilledLibrarian;

    private void OnEnable()
    {
        _countsAsPlayerBook = false;
        _hasKilledLibrarian = false;
        _hasLaunched = false;
        _velocityBeforePhysicsStep = Vector3.zero;
        _lastBounceFixedTime = float.NegativeInfinity;
    }

    private void FixedUpdate()
    {
        if (_hasLaunched && _rigidbody)
            _velocityBeforePhysicsStep = _rigidbody.linearVelocity;
    }

    public void Launch(
        Vector3 direction,
        float force,
        Collider[] ignoredColliders,
        bool countsAsPlayerBook = false)
    {
        _countsAsPlayerBook = countsAsPlayerBook;
        _hasKilledLibrarian = false;
        _hasLaunched = true;
        if (!_rigidbody)
            TryGetComponent(out _rigidbody);

        if (!_rigidbody)
        {
            Debug.LogError(
                "[Book] ไม่พบ Rigidbody",
                this);
            return;
        }

        if (ignoredColliders != null)
        {
            Collider[] ownColliders =
                GetComponentsInChildren<Collider>(true);

            foreach (Collider own in ownColliders)
            {
                if (!own)
                    continue;

                foreach (Collider other in ignoredColliders)
                {
                    if (other)
                    {
                        Physics.IgnoreCollision(
                            own,
                            other,
                            true);
                    }
                }
            }
        }

        _rigidbody.linearVelocity = Vector3.zero;
        _rigidbody.angularVelocity = Vector3.zero;
        _rigidbody.WakeUp();

        _rigidbody.AddForce(
            direction.normalized * force,
            ForceMode.Impulse);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!_hasLaunched || !_rigidbody || collision == null || !IsBounceLayer(collision.collider.transform))
            return;

        if (Mathf.Approximately(_lastBounceFixedTime, Time.fixedTime))
            return;

        _lastBounceFixedTime = Time.fixedTime;

        Vector3 incomingVelocity = _velocityBeforePhysicsStep;
        if (incomingVelocity.sqrMagnitude < 0.000001f)
            incomingVelocity = _rigidbody.linearVelocity;

        if (collision.contactCount == 0 || incomingVelocity.sqrMagnitude < 0.000001f)
        {
            StopBook();
            return;
        }

        float reboundSpeed = incomingVelocity.magnitude * _bounceSpeedMultiplier;
        if (reboundSpeed <= _stopSpeedThreshold)
        {
            StopBook();
            return;
        }

        Vector3 normal = collision.GetContact(0).normal.normalized;
        _rigidbody.linearVelocity = Vector3.Reflect(incomingVelocity.normalized, normal) * reboundSpeed;
        _velocityBeforePhysicsStep = _rigidbody.linearVelocity;
        _rigidbody.WakeUp();
    }

    private bool IsBounceLayer(Transform surface)
    {
        for (Transform current = surface; current; current = current.parent)
        {
            if ((_bounceLayers.value & (1 << current.gameObject.layer)) != 0)
                return true;
        }

        return false;
    }

    private void StopBook()
    {
        _rigidbody.linearVelocity = Vector3.zero;
        _rigidbody.angularVelocity = Vector3.zero;
        _hasLaunched = false;
        _rigidbody.Sleep();
    }

    public void RecordLibrarianKillIfPlayerBook()
    {
        if (!_countsAsPlayerBook || _hasKilledLibrarian)
            return;

        _hasKilledLibrarian = true;
        PlayerRunSession.RecordLibrarianKilledWithBook();
    }
}
