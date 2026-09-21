using UnityEngine;

namespace Kogetsu.Library.DesignPatternCore
{
    /// <summary>
    /// <para> Summary : </para>
    /// <para> (TH) : ระบบจัดการ Pool ของ Object ชนิดเดียว รองรับการงอกเกินขีดจำกัดแบบเปอร์เซ็นต์ </para>
    /// <para> (EN) : Single type object pool system supporting percentage-based overflow. </para>
    /// </summary>
    /// <typeparam name="T">Component ใดๆ ที่ต้องการทำ Pooling (รองรับทั้ง Transform และ MonoBehaviour)</typeparam>
    public class ObjectPool<T> where T : Component
    {
        private readonly T _prefab;
        private readonly int _maxSize;
        private readonly int _overPercent;
        private readonly Transform _container;

        private readonly Queue<T> _queue = new();
        private readonly List<T> _activeItems = new();
        private int _totalCreatedCount = 0;

        /// <summary>
        /// <para> Summary : </para>
        /// <para> (TH) : จำนวน Object ที่ว่างอยู่ใน Pool </para>
        /// </summary>
        public int InactiveCount => _queue.Count;

        /// <summary>
        /// <para> Summary : </para>
        /// <para> (TH) : ขีดจำกัดสูงสุดมาตรฐาน </para>
        /// </summary>
        public int MaxSize => _maxSize;

        /// <summary>
        /// <para> Summary : </para>
        /// <para> (TH) : คอนสตรักเตอร์เริ่มต้นระบบ Pool </para>
        /// </summary>
        public ObjectPool(T prefab, int maxSize, int overPercent, Transform container)
        {
            _prefab = prefab;
            _maxSize = maxSize;
            _overPercent = overPercent;
            _container = container;
        }

        /// <summary>
        /// <para> Summary : </para>
        /// <para> (TH) : ดึง Object ออกมาใช้งาน </para>
        /// </summary>
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

        /// <summary>
        /// <para> Summary : </para>
        /// <para> (TH) : ส่งคืน Object กลับเข้า Pool </para>
        /// </summary>
        public void Return(T item)
        {
            if (item == null) return;

            if (_activeItems.Contains(item)) _activeItems.Remove(item);
            _queue.Enqueue(item);
        }

        /// <summary>
        /// <para> Summary : </para>
        /// <para> (TH) : เรียกคืน Object ทั้งหมดที่กำลัง Active กลับเข้า Pool </para>
        /// </summary>
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

        /// <summary>
        /// <para> Summary : </para>
        /// <para> (TH) : ทำลาย Object ทิ้งถาวร </para>
        /// </summary>
        public void DestroyItem(T item)
        {
            if (item == null) return;

            if (_activeItems.Contains(item)) _activeItems.Remove(item);
            _totalCreatedCount--;
            Object.Destroy(item.gameObject);
        }
    }
}