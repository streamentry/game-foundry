using System;

namespace Evergrove.Core
{
    /// <summary>
    /// I315 (idea 32, after village life games where neighbours give back): at a few true
    /// milestones of puzzles solved, a neighbour leaves a cutting of their own kind in your
    /// basket, from a nursery row you already have open. A few, never endless; earned by
    /// play like any first-solve seeds, so the economy stays logical.
    /// </summary>
    public static class NeighbourGifts
    {
        /// <summary>Puzzles solved at which a cutting comes, in order: Mira, Orin, Fen, Mira.</summary>
        public static readonly int[] Milestones = { 8, 20, 40, 80 };

        private static readonly string[] Givers = { "Mira", "Orin", "Fen" };

        /// <summary>How many gifts are due now, given how many were already given.</summary>
        public static int Due(int puzzlesSolved, int alreadyGiven)
        {
            int reached = 0;
            foreach (int milestone in Milestones) if (puzzlesSolved >= milestone) reached++;
            return Math.Max(0, reached - Math.Max(0, alreadyGiven));
        }

        public static string From(int milestone) { return Givers[((milestone % 3) + 3) % 3]; }

        /// <summary>The cutting for a milestone: the giver's own family, from the lowest open row; -1 if none.</summary>
        public static int CuttingFor(WorldModel world, int milestone)
        {
            if (world == null) return -1;
            var family = (VillageChoice)(((milestone % 3) + 3) % 3);
            int best = -1;
            for (int kind = 0; kind < Nursery.Kinds; kind++)
            {
                if (Nursery.Family(kind) != family || !world.RowOpen(Nursery.Row(kind))) continue;
                // A different plant each time the same neighbour gives: step along their open kinds.
                if (best < 0) best = kind;
                if (milestone >= 3 && kind != best) return kind;
            }
            return best;
        }
    }
}
