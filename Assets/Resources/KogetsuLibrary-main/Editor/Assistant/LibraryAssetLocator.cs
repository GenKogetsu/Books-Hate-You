using System;
using UnityEditor;
using UnityEngine;

namespace Kogetsu.Library.Editor
{
    #region Documentation - XML
    /// <summary>
    /// <para> Summary : </para>
    /// <para> (TH) : ค้นหา asset ของไลบรารีทั้งใน Assets และ Packages โดยไม่ผูกกับตำแหน่งติดตั้งตายตัว </para>
    /// <para> (EN) : Resolves library assets in Assets or Packages without a fixed installation path. </para>
    /// </summary>
    #endregion
    public static class LibraryAssetLocator
    {
        #region Documentation - XML
        /// <summary>
        /// <para> Summary : </para>
        /// <para> (TH) : ค้นหา asset ด้วยชื่อไฟล์รวม extension โดยตรวจว่าชื่อ path มี Kogetsu และไม่อยู่ใน Third Party Library </para>
        /// <para> (EN) : Finds an asset by filename including extension, requiring Kogetsu in the path and excluding Third Party Library. </para>
        /// <para> Refactor : </para>
        /// <para> (TH) : ใช้แทน path ตายตัวของเมนูและ template; หากมีหลายไฟล์ชื่อเดียวกันจะคืนรายการแรกที่ AssetDatabase ค้นพบ </para>
        /// <para> (EN) : Replaces fixed paths in menus and templates; duplicate filenames resolve to the first matching AssetDatabase result. </para>
        /// </summary>
        /// <param name="fileName">ชื่อไฟล์ที่ต้องตรงกัน เช่น EventBus.prefab / Exact filename, for example EventBus.prefab.</param>
        /// <returns>
        /// <para> Return : </para>
        /// <para> (TH) : path ของ asset ที่พบ ใช้ส่งต่อให้ AssetDatabase ได้ </para>
        /// <para> (EN) : Asset path suitable for subsequent AssetDatabase calls. </para>
        /// </returns>
        /// <exception cref="InvalidOperationException">ไม่พบไฟล์ที่ตรงเงื่อนไข / No matching library asset was found.</exception>
        #endregion
        public static string Find(string fileName)
        {
            string filter = System.IO.Path.GetFileNameWithoutExtension(fileName);
            foreach (string guid in AssetDatabase.FindAssets(filter))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (System.IO.Path.GetFileName(path) == fileName &&
                    path.IndexOf("Kogetsu", StringComparison.OrdinalIgnoreCase) >= 0 &&
                    path.IndexOf("Third Party Library", StringComparison.OrdinalIgnoreCase) < 0)
                    return path;
            }
            throw new InvalidOperationException($"KogetsuLibrary asset not found: {fileName}");
        }
    }
}
