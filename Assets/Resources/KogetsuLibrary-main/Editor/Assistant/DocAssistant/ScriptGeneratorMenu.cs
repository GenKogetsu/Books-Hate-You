#if UNITY_EDITOR
using UnityEditor;

namespace Kogetsu.Library.Assistant
{
    public static class ScriptGeneratorMenuPackages
    {
        private static string _interfaceTemplatePath => Editor.LibraryAssetLocator.Find("InterfaceTemplate.txt");
        private static string _enumTemplatePath => Editor.LibraryAssetLocator.Find("EnumTemplate.txt");
        private static string _structTemplatePath => Editor.LibraryAssetLocator.Find("StructTemplate.txt");
        private static string _recordStructEventTemplatePath => Editor.LibraryAssetLocator.Find("RecordStructEventTemplate.txt");

        #region Documentation - Notes
        // --- เมนูในหน้าหลัก (ต่อจาก C# Script) ---
        #endregion
        [MenuItem("Assets/Create/Interface C# Script", false, 81)]
        public static void CreateInterface() => ProjectWindowUtil.CreateScriptAssetFromTemplateFile(_interfaceTemplatePath, "I.cs");

        [MenuItem("Assets/Create/Enum C# Script", false, 82)]
        public static void CreateEnum() => ProjectWindowUtil.CreateScriptAssetFromTemplateFile(_enumTemplatePath, "NewEnum.cs");

        [MenuItem("Assets/Create/Struct C# Script", false, 83)]
        public static void CreateStruct() => ProjectWindowUtil.CreateScriptAssetFromTemplateFile(_structTemplatePath, "NewStruct.cs");

        [MenuItem("Assets/Create/Record Struct Event C# Script", false, 84)]
        public static void CreateRecordStruct() => ProjectWindowUtil.CreateScriptAssetFromTemplateFile(_recordStructEventTemplatePath, "NewRecordStructEvent.cs");

        #region Documentation - Notes
        // --- เมนูในโฟลเดอร์ Scripting (ต่อจาก Script ของ Unity) ---
        #endregion
        [MenuItem("Assets/Create/Scripting/Interface C# Script", false, 81)]
        public static void CreateInterfaceScripting() => ProjectWindowUtil.CreateScriptAssetFromTemplateFile(_interfaceTemplatePath, "I.cs");

        [MenuItem("Assets/Create/Scripting/Enum C# Script", false, 82)]
        public static void CreateEnumScripting() => ProjectWindowUtil.CreateScriptAssetFromTemplateFile(_enumTemplatePath, "NewEnum.cs");

        [MenuItem("Assets/Create/Scripting/Struct C# Script", false, 83)]
        public static void CreateStructScripting() => ProjectWindowUtil.CreateScriptAssetFromTemplateFile(_structTemplatePath, "NewStruct.cs");

        [MenuItem("Assets/Create/Scripting/Record Struct C# Script", false, 84)]
        public static void CreateRecordStructScripting() => ProjectWindowUtil.CreateScriptAssetFromTemplateFile(_recordStructEventTemplatePath, "NewRecordStructEvent.cs");
    }
}
#endif
