using System;
using System.Collections.Generic;
using System.Text;

namespace Evergrove.Core
{
    /// <summary>
    /// A spoiler-free text postcard of a garden, for the player to paste into a
    /// message (vision 4.4, rung 1: no-server sharing). It shows only what the
    /// player chose and how far they have walked; no puzzle layouts, no score,
    /// no identifiers, no link tracking. Deterministic for a given world.
    /// </summary>
    public static class GardenPostcard
    {
        // Plain emoji so the postcard reads the same in any messaging app.
        private const string Tree = "\U0001F333";     // deciduous tree
        private const string Bridge = "\U0001F309";   // bridge at night (the only bridge emoji)
        private const string Flowers = "\U0001F33C";  // blossom
        private const string Passed = "●";       // black circle
        private const string Ahead = "○";        // white circle
        private const int ShownTogether = 3;

        /// <param name="world">The garden to describe.</param>
        /// <param name="streamLine">Localized "Stream n / 12" line (caller supplies the catalog text).</param>
        public static string Build(WorldModel world, string streamLine)
        {
            var text = new StringBuilder();
            text.Append("Mosselora\n");
            // Checklist 3: a named garden says where it is from. Unnamed gardens are unchanged.
            if (world.GardenName.Length > 0) text.Append("From ").Append(world.GardenName).Append('\n');
            int[] choices = world.Choices;
            for (int i = 0; i < choices.Length; i++)
            {
                text.Append(choices[i] == (int)VillageChoice.ShadeTree ? Tree
                    : choices[i] == (int)VillageChoice.Bridge ? Bridge : Flowers);
            }
            if (choices.Length > 0) text.Append('\n');
            int walked = world.Completed ? PuzzleCatalog.Count : world.Round;
            for (int i = 0; i < PuzzleCatalog.Count; i++) text.Append(i < walked ? Passed : Ahead);
            if (!string.IsNullOrEmpty(streamLine)) text.Append("  ").Append(streamLine);
            text.Append('\n');
            string together = SolvedTogetherLine(world);
            if (together.Length > 0) text.Append(together).Append('\n');
            return text.ToString();
        }

        /// <summary>
        /// Vision run 20261128-2001, idea 3: "Solved together: the A and the B." for the things two or
        /// more people solved by taking turns. Empty when no mark resolves to a name, so a garden
        /// without marks keeps the old postcard byte for byte.
        /// </summary>
        private static string SolvedTogetherLine(WorldModel world)
        {
            var names = new List<string>();
            foreach (int mark in world.Together)
            {
                string name = TogetherName(world, mark);
                if (name != null) names.Add(name);
            }
            if (names.Count == 0) return "";

            var list = new StringBuilder();
            int shown = Math.Min(names.Count, ShownTogether);
            for (int i = 0; i < shown; i++)
            {
                if (i > 0) list.Append(i == names.Count - 1 ? " and " : ", ");
                list.Append("the ").Append(names[i]);
            }
            if (names.Count > ShownTogether) list.Append(" and ").Append(names.Count - ShownTogether).Append(" more");
            return "Solved together: " + list + ".";
        }

        /// <summary>The item name for one together mark, or null when the mark does not resolve.</summary>
        private static string TogetherName(WorldModel world, int mark)
        {
            int[] choices = world.Choices;
            if (mark < WorldModel.BoughtBase)
            {
                if (mark >= choices.Length) return null;
                int chapter = Math.Min(mark, GardenCatalog.ChaptersPerGarden - 1);
                return GardenCatalog.For(world.Garden, chapter, (VillageChoice)choices[mark]).Name;
            }
            int k = mark - WorldModel.BoughtBase;
            int[] bought = world.Bought;
            if (2 * k + 1 >= bought.Length || bought[2 * k] < 0 || bought[2 * k] >= Nursery.Kinds) return null;
            return Nursery.Item(bought[2 * k]).Name;
        }
    }
}
