using System;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

namespace Evergrove.UnityClient
{
    /// <summary>
    /// EG-006 iteration 7 prototype: draws one string with the operating system's
    /// text stack (CoreText on iOS, android.text.StaticLayout on Android) into a
    /// Texture2D. uGUI has no Indic shaping, so Bengali conjuncts break there; the
    /// OS stacks form them. Used only by <see cref="NativeTextLabel"/> for the bn
    /// locale. Editor and desktop builds return null and keep uGUI Text.
    /// Ink is straight-alpha RGBA; the texture is bottom-up, as Unity expects.
    /// </summary>
    public static class NativeTextBridge
    {
        public static bool Supported
        {
            get
            {
#if (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
                return true;
#else
                return false;
#endif
            }
        }

        /// <summary>
        /// Renders text wrapped to widthPx. The returned texture's height is the
        /// measured wrapped height in pixels. Returns null when unavailable or empty.
        /// </summary>
        public static Texture2D Render(string text, float sizePx, int widthPx, bool bold,
                                       Color ink, TextAnchor alignment)
        {
            if (!Supported || string.IsNullOrEmpty(text) || widthPx <= 0 || sizePx <= 0f) return null;
            int align = AlignmentIndex(alignment);
#if UNITY_IOS && !UNITY_EDITOR
            return RenderIos(text, sizePx, widthPx, bold, ink, align);
#elif UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                return RenderAndroid(text, sizePx, widthPx, bold, ink, alignment);
            }
            catch (AndroidJavaException exception)
            {
                Debug.LogWarning("Mosselora native text bridge failed: " + exception.GetType().Name);
                return null;
            }
#else
            return null;
#endif
        }

        private static int AlignmentIndex(TextAnchor alignment)
        {
            switch (alignment)
            {
                case TextAnchor.UpperCenter:
                case TextAnchor.MiddleCenter:
                case TextAnchor.LowerCenter:
                    return 1;
                case TextAnchor.UpperRight:
                case TextAnchor.MiddleRight:
                case TextAnchor.LowerRight:
                    return 2;
                default:
                    return 0;
            }
        }

#if UNITY_IOS && !UNITY_EDITOR
        // Plugin: Assets/Plugins/iOS/MosseloraNativeText.mm
        [DllImport("__Internal")]
        private static extern void MosseloraNativeTextMeasure(byte[] utf8, float size, float maxWidth,
                                                              int bold, int[] outWH);

        [DllImport("__Internal")]
        private static extern void MosseloraNativeTextRender(byte[] utf8, float size, float maxWidth,
                                                             int bold, int align, float r, float g, float b, float a,
                                                             byte[] buffer, int w, int h);

        private static Texture2D RenderIos(string text, float sizePx, int widthPx, bool bold,
                                           Color ink, int align)
        {
            byte[] utf8 = Encoding.UTF8.GetBytes(text + "\0");
            int flag = bold ? 1 : 0;
            int[] wh = new int[2];
            MosseloraNativeTextMeasure(utf8, sizePx, widthPx, flag, wh);
            int heightPx = wh[1];
            if (heightPx <= 0) return null;
            byte[] pixels = new byte[widthPx * heightPx * 4];
            MosseloraNativeTextRender(utf8, sizePx, widthPx, flag, align, ink.r, ink.g, ink.b, ink.a,
                                      pixels, widthPx, heightPx);
            var texture = new Texture2D(widthPx, heightPx, TextureFormat.RGBA32, false);
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.filterMode = FilterMode.Bilinear;
            texture.LoadRawTextureData(pixels);
            texture.Apply(false, true);
            return texture;
        }
#endif

#if UNITY_ANDROID && !UNITY_EDITOR
        private const int AntiAliasFlag = 1;
        private const int PngFormatQuality = 100;

        // StaticLayout is the framework's wrapping and shaping engine (HarfBuzz underneath).
        private static Texture2D RenderAndroid(string text, float sizePx, int widthPx, bool bold,
                                               Color ink, TextAnchor alignment)
        {
            using (var paint = new AndroidJavaObject("android.text.TextPaint", AntiAliasFlag))
            using (var layout = BuildLayout(paint, text, sizePx, widthPx, bold, ink, alignment))
            {
                int heightPx = layout.Call<int>("getHeight");
                if (heightPx <= 0) return null;
                using (var argb = new AndroidJavaClass("android.graphics.Bitmap$Config"))
                using (var config = argb.GetStatic<AndroidJavaObject>("ARGB_8888"))
                using (var bitmapClass = new AndroidJavaClass("android.graphics.Bitmap"))
                using (var bitmap = bitmapClass.CallStatic<AndroidJavaObject>("createBitmap", widthPx, heightPx, config))
                using (var canvas = new AndroidJavaObject("android.graphics.Canvas", bitmap))
                {
                    layout.Call("draw", canvas);
                    byte[] png = EncodePng(bitmap);
                    bitmap.Call("recycle");
                    if (png == null) return null;
                    var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                    texture.wrapMode = TextureWrapMode.Clamp;
                    texture.filterMode = FilterMode.Bilinear;
                    if (!ImageConversion.LoadImage(texture, png)) return null;
                    return texture;
                }
            }
        }

        private static AndroidJavaObject BuildLayout(AndroidJavaObject paint, string text, float sizePx,
                                                     int widthPx, bool bold, Color ink, TextAnchor alignment)
        {
            paint.Call("setTextSize", sizePx);
            paint.Call("setColor", ArgbOf(ink));
            if (bold)
            {
                using (var typefaceClass = new AndroidJavaClass("android.graphics.Typeface"))
                using (var boldFace = typefaceClass.GetStatic<AndroidJavaObject>("DEFAULT_BOLD"))
                    paint.Call<AndroidJavaObject>("setTypeface", boldFace);
            }
            using (var builderClass = new AndroidJavaClass("android.text.StaticLayout$Builder"))
            using (var builder = builderClass.CallStatic<AndroidJavaObject>(
                       "obtain", text, 0, text.Length, paint, widthPx))
            using (var alignClass = new AndroidJavaClass("android.text.Layout$Alignment"))
            {
                string name = "ALIGN_NORMAL";
                if (AlignmentIndex(alignment) == 1) name = "ALIGN_CENTER";
                if (AlignmentIndex(alignment) == 2) name = "ALIGN_OPPOSITE";
                using (var align = alignClass.GetStatic<AndroidJavaObject>(name))
                {
                    builder.Call<AndroidJavaObject>("setAlignment", align);
                }
                builder.Call<AndroidJavaObject>("setIncludePad", false);
                return builder.Call<AndroidJavaObject>("build");
            }
        }

        private static byte[] EncodePng(AndroidJavaObject bitmap)
        {
            using (var formatClass = new AndroidJavaClass("android.graphics.Bitmap$CompressFormat"))
            using (var png = formatClass.GetStatic<AndroidJavaObject>("PNG"))
            using (var stream = new AndroidJavaObject("java.io.ByteArrayOutputStream"))
            {
                bool ok = bitmap.Call<bool>("compress", png, PngFormatQuality, stream);
                if (!ok) return null;
                return stream.Call<byte[]>("toByteArray");
            }
        }

        private static int ArgbOf(Color color)
        {
            int a = Mathf.RoundToInt(color.a * 255f);
            int r = Mathf.RoundToInt(color.r * 255f);
            int g = Mathf.RoundToInt(color.g * 255f);
            int b = Mathf.RoundToInt(color.b * 255f);
            return unchecked((a << 24) | (r << 16) | (g << 8) | b);
        }
#endif
    }
}
