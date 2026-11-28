using System.Collections;
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

        // Vision run 20261128-1801, idea 3: true only while the paint that undo or redo causes is speaking.
        private bool handedBack;

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

        /// <summary>Vision run 20261128-1901, idea 2: how many distinct seats moved on this board so far (0 when untracked).</summary>
        private int MoversNow() { return tracked == null ? 0 : ledger.Movers(tracked.UndoSteps); }

        /// <summary>
        /// Vision run 20261128-1901, idea 3: the token's newest child, with its long-press caption saying what a tap
        /// will do. The caption is read when shown, so it always names the current table size.
        /// </summary>
        private Transform TableRow(Transform top)
        {
            Transform row = top.GetChild(top.childCount - 1);
            LongPressCaption.AttachLive(row.gameObject, "Take turns with someone", () => TakeTurns.TableCaption(players), LongPressCaption.DefaultFont);
            return row;
        }

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
            handedBack = true;
            try { PaintTurn(); }
            finally { handedBack = false; }
        }

        private void SpeakTurn(Transform token)
        {
            // Before any move on this board, the starter is announced ("Second player starts, of three.").
            bool noMoveYet = tracked == null || tracked.UndoSteps == 0;
            string words = together && handedBack
                ? TakeTurns.SpokenBack(turn, players)
                : together && turn != 0 && noMoveYet
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

        // Idea 4: the one running colour fade on the take-turns token, so a second turn stops the first.
        private Coroutine tintFade;

        /// <summary>
        /// Idea 4: the token eases to its next colour. Reduced motion (not gentle) or an inactive token
        /// takes the colour at once, so nothing animates.
        /// </summary>
        private void TintDisc(Image disc, Color target)
        {
            if (!gentle || !isActiveAndEnabled || !disc.isActiveAndEnabled)
            {
                if (tintFade != null) StopCoroutine(tintFade);
                tintFade = null;
                disc.color = target;
                return;
            }
            if (tintFade != null) StopCoroutine(tintFade);
            tintFade = StartCoroutine(FadeTint(disc, disc.color, target));
        }

        private IEnumerator FadeTint(Image disc, Color from, Color to)
        {
            float elapsed = 0f;
            while (elapsed < TurnPalette.FadeSeconds)
            {
                if (disc == null) yield break;
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / TurnPalette.FadeSeconds;
                disc.color = Color.Lerp(from, to, TurnPalette.Ease(t));
                yield return null;
            }
            if (disc != null) disc.color = to;
            tintFade = null;
        }

        /// <summary>Idea 3: the table changed size mid-board. The turn stays with its seat if it still exists;
        /// alone, the history is forgotten. Otherwise the turn is noted at the current undo step.</summary>
        private void ResizeTable()
        {
            turn = TakeTurns.TurnAfterResize(turn, players);
            if (players <= 1 || tracked == null) ledger.Clear();
            else ledger.Record(tracked.UndoSteps, turn);
        }

        // Idea 3: each seat's pip wears its own ink on the card (TurnPalette.PipInk darkens only as far as it must read).
        private static readonly float[] CardFillRgb = { MosseloraTheme.CardFill.r, MosseloraTheme.CardFill.g, MosseloraTheme.CardFill.b };
        private static readonly Color[] PipInks = { PipInk(0), PipInk(1), PipInk(2), PipInk(3) };
        private static Color PipInk(int t) { float[] c = TurnPalette.PipInk(t, CardFillRgb); return new Color(c[0], c[1], c[2], 1f); }

        /// <summary>A seat's pip colour: its own ink, always opaque so every pip holds 3:1 on the card. Whose turn
        /// it is shows by shape (filled, not ringed), never by fading the others.</summary>
        private static Color PipInkColour(int p)
        {
            return PipInks[p % PipInks.Length];
        }

        /// <summary>
        /// Idea 3: a board solved while playing together. The filled pips show everyone lit, the disc
        /// pulses as before, and the turn caption says "Solved together, ...". Nothing repaints after this.
        /// </summary>
        private void TableSolved()
        {
            // Vision run 20261128-1901, idea 2: only a board that more than one person moved on is "solved together".
            if (!PlayedTogether) return;
            if (turnDiscs != null && turnDiscs[0] != null && gentle) BloomPulse.Add(turnDiscs[0].gameObject, 0.2f, 0.3f);
            if (pips != null)
                for (int p = 0; p < pips.Length && p < players; p++)
                {
                    if (!ledger.Moved(p, tracked.UndoSteps)) continue; // only the seats that moved light up
                    pips[p].sprite = MosseloraArt.Rounded(8);
                    pips[p].color = PipInks[p % PipInks.Length];
                    if (gentle) BloomPulse.Add(pips[p].gameObject, 0.2f + 0.06f * p, 0.25f);
                }
            if (turnDiscs == null || turnDiscs.Length == 0 || turnDiscs[0] == null) return;
            Transform token = turnDiscs[0].transform;
            Transform caption = token.Find("Turn caption");
            if (caption == null)
            {
                if (uiFont == null) return; // the card isn't built yet
                Hidden(token, "", uiFont);
                caption = token.GetChild(token.childCount - 1);
                caption.name = "Turn caption";
            }
            Text text = caption.GetComponent<Text>();
            if (text != null) text.text = TakeTurns.SpokenSolved(Players);
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
