namespace Evergrove.Core
{
    /// <summary>
    /// A possible choice, evaluated by the real rules on a detached garden.
    /// Inspecting or discarding it cannot change the player's garden. To commit,
    /// the caller must still call Choose on the authoritative WorldModel.
    /// </summary>
    public sealed class VillageChoicePreview
    {
        public VillageChoice Choice { get; }

        /// <summary>
        /// Detached result for the existing scene and localized-story projections.
        /// This is not a saved garden or a replacement for the authoritative world.
        /// </summary>
        public WorldModel WorldAfter { get; }

        /// <summary>The actual next board, including its fixed tile; null at the ending.</summary>
        public Puzzle NextPuzzle { get { return WorldAfter.CurrentPuzzle; } }

        private VillageChoicePreview(VillageChoice choice, WorldModel worldAfter)
        {
            Choice = choice;
            WorldAfter = worldAfter;
        }

        /// <summary>
        /// Only a solved Living World board can offer a choice. Export/restore
        /// preserves migrated board state and isolates every array before the
        /// same Choose operation used by actual play evaluates the consequence.
        /// </summary>
        public static bool TryCreate(WorldModel source, VillageChoice choice,
            out VillageChoicePreview preview)
        {
            preview = null;
            if (source == null || source.Mode != PlayMode.Living ||
                source.Completed || !source.Solved)
                return false;

            WorldModel projected;
            if (!WorldModel.TryRestore(source.Export(), out projected) ||
                !projected.Choose(choice))
                return false;

            preview = new VillageChoicePreview(choice, projected);
            return true;
        }
    }
}
