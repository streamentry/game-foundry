using System.Collections.Generic;

namespace Evergrove.Core
{
    /// <summary>
    /// Vision run 20261128-1001, idea 3: whose turn it was at each undo step, so undo and redo hand
    /// the turn back exactly. A board's history counts recorded moves; not every recorded move passes
    /// the turn (lifting a piece, a reshuffle, a multi-step hint), so the turn is looked up by step
    /// rather than stepped back once per undo. Session-only, like the history itself; nothing is saved.
    /// </summary>
    public sealed class TurnLedger
    {
        // step -> whose turn it was after the move that reached that step.
        private readonly SortedDictionary<int, int> afterStep = new SortedDictionary<int, int>();

        /// <summary>Notes the turn reached at a step. Later steps belong to an abandoned future and are forgotten.</summary>
        public void Record(int step, int turn)
        {
            if (step < 0) step = 0;
            var stale = new List<int>();
            foreach (int key in afterStep.Keys) if (key > step) stale.Add(key);
            foreach (int key in stale) afterStep.Remove(key);
            afterStep[step] = turn;
        }

        /// <summary>The turn at a step: the latest note at or before it; the first player before any note.</summary>
        public int At(int step)
        {
            int turn = 0;
            foreach (KeyValuePair<int, int> note in afterStep)
            {
                if (note.Key > step) break;
                turn = note.Value;
            }
            return turn;
        }

        /// <summary>Forgets everything (the number of players changed, so turns restart with the first player).</summary>
        public void Clear() { afterStep.Clear(); }
    }
}
