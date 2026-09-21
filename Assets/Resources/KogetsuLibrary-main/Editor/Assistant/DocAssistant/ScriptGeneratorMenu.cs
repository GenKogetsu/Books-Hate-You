#if UNITY_EDITOR
using UnityEditor;

namespace Kogetsu.Library.Assistant
{
    public static class ScriptGeneratorMenuPackages
    {
        private const string _rootPath = "Packages/com.Kogetsu.Library/Editor/Assistant/DocAssistant/Templates/";

        private const string _interfaceTemplatePath = _rootPath + "InterfaceTemplate.txt";
        private const string _enumTemplatePath = _rootPath + "EnumTemplate.txt";
        private const string _structTemplatePath = _rootPath + "StructTemplate.txt";
        private const string _recordStructEventTemplatePath = _rootPath + "RecordStructEventTemplate.txt";

        // --- เมนูในหน้าหลัก (ต่อจาก C# Script) ---
        [MenuItem("Assets/Create/Interface C# Script", false, 81)]
        public static void CreateInterface() => ProjectWindowUtil.CreateScriptAssetFromTemplateFile(_interfaceTemplatePath, "I.cs");

        [MenuItem("Assets/Create/Enum C# Script", false, 82)]
        public static void CreateEnum() => ProjectWindowUtil.CreateScriptAssetFromTemplateFile(_enumTemplatePath, "NewEnum.cs");

        [MenuItem("Assets/Create/Struct C# Script", false, 83)]
        public static void CreateStruct() => ProjectWindowUtil.CreateScriptAssetFromTemplateFile(_structTemplatePath, "NewStruct.cs");

        [MenuItem("Assets/Create/Record Struct Event C# Script", false, 84)]
        public static void CreateRecordStruct() => ProjectWindowUtil.CreateScriptAssetFromTemplateFile(_recordStructEventTemplatePath, "NewRecordStructEvent.cs");

        // --- เมนูในโฟลเดอร์ Scripting (ต่อจาก Script ของ Unity) ---
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