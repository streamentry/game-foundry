using System;

namespace Evergrove.Core
{
    /// <summary>
    /// Neighbour life arcs (idea 83): Orin builds a boat over a season and Fen mends the mill.
    /// Each arc is a fixed list of short steps. A step is reached by public progress the save
    /// already records (puzzles solved, WorldModel.PuzzlesSolved), never by the clock, so a
    /// neighbour's project moves forward only as the player plays. The last step is final: it
    /// stays once reached and never resets. Lines are gentle and make no health, loneliness or
    /// memory claims.
    /// Villager indices follow LocalizedVillage: 0 Mira, 1 Orin, 2 Fen.
    /// </summary>
    public static class NeighbourArcs
    {
        private const int Orin = 1;
        private const int Fen = 2;

        // Steps in order. Step i begins once progress reaches Reached[i]; Reached[0] is always 0.
        private static readonly int[] OrinReached = { 0, 3, 6, 10, 15, 21 };
        private static readonly string[] OrinLines =
        {
            "Orin is gathering timber for a small boat.",
            "Orin has the keel laid along the riverbank.",
            "Orin is shaping the hull, one plank at a time.",
            "Orin has the hull sealed and the mast raised.",
            "Orin is fitting the sail and the oars.",
            "The boat is in the water.",
        };

        private static readonly int[] FenReached = { 0, 4, 8, 12, 17, 23 };
        private static readonly string[] FenLines =
        {
            "Fen has found the old mill wheel in the reeds.",
            "Fen is clearing the stones around the mill.",
            "Fen has the wheel turning slowly again.",
            "Fen is mending the sluice gate by the river.",
            "Fen has painted the mill door a soft green.",
            "The mill is turning again.",
        };

        /// <summary>True when the villager has a life arc.</summary>
        public static bool HasArc(int villager)
        {
            return villager == Orin || villager == Fen;
        }

        /// <summary>Number of steps in the arc, or 0 when there is none.</summary>
        public static int StepCount(int villager)
        {
            if (villager == Orin) return OrinLines.Length;
            if (villager == Fen) return FenLines.Length;
            return 0;
        }

        /// <summary>The progress value at which a step begins, or -1 when there is no arc.</summary>
        public static int ReachedAt(int villager, int step)
        {
            if (!Table(villager, out int[] reached, out _)) return -1;
            return reached[step];
        }

        /// <summary>Current step index for the given progress, or -1 when there is no arc.</summary>
        public static int StepAt(int villager, int progress)
        {
            if (!Table(villager, out int[] reached, out _)) return -1;
            int step = 0;
            for (int i = 1; i < reached.Length; i++)
                if (progress >= reached[i]) step = i;
            return step;
        }

        /// <summary>The line for the given progress, or null when there is no arc.</summary>
        public static string LineAt(int villager, int progress)
        {
            int step = StepAt(villager, progress);
            if (step < 0) return null;
            Table(villager, out _, out string[] lines);
            return lines[step];
        }

        /// <summary>Current step index for a living save, or -1 when there is nothing to show.</summary>
        public static int Step(WorldModel world, int villager)
        {
            if (world == null || world.Mode != PlayMode.Living) return -1;
            return StepAt(villager, world.PuzzlesSolved);
        }

        /// <summary>The current arc line for a living save, or null when there is none.</summary>
        public static string Line(WorldModel world, int villager)
        {
            if (world == null || world.Mode != PlayMode.Living) return null;
            return LineAt(villager, world.PuzzlesSolved);
        }

        private static bool Table(int villager, out int[] reached, out string[] lines)
        {
            if (villager == Orin) { reached = OrinReached; lines = OrinLines; return true; }
            if (villager == Fen) { reached = FenReached; lines = FenLines; return true; }
            reached = null;
            lines = null;
            return false;
        }
    }
}
