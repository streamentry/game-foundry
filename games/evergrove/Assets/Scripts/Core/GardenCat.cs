namespace Evergrove.Core
{
    /// <summary>
    /// I335 (idea 20, after Japanese pocket pets, without the neglect): a garden cat that naps
    /// beside the thing you most recently tended by day, and by the cottage door after dusk.
    /// It has no needs at all: nothing to feed, nothing that can go wrong, nothing to miss.
    /// </summary>
    public static class GardenCat
    {
        public const int NotHere = -1, ByTheDoor = -2;

        /// <summary>The story thing it naps by, <see cref="ByTheDoor"/> after dusk, or <see cref="NotHere"/>.</summary>
        public static int NapBy(WorldModel world, DayPhase phase)
        {
            if (world == null || world.Mode != PlayMode.Living) return NotHere;
            int newest = world.NewestSolvedThing;
            if (newest < 0) return NotHere;
            return phase == DayPhase.Dusk || phase == DayPhase.Night ? ByTheDoor : newest;
        }
    }

    public sealed partial class WorldModel
    {
        /// <summary>The story thing whose puzzle was solved most recently (solve order is kept), or -1.</summary>
        public int NewestSolvedThing { get { return state.treeSolved.Length > 0 ? state.treeSolved[state.treeSolved.Length - 1] : -1; } }
    }
}
