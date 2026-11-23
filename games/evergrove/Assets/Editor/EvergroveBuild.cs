using System;
using System.Collections.Generic;
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
    /// Channels (EVERGROVE_BUILD_CHANNEL):
    ///   development (default) - BuildOptions.Development playtest, research controls enabled.
    ///   release-candidate     - no Development flag, store product name and id, no research
    ///                           controls or research locale catalogs. Never uploads or signs.
    ///
    /// IMPORTANT: This method has not been executed until a licensed Unity
    /// runner actually invokes it. It deliberately never uploads or publishes.
    /// </summary>
    public static class EvergroveBuild
    {
        public const string DevelopmentChannel = "development";
        public const string ReleaseCandidateChannel = "release-candidate";
        public const string StoreApplicationId = "com.streamentry.evergrove";
        private const string LocalesRelativePath = "Assets/Resources/Evergrove/Locales";

        public static void Build()
        {
            BuildTarget target = EditorUserBuildSettings.activeBuildTarget;
            if (target != BuildTarget.Android && target != BuildTarget.WebGL && target != BuildTarget.iOS &&
                target != BuildTarget.StandaloneOSX)
                throw new NotSupportedException("Evergrove preview supports Android, WebGL, unsigned iOS Xcode export and a local macOS QA player.");

            string channel = ReadChannel();
            bool releaseCandidate = channel == ReleaseCandidateChannel;

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
                    : target == BuildTarget.StandaloneOSX
                        ? Path.Combine(outDir, "Mosselora-QA.app")
                        : Path.Combine(outDir, "Evergrove-Playtest");

            PlayerSettings.productName = releaseCandidate ? "Mosselora" : "Mosselora Playtest";
            Texture2D appIcon = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Branding/MosseloraAppIcon.png");
            if (appIcon != null)
                PlayerSettings.SetIcons(UnityEditor.Build.NamedBuildTarget.Unknown, new[] { appIcon }, IconKind.Any);
            PlayerSettings.companyName = "Game Foundry";
            PlayerSettings.bundleVersion = releaseCandidate ? ReadBundleVersion() : "0.1.0";
            // No store signing keys or release deployment in either channel.
            if (target == BuildTarget.Android)
            {
                PlayerSettings.SetApplicationIdentifier(
                    UnityEditor.Build.NamedBuildTarget.Android,
                    releaseCandidate ? StoreApplicationId : "com.streamentry.evergrove.playtest");
                EditorUserBuildSettings.buildAppBundle = false;
                EditorUserBuildSettings.exportAsGoogleAndroidProject = false;
            }
            else if (target == BuildTarget.iOS)
            {
                // Generating an Xcode project is not a signed .ipa build.
                PlayerSettings.SetApplicationIdentifier(
                    UnityEditor.Build.NamedBuildTarget.iOS,
                    releaseCandidate ? StoreApplicationId : "com.streamentry.evergrove.playtest");
                // EVERGROVE_IOS_SDK=simulator exports an unsigned Xcode
                // project for the iOS Simulator (local QA only); default is device.
                bool simulator = string.Equals(
                    Environment.GetEnvironmentVariable("EVERGROVE_IOS_SDK"), "simulator",
                    StringComparison.OrdinalIgnoreCase);
                PlayerSettings.iOS.sdkVersion = simulator ? iOSSdkVersion.SimulatorSDK : iOSSdkVersion.DeviceSDK;
                // Apple-silicon simulators on current iOS reject x86_64-only apps.
                if (simulator)
                    PlayerSettings.iOS.simulatorSdkArchitecture = AppleMobileArchitectureSimulator.ARM64;
            }
            else if (target == BuildTarget.StandaloneOSX && releaseCandidate)
            {
                PlayerSettings.SetApplicationIdentifier(
                    UnityEditor.Build.NamedBuildTarget.Standalone, StoreApplicationId);
            }

            // Research locale catalogs are removed from the player's Resources
            // for release candidates only. They are restored in finally.
            string localesDir = Path.Combine(repositoryRoot, "games", "evergrove", LocalesRelativePath);
            string localeStagingDir = Path.Combine(outputRoot, "rc-locale-staging");
            List<string> stagedLocales = releaseCandidate
                ? StageResearchLocales(localesDir, localeStagingDir)
                : null;
            BuildReport report;
            try
            {
                report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
                {
                    scenes = new[] { "Assets/Scenes/Evergrove.unity" },
                    locationPathName = output,
                    target = target,
                    // Development channel: DEVELOPMENT_BUILD enables the playtest-only
                    // controls in EvergroveApp. Release candidate: no Development flag,
                    // so every #if DEVELOPMENT_BUILD block is compiled out.
                    options = releaseCandidate ? BuildOptions.None : BuildOptions.Development
                });
            }
            finally
            {
                if (stagedLocales != null) RestoreResearchLocales(localesDir, localeStagingDir, stagedLocales);
            }
            if (report == null || report.summary.result != BuildResult.Succeeded)
                throw new InvalidOperationException("Unity player build FAILED for " + platform + " (" + channel + ")");

            // Development keeps the historical marker that the local preflight matches exactly.
            Debug.Log(releaseCandidate
                ? "EVERGROVE_BUILD_SUCCEEDED " + channel + " " + output
                : "EVERGROVE_BUILD_SUCCEEDED " + output);
        }

        private static string ReadChannel()
        {
            string channel = Environment.GetEnvironmentVariable("EVERGROVE_BUILD_CHANNEL");
            if (string.IsNullOrWhiteSpace(channel)) return DevelopmentChannel;
            channel = channel.Trim();
            if (channel != DevelopmentChannel && channel != ReleaseCandidateChannel)
                throw new InvalidOperationException("EVERGROVE_BUILD_CHANNEL must be 'development' or 'release-candidate'.");
            return channel;
        }

        private static string ReadBundleVersion()
        {
            string version = Environment.GetEnvironmentVariable("EVERGROVE_BUNDLE_VERSION");
            if (string.IsNullOrWhiteSpace(version)) return "0.1.0";
            version = version.Trim();
            foreach (char c in version)
                if (!(char.IsDigit(c) || c == '.'))
                    throw new InvalidOperationException("EVERGROVE_BUNDLE_VERSION may contain only digits and dots.");
            return version;
        }

        // Moves every research catalog out of Assets (and its .meta, so GUIDs survive)
        // into a private staging folder outside the Unity project. English has no catalog
        // file, and LocaleShippingPolicy allows English only, so nothing is lost.
        private static List<string> StageResearchLocales(string localesDir, string stagingDir)
        {
            var moved = new List<string>();
            if (!Directory.Exists(localesDir)) return moved;
            if (Directory.Exists(stagingDir) && Directory.GetFileSystemEntries(stagingDir).Length > 0)
                throw new InvalidOperationException("Locale staging folder is not empty; a previous build did not restore it: " + stagingDir);
            Directory.CreateDirectory(stagingDir);
            foreach (string file in Directory.GetFiles(localesDir))
            {
                string name = Path.GetFileName(file);
                if (!name.EndsWith(".json", StringComparison.Ordinal) && !name.EndsWith(".json.meta", StringComparison.Ordinal))
                    continue;
                File.Move(file, Path.Combine(stagingDir, name));
                moved.Add(name);
            }
            AssetDatabase.Refresh();
            return moved;
        }

        private static void RestoreResearchLocales(string localesDir, string stagingDir, List<string> staged)
        {
            Directory.CreateDirectory(localesDir);
            foreach (string name in staged)
                File.Move(Path.Combine(stagingDir, name), Path.Combine(localesDir, name));
            if (Directory.Exists(stagingDir) && Directory.GetFileSystemEntries(stagingDir).Length == 0)
                Directory.Delete(stagingDir);
            AssetDatabase.Refresh();
        }
    }
}
