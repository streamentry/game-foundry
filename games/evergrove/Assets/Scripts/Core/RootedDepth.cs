using System;

namespace Evergrove.Core
{
    /// <summary>
    /// Deduction depth for root-stone boards (see RootedBoards). Arc consistency works as in
    /// DeductionDepth, with one extra rule: no port may face the board edge or a root stone.
    /// Guessing measures how many trial orientations a solver needs once deduction stalls.
    /// SOLVER GATE ONLY: not wired into the game. Pure: no randomness, no clock, no Unity.
    /// </summary>
    public static class RootedDepth
    {
        /// <summary>
        /// Orientations still possible per cell after propagation. Index 0 is unused; index r is
        /// true when orientation r (1-based) is still possible. Root cells get a length-1 array
        /// with no candidates. Throws if propagation removes the true solution.
        /// </summary>
        public static bool[][] Candidates(RootedBoard b)
        {
            RequireBoard(b);
            bool[][] candidates = InitialCandidates(b);
            Settle(b, candidates, true);
            return candidates;
        }

        /// <summary>Pipe cells (more than one orientation) that propagation reduces to one orientation.</summary>
        public static int ForcedMoves(RootedBoard b)
        {
            bool[][] candidates = Candidates(b);
            int forced = 0;
            for (int i = 0; i < candidates.Length; i++)
            {
                if (!b.Root[i] && PipeRules.OrientationCount(b.Kinds[i]) > 1 &&
                    CountTrue(candidates[i]) == 1) forced++;
            }
            return forced;
        }

        /// <summary>
        /// Trial orientations tried, failed or not, by a deduce-then-guess depth-first search
        /// before it reaches the first full assignment that RootedBoards.IsComplete accepts.
        /// </summary>
        public static int Guesses(RootedBoard b)
        {
            Solve(b, out int guesses);
            return guesses;
        }

        /// <summary>The assignment found by deduce-then-guess search; 0 for root stones.</summary>
        public static int[] SolveByGuessing(RootedBoard b)
        {
            int[] rotations = Solve(b, out _);
            if (rotations == null) throw new InvalidOperationException("Guessing found no complete answer.");
            return rotations;
        }

        private static int[] Solve(RootedBoard b, out int guesses)
        {
            RequireBoard(b);
            guesses = 0;
            return Search(b, InitialCandidates(b), ref guesses);
        }

        // Deduce, then branch on the lowest undecided pipe cell, trying candidates in ascending order.
        private static int[] Search(RootedBoard b, bool[][] candidates, ref int guesses)
        {
            if (!Settle(b, candidates, false)) return null;
            int cell = FirstUndecided(b, candidates);
            if (cell < 0) return AssignmentIfComplete(b, candidates);
            for (int r = 1; r < candidates[cell].Length; r++)
            {
                if (!candidates[cell][r]) continue;
                guesses++;
                bool[][] trial = Copy(candidates);
                Narrow(trial[cell], r);
                int[] found = Search(b, trial, ref guesses);
                if (found != null) return found;
            }
            return null;
        }

        private static bool[][] InitialCandidates(RootedBoard b)
        {
            var candidates = new bool[b.Root.Length][];
            for (int i = 0; i < candidates.Length; i++)
            {
                if (b.Root[i])
                {
                    candidates[i] = new bool[1];
                    continue;
                }
                int orientations = PipeRules.OrientationCount(b.Kinds[i]);
                candidates[i] = new bool[orientations + 1];
                for (int r = 1; r <= orientations; r++)
                    candidates[i][r] = !FacesEdgeOrRoot(b, i, r);
            }
            return candidates;
        }

        private static bool FacesEdgeOrRoot(RootedBoard b, int cell, int rotation)
        {
            for (int d = 0; d < 4; d++)
            {
                if (!PipeRules.HasPort(b.Kinds[cell], rotation, d)) continue;
                int other = PipeRules.Neighbor(cell, d, b.Size);
                if (other < 0 || b.Root[other]) return true;
            }
            return false;
        }

        // Removes unsupported orientations until nothing changes. Returns false when a cell is
        // wiped out. With guardSolution, removing the true orientation throws instead.
        private static bool Settle(RootedBoard b, bool[][] candidates, bool guardSolution)
        {
            bool changed = true;
            while (changed)
            {
                changed = false;
                for (int i = 0; i < candidates.Length; i++)
                {
                    if (b.Root[i]) continue;
                    for (int r = 1; r < candidates[i].Length; r++)
                    {
                        if (!candidates[i][r] || Supported(b, candidates, i, r)) continue;
                        if (guardSolution && r == b.Solution[i])
                            throw new InvalidOperationException("Deduction removed the true orientation.");
                        candidates[i][r] = false;
                        changed = true;
                    }
                    if (CountTrue(candidates[i]) == 0) return false;
                }
            }
            return true;
        }

        // Every pipe neighbour must have a surviving orientation agreeing on the shared edge.
        private static bool Supported(RootedBoard b, bool[][] candidates, int cell, int rotation)
        {
            for (int d = 0; d < 4; d++)
            {
                int other = PipeRules.Neighbor(cell, d, b.Size);
                if (other < 0 || b.Root[other]) continue;
                bool port = PipeRules.HasPort(b.Kinds[cell], rotation, d);
                if (!AnyAgrees(b, candidates[other], other, (d + 2) % 4, port)) return false;
            }
            return true;
        }

        private static bool AnyAgrees(RootedBoard b, bool[] options, int cell, int direction, bool port)
        {
            for (int s = 1; s < options.Length; s++)
            {
                if (options[s] && PipeRules.HasPort(b.Kinds[cell], s, direction) == port) return true;
            }
            return false;
        }

        private static int FirstUndecided(RootedBoard b, bool[][] candidates)
        {
            for (int i = 0; i < candidates.Length; i++)
            {
                if (!b.Root[i] && CountTrue(candidates[i]) > 1) return i;
            }
            return -1;
        }

        private static int[] AssignmentIfComplete(RootedBoard b, bool[][] candidates)
        {
            var rotations = new int[candidates.Length];
            for (int i = 0; i < rotations.Length; i++)
                rotations[i] = b.Root[i] ? 0 : OnlyCandidate(candidates[i]);
            return RootedBoards.IsComplete(b, rotations) ? rotations : null;
        }

        private static bool[][] Copy(bool[][] candidates)
        {
            var copy = new bool[candidates.Length][];
            for (int i = 0; i < candidates.Length; i++) copy[i] = (bool[])candidates[i].Clone();
            return copy;
        }

        private static void Narrow(bool[] options, int rotation)
        {
            for (int r = 0; r < options.Length; r++) options[r] = r == rotation;
        }

        private static int OnlyCandidate(bool[] options)
        {
            for (int r = 1; r < options.Length; r++)
                if (options[r]) return r;
            return 0;
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

        private static void RequireBoard(RootedBoard b)
        {
            if (b == null) throw new ArgumentNullException(nameof(b));
        }
    }
}
