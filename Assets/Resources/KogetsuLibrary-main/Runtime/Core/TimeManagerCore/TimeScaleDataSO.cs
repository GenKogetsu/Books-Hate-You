namespace Kogetsu.Library.Core
{
    [CreateAssetMenu(fileName = "TimeScaleData", menuName = "KogetsuLibrary/Core/TimeScaleData")]
    public class TimeScaleDataSO : ScriptableObject
    {
        public float BaseAcceleratorScale = 1.0f;
        public float BaseAccelerationMultiplier = 1.0f;
    }
}