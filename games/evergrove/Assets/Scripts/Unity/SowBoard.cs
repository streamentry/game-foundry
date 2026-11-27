using Evergrove.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Evergrove.UnityClient
{
    /// <summary>
    /// I318 (idea 8, after the mancala family of Africa and Southeast Asia, played here as a
    /// solitaire): a row of beds with the basket at the near end. A bed is ripe when it holds
    /// as many seeds as its distance from the basket; tap it and its seeds are sown, one per
    /// bed toward the basket, the last falling in. Clear every seed into the basket. Ripe beds
    /// show a soft green; a board with no ripe bed left starts again by itself, gently.
    /// </summary>
    internal sealed class SowBoard : TreePlay.IPuzzleBoard
    {
        private readonly BoardHistory<SowingGame> history; // step 39: undo and redo
        private SowingGame game { get { return history.Current; } }
        private BoardKit.UndoRow undo;
        private readonly int seed, tier;
        private TreePlay play;
        private Image[] beds;           // index 1..Beds
        private Image[][] seedDots;
        private Text basketCount;
        private Image basket;
        private bool locked;
        private int shown = -1;
        private const int MaxShown = 8;

        public SowBoard(int seed, int tier)
        {
            this.seed = seed;
            this.tier = tier;
            history = new BoardHistory<SowingGame>(new SowingGame(seed, tier), () => new SowingGame(seed, tier));
        }

        public void Build(Transform card, float width, float screenHeight, Font font, TreePlay host)
        {
            play = host;
            Sprite sprout = MosseloraArt.Load(TreePlay.SeedIcon);
            Sprite basketArt = MosseloraArt.Load("food-basket");
            float rule = BoardKit.FirstTimeRule(card, "sowing", row =>
            {
                // "A bed two from the basket, with two seeds, is ripe": its seeds go into the basket.
                BoardKit.RuleSquare(row, BoardKit.Leafy, sprout, 34f);
                BoardKit.RuleNumber(row, "2", font);
                BoardKit.RuleArrow(row);
                BoardKit.RulePicture(row, basketArt, 34f);
            });
            Vector2 room = BoardKit.Room(width, screenHeight, rule + BoardKit.UndoRowSpace);
            int n = game.Beds;
            float cell = Mathf.Min(Mathf.Floor(room.x / (n + 1)), 64f);
            float tall = Mathf.Min(cell * 2.4f, room.y - 30f);
            const float gap = 5f;
            RectTransform board = BoardKit.Bed(card, "Sowing beds", new Vector2((n + 1) * cell, tall + 30f));
            beds = new Image[n + 1];
            seedDots = new Image[n + 1][];
            for (int b = 1; b <= n; b++)
            {
                int bed = b;
                float x = (n - b) * cell; // bed 1 sits next to the basket, on the right
                beds[b] = BoardKit.Cell(board, "Bed " + b + " from the basket", new Vector2(x + gap / 2f, gap / 2f),
                    new Vector2(cell - gap, tall - gap), () => Tap(bed), 12);
                seedDots[b] = new Image[MaxShown];
                for (int s = 0; s < MaxShown; s++)
                {
                    var dot = new GameObject("Seed", typeof(RectTransform));
                    dot.transform.SetParent(beds[b].transform, false);
                    var rect = (RectTransform)dot.transform;
                    rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0f);
                    float d = Mathf.Min((cell - gap) * 0.42f, (tall - gap) / 5.2f);
                    rect.sizeDelta = new Vector2(d, d);
                    rect.anchoredPosition = new Vector2((s % 2 == 0 ? -1f : 1f) * d * 0.52f, d * 0.7f + (s / 2) * d * 1.02f);
                    seedDots[b][s] = dot.AddComponent<Image>();
                    seedDots[b][s].sprite = sprout;
                    seedDots[b][s].preserveAspect = true;
                    seedDots[b][s].raycastTarget = false;
                }
                // Its distance from the basket, under it: how many seeds make it ripe.
                BoardKit.Label(board, "Distance " + b, b.ToString(), new Vector2(x, tall + 2f), new Vector2(cell, 26f), font, 18,
                    MosseloraTheme.EarthInk, TextAnchor.MiddleCenter);
            }
            basket = BoardKit.Cell(board, "The basket", new Vector2(n * cell + gap / 2f, gap / 2f),
                new Vector2(cell - gap, tall - gap), null, 12);
            basket.color = BoardKit.Given;
            BoardKit.Glyph(basket.transform, "Basket", basketArt, cell * 0.08f);
            basketCount = BoardKit.Label(board, "Seeds in the basket", "0", new Vector2(n * cell, tall + 2f),
                new Vector2(cell, 26f), font, 18, MosseloraTheme.ForestAction, TextAnchor.MiddleCenter);
            undo = BoardKit.UndoRow.Build(card, font, history, () => { shown = -1; Paint(); });
            Paint();
        }

        private void Tap(int bed)
        {
            if (locked) return;
            SowMove move = history.Play(g => g.Sow(bed), m => m == SowMove.Sowed);
            if (move == SowMove.NotRipe || move == SowMove.Empty) { BoardKit.Shiver(beds[bed].gameObject); return; }
            if (move != SowMove.Sowed) return;
            shown = -1;
            play.Moved();
            Paint();
            if (game.Solved) { play.Done(); return; }
            if (game.Hint() < 0) StartAgain();
        }

        // No ripe bed and seeds still out: the order went astray. The beds shiver, then the
        // seeds return to where they began. Never a penalty, never a count of tries.
        private void StartAgain()
        {
            for (int b = 1; b < beds.Length; b++) BoardKit.Shiver(beds[b].gameObject);
            history.Reset(); // the board starts again from its seed: its steps go with it
            Paint();
        }

        private void Paint()
        {
            for (int b = 1; b < beds.Length; b++)
            {
                int count = game.Seeds(b);
                beds[b].color = b == shown ? BoardKit.Picked : !locked && game.IsRipe(b) ? BoardKit.Leafy : BoardKit.Soil;
                for (int s = 0; s < MaxShown; s++) seedDots[b][s].enabled = s < count;
            }
            basketCount.text = game.InBasket + " / " + game.Total;
            if (undo != null) undo.Refresh(locked);
        }

        public void Hint()
        {
            if (locked) return;
            int next = game.Hint();
            if (next < 0) { StartAgain(); return; }
            if (next != shown)
            {
                shown = next;
                Paint();
                BloomPulse.Add(beds[next].gameObject, 0f, 0.18f);
                return;
            }
            Tap(next);
        }

        public void Lock()
        {
            locked = true;
            Paint();
            if (play.Gentle) BloomPulse.Add(basket.gameObject, 0.05f, 0.25f);
        }

        public void QaSolve()
        {
            for (int guard = 0; guard < 200 && !game.Solved; guard++)
            {
                int next = game.Hint();
                if (next < 0) break;
                Tap(next);
            }
        }
    }
}
