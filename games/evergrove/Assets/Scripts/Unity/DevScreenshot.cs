using System;
using System.Collections;
using System.IO;
using UnityEngine;

namespace Evergrove.UnityClient
{
    /// <summary>
    /// Development-build-only visual QA hook. Launch a development player with
    /// <c>-mosseloraScreenshot /absolute/path.png</c> (optionally
    /// <c>-mosseloraScreenshotFrames 90</c>) to save one frame and quit.
    /// Inactive in release builds and when the argument is absent. It never
    /// uploads anything; it only writes the local file the operator names.
    /// </summary>
    public sealed class DevScreenshot : MonoBehaviour
    {
        private const string PathArg = "-mosseloraScreenshot";
        private const string FramesArg = "-mosseloraScreenshotFrames";
        private const int DefaultFrames = 90;

        private string outputPath;
        private int waitFrames = DefaultFrames;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
            if (!Debug.isDebugBuild || Application.isEditor) return;
            string path = ReadArg(PathArg);
            if (string.IsNullOrWhiteSpace(path) || !Path.IsPathRooted(path)) return;

            var host = new GameObject("Mosselora DevScreenshot");
            DontDestroyOnLoad(host);
            DevScreenshot hook = host.AddComponent<DevScreenshot>();
            hook.outputPath = path;
            int frames;
            if (int.TryParse(ReadArg(FramesArg), out frames) && frames > 0 && frames < 10000)
                hook.waitFrames = frames;
        }

        private IEnumerator Start()
        {
            for (int i = 0; i < waitFrames; i++) yield return null;
            yield return new WaitForEndOfFrame();
            try
            {
                // ScreenCaptureModule is not enabled; read the back buffer,
                // which includes screen-space overlay UI after end of frame.
                var frame = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
                frame.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
                frame.Apply();
                File.WriteAllBytes(outputPath, frame.EncodeToPNG());
                Destroy(frame);
                Debug.Log("MOSSELORA_SCREENSHOT_SAVED " + outputPath);
            }
            catch (Exception error)
            {
                Debug.LogError("MOSSELORA_SCREENSHOT_FAILED " + error.Message);
            }
            Application.Quit();
        }

        private static string ReadArg(string name)
        {
            string[] args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length - 1; i++)
                if (string.Equals(args[i], name, StringComparison.Ordinal)) return args[i + 1];
            return null;
        }
    }
}
