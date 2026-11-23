#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UIE = UnityEngine.UIElements;

namespace Evergrove.UnityClient
{
    /// <summary>
    /// Development-build-only EG-006 architecture spike. Launch with
    /// <c>-mosseloraTextSpike /absolute/path.png</c> to render the same draft
    /// strings through legacy uGUI Text (left) and UI Toolkit labels with the
    /// Advanced Text Generator (right), save one frame and quit. It never
    /// ships in release builds and never touches player save data. The OS
    /// fonts come from LocaleFonts; no font files are added to the repository.
    /// </summary>
    public sealed class TextSpike : MonoBehaviour
    {
        private const string PathArg = "-mosseloraTextSpike";
        private const int WaitFrames = 60;

        private static readonly string[] Codes = { "ar", "fa", "he", "hi", "bn", "ta", "th" };
        private static readonly string[] Keys = { "title", "first" };

        private string outputPath;

        [Serializable]
        public sealed class SpikeEntry
        {
            public string key;
            public string value;
        }

        [Serializable]
        public sealed class SpikeBundle
        {
            public string code;
            public SpikeEntry[] entries;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
            if (!Debug.isDebugBuild || Application.isEditor) return;
            string path = ReadArg(PathArg);
            if (string.IsNullOrWhiteSpace(path) || !Path.IsPathRooted(path)) return;

            var host = new GameObject("Mosselora TextSpike");
            DontDestroyOnLoad(host);
            host.AddComponent<TextSpike>().outputPath = path;
        }

        private IEnumerator Start()
        {
            // An unfocused QA window stops advancing frames; keep the spike moving.
            Application.runInBackground = true;
            // Hide the normal game UI so only the spike is captured.
            GameObject game = GameObject.Find("Evergrove");
            if (game != null) game.SetActive(false);

            try
            {
                BuildLegacyColumn();
                BuildUiToolkitColumn();
            }
            catch (Exception error)
            {
                Debug.LogError("MOSSELORA_TEXT_SPIKE_BUILD_FAILED " + error.GetType().Name + ": " + error.Message);
            }

            for (int i = 0; i < WaitFrames; i++)
            {
                if (i % 10 == 0) Debug.Log("MOSSELORA_TEXT_SPIKE_FRAME " + i + " t=" + Time.realtimeSinceStartup);
                yield return null;
            }
            yield return new WaitForEndOfFrame();
            try
            {
                var frame = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
                frame.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
                frame.Apply();
                File.WriteAllBytes(outputPath, frame.EncodeToPNG());
                Destroy(frame);
                Debug.Log("MOSSELORA_TEXT_SPIKE_SAVED " + outputPath);
            }
            catch (Exception error)
            {
                Debug.LogError("MOSSELORA_TEXT_SPIKE_FAILED " + error.Message);
            }
            Application.Quit();
        }

        // Left half: legacy uGUI Text. Rows are fractions of the screen so both
        // columns share the same vertical grid on any window size.
        private void BuildLegacyColumn()
        {
            var canvasGo = new GameObject("Spike uGUI Canvas", typeof(Canvas), typeof(CanvasScaler));
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 0;
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
            scaler.scaleFactor = 1f;

            var background = new GameObject("Background", typeof(RectTransform), typeof(Image));
            background.transform.SetParent(canvasGo.transform, false);
            background.GetComponent<Image>().color = Paper;
            Stretch(background.GetComponent<RectTransform>(), 0f, 0f, 1f, 1f, 0f);

            Font baseFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            AddLegacy(canvasGo.transform, "Column header", "uGUI Text (legacy)", baseFont, 0.0f, 0.02f, 0.5f, 0.06f, 18, true);

            for (int row = 0; row < Codes.Length; row++)
            {
                string code = Codes[row];
                Font font = LocaleFonts.Choose(code, baseFont);
                SpikeBundle bundle = LoadBundle(code);
                float top = 0.07f + row * 0.13f;
                AddLegacy(canvasGo.transform, "Legacy " + code, Sample(code, bundle), font,
                          0.0f, top, 0.5f, 0.13f, 20, false);
                Debug.Log("MOSSELORA_TEXT_SPIKE_FONT legacy " + code + " -> " + (font == null ? "null" : font.name));
            }
        }

        private void AddLegacy(Transform parent, string name, string content, Font font,
                               float x, float y, float width, float height, int size, bool bold)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            var text = go.GetComponent<Text>();
            text.font = font;
            text.fontSize = size;
            text.fontStyle = bold ? FontStyle.Bold : FontStyle.Normal;
            text.color = Ink;
            text.alignment = TextAnchor.UpperLeft;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.text = content;
            Stretch(go.GetComponent<RectTransform>(), x, y, x + width, y + height, 12f);
        }

        // Right half: UI Toolkit labels with the Advanced Text Generator. The
        // root is transparent so the uGUI background shows through.
        private void BuildUiToolkitColumn()
        {
            var doc = new GameObject("Spike UI Toolkit", typeof(UIE.UIDocument)).GetComponent<UIE.UIDocument>();
            var settings = ScriptableObject.CreateInstance<UIE.PanelSettings>();
            settings.scaleMode = UIE.PanelScaleMode.ConstantPixelSize;
            settings.scale = 1f;
            settings.sortingOrder = 10;
            doc.panelSettings = settings;

            UIE.VisualElement root = doc.rootVisualElement;
            root.pickingMode = UIE.PickingMode.Ignore;
            root.style.position = UIE.Position.Absolute;
            root.style.left = 0;
            root.style.top = 0;
            root.style.right = 0;
            root.style.bottom = 0;
            root.style.unityTextGenerator = new UIE.StyleEnum<TextGeneratorType>(TextGeneratorType.Advanced);

            Font baseFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            AddLabel(root, "UITK header", "UI Toolkit + Advanced Text Generator", baseFont, false,
                     0.5f, 0.02f, 0.5f, 0.06f, 18, true);

            for (int row = 0; row < Codes.Length; row++)
            {
                string code = Codes[row];
                Font font = LocaleFonts.Choose(code, baseFont);
                SpikeBundle bundle = LoadBundle(code);
                float top = 0.07f + row * 0.13f;
                AddLabel(root, "UITK " + code, Sample(code, bundle), font, IsRtl(code),
                         0.5f, top, 0.5f, 0.13f, 20, false);
                Debug.Log("MOSSELORA_TEXT_SPIKE_FONT uitk " + code + " -> " + (font == null ? "null" : font.name));
            }
        }

        private void AddLabel(UIE.VisualElement root, string name, string content, Font font, bool rtl,
                              float x, float y, float width, float height, int size, bool bold)
        {
            var label = new UIE.Label(content);
            label.name = name;
            label.style.position = UIE.Position.Absolute;
            label.style.left = UIE.Length.Percent(x * 100f + 1f);
            label.style.top = UIE.Length.Percent(y * 100f + 1f);
            label.style.width = UIE.Length.Percent(width * 100f - 2f);
            label.style.height = UIE.Length.Percent(height * 100f - 1f);
            label.style.whiteSpace = UIE.WhiteSpace.Normal;
            label.style.unityTextGenerator = new UIE.StyleEnum<TextGeneratorType>(TextGeneratorType.Advanced);
            label.style.unityFontDefinition = new UIE.StyleFontDefinition(UIE.FontDefinition.FromFont(font));
            label.style.fontSize = size;
            label.style.unityFontStyleAndWeight = bold ? FontStyle.Bold : FontStyle.Normal;
            label.style.color = Ink;
            label.style.unityTextAlign = TextAnchor.UpperLeft;
            label.languageDirection = rtl ? UIE.LanguageDirection.RTL : UIE.LanguageDirection.LTR;
            root.Add(label);
        }

        private static string Sample(string code, SpikeBundle bundle)
        {
            if (bundle == null || bundle.entries == null) return code + "\n(missing bundle)";
            var lines = new string[Keys.Length + 1];
            lines[0] = code;
            for (int i = 0; i < Keys.Length; i++)
                lines[i + 1] = Value(bundle, Keys[i]);
            return string.Join("\n", lines);
        }

        private static string Value(SpikeBundle bundle, string key)
        {
            foreach (SpikeEntry entry in bundle.entries)
                if (entry != null && entry.key == key) return entry.value;
            return "(missing " + key + ")";
        }

        private static SpikeBundle LoadBundle(string code)
        {
            try
            {
                var asset = Resources.Load<TextAsset>("Evergrove/Locales/" + code);
                if (asset == null) return null;
                return JsonUtility.FromJson<SpikeBundle>(asset.text);
            }
            catch (Exception error)
            {
                Debug.LogWarning("MOSSELORA_TEXT_SPIKE_BUNDLE " + code + " " + error.GetType().Name);
                return null;
            }
        }

        private static bool IsRtl(string code)
        {
            return code == "ar" || code == "fa" || code == "he";
        }

        // Anchors are fractions from the top-left; padding is in pixels.
        private static void Stretch(RectTransform rect, float x0, float y0, float x1, float y1, float pad)
        {
            rect.anchorMin = new Vector2(x0, 1f - y1);
            rect.anchorMax = new Vector2(x1, 1f - y0);
            rect.offsetMin = new Vector2(pad, pad);
            rect.offsetMax = new Vector2(-pad, -pad);
        }

        private static readonly Color Paper = new Color(0.96f, 0.94f, 0.89f, 1f);
        private static readonly Color Ink = new Color(0.10f, 0.12f, 0.10f, 1f);

        private static string ReadArg(string name)
        {
            return DevArgs.Read(name);
        }
    }
}
#endif
