using UnityEngine;

namespace Kogetsu.Library.DesignPatternCore
{
    #region Documentation - XML
    /// <summary>
    /// <para> Summary : </para>
    /// <para> (TH) : คลาสพื้นฐานสำหรับสร้าง Singleton Pattern ในรูปแบบ MonoBehaviour </para>
    /// <para> (EN) : Base class for creating MonoBehaviour-based Singleton Pattern. </para>
    /// </summary>
    /// <typeparam name="T">MonoBehaviour ที่ต้องการทำให้เป็น Singleton</typeparam>
    #endregion
    public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;
        private static bool _applicationIsQuitting;

        #region Documentation - XML
        /// <summary>
        /// <para> Summary : </para>
        /// <para> (TH) : ติ๊กใน Inspector เพื่อให้ object คงอยู่ข้าม Scene </para>
        /// <para> (EN) : Toggle in Inspector to keep this object alive across scenes. </para>
        /// </summary>
        #endregion
        [SerializeField] protected bool UseDontDestroyOnLoad = true;

        #region Documentation - XML
        /// <summary>
        /// <para> Summary : </para>
        /// <para> (TH) : เข้าถึง Instance ของ Singleton ถ้ายังไม่มีจะพยายามหาใน Scene </para>
        /// <para> (EN) : Accesses the Singleton instance. If null, it attempts to find it in the scene. </para>
        /// </summary>
        /// <returns>
        /// <para> Return : </para>
        /// <para> (TH) : instance ที่ cache ไว้หรือค้นพบใน Scene; เป็น null เมื่อกำลังปิดแอปหรือหาไม่พบ โดยไม่สร้าง object ใหม่ </para>
        /// <para> (EN) : The cached or scene-discovered instance; null during application shutdown or when absent, without creating an object. </para>
        /// </returns>
        #endregion
        public static T Instance
        {
            get
            {
                if (_applicationIsQuitting) return null;

                if (_instance != null) return _instance;

                _instance = FindFirstObjectByType<T>();

                if (_instance == null)
                {
#if UNITY_EDITOR
                    Debug.LogError($"<b><color=#FF5252>[Singleton Error]</color></b> {typeof(T)} not found in scene.");
#endif
                }

                return _instance;
            }
        }

        #region Documentation - XML
        /// <summary>
        /// <para> Summary : </para>
        /// <para> (TH) : ตรวจสอบและกำหนดค่า Instance เมื่อ Object ถูกสร้างขึ้น พร้อมระบบป้องกัน Object ซ้ำซ้อน </para>
        /// <para> (EN) : Validates and assigns the Instance on Awake with duplicate prevention logic. </para>
        /// </summary>
        #endregion
        protected virtual void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this as T;

            if (UseDontDestroyOnLoad)
            {
                if (transform.parent != null)
                    transform.SetParent(null);

                DontDestroyOnLoad(gameObject);
            }
        }

        #region Documentation - XML
        /// <summary>
        /// <para> Summary : </para>
        /// <para> (TH) : แจ้งสถานะเมื่อแอปพลิเคชันกำลังจะปิด เพื่อป้องกันการสร้าง Instance ใหม่โดยไม่ตั้งใจ </para>
        /// <para> (EN) : Signals that the application is quitting to prevent accidental re-instantiation. </para>
        /// </summary>
        #endregion
        protected virtual void OnApplicationQuit()
        {
            _applicationIsQuitting = true;
        }
    }
}
