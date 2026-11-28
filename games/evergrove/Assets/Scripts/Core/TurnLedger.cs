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

        /// <summary>
        /// Vision run 20261128-1901, idea 2: how many distinct seats actually moved up to a step. Each recorded
        /// move k (1 to step) was made by whoever held the turn at k - 1; key 0 is the opening turn, not a move.
        /// Undone moves (keys above step) don't count. Zero when nothing was recorded; never throws.
        /// </summary>
        public int Movers(int step)
        {
            return MovedSeats(step).Count;
        }

        /// <summary>Whether a seat made at least one of the recorded moves up to a step (same rule as <see cref="Movers"/>).</summary>
        public bool Moved(int seat, int step)
        {
            return MovedSeats(step).Contains(seat);
        }

        private HashSet<int> MovedSeats(int step)
        {
            var seats = new HashSet<int>();
            foreach (KeyValuePair<int, int> note in afterStep)
            {
                if (note.Key < 1) continue;
                if (note.Key > step) break;
                seats.Add(At(note.Key - 1));
            }
            return seats;
        }

        /// <summary>Forgets everything (the number of players changed, so turns restart with the first player).</summary>
        public void Clear() { afterStep.Clear(); }
    }
}
