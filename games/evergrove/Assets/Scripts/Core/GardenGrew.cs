using System;
using System.Globalization;
using System.Text;

namespace Evergrove.Core
{
    /// <summary>
    /// Checklist 33, "Garden layouts that remember": the almanac page "How my garden grew". One
    /// short line for each good-neighbour pairing the garden has found, in the order the pairings
    /// are defined, then one closing count. It reads only the saved pairings and the garden name,
    /// so the same save always gives the same page, including after a save or garden-file round
    /// trip. Pure: no clock, no UI, no randomness.
    /// </summary>
    public static class GardenGrew
    {
        /// <summary>Title, at most one line per pairing (seven), and the closing count.</summary>
        public const int MaxLines = 9;
        public const int MaxLineLength = 80;
        public const string EmptyLine = "Nothing paired up yet. Things that grow well together will show here.";

        /// <summary>
        /// The page as text: lines joined by '\n'. Only Living gardens have pairings, so any other
        /// garden gets the title and the empty-state line.
        /// </summary>
        public static string Page(WorldModel world)
        {
            var text = new StringBuilder();
            text.Append(Title(world));
            int found = 0;
            foreach (Pairing pairing in Enum.GetValues(typeof(Pairing)))
            {
                if (!Has(world, pairing)) continue;
                found++;
                text.Append('\n').Append(Pairings.Hint(pairing));
            }
            if (found == 0)
            {
                text.Append('\n').Append(EmptyLine);
                return text.ToString();
            }
            int total = Enum.GetValues(typeof(Pairing)).Length;
            text.Append('\n')
                .Append(found.ToString(CultureInfo.InvariantCulture))
                .Append(" of ")
                .Append(total.ToString(CultureInfo.InvariantCulture))
                .Append(found == 1 ? " good-neighbour pairing found." : " good-neighbour pairings found.");
            return text.ToString();
        }

        private static string Title(WorldModel world)
        {
            string name = world == null ? "" : world.GardenName;
            return name.Length > 0 ? "How " + name + " grew" : "How my garden grew";
        }

        private static bool Has(WorldModel world, Pairing pairing)
        {
            return world != null && world.Mode == PlayMode.Living && world.HasPairing(pairing);
        }
    }
}
