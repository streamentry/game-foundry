using System;
using UnityEngine;

namespace Evergrove.UnityClient
{
    /// <summary>
    /// Standard or Large water board (EG-112). Large widens the board card,
    /// fills the available width and lifts the tile cap so 3x3 and 4x4 boards
    /// get bigger tiles. The preference lives in PlayerPrefs, separate from the
    /// world save, like <see cref="TextScale"/>. Layout only: no zoom or pan.
    /// </summary>
    public static class BoardSize
    {
        public const string PreferenceKey = "evergrove:board-size:v1";

        private const string DevArgument = "-mosseloraBoardSize";
        private static bool large;
        private static bool initialized;
        // Dev-only launch override: applies to this run and is never persisted.
        private static bool sessionOverride;

        public static bool IsLarge { get { return large; } }

        /// <summary>Reads the saved choice, or the dev-only launch override.</summary>
        public static void Initialize()
        {
            if (initialized) return;
            initialized = true;
            large = Load();
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            bool requested;
            if (TryReadOverride(out requested))
            {
                large = requested;
                sessionOverride = true;
                Debug.Log("MOSSELORA_BOARD_SIZE_OVERRIDE " + (large ? "large" : "standard"));
            }
#endif
        }

        public static bool Toggle()
        {
            return Choose(!large);
        }

        public static bool Choose(bool useLarge)
        {
            large = useLarge;
            if (sessionOverride) return true; // a QA run must not change the player's saved board
            try
            {
                PlayerPrefs.SetInt(PreferenceKey, large ? 1 : 0);
                PlayerPrefs.Save();
            }
            catch (Exception exception)
            {
                Debug.LogWarning("Evergrove board size not saved: " + exception.GetType().Name);
            }
            return true;
        }

        private static bool Load()
        {
            try
            {
                return PlayerPrefs.GetInt(PreferenceKey, 0) == 1;
            }
            catch (Exception)
            {
                return false;
            }
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private static bool TryReadOverride(out bool value)
        {
            value = false;
            string[] args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length - 1; i++)
            {
                if (!string.Equals(args[i], DevArgument, StringComparison.Ordinal)) continue;
                string requested = args[i + 1];
                if (string.Equals(requested, "large", StringComparison.OrdinalIgnoreCase))
                {
                    value = true;
                    return true;
                }
                if (string.Equals(requested, "standard", StringComparison.OrdinalIgnoreCase))
                    return true;
                Debug.LogWarning("Evergrove ignores board size; use standard or large.");
                return false;
            }
            return false;
        }
#endif
    }
}
