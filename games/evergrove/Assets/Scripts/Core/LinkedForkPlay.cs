using System;
using System.Collections.Generic;

namespace Evergrove.Core
{
    /// <summary>
    /// Pure play model for a linked-fork board (vision run 20261128-2001, idea 1: not wired into play).
    /// A tap turns one cell a quarter turn clockwise; a tap on an axle end turns both ends together, once each.
    /// Undo reverses the last tap that is not already undone. No clock, no randomness, no Unity.
    /// </summary>
    public sealed class LinkedForkPlay
    {
        private readonly LinkedForkBoard board;
        private readonly int[] masks;
        private readonly List<int> history = new List<int>();

        public LinkedForkPlay(LinkedForkBoard board)
        {
            if (board == null) throw new ArgumentNullException(nameof(board));
            this.board = board;
            masks = (int[])board.Board.Initial.Clone();
        }

        /// <summary>Current port mask per cell (a copy).</summary>
        public int[] Masks => (int[])masks.Clone();

        /// <summary>Taps not yet undone.</summary>
        public int Taps => history.Count;

        /// <summary>True when the current masks form the solved tree (see <see cref="ForkBoards.IsComplete"/>).</summary>
        public bool Solved => ForkBoards.IsComplete(board.Board, masks);

        /// <summary>Turns the cell a quarter turn clockwise, and its axle partner too when it is linked. Always true.</summary>
        public bool Tap(int cell)
        {
            CheckCell(cell);
            Turn(cell, 1);
            history.Add(cell);
            return true;
        }

        /// <summary>Reverses the last tap not already undone. False when there is nothing to undo.</summary>
        public bool Undo()
        {
            if (history.Count == 0) return false;
            int last = history[history.Count - 1];
            history.RemoveAt(history.Count - 1);
            Turn(last, 3);
            return true;
        }

        /// <summary>True when the cell is one of the axle's two ends.</summary>
        public bool IsLinked(int cell)
        {
            CheckCell(cell);
            return cell == board.A || cell == board.B;
        }

        /// <summary>The other end of the axle, or -1 when the cell is not linked.</summary>
        public int Partner(int cell)
        {
            CheckCell(cell);
            if (cell == board.A) return board.B;
            if (cell == board.B) return board.A;
            return -1;
        }

        /// <summary>
        /// Fewest taps that solve the board by hand: for each unlinked cell its fewest clockwise quarter turns
        /// from the start mask to the solution mask, plus the axle's shared turn count k once (the fewest t that
        /// solves both ends). Vision run 20261128-2001, idea 1: not wired into play.
        /// </summary>
        public static int FewestTaps(LinkedForkBoard b)
        {
            if (b == null) throw new ArgumentNullException(nameof(b));
            ForkBoard board = b.Board;
            int total = AxleTurns(b);
            for (int cell = 0; cell < board.CellCount; cell++)
            {
                if (cell == b.A || cell == b.B) continue;
                total += FewestTurns(board.Initial[cell], board.Solution[cell]);
            }
            return total;
        }

        // Turns the cell, and its partner when linked, by the given number of quarter turns.
        private void Turn(int cell, int turns)
        {
            masks[cell] = ForkBoards.Rotate(masks[cell], turns);
            int partner = Partner(cell);
            if (partner >= 0) masks[partner] = ForkBoards.Rotate(masks[partner], turns);
        }

        private void CheckCell(int cell)
        {
            if (cell < 0 || cell >= masks.Length)
                throw new ArgumentOutOfRangeException(nameof(cell));
        }

        // The smallest t in 0..3 that turns the start mask into the solution mask.
        private static int FewestTurns(int from, int to)
        {
            for (int t = 0; t < 4; t++)
                if (ForkBoards.Rotate(from, t) == to) return t;
            throw new InvalidOperationException("No rotation turns the start mask into the solution mask.");
        }

        // The smallest t in 0..3 that turns both axle ends into their solution masks at once.
        private static int AxleTurns(LinkedForkBoard b)
        {
            ForkBoard board = b.Board;
            for (int t = 0; t < 4; t++)
                if (ForkBoards.Rotate(board.Initial[b.A], t) == board.Solution[b.A] &&
                    ForkBoards.Rotate(board.Initial[b.B], t) == board.Solution[b.B]) return t;
            throw new InvalidOperationException("The axle ends share no turn count that solves them.");
        }
    }
}
