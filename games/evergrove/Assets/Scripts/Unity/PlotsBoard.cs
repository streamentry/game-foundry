using Evergrove.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Evergrove.UnityClient
{
    /// <summary>
    /// I245 — garden plots (Shikaku). Split the field into rectangular beds: each bed
    /// holds one marker, and the marker's number is how many squares the bed covers.
    /// Tap one corner, then the opposite corner, to stake a bed; tap a staked bed to
    /// lift it. A bed that can't be (two markers, or the wrong size) gives a small
    /// shiver and nothing else. Two taps, no dragging: easy on any hand.
    /// </summary>
    internal sealed class PlotsBoard : TreePlay.IPuzzleBoard
    {
        // Soft bed colours, told apart by tone as well as hue.
        private static readonly Color[] BedTones = {
            new Color(0.78f, 0.89f, 0.70f, 1f), new Color(0.93f, 0.84f, 0.66f, 1f), new Color(0.72f, 0.86f, 0.86f, 1f),
            new Color(0.90f, 0.78f, 0.80f, 1f), new Color(0.84f, 0.82f, 0.92f, 1f), new Color(0.96f, 0.92f, 0.72f, 1f),
        };

        private readonly PlotsGame game;
        private TreePlay play;
        private Image[] cells;
        private int corner = -1;
        private bool locked;
        private int[] shown; // I253: the hinted bed, waiting for a second tap

        public PlotsBoard(PlotsGame game) { this.game = game; }

        public void Build(Transform card, float width, float screenHeight, Font font, TreePlay host)
        {
            play = host;
            int n = game.Size;
            float rule = BoardKit.FirstTimeRule(card, "plots", row =>
            {
                BoardKit.RuleNumber(row, "4", font);
                BoardKit.RuleArrow(row);
                for (int k = 0; k < 4; k++) BoardKit.RuleSquare(row, BedTones[0]);
                BoardKit.RuleNumber(row, "2", font);
                BoardKit.RuleArrow(row);
                for (int k = 0; k < 2; k++) BoardKit.RuleSquare(row, BedTones[1]);
            });
            Vector2 room = BoardKit.Room(width, screenHeight, rule);
            float cell = Mathf.Min(Mathf.Floor(Mathf.Min(room.x, room.y) / n), 64f);
            const float gap = 2f;
            RectTransform board = BoardKit.Bed(card, "Garden plots", new Vector2(n * cell, n * cell));
            int numberSize = Mathf.Clamp(Mathf.RoundToInt(cell * 0.5f), 16, 28);
            cells = new Image[n * n];
            for (int c = 0; c < n * n; c++)
            {
                int index = c;
                cells[c] = BoardKit.Cell(board, "Square row " + (c / n + 1) + " column " + (c % n + 1),
                    new Vector2((c % n) * cell + gap / 2f, (c / n) * cell + gap / 2f), new Vector2(cell - gap, cell - gap),
                    () => Tap(index), 5);
                if (game.Clue(c) > 0)
                    BoardKit.Number(cells[c].transform, "Marker", game.Clue(c).ToString(), font, numberSize,
                        MosseloraTheme.EarthInk);
            }
            Paint();
        }

        private void Tap(int cell)
        {
            if (locked) return;
            if (corner < 0)
            {
                // A staked bed lifts with one tap; anywhere else starts a new bed.
                if (game.BedAt(cell) >= 0 && game.Draw(cell, cell) == PlotMove.Removed) { play.Moved(); Paint(); return; }
                corner = cell;
                Paint();
                return;
            }
            int from = corner;
            corner = -1;
            if (game.Draw(from, cell) == PlotMove.Invalid) { BoardKit.Shiver(cells[cell].gameObject); Paint(); return; }
            play.Moved();
            Paint();
            if (game.Solved) play.Done();
        }

        // Neighbouring beds never share a tone (I245 tune): greedy colouring, steady per marker.
        private int[] Tones()
        {
            int n = game.Size, beds = game.BedCount;
            var tone = new int[beds];
            for (int b = 0; b < beds; b++)
            {
                bool[] taken = new bool[BedTones.Length];
                for (int other = 0; other < b; other++)
                    if (Touch(b, other)) taken[tone[other]] = true;
                int start = (game.BedMarker(b) * 7) % BedTones.Length;
                tone[b] = start;
                for (int k = 0; k < BedTones.Length; k++)
                    if (!taken[(start + k) % BedTones.Length]) { tone[b] = (start + k) % BedTones.Length; break; }
            }
            return tone;
        }

        private bool Touch(int a, int b)
        {
            int al = game.BedLeft(a), at = game.BedTop(a), ar = al + game.BedWidth(a), ab = at + game.BedHeight(a);
            int bl = game.BedLeft(b), bt = game.BedTop(b), br = bl + game.BedWidth(b), bb = bt + game.BedHeight(b);
            bool sideBySide = (ar == bl || br == al) && at < bb && bt < ab;
            bool stacked = (ab == bt || bb == at) && al < br && bl < ar;
            return sideBySide || stacked;
        }

        private void Paint()
        {
            int n = game.Size;
            int[] tones = Tones();
            for (int c = 0; c < n * n; c++)
            {
                int bed = game.BedAt(c);
                cells[c].color = c == corner ? BoardKit.Picked
                    : bed >= 0 ? BedTones[tones[bed]] : BoardKit.Soil;
                cells[c].transform.localScale = c == corner ? new Vector3(1.08f, 1.08f, 1f) : Vector3.one;
            }
        }

        public void Hint()
        {
            if (locked) return;
            int[] bed = game.HintBed();
            if (bed.Length < 4) return;
            int n = game.Size;
            bool same = shown != null && shown[0] == bed[0] && shown[1] == bed[1] && shown[2] == bed[2] && shown[3] == bed[3];
            if (!same)
            {
                shown = bed;
                corner = -1;
                Paint();
                for (int r = bed[1]; r < bed[1] + bed[3]; r++)
                    for (int c = bed[0]; c < bed[0] + bed[2]; c++)
                    {
                        cells[r * n + c].color = BoardKit.Picked;
                        BloomPulse.Add(cells[r * n + c].gameObject, 0f, 0.12f);
                    }
                return;
            }
            shown = null;
            corner = -1;
            // Lift any wrong bed in the way, then stake the right one, as taps would.
            Tap(bed[1] * n + bed[0]);
            if (corner < 0) Tap(bed[1] * n + bed[0]);
            Tap((bed[1] + bed[3] - 1) * n + bed[0] + bed[2] - 1);
        }

        public void Lock()
        {
            locked = true;
            corner = -1;
            Paint();
            if (!play.Gentle) return;
            int n = game.Size;
            for (int c = 0; c < n * n; c++) BloomPulse.Add(cells[c].gameObject, 0.05f + (c / n + c % n) * 0.04f);
        }

        public void QaSolve()
        {
            int n = game.Size;
            for (int guard = 0; guard < n * n && !game.Solved; guard++)
            {
                int[] bed = game.HintBed();
                if (bed.Length < 4) break;
                corner = -1;
                Tap(bed[1] * n + bed[0]);
                Tap((bed[1] + bed[3] - 1) * n + bed[0] + bed[2] - 1);
            }
        }
    }
}
