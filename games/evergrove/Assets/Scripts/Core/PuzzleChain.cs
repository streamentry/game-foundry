using System;

namespace Evergrove.Core
{
    /// <summary>
    /// Checklist 38, chains between puzzles (visible only): finishing a stream path waters the
    /// garden's plots, and finishing the plots sends a seedling to the nursery. Each link names
    /// its one cause in a short line on the solved card, and the hub shows a soft watered mark for
    /// the rest of that day. Pure rules: no seeds, no stock, no saved world change.
    /// </summary>
    public static class PuzzleChain
    {
        public const string StreamCause = "The stream runs on and waters the garden plots.";
        public const string SeedlingCause = "The plots send a seedling to the nursery.";

        /// <summary>
        /// The one line to show after a solve, or null. A stream path needs a thing holding the plots
        /// puzzle in this garden; plots need the nursery (a living garden, which has the basket).
        /// </summary>
        public static string CauseFor(PuzzleKind solved, bool gardenHasPlots, bool nurseryOpen)
        {
            if (solved == PuzzleKind.StreamPath) return gardenHasPlots ? StreamCause : null;
            if (solved == PuzzleKind.Plots) return nurseryOpen ? SeedlingCause : null;
            return null;
        }

        /// <summary>True when a chosen story thing or a nursery plant holds the plots puzzle.</summary>
        public static bool GardenHoldsPlots(int[] choices, int[] bought)
        {
            if (choices != null)
                for (int i = 0; i < choices.Length; i++)
                    if (choices[i] >= 0 && ThingPuzzle.ForStory(i, (VillageChoice)Math.Min(choices[i], 2)) == PuzzleKind.Plots)
                        return true;
            if (bought != null)
                for (int k = 0; k * 2 + 1 < bought.Length; k++)
                {
                    int kind = bought[k * 2];
                    if (kind >= 0 && kind < Nursery.Kinds && ThingPuzzle.ForNursery(kind) == PuzzleKind.Plots) return true;
                }
            return false;
        }

        /// <summary>A local calendar day as yyyymmdd (0 is never a day).</summary>
        public static int DayStamp(DateTime day)
        {
            return day.Year * 10000 + day.Month * 100 + day.Day;
        }

        /// <summary>The garden was watered today: a stream path was solved on this same day.</summary>
        public static bool WateredToday(int lastStreamSolveDay, int today)
        {
            return today > 0 && lastStreamSolveDay == today;
        }

        /// <summary>A seedling is ready in the nursery today: a plots puzzle was solved on this same day.</summary>
        public static bool SeedlingToday(int lastPlotsSolveDay, int today)
        {
            return today > 0 && lastPlotsSolveDay == today;
        }
    }
}
