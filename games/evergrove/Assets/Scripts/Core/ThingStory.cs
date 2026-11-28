using System;

namespace Evergrove.Core
{
    /// <summary>
    /// The caption for a grown thing the player chose: "Quince tree, chosen after stream 3.
    /// Four streams have run past it since." Counted in streams (choices), never in days or
    /// clock time, so every number is a fact the save already holds. English only; the Unity
    /// caller keeps the family line for other languages.
    /// </summary>
    public static class ThingStory
    {
        private static readonly string[] Words =
        {
            "", "One", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine", "Ten", "Eleven", "Twelve"
        };

        /// <summary>
        /// Pure sentence. <paramref name="choiceIndex"/> is the stream (0-based) where the thing was
        /// chosen; <paramref name="choicesMade"/> is how many streams have been chosen so far.
        /// Returns "" for a blank name or an index outside 0 .. choicesMade - 1.
        /// </summary>
        public static string Line(string name, int choiceIndex, int choicesMade)
        {
            if (string.IsNullOrWhiteSpace(name)) return "";
            if (choiceIndex < 0 || choiceIndex >= choicesMade) return "";

            string thing = Capitalise(name.Trim());
            int since = choicesMade - 1 - choiceIndex;
            return thing + ", chosen after stream " + (choiceIndex + 1) + ". " + Since(since);
        }

        /// <summary>
        /// Truthful wrapper for the app. Only a living world's recorded choices can be described;
        /// puzzle-only, matched, missing or out-of-range choices give "".
        /// </summary>
        public static string For(WorldModel world, int choiceIndex, string name)
        {
            if (world == null || world.Mode != PlayMode.Living) return "";
            int made = world.Choices.Length;
            if (choiceIndex < 0 || choiceIndex >= made) return "";
            return Line(name, choiceIndex, made);
        }

        private static string Since(int count)
        {
            if (count == 0) return "No streams have run past it yet.";
            if (count == 1) return "One stream has run past it since.";
            if (count <= 12) return Words[count] + " streams have run past it since.";
            return count.ToString(System.Globalization.CultureInfo.InvariantCulture) +
                " streams have run past it since.";
        }

        private static string Capitalise(string text)
        {
            return char.ToUpperInvariant(text[0]) + text.Substring(1);
        }
    }
}
