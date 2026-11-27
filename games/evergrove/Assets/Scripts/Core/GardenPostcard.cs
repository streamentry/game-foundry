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
            return text.ToString();
        }
    }
}
