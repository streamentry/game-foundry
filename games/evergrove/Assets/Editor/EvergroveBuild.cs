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

            for (int i = 0; i < PuzzleCatalog.Count; i++)
            {
                Puzzle puzzle = PuzzleCatalog.Get(i);
                if (!PipeRules.IsComplete(puzzle, puzzle.Solution) ||
                    PipeRules.CountSolutions(puzzle) != 1)
                    throw new InvalidOperationException("Puzzle failed solver check: " + i);
            }

            // Unity is running from games/evergrove/Assets. Keep build
            // artifacts outside the Unity project to avoid recursive import.
            string repositoryRoot = Path.GetFullPath(
                Path.Combine(Application.dataPath, "..", "..", ".."));
            string platform = target.ToString();
            string outDir = Path.Combine(repositoryRoot, "build", platform);
            Directory.CreateDirectory(outDir);
            string output = target == BuildTarget.Android
                ? Path.Combine(outDir, "Evergrove-Playtest.apk")
                : target == BuildTarget.iOS
                    ? Path.Combine(outDir, "Evergrove-Xcode")
                    : Path.Combine(outDir, "Evergrove-Playtest");

            PlayerSettings.productName = "Evergrove Playtest";
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
