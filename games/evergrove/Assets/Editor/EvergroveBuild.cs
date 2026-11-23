using System;
using System.IO;
using Evergrove.Core;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Evergrove.UnityClient.Editor
{
    /// <summary>
    /// A real (not simulated) Unity build entrypoint for GameCI or a local
    /// licensed Unity Editor. Creates the empty boot scene deterministically
    /// and places a downloadable WebGL or unsigned Android playtest under
    /// the repository's build/ directory.
    ///
    /// IMPORTANT: This method has not been executed until a licensed Unity
    /// runner actually invokes it. It deliberately never uploads or publishes.
    /// </summary>
    public static class EvergroveBuild
    {
        public static void Build()
        {
            BuildTarget target = EditorUserBuildSettings.activeBuildTarget;
            if (target != BuildTarget.Android && target != BuildTarget.WebGL && target != BuildTarget.iOS)
                throw new NotSupportedException("Evergrove preview supports Android, WebGL and unsigned iOS Xcode export.");

            EvergroveSceneSetup.CreateScene();
            // A generated APK or Xcode project must not bypass actual Unity
            // DTO serialization, Resources imports or editor-version checks.
            EvergroveSceneSetup.ValidateImportedProject();

            // Unity is running from games/evergrove/Assets. Keep build
            // artifacts outside the Unity project to avoid recursive import.
            string repositoryRoot = Path.GetFullPath(
                Path.Combine(Application.dataPath, "..", "..", ".."));
            string platform = target.ToString();
            // GameCI keeps its default repo/build/<platform> convention.
            // Local preflight isolates every attempt under a unique private
            // directory so old successful artifacts cannot fake a new pass.
            string privateRoot = Environment.GetEnvironmentVariable("EVERGROVE_OUTPUT_ROOT");
            if (!string.IsNullOrWhiteSpace(privateRoot) && !Path.IsPathRooted(privateRoot))
                throw new InvalidOperationException("EVERGROVE_OUTPUT_ROOT must be absolute.");
            string outputRoot = string.IsNullOrWhiteSpace(privateRoot)
                ? Path.Combine(repositoryRoot, "build")
                : Path.GetFullPath(privateRoot);
            string assetsPath = Path.GetFullPath(Application.dataPath)
                .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                + Path.DirectorySeparatorChar;
            if (outputRoot.StartsWith(assetsPath, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Never export builds into Unity Assets.");
            string outDir = Path.Combine(outputRoot, platform);
            Directory.CreateDirectory(outDir);
            string output = target == BuildTarget.Android
                ? Path.Combine(outDir, "Evergrove-Playtest.apk")
                : target == BuildTarget.iOS
                    ? Path.Combine(outDir, "Evergrove-Xcode")
                    : Path.Combine(outDir, "Evergrove-Playtest");

            PlayerSettings.productName = "Mosselora Playtest";
            Texture2D appIcon = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Branding/MosseloraAppIcon.png");
            if (appIcon != null)
                PlayerSettings.SetIcons(UnityEditor.Build.NamedBuildTarget.Unknown, new[] { appIcon }, IconKind.Any);
            PlayerSettings.companyName = "Game Foundry";
            PlayerSettings.bundleVersion = "0.1.0";
            // No production app ID, store signing keys or release deployment.
            if (target == BuildTarget.Android)
            {
                PlayerSettings.SetApplicationIdentifier(
                    UnityEditor.Build.NamedBuildTarget.Android,
                    "com.streamentry.evergrove.playtest");
                EditorUserBuildSettings.buildAppBundle = false;
                EditorUserBuildSettings.exportAsGoogleAndroidProject = false;
            }
            else if (target == BuildTarget.iOS)
            {
                // Generating an Xcode project is not a signed .ipa build.
                PlayerSettings.SetApplicationIdentifier(
                    UnityEditor.Build.NamedBuildTarget.iOS,
                    "com.streamentry.evergrove.playtest");
            }

            BuildReport report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = new[] { "Assets/Scenes/Evergrove.unity" },
                locationPathName = output,
                target = target,
                // This workflow is EXPLICITLY a research/development build.
                // DEVELOPMENT_BUILD enables the playtest-only controls in
                // EvergroveApp. A production candidate must use a distinct
                // non-development build/release pipeline and be audited.
                options = BuildOptions.Development
            });
            if (report == null || report.summary.result != BuildResult.Succeeded)
                throw new InvalidOperationException("Unity player build FAILED for " + platform);

            Debug.Log("EVERGROVE_BUILD_SUCCEEDED " + output);
        }
    }
}
