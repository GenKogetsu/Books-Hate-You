namespace Kogetsu.Library.Core;

/// <summary>
/// กระสุนเดี่ยว (pellet) — เคลื่อนที่ผ่าน Transform.Translate (ไม่ใช้ Rigidbody)
/// Return to pool เมื่อเดินทางครบ ShootRange หรือ ชน Enemy/Ground/Wall
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class ElectricBullet : MonoBehaviour
{
    [Header("VFX (optional)")]
    [SerializeField] private ParticleSystem SparkVFX;
    [SerializeField] private ParticleSystem HitVFX;

    private Vector2           _dir;
    private float             _speed;
    private float             _maxRange;
    private float             _traveled;
    private BulletPoolManager _pool;

    public void Launch(Vector2 direction, float speed, float maxRange, BulletPoolManager pool)
    {
        _dir      = direction.normalized;
        _speed    = speed;
        _maxRange = maxRange;
        _traveled = 0f;
        _pool     = pool;

        SparkVFX?.Play();
    }

    private void Update()
    {
        float step = _speed * Time.deltaTime;
        transform.Translate(_dir * step, Space.World);
        _traveled += step;

        if (_traveled >= _maxRange)
            ReturnToPool();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy") || other.CompareTag("Ground") || other.CompareTag("Wall"))
            ReturnToPool();
    }

    private void ReturnToPool()
    {
        if (HitVFX != null)
        {
            HitVFX.transform.SetParent(null);
            HitVFX.Play();
        }

        gameObject.SetActive(false);
        _pool?.Return(this);
    }
}
