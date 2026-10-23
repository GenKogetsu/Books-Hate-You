using System;

namespace Kogetsu.Library.Attribute
{
    #region Documentation - XML
    /// <summary>
    /// <para> Summary : </para>
    /// <para> (TH) : กำหนด menu path ให้ MonoBehaviour เพื่อ generate เมนูสร้าง prefab ทุกตัวที่มี component นี้ โดยใช้ชื่อ prefab ที่ค้นพบ </para>
    /// <para> (EN) : Marks a MonoBehaviour class so HierarchyMenuGenerator produces "GameObject / [MenuPath] / [PrefabName]" entries for every prefab in the project that contains this component. The prefab name is discovered automatically — no need to declare it: </para>
    /// <code>
    ///   [CreateHierarchyMenu("KogetsuLibrary/Core/Manager")]
    ///   public class BasicInputManager : MonoBehaviour { }
    /// </code>
    /// <para> (EN) : If two prefabs share the same component they both appear in the menu, each under its own prefab name. </para>
    /// </summary>
    #endregion
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class CreateHierarchyMenu : System.Attribute
    {
        #region Documentation - XML
        /// <summary>
        /// <para> Summary : </para>
        /// <para> (TH) : ส่วนของ menu path ที่ต่อจาก GameObject/ เช่น KogetsuLibrary/Core/Manager </para>
        /// <para> (EN) : Path after "GameObject/" in the menu, e.g. "KogetsuLibrary/Core/Manager". </para>
        /// </summary>
        #endregion
        public string MenuPath { get; }

        public CreateHierarchyMenu(string menuPath) => MenuPath = menuPath;
    }
}
