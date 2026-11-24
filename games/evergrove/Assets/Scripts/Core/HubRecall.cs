namespace Evergrove.Core
{
    /// <summary>
    /// Which remembered choice the hub recalls on this launch (I107). The newest
    /// memory comes first; after that, each launch steps back to an earlier one
    /// and the cycle wraps to the newest. Driven by launches and real saved
    /// choices only: never the clock, never a streak, never "you were away".
    /// </summary>
    public static class HubRecall
    {
        /// <param name="memoryCount">How many choices the garden remembers now.</param>
        /// <param name="lastCount">How many it remembered when the hub last recalled one.</param>
        /// <param name="lastIndex">The index recalled last time, or -1.</param>
        /// <returns>The choice index to recall, or -1 when there is none.</returns>
        public static int Next(int memoryCount, int lastCount, int lastIndex)
        {
            if (memoryCount <= 0) return -1;
            int newest = memoryCount - 1;
            // Something new happened since last time, or the stored state is stale.
            if (memoryCount != lastCount || lastIndex < 0 || lastIndex > newest) return newest;
            return lastIndex == 0 ? newest : lastIndex - 1;
        }
    }
}
