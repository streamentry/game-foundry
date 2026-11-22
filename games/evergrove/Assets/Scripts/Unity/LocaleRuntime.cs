using System;
using System.Collections.Generic;
using System.Globalization;
using Evergrove.Core;
using UnityEngine;

namespace Evergrove.UnityClient
{
    /// <summary>
    /// Explicit language override in PlayerPrefs; world save is never mutated.
    /// JSON bundles are embedded as TextAssets in Resources, so all languages
    /// work offline. A missing or invalid bundle falls back to English.
    /// </summary>
    public static class LocaleRuntime
    {
        private const string PreferenceKey = "evergrove:language-choice:v1";
        public static string Preference { get { return PlayerPrefs.GetString(PreferenceKey, GameLocalization.Auto); } }
        public static string Current { get { return GameLocalization.Current; } }

        // Research builds expose the 30 draft catalogs for testing. An
        // ordinary player build offers only explicitly allowed launch
        // candidates; currently English alone, still NOT certified.
        private static bool ResearchBuild
        {
            get
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                return true;
#else
                return false;
#endif
            }
        }

        public static IReadOnlyList<GameLanguage> SelectableLanguages
        {
            get { return LocaleShippingPolicy.Selectable(ResearchBuild); }
        }

        public static void Initialize()
        {
            Apply(Preference);
        }

        public static bool Apply(string preference)
        {
            string code = LocaleShippingPolicy.Resolve(
                GameLocalization.Detect(
                    preference, CultureInfo.CurrentUICulture.Name,
                    Application.systemLanguage.ToString()),
                ResearchBuild);
            GameLocalization.SetCurrent(GameLocalization.Fallback);
            if (code == "en") return true;
            try
            {
                var asset = Resources.Load<TextAsset>("Evergrove/Locales/" + code);
                if (asset == null) return false;
                TranslationBundle bundle = JsonUtility.FromJson<TranslationBundle>(asset.text);
                if (bundle == null ||
                    !string.Equals(bundle.code, code, StringComparison.OrdinalIgnoreCase))
                    return false;
                return GameLocalization.Register(bundle) && GameLocalization.SetCurrent(code);
            }
            catch (Exception exception)
            {
                Debug.LogWarning("Evergrove language bundle failed: " + exception.GetType().Name);
                return false;
            }
        }

        public static bool Choose(string preference)
        {
            if (preference != GameLocalization.Auto &&
                !LocaleShippingPolicy.Allows(preference, ResearchBuild))
                return false;
            string oldPreference = Preference;
            if (!Apply(preference))
            {
                Apply(oldPreference);
                return false;
            }
            PlayerPrefs.SetString(PreferenceKey, preference);
            PlayerPrefs.Save();
            return true;
        }
    }

    /// <summary>
    /// Best-effort OS font selection for non-Latin scripts. No licensed fonts
    /// are embedded in the repo. Until native-device QA + RTL/Indic shaping
    /// validation, font presence alone is NOT a finished-language sign-off.
    /// </summary>
    public static class LocaleFonts
    {
        private static readonly Dictionary<string, Font> Cache =
            new Dictionary<string, Font>(StringComparer.OrdinalIgnoreCase);
        // Opening the 30-language picker should not generate 15 copies of
        // the same Latin font or enumerate OS fonts 30 times.
        private static readonly Dictionary<string, Font> ByFamily =
            new Dictionary<string, Font>(StringComparer.OrdinalIgnoreCase);
        private static string[] installedFonts;

        public static Font Choose(string language, Font defaultFont)
        {
            if (language == "en") return defaultFont;
            Font cached;
            if (Cache.TryGetValue(language, out cached) && cached != null)
                return cached;
            string[] candidates = Candidates(language);
            try
            {
                if (installedFonts == null) installedFonts = Font.GetOSInstalledFontNames();
                string[] installed = installedFonts;
                foreach (string desired in candidates)
                {
                    foreach (string available in installed)
                    {
                        if (!available.Equals(desired, StringComparison.OrdinalIgnoreCase))
                            continue;
                        Font candidate;
                        if (!ByFamily.TryGetValue(available, out candidate) || candidate == null)
                        {
                            candidate = Font.CreateDynamicFontFromOSFont(available, 17);
                            if (candidate != null) ByFamily[available] = candidate;
                        }
                        if (candidate != null)
                        {
                            Cache[language] = candidate;
                            return candidate;
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                Debug.LogWarning("Evergrove OS font lookup unavailable: " + exception.GetType().Name);
            }
            return defaultFont;
        }

        private static string[] Candidates(string language)
        {
            switch (language)
            {
                case "zh-Hans": return new[] { "Noto Sans CJK SC", "Noto Sans SC", "PingFang SC", "Microsoft YaHei" };
                case "zh-Hant": return new[] { "Noto Sans CJK TC", "Noto Sans TC", "PingFang TC", "Microsoft JhengHei" };
                case "ja": return new[] { "Noto Sans CJK JP", "Noto Sans JP", "Hiragino Sans", "Yu Gothic" };
                case "ko": return new[] { "Noto Sans CJK KR", "Noto Sans KR", "Apple SD Gothic Neo", "Malgun Gothic" };
                case "ar": case "fa": case "ur":
                    return new[] { "Noto Naskh Arabic", "Noto Sans Arabic", "Geeza Pro", "Arial Unicode MS" };
                case "he": return new[] { "Noto Sans Hebrew", "Arial Hebrew", "Arial Unicode MS" };
                case "hi": return new[] { "Noto Sans Devanagari", "Kohinoor Devanagari", "Mangal" };
                case "bn": return new[] { "Noto Sans Bengali", "Kohinoor Bangla", "Vrinda" };
                case "ta": return new[] { "Noto Sans Tamil", "Tamil Sangam MN", "Latha" };
                case "te": return new[] { "Noto Sans Telugu", "Kohinoor Telugu", "Gautami" };
                case "th": return new[] { "Noto Sans Thai", "Thonburi", "Tahoma" };
                default: return new[] { "Noto Sans", "Arial", "Helvetica Neue", "Roboto" };
            }
        }
    }
}
