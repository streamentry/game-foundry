using System;

namespace Evergrove.Core
{
    /// <summary>
    /// Consent-free offline puzzle assistance, not a hidden solver at each tap.
    /// The player asks for three progressively stronger hints: a row, a tile,
    /// then one aligned connection. Selection is deterministic and never targets
    /// an anchored habitat. No time gate or currency.
    /// </summary>
    public static class HintPlanner
    {
        public static int SelectTarget(Puzzle puzzle, int[] rotations)
        {
            if (puzzle == null || !PipeRules.ValidRotations(puzzle, rotations))
                return -1;
            PipeInsight insight = PipeRules.Inspect(puzzle, rotations);
            int fallback = -1;
            for (int i = 0; i < puzzle.CellCount; i++)
            {
                if (puzzle.IsAnchored(i) || rotations[i] == puzzle.Solution[i])
                    continue;
                if (fallback == -1) fallback = i;
                // Prefer a tile at the frontier of already connected water,
                // rather than giving an unrelated solution fragment.
                if (insight.Flowing[i]) return i;
                for (int direction = 0; direction < 4; direction++)
                {
                    int adjacent = PipeRules.Neighbor(i, direction, puzzle.Size);
                    if (adjacent >= 0 && insight.Flowing[adjacent]) return i;
                }
            }
            return fallback;
        }
    }
}
