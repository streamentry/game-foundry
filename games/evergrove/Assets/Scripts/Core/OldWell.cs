using System;

namespace Evergrove.Core
{
    /// <summary>
    /// I333 (idea 11, after Japanese puzzle-adventure games where puzzles are found in the world):
    /// the old well at the head of the river holds the deepest boards, one for each stretch of
    /// river opened, each a different kind in turn. Solving one pays two fruit, once. Bounded
    /// by progress, so the economy stays logical.
    /// </summary>
    public static class OldWell
    {
        public const int FruitPerBoard = 2;
        public static int Tier { get { return TreePuzzles.Tiers - 1; } }

        /// <summary>Boards waiting in the well: stretches of river open, less those already solved.</summary>
        public static int Waiting(WorldModel world)
        {
            if (world == null || world.Mode != PlayMode.Living) return 0;
            int open = GardenMap.ReachesFor(world.Choices.Length, world.Completed, world.PuzzlesSolved);
            return Math.Max(0, open - world.WellSolved);
        }

        /// <summary>The kind of the well's nth board: every kind, in turn.</summary>
        public static PuzzleKind KindFor(int index)
        {
            int kinds = Enum.GetValues(typeof(PuzzleKind)).Length;
            return (PuzzleKind)(((index % kinds) + kinds) % kinds);
        }

        public static int SeedFor(int garden, int index)
        {
            unchecked { return garden * 100003 + 7919 * (index + 1) + 31; }
        }
    }

    public sealed partial class WorldModel
    {
        /// <summary>The well's boards solved so far (I333).</summary>
        public int WellSolved { get { return state.well; } }

        /// <summary>A well board solved: two fruit, once per board waiting. -1 when none waits.</summary>
        public int TendWell()
        {
            if (Mode != PlayMode.Living || OldWell.Waiting(this) <= 0) return -1;
            state.well++;
            int before = state.fruit;
            for (int i = 0; i < OldWell.FruitPerBoard; i++) PickFruit();
            return state.fruit - before;
        }

        /// <summary>
        /// Fail closed (I363): the well can hold no more solved boards than stretches the
        /// saved progress has opened. Scales with the save, never a fixed count.
        /// </summary>
        private static bool ReadWell(WorldSnapshot source)
        {
            if (source.well < 0 || (source.mode != "living" && source.well != 0)) return false;
            int solved = (source.treeSolved ?? Array.Empty<int>()).Length + (source.boughtSolved ?? Array.Empty<int>()).Length;
            int open = GardenMap.ReachesFor(source.choices.Length, source.round == PuzzleCatalog.Count, solved);
            return source.well <= open;
        }
    }
}
