#if UNITY_EDITOR
using System.IO;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEditorInternal;

[InitializeOnLoad]
public static class TargetTagEnumGenerator
{
    private const string OutputPath = "Assets/Scripts/Enum/TargetTag.cs";
    private static string _lastSignature;

    static TargetTagEnumGenerator()
    {
        UnityEditor.EditorApplication.update += GenerateIfNeeded;
        GenerateIfNeeded();
    }

    private static void GenerateIfNeeded()
    {
        string[] tags = InternalEditorUtility.tags;
        string signature = string.Join("|", tags);

        if (signature == _lastSignature)
            return;

        _lastSignature = signature;
        Generate(tags);
    }

    private static void Generate(string[] unityTags)
    {
        List<string> uniqueTags = new();
        HashSet<string> usedEnumNames = new();

        foreach (string tag in unityTags)
        {
            if (usedEnumNames.Add(ToEnumName(tag)))
                uniqueTags.Add(tag);
        }

        StringBuilder builder = new();
        builder.AppendLine("[System.Flags]");
        builder.AppendLine("public enum TargetTag");
        builder.AppendLine("{");
        builder.AppendLine("    None = 0,");

        for (int i = 0; i < uniqueTags.Count; i++)
            builder.AppendLine($"    {ToEnumName(uniqueTags[i])} = 1 << {i},");

        builder.AppendLine("}");
        builder.AppendLine();
        builder.AppendLine("public static class TargetTagExtensions");
        builder.AppendLine("{");
        builder.AppendLine("    public static bool MatchesUnityTag(this TargetTag targetTags, string unityTag)");
        builder.AppendLine("    {");
        builder.AppendLine("        return");

        for (int i = 0; i < uniqueTags.Count; i++)
        {
            builder.AppendLine(
                $"            ((targetTags & TargetTag.{ToEnumName(uniqueTags[i])}) != 0 && unityTag == \"{Escape(uniqueTags[i])}\")"
                + (i == uniqueTags.Count - 1 ? ";" : " ||"));
        }

        builder.AppendLine("    }");
        builder.AppendLine("}");

        string directory = Path.GetDirectoryName(OutputPath);
        Directory.CreateDirectory(directory);

        string newContent = builder.ToString();
        string oldContent = File.Exists(OutputPath)
            ? File.ReadAllText(OutputPath)
            : string.Empty;

        if (oldContent == newContent)
            return;

        File.WriteAllText(OutputPath, newContent, Encoding.UTF8);
        AssetDatabase.ImportAsset(OutputPath);
    }

    private static string ToEnumName(string tag)
    {
        StringBuilder builder = new();

        foreach (char character in tag)
            builder.Append(char.IsLetterOrDigit(character) || character == '_' ? character : '_');

        string value = builder.ToString().Trim('_');

        if (string.IsNullOrEmpty(value))
            value = "UnnamedTag";

        if (char.IsDigit(value[0]))
            value = "Tag_" + value;

        return value;
    }

    private static string Escape(string value)
    {
        return value.Replace("\\", "\\\\").Replace("\"", "\\\"");
    }
}
#endif
