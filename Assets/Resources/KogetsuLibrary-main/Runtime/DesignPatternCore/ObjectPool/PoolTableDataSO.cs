using System;

namespace Kogetsu.Library.DesignPatternCore
{
    [CreateAssetMenu(fileName = "PoolTable_", menuName = "KogetsuLibrary/DesignPattern/PoolTable")]
    public sealed class PoolTableDataSO : ScriptableObject
    {
        [Serializable]
        public struct PoolEntry
        {
            public GameObject Prefab;
            public int InitialSize;
            public int MaxSize;
            public int LimitOverPercent;
        }

        public List<PoolEntry> Entries = new();
    }
}