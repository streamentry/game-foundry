using System;
using Evergrove.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Evergrove.UnityClient
{
    /// <summary>
    /// I244 — seed sudoku. Each row, column and box holds each plant once. Plants
    /// already growing are set in warm soil and can't move. Tap an empty bed, then a
    /// plant from the tray below (the eraser clears). A plant that repeats glows rose,
    /// a hint, never a penalty. Pictures, not digits: it reads in every language.
    /// </summary>
    internal sealed class SudokuBoard : TreePlay.IPuzzleBoard
    {
        private readonly BoardHistory<SeedSudokuGame> history; // step 39: undo and redo
        private SeedSudokuGame game { get { return history.Current; } }
        private BoardKit.UndoRow undo;
        private readonly Sprite[] plants;
        private TreePlay play;
        private Image[] cells;
        private Image[] faces;
        private Image[] trayDiscs;
        private int selected = -1;
        private bool locked;
        private int shown = -1; // I253
        private Font ruleFont;

        public SudokuBoard(SeedSudokuGame game, Sprite[] plants)
        {
            history = new BoardHistory<SeedSudokuGame>(game, () => new SeedSudokuGame(game.Seed, game.Tier));
            this.plants = plants;
        }

        public void Build(Transform card, float width, float screenHeight, Font font, TreePlay host)
        {
            play = host;
            ruleFont = font;
            int n = game.Size;
            const float trayHeight = 64f;
            float rule = BoardKit.FirstTimeRule(card, "sudoku", row =>
            {
                for (int k = 0; k < Mathf.Min(4, plants != null ? plants.Length : 0); k++)
                    BoardKit.RuleSquare(row, BoardKit.Leafy, plants[k]);
                BoardKit.RuleArrow(row);
                BoardKit.RulePicture(row, MosseloraArt.Load(MosseloraArt.IconConfirm), 28f, MosseloraTheme.ForestAction);
            });
            Vector2 room = BoardKit.Room(width, screenHeight, trayHeight + 12f + rule + BoardKit.UndoRowSpace);
            float cell = Mathf.Floor(Mathf.Min(room.x, room.y) / n);
            cell = Mathf.Min(cell, 72f);
            float boxGap = 6f, gap = 3f;
            int boxesAcross = n / game.BoxColumns, boxesDown = n / game.BoxRows;
            float side = n * cell;
            RectTransform board = BoardKit.Bed(card, "Seed sudoku", new Vector2(side + (boxesAcross - 1) * boxGap,
                side + (boxesDown - 1) * boxGap));
            cells = new Image[n * n];
            faces = new Image[n * n];
            for (int c = 0; c < n * n; c++)
            {
                int index = c, row = c / n, column = c % n;
                Vector2 at = new Vector2(column * cell + (column / game.BoxColumns) * boxGap + gap / 2f,
                    row * cell + (row / game.BoxRows) * boxGap + gap / 2f);
                cells[c] = BoardKit.Cell(board, "Bed row " + (row + 1) + " column " + (column + 1), at,
                    new Vector2(cell - gap, cell - gap), () => Select(index), 8);
                faces[c] = BoardKit.Glyph(cells[c].transform, "Plant", null, cell * 0.1f);
            }
            // The tray: one big token per plant. Tapping the plant a bed already holds clears it,
            // so no separate eraser takes room from the plants (I244 tune: 46 pt tokens, not 40).
            Transform tray = TreePlay.Row(card, "Plant tray", trayHeight);
            tray.GetComponent<HorizontalLayoutGroup>().spacing = 8f;
            float token = Mathf.Floor(Mathf.Min(60f, (width - BoardKit.CardPadding - 8f * (n - 1)) / n));
            trayDiscs = new Image[n + 1];
            for (int v = 1; v <= n; v++)
            {
                int value = v;
                TreePlay.Token(tray, "Plant " + v, plants != null && v - 1 < plants.Length ? plants[v - 1] : null,
                    token, MosseloraTheme.QuietAction, () => Put(value), true);
                Transform icon = tray.GetChild(tray.childCount - 1).Find("Icon");
                if (icon != null) ((RectTransform)icon).sizeDelta = new Vector2(token * 0.82f, token * 0.82f);
                trayDiscs[v] = tray.GetChild(tray.childCount - 1).GetComponent<Image>();
            }
            // Start on the first empty bed, so the first tap on a plant does something.
            for (int c = 0; c < n * n && selected < 0; c++) if (!game.IsGiven(c)) selected = c;
            BoardKit.ClashLens(card.Find("Tree puzzle top"), Paint);
            undo = BoardKit.UndoRow.Build(card, font, history, () => { shown = -1; Paint(); });
            Paint();
        }

        private void Select(int cell)
        {
            if (locked || game.IsGiven(cell)) return;
            selected = cell;
            Paint();
        }

        private void Put(int value)
        {
            if (locked || selected < 0) return;
            if (value > 0 && game.Value(selected) == value) value = 0; // the same plant again lifts it
            // v1 #36: name the technique from the board as the player sees it, before the placement.
            SudokuTechnique.Technique technique = value > 0 && game.Solution(selected) == value
                ? SudokuTechnique.Classify(game, selected, value) : SudokuTechnique.Technique.None;
            int at = selected; // copied now: a replay runs the same move later, after the selection may have moved
            SudokuMove move = history.Play(g => g.Place(at, value), m => m != SudokuMove.Ignored);
            if (move == SudokuMove.Ignored) return;
            play.Moved();
            if (technique != SudokuTechnique.Technique.None && game.Value(selected) == value) NoteTechnique(technique);
            if (value > 0 && game.Conflicts(selected)) BoardKit.Shiver(cells[selected].gameObject);
            // Walk on to the next empty bed: fewer taps for a steady hand or a shaky one.
            if (value > 0)
            {
                int n = game.Size;
                for (int step = 1; step < n * n; step++)
                {
                    int next = (selected + step) % (n * n);
                    if (!game.IsGiven(next) && game.Value(next) == 0) { selected = next; break; }
                }
            }
            Paint();
            if (game.Solved) play.Done();
        }

        // v1 #36: the first time a technique is used on this device, a gentle one-line note above the bed.
        // Only the first sighting shows; the almanac keeps the list afterwards.
        private void NoteTechnique(SudokuTechnique.Technique technique)
        {
            if (!TechniqueLog.MarkSeen(technique) || cells == null || selected < 0) return;
            RuleToast.Show(cells[selected].rectTransform, "Found a way: " + SudokuTechnique.Name(technique) + ".",
                ruleFont, MosseloraTheme.EarthInk, MosseloraTheme.CardFill);
        }

        /// <summary>
        /// Device-only record of which techniques the player has used (PlayerPrefs, like the first-time
        /// rules in BoardKit; not in the save). Read by the almanac.
        /// </summary>
        internal static class TechniqueLog
        {
            private const string KeyPrefix = "evergrove:technique:";

            public static bool Seen(SudokuTechnique.Technique technique)
            {
                if (technique == SudokuTechnique.Technique.None) return false;
                try { return PlayerPrefs.GetInt(KeyPrefix + (int)technique, 0) == 1; }
                catch (Exception) { return false; }
            }

            // True only on the first sighting, when it was recorded.
            public static bool MarkSeen(SudokuTechnique.Technique technique)
            {
                if (technique == SudokuTechnique.Technique.None || Seen(technique)) return false;
                try
                {
                    PlayerPrefs.SetInt(KeyPrefix + (int)technique, 1);
                    PlayerPrefs.Save();
                    return true;
                }
                catch (Exception) { return false; }
            }
        }

        private void Paint()
        {
            int n = game.Size;
            for (int c = 0; c < n * n; c++)
            {
                int value = game.Value(c);
                bool given = game.IsGiven(c);
                bool clash = !given && !(c == selected && !locked) && value > 0 && BoardKit.ShowClashes && game.Conflicts(c);
                cells[c].color = given ? BoardKit.Given
                    : c == selected && !locked ? BoardKit.Picked
                    : clash ? BoardKit.Clash
                    : value > 0 ? BoardKit.Leafy : BoardKit.Soil;
                BoardKit.MarkClash(cells[c], clash);
                cells[c].transform.localScale = c == selected && !locked ? new Vector3(1.05f, 1.05f, 1f) : Vector3.one;
                Sprite plant = value > 0 && plants != null && value - 1 < plants.Length ? plants[value - 1] : null;
                faces[c].sprite = plant;
                faces[c].enabled = plant != null;
            }
            if (undo != null) undo.Refresh(locked);
        }

        public void Hint()
        {
            if (locked) return;
            int cell = game.HintCell();
            if (cell < 0) return;
            if (cell != shown)
            {
                shown = cell;
                selected = cell;
                Paint();
                BloomPulse.Add(cells[cell].gameObject, 0f, 0.18f);
                return;
            }
            shown = -1;
            selected = cell;
            Put(game.Solution(cell));
        }

        public void Lock()
        {
            locked = true;
            Paint();
            if (!play.Gentle) return;
            int n = game.Size;
            for (int c = 0; c < n * n; c++) BloomPulse.Add(cells[c].gameObject, 0.05f + (c / n + c % n) * 0.05f);
        }

        public void QaSolve()
        {
            int n = game.Size;
            for (int c = 0; c < n * n && !game.Solved; c++)
            {
                if (game.IsGiven(c) || game.Value(c) == game.Solution(c)) continue;
                selected = c;
                Put(game.Solution(c));
            }
        }
    }
}
