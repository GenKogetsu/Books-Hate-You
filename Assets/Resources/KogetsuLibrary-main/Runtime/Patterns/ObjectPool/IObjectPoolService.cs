using UnityEngine;

namespace Kogetsu.Library.DesignPatternCore
{
    #region Documentation - XML
    /// <summary>
    /// <para> Summary : </para>
    /// <para> (TH) : สัญญาดึงและคืน object ให้ผู้เรียกใช้ pool โดยไม่ผูกกับ manager ตัวใดตัวหนึ่ง </para>
    /// <para> (EN) : Pool API for consumers that should not depend on a manager implementation. </para>
    /// </summary>
    #endregion
    public interface IObjectPoolService
    {
        #region Documentation - XML
        /// <summary>
        /// <para> Summary : </para>
        /// <para> (TH) : ดึง object ตาม key กำหนดตำแหน่งและการหมุนแล้วเปิดใช้งาน; manager ปัจจุบันใช้ชื่อ prefab เป็น key </para>
        /// <para> (EN) : Acquires an object by key, positions and rotates it, then activates it; the current manager uses prefab names as keys. </para>
        /// </summary>
        /// <returns>
        /// <para> Return : </para>
        /// <para> (TH) : component T หรือ null เมื่อไม่มี pool, เต็ม capacity หรือไม่พบ T </para>
        /// <para> (EN) : The T component, or null if the pool is missing, capacity is exhausted, or T is absent. </para>
        /// </returns>
        #endregion
        T Get<T>(string key, Vector3 position, Quaternion rotation) where T : MonoBehaviour;
        #region Documentation - XML
        /// <summary>
        /// <para> Summary : </para>
        /// <para> (TH) : คืน component ให้ pool ตาม key; manager ปัจจุบันทำลาย object ถ้า key ไม่ตรง pool ใด </para>
        /// <para> (EN) : Returns a component to its keyed pool; the current manager destroys objects whose key has no pool. </para>
        /// </summary>
        #endregion
        void Release(string key, Component item);
        #region Documentation - XML
        /// <summary>
        /// <para> Summary : </para>
        /// <para> (TH) : เรียกคืน object ที่ติดตามว่า active จากทุก pool โดยไม่ใช้ข้อยกเว้นการโหลด Scene </para>
        /// <para> (EN) : Returns tracked active objects across all pools regardless of scene-load exemptions. </para>
        /// </summary>
        #endregion
        void ReleaseAllPools();
    }
}
