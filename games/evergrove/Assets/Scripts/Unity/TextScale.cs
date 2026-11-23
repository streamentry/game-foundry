using System;
using UnityEngine;

namespace Evergrove.UnityClient
{
    /// <summary>
    /// In-game text size (100%, 125%, 150%). Unity legacy uGUI Text ignores the
    /// OS text-size setting, so every generated label, button and banner passes
    /// its base size through <see cref="Scale"/>. The preference lives in
    /// PlayerPrefs, separate from the world save, like the language choice.
    /// Board tiles and pipe geometry are deliberately NOT scaled.
    /// </summary>
    public static class TextScale
    {
        public const string PreferenceKey = "evergrove:text-scale:v1";
        public const int DefaultPercent = 100;
        public static readonly int[] Steps = { 100, 125, 150 };

        private const string DevArgument = "-mosseloraTextScale";
        private static int percent = DefaultPercent;
        private static bool initialized;
        // Dev-only launch override: applies to this run and is never persisted.
        private static bool sessionOverride;

        public static int Percent { get { return percent; } }
        public static float Factor { get { return percent / 100f; } }

        /// <summary>Reads the saved size, or the dev-only launch override.</summary>
        public static void Initialize()
        {
            if (initialized) return;
            initialized = true;
            percent = Load();
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            int requested;
            if (TryReadOverride(out requested))
            {
                percent = requested;
                sessionOverride = true;
                Debug.Log("MOSSELORA_TEXT_SCALE_OVERRIDE " + percent);
            }
#endif
        }

        /// <summary>Scales a base point size; rounds half up so 15 * 125% = 19.</summary>
        public static int Scale(int basePoints)
        {
            return Mathf.FloorToInt(basePoints * Factor + 0.5f);
        }

        /// <summary>Advances 100 to 125 to 150 and back to 100.</summary>
        public static bool Cycle()
        {
            int index = Array.IndexOf(Steps, percent);
            return Choose(Steps[(index + 1) % Steps.Length]);
        }

        public static bool Choose(int next)
        {
            if (Array.IndexOf(Steps, next) < 0) return false;
            percent = next;
            if (sessionOverride) return true; // a QA run must not change the player's saved size
            try
            {
                PlayerPrefs.SetInt(PreferenceKey, next);
                PlayerPrefs.Save();
            }
            catch (Exception exception)
            {
                Debug.LogWarning("Evergrove text size not saved: " + exception.GetType().Name);
            }
            return true;
        }

        private static int Load()
        {
            try
            {
                int saved = PlayerPrefs.GetInt(PreferenceKey, DefaultPercent);
                return Array.IndexOf(Steps, saved) >= 0 ? saved : DefaultPercent;
            }
            catch (Exception)
            {
                return DefaultPercent;
            }
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private static bool TryReadOverride(out int value)
        {
            value = DefaultPercent;
            string[] args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length - 1; i++)
            {
                if (!string.Equals(args[i], DevArgument, StringComparison.Ordinal)) continue;
                int parsed;
                if (int.TryParse(args[i + 1], out parsed) && Array.IndexOf(Steps, parsed) >= 0)
                {
                    value = parsed;
                    return true;
                }
                Debug.LogWarning("Evergrove ignores text scale; use 100, 125 or 150.");
                return false;
            }
            return false;
        }
#endif
    }
}
