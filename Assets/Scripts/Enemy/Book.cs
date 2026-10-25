using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Book : MonoBehaviour
{
    [SerializeField] private Rigidbody _rigidbody;

    private void Reset()
    {
        TryGetComponent(out _rigidbody);
    }

    private void Awake()
    {
        if (!_rigidbody)
            TryGetComponent(out _rigidbody);
    }

    private bool _countsAsPlayerBook;
    private bool _hasKilledLibrarian;

    private void OnEnable()
    {
        _countsAsPlayerBook = false;
        _hasKilledLibrarian = false;
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

        _rigidbody.AddForce(
            direction.normalized * force,
            ForceMode.Impulse);
    }

    public void RecordLibrarianKillIfPlayerBook()
    {
        if (!_countsAsPlayerBook || _hasKilledLibrarian)
            return;

        _hasKilledLibrarian = true;
        PlayerRunSession.RecordLibrarianKilledWithBook();
    }
}
