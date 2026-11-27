using System;
using System.Collections.Generic;

namespace Evergrove.Core
{
    /// <summary>
    /// Session-only undo and redo for one puzzle board (step 39, undo everywhere). Nothing
    /// here is saved: a board reopened from the menu starts with an empty history, as it
    /// always has.
    /// </summary>
    /// <remarks>
    /// Every board is dealt from (seed, tier) and changes only through its public moves, so
    /// the history keeps the moves themselves and undo rebuilds the board from its start and
    /// replays all but the last move. That restores every public and private counter exactly,
    /// with no per-kind copy code to fall out of date. Redo replays the undone move on the
    /// live board. A new move clears redo. <see cref="Reset"/> starts the board over and
    /// forgets its steps (used when a board restarts by itself).
    /// </remarks>
    public interface IBoardHistory
    {
        /// <summary>Steps that Undo can take back.</summary>
        int UndoSteps { get; }
        /// <summary>Steps that Redo can put back.</summary>
        int RedoSteps { get; }
        bool Undo();
        bool Redo();
    }

    public sealed class BoardHistory<TGame> : IBoardHistory where TGame : class
    {
        private readonly Func<TGame> fresh;
        // Each entry runs one recorded move on a board and reports whether it changed the board.
        private readonly List<Func<TGame, bool>> past = new List<Func<TGame, bool>>();
        private readonly List<Func<TGame, bool>> future = new List<Func<TGame, bool>>();

        /// <param name="initial">The board as the caller holds it now (the history starts here).</param>
        /// <param name="fresh">Builds the same board from its seed, before any move.</param>
        public BoardHistory(TGame initial, Func<TGame> fresh)
        {
            if (initial == null || fresh == null) throw new ArgumentNullException(initial == null ? nameof(initial) : nameof(fresh));
            this.fresh = fresh;
            Current = initial;
        }

        /// <summary>The live board. Read it after every move; undo replaces it.</summary>
        public TGame Current { get; private set; }

        public int UndoSteps { get { return past.Count; } }
        public int RedoSteps { get { return future.Count; } }
        public bool CanUndo { get { return past.Count > 0; } }
        public bool CanRedo { get { return future.Count > 0; } }

        /// <summary>
        /// Plays one move on the live board and returns what the board's own method returned.
        /// <paramref name="changed"/> says whether that result changed the board; only those
        /// moves are recorded, so a refused tap never becomes an undo step.
        /// </summary>
        public T Play<T>(Func<TGame, T> move, Func<T, bool> changed)
        {
            if (move == null || changed == null) throw new ArgumentNullException(move == null ? nameof(move) : nameof(changed));
            T result = move(Current);
            if (!changed(result)) return result;
            past.Add(board => changed(move(board)));
            future.Clear();
            return result;
        }

        /// <summary>Takes back the last recorded move. Rebuilds the board and replays the rest.</summary>
        public bool Undo()
        {
            if (past.Count == 0) return false;
            int last = past.Count - 1;
            Func<TGame, bool> undone = past[last];
            past.RemoveAt(last);
            future.Add(undone);
            Current = fresh();
            foreach (Func<TGame, bool> move in past) move(Current);
            return true;
        }

        /// <summary>Puts back the last undone move on the live board.</summary>
        public bool Redo()
        {
            if (future.Count == 0) return false;
            int last = future.Count - 1;
            Func<TGame, bool> redone = future[last];
            future.RemoveAt(last);
            redone(Current);
            past.Add(redone);
            return true;
        }

        /// <summary>Starts the board over from its seed and forgets every step.</summary>
        public void Reset()
        {
            Current = fresh();
            past.Clear();
            future.Clear();
        }
    }
}
