using Kogetsu.Library.DesignPatternCore;

namespace Kogetsu.Library.Core;

/// <summary>
/// ห่อ ObjectPool&lt;ElectricBullet&gt; ของ library
/// Setup: Assign BulletPrefab, ตั้ง MaxSize (แนะนำ 60 สำหรับ 7 pellets × fire rate)
/// </summary>
public class BulletPoolManager : MonoBehaviour
{
    [SerializeField] private ElectricBullet BulletPrefab;
    [SerializeField] private int            MaxSize     = 60;
    [SerializeField] private int            OverPercent = 20;

    private ObjectPool<ElectricBullet> _pool;

    private void Awake() =>
        _pool = new ObjectPool<ElectricBullet>(BulletPrefab, MaxSize, OverPercent, transform);

    public ElectricBullet Get()                   => _pool.Get();
    public void Return(ElectricBullet bullet)      => _pool.Return(bullet);
}
