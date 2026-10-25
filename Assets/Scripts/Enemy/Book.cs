using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Book : MonoBehaviour
{
    [SerializeField] private Rigidbody _rigidbody;
    [SerializeField] private LayerMask _bounceLayers;
    [SerializeField, Range(0.01f, 1f)] private float _bounceSpeedMultiplier = 0.25f;
    [SerializeField, Min(0f)] private float _stopSpeedThreshold = 0.2f;

    private bool _hasLaunched;
    private bool _launchImpulsePending;
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

        _bounceLayers |= LayerMask.GetMask("Wall", "Obstacle", "Marker", "Ground");
    }

    private bool _countsAsPlayerBook;
    private bool _hasKilledLibrarian;

    private void OnEnable()
    {
        _countsAsPlayerBook = false;
        _hasKilledLibrarian = false;
        _hasLaunched = false;
        _launchImpulsePending = false;
        _velocityBeforePhysicsStep = Vector3.zero;
        _lastBounceFixedTime = float.NegativeInfinity;
    }

    private void FixedUpdate()
    {
        if (!_hasLaunched || !_rigidbody)
            return;

        // AddForce(Impulse) is applied by the next physics step. Keep the
        // launch speed for a collision in that first step.
        if (_launchImpulsePending)
        {
            _launchImpulsePending = false;
            return;
        }

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

        _hasLaunched = true;
        _launchImpulsePending = true;
        _velocityBeforePhysicsStep = direction.normalized * (force / _rigidbody.mass);

        _rigidbody.AddForce(
            direction.normalized * force,
            ForceMode.Impulse);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!_hasLaunched || !_rigidbody || collision == null || !IsBounceLayer(collision.collider.transform))
            return;

        if (_lastBounceFixedTime == Time.fixedTime)
            return;

        Vector3 incomingVelocity = _velocityBeforePhysicsStep;
        if (incomingVelocity.sqrMagnitude < 0.000001f)
            incomingVelocity = collision.relativeVelocity;

        if (collision.contactCount == 0 || incomingVelocity.sqrMagnitude < 0.000001f)
            return;

        Vector3 normal = collision.GetContact(0).normal.normalized;
        float approachSpeed = -Vector3.Dot(incomingVelocity, normal);

        // A book leaving its spawn surface or skimming a floor has not hit it
        // head-on. Do not drain the launch impulse in those contacts.
        if (approachSpeed < 0.5f)
            return;

        _lastBounceFixedTime = Time.fixedTime;

        float reboundSpeed = incomingVelocity.magnitude * _bounceSpeedMultiplier;
        if (reboundSpeed <= _stopSpeedThreshold)
        {
            StopBook();
            return;
        }

        _rigidbody.linearVelocity = Vector3.Reflect(incomingVelocity, normal) * _bounceSpeedMultiplier;
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
        _launchImpulsePending = false;
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
