using Kogetsu.Library.Attribute;
using Kogetsu.Library.DesignPatternCore;

namespace Kogetsu.Library.Core;

#region Documentation - XML
/// <summary>
/// <para> Summary : </para>
/// <para> (TH) : พลังงานร่วมระหว่างปืนและไฟฉาย GunShooter ใช้ TrySpend() ตอนยิง FlashlightController ใช้ Drain() ทุก frame ตอนเปิดไฟ </para>
/// <para> (EN) : Shares an energy reserve between gun spending and per-frame flashlight drain. </para>
/// </summary>
#endregion
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

    #region Documentation - XML
    /// <summary>
    /// <para> Summary : </para>
    /// <para> (TH) : ใช้พลังงาน — คืน false ถ้าไม่พอ ไม่หักถ้า fail </para>
    /// <para> (EN) : Spends the requested energy only when enough remains; returns false without spending on failure. </para>
    /// </summary>
    /// <returns>
    /// <para> Return : </para>
    /// <para> (TH) : true เมื่อหักพลังงานแล้ว; false เมื่อพลังงานไม่พอ โดยไม่หักหรือ Publish ในกรณีนั้น </para>
    /// <para> (EN) : True after spending energy; false when insufficient, with no spending or publication in that case. </para>
    /// </returns>
    #endregion
    public bool TrySpend(float amount)
    {
        if (_currentEnergy < amount) return false;
        _currentEnergy -= amount;
        Publish();
        return true;
    }

    #region Documentation - XML
    /// <summary>
    /// <para> Summary : </para>
    /// <para> (TH) : ดูดพลังงานต่อเนื่อง (flashlight drain) — ไม่มี fail </para>
    /// <para> (EN) : Drains continuous energy use and clamps the reserve without a failure result. </para>
    /// </summary>
    #endregion
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
