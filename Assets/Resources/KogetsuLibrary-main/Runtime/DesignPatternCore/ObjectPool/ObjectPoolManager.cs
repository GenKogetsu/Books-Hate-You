using Kogetsu.Library.Attribute;

namespace Kogetsu.Library.DesignPatternCore
{
    /// <summary>
    /// <para> Summary : </para>
    /// <para> (TH) : ศูนย์กลางจัดการ Object Pool ทั้งหมดในเกม คอยเริ่มการทำงานและกระจายคำสั่ง Get/Release </para>
    /// <para> (EN) : Central Object Pool manager handling initialization and Get/Release requests. </para>
    /// </summary>
    [CreateHierarchyMenu("KogetsuLibrary/DesignPattern")]
    public sealed class ObjectPoolManager : Singleton<ObjectPoolManager>
    {
        [Header("Pool Data Configuration")]
        [SerializeField] private List<PoolTableDataSO> _poolTables = new();

        private readonly Dictionary<string, ObjectPool<Transform>> _poolDictionary = new();

        /// <summary>
        /// <para> Summary : </para>
        /// <para> (TH) : เริ่มต้นระบบและเตรียมค่าเริ่มต้นสำหรับทุก Pool ที่ระบุไว้ </para>
        /// <para> (EN) : Initializes the system and pre-warms all configured pools. </para>
        /// </summary>
        protected override void Awake()
        {
            base.Awake();
            ExecuteInitialize();
        }

        /// <summary>
        /// <para> Summary : </para>
        /// <para> (TH) : ดึง Object จาก Pool ออกมาใช้งานตามชื่อ Key (ชื่อ Prefab) </para>
        /// <para> (EN) : Retrieves an object from the pool by key name. </para>
        /// </summary>
        public T Get<T>(string key, Vector3 position, Quaternion rotation) where T : MonoBehaviour
        {
            if (!_poolDictionary.TryGetValue(key, out var pool))
            {
#if UNITY_EDITOR
                Debug.LogError($"<b><color=#FF5252>[Pool Get Fail]</color></b> Key: <b>{key}</b> not found!");
#endif
                return null;
            }

            var item = pool.Get();

            if (item == null) return null;

            item.SetPositionAndRotation(position, rotation);
            item.gameObject.SetActive(true);
            return item.GetComponent<T>();
        }

        /// <summary>
        /// <para> Summary : </para>
        /// <para> (TH) : ส่งคืน Object กลับเข้า Pool เมื่อเลิกใช้งาน พร้อมระบบควบคุมจำนวนเกินขีดจำกัด </para>
        /// <para> (EN) : Returns an object to the pool with capacity control logic. </para>
        /// </summary>
        public void Release(string key, Component item)
        {
            if (item == null) return;

            if (!_poolDictionary.TryGetValue(key, out var pool))
            {
                Destroy(item.gameObject);
#if UNITY_EDITOR
                Debug.LogWarning($"<b><color=#FF1744>[Pool Error]</color></b> Key <b>{key}</b> not found during release! Destroying object.");
#endif
                return;
            }

            item.gameObject.SetActive(false);

            if (pool.InactiveCount >= pool.MaxSize)
            {
                pool.DestroyItem(item.transform);
#if UNITY_EDITOR
                Debug.Log($"<b><color=#F06292>[Pool Capacity Control]</color></b> <b>{key}</b> เกินค่า Max ({pool.MaxSize}) ทำลายทิ้งทันที");
#endif
                return;
            }

            pool.Return(item.transform);
#if UNITY_EDITOR
            Debug.Log($"<b><color=#69F0AE>[Pool Return]</color></b> <b>{key}</b> returned to pool.");
#endif
        }

        /// <summary>
        /// <para> Summary : </para>
        /// <para> (TH) : เรียกใช้เพื่อเคลียร์ Object ทั้งหมดในฉากกลับเข้า Pool (แนะนำให้เรียกก่อนโหลด Scene ใหม่) </para>
        /// <para> (EN) : Returns all active objects from all pools back to their respective containers. </para>
        /// </summary>
        public void ReleaseAllPools()
        {
            foreach (var kvp in _poolDictionary)
            {
                kvp.Value.ReturnAllActive();
            }
#if UNITY_EDITOR
            Debug.Log("<b><color=#FFEB3B>[PoolManager]</color></b> Clear All Object To Pool");
#endif
        }

        /// <summary>
        /// <para> Summary : </para>
        /// <para> (TH) : ประมวลผลการสร้าง Pool และทำ Pre-warm ตามค่าที่กำหนดใน Configuration </para>
        /// <para> (EN) : Processes pool creation and pre-warms objects based on configuration data. </para>
        /// </summary>
        private void ExecuteInitialize()
        {
            foreach (var table in _poolTables)
            {
                if (table == null) continue;

                foreach (var entry in table.Entries)
                {
                    if (entry.Prefab == null) continue;

                    var key = entry.Prefab.name;

                    if (_poolDictionary.ContainsKey(key)) continue;

                    var containerObj = new GameObject($"Pool_{key}");
                    containerObj.transform.SetParent(transform);

                    ObjectPool<Transform> pool = new(
                        entry.Prefab.transform,
                        entry.MaxSize,
                        entry.LimitOverPercent,
                        containerObj.transform
                    );

                    _poolDictionary.Add(key, pool);

                    var countToPreWarm = Mathf.Min(entry.InitialSize, entry.MaxSize);
                    List<Transform> tempStack = new();

                    for (int i = 0; i < countToPreWarm; i++)
                    {
                        var item = pool.Get();

                        if (item == null) break;

                        item.gameObject.SetActive(false);
                        tempStack.Add(item);
                    }

                    foreach (var item in tempStack)
                    {
                        pool.Return(item);
                    }

#if UNITY_EDITOR
                    Debug.Log($"<b><color=#4FC3F7>[Pool Initialized]</color></b> <b>{key}</b> (Init: {countToPreWarm} | Max: {entry.MaxSize})");
#endif
                }
            }
        }
    }
}