namespace Evergrove.Core
{
    /// <summary>
    /// Idea 5 (vision run 20261128-1502): the water board says how far the water reaches, in words a
    /// screen reader can read. "Water reaches 5 of 9 pipes." and, once every pipe is wet, the solved line.
    /// Pipes are every cell of the board: each cell is an end, a straight or a corner, so the total is the
    /// board's cell count, and the wet count is PipeInsight.ConnectedCount (cells reached from the source).
    /// Never throws.
    /// </summary>
    public static class WaterReachWords
    {
        /// <summary>The line for a wet count out of a total. Wet is clamped to 0..total; no pipes, no line.</summary>
        public static string Line(int wet, int total)
        {
            if (total <= 0) return "";
            if (wet < 0) wet = 0;
            if (wet > total) wet = total;
            if (wet == total) return "Water runs through every pipe.";
            return "Water reaches " + wet + " of " + total + " pipes.";
        }

        /// <summary>The line for a board's live inspection; no inspection, no line.</summary>
        public static string For(PipeInsight insight, int pipeCount)
        {
            if (insight == null) return "";
            return Line(insight.ConnectedCount, pipeCount);
        }
    }
}
