using Evergrove.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Evergrove.UnityClient
{
    /// <summary>
    /// Vision run 20261128-0927, idea 5: when turns pass, the take-turns token also says whose turn it is
    /// to a screen reader ("Second player's turn, of three."). One hidden caption per token, reused on every
    /// turn, never a new node per turn. Nothing visible changes.
    /// </summary>
    internal sealed partial class TreePlay
    {
        // Vision run 20261128-1001, idea 3: undo and redo hand the turn back with the board.
        private readonly TurnLedger ledger = new TurnLedger();
        private IBoardHistory tracked;

        // Session memory only, never saved (no PlayerPrefs, no names): the table's size, set by the token,
        // and how many boards were finished while playing together, so the next board's first move rotates.
        private static int tablePlayers = 1;
        private static int boardsTogether;

        /// <summary>A new board opens with the table's size and whoever starts it this time.</summary>
        private static int OpeningTurn() { return TakeTurns.StartingTurn(boardsTogether, tablePlayers); }

        /// <summary>Setting the token to alone ends the table; the next table starts fresh.</summary>
        private void RememberTable() { tablePlayers = players; boardsTogether = 0; }

        /// <summary>A board solved while playing together counts toward the rotation of the next board.</summary>
        private void TableFinished() { if (together) boardsTogether++; }

        /// <summary>The undo row tells its TreePlay which history it steps, when it is built.</summary>
        internal void Track(IBoardHistory history)
        {
            tracked = history;
            ledger.Clear();
            ledger.Record(0, turn); // the board's first turn, which may not be the first player's
        }

        /// <summary>After a move passes the turn, note whose turn it is at this undo step.</summary>
        private void NoteTurn()
        {
            if (together && tracked != null) ledger.Record(tracked.UndoSteps, turn);
        }

        /// <summary>After undo or redo: the turn is whoever's it was at that step. Only while playing together.</summary>
        internal void TurnToStep()
        {
            if (!together || solved || tracked == null) return;
            turn = ledger.At(tracked.UndoSteps) % players;
            PaintTurn();
        }

        private void SpeakTurn(Transform token)
        {
            // Before any move on this board, the starter is announced ("Second player starts, of three.").
            bool noMoveYet = tracked == null || tracked.UndoSteps == 0;
            string words = together && turn != 0 && noMoveYet
                ? TakeTurns.SpokenStart(turn, players)
                : TakeTurns.Spoken(turn, players);
            Transform caption = token.Find("Turn caption");
            if (caption == null)
            {
                if (uiFont == null) return; // the card isn't built yet; the next PaintTurn makes it
                Hidden(token, words, uiFont);
                caption = token.GetChild(token.childCount - 1);
                caption.name = "Turn caption";
            }
            Text text = caption.GetComponent<Text>();
            if (text != null) text.text = words;
            if (hintToken != null) SpeakHint(hintToken.transform); // idea 5: the offer is spoken with the turn
        }

        /// <summary>Idea 3: the token is painted, and a board that opens with someone other than the first
        /// player gives the starter's pip one soft pulse (gentle motion only).</summary>
        private void OpenTurn()
        {
            PaintTurn();
            if (together && turn != 0 && pips != null && turn < pips.Length && gentle)
                BloomPulse.Add(pips[turn].gameObject, 0f, 0.12f);
        }

        /// <summary>Idea 5: the hint token says "Hint offered." while its offer glows. One hidden caption, reused.</summary>
        private void SpeakHint(Transform token)
        {
            Transform caption = token.Find("Hint offer caption");
            if (caption == null)
            {
                if (uiFont == null) return; // the card isn't built yet; the next PaintTurn makes it
                Hidden(token, "", uiFont);
                caption = token.GetChild(token.childCount - 1);
                caption.name = "Hint offer caption";
            }
            Text text = caption.GetComponent<Text>();
            if (text != null) text.text = TakeTurns.SpokenHint(turn, players);
        }
    }
}
