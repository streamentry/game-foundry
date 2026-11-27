using System;
using Evergrove.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Evergrove.UnityClient
{
    /// <summary>
    /// I204: the puzzle a grown thing holds, played over your garden. A card with
    /// the thing's painting, the seeds it pays, the water board and a close token.
    /// Tapping turns a tile; when every channel joins, the board blooms, the seeds
    /// show, and the card closes by itself. Practice only: the story is untouched.
    /// Kept apart from EvergroveApp's story board on purpose (no hints, undo or
    /// chapter state), so other puzzle kinds can live beside it later.
    /// </summary>
    internal sealed class TreePlay : MonoBehaviour
    {
        public const string SeedIcon = "glyph-sprout";

        private Puzzle puzzle;
        private int[] rotations;
        private Image[] tiles;
        private PipeGraphic[] pipes;
        private Button[] buttons;
        private bool solved;
        private bool gentle;
        private Action onTurn;
        private Func<int> onSolved; // records the solve, returns the seeds earned
        private Action onClose;
        private Text rewardText;
        private GameObject rewardPill;
        private float closeAt = -1f;
        // Pairs (flowers): the table, its pictures and the card images.
        private PairsGame pairs;
        private Sprite[] pictures;
        private Image[] cardFaces;
        private Sprite cardBack;
        // Mosaic (buildings): the scattered painting.
        private MosaicGame mosaic;
        private Sprite painting;
        private RawImage[] pieces;
        // The five newer kinds (I243 on) each draw their own board.
        private IPuzzleBoard board;

        /// <summary>A newer kind's board: built into the card, tells the card when it's solved.</summary>
        internal interface IPuzzleBoard
        {
            void Build(Transform card, float width, float screenHeight, Font font, TreePlay play);
            void Lock();     // solved: no more input, the board blooms
            void QaSolve();  // tour only: solve by the same paths as taps
            void Hint();     // I253: first tap shows the next sensible move, the second makes it
        }

        /// <summary>A newer kind's board in the same card as every other puzzle.</summary>
        public static TreePlay OpenBoard(RectTransform host, IPuzzleBoard board, Sprite art, string name, int reward,
                                         Font font, bool gentleMotion, Action turned, Func<int> solved, Action closed)
        {
            TreePlay play = Open(host, null, null, null, null, null, art, name, reward, font, gentleMotion, turned,
                solved, closed, board);
            return play;
        }

        internal bool Gentle { get { return gentle; } }

        private float ClassicRule(Transform card)
        {
            if (pairs != null)
                return BoardKit.FirstTimeRule(card, "pairs", row =>
                {
                    Sprite face = pictures != null && pictures.Length > 0 ? pictures[0] : null;
                    BoardKit.RuleSquare(row, new Color(0.62f, 0.72f, 0.58f, 1f), MosseloraArt.Load(MosseloraArt.Sprig), 34f);
                    BoardKit.RuleArrow(row);
                    BoardKit.RuleSquare(row, BoardKit.Soil, face, 34f);
                    BoardKit.RuleSquare(row, BoardKit.Soil, face, 34f);
                    BoardKit.RuleArrow(row);
                    BoardKit.RulePicture(row, MosseloraArt.Load(MosseloraArt.IconConfirm), 28f, MosseloraTheme.ForestAction);
                });
            if (mosaic != null)
                return BoardKit.FirstTimeRule(card, "mosaic", row =>
                {
                    BoardKit.RuleSquare(row, BoardKit.Soil);
                    BoardKit.RuleSquare(row, BoardKit.Picked);
                    BoardKit.RuleArrow(row);
                    BoardKit.RuleSquare(row, BoardKit.Picked);
                    BoardKit.RuleSquare(row, BoardKit.Soil);
                    BoardKit.RuleArrow(row);
                    BoardKit.RulePicture(row, painting, 38f);
                });
            return BoardKit.FirstTimeRule(card, "water", row =>
            {
                BoardKit.RulePicture(row, MosseloraArt.Load("glyph-waves"), 30f, MosseloraTheme.WaterAction);
                BoardKit.RuleArrow(row);
                for (int k = 0; k < 3; k++) BoardKit.RuleSquare(row, BoardKit.Water);
                BoardKit.RuleArrow(row);
                BoardKit.RulePicture(row, MosseloraArt.Load(SeedIcon), 30f);
            });
        }

        /// <summary>I261: the board's depth as leaves under the thing (one to three), no words.</summary>
        internal void ShowTier(int tier)
        {
            Transform thing = transform.Find("Tree puzzle card/Tree puzzle top/The thing");
            if (thing == null) return;
            var row = new GameObject("How deep", typeof(RectTransform));
            row.transform.SetParent(thing, false);
            var rect = (RectTransform)row.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(72f, 24f);
            rect.anchoredPosition = new Vector2(0f, -4f);
            // On its own paper pill, so the leaves read against any painting (b287: they hid in the tree).
            var pill = row.AddComponent<Image>();
            MosseloraArt.Round(pill, new Color(0.98f, 0.95f, 0.86f, 0.96f), 12);
            pill.raycastTarget = false;
            Sprite leaf = MosseloraArt.Load(SeedIcon);
            for (int i = 0; i < Tiers; i++)
            {
                var pip = new GameObject("Leaf " + (i + 1), typeof(RectTransform));
                pip.transform.SetParent(row.transform, false);
                var pipRect = (RectTransform)pip.transform;
                pipRect.sizeDelta = new Vector2(18f, 18f);
                pipRect.anchoredPosition = new Vector2((i - 1) * 21f, 0f);
                var image = pip.AddComponent<Image>();
                image.sprite = leaf;
                image.preserveAspect = true;
                image.raycastTarget = false;
                image.color = i <= tier ? Color.white : new Color(0.35f, 0.33f, 0.28f, 0.45f); // dimmed, not gone
            }
            Hidden(transform, "Depth " + (tier + 1) + " of " + Tiers + ".", uiFont);
        }

        private const int Tiers = 3;
        private Font uiFont;
        internal void Moved() { if (!solved) onTurn?.Invoke(); }
        internal void Done() { if (!solved) Finish(); }
        internal void QaHint() { Hint(); }

        // I256: the classic three get the same two-tap hint as the newer kinds.
        private int hintA = -1, hintB = -1;

        private void Hint()
        {
            if (solved) return;
            if (board != null) { board.Hint(); return; }
            int a = -1, b = -1;
            if (pairs != null)
            {
                // Two cards that match, glowing face down: memory stays memory.
                for (int c = 0; c < pairs.Count && a < 0; c++)
                {
                    if (pairs.Kept(c)) continue;
                    for (int d = c + 1; d < pairs.Count; d++)
                        if (!pairs.Kept(d) && pairs.Face(d) == pairs.Face(c)) { a = c; b = d; break; }
                }
            }
            else if (mosaic != null)
            {
                // A tile out of place, and the slot holding its piece.
                for (int slot = 0; slot < mosaic.Count && a < 0; slot++)
                {
                    if (mosaic.InPlace(slot)) continue;
                    for (int s2 = 0; s2 < mosaic.Count; s2++)
                        if (mosaic.At(s2) == slot) { a = slot; b = s2; break; }
                }
            }
            else
            {
                // A tile turned the wrong way.
                for (int i = 0; i < puzzle.CellCount && a < 0; i++)
                    if (rotations[i] != puzzle.Solution[i] && PipeRules.OrientationCount(puzzle.Kinds[i]) > 1) a = i;
            }
            if (a < 0) return;
            if (a != hintA || b != hintB)
            {
                hintA = a;
                hintB = b;
                foreach (int t in new[] { a, b })
                {
                    if (t < 0) continue;
                    tiles[t].color = new Color(0.98f, 0.93f, 0.74f, 1f);
                    BloomPulse.Add(tiles[t].gameObject, 0f, 0.18f);
                }
                return;
            }
            hintA = hintB = -1;
            if (pairs != null) { TurnCard(a); TurnCard(b); return; }
            if (mosaic != null)
            {
                if (mosaic.Picked >= 0) TapPiece(mosaic.Picked); // put down a stray pick
                TapPiece(a);
                TapPiece(b);
                return;
            }
            for (int guard = 0; guard < 4 && rotations[a] != puzzle.Solution[a] && !solved; guard++) Turn(a);
        }

        public static TreePlay Open(RectTransform host, Puzzle puzzle, Sprite art, string name, int reward,
                                    Font font, bool gentleMotion, Action turned, Func<int> solved, Action closed)
        {
            return Open(host, puzzle, null, null, null, null, art, name, reward, font, gentleMotion, turned, solved, closed, null);
        }

        /// <summary>The Pairs table a flower holds: same card, same rewards, cards instead of channels.</summary>
        public static TreePlay OpenPairs(RectTransform host, PairsGame game, Sprite[] faces, Sprite art, string name,
                                         int reward, Font font, bool gentleMotion, Action turned, Func<int> solved,
                                         Action closed)
        {
            return Open(host, null, game, faces, null, null, art, name, reward, font, gentleMotion, turned, solved, closed, null);
        }

        /// <summary>The Mosaic a building holds: a memory painting in scattered tiles, swapped home.</summary>
        public static TreePlay OpenMosaic(RectTransform host, MosaicGame game, Sprite painting, Sprite art, string name,
                                          int reward, Font font, bool gentleMotion, Action turned, Func<int> solved,
                                          Action closed)
        {
            return Open(host, null, null, null, game, painting, art, name, reward, font, gentleMotion, turned, solved, closed, null);
        }

        private static TreePlay Open(RectTransform host, Puzzle puzzle, PairsGame game, Sprite[] faces,
                                     MosaicGame mosaicGame, Sprite painting, Sprite art, string name, int reward,
                                     Font font, bool gentleMotion, Action turned, Func<int> solved, Action closed,
                                     IPuzzleBoard newerBoard)
        {
            var root = new GameObject("Tree puzzle", typeof(RectTransform));
            root.transform.SetParent(host, false);
            var rect = (RectTransform)root.transform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            var veil = root.AddComponent<Image>();
            veil.color = new Color(0.08f, 0.14f, 0.11f, 0.38f); // the garden stays in view, dimmed
            var play = root.AddComponent<TreePlay>();
            play.puzzle = puzzle;
            play.rotations = puzzle != null ? (int[])puzzle.Initial.Clone() : null;
            play.pairs = game;
            play.pictures = faces;
            play.mosaic = mosaicGame;
            play.board = newerBoard;
            play.painting = painting;
            play.gentle = gentleMotion;
            play.onTurn = turned;
            play.onSolved = solved;
            play.onClose = closed;
            play.Build(rect, art, name, reward, font);
            return play;
        }

        private void Build(RectTransform root, Sprite art, string name, int reward, Font font)
        {
            uiFont = font;
            var card = new GameObject("Tree puzzle card", typeof(RectTransform));
            card.transform.SetParent(root, false);
            var cardRect = (RectTransform)card.transform;
            cardRect.anchorMin = cardRect.anchorMax = new Vector2(0.5f, 0.5f);
            cardRect.pivot = new Vector2(0.5f, 0.5f);
            var cardImage = card.AddComponent<Image>();
            MosseloraArt.Round(cardImage, MosseloraTheme.CardFill, 26);
            MosseloraArt.Lift(cardImage, 4f, 0.22f);
            var column = card.AddComponent<VerticalLayoutGroup>();
            column.padding = new RectOffset(18, 18, 16, 18);
            column.spacing = 12;
            column.childAlignment = TextAnchor.UpperCenter;
            column.childControlWidth = true;
            column.childControlHeight = true;
            column.childForceExpandWidth = true;
            column.childForceExpandHeight = false;
            card.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // Width: the screen less a gutter, never wider than a comfortable card.
            float screenWidth = root.rect.width > 1f ? root.rect.width : 390f;
            float screenHeight = root.rect.height > 1f ? root.rect.height : 844f;
            // The card is about its width plus 100 pt tall: sideways, the height decides (I204).
            float width = Mathf.Min(screenWidth - 28f, 440f, screenHeight - 112f);
            cardRect.sizeDelta = new Vector2(width, 0f);

            // Top row: close token, the thing itself, the seeds it pays.
            var top = Row(card.transform, "Tree puzzle top", 84f);
            Token(top, "Close tree puzzle", MosseloraArt.Load(MosseloraArt.IconCancel), 56f, MosseloraTheme.QuietAction,
                Close, true);
            var thing = new GameObject("The thing", typeof(RectTransform));
            thing.transform.SetParent(top, false);
            var thingLayout = thing.AddComponent<LayoutElement>();
            thingLayout.flexibleWidth = 1f;
            thingLayout.preferredHeight = 84f;
            var thingImage = thing.AddComponent<Image>();
            thingImage.sprite = art;
            thingImage.preserveAspect = true;
            thingImage.raycastTarget = false;
            if (gentle) BloomPulse.Add(thing, 0.2f, 0.05f);
            // A hint on every kind (I253, I256): shown, then made, never a penalty.
            Token(top, "Hint: show the next move", MosseloraArt.Load(MosseloraArt.IconHint), 52f,
                MosseloraTheme.QuietAction, Hint, true);
            rewardPill = Pill(top, "+" + reward, font, out rewardText);
            // Helping family (I227) pays nothing: no seed pill, said plainly.
            if (reward < 0) rewardPill.SetActive(false);
            // Spoken: what this is and what it pays (screen readers read Text).
            Hidden(card.transform, name + (reward < 0 ? ". Help with it for them." : ". " + reward + " seeds."), font);

            if (board != null) { board.Build(card.transform, width, screenHeight, font, this); return; }
            // The classic three get their picture rule too (I262), the first few times.
            float rule = ClassicRule(card.transform);
            screenHeight -= rule;
            if (pairs != null) { BuildPairs(card.transform, width, screenHeight); return; }
            if (mosaic != null) { BuildMosaic(card.transform, width, screenHeight); return; }
            // The board: the same paper tiles and channels as the streams.
            int size = puzzle.Size;
            float boardWidth = width - 36f;
            float gap = 8f;
            float cell = Mathf.Floor((boardWidth - gap * (size - 1) - 16f) / size);
            var bed = new GameObject("Tree puzzle board", typeof(RectTransform));
            bed.transform.SetParent(card.transform, false);
            var bedLayout = bed.AddComponent<LayoutElement>();
            bedLayout.preferredHeight = bedLayout.minHeight = cell * size + gap * (size - 1) + 16f;
            var bedImage = bed.AddComponent<Image>();
            MosseloraArt.Round(bedImage, new Color(0.55f, 0.5f, 0.36f, 0.55f), 18);
            bedImage.raycastTarget = false;
            var grid = bed.AddComponent<GridLayoutGroup>();
            grid.padding = new RectOffset(8, 8, 8, 8);
            grid.cellSize = new Vector2(cell, cell);
            grid.spacing = new Vector2(gap, gap);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = size;
            grid.childAlignment = TextAnchor.MiddleCenter;
            tiles = new Image[puzzle.CellCount];
            pipes = new PipeGraphic[puzzle.CellCount];
            buttons = new Button[puzzle.CellCount];
            for (int i = 0; i < puzzle.CellCount; i++)
            {
                int index = i;
                var tile = new GameObject("Row " + (i / size + 1) + " column " + (i % size + 1) + ", tap to turn",
                    typeof(RectTransform));
                tile.transform.SetParent(bed.transform, false);
                var image = tile.AddComponent<Image>();
                MosseloraArt.PaperTile(image, 12);
                MosseloraArt.Lift(image, 2f, 0.14f);
                MosseloraArt.Edge(tile.transform, MosseloraTheme.TileEdge, 12);
                var button = tile.AddComponent<Button>();
                button.targetGraphic = image;
                button.transition = Selectable.Transition.None;
                button.onClick.AddListener(() => Turn(index));
                PressSquash.Add(tile);
                var channel = new GameObject("Visible channel", typeof(RectTransform));
                channel.transform.SetParent(tile.transform, false);
                var channelRect = (RectTransform)channel.transform;
                channelRect.anchorMin = Vector2.zero;
                channelRect.anchorMax = Vector2.one;
                channelRect.offsetMin = channelRect.offsetMax = Vector2.zero;
                var pipe = channel.AddComponent<PipeGraphic>();
                pipe.raycastTarget = false;
                tiles[i] = image;
                pipes[i] = pipe;
                buttons[i] = button;
            }
            Paint();
        }

        private void Turn(int index)
        {
            if (solved || index < 0 || index >= rotations.Length) return;
            rotations[index] = rotations[index] % PipeRules.OrientationCount(puzzle.Kinds[index]) + 1;
            onTurn?.Invoke();
            Paint();
            if (!PipeRules.IsComplete(puzzle, rotations)) return;
            Finish();
        }

        // Solved, whatever the kind: record it, show the seeds, bloom, close soon.
        private void Finish()
        {
            solved = true;
            int earned = onSolved != null ? onSolved() : 0;
            rewardText.text = "+" + earned;
            if (board != null)
            {
                board.Lock();
                if (gentle && rewardPill.activeSelf) BloomPulse.Add(rewardPill, 0.3f, 0.25f);
                closeAt = Time.unscaledTime + 2.2f;
                return;
            }
            if (pairs != null || mosaic != null)
            {
                if (mosaic != null) PaintMosaic();
                for (int i = 0; i < tiles.Length; i++)
                {
                    if (buttons[i] != null) buttons[i].interactable = false;
                    if (gentle) BloomPulse.Add(tiles[i].gameObject, 0.08f + i * 0.03f);
                }
                if (gentle) BloomPulse.Add(rewardPill, 0.3f, 0.25f);
                closeAt = Time.unscaledTime + 2.2f;
                return;
            }
            Paint();
            int spring = Array.IndexOf(puzzle.Kinds, PipeKind.End);
            for (int i = 0; i < tiles.Length; i++)
            {
                buttons[i].interactable = false;
                int steps = Mathf.Abs(i / puzzle.Size - spring / puzzle.Size) + Mathf.Abs(i % puzzle.Size - spring % puzzle.Size);
                if (gentle) BloomPulse.Add(tiles[i].gameObject, 0.08f + steps * 0.07f);
            }
            if (gentle) BloomPulse.Add(rewardPill, 0.3f, 0.25f);
            closeAt = Time.unscaledTime + 2.2f; // long enough to see the water run and the seeds
        }

        // ---- Pairs: a 4-wide table of painted cards, face down on paper.
        private void BuildPairs(Transform card, float width, float screenHeight)
        {
            int columns = 4, rows = pairs.Count / columns;
            float gap = 8f;
            float cell = Mathf.Floor((width - 36f - gap * (columns - 1) - 16f) / columns);
            float tall = Mathf.Floor(cell * 1.18f); // cards stand a little taller than wide
            // Sideways the height decides: the whole table must fit under the top row (I207).
            float roomy = Mathf.Floor((screenHeight - 176f - gap * (rows - 1) - 16f) / rows);
            if (tall > roomy) { tall = roomy; cell = Mathf.Floor(roomy / 1.18f); }
            var bed = new GameObject("Pairs table", typeof(RectTransform));
            bed.transform.SetParent(card, false);
            var bedLayout = bed.AddComponent<LayoutElement>();
            bedLayout.preferredHeight = bedLayout.minHeight = tall * rows + gap * (rows - 1) + 16f;
            var bedImage = bed.AddComponent<Image>();
            MosseloraArt.Round(bedImage, new Color(0.55f, 0.5f, 0.36f, 0.55f), 18);
            bedImage.raycastTarget = false;
            var grid = bed.AddComponent<GridLayoutGroup>();
            grid.padding = new RectOffset(8, 8, 8, 8);
            grid.cellSize = new Vector2(cell, tall);
            grid.spacing = new Vector2(gap, gap);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = columns;
            grid.childAlignment = TextAnchor.MiddleCenter;
            cardBack = MosseloraArt.Load(MosseloraArt.Sprig);
            tiles = new Image[pairs.Count];
            cardFaces = new Image[pairs.Count];
            buttons = new Button[pairs.Count];
            for (int i = 0; i < pairs.Count; i++)
            {
                int index = i;
                var node = new GameObject("Card " + (i + 1) + ", tap to turn over", typeof(RectTransform));
                node.transform.SetParent(bed.transform, false);
                var image = node.AddComponent<Image>();
                MosseloraArt.PaperTile(image, 12);
                MosseloraArt.Lift(image, 2f, 0.14f);
                MosseloraArt.Edge(node.transform, MosseloraTheme.TileEdge, 12);
                var button = node.AddComponent<Button>();
                button.targetGraphic = image;
                button.transition = Selectable.Transition.None;
                button.onClick.AddListener(() => TurnCard(index));
                PressSquash.Add(node);
                var face = new GameObject("Picture", typeof(RectTransform));
                face.transform.SetParent(node.transform, false);
                var faceRect = (RectTransform)face.transform;
                faceRect.anchorMin = Vector2.zero;
                faceRect.anchorMax = Vector2.one;
                faceRect.offsetMin = new Vector2(6f, 6f);
                faceRect.offsetMax = new Vector2(-6f, -6f);
                var faceImage = face.AddComponent<Image>();
                faceImage.preserveAspect = true;
                faceImage.raycastTarget = false;
                tiles[i] = image;
                cardFaces[i] = faceImage;
                buttons[i] = button;
            }
            PaintPairs();
        }

        private void TurnCard(int index)
        {
            if (solved) return;
            PairsGame.Turn turn = pairs.Flip(index);
            if (turn == PairsGame.Turn.Ignored) return;
            onTurn?.Invoke();
            PaintPairs();
            if (turn == PairsGame.Turn.Match && gentle)
                for (int i = 0; i < pairs.Count; i++)
                    if (pairs.Kept(i) && pairs.Face(i) == pairs.Face(index)) BloomPulse.Add(tiles[i].gameObject, 0.02f, 0.1f);
            if (pairs.Solved) Finish();
        }

        private void PaintPairs()
        {
            for (int i = 0; i < pairs.Count; i++)
            {
                bool up = pairs.Showing(i);
                int picture = pairs.Face(i);
                tiles[i].color = up
                    ? (pairs.Kept(i) ? new Color(0.86f, 0.94f, 0.9f, 1f) : MosseloraTheme.CardFill)
                    : new Color(0.62f, 0.72f, 0.58f, 1f); // a sage back, the same for every card
                cardFaces[i].sprite = up && pictures != null && picture < pictures.Length ? pictures[picture] : cardBack;
                cardFaces[i].color = up ? Color.white : new Color(1f, 1f, 1f, 0.55f);
            }
        }

        // ---- Mosaic: the painting's central square, cut into Size×Size tiles.
        private Rect square; // the painting's central square, in texture UVs

        private void BuildMosaic(Transform card, float width, float screenHeight)
        {
            int size = mosaic.Size;
            float gap = 3f; // tight seams: the picture should read as one painting
            float cell = Mathf.Floor((width - 36f - gap * (size - 1) - 12f) / size);
            cell = Mathf.Min(cell, Mathf.Floor((screenHeight - 176f - gap * (size - 1) - 12f) / size));
            var bed = new GameObject("Mosaic", typeof(RectTransform));
            bed.transform.SetParent(card, false);
            var bedLayout = bed.AddComponent<LayoutElement>();
            bedLayout.preferredHeight = bedLayout.minHeight = cell * size + gap * (size - 1) + 12f;
            var bedImage = bed.AddComponent<Image>();
            MosseloraArt.Round(bedImage, new Color(0.45f, 0.38f, 0.26f, 0.7f), 14);
            bedImage.raycastTarget = false;
            var grid = bed.AddComponent<GridLayoutGroup>();
            grid.padding = new RectOffset(6, 6, 6, 6);
            grid.cellSize = new Vector2(cell, cell);
            grid.spacing = new Vector2(gap, gap);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = size;
            grid.childAlignment = TextAnchor.MiddleCenter;
            Texture texture = painting != null ? painting.texture : null;
            if (painting != null)
            {
                Rect r = painting.textureRect;
                float side = Mathf.Min(r.width, r.height);
                float tw = painting.texture.width, th = painting.texture.height;
                square = new Rect((r.x + (r.width - side) / 2f) / tw, (r.y + (r.height - side) / 2f) / th, side / tw, side / th);
            }
            tiles = new Image[mosaic.Count];
            pieces = new RawImage[mosaic.Count];
            buttons = new Button[mosaic.Count];
            for (int i = 0; i < mosaic.Count; i++)
            {
                int slot = i;
                var node = new GameObject("Tile " + (i + 1) + ", tap two to swap", typeof(RectTransform));
                node.transform.SetParent(bed.transform, false);
                var frame = node.AddComponent<Image>();
                frame.sprite = MosseloraArt.Rounded(6);
                frame.type = Image.Type.Sliced;
                var button = node.AddComponent<Button>();
                button.targetGraphic = frame;
                button.transition = Selectable.Transition.None;
                button.onClick.AddListener(() => TapPiece(slot));
                PressSquash.Add(node);
                var pictureNode = new GameObject("Piece", typeof(RectTransform));
                pictureNode.transform.SetParent(node.transform, false);
                var pictureRect = (RectTransform)pictureNode.transform;
                pictureRect.anchorMin = Vector2.zero;
                pictureRect.anchorMax = Vector2.one;
                pictureRect.offsetMin = new Vector2(2f, 2f);
                pictureRect.offsetMax = new Vector2(-2f, -2f);
                var piece = pictureNode.AddComponent<RawImage>();
                piece.texture = texture;
                piece.raycastTarget = false;
                tiles[i] = frame;
                pieces[i] = piece;
                buttons[i] = button;
            }
            PaintMosaic();
        }

        private void TapPiece(int slot)
        {
            if (solved) return;
            MosaicGame.Step step = mosaic.Select(slot);
            if (step == MosaicGame.Step.Ignored) return;
            onTurn?.Invoke();
            PaintMosaic();
            if (step == MosaicGame.Step.Swapped && gentle)
                for (int i = 0; i < mosaic.Count; i++)
                    if (mosaic.InPlace(i) && (i == slot)) BloomPulse.Add(tiles[i].gameObject, 0.02f, 0.08f);
            if (mosaic.Solved) Finish();
        }

        private void PaintMosaic()
        {
            int size = mosaic.Size;
            for (int i = 0; i < mosaic.Count; i++)
            {
                int piece = mosaic.At(i);
                int column = piece % size, row = piece / size; // row 0 is the painting's top
                float w = square.width / size, h = square.height / size;
                pieces[i].uvRect = new Rect(square.x + column * w, square.y + (size - 1 - row) * h, w, h);
                bool home = mosaic.InPlace(i);
                tiles[i].color = i == mosaic.Picked ? MosseloraTheme.WaterAction
                    : home ? new Color(0.98f, 0.93f, 0.74f, 1f) : new Color(0.93f, 0.91f, 0.86f, 1f);
                tiles[i].transform.localScale = i == mosaic.Picked ? new Vector3(1.06f, 1.06f, 1f) : Vector3.one;
            }
        }

        /// <summary>Tour only: every tile but one in place, then the last turn by the same path as a tap.</summary>
        internal void QaSolve()
        {
            if (solved) return;
            if (board != null) { board.QaSolve(); return; }
            if (pairs != null)
            {
                // Turn the pairs up by memory, perfectly.
                for (int c = 0; c < pairs.Count && !pairs.Solved; c++)
                {
                    if (pairs.Kept(c)) continue;
                    for (int d = c + 1; d < pairs.Count; d++)
                        if (!pairs.Kept(d) && pairs.Face(d) == pairs.Face(c)) { TurnCard(c); TurnCard(d); break; }
                }
                return;
            }
            if (mosaic != null)
            {
                // Bring each piece home in one swap, as a careful player would.
                for (int slot = 0; slot < mosaic.Count && !mosaic.Solved; slot++)
                {
                    if (mosaic.InPlace(slot)) continue;
                    for (int s2 = 0; s2 < mosaic.Count; s2++)
                        if (mosaic.At(s2) == slot) { TapPiece(slot); TapPiece(s2); break; }
                }
                return;
            }
            int last = puzzle.CellCount - 1;
            rotations = (int[])puzzle.Solution.Clone();
            int turns = PipeRules.OrientationCount(puzzle.Kinds[last]);
            rotations[last] = (puzzle.Solution[last] - 2 + turns) % turns + 1; // one turn short
            Turn(last);
        }

        private void Update()
        {
            if (closeAt > 0f && Time.unscaledTime >= closeAt) Close();
        }

        private void Paint()
        {
            PipeInsight insight = PipeRules.Inspect(puzzle, rotations);
            for (int i = 0; i < tiles.Length; i++)
            {
                tiles[i].color = MosseloraTheme.TileFill(solved, insight.Flowing[i], false, 0, -1, i, puzzle.Size);
                pipes[i].Configure(puzzle.Kinds[i], rotations[i], insight.Flowing[i], solved, OpenPorts(i));
            }
        }

        private int OpenPorts(int i)
        {
            int open = 0;
            for (int d = 0; d < 4; d++)
            {
                if (!PipeRules.HasPort(puzzle.Kinds[i], rotations[i], d)) continue;
                int j = PipeRules.Neighbor(i, d, puzzle.Size);
                if (j < 0 || !PipeRules.HasPort(puzzle.Kinds[j], rotations[j], (d + 2) % 4)) open++;
            }
            return open;
        }

        private void Close()
        {
            closeAt = -1f;
            Action closed = onClose;
            onClose = null;
            Destroy(gameObject);
            closed?.Invoke();
        }

        // ---- small builders
        internal static Transform Row(Transform parent, string name, float height)
        {
            var row = new GameObject(name, typeof(RectTransform));
            row.transform.SetParent(parent, false);
            row.AddComponent<LayoutElement>().preferredHeight = height;
            var group = row.AddComponent<HorizontalLayoutGroup>();
            group.spacing = 10;
            group.childAlignment = TextAnchor.MiddleCenter;
            group.childControlWidth = true;
            group.childControlHeight = true;
            group.childForceExpandWidth = false;
            group.childForceExpandHeight = false;
            return row.transform;
        }

        internal static void Token(Transform parent, string name, Sprite glyph, float size, Color tone, Action action,
                                  bool glyphOnly)
        {
            var node = new GameObject(name, typeof(RectTransform));
            node.transform.SetParent(parent, false);
            var layout = node.AddComponent<LayoutElement>();
            layout.minWidth = layout.preferredWidth = size;
            layout.minHeight = layout.preferredHeight = size;
            var image = node.AddComponent<Image>();
            MosseloraArt.Round(image, tone, Mathf.RoundToInt(size / 2f));
            MosseloraArt.Lift(image, 2f, 0.2f);
            var button = node.AddComponent<Button>();
            button.targetGraphic = image;
            button.onClick.AddListener(() => action());
            PressSquash.Add(node);
            if (glyph == null) return;
            var icon = new GameObject("Icon", typeof(RectTransform));
            icon.transform.SetParent(node.transform, false);
            var iconRect = (RectTransform)icon.transform;
            iconRect.anchorMin = iconRect.anchorMax = new Vector2(0.5f, 0.5f);
            iconRect.sizeDelta = new Vector2(size * 0.5f, size * 0.5f);
            var iconImage = icon.AddComponent<Image>();
            iconImage.sprite = glyph;
            iconImage.preserveAspect = true;
            iconImage.raycastTarget = false;
            iconImage.color = Color.white;
        }

        internal static GameObject Pill(Transform parent, string words, Font font, out Text label)
        {
            var pill = new GameObject("Seeds it pays", typeof(RectTransform));
            pill.transform.SetParent(parent, false);
            var layout = pill.AddComponent<LayoutElement>();
            layout.minWidth = layout.preferredWidth = 96f;
            layout.minHeight = layout.preferredHeight = 44f;
            var image = pill.AddComponent<Image>();
            MosseloraArt.Round(image, new Color(0.93f, 0.88f, 0.72f, 1f), 22);
            image.raycastTarget = false;
            var row = pill.AddComponent<HorizontalLayoutGroup>();
            row.padding = new RectOffset(8, 10, 4, 4);
            row.spacing = 4;
            row.childAlignment = TextAnchor.MiddleCenter;
            row.childControlWidth = true;
            row.childControlHeight = true;
            row.childForceExpandWidth = false;
            row.childForceExpandHeight = false;
            var icon = new GameObject("Seed", typeof(RectTransform));
            icon.transform.SetParent(pill.transform, false);
            var iconLayout = icon.AddComponent<LayoutElement>();
            iconLayout.preferredWidth = iconLayout.preferredHeight = 28f;
            var iconImage = icon.AddComponent<Image>();
            iconImage.sprite = MosseloraArt.Load(SeedIcon);
            iconImage.preserveAspect = true;
            iconImage.raycastTarget = false;
            var text = new GameObject("Seed count", typeof(RectTransform));
            text.transform.SetParent(pill.transform, false);
            label = text.AddComponent<Text>();
            label.font = font;
            label.fontSize = 20;
            label.fontStyle = FontStyle.Bold;
            label.color = MosseloraTheme.EarthInk;
            label.alignment = TextAnchor.MiddleLeft;
            label.horizontalOverflow = HorizontalWrapMode.Overflow; // "+20" wrapped to "+2" (I204)
            label.raycastTarget = false;
            label.text = words;
            return pill;
        }

        private static void Hidden(Transform parent, string words, Font font)
        {
            var node = new GameObject("Caption", typeof(RectTransform));
            node.transform.SetParent(parent, false);
            node.AddComponent<LayoutElement>().ignoreLayout = true;
            var text = node.AddComponent<Text>();
            text.font = font;
            text.color = new Color(0f, 0f, 0f, 0f);
            text.raycastTarget = false;
            text.text = words;
        }
    }
}
