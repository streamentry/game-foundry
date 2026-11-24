using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace Evergrove.UnityClient.Editor
{
    /// <summary>
    /// iOS launch screen: Unity's default storyboard is a white view with a
    /// 1x1 placeholder image. After every iOS export, write the parchment
    /// background colour and the painted launch image (brand/launch-portrait.png,
    /// built by tools/evergrove_art_pipeline.py) into the Xcode project, so the
    /// first frame a player sees is the same paper as the game.
    /// </summary>
    internal static class MosseloraLaunchScreen
    {
        private const string White =
            "<color key=\"backgroundColor\" red=\"1\" green=\"1\" blue=\"1\" alpha=\"1\" colorSpace=\"custom\" customColorSpace=\"sRGB\"/>";
        private const string Parchment =
            "<color key=\"backgroundColor\" red=\"0.9765\" green=\"0.9490\" blue=\"0.8902\" alpha=\"1\" colorSpace=\"custom\" customColorSpace=\"sRGB\"/>";

        [PostProcessBuild(100)]
        public static void OnPostprocessBuild(BuildTarget target, string exportPath)
        {
            if (target != BuildTarget.iOS) return;
            string image = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "brand", "launch-portrait.png"));
            if (File.Exists(image))
                File.Copy(image, Path.Combine(exportPath, "LaunchScreen-iPhonePortrait.png"), true);
            else
                Debug.LogWarning("Mosselora launch image missing: " + image);
            foreach (string board in new[] { "LaunchScreen-iPhone.storyboard", "LaunchScreen-iPad.storyboard" })
            {
                string path = Path.Combine(exportPath, board);
                if (!File.Exists(path)) continue;
                string xml = File.ReadAllText(path);
                if (xml.Contains(White)) File.WriteAllText(path, xml.Replace(White, Parchment));
            }
            Debug.Log("MOSSELORA_LAUNCH_SCREEN parchment " + File.Exists(image));
        }
    }
}
