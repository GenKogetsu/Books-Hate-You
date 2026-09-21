using Kogetsu.Library.Attribute;
using Kogetsu.Library.DesignPatternCore;

namespace Kogetsu.Library.Core;

/// <summary>
/// พลังงานร่วมระหว่างปืนและไฟฉาย
/// GunShooter ใช้ TrySpend() ตอนยิง
/// FlashlightController ใช้ Drain() ทุก frame ตอนเปิดไฟ
/// </summary>
public class EnergySystem : MonoBehaviour
{
    [SerializeField] private float _maxEnergy    = 100f;
    [SerializeField] private float _rechargeRate = 5f;

    [ReadOnly]
    [SerializeField] private float _currentEnergy;

    public float Current => _currentEnergy;
    public float Max     => _maxEnergy;
    public float Ratio   => _maxEnergy > 0f ? _currentEnergy / _maxEnergy : 0f;

    private void Awake() => _currentEnergy = _maxEnergy;

    private void Update()
    {
        if (_rechargeRate > 0f && _currentEnergy < _maxEnergy)
            Add(_rechargeRate * Time.deltaTime);
    }

    /// <summary>ใช้พลังงาน — คืน false ถ้าไม่พอ ไม่หักถ้า fail</summary>
    public bool TrySpend(float amount)
    {
        if (_currentEnergy < amount) return false;
        _currentEnergy -= amount;
        Publish();
        return true;
    }

    /// <summary>ดูดพลังงานต่อเนื่อง (flashlight drain) — ไม่มี fail</summary>
    public void Drain(float amount)
    {
        _currentEnergy = Mathf.Clamp(_currentEnergy - amount, 0f, _maxEnergy);
        Publish();
    }

    public void Add(float amount)
    {
        _currentEnergy = Mathf.Clamp(_currentEnergy + amount, 0f, _maxEnergy);
        Publish();
    }

    private void Publish()
    {
        if (EventBus.Instance)
            EventBus.Instance.Publish(new EnergyChangedEvent(_currentEnergy, _maxEnergy));
    }
}
