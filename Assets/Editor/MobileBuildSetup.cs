using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

/// <summary>
/// Applies the shared phone build settings without changing the desktop quality profile.
/// Run from Tools > Books Hate You > Configure Mobile Player Settings.
/// </summary>
public static class MobileBuildSetup
{
    private const string ProductName = "Books Hate You";
    private const string ApplicationId = "com.bookshateyou.game";

    [MenuItem("Tools/Books Hate You/Configure Mobile Player Settings")]
    public static void ConfigureMobilePlayerSettings()
    {
        PlayerSettings.productName = ProductName;
        PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, ApplicationId);
        PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.iOS, ApplicationId);

        // Auto rotation is shared by Android and iOS, so allow both landscape directions only.
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.AutoRotation;
        PlayerSettings.allowedAutorotateToPortrait = false;
        PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
        PlayerSettings.allowedAutorotateToLandscapeLeft = true;
        PlayerSettings.allowedAutorotateToLandscapeRight = true;

        // Keep the game and its touch controls inside display cutouts on Android.
        PlayerSettings.Android.renderOutsideSafeArea = false;
        PlayerSettings.Android.optimizedFramePacing = true;
        PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;

        AssetDatabase.SaveAssets();
        Debug.Log("Mobile player settings configured for Android and iOS.");
    }

    public static void BuildAndroidApk()
    {
        ConfigureMobilePlayerSettings();

        var scenes = EditorBuildSettings.scenes
            .Where(scene => scene.enabled)
            .Select(scene => scene.path)
            .ToArray();
        if (scenes.Length == 0)
            throw new InvalidOperationException("No enabled scenes are configured for the Android build.");

        var projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
        var outputDirectory = Path.Combine(projectRoot, "Builds", "Android");
        Directory.CreateDirectory(outputDirectory);
        var outputPath = Path.Combine(outputDirectory, "BooksHateYou.apk");

        var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = outputPath,
            target = BuildTarget.Android,
            options = BuildOptions.None
        });

        if (report.summary.result != BuildResult.Succeeded)
            throw new InvalidOperationException($"Android build failed: {report.summary.result}");

        Debug.Log($"Android APK built: {outputPath}");
    }
}
