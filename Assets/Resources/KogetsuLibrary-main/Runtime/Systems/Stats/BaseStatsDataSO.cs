namespace Kogetsu.Library.DesignPatternCore
{
    [CreateAssetMenu(fileName = "BaseStatsData", menuName = "KogetsuLibrary/DesignPattern/DataAssets/BaseStatsData")]
    public class BaseStatsDataSO : ScriptableObject
    {
        public float BaseMoveSpeed = 5f;
        public float BaseJumpForce = 10f;

        public float BaseHp      = 100f;
        public float BaseStamina = 100f;
    }
}
