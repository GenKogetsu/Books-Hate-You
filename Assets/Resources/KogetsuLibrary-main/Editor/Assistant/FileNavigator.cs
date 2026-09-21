#if UNITY_EDITOR

using UnityEditor;
using Kogetsu.Library.DesignPatternCore;

public class FileNavigator : Editor
{
    [MenuItem("Tools/Go to EventBus Prefab")]
    public static void NavigateToEventBus()
    {
        
        // Path ของไฟล์ที่ต้องการ (ใช้ Path เดิมที่คุณให้มาได้เลย)
        string assetPath = "Packages/com.Kogetsu.Library/Runtime/DesignPatternCore/EventBus/EventBus.prefab";

        // โหลด Asset จาก Path
        Object asset = AssetDatabase.LoadAssetAtPath<Object>(assetPath);

        if (asset != null)
        {
            // เลือกไฟล์นั้นใน Project Window
            Selection.activeObject = asset;

            // สั่งให้ Project Window กระพริบ (Ping) ที่ไฟล์นั้นเพื่อให้หาเจอได้ง่าย
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