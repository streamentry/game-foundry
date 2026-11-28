using System;

namespace Evergrove.Core
{
    /// <summary>
    /// Small English helpers so neighbour lines can name a catalogue item correctly
    /// ("an oak", "sunflowers", "lavender"). Pure: null or empty input is safe and
    /// yields "not plural", "not mass", or an empty article phrase. English only.
    /// </summary>
    public static class ItemWords
    {
        private static readonly string[] MassNames =
        {
            "lavender", "chamomile", "clover", "heather", "honeysuckle",
            "jasmine", "wisteria", "lily of the valley"
        };

        /// <summary>
        /// True for names ending in "s" (so "cosmos" is plural), except singular endings "ss", "us" and
        /// "is" ("apple press", "lotus", "hibiscus", "vine trellis").
        /// </summary>
        public static bool Plural(string name)
        {
            if (string.IsNullOrEmpty(name) || name[name.Length - 1] != 's') return false;
            return !(name.EndsWith("ss", StringComparison.Ordinal) || name.EndsWith("us", StringComparison.Ordinal) ||
                     name.EndsWith("is", StringComparison.Ordinal));
        }

        /// <summary>True for names that take no plural and are uncountable in a garden line.</summary>
        public static bool Mass(string name)
        {
            if (string.IsNullOrEmpty(name)) return false;
            return Array.IndexOf(MassNames, name) >= 0;
        }

        /// <summary>"an " before a vowel letter, otherwise "a ". Empty input gives an empty string.</summary>
        public static string WithArticle(string name)
        {
            if (string.IsNullOrEmpty(name)) return "";
            return "aeiouAEIOU".IndexOf(name[0]) >= 0 ? "an " + name : "a " + name;
        }
    }
}
