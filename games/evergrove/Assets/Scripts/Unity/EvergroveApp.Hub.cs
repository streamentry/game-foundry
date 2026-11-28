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

        // A calm daily ritual entry: the village painting full-bleed, the
        // wordmark, the stones walked so far and one big play button. No
        // streaks, no "you were away" - the garden simply waited.
        private void BuildTitleScreen(Transform host)
        {
            if (quietLayer != null) ExitQuiet(); // a new home never starts hidden (I294)
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (qaInMemoryOnly && !qaShowHub) return; // the scripted QA tour starts on the board
#endif
            // A finished garden opens on its festival; otherwise the everyday village.
            // The hub opens on *your* story: the finished garden's festival, else
            // the painting of your most recent memory, else the everyday village.
            int[] story = world.Choices;
            // Newest memory first; each later launch recalls an earlier one (I107).
            if (!hubRecallPicked)
            {
                hubRecall = world.Mode == PlayMode.Living ? PickHubRecall(story.Length) : -1;
                hubRecallPicked = true;
            }
            Sprite hero = null;
            if (world.Completed) hero = MosseloraArt.Load(MosseloraArt.EndingFestival);
            else if (hubRecall >= 0)
                hero = MosseloraArt.Load(story[hubRecall] == 0 ? MosseloraArt.VignetteTree
                    : story[hubRecall] == 1 ? MosseloraArt.VignetteBridge : MosseloraArt.VignetteFlowers);
            if (hero == null) hero = MosseloraArt.Load(MosseloraArt.HeroVillage);
            if (hero == null) return;
            titleScreen = Node("Title screen", host);
            Fill(titleScreen.GetComponent<RectTransform>());
            MosseloraArt.PagePaper(titleScreen.AddComponent<Image>(), Paper);

            // The village at the top, clipped to the upper half.
            var window = Node("Village window", titleScreen.transform);
            var windowRect = window.GetComponent<RectTransform>();
            // Reaches down behind the strip's sky so no blank band opens between them (I73).
            windowRect.anchorMin = new Vector2(0f, 0.41f);
            windowRect.anchorMax = Vector2.one;
            windowRect.offsetMin = windowRect.offsetMax = Vector2.zero;
            window.AddComponent<RectMask2D>();
            // Now and then a butterfly drifts across the village: the garden is
            // alive when you come back to it (I157). Decoration only, gentle motion only.
            bool lively = gentleMotion;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (qaInMemoryOnly) lively = false;
#endif
            // I325: butterflies come to flowers in spring and summer (before I325 they always flew).
            bool livingGarden = world != null && world.Mode == PlayMode.Living;
            if (lively && (!livingGarden || (Visitors.Welcomes(world, Visitor.Butterflies) &&
                Visitors.OutIn(Visitor.Butterflies, GardenSeason.For(world), DayLightOn ? CurrentPhase() : DayPhase.Day, TodayWeather(world)))))
                window.AddComponent<HubButterflies>();
            // The living garden opens on its own map (E3, I176): everything you made
            // standing where it grew. Otherwise, the painting as before.
            bool mapShown = world.Mode == PlayMode.Living && GardenMapOnHub(windowRect);
            var art = Node("Village at the start", window.transform);
            art.SetActive(!mapShown);
            var artRect = art.GetComponent<RectTransform>();
            artRect.anchorMin = Vector2.zero;
            artRect.anchorMax = Vector2.one;
            artRect.pivot = new Vector2(0.5f, 0.6f);
            artRect.offsetMin = artRect.offsetMax = Vector2.zero;
            var artImage = art.AddComponent<Image>();
            artImage.sprite = hero;
            artImage.raycastTarget = false;
            // Cover the window whatever the painting's shape (portrait village,
            // landscape memory vignette), so no blank band appears.
            var fitter = art.AddComponent<AspectRatioFitter>();
            fitter.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
            fitter.aspectRatio = MosseloraArt.Aspect(hero);
            var melt = Node("Village melt", window.transform);
            var meltRect = melt.GetComponent<RectTransform>();
            meltRect.anchorMin = Vector2.zero;
            meltRect.anchorMax = new Vector2(1f, mapShown ? 0.08f : 0.24f); // the map runs nearly to the wordmark (I181)
            meltRect.offsetMin = meltRect.offsetMax = Vector2.zero;
            var meltImage = melt.AddComponent<Image>();
            meltImage.sprite = MosseloraArt.TopFade();
            meltImage.color = Paper;
            meltImage.rectTransform.localScale = new Vector3(1f, -1f, 1f);
            meltImage.raycastTarget = false;

            // Your creek, as you left it: real water, your landmarks, your neighbors.
            var strip = Node("Your village", titleScreen.transform);
            var stripRect = strip.GetComponent<RectTransform>();
            stripRect.anchorMin = new Vector2(0f, 0.33f);
            stripRect.anchorMax = new Vector2(1f, 0.52f);
            stripRect.offsetMin = stripRect.offsetMax = Vector2.zero;
            // With the map, the neighbours stand in the garden itself and the map
            // takes the strip's place (I181).
            strip.SetActive(!mapShown);
            if (mapShown) windowRect.anchorMin = new Vector2(0f, 0.33f);
            var painting = strip.AddComponent<PaintedVillage>();
            painting.Build(0f, PaintedVillage.ArtSeason(world));
            if (painting.Ready)
            {
                VillageScenePlan plan = VillageScenePlan.From(world);
                painting.Show(plan.Chapter, plan.TreeCount, plan.CrossingCount, plan.FlowerCount,
                    plan.WaterRestored, plan.WaterReachPercent);
            }
            if (world.Mode == PlayMode.Living) NeighborsOnPainting(strip.transform, true);

            // Home is your garden, full screen (I190): no paper, no wordmark, no
            // tagline; the walk, the next stream's sign and one round play button
            // stand on the grass at the bottom.
            bool worldHome = mapShown;
            if (worldHome)
            {
                windowRect.anchorMin = Vector2.zero;
                melt.SetActive(false);
            }
            var stack = Node("Title stack", titleScreen.transform);
            var stackRect = stack.GetComponent<RectTransform>();
            stackRect.anchorMin = new Vector2(0f, 0.03f);
            stackRect.anchorMax = new Vector2(1f, 0.33f);
            stackRect.offsetMin = new Vector2(24f, 0f);
            stackRect.offsetMax = new Vector2(-24f, 0f);
            var column = stack.AddComponent<VerticalLayoutGroup>();
            column.childAlignment = TextAnchor.MiddleCenter;
            column.spacing = 12;
            column.childControlWidth = true;
            column.childControlHeight = true;
            column.childForceExpandWidth = true;
            column.childForceExpandHeight = false;
            if (worldHome) column.childAlignment = TextAnchor.LowerCenter;
            if (worldHome && landscape)
            {
                // Sideways the screen is short and the neighbours stand mid-garden, so
                // the sign and play token keep to a right-hand column (I197).
                stackRect.anchorMin = new Vector2(0.7f, 0.04f);
                stackRect.anchorMax = new Vector2(1f, 0.8f);
            }
            Sprite mark = worldHome ? null : MosseloraArt.Load(MosseloraArt.Wordmark);
            if (mark != null)
            {
                var markNode = Node("Mosselora wordmark", stack.transform);
                Height(markNode, 96f); // the repo's painted brand logo (about 2.6:1)
                var markImage = markNode.AddComponent<Image>();
                markImage.sprite = mark;
                markImage.preserveAspect = true;
                markImage.raycastTarget = false;
            }
            if (!worldHome)
            {
                Text tagline = Label(stack.transform, "Title", GameLocalization.Text("subtitle"), 17, 0, Ink,
                    TextAnchor.MiddleCenter);
                Emphasize(tagline);
            }
            // How far along the creek this garden has walked: resumes, never resets.
            if (!world.Completed && !(worldHome && landscape))
                StreamStones(world.CurrentPuzzle, stack.transform, false);
            // What changed last time, told by the neighbor who remembers it.
            if (world.Mode == PlayMode.Living && world.Choices.Length > 0 && !worldHome)
                KeeperLine(stack.transform, "Last memory", hubRecall);
            if (worldHome)
            {
                // I204, checklist 13: a thing you've grown, picked up, holds its own puzzle. Its Play and
                // Move tokens sit beside it on the map (ThingActions.cs), so no bottom play token here.
                int held = world.Mode == PlayMode.Living && mapInHand >= 0 && mapInHand < world.Choices.Length
                    ? mapInHand : -1;
                int heldPlant = world.Mode == PlayMode.Living && heldBought >= 0 && heldBought < world.BoughtCount
                    ? heldBought : -1;
                if (held >= 0 || heldPlant >= 0)
                {
                    SeedPurse();
                    DayRhythmTokens(titleScreen.transform); // checklist 54
                    if (world.Mode == PlayMode.Living) BasketControls(titleScreen.transform, stack.transform);
                    HubChrome();
                    MaybeAskNotes();
                    FollowNote();
                    titleScreen.transform.SetAsLastSibling();
                    return;
                }
                // The same plank as over the board, then a round play token (I190).
                if (!world.Completed && GameLocalization.Current == "en")
                    NextStreamLine(stack.transform, world.CurrentPuzzle.Name, false);
                Transform token = CenteredRowIn(stack.transform, "Play row");
                // Checklist 5: sideways, home's column sits at the right edge; with the menu in the right corner
                // the play token keeps clear of it.
                if (MenuOnRight && Screen.width > Screen.height)
                    token.GetComponent<HorizontalLayoutGroup>().padding.right = (int)MenuReserve;
                // I351 (run 3, P9): after the story, the big arrow plays what's waiting, whose picture
                // peeks from the token. SC05: the ranker picks it (family help first, then a new
                // arrival), else the newest thing still holding its first puzzle, as before.
                CompanionAttention.Target arrow = CompanionAttention.ArrowTarget(world, CompanionFacts.For(world, CompanionNow()),
                    CompanionContext.DayNumberOf(CompanionToday()));
                int waitingBought = arrow.Bought, waitingThing = arrow.Thing;
                Button playToken = IconButton(token, "Play", world.Completed ? GameLocalization.Text("continue")
                        : world.CurrentPuzzle.Name, MosseloraArt.IconContinue, () => {
                            basketOpen = false; basketKind = -1; heldPlanting = -1;
                            nurseryOpen = false; nurseryKind = -1;
                            if (waitingBought >= 0) { heldBought = waitingBought; mapInHand = -1; OpenBoughtPlay(waitingBought); return; }
                            heldBought = -1;
                            if (waitingThing >= 0) { mapInHand = waitingThing; OpenTreePlay(waitingThing); return; }
                            CloseTitle();
                        }, 96, MosseloraTheme.WaterAction, false, null, false, 0, 0, true);
                Sprite waiting = waitingBought >= 0 ? MosseloraArt.Load(Nursery.Item(world.Bought[waitingBought * 2]).Art)
                    : waitingThing >= 0 ? ItemArt(waitingThing, world.Choices[waitingThing]) : null;
                if (waiting != null)
                    MosseloraArt.Ornament(playToken.transform, "What it plays", waiting, new Vector2(1f, 1f),
                        new Vector2(0.5f, 0.5f), new Vector2(44f, 44f), new Vector2(-4f, -4f));
                if (world.Mode == PlayMode.Living) BasketControls(titleScreen.transform, stack.transform);
                SeedPurse();
                DayRhythmTokens(titleScreen.transform); // checklist 54
                HubChrome();
                MaybeAskNotes();
                FollowNote();
                titleScreen.transform.SetAsLastSibling();
                return;
            }
            Transform play = CenteredRowIn(stack.transform, "Play row");
            // The big button names where the walk continues ("First Bend").
            string nextStop = !world.Completed && GameLocalization.Current == "en"
                ? world.CurrentPuzzle.Name : GameLocalization.Text("continue");
            // The play button carries the next stream's name beside its arrow, as
            // casual games show "Level 23" on Play: a returning player could not
            // tell what the bare arrow would start (I156). Words only where a
            // name is known; otherwise the arrow alone.
            bool named = !world.Completed && GameLocalization.Current == "en";
            int cut = nextStop.IndexOf(" · ", System.StringComparison.Ordinal);
            string place = cut > 0 ? nextStop.Substring(0, cut) : nextStop;
            Button start = IconButton(play, "Play", named ? place : nextStop,
                MosseloraArt.IconContinue, CloseTitle, 72, MosseloraTheme.WaterAction, named);
            if (named)
            {
                Transform caption = start.transform.Find("Caption");
                Text placeText = caption != null ? caption.GetComponent<Text>() : null;
                if (placeText != null) { StyleText(placeText, 20); Emphasize(placeText); }
                Width(start, 250);
            }
            else Width(start, 150);
            HubChrome();
            titleScreen.transform.SetAsLastSibling();
            if (landscape)
            {
                // Sideways (I114): the painting and village fill the left, the
                // wordmark and Play stand in the right-hand column.
                const float split = 0.56f;
                windowRect.anchorMin = Vector2.zero;
                windowRect.anchorMax = new Vector2(split, 1f);
                stripRect.anchorMin = Vector2.zero;
                stripRect.anchorMax = new Vector2(split, 0.36f);
                stackRect.anchorMin = new Vector2(split, 0.04f);
                stackRect.anchorMax = new Vector2(1f, 0.96f);
                var side = Node("Village melt right", window.transform);
                var sideRect = side.GetComponent<RectTransform>();
                sideRect.anchorMin = new Vector2(1f, 0f);
                sideRect.anchorMax = new Vector2(1f, 1f);
                sideRect.pivot = new Vector2(1f, 0.5f);
                sideRect.sizeDelta = new Vector2(56f, 0f);
                var sideImage = side.AddComponent<Image>();
                sideImage.sprite = MosseloraArt.SideFade();
                sideImage.color = Paper;
                sideImage.raycastTarget = false;
            }
            else
                StartCoroutine(FitHubStack(windowRect, stripRect, stackRect, mapShown ? 0.33f : 0.41f));
            OfferFamilyOnce();
        }

        // I201: the memory screen's map grows into the blank paper between the
        // memory card and the pinned buttons, measured on screen after layout, so
        // it fits any phone and text size.
        private GameObject fillTarget;
        private IEnumerator GrowToFill(GameObject element, RectTransform above, RectTransform buttons)
        {
            yield return null; // after the first layout pass, at the real canvas size
            if (element == null || above == null || buttons == null) yield break;
            var layout = element.GetComponent<LayoutElement>();
            if (layout == null) yield break;
            var corners = new Vector3[4];
            above.GetWorldCorners(corners);
            float cardBottom = element.transform.parent.InverseTransformPoint(corners[0]).y;
            float buttonTop = float.NegativeInfinity;
            foreach (Transform child in buttons)
            {
                if (!child.gameObject.activeSelf) continue;
                ((RectTransform)child).GetWorldCorners(corners);
                buttonTop = Mathf.Max(buttonTop, element.transform.parent.InverseTransformPoint(corners[1]).y);
            }
            float gap = cardBottom - buttonTop - 22f; // keep a little air above the buttons
            if (float.IsInfinity(buttonTop) || gap < 12f) yield break;
            layout.minHeight = layout.preferredHeight = Mathf.Round(layout.preferredHeight + Mathf.Min(gap, 320f));
        }

        // The words and Play under the painting take the room they need: at
        // larger text, or with a memory line and the next stream, the painting
        // and village strip move up instead of the words overflowing (I85).
        private IEnumerator FitHubStack(RectTransform window, RectTransform strip, RectTransform stack,
                                        float windowBase)
        {
            yield return null; // real canvas size, after the first layout pass
            if (stack == null || canvasRoot == null) yield break;
            LayoutRebuilder.ForceRebuildLayoutImmediate(stack);
            float canvasHeight = canvasRoot.rect.height;
            if (canvasHeight <= 1f) yield break;
            float needed = LayoutUtility.GetPreferredHeight(stack) + 28f; // air above the wordmark
            float top = Mathf.Clamp(0.03f + needed / canvasHeight, 0.33f, 0.52f);
            float shift = top - 0.33f;
            if (shift <= 0.001f) yield break;
            stack.anchorMax = new Vector2(1f, top);
            strip.anchorMin = new Vector2(0f, 0.33f + shift);
            strip.anchorMax = new Vector2(1f, 0.52f + shift);
            // The map reaches down to the words (I181); the painting kept its old base.
            window.anchorMin = new Vector2(0f, windowBase + shift);
        }

        // The hub's one wooden menu button (checklist 94: bottom-left, one thumb's reach). It opens the
        // fan of everything else; the book token and the other controls live in the fan (I191).
        private void HubChrome()
        {
            var bar = Node("Hub chrome", titleScreen.transform);
            var rect = bar.GetComponent<RectTransform>();
            Rect safe = Screen.safeArea;
            float w = Mathf.Max(1, Screen.width), h = Mathf.Max(1, Screen.height);
            rect.anchorMin = new Vector2(safe.xMin / w, safe.yMin / h);
            rect.anchorMax = new Vector2(safe.xMax / w, safe.yMin / h);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.sizeDelta = new Vector2(0f, MenuSize + MenuInset + 2f);
            rect.anchoredPosition = Vector2.zero;
            Button menu = IconButton(bar.transform, "Open menu", GameLocalization.Text("menu"),
                MosseloraArt.IconMenu, () => ToggleFan(true), 54, MosseloraTheme.QuietAction);
            PlaceChrome(menu, MenuCornerAnchor, new Vector2(MenuSideOffset, MenuInset));
            if (world.Mode == PlayMode.Living && world.Choices.Length > 0) MarkUnreadMemory(menu, MenuSeenKey);
            MenuLetterCue(menu); // checklist 11: the letter's arrival and unread dot live on the menu button
            MenuGiftCue(menu); // checklist 11 (B5b): a cutting waiting in the folded basket
        }

        // "Rainwater" with the painted landmark your last choice planted in it:
        // the next stream visibly carries what you grew (I73). Placed above Play.
        private void NextStreamLine(Transform stack, string nextStop, bool abovePlay)
        {
            int split = nextStop.IndexOf(" · ", System.StringComparison.Ordinal);
            int[] past = world.Choices;
            Sprite mark = split > 0 && past.Length > 0
                ? ItemArt(past.Length - 1, past[past.Length - 1]) : null;
            Transform row = CenteredRowIn(stack, "Next stream row");
            row.GetComponent<HorizontalLayoutGroup>().spacing = 6;
            Transform outer = row;
            row = PaintedSign(row) ?? row; // the same plank as on the board (I165)
            if (mark != null)
            {
                var icon = Node("Shaped by your choice", row);
                var iconLayout = icon.AddComponent<LayoutElement>();
                iconLayout.minWidth = iconLayout.preferredWidth = 34;
                iconLayout.minHeight = iconLayout.preferredHeight = 34;
                var iconImage = icon.AddComponent<Image>();
                iconImage.sprite = mark;
                iconImage.preserveAspect = true;
                iconImage.raycastTarget = false;
                BloomPulse.Add(icon, 0.6f, 0.05f);
            }
            bool onSign = row != outer;
            Text where = Label(row, "Next stream", mark != null ? nextStop.Substring(0, split) : nextStop,
                16, 0, onSign ? Ink : Moss, TextAnchor.MiddleCenter);
            Emphasize(where);
            if (onSign) OneLine(where);
            if (abovePlay) outer.SetSiblingIndex(stack.childCount - 2);
        }

        private int hubRecall = -1;
        private bool hubRecallPicked; // a rotation rebuilds the hub; it must not advance the recall
        private const string HubRecallKey = "evergrove:hub-recall";

        // Reads "count:index" from the last launch, picks this launch's memory
        // with Core.HubRecall, and stores it. Local only; no clock involved.
        private static int PickHubRecall(int memoryCount)
        {
            int lastCount = 0, lastIndex = -1;
            string[] saved = PlayerPrefs.GetString(HubRecallKey, "").Split(':');
            if (saved.Length == 2)
            {
                int.TryParse(saved[0], out lastCount);
                if (!int.TryParse(saved[1], out lastIndex)) lastIndex = -1;
            }
            int index = HubRecall.Next(memoryCount, lastCount, lastIndex);
            if (index >= 0)
            {
                PlayerPrefs.SetString(HubRecallKey, memoryCount + ":" + index);
                PlayerPrefs.Save();
            }
            return index;
        }

        // ---- E3 (I176): the open garden map.
        private GardenMap gardenMap;
        private int qaLongRiver; // checklist 50 tour; read by shipping code, so declared outside the dev-only block

        // The endless river (I192): one reach of the painting per three streams
        // restored, all of them once the garden is finished.
        private GardenMap LoadGardenMap()
        {
            // After the story the river keeps opening as you tend your things (I220).
            int reaches = GardenMap.ReachesFor(world.Choices.Length, world.Completed, world.PuzzlesSolved, world.RiverReplays);
            if (qaLongRiver > 0) reaches = qaLongRiver; // checklist 50 tour only: a long river to scroll; 0 in play
            if (gardenMap != null && gardenMap.Reaches == reaches) return gardenMap;
            var mask = Resources.Load<TextAsset>("Evergrove/garden-map-mask");
            if (mask == null) return null;
            try { gardenMap = GardenMap.River(10, 15, mask.text.Trim(), reaches); }
            catch (ArgumentException error) { Debug.LogWarning("Garden map mask unreadable: " + error.Message); }
            return gardenMap;
        }

        // After the story, the stretch in the mist wears the season it will bring (I252).
        private Sprite NextSeasonPlot()
        {
            if (world.Mode != PlayMode.Living || !world.Completed) return null;
            if (!world.TowardNextStretch(out int have, out int need)) return null; // checklist 41: replays count once nothing new is left
            Season next = GardenSeason.AfterStory(world.PuzzlesSolved + (need - have));
            int art = next == Season.Spring ? 3 : next == Season.Autumn ? 1 : next == Season.Winter ? 2 : 0;
            return MosseloraArt.Load(PaintedVillage.MapFor(art));
        }

        // The next reach shows in mist below the open river until it opens.
        private static bool RiverGoesOn(GardenMap map) { return map != null && map.Reaches < GardenMap.SafetyReachBound; }

        private readonly float[] hubZoom = { 0f, 0f };
        private bool markPuzzles;

        // ---- I204: the puzzle a grown thing holds, and the seeds it pays.
        private void OpenTreePlay(int item)
        {
            int[] made = world.Choices;
            if (titleScreen == null || item < 0 || item >= made.Length) return;
            GardenItem thing = ItemAt(item, made[item]);
            // A thing's first board is its own (seeded by garden and place); replays deal fresh
            // boards that deepen as you come back to it (I259/I260).
            int replays = world.TreeSolved(item) ? Replays(item) : 0;
            int tier = ReplayBoards.Tier(TreePuzzles.TierFor(item), replays);
            int seed = ReplayBoards.Seed(world.Garden * 1000 + item * 7 + 1, world.TreeSolved(item) ? replays + 1 : 0);
            // v1 #80: one day decides both the reward shown here and the seeds paid on solve.
            int day = LocalDayStamp();
            int reward = world.TreeSolved(item)
                ? world.ReplayRewardOn(TreePuzzles.TierFor(item), day)
                : WorldModel.TreeReward(TreePuzzles.TierFor(item), true);
            bool replay = world.TreeSolved(item);
            Func<int> solved = () => { int fruitBeforeSolve = world.Fruit; int earned = world.SolveTree(item, day); if (replay) CountReplay(item);
                NotePollinated(item, world.Fruit > fruitBeforeSolve); // I378, gated by the fruit this solve picked
                SaveWorld(); PlayChime(); return earned; };
            OpenThingPuzzle(seed, tier, ThingPuzzle.ForStory(item, (VillageChoice)made[item]), ItemArt(item, made[item]),
                thing.Name, reward, item, solved, ownBoard: true);
        }

        // Replays per thing, on this device (I259): only what boards to deal next, never progress.
        private string ReplayKey(int index) { return "evergrove:replays:" + world.Garden + ":" + index; }
        private int Replays(int index) { return Mathf.Max(0, PlayerPrefs.GetInt(ReplayKey(index), 0)); }
        private void CountReplay(int index)
        {
            PlayerPrefs.SetInt(ReplayKey(index), Replays(index) + 1);
            PlayerPrefs.Save();
        }

        // A nursery plant's puzzle (I208): its row is its tier; its own seed.
        private void OpenBoughtPlay(int k)
        {
            if (titleScreen == null || k < 0 || k >= world.BoughtCount) return;
            int kind = world.Bought[k * 2];
            GardenItem plant = Nursery.Item(kind);
            bool replay = world.BoughtSolved(k);
            int replays = replay ? Replays(BoughtIndex + k) : 0;
            int tier = ReplayBoards.Tier(Nursery.Row(kind), replays);
            int day = LocalDayStamp(); // v1 #80: the same day for the reward shown and the seeds paid
            int reward = replay ? world.ReplayRewardOn(Nursery.Row(kind), day) : WorldModel.TreeReward(Nursery.Row(kind), true);
            OpenThingPuzzle(ReplayBoards.Seed(world.Garden * 1000 + 500 + k * 7, replay ? replays + 1 : 0), tier,
                ThingPuzzle.ForNursery(kind),
                MosseloraArt.Load(plant.Art), plant.Name, reward, k * 5 + kind,
                () => { int fruitBeforeSolve = world.Fruit; int earned = world.SolveBought(k, day); if (replay) CountReplay(BoughtIndex + k);
                    NotePollinated(world.Choices.Length + world.PlantingCount + k, world.Fruit > fruitBeforeSolve); // I378
                    SaveWorld(); PlayChime(); return earned; }, ownBoard: true);
        }

        // Each thing holds one kind of puzzle (I243: ThingPuzzle).
        private void OpenThingPuzzle(int seed, int tier, PuzzleKind kind, Sprite art, string name, int reward,
                                     int pictureStart, Func<int> solved, bool ownBoard = false)
        {
            // Item 34: only your own things follow your chosen level; a relative's shared board, the well
            // and the weekly deeper board stay exactly as dealt.
            if (ownBoard)
            {
                Func<int, int, Func<int>, Action> reopenAt = (s, t, sv) => () => OpenThingPuzzle(s, t, kind, art, name, reward, pictureStart, sv, ownBoard);
                // Own nonograms measure the chosen level on the garden's own bed (OwnGardenBoards.DealAt).
                (seed, tier) = kind == PuzzleKind.Nonogram ? OwnNonogramDeal(seed, tier, solved, reopenAt)
                    : ChosenDeal(kind, seed, tier, solved, reopenAt);
            }
            moveNote = 0; // I305: each board starts its phrase from the top
            Action turned = () => { PlayMove(kind); Haptics.Placed(); };
            int shown = mapInHand >= 0 ? mapInHand : heldBought >= 0 ? BoughtIndex + heldBought : -1;
            // Back home, the view returns to the thing and its ring grows in (I212).
            string hintsKey = "evergrove:hints:" + (int)kind; // I322: per kind, on this device
            Action closed = () => { if (openCard != null) { PlayerPrefs.SetInt(hintsKey, openCard.HintsUsed); PlayerPrefs.Save(); }
                justSolved = solvedHere ? shown : -1; solvedHere = false;
                mapInHand = -1; heldBought = -1; keepHubAt = new Vector2(-1f, -1f); RefreshMapsFresh(); };
            Func<int> solvedOnce = solved;
            solved = () =>
            {
                solvedHere = true;
                int before = OpenReaches(), solvedBefore = world.PuzzlesSolved, fruitBefore = world.Fruit;
                int earned = solvedOnce();
                // Played taking turns (I263/I264): the garden remembers it was solved together.
                if (openCard != null && openCard.PlayedTogether && shown >= 0 && world.MarkTogether(shown))
                {
                    SaveWorld();
                    togetherKind = kind; // I266: its keeper has news
                    togetherKeeper = NeighbourNotice.OwnerOf(kind);
                }
                fruitPicked = world.Fruit > fruitBefore; // trees bear fruit when tended (I229)
                // After the story, every fourth solve opens more river (I220): home shows it (I223).
                riverOpened = OpenReaches() > before;
                // Mira leaves a note in the letterbox when the river opens (I271).
                if (riverOpened && world.Completed &&
                    world.KeepNote("Mira", NeighbourNotice.RiverNote(GardenSeason.AfterStory(world.PuzzlesSolved),
                        RiverNames.For(OpenReaches() - 1)))) // I362: by name
                {
                    letterArrived = true;
                    SaveWorld();
                }
                dropEarned = world.PuzzlesSolved > solvedBefore;
                return earned;
            };
            ownDealing = ownBoard; // own boards draw from the garden (nonogram beds, mosaic picture)
            PuzzleCard((RectTransform)titleScreen.transform, seed, tier, kind, art, name, reward,
                count => PairsPictures(count, pictureStart, (c, f) => ItemArt(c, f)), turned, solved, closed);
            ownDealing = false;
            TreePlay card = titleScreen.GetComponentInChildren<TreePlay>();
            openCard = card;
            // I261: how deep this board is, in leaves. I336 (idea 13, after Nikoli's honest
            // difficulty): kinds that can measure their own logic show that, not their size.
            if (card != null) card.ShowTier(boardLeaves >= 1 ? boardLeaves - 1 : tier);
            // I323: each kind's flourish when solved.
            if (card != null) { card.Flourish = FlourishArt(kind, out Color tint); card.FlourishTint = tint; }
            // I322: a kind that needed hints last time starts with a move or two made.
            if (card != null) card.GiveHeadStart(HeadStart.Moves(PlayerPrefs.GetInt(hintsKey, 0), tier));
            // I309: a board of your own can be sent to family once solved. v1 #80: ownership is
            // explicit, because a capped replay of your own thing now pays 0 seeds.
            // A nonogram of your garden is offered only when a relative can play the same clues (SharedSeedFor).
            if (card != null && ownBoard && world.Mode == PlayMode.Living && FamilyShown())
            {
                if (ShareSeedFor(kind, seed, tier) >= 0) card.SendBoard = () => SendBoardToFamily(kind, tier, seed, name);
            }
        }

        // The seed a relative plays this board from: the board's own seed, or for a nonogram of your
        // garden the standard seed with the same clues (-1 when none, so nothing is offered).
        private int ShareSeedFor(PuzzleKind kind, int seed, int tier)
        {
            if (kind != PuzzleKind.Nonogram) return seed;
            return world == null ? -1 : OwnGardenBoards.SharedSeedFor(OwnGardenBoards.NonogramFor(world, seed, tier), tier);
        }

        // Set only while OpenThingPuzzle deals one of your own things, so PuzzleCard draws from the garden.
        private bool ownDealing;

        // The own nonogram deal (item 34 for your things): same flow as ChosenDeal, measured on the own bed.
        private (int, int) OwnNonogramDeal(int seed, int tier, Func<int> solved, Func<int, int, Func<int>, Action> reopenAt)
        {
            chooserKind = PuzzleKind.Nonogram;
            chooserReopen = reopenAt(seed, tier, solved);
            int level = ChosenLeaves(PuzzleKind.Nonogram);
            (int, int) deal = level == 0 ? (seed, tier) : OwnGardenBoards.DealAt(world, seed, level);
            chooserTier = deal.Item2;
            StartCoroutine(AttachLeavesChooser());
            return deal;
        }

        // The card itself, for your things and, visiting, for theirs (I227); eight kinds (I243).
        private int boardLeaves; // I336: 1..3 from the board's logic, or 0 when the kind can't measure it

        private void PuzzleCard(RectTransform host, int seed, int tier, PuzzleKind kind, Sprite art, string name,
                                int reward, Func<int, Sprite[]> pictures, Action turned, Func<int> solved, Action closed)
        {
            TreePlay.IPuzzleBoard board = null;
            // Checklist 38: what this board belongs to, for the chain line and the watered mark (own boards only).
            ChainCause.Opening(kind, ownDealing && world != null, world != null && PuzzleChain.GardenHoldsPlots(world.Choices, world.Bought),
                world != null && world.Mode == PlayMode.Living, world != null ? world.Garden : 0);
            boardLeaves = 0; // I336: set by kinds that measure their own logic
            switch (kind)
            {
                case PuzzleKind.Nonogram:
                {
                    var nonogram = ownDealing && world != null ? OwnGardenBoards.NonogramFor(world, seed, tier)
                        : new NonogramGame(seed, tier);
                    boardLeaves = nonogram.Leaves;
                    board = new NonogramBoard(nonogram);
                    break;
                }
                case PuzzleKind.Sudoku:
                {
                    var sudoku = new SeedSudokuGame(seed, tier);
                    boardLeaves = sudoku.Leaves;
                    board = new SudokuBoard(sudoku, pictures(sudoku.Size));
                    break;
                }
                case PuzzleKind.Plots: board = new PlotsBoard(new PlotsGame(seed, tier)); break;
                case PuzzleKind.StreamPath: board = new StreamBoard(new StreamPathGame(seed, tier)); break;
                case PuzzleKind.Sowing: board = new SowBoard(seed, tier); break;            // I318
                case PuzzleKind.Sums: // I324
                {
                    var sums = new SeedSumsGame(seed, tier);
                    boardLeaves = sums.Leaves;
                    board = new SumsBoard(sums);
                    break;
                }
                case PuzzleKind.Pieces: board = new PiecesBoard(new GardenPiecesGame(seed, tier), tier); break; // I331
                case PuzzleKind.Doorstep: // I332
                {
                    var doorstep = new DoorstepGame(seed, tier);
                    boardLeaves = doorstep.Leaves;
                    board = new DoorstepBoard(doorstep, tier);
                    break;
                }
                case PuzzleKind.Mahjong: board = new MahjongBoard(new MahjongGame(seed, tier), tier, pictures(MahjongGame.FacesFor(tier))); break;
            }
            if (board != null)
            {
                TreePlay.OpenBoard(host, board, art, name, reward, font, gentleMotion, turned, solved, closed);
                return;
            }
            if (kind == PuzzleKind.Pairs)
            {
                var game = new PairsGame(seed, tier);
                TreePlay.OpenPairs(host, game, pictures(game.Pairs), art, name, reward, font,
                    gentleMotion, turned, solved, closed);
                return;
            }
            if (kind == PuzzleKind.Mosaic)
            {
                // Buildings hold a Mosaic of one of the village's memory paintings.
                string[] paintings = { MosseloraArt.VignetteBridge, MosseloraArt.VignetteTree,
                    MosseloraArt.VignetteFlowers, MosseloraArt.HeroVillage, MosseloraArt.EndingFestival };
                // Your own things are mosaicked in their own picture (item 37); others keep a village painting.
                Sprite picture = ownDealing && art != null ? art : MosseloraArt.Load(paintings[Mathf.Abs(seed) % paintings.Length]);
                if (picture != null)
                {
                    TreePlay.OpenMosaic(host, new MosaicGame(seed, tier), picture, art, name, reward, font,
                        gentleMotion, turned, solved, closed);
                    return;
                }
            }
                        TreePlay.Open(host, TreePuzzles.For(seed, tier, name), art, name, reward, font, gentleMotion,
                turned, solved, closed);
        }

        // Distinct paintings from this garden's catalogue for a Pairs table, starting
        // at a place set by the flower, so each flower deals different pictures.
        private Sprite[] PairsPictures(int count, int item, Func<int, int, Sprite> artOf)
        {
            var found = new System.Collections.Generic.List<Sprite>();
            // I290: the table is dealt from the flowers of the garden's season first, a
            // card for each flower in bloom, starting where this thing points.
            int[] inBloom = GardenCatalog.FlowersOf(world != null ? GardenSeason.For(world) : Season.Spring);
            for (int step = 0; step < inBloom.Length && found.Count < count; step++)
            {
                int flower = inBloom[(Mathf.Abs(item) * 3 + step) % inBloom.Length];
                Sprite art = MosseloraArt.Load(GardenCatalog.Get(VillageChoice.Flowers, flower).Art);
                if (art != null && !found.Contains(art)) found.Add(art);
            }
            int total = GardenCatalog.ChaptersPerGarden * 3;
            for (int step = 0; step < total && found.Count < count; step++)
            {
                int slot = (item * 5 + step) % total;
                Sprite art = artOf(slot / 3, slot % 3);
                if (art != null && !found.Contains(art)) found.Add(art);
            }
            return found.ToArray();
        }

        // ---- I215: family parcels, over home (or the page when on a board).
        private FamilySheet familySheet;
    }
}
