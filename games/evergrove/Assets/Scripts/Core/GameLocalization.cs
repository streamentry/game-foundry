using System;
using System.Collections.Generic;
using System.Globalization;

namespace Evergrove.Core
{
    [Serializable]
    public sealed class TranslationEntry
    {
        public string key;
        public string value;
    }

    // Unity's JsonUtility supports flat arrays of serializable objects.
    [Serializable]
    public sealed class TranslationBundle
    {
        public string code;
        public TranslationEntry[] entries;
    }

    public readonly struct GameLanguage
    {
        public readonly string Code;
        public readonly string NativeName;
        public readonly bool RightToLeft;

        public GameLanguage(string code, string nativeName, bool rightToLeft = false)
        {
            Code = code;
            NativeName = nativeName;
            RightToLeft = rightToLeft;
        }
    }

    /// <summary>
    /// Offline, deterministic game-localization registry. Never touches world
    /// state; language preferences are stored independently by the Unity layer.
    /// Human-native review and RTL font shaping remain explicit release gates.
    /// </summary>
    public static class GameLocalization
    {
        public const string Auto = "auto";
        public const string Fallback = "en";
        private static readonly GameLanguage[] Languages = {
            new GameLanguage("en", "English"),
            new GameLanguage("zh-Hans", "简体中文"),
            new GameLanguage("zh-Hant", "繁體中文"),
            new GameLanguage("es", "Español"),
            new GameLanguage("pt-BR", "Português (Brasil)"),
            new GameLanguage("fr", "Français"),
            new GameLanguage("de", "Deutsch"),
            new GameLanguage("ja", "日本語"),
            new GameLanguage("ko", "한국어"),
            new GameLanguage("ar", "العربية", true),
            new GameLanguage("hi", "हिन्दी"),
            new GameLanguage("ru", "Русский"),
            new GameLanguage("id", "Bahasa Indonesia"),
            new GameLanguage("vi", "Tiếng Việt"),
            new GameLanguage("tr", "Türkçe"),
            new GameLanguage("th", "ไทย"),
            new GameLanguage("it", "Italiano"),
            new GameLanguage("nl", "Nederlands"),
            new GameLanguage("pl", "Polski"),
            new GameLanguage("uk", "Українська"),
            new GameLanguage("fa", "فارسی", true),
            new GameLanguage("he", "עברית", true),
            new GameLanguage("ms", "Bahasa Melayu"),
            new GameLanguage("fil", "Filipino"),
            new GameLanguage("bn", "বাংলা"),
            new GameLanguage("ur", "اردو", true),
            new GameLanguage("ta", "தமிழ்"),
            new GameLanguage("te", "తెలుగు"),
            new GameLanguage("sw", "Kiswahili"),
            new GameLanguage("sv", "Svenska")
        };

        private static readonly string[] Ids = {
            "title","subtitle","language","auto","language_help",
            "living","puzzle","puzzle_desc","memory_title","continue","change",
            "complete","explore","erase","keep","neighbors","thanks","playtest",
            "art_2d","art_3d","motion_on","motion_off","sound_on","sound_off",
            "record","privacy","copy","copied","clear","offline",
            "journal_show","journal_hide","journal_title","journal_empty",
            "stream","first","solved","progress","undo_last","next","choose",
            "undo","hint","restart","how",
            "tree","bridge","flowers","memory_tree","memory_bridge","memory_flowers",
            "world_none","world_tree","world_bridge","world_flowers",
            "world_tree_bridge","world_tree_flowers","world_bridge_flowers","world_all",
            "mira_new","mira_after","orin_new","orin_after","fen_new","fen_after",
            "ending_all","ending_tree","ending_bridge","ending_flowers",
            "prompt_early","prompt_mid","prompt_late","journal_entry",
            "landmark_tree","landmark_bridge","landmark_flowers",
            "hint_notice","hint_focus","hint_more","hint_turn","save_warning","ending_title",
            "sr_tile_wet","sr_tile_dry","sr_straight","sr_corner","sr_spring","sr_outlet"
        };

        private static readonly string[] English = {
            "MOSSELORA",
            "A living garden that remembers.",
            "Language",
            "Automatic (device language)",
            "The device language is used until you choose another.",
            "Living world",
            "Puzzle only",
            "Same water puzzles, without the living-world layer.",
            "A NEW MEMORY IN THE GROVE",
            "Continue along the stream",
            "Change this choice",
            "Twelve streams completed. Which puzzle did you enjoy most?",
            "Explore another garden",
            "Confirm: erase this garden",
            "Keep this garden",
            "Talk to a neighbor",
            "Thanks for telling me",
            "PLAYTEST MODE · Choose before your first move",
            "Visual: 2D storybook (tap for 2.5D)",
            "Visual: 2.5D miniature (tap for 2D)",
            "Miniature motion: ON (tap to stop)",
            "Miniature motion: OFF (tap to animate)",
            "Gentle chime: ON (tap to mute)",
            "Gentle chime: OFF (tap to enable)",
            "Start an optional private playtest",
            "Only starts when you tap. Nothing is uploaded or linked to you.",
            "Copy my local playtest notes",
            "Copied to clipboard.",
            "Stop and erase local notes",
            "Offline · No timer · No ads · No account",
            "Read village memories",
            "Hide village memories",
            "OUR CHOICES",
            "The first memory is still waiting.",
            "STREAM {0} / {1}",
            "Blue waves = spring · green sprout = garden. Tap to connect water.",
            "The water flows through every tile.",
            "Water reaches {0}/{1} tiles · {2} open joins",
            "Undo the last water turn",
            "Next stream",
            "What will you help grow?",
            "Undo",
            "Guide me",
            "Restart stream",
            "Tap to turn a pipe. Circles mark open ends; guide water to the garden.",
            "Plant a shade tree",
            "Restore a crossing",
            "Grow wildflowers",
            "Mira remembers the tree you planted.",
            "Orin remembers the bridge you restored.",
            "Fen remembers the flowers you grew.",
            "The old creek waits for water.",
            "A young tree grows by the stream.",
            "The crossing connects both banks.",
            "Wildflowers brighten the bank.",
            "Your trees shade the restored bridge.",
            "Butterflies gather beneath the trees.",
            "Neighbors cross to see the meadow.",
            "Trees, paths and flowers bring the village together.",
            "I'm Mira. Could we plant a tree beside the water?",
            "I remember the trees you planted. The village has shade now.",
            "I'm Orin. I hope we can rebuild the crossing.",
            "The restored bridge brings us together. Thank you!",
            "I'm Fen. I miss seeing wildflowers along the water.",
            "Your flowers have brought life back to the bank.",
            "Our village thrives. Every choice helped make a home.",
            "A quiet orchard grew from your care.",
            "Your bridges brought neighbors together.",
            "You made a meadow where life returned.",
            "A little water can change a place.",
            "The village is beginning to grow.",
            "Every choice becomes part of this place.",
            "Chapter {0} · {1} · {2}",
            "Tree roots anchor this pipe. It cannot rotate.",
            "Bridge foundations anchor this pipe. It cannot rotate.",
            "Wildflowers protect this watercourse. It cannot rotate.",
            "Notice row {0}. One channel needs attention.",
            "Row {0}, column {1}: compare the open ends.",
            "Show a closer clue",
            "Fix one connection",
            "Progress could not be saved. Check device storage.",
            "YOUR GARDEN'S STORY",
            "Row {0}, column {1}: {2}, connected to water.",
            "Row {0}, column {1}: {2}, dry.",
            "straight pipe",
            "corner pipe",
            "water spring",
            "garden outlet"
        };

        private static readonly Dictionary<string, int> Index = CreateIndex();
        private static readonly Dictionary<string, string[]> Translations =
            new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
        private static string current = Fallback;

        public static IReadOnlyList<GameLanguage> Available { get { return Languages; } }
        public static IReadOnlyList<string> Keys { get { return Ids; } }
        public static string Current { get { return current; } }

        private static Dictionary<string, int> CreateIndex()
        {
            if (Ids.Length != English.Length) throw new InvalidOperationException("Key table size mismatch.");
            var index = new Dictionary<string, int>(StringComparer.Ordinal);
            for (int i = 0; i < Ids.Length; i++) index.Add(Ids[i], i);
            return index;
        }

        public static bool IsSupported(string code)
        {
            if (code == null) return false;
            foreach (GameLanguage language in Languages)
                if (language.Code.Equals(code, StringComparison.OrdinalIgnoreCase))
                    return true;
            return false;
        }

        public static bool IsRtl(string code)
        {
            foreach (GameLanguage language in Languages)
                if (language.Code.Equals(code, StringComparison.OrdinalIgnoreCase))
                    return language.RightToLeft;
            return false;
        }

        public static string NativeName(string code)
        {
            foreach (GameLanguage language in Languages)
                if (language.Code.Equals(code, StringComparison.OrdinalIgnoreCase))
                    return language.NativeName;
            return "English";
        }

        /// <summary>
        /// The input is a BCP-47 language tag. Defaults: Portuguese to Brazil;
        /// Chinese HK/MO/TW and explicit Hant to traditional; others simplified.
        /// Unsupported languages fail closed to English.
        /// </summary>
        public static string Normalize(string tag)
        {
            if (string.IsNullOrWhiteSpace(tag)) return Fallback;
            string text = tag.Trim().Replace('_', '-').ToLowerInvariant();
            if (text == "auto") return Auto;
            if (text.StartsWith("zh"))
            {
                if (text.Contains("hant") || text.Contains("-tw") ||
                    text.Contains("-hk") || text.Contains("-mo")) return "zh-Hant";
                return "zh-Hans";
            }
            string head = text.Split('-')[0];
            if (head == "pt") return "pt-BR";
            if (head == "tl") return "fil";
            foreach (GameLanguage language in Languages)
                if (language.Code.Equals(head, StringComparison.OrdinalIgnoreCase))
                    return language.Code;
            return Fallback;
        }

        public static string Detect(string preference, string uiCulture, string unitySystemLanguage)
        {
            if (!string.IsNullOrWhiteSpace(preference) &&
                !preference.Equals(Auto, StringComparison.OrdinalIgnoreCase) &&
                (IsSupported(preference) ||
                 preference.StartsWith("zh-", StringComparison.OrdinalIgnoreCase) ||
                 preference.StartsWith("pt-", StringComparison.OrdinalIgnoreCase)))
            {
                string chosen = Normalize(preference);
                if (IsSupported(chosen)) return chosen;
            }
            if (!string.IsNullOrWhiteSpace(uiCulture) && Normalize(uiCulture) != Fallback)
                return Normalize(uiCulture);
            // Application.systemLanguage.ToString() is an enum name, not BCP-47.
            // On some WebGL/Unity runtimes, CurrentUICulture defaults to en-US
            // even while the browser's actual device language is non-English.
            // Prefer that platform signal before a generic English fallback.
            var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "Chinese", "zh-Hans" }, { "ChineseSimplified", "zh-Hans" },
                { "ChineseTraditional", "zh-Hant" },
                { "Portuguese", "pt-BR" }, { "Filipino", "fil" },
                { "Indonesian", "id" }, { "Vietnamese", "vi" },
                { "Japanese", "ja" }, { "Korean", "ko" },
                { "Hindi", "hi" }, { "Arabic", "ar" },
                { "Hebrew", "he" }, { "Persian", "fa" },
                { "Turkish", "tr" }, { "Thai", "th" },
                { "Russian", "ru" }, { "Ukrainian", "uk" },
                { "Dutch", "nl" }, { "Polish", "pl" },
                { "French", "fr" }, { "German", "de" },
                { "Italian", "it" }, { "Spanish", "es" },
                { "Swedish", "sv" }, { "English", "en" }
            };
            if (!string.IsNullOrWhiteSpace(unitySystemLanguage))
            {
                string code;
                if (map.TryGetValue(unitySystemLanguage, out code)) return code;
                string asTag = Normalize(unitySystemLanguage);
                if (asTag != Fallback) return asTag;
            }
            return Fallback;
        }

        public static bool Register(TranslationBundle bundle)
        {
            if (bundle == null || !IsSupported(bundle.code) || bundle.entries == null)
                return false;
            var entries = new string[Ids.Length];
            var seen = new HashSet<string>(StringComparer.Ordinal);
            foreach (TranslationEntry item in bundle.entries)
            {
                if (item == null || string.IsNullOrEmpty(item.key) ||
                    !seen.Add(item.key) || string.IsNullOrWhiteSpace(item.value))
                    return false;
                int pos;
                if (!Index.TryGetValue(item.key, out pos)) return false;
                entries[pos] = item.value;
                if (!PlaceholdersMatch(English[pos], item.value)) return false;
            }
            // Register only COMPLETE catalogs. No silent, patchwork English UI.
            for (int i = 0; i < entries.Length; i++)
                if (entries[i] == null) return false;
            Translations[Normalize(bundle.code)] = entries;
            return true;
        }

        private static bool PlaceholdersMatch(string a, string b)
        {
            for (int i = 0; i < 4; i++)
                if (a.Contains("{" + i + "}") != b.Contains("{" + i + "}"))
                    return false;
            return true;
        }

        public static bool SetCurrent(string code)
        {
            if (!IsSupported(code)) return false;
            string selected = Normalize(code);
            if (!IsSupported(selected)) return false;
            if (selected != Fallback && !Translations.ContainsKey(selected)) return false;
            current = selected;
            return true;
        }

        public static string Text(string key, params object[] args)
        {
            int index;
            if (!Index.TryGetValue(key ?? "", out index)) return "[missing:" + key + "]";
            string message = English[index];
            string[] values;
            if (current != Fallback && Translations.TryGetValue(current, out values))
                message = values[index];
            return args == null || args.Length == 0
                ? message
                : string.Format(CultureInfo.InvariantCulture, message, args);
        }

        /// <summary>
        /// For machine-checked UI copy that has already been identified by its
        /// English text. Unknown strings are English and are logged during QA.
        /// Game logic never relies on translated prose.
        /// </summary>
        public static string Display(string english)
        {
            int index = Array.IndexOf(English, english);
            return index < 0 ? english : Text(Ids[index]);
        }

        public static string Source(string key)
        {
            int index;
            return Index.TryGetValue(key, out index) ? English[index] : null;
        }
    }
}
