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

        /// <summary>The undo row tells its TreePlay which history it steps, when it is built.</summary>
        internal void Track(IBoardHistory history) { tracked = history; ledger.Clear(); }

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
            string words = TakeTurns.Spoken(turn, players);
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
        }
    }
}
