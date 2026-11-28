using System;

namespace Evergrove.Core
{
    /// <summary>
    /// The caption for a grown thing the player chose: "Quince tree, chosen after stream 3.
    /// Four streams have run past it since." When its puzzle was finished, the caption adds
    /// " You finished its puzzle."; when it was also solved together, the caption ends with
    /// " You solved it together." Counted in streams (choices), never in days or
    /// clock time, so every number is a fact the save already holds. Nursery plants read
    /// "Name, from the nursery." (with the puzzle and together facts once solved), and basket
    /// plantings read "Planted from your basket. It came from the name you chose after stream 3." English only; the Unity
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
        /// As <see cref="Line(string, int, int)"/>, plus " You solved it together." when
        /// <paramref name="together"/> is true. Still "" wherever the plain line is "".
        /// </summary>
        public static string Line(string name, int choiceIndex, int choicesMade, bool together)
        {
            string line = Line(name, choiceIndex, choicesMade);
            if (line.Length == 0 || !together) return line;
            return line + " You solved it together.";
        }

        /// <summary>
        /// As <see cref="Line(string, int, int)"/>, plus " You finished its puzzle." when
        /// <paramref name="finished"/> is true, and " You solved it together." only when
        /// <paramref name="finished"/> and <paramref name="together"/> are both true (a together
        /// mark alone adds nothing). Still "" wherever the plain line is "".
        /// </summary>
        public static string Line(string name, int choiceIndex, int choicesMade, bool finished, bool together)
        {
            string line = Line(name, choiceIndex, choicesMade);
            if (line.Length == 0) return "";
            if (finished) line += " You finished its puzzle.";
            if (finished && together) line += " You solved it together.";
            return line;
        }

        /// <summary>
        /// Truthful wrapper for the app. Only a living world's recorded choices can be described;
        /// puzzle-only, matched, missing or out-of-range choices give "". The finished and together
        /// sentences come only from the world's recorded puzzle solves and "solved together" marks.
        /// </summary>
        public static string For(WorldModel world, int choiceIndex, string name)
        {
            if (world == null || world.Mode != PlayMode.Living) return "";
            int made = world.Choices.Length;
            if (choiceIndex < 0 || choiceIndex >= made) return "";
            return Line(name, choiceIndex, made, world.TreeSolved(choiceIndex), world.SolvedTogether(choiceIndex));
        }

        /// <summary>
        /// Pure sentence for a nursery plant. "" for a blank name. "You grew it from its puzzle." needs
        /// <paramref name="grown"/>; "You solved it together." needs both <paramref name="grown"/> and
        /// <paramref name="together"/> (a together mark alone adds nothing).
        /// </summary>
        public static string NurseryLine(string name, bool grown, bool together)
        {
            if (string.IsNullOrWhiteSpace(name)) return "";
            string line = Capitalise(name.Trim()) + ", from the nursery.";
            if (grown) line += " You grew it from its puzzle.";
            if (grown && together) line += " You solved it together.";
            return line;
        }

        /// <summary>
        /// Truthful wrapper for a bought plant k. "" when the world is null or k is outside the bought
        /// pairs or the garden is not Living. Together is read only from the recorded marks.
        /// </summary>
        public static string ForNursery(WorldModel world, int k, string name)
        {
            if (world == null || world.Mode != PlayMode.Living) return "";
            if (k < 0 || k >= world.Bought.Length / 2) return "";
            bool grown = world.BoughtSolved(k);
            bool together = grown && world.SolvedTogether(WorldModel.BoughtBase + k);
            return NurseryLine(name, grown, together);
        }

        /// <summary>
        /// Pure sentence for a basket planting of choice <paramref name="sourceChoiceIndex"/>.
        /// Returns "" for a blank name or an index outside 0 .. choicesMade - 1. The name is used as given, trimmed.
        /// </summary>
        public static string PlantingLine(string sourceName, int sourceChoiceIndex, int choicesMade)
        {
            if (string.IsNullOrWhiteSpace(sourceName)) return "";
            if (sourceChoiceIndex < 0 || sourceChoiceIndex >= choicesMade) return "";
            return "Planted from your basket. It came from the " + sourceName.Trim() + " you chose after stream " +
                (sourceChoiceIndex + 1) + ".";
        }

        /// <summary>
        /// Truthful wrapper for basket planting k (a pair in world.Plantings). "" when k is out of range
        /// or the recorded source is not a chosen thing.
        /// </summary>
        public static string ForPlanting(WorldModel world, int k, string sourceName)
        {
            if (world == null || world.Mode != PlayMode.Living) return "";
            int[] planted = world.Plantings;
            if (k < 0 || k * 2 + 1 >= planted.Length) return "";
            int source = planted[k * 2];
            int made = world.Choices.Length;
            if (source < 0 || source >= made) return "";
            return PlantingLine(sourceName, source, made);
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
