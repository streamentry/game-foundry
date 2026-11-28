using System;
using System.Collections;
using System.Globalization;
using Evergrove.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif
namespace Evergrove.UnityClient
{
    public sealed partial class EvergroveApp
    {

        private void BuildPuzzle()
        {
            Puzzle puzzle = world.CurrentPuzzle;
            PipeInsight insight = world.Insight;
            StreamStones(puzzle);
            if (world.Round == 0)
            {
                // A speech bubble pointing at the board, like a game's first-run
                // tip; it leaves after the first move (RefreshBoard).
                if (!world.Solved && !world.CanUndo) CoachBubble(GameLocalization.Text("first"));
                else if (spentTipHeight > 0f && !landscape)
                {
                    // Any later rebuild of the first board (the solve, the menu, text
                    // size) keeps the spent tip's space, so the board never jumps; the
                    // solve's own glide is then the only movement (I141).
                    var space = Node("Spent tip space", content);
                    MinHeight(space, spentTipHeight);
                }
            }
            else spentTipHeight = 0f;
            // Later boards open on play and the place alone: the generic line that
            // stood over every board ("A little water can change a place.") said
            // nothing about this board, so it is gone (I133).
            // The anchor line sits directly above the board: it explains the framed tile.
            // Spoken, not shown: the framed tile carries the painted landmark that
            // roots it, and tapping it shows the rule in a toast (TapTile).
            if (puzzle.AnchoredTile >= 0)
            {
                Text rule = Label(content, "Protected habitat", LocalizedVillage.EnvironmentalRule(world),
                    14, 0, new Color(0f, 0f, 0f, 0f), TextAnchor.MiddleCenter);
                rule.GetComponent<LayoutElement>().ignoreLayout = true;
            }
            // The board sits on the page itself; each tile is its own parchment square.
            var board = Card(content, "The water puzzle", new Color(1f, 1f, 1f, 0f));
            solvedBoard = board.GetComponent<RectTransform>();
            // The tiles sit in a painted garden bed of mossy stones (Codex art).
            Sprite bed = MosseloraArt.Load(MosseloraArt.BoardBed);
            RectTransform bedFrame = null;
            if (bed != null)
            {
                var bedNode = Node("Garden bed", board.transform);
                bedNode.AddComponent<LayoutElement>().ignoreLayout = true;
                var bedRect = bedNode.GetComponent<RectTransform>();
                Fill(bedRect);
                bedRect.offsetMin = new Vector2(-10f, -10f);
                bedRect.offsetMax = new Vector2(10f, 10f);
                bedFrame = bedRect;
                var bedImage = bedNode.AddComponent<Image>();
                bedImage.sprite = bed;
                bedImage.raycastTarget = false;
                bedNode.transform.SetAsFirstSibling();
                // The board sits in the landscape (I110): this season's painted creek,
                // dry until the stream is solved and flowing after, softly behind the
                // stone bed. Tiles stay opaque parchment, so the puzzle reads the same.
                // Living world only; the Puzzle-only control keeps a plain page.
                if (world.Mode == PlayMode.Living && !landscape && !worldShownNow) CreekBehindBoard(board.transform);
            }
            var gridNode = Node("Water connections", board.transform);
            // Keep the entire puzzle inside a compact phone, including the
            // story card's horizontal padding and safe-area insets.
            int gap = puzzle.Size == 3 ? 7 : 5;
            // CanvasScaler.scaleFactor is only the constant-pixel field (always 1
            // here); the live ScaleWithScreenSize factor gives true logical width.
            float layoutWidth = Screen.safeArea.width / LiveCanvasScale() - columnLeft - columnRight;
            // Standard keeps the EG-007 sizing exactly. Large (EG-112) trims the
            // card's side padding and lifts the tile cap so the board uses the width.
            float available = Mathf.Max(225f, layoutWidth - 34f - 20f - 5f);
            // Pure and tested (BoardFit, I118): portrait by width; sideways by the
            // height left over, with the tool rail beside an unsolved board (I114-I116).
            int standardTile = landscape
                ? BoardFit.Landscape(Screen.safeArea.height / LiveCanvasScale(), available,
                    puzzle.Size, gap, world.Solved, landscapeTablet ? BoardFit.TabletCap : 0)
                : BoardFit.Portrait(available, puzzle.Size, gap);
            int tileSize = standardTile;
            if (BoardSize.IsLarge)
            {
                var boardPadding = board.GetComponent<VerticalLayoutGroup>();
                boardPadding.padding = new RectOffset(6, 6, 14, 14);
                float largeAvailable = Mathf.Max(225f, layoutWidth - 34f - 12f - 2f);
                tileSize = Mathf.Clamp(
                    Mathf.FloorToInt((largeAvailable - (puzzle.Size - 1) * gap) / puzzle.Size),
                    48, LargeTileCap);
            }
            // Sideways the screen height is the limit for every board size (I116).
            if (landscape) tileSize = Mathf.Min(tileSize, standardTile);
            // Markers, badges and endpoint icons scale with the tile; Standard is 1.
            float markerScale = tileSize / (float)standardTile;
            int span = (tileSize + gap) * puzzle.Size - gap;
            Height(gridNode, span + 1);
            // Sideways the stone bed hugs the tiles, not the whole column (I116);
            // while unsolved the board steps left to make room for the tool rail.
            float railShift = landscape && !world.Solved ? ToolRailWidth + ToolRailGap : 0f;
            if (railShift > 0f)
            {
                var boardLayout = board.GetComponent<VerticalLayoutGroup>();
                boardLayout.padding = new RectOffset(boardLayout.padding.left,
                    boardLayout.padding.right + (int)railShift, boardLayout.padding.top, boardLayout.padding.bottom);
            }
            if (landscape && bedFrame != null)
            {
                bedFrame.anchorMin = new Vector2(0.5f, 0f);
                bedFrame.anchorMax = new Vector2(0.5f, 1f);
                bedFrame.sizeDelta = new Vector2(span + 48f, 20f);
                bedFrame.anchoredPosition = new Vector2(-railShift / 2f, 0f);
            }
            var grid = gridNode.AddComponent<GridLayoutGroup>();
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = puzzle.Size;
            grid.childAlignment = TextAnchor.MiddleCenter;
            grid.cellSize = new Vector2(tileSize, tileSize);
            grid.spacing = new Vector2(gap, gap);

            int[] rotations = world.Rotations;
            visibleTiles = new Button[puzzle.CellCount];
            tileBackgrounds = new Image[puzzle.CellCount];
            tileArtwork = new PipeGraphic[puzzle.CellCount];
            visibleLeaks = new Text[puzzle.CellCount];
            int seenEnds = 0;
            RectTransform gardenTile = null;
            // Before the very first move, one tile that needs turning wears a small
            // turning-arrow mark: "tap turns a tile", shown rather than told (I138).
            int firstTurn = world.Round == 0 && !world.CanUndo && !world.Solved
                ? HintPlanner.SelectTarget(puzzle, rotations) : -1;
            int[] flowSteps = celebrateSolve ? FlowOrder.StepsFromSpring(puzzle, rotations) : null;
            for (int index = 0; index < puzzle.CellCount; index++)
            {
                int tappedIndex = index;
                var tileNode = Node("Water tile", gridNode.transform);
                var image = tileNode.AddComponent<Image>();
                bool reached = insight.Flowing[index];
                MosseloraArt.PaperTile(image, 12);
                image.color = MosseloraTheme.TileFill(world.Solved, reached,
                    puzzle.IsAnchored(index), world.HintStep, world.HintTile,
                    index, puzzle.Size);
                MosseloraArt.Lift(image, 2f, 0.14f);
                MosseloraArt.Edge(tileNode.transform, MosseloraTheme.TileEdge, 12);
                var tile = tileNode.AddComponent<Button>();
                string bareCaption = GameLocalization.Display(puzzle.IsAnchored(index) ? "This tile stays where it is." : "Turn this tile");
                // English only: the caption adds which way the tile opens now, read when it is shown (vision run 1401).
                if (GameLocalization.Current == "en")
                    LongPressCaption.AttachLive(tileNode, bareCaption, () => world == null || world.Completed || world.Rotations.Length != puzzle.CellCount ? null
                        : TileOpenings.Caption(puzzle.IsAnchored(tappedIndex), puzzle.Kinds[tappedIndex], world.Rotations[tappedIndex], LandmarkCause.Specific(world)), font);
                else LongPressCaption.Attach(tileNode, bareCaption, font);
                tile.targetGraphic = image;
                // Protected tiles stay tappable: a tap explains them without a move.
                tile.interactable = !world.Solved;
                ColorBlock colors = PressTint();
                // A solved board is a finished picture, not a greyed-out control.
                colors.disabledColor = Color.white;
                tile.colors = colors;
                tile.onClick.AddListener(() => TapTile(tappedIndex));
                if (!world.Solved) PressSquash.Add(tileNode);
                // A new stream's tiles settle onto the bed one by one (first view only).
                bool settle = gentleMotion && !world.Solved && settledRound != world.Round;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                if (qaInMemoryOnly) settle = false; // tour screenshots need settled boards
#endif
                if (settle) SettleIn.Add(tileNode, 0.03f + index * 0.035f);
                else if (celebrateSolve)
                {
                    // The finished creek blooms along the stream the player made, tile by tile.
                    int steps = flowSteps != null && flowSteps[index] >= 0 ? flowSteps[index] : 0;
                    if (gentleMotion) BloomPulse.Add(tileNode, 0.08f + steps * 0.07f);
                }
                visibleTiles[index] = tile;
                tileBackgrounds[index] = image;

                string kind = puzzle.Kinds[index] == PipeKind.End ?
                    (++seenEnds == 1 ? "spring" : "outlet") : "pipe";
                tileNode.name = "Row " + (index / puzzle.Size + 1) + " column " +
                    (index % puzzle.Size + 1) + " " + kind +
                    (puzzle.IsAnchored(index) ? ", protected landmark" : ", tap to turn");
                // An anchored landmark is always visually distinct, even under hints.
                if (puzzle.IsAnchored(index))
                {
                    // Created before the channel so arms and endpoints draw on top.
                    var landmarkNode = Node("Protected landmark marker", tileNode.transform);
                    Fill(landmarkNode.GetComponent<RectTransform>());
                    landmarkNode.AddComponent<LandmarkGraphic>()
                        .Configure(Mathf.Clamp(world.BoardVariant, 1, 3));
                    // The thing that roots it, painted, over the badge: tree, bridge or flowers.
                    int rootedFamily = Mathf.Clamp(world.BoardVariant, 1, 3) - 1;
                    Sprite holder = world.Round > 0 ? ItemArt(world.Round - 1, rootedFamily)
                        : MosseloraArt.ChoiceIcon((VillageChoice)rootedFamily);
                    if (holder != null)
                        MosseloraArt.Ornament(landmarkNode.transform, "Rooted by", holder, new Vector2(0f, 1f),
                            new Vector2(0f, 1f), new Vector2(0.46f, 0.46f) * tileSize, new Vector2(2f, -2f));
                }
                var pipeNode = Node("Visible channel", tileNode.transform);
                Fill(pipeNode.GetComponent<RectTransform>());
                var pipe = pipeNode.AddComponent<PipeGraphic>();
                pipe.Configure(puzzle.Kinds[index], rotations[index],
                    insight.Flowing[index], world.Solved, OpenPorts(puzzle, rotations, index));
                pipe.raycastTarget = false;
                tileArtwork[index] = pipe;
                // A hollow circle denotes an open water join. Keep the
                // non-color cue; avoid alarming red error punctuation.
                var leakNode = Node("Unmatched water port marker", tileNode.transform);
                RectTransform leakRect = leakNode.GetComponent<RectTransform>();
                leakRect.anchorMin = Vector2.zero;
                leakRect.anchorMax = Vector2.zero;
                leakRect.pivot = Vector2.zero;
                leakRect.sizeDelta = new Vector2(23f, 23f) * markerScale;
                leakRect.anchoredPosition = new Vector2(3f, 3f) * markerScale;
                var leakText = leakNode.AddComponent<Text>();
                leakText.font = font;
                leakText.fontSize = Mathf.RoundToInt(23 * markerScale);
                leakText.fontStyle = FontStyle.Bold;
                leakText.alignment = TextAnchor.MiddleCenter;
                leakText.color = MosseloraTheme.EarthInk;
                leakText.raycastTarget = false;
                leakText.text = insight.Leaking[index] ? "○" : "";
                // The visible mark is now the open mouth drawn on the pipe end;
                // this "○" stays as the semantic source, invisible.
                leakText.color = new Color(0f, 0f, 0f, 0f);
                visibleLeaks[index] = leakText;
                // Hint step 1: soft honey rings along the row to look at; the
                // tiles keep their water colours (I91).
                if (world.HintStep == 1 && world.HintTile >= 0 && index / puzzle.Size == world.HintTile / puzzle.Size)
                {
                    var rowNode = Node("Hint row marker", tileNode.transform);
                    rowNode.AddComponent<LayoutElement>().ignoreLayout = true;
                    RectTransform rowRect = rowNode.GetComponent<RectTransform>();
                    Fill(rowRect);
                    rowRect.offsetMin = new Vector2(-3f, -3f);
                    rowRect.offsetMax = new Vector2(3f, 3f);
                    var rowRing = rowNode.AddComponent<Image>();
                    rowRing.sprite = MosseloraArt.Ring(15);
                    rowRing.type = Image.Type.Sliced;
                    rowRing.pixelsPerUnitMultiplier = 1.25f; // still thinner than the step-2 ring
                    rowRing.color = new Color(0.89f, 0.63f, 0.18f, 1f); // a cold-read missed the 75% rings (I92)
                    rowRing.raycastTarget = false;
                    hintMarkers.Add(rowNode);
                }
                if (index == hintTurnedTile && tileArtwork != null && index < tileArtwork.Length && tileArtwork[index] != null)
                {
                    if (gentleMotion) BloomPulse.Add(tileArtwork[index].gameObject, 0.05f, 0.22f);
                    hintTurnedTile = -1;
                }
                if (world.HintStep == 2 && index == world.HintTile)
                {
                    // A soft honey glow around the tile to turn: it breathes when
                    // gentle motion is on, and holds still when it is off.
                    var clueNode = Node("Hint focus marker", tileNode.transform);
                    hintMarkers.Add(clueNode);
                    clueNode.AddComponent<LayoutElement>().ignoreLayout = true;
                    RectTransform clueRect = clueNode.GetComponent<RectTransform>();
                    Fill(clueRect);
                    clueRect.offsetMin = new Vector2(-5f, -5f);
                    clueRect.offsetMax = new Vector2(5f, 5f);
                    var glow = clueNode.AddComponent<Image>();
                    glow.sprite = MosseloraArt.Ring(15);
                    glow.type = Image.Type.Sliced;
                    glow.pixelsPerUnitMultiplier = 0.9f; // bolder than the step-1 row rings
                    glow.color = new Color(0.89f, 0.63f, 0.18f, 1f);
                    glow.raycastTarget = false;
                    if (gentleMotion) Breathe.Add(glow);
                }
                bool hintedHere = world.HintStep > 0 && world.HintTile >= 0
                    && index / puzzle.Size == world.HintTile / puzzle.Size
                    && (world.HintStep == 1 || index == world.HintTile);
                // A hint ring and the teal goal glow never share a tile: the hint wins (I91).
                // Every board, not only the first: a cold read of a later board could
                // not tell what finishing it meant (I148). Gone after the first move.
                if (!hintedHere && puzzle.Kinds[index] == PipeKind.End && !world.CanUndo && !world.Solved)
                {
                    // Before the first move, the start and the goal glow softly.
                    var goal = Node("Start or goal glow", tileNode.transform);
                    goalGlows.Add(goal);
                    goal.AddComponent<LayoutElement>().ignoreLayout = true;
                    var goalRect = goal.GetComponent<RectTransform>();
                    Fill(goalRect);
                    goalRect.offsetMin = new Vector2(-4f, -4f);
                    goalRect.offsetMax = new Vector2(4f, 4f);
                    var ring = goal.AddComponent<Image>();
                    ring.sprite = MosseloraArt.Ring(15);
                    ring.type = Image.Type.Sliced;
                    ring.pixelsPerUnitMultiplier = 1.2f;
                    ring.color = MosseloraTheme.WaterChannel;
                    ring.raycastTarget = false;
                    if (gentleMotion) Breathe.Add(ring);
                }
                if (index == firstTurn) TurnMark(tileNode.transform, tileSize);
                if (puzzle.Kinds[index] == PipeKind.End)
                {
                    // Visible endpoint semantics instead of relying only on
                    // screen-reader names or a color difference.
                    bool spring = seenEnds == 1;
                    if (!spring) gardenTile = tileNode.GetComponent<RectTransform>();
                    var markerNode = Node(spring ? "Water spring waves" : "Garden sprout",
                        tileNode.transform);
                    RectTransform markerRect = markerNode.GetComponent<RectTransform>();
                    markerRect.anchorMin = new Vector2(0f, 1f);
                    markerRect.anchorMax = new Vector2(0f, 1f);
                    markerRect.pivot = new Vector2(0f, 1f);
                    markerRect.sizeDelta = new Vector2(30f, 30f) * markerScale;
                    markerRect.anchoredPosition = new Vector2(4f, -4f) * markerScale;
                    Sprite glyph = MosseloraArt.Load(spring ? MosseloraArt.SpringWaves : MosseloraArt.Sprout);
                    if (glyph != null)
                    {
                        var glyphImage = markerNode.AddComponent<Image>();
                        glyphImage.sprite = glyph;
                        glyphImage.preserveAspect = true;
                        glyphImage.raycastTarget = false;
                    }
                    else
                        DrawEndpointIcon(markerNode.transform, spring, markerScale);
                }
            }

            if (celebrateSolve && gentleMotion) ButterflyFlight.Launch(gardenTile, board.transform);
            settledRound = world.Round;
            celebrateSolve = false;
            string feedback = world.Solved
                ? GameLocalization.Text("solved")
                : GameLocalization.Text("progress", insight.ConnectedCount, puzzle.CellCount, insight.LeakCount);
            WaterMeter(puzzle.CellCount, world.Solved ? puzzle.CellCount : insight.ConnectedCount);
            lastFlowing = (bool[])insight.Flowing.Clone();
            // Spoken, not shown: the droplets and the open mouths already show
            // reach and leaks, and the protected-habitat line explains landmarks.
            Transform meterRow = waterDrops != null ? content.GetChild(content.childCount - 1) : content;
            feedbackText = Label(meterRow, "Water feedback", feedback, 17, 0, new Color(0f, 0f, 0f, 0f),
                TextAnchor.MiddleCenter);
            feedbackText.GetComponent<LayoutElement>().ignoreLayout = true;
            Fill(feedbackText.rectTransform);

            if (world.Solved)
            {
                // Both puzzle-only and matched runs have no village decisions;
                // only the Living world may show choices.
                if (world.Mode != PlayMode.Living)
                    IconButton(content, "Next stream", GameLocalization.Text("next"),
                        MosseloraArt.IconContinue, () => {
                            if (!world.NextPuzzle()) return;
                            SaveWorld();
                            Rebuild(true);
                        }, 64, MosseloraTheme.ForestAction);
                else
                {
                    // I349 (run 3, P12): the heading keeps clear of the floating menu button on
                    // both sides, so no scroll position can hide its first letters under it.
                    var titleRow = Node("Choose title row", content);
                    var titleGroup = titleRow.AddComponent<HorizontalLayoutGroup>();
                    titleGroup.padding = new RectOffset(72, 72, 0, 0);
                    titleGroup.childControlWidth = titleGroup.childControlHeight = true;
                    titleGroup.childForceExpandWidth = true;
                    titleGroup.childForceExpandHeight = false;
                    Text chooseTitle = Label(titleRow.transform, "Choose title", GameLocalization.Text("choose"),
                        24, 56, Ink, TextAnchor.MiddleCenter);
                    Emphasize(chooseTitle);
                    decisionHeading = chooseTitle.rectTransform;
                    Text chooseHelp = Label(content, "Choice preview help", GameLocalization.Text("preview_help"),
                        16, 0, Ink, TextAnchor.MiddleCenter);
                    // I393 (run 3, P18): said once, at the first choice; after that it is known,
                    // and still spoken to a screen reader.
                    if (world.Round > 0) SpokenOnly(chooseHelp);
                    // Three tall painted cards side by side, like cards on a table.
                    // With larger text they stack as wide cards so no word breaks.
                    bool stacked = TextScale.Percent > 100;
                    GameObject choiceRow;
                    if (stacked)
                    {
                        choiceRow = Node("Village choices", content);
                        var column = choiceRow.AddComponent<VerticalLayoutGroup>();
                        column.spacing = 10;
                        column.childControlWidth = true;
                        column.childControlHeight = true;
                        column.childForceExpandWidth = true;
                        column.childForceExpandHeight = false;
                    }
                    else
                    {
                        choiceRow = Row(content, "Village choices");
                        var choiceGroup = choiceRow.GetComponent<HorizontalLayoutGroup>();
                        choiceGroup.childForceExpandHeight = true;
                        choiceGroup.childAlignment = TextAnchor.UpperCenter;
                    }
                    decisionCards = choiceRow.GetComponent<RectTransform>();
                    for (int choiceIndex = 0; choiceIndex < 3; choiceIndex++)
                    {
                        VillageChoice choice = (VillageChoice)choiceIndex;
                        // None is visually preselected. A golden default would
                        // bias research toward the first habitat, not player care.
                        Transform cards = choiceRow.transform;
                        ChoiceCard(cards, choice, () => PreviewChoice(choice), stacked);
                    }
                    BuildChoicePreview(choiceRow.transform);
                }
                // No "revisit the water" token here any more (I169): once the stream
                // runs, the only decision is what to grow. A cold read took the round
                // undo arrow for "cancel the choice", and second thoughts about the
                // choice itself already live on the memory card ("change my choice").
            }
            else
            {
                var tools = Row(content, "Gentle controls");
                // Progressive disclosure: the first stream teaches the three tools
                // with small captions; afterwards they are pictures only.
                // Icon-only in every stream: Mira's coach bubble teaches the board,
                // and every tool names itself on press-and-hold (and is spoken).
                bool teach = false;
                // Upright, the tools are two round tokens flanking the water drops,
                // not two wide slabs under them (I129). Sideways they stand in a rail.
                // A cold read preferred the tokens but asked for them bigger: 76pt upright.
                bool roundTools = true;
                int toolHeight = landscape ? 72 : 76;
                // Undo | step count | Redo, one row (BoardKit.UndoRow's look). The row is a single
                // child of the tools, so the drops and Hint keep their places.
                undoRowNode = Node("Undo and redo", tools.transform);
                var undoRowGroup = undoRowNode.AddComponent<HorizontalLayoutGroup>();
                undoRowGroup.spacing = WaterUndoGap;
                undoRowGroup.childAlignment = TextAnchor.MiddleCenter;
                undoRowGroup.childControlWidth = undoRowGroup.childControlHeight = true;
                undoRowGroup.childForceExpandWidth = undoRowGroup.childForceExpandHeight = false;
                undoButton = IconButton(undoRowNode.transform, "Undo", GameLocalization.Text("undo"),
                    MosseloraArt.IconUndo, () => Act(() => world.Undo(), "undo"),
                    toolHeight, MosseloraTheme.ForestAction, false, null, teach, 0, 0, roundTools);
                var steps = Node("Undo steps", undoRowNode.transform);
                var stepsLayout = steps.AddComponent<LayoutElement>();
                stepsLayout.minWidth = stepsLayout.preferredWidth = WaterUndoCountWidth;
                stepsLayout.minHeight = stepsLayout.preferredHeight = toolHeight;
                undoStepCount = BoardKit.Number(steps.transform, "Step count", "0", font, 24, MosseloraTheme.EarthInk);
                redoButton = IconButton(undoRowNode.transform, "Redo", "Redo",
                    MosseloraArt.IconUndo, () => Act(() => world.Redo(), "redo"),
                    toolHeight, MosseloraTheme.ForestAction, false, null, teach, 0, 0, roundTools);
                // The redo arrow is the undo arrow turned round: one glyph, two jobs (as in BoardKit).
                Transform redoIcon = redoButton.transform.Find("Icon");
                if (redoIcon != null) redoIcon.localScale = new Vector3(-1f, 1f, 1f);
                RefreshUndoRow();
                ShowUndo(world.CanUndo || world.CanRedo, false);
                // Hint depth is the button's own words (Guide me, Show a closer clue, Fix one
                // connection). No level dots here: a dot means something new, and nothing else (I16).
                Button hintButton = IconButton(tools.transform, "Hint",
                    GameLocalization.Text(world.HintStep == 2 ? "hint_turn" :
                        world.HintStep == 1 ? "hint_more" : "hint"),
                    MosseloraArt.IconHint, UseGentleHint, toolHeight, MosseloraTheme.WaterAction, false,
                    null, teach, 0, 0, roundTools);
                if (!landscape) FlankDrops(tools, toolHeight);
                Transform hintDots = hintButton.transform.Find("Level dots");
                if (hintDots != null) hintMarkers.Add(hintDots.gameObject); // a move ends the hint: dots go too
                if (landscape && solvedBoard != null) DockToolRail(tools, toolHeight);
                // Restart stream lives in the menu (I109): it clears the undo history,
                // is rarely needed, and its arrow sat beside Undo's. The board keeps
                // only the two tools a player reaches for: Undo and Guide me.
                if (world.HintStep > 0 && world.HintTile >= 0)
                {
                    int hintRow = world.HintTile / puzzle.Size + 1;
                    int hintColumn = world.HintTile % puzzle.Size + 1;
                    // Spoken, not shown: the honey row tint and the breathing tile
                    // glow already point at the place to look.
                    Text clue = Label(content, "Optional progressive clue",
                        world.HintStep == 1 ?
                            GameLocalization.Text("hint_notice", hintRow) :
                            GameLocalization.Text("hint_focus", hintRow, hintColumn),
                        14, 0, new Color(0f, 0f, 0f, 0f), TextAnchor.MiddleCenter);
                    clue.GetComponent<LayoutElement>().ignoreLayout = true;
                }
            }
            // The coach bubble teaches; every tile's spoken hint still carries "how".
        }

        // Painted glyphs sit in cream discs; UI glyphs are drawn white on the bubble.
        private static void CoachGlyph(Transform parent, Sprite sprite, bool painted)
        {
            if (sprite == null) return;
            var holder = new GameObject(painted ? "Coach disc" : "Coach mark", typeof(RectTransform));
            holder.transform.SetParent(parent, false);
            var layout = holder.AddComponent<LayoutElement>();
            layout.minWidth = layout.preferredWidth = painted ? 46 : 30;
            layout.minHeight = layout.preferredHeight = painted ? 46 : 30;
            if (painted)
            {
                var disc = holder.AddComponent<Image>();
                MosseloraArt.Round(disc, MosseloraTheme.SagePanel, 23);
                disc.raycastTarget = false;
            }
            var mark = new GameObject("Glyph", typeof(RectTransform));
            mark.transform.SetParent(holder.transform, false);
            var rect = mark.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(7f, 7f) * (painted ? 1f : 0f);
            rect.offsetMax = new Vector2(-7f, -7f) * (painted ? 1f : 0f);
            var image = mark.AddComponent<Image>();
            image.sprite = sprite;
            image.preserveAspect = true;
            image.raycastTarget = false;
            if (!painted) image.color = MosseloraTheme.WaterChannel; // ink-blue marks on parchment
        }

        // Undo | water drops | Hint on one line under the board (I129). When a big
        // board's drops are too wide to sit between the tokens, they keep their own
        // line above, and the tokens stand at either end.
        private void FlankDrops(GameObject tools, int toolHeight)
        {
            var across = tools.GetComponent<HorizontalLayoutGroup>();
            across.childForceExpandWidth = false;
            across.childAlignment = TextAnchor.MiddleCenter;
            across.spacing = 10;
            MinHeight(tools, toolHeight);
            Transform meter = waterDrops != null && waterDrops.Length > 0 ? waterDrops[0].transform.parent : null;
            // Undo | count | Redo takes two tokens and the count; Hint takes one.
            float undoSide = 2f * toolHeight + 2f * WaterUndoGap + WaterUndoCountWidth;
            float room = PageWidth() - undoSide - toolHeight - 2f * across.spacing - 8f;
            if (meter != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)meter);
                float wanted = LayoutUtility.GetPreferredWidth((RectTransform)meter);
                // A big board's drops stack in two rows at full size between the
                // tokens, instead of one shrunken row (I139: 16 drops were ~11pt).
                if (wanted > room && waterDrops.Length > 9 && waterDrops[0].sprite != null)
                {
                    int columns = Mathf.CeilToInt(waterDrops.Length / 2f);
                    float dropHeight = 22f;
                    float dropWidth = Mathf.Round(dropHeight * MosseloraArt.Aspect(waterDrops[0].sprite));
                    float gapX = 4f, gapY = 4f;
                    float gridWidth = columns * dropWidth + (columns - 1) * gapX;
                    if (gridWidth <= room)
                    {
                        DestroyImmediate(meter.GetComponent<HorizontalLayoutGroup>());
                        var stack = meter.gameObject.AddComponent<GridLayoutGroup>();
                        stack.cellSize = new Vector2(dropWidth, dropHeight);
                        stack.spacing = new Vector2(gapX, gapY);
                        stack.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                        stack.constraintCount = columns;
                        stack.childAlignment = TextAnchor.MiddleCenter;
                        var stackLayout = meter.GetComponent<LayoutElement>();
                        if (stackLayout == null) stackLayout = meter.gameObject.AddComponent<LayoutElement>();
                        stackLayout.minWidth = stackLayout.preferredWidth = gridWidth;
                        stackLayout.minHeight = stackLayout.preferredHeight = 2f * dropHeight + gapY;
                        wanted = gridWidth;
                    }
                }
                // Otherwise a 4x4 board's sixteen drops shrink a little (to 70% at most) to fit.
                float shrink = wanted > room ? room / wanted : 1f;
                if (shrink >= 0.7f && shrink < 1f && meter.GetComponent<HorizontalLayoutGroup>() != null)
                {
                    foreach (Image drop in waterDrops)
                    {
                        var dropLayout = drop.GetComponent<LayoutElement>();
                        dropLayout.minWidth = dropLayout.preferredWidth = Mathf.Floor(dropLayout.preferredWidth * shrink);
                        dropLayout.minHeight = dropLayout.preferredHeight = Mathf.Floor(dropLayout.preferredHeight * shrink);
                    }
                    var meterGroup = meter.GetComponent<HorizontalLayoutGroup>();
                    meterGroup.spacing = Mathf.Floor(meterGroup.spacing * shrink);
                    wanted *= shrink;
                }
                if (wanted <= room)
                {
                    meter.SetParent(tools.transform, false);
                    meter.SetSiblingIndex(1);
                    var meterLayout = meter.GetComponent<LayoutElement>();
                    if (meterLayout == null) meterLayout = meter.gameObject.AddComponent<LayoutElement>();
                    meterLayout.flexibleWidth = 1f;
                    return;
                }
            }
            var gap = Node("Between the tools", tools.transform);
            gap.AddComponent<LayoutElement>().flexibleWidth = 1f;
            gap.transform.SetSiblingIndex(1);
        }

        // Undo above Hint, standing right of the board (landscape, unsolved). The undo row
        // stacks Undo, its count and Redo down the rail, since three across would not fit it.
        private void DockToolRail(GameObject tools, int toolHeight)
        {
            var across = tools.GetComponent<HorizontalLayoutGroup>();
            if (across != null) DestroyImmediate(across);
            var down = tools.AddComponent<VerticalLayoutGroup>();
            down.spacing = ToolRailGap;
            down.childAlignment = TextAnchor.MiddleCenter;
            down.childControlWidth = true;
            down.childControlHeight = true;
            down.childForceExpandWidth = true;
            down.childForceExpandHeight = false;
            if (undoRowNode != null)
            {
                var stack = undoRowNode.GetComponent<HorizontalLayoutGroup>();
                if (stack != null) DestroyImmediate(stack);
                var column = undoRowNode.AddComponent<VerticalLayoutGroup>();
                column.spacing = WaterUndoGap;
                column.childAlignment = TextAnchor.MiddleCenter;
                column.childControlWidth = true;
                column.childControlHeight = true;
                column.childForceExpandWidth = true;
                column.childForceExpandHeight = false;
                var countLayout = undoStepCount.transform.parent.GetComponent<LayoutElement>();
                if (countLayout != null) countLayout.minHeight = countLayout.preferredHeight = WaterUndoCountHeight;
            }
            tools.transform.SetParent(solvedBoard, false);
            var place = tools.GetComponent<LayoutElement>();
            if (place == null) place = tools.AddComponent<LayoutElement>();
            place.ignoreLayout = true;
            var rect = tools.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(1f, 0.5f);
            rect.pivot = new Vector2(1f, 0.5f);
            float undoStack = 2f * toolHeight + WaterUndoCountHeight + 2f * WaterUndoGap;
            rect.sizeDelta = new Vector2(ToolRailWidth, undoStack + ToolRailGap + toolHeight);
            rect.anchoredPosition = Vector2.zero;
        }

        private void CoachBubble(string message)
        {
            // Sideways Mira speaks from the painting, pointing right at the board, so
            // the whole board and its tools fit the short screen on first play (I116).
            bool beside = landscape && landscapeGarden != null;
            var row = Node("Prompt", beside ? landscapeGarden.transform : content);
            if (beside)
            {
                var rowRect = row.GetComponent<RectTransform>();
                rowRect.anchorMin = new Vector2(0f, 0.36f);
                rowRect.anchorMax = new Vector2(1f, 0.36f);
                rowRect.pivot = new Vector2(0.5f, 0f);
                // Clear of the camera cutout when it sits on the painting's side.
                rowRect.offsetMin = new Vector2(16f + Screen.safeArea.xMin / LiveCanvasScale(), 8f);
                rowRect.offsetMax = new Vector2(-30f, 8f);
                row.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            }
            var rowGroup = row.AddComponent<HorizontalLayoutGroup>();
            rowGroup.childAlignment = TextAnchor.MiddleCenter;
            rowGroup.padding = new RectOffset(10, 10, 0, 12); // room for the tail
            rowGroup.childControlWidth = true;
            rowGroup.childControlHeight = true;
            rowGroup.childForceExpandWidth = true;
            rowGroup.childForceExpandHeight = false;
            rowGroup.spacing = 8;
            rowGroup.childForceExpandWidth = false;
            rowGroup.childAlignment = TextAnchor.LowerCenter;
            Sprite guide = MosseloraArt.Portrait(0); // Mira welcomes newcomers to the creek
            if (guide != null)
            {
                var face = Node("Coach portrait", row.transform);
                var faceLayout = face.AddComponent<LayoutElement>();
                faceLayout.minWidth = faceLayout.preferredWidth = 54;
                faceLayout.minHeight = faceLayout.preferredHeight = 54;
                var faceImage = face.AddComponent<Image>();
                faceImage.sprite = guide;
                faceImage.preserveAspect = true;
                faceImage.raycastTarget = false;
                MosseloraArt.Lift(faceImage, 2f, 0.2f);
            }
            var bubble = Node("Coach bubble", row.transform);
            bubble.AddComponent<LayoutElement>().flexibleWidth = 1f;
            // Mira's note on painted parchment, like the choice cards, rather
            // than a dark UI panel (I77): part of the garden, not a tooltip.
            var fill = bubble.AddComponent<Image>();
            MosseloraArt.PaperTile(fill, 18);
            fill.color = MosseloraTheme.CardFill;
            fill.raycastTarget = false;
            MosseloraArt.Lift(fill, 3f, 0.18f);
            MosseloraArt.Edge(bubble.transform, MosseloraTheme.CardEdge, 18);
            var pad = bubble.AddComponent<VerticalLayoutGroup>();
            pad.padding = new RectOffset(16, 16, 12, 18); // last line clears the rim (I79)
            pad.childControlWidth = true;
            pad.childControlHeight = true;
            pad.childForceExpandWidth = true;
            // A picture, not a sentence: spring waves → garden sprout, and a
            // tapping hand. The sentence stays as spoken text for screen readers.
            // At larger text the sentence alone carries the rule, so the
            // picture row steps aside and the board's tools stay on screen (I85).
            // Since I151 the pictures are the tip at every size; at larger text the
            // sentence returns under them (it was hidden everywhere, leaving an
            // empty bubble at 125-150%; I163).
            bool showPicture = true;
            var picture = Node("Coach picture", bubble.transform);
            picture.SetActive(showPicture);
            var pictureRow = picture.AddComponent<HorizontalLayoutGroup>();
            pictureRow.spacing = 10;
            pictureRow.childAlignment = TextAnchor.MiddleCenter;
            pictureRow.childControlWidth = true;
            pictureRow.childControlHeight = true;
            pictureRow.childForceExpandWidth = false;
            pictureRow.childForceExpandHeight = false;
            CoachGlyph(picture.transform, MosseloraArt.Load(MosseloraArt.SpringWaves), true);
            CoachGlyph(picture.transform, MosseloraArt.Load(MosseloraArt.IconContinue), false);
            CoachGlyph(picture.transform, MosseloraArt.Load(MosseloraArt.Sprout), true);
            var gap = Node("Coach gap", picture.transform);
            gap.AddComponent<LayoutElement>().minWidth = 6;
            CoachGlyph(picture.transform, MosseloraArt.Load(MosseloraArt.IconTap), false);
            // One plain sentence under the pictures: a first-time proxy walkthrough
            // could not tell from pictures alone that a tap turns a pipe (I64).
            Text words = Label(bubble.transform, "Coach words", message, 16, 0, Ink, TextAnchor.MiddleCenter);
            Emphasize(words);
            // Pictures only at standard text (I151): the board now demonstrates the
            // turn (I138 arrow, I147 finger), and a blind A/B read understood the
            // goal and the turn as well without the sentence. A player who has
            // asked for larger text also gets the sentence (I163). Always spoken.
            if (TextScale.Percent <= 100)
            {
                words.color = new Color(0f, 0f, 0f, 0f);
                words.GetComponent<LayoutElement>().ignoreLayout = true;
            }
            var tail = Node("Coach tail", bubble.transform);
            tail.AddComponent<LayoutElement>().ignoreLayout = true;
            var tailRect = tail.GetComponent<RectTransform>();
            tailRect.anchorMin = tailRect.anchorMax = beside ? new Vector2(1f, 0.5f) : new Vector2(0.5f, 0f);
            tailRect.sizeDelta = new Vector2(16f, 16f);
            tailRect.anchoredPosition = beside ? new Vector2(2f, 0f) : new Vector2(0f, -2f);
            tailRect.localRotation = Quaternion.Euler(0f, 0f, 45f);
            var tailImage = tail.AddComponent<Image>();
            tailImage.color = MosseloraTheme.CardFill;
            tailImage.raycastTarget = false;
            tail.transform.SetAsFirstSibling(); // under the bubble body
            coachBubble = row;
        }
    }
}
