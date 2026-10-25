using UnityEngine;

namespace Kogetsu.Library.DesignPatternCore
{
    #region Documentation - XML
    /// <summary>
    /// <para> Summary : </para>
    /// <para> (TH) : ระบบจัดการ Pool ของ Object ชนิดเดียว รองรับการงอกเกินขีดจำกัดแบบเปอร์เซ็นต์ </para>
    /// <para> (EN) : Single type object pool system supporting percentage-based overflow. </para>
    /// </summary>
    /// <typeparam name="T">Component ใดๆ ที่ต้องการทำ Pooling (รองรับทั้ง Transform และ MonoBehaviour)</typeparam>
    #endregion
    public class ObjectPool<T> where T : Component
    {
        private readonly T _prefab;
        private readonly int _maxSize;
        private readonly int _overPercent;
        private readonly Transform _container;

        private readonly Queue<T> _queue = new();
        private readonly List<T> _activeItems = new();
        private int _totalCreatedCount = 0;

        #region Documentation - XML
        /// <summary>
        /// <para> Summary : </para>
        /// <para> (TH) : จำนวน Object ที่ว่างอยู่ใน Pool </para>
        /// <para> (EN) : Number of entries currently waiting in the inactive queue. </para>
        /// </summary>
        /// <returns>
        /// <para> Return : </para>
        /// <para> (TH) : จำนวน entry ในคิวที่รอ reuse; ไม่ใช่จำนวน GameObject ทั้งหมด </para>
        /// <para> (EN) : Queue entries waiting for reuse, not the total GameObject count. </para>
        /// </returns>
        #endregion
        public int InactiveCount => _queue.Count;

        public bool ContainsActive(T item) => item != null && _activeItems.Contains(item);

        #region Documentation - XML
        /// <summary>
        /// <para> Summary : </para>
        /// <para> (TH) : ขีดจำกัดสูงสุดมาตรฐาน </para>
        /// <para> (EN) : Standard pool capacity, before percentage overflow is applied. </para>
        /// </summary>
        /// <returns>
        /// <para> Return : </para>
        /// <para> (TH) : capacity มาตรฐานที่ยังไม่รวม LimitOverPercent </para>
        /// <para> (EN) : Standard capacity before applying overflow percentage. </para>
        /// </returns>
        #endregion
        public int MaxSize => _maxSize;

        #region Documentation - XML
        /// <summary>
        /// <para> Summary : </para>
        /// <para> (TH) : คอนสตรักเตอร์เริ่มต้นระบบ Pool </para>
        /// <para> (EN) : Initializes prefab, standard capacity, overflow percentage and parent container for this pool. </para>
        /// </summary>
        #endregion
        public ObjectPool(T prefab, int maxSize, int overPercent, Transform container)
        {
            _prefab = prefab;
            _maxSize = maxSize;
            _overPercent = overPercent;
            _container = container;
        }

        #region Documentation - XML
        /// <summary>
        /// <para> Summary : </para>
        /// <para> (TH) : ดึง Object ออกมาใช้งาน </para>
        /// <para> (EN) : Dequeues an inactive object or creates one within the hard limit, then tracks it as active. </para>
        /// </summary>
        /// <returns>
        /// <para> Return : </para>
        /// <para> (TH) : object ที่ติดตามว่า active หรือ null เมื่อถึง hard limit; ตัว pool ไม่ได้เปิด GameObject ที่ reuse ให้อัตโนมัติ </para>
        /// <para> (EN) : An object tracked as active, or null at the hard limit; the pool does not automatically reactivate reused GameObjects. </para>
        /// </returns>
        #endregion
        public T Get()
        {
            if (_queue.Count > 0)
            {
                T pooledItem = _queue.Dequeue();
#if UNITY_EDITOR
                Debug.Log($"<b><color=#FFEB3B>[Pool Reuse]</color></b> ♻️ Dequeued: <b>{_prefab.name}</b> (In Queue: {_queue.Count})");
#endif
                _activeItems.Add(pooledItem);
                return pooledItem;
            }

            var bonus = _maxSize * (_overPercent / 100f);
            var finalLimit = _maxSize + Mathf.CeilToInt(bonus);

            if (_totalCreatedCount >= finalLimit)
            {
#if UNITY_EDITOR
                Debug.LogWarning($"<b><color=#FF1744>[Pool Limit]</color></b> {_prefab.name} reached HARD LIMIT ({finalLimit})!");
#endif
                return null;
            }

            T newItem = Object.Instantiate(_prefab, _container);
            newItem.name = _prefab.name;
            _totalCreatedCount++;
            _activeItems.Add(newItem);

#if UNITY_EDITOR
            string color = _totalCreatedCount > _maxSize ? "#FFCA28" : "#FF8A65";
            Debug.Log($"<b><color={color}>[Pool Create]</color></b> Created NEW: <b>{_prefab.name}</b> ({_totalCreatedCount}/{finalLimit})");
#endif
            return newItem;
        }

        #region Documentation - XML
        /// <summary>
        /// <para> Summary : </para>
        /// <para> (TH) : เตรียม object ล่วงหน้าด้วย Get แล้วปิด GameObject ก่อนคืนด้วย Return; จำนวนที่ขอถูกจำกัดไม่เกิน MaxSize </para>
        /// <para> (EN) : Prepares objects through Get, deactivates them, then returns them through Return; the requested count is capped at MaxSize. </para>
        /// <para> Refactor : </para>
        /// <para> (TH) : ย้ายขั้นตอน prewarm จาก ObjectPoolManager มาอยู่ใน pool เพื่อใช้ lifecycle เดียวกับผู้เรียกทั่วไป </para>
        /// <para> (EN) : Moves prewarming out of ObjectPoolManager so preparation uses the pool's own consumer lifecycle. </para>
        /// <para> Notes : </para>
        /// <para> (TH) : หยุดเมื่อ Get คืน null; object ที่ Instantiate อาจเรียก Awake/OnEnable ก่อนถูกปิด และเมธอดนี้ไม่ได้รับประกันว่าจะเพิ่มสำรองใหม่เท่ากับ count หากมี object ว่างอยู่แล้ว </para>
        /// <para> (EN) : Stops if Get returns null; newly instantiated objects may run Awake/OnEnable before deactivation, and existing reserves can satisfy the request without creating count new objects. </para>
        /// </summary>
        /// <param name="count">จำนวนสูงสุดที่จะดึงออกมาเตรียมในรอบนี้ / Maximum number of objects to prepare in this pass.</param>
        #endregion
        public void Prewarm(int count)
        {
            var items = new List<T>();
            for (int i = 0; i < Mathf.Min(count, MaxSize); i++)
            {
                T item = Get();
                if (item == null) break;
                item.gameObject.SetActive(false);
                items.Add(item);
            }
            foreach (T item in items) Return(item);
        }

        #region Documentation - XML
        /// <summary>
        /// <para> Summary : </para>
        /// <para> (TH) : ส่งคืน Object กลับเข้า Pool </para>
        /// <para> (EN) : Removes a non-null item from active tracking and enqueues it for reuse. </para>
        /// </summary>
        #endregion
        public void Return(T item)
        {
            if (item == null) return;

            if (_activeItems.Contains(item)) _activeItems.Remove(item);
            _queue.Enqueue(item);
        }

        #region Documentation - XML
        /// <summary>
        /// <para> Summary : </para>
        /// <para> (TH) : เรียกคืน Object ทั้งหมดที่กำลัง Active กลับเข้า Pool </para>
        /// <para> (EN) : Deactivates tracked live objects, enqueues them for reuse, then clears active tracking. </para>
        /// </summary>
        #endregion
        public void ReturnAllActive()
        {
            for (int i = _activeItems.Count - 1; i >= 0; i--)
            {
                T item = _activeItems[i];
                if (item == null) continue;

                item.gameObject.SetActive(false);
                _queue.Enqueue(item);
            }
            _activeItems.Clear();
        }

        #region Documentation - XML
        /// <summary>
        /// <para> Summary : </para>
        /// <para> (TH) : ทำลาย Object ทิ้งถาวร </para>
        /// <para> (EN) : Removes an item from active tracking, decreases the created count and destroys its GameObject. </para>
        /// </summary>
        #endregion
        public void DestroyItem(T item)
        {
            if (item == null) return;

            if (_activeItems.Contains(item)) _activeItems.Remove(item);
            _totalCreatedCount--;
            Object.Destroy(item.gameObject);
        }
    }
}
