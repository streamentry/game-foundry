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
    /// A living choice also says which stream its pipe shaped, right after the "since" sentence:
    /// "Its roots hold a pipe in stream 4." (present while that board is in play) or "It held a pipe in stream 3."
    /// (past). The shaped stream is the one after the choice's own, read from the recorded choice and round.
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
            return Line(name, choiceIndex, choicesMade, "", finished, together);
        }

        /// <summary>
        /// As the finished/together overload, with <paramref name="shaped"/> (a sentence from
        /// <see cref="ShapedLine"/>, or "") placed right after the "since" sentence. An empty
        /// <paramref name="shaped"/> adds nothing. Still "" wherever the plain line is "".
        /// </summary>
        public static string Line(string name, int choiceIndex, int choicesMade, string shaped, bool finished, bool together)
        {
            string line = Line(name, choiceIndex, choicesMade);
            if (line.Length == 0) return "";
            if (!string.IsNullOrEmpty(shaped)) line += " " + shaped;
            if (finished) line += " You finished its puzzle.";
            if (finished && together) line += " You solved it together.";
            return line;
        }

        /// <summary>
        /// Pure sentence saying which stream a chosen thing's pipe shaped. A ShadeTree or Flowers
        /// roots hold a pipe; a Bridge holds one. <paramref name="current"/> is the present tense
        /// (the shaped board is the one now in play); otherwise the past tense. "" when
        /// <paramref name="shapedStream"/> is below 1 or the family is not a known choice.
        /// </summary>
        public static string ShapedLine(VillageChoice family, int shapedStream, bool current)
        {
            if (shapedStream < 1) return "";
            string stream = shapedStream.ToString(System.Globalization.CultureInfo.InvariantCulture);
            switch (family)
            {
                case VillageChoice.ShadeTree:
                case VillageChoice.Flowers:
                    return (current ? "Its roots hold a pipe in stream " : "Its roots held a pipe in stream ") + stream + ".";
                case VillageChoice.Bridge:
                    return (current ? "It holds a pipe in stream " : "It held a pipe in stream ") + stream + ".";
                default:
                    return "";
            }
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
            return Line(name, choiceIndex, made, ShapedFor(world, choiceIndex), world.TreeSolved(choiceIndex),
                world.SolvedTogether(choiceIndex));
        }

        /// <summary>
        /// The shaped-stream sentence for choice <paramref name="choiceIndex"/>: choice i shapes board i+1,
        /// which is stream i+2. Present tense while that board is the one in play. The last story choice
        /// shapes nothing, so it gives "".
        /// </summary>
        private static string ShapedFor(WorldModel world, int choiceIndex)
        {
            if (choiceIndex < 0 || choiceIndex + 1 >= PuzzleCatalog.Count) return "";
            int family = world.Choices[choiceIndex];
            if (family < 0 || family > 2) return "";
            return ShapedLine((VillageChoice)family, choiceIndex + 2, world.Round == choiceIndex + 1);
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
