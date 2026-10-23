#if UNITY_EDITOR

using UnityEditor;
using Kogetsu.Library.DesignPatternCore;

public class FileNavigator : Editor
{
    [MenuItem("Tools/Go to EventBus Prefab")]
    public static void NavigateToEventBus()
    {
        
        #region Documentation - Notes
        // Path ของไฟล์ที่ต้องการ (ใช้ Path เดิมที่คุณให้มาได้เลย)
        #endregion
        string assetPath = Kogetsu.Library.Editor.LibraryAssetLocator.Find("EventBus.prefab");

        #region Documentation - Notes
        // โหลด Asset จาก Path
        #endregion
        Object asset = AssetDatabase.LoadAssetAtPath<Object>(assetPath);

        if (asset != null)
        {
            #region Documentation - Notes
            // เลือกไฟล์นั้นใน Project Window
            #endregion
            Selection.activeObject = asset;

            #region Documentation - Notes
            // สั่งให้ Project Window กระพริบ (Ping) ที่ไฟล์นั้นเพื่อให้หาเจอได้ง่าย
            #endregion
            EditorGUIUtility.PingObject(asset);

            Debug.Log("Found and highlighted: " + assetPath);
        }
        else
        {
            Debug.LogError("ไม่พบไฟล์ที่ Path: " + assetPath + " กรุณาเช็คว่าชื่อ Package หรือชื่อไฟล์ถูกต้องหรือไม่");
        }
    }
}
#endif
