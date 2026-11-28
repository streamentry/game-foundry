using System;

namespace Evergrove.Core
{
    /// <summary>
    /// Deterministic local deduction over a pipe board. Each tile starts with every
    /// orientation it could show. The protected landmark is pinned to its solution
    /// (when the anchor is enabled), edge ports are forbidden, and neighbours must agree
    /// on each shared edge. Arc consistency then removes orientations with no supporting
    /// neighbour until nothing changes. A rotatable tile reduced to exactly one orientation
    /// is a forced move. Pure: no randomness, no clock, no Unity.
    /// </summary>
    public static class DeductionDepth
    {
        /// <summary>
        /// Number of rotatable, non-anchored tiles that propagation reduces to one orientation.
        /// With useAnchor false the landmark is ignored entirely, so rotated variants of a
        /// chapter can be compared with their base board.
        /// </summary>
        public static int ForcedMoves(Puzzle puzzle, bool useAnchor = true)
        {
            bool[][] candidates = Candidates(puzzle, useAnchor);
            int forced = 0;
            for (int i = 0; i < puzzle.CellCount; i++)
            {
                if (IsCounted(puzzle, i, useAnchor) && CountTrue(candidates[i]) == 1) forced++;
            }
            return forced;
        }

        /// <summary>Tiles whose kind can turn (more than one orientation), excluding the anchor.</summary>
        public static int RotatableTiles(Puzzle puzzle, bool useAnchor = true)
        {
            int rotatable = 0;
            for (int i = 0; i < puzzle.CellCount; i++)
            {
                if (IsCounted(puzzle, i, useAnchor)) rotatable++;
            }
            return rotatable;
        }

        /// <summary>
        /// Remaining orientations per cell after propagation. Index 0 is unused; index r is
        /// true when orientation r (1-based) is still possible for that cell.
        /// </summary>
        public static bool[][] Candidates(Puzzle puzzle, bool useAnchor = true)
        {
            int cells = puzzle.CellCount;
            var candidates = new bool[cells][];
            for (int i = 0; i < cells; i++)
            {
                int orientations = PipeRules.OrientationCount(puzzle.Kinds[i]);
                bool pinned = useAnchor && puzzle.IsAnchored(i);
                candidates[i] = new bool[orientations + 1];
                for (int r = 1; r <= orientations; r++)
                {
                    bool edgeFree = true;
                    for (int d = 0; d < 4 && edgeFree; d++)
                    {
                        if (PipeRules.Neighbor(i, d, puzzle.Size) < 0 &&
                            PipeRules.HasPort(puzzle.Kinds[i], r, d)) edgeFree = false;
                    }
                    candidates[i][r] = edgeFree && (!pinned || r == puzzle.Solution[i]);
                }
            }
            Propagate(puzzle, candidates);
            return candidates;
        }

        private static void Propagate(Puzzle puzzle, bool[][] candidates)
        {
            bool changed = true;
            while (changed)
            {
                changed = false;
                for (int i = 0; i < puzzle.CellCount; i++)
                {
                    for (int r = 1; r < candidates[i].Length; r++)
                    {
                        if (!candidates[i][r] || Supported(puzzle, candidates, i, r)) continue;
                        if (r == puzzle.Solution[i])
                            throw new InvalidOperationException("Deduction removed the true orientation.");
                        candidates[i][r] = false;
                        changed = true;
                    }
                }
            }
        }

        private static bool Supported(Puzzle puzzle, bool[][] candidates, int cell, int rotation)
        {
            for (int d = 0; d < 4; d++)
            {
                int other = PipeRules.Neighbor(cell, d, puzzle.Size);
                if (other < 0) continue;
                bool port = PipeRules.HasPort(puzzle.Kinds[cell], rotation, d);
                bool found = false;
                for (int s = 1; s < candidates[other].Length && !found; s++)
                {
                    if (candidates[other][s] &&
                        PipeRules.HasPort(puzzle.Kinds[other], s, (d + 2) % 4) == port) found = true;
                }
                if (!found) return false;
            }
            return true;
        }

        private static bool IsCounted(Puzzle puzzle, int cell, bool useAnchor)
        {
            if (PipeRules.OrientationCount(puzzle.Kinds[cell]) <= 1) return false;
            return !(useAnchor && puzzle.IsAnchored(cell));
        }

        private static int CountTrue(bool[] values)
        {
            int count = 0;
            for (int r = 1; r < values.Length; r++)
            {
                if (values[r]) count++;
            }
            return count;
        }
    }
}
