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

        private System.Collections.Generic.List<RectTransform> NeighborsOnMap(RectTransform land,
            Func<int, Vector2> point, GardenMap map, bool fromTitle)
        {
            var placed = new System.Collections.Generic.List<RectTransform>();
            if (world.Mode != PlayMode.Living) return placed;
            int[] made = world.Choices;
            int[] cells = map.Layout(made, world.Placements);
            // Before they have a thing of their own, each waits in a different
            // part of the valley (land cells in the mask), never in a huddle.
            // Row 2 col 5 (Mira), row 3 col 6 (Orin), row 6 col 7 (Fen): clear of the
            // corner buttons and of each other, on land.
            int[] byCottage = { 25, 36, 67 };
            Vector2 step = point(1) - point(0);
            // Those with a thing of their own take their place first; newcomers fit around them.
            var order = new System.Collections.Generic.List<int> { 0, 1, 2 };
            order.Sort((a, b) => (Array.IndexOf(made, b) >= 0).CompareTo(Array.IndexOf(made, a) >= 0) != 0
                ? (Array.IndexOf(made, b) >= 0).CompareTo(Array.IndexOf(made, a) >= 0) : a.CompareTo(b));
            foreach (int villager in order)
            {
                // I301: beside the newest thing of their kind, a nursery plant of theirs first.
                int cell = -1;
                int beside = NeighbourNotice.StandBy(world, villager, out bool besideBought);
                int[] boughtNow = world.Bought;
                if (beside >= 0 && besideBought && beside * 2 + 1 < boughtNow.Length && map.IsLand(boughtNow[beside * 2 + 1]))
                    cell = boughtNow[beside * 2 + 1];
                else
                    for (int i = made.Length - 1; i >= 0 && cell < 0; i--)
                        if (made[i] == villager) cell = cells[i];
                Vector2 at = cell >= 0
                    // I397 (b320 tour: Fen stood half on her birch): a painting is wider than its
                    // cell, so they stand a step and a quarter beside it, toward the middle.
                    ? point(cell) + new Vector2(step.x * (cell % map.Columns < map.Columns / 2 ? 1.25f : -1.25f), 0f)
                    : point(Mathf.Min(byCottage[villager], map.CellCount - 1));
                // Nothing of their own yet: they come to see the newest thing you made,
                // which the map always opens on, so they are in view (I187).
                float rowStep = Mathf.Abs((point(map.Columns) - point(0)).y);
                if (cell < 0 && made.Length > 0 && cells[made.Length - 1] >= 0)
                {
                    // Try the spots around it in turn: either side, then a step up.
                    Vector2 newestAt = point(cells[made.Length - 1]);
                    // Toward the middle of the valley first, so nobody stands off the edge (I188).
                    float inward = cells[made.Length - 1] % map.Columns < map.Columns / 2 ? 1f : -1f;
                    Vector2[] around = {
                        new Vector2(inward * 1.15f * step.x, 0f), new Vector2(inward * 2.5f * step.x, 0f),
                        new Vector2(inward * 1.2f * step.x, -1.6f * rowStep), new Vector2(inward * 1.2f * step.x, 1.6f * rowStep),
                        new Vector2(inward * 3.8f * step.x, 0f), new Vector2(-inward * 1.15f * step.x, 0f) };
                    at = newestAt + around[0];
                    foreach (Vector2 offset in around)
                    {
                        Vector2 spot = newestAt + offset;
                        bool taken = false;
                        foreach (RectTransform other in placed)
                            if (Mathf.Abs(NeighbourSpot(other).x - spot.x) < Mathf.Abs(step.x) * 1.3f &&
                                Mathf.Abs(NeighbourSpot(other).y - spot.y) < rowStep * 1.3f) taken = true;
                        if (!taken) { at = spot; break; }
                    }
                }
                // I402: in the golden hour they sit by the shady seat, if the garden has one.
                if (fromTitle && DayLightOn && CurrentPhase() == DayPhase.Golden && world.HasPairing(Pairing.ShadySeat))
                {
                    int seatCell = Pairings.SpotFor(map, Pairings.Things(world, map), Pairing.ShadySeat);
                    if (seatCell >= 0)
                    {
                        float side = villager == 0 ? -1.3f : villager == 1 ? 1.3f : 0f;
                        at = point(seatCell) + new Vector2(side * step.x, villager == 2 ? -1.4f * rowStep : 0f);
                    }
                }
                // I367: in the evening they gather on the path below the lamplit cottage door.
                if (fromTitle && DayLightOn && DayLight.NeighboursGather(CurrentPhase()))
                {
                    int[] byDoor = { 2 * map.Columns + 7, 3 * map.Columns + 8, 2 * map.Columns + 5 };
                    int spot = byDoor[villager % byDoor.Length];
                    // I434 (P27, b324: Fen stood on the wisteria at dusk): a spot where a thing
                    // stands moves to the nearest free one beside it.
                    var thingCells = new System.Collections.Generic.HashSet<int>();
                    foreach (PlacedThing t in Pairings.Things(world, map)) thingCells.Add(t.Cell);
                    int[] steps = { 0, 1, -1, map.Columns, -map.Columns, 2, -2 };
                    foreach (int d in steps)
                    {
                        int tryCell = spot + d;
                        if (tryCell < 0 || tryCell >= map.CellCount || thingCells.Contains(tryCell) || !map.IsLand(tryCell)) continue;
                        spot = tryCell;
                        break;
                    }
                    if (spot < map.CellCount) at = point(spot);
                }
                // Never stand on another neighbour (I184): step down a row and a half
                // until clear of everyone already placed.
                for (int tries = 0; tries < 4; tries++)
                {
                    bool clash = false;
                    foreach (RectTransform other in placed)
                        if (Mathf.Abs(NeighbourSpot(other).x - at.x) < Mathf.Abs(step.x) * 1.3f &&
                            Mathf.Abs(NeighbourSpot(other).y - at.y) < rowStep * 1.3f) clash = true;
                    if (!clash) break;
                    at.y -= rowStep * 1.5f;
                }
                // Checklist 24: a neighbour whose spot moved since the last hub view walks there.
                bool walking = NeighbourStartFor(villager, at, fromTitle, out Vector2 walkFrom, out float walkSince);
                var holder = Node("Neighbour on the map", land);
                var rect = holder.GetComponent<RectTransform>();
                rect.anchorMin = rect.anchorMax = new Vector2(0f, 1f);
                rect.pivot = new Vector2(0.5f, 0f);
                rect.anchoredPosition = walking ? NeighbourWalker.PoseAt(walkFrom, at, walkSince) : at;
                // A fixed footprint (no nested size fitters) keeps face and name centred.
                rect.sizeDelta = new Vector2(TextScale.Scale(96), 62f + TextScale.Scale(30));
                var stack = holder.AddComponent<VerticalLayoutGroup>();
                stack.childAlignment = TextAnchor.LowerCenter;
                stack.childControlWidth = true;
                stack.childControlHeight = true;
                stack.childForceExpandWidth = true;
                stack.childForceExpandHeight = true;
                PortraitButton(holder.transform, villager, true, fromTitle);
                if (walking)
                {
                    var walker = holder.AddComponent<NeighbourWalker>(); // the walk replaces this time's hop
                    walker.From = walkFrom;
                    walker.To = at;
                    walker.StartedAt = walkSince;
                }
                // I298 (idea 35, presence without words): when you come back, they look up and
                // give a little hop, one after another. Once per return; never with reduced motion.
                else if (fromTitle && waveHello && gentleMotion)
                    holder.AddComponent<HelloHop>().Delay = 0.35f + placed.Count * 0.28f;
                // I424: the keeper whose good neighbour was just found gives a little hop of joy.
                else if (fromTitle && celebrateKeeper == villager && gentleMotion)
                    holder.AddComponent<HelloHop>().Delay = 0.6f;
                placed.Add(rect);
            }
            return placed;
        }

        // What a neighbour says. Smart companion: the true facts of the save and the day
        // (CompanionFacts, SC01), the one worth saying now (CompanionRanker, SC02), in today's
        // words (I184, I210, I236, I266, I281, I357–I359, I370). With nothing worth saying, or
        // outside English, their authored lines (LocalizedVillage).
        private string NeighborSays(int villager)
        {
            if (SpeakLessSilences(villager)) return ""; // idea 85: speak less keeps half the remarks
            string echo = ReplyEchoSaid(villager); // checklist 81: their answer to a reply, once, within three days
            if (echo != null) return echo;
            string arc = ArcRemark(villager); // idea 83: a neighbour's project, when its step is news
            if (arc != null) return arc;
            Fact pick = CompanionPick(villager);
            if (pick == null) return LocalizedVillage.Speak(world, villager);
            return CompanionWords(pick, villager, out _) ?? LocalizedVillage.Speak(world, villager); // never a blank card
        }

        // SC03: the reviewed line bank's words where it has them (a wording not heard in 14 days,
        // kept for the rest of the visit), else today's sentence.
        private string CompanionWords(Fact pick, int villager, out int variant)
        {
            int keep = spokenThisVisit.TryGetValue(villager, out string key) && key == pick.Key &&
                spokenVariant.TryGetValue(villager, out int kept) ? kept : -1;
            // SC06: English falls back to today's sentence; another language speaks only its reviewed bank.
            return CompanionLocale.Words(GameLocalization.Current, LineBankNow(), pick, villager, world.Garden,
                CompanionDayNow(), CompanionMemoryNow(), keep, out variant); // CP42: the companion's day
        }

        // SC03: one bank per locale (Resources/Evergrove/Lines/<locale>.json), kept apart from
        // GameLocalization's catalogue. A missing or unreadable bank is an empty one.
        private LineBank lineBank;
        private string lineBankLocale;

        private LineBank LineBankNow()
        {
            string locale = GameLocalization.Current;
            if (lineBank != null && lineBankLocale == locale) return lineBank;
            lineBankLocale = locale;
            lineBank = LineBank.Empty;
            var asset = Resources.Load<TextAsset>("Evergrove/Lines/" + locale);
            if (asset != null)
            {
                try { lineBank = LineBank.Parse(asset.text); }
                catch (FormatException error) { Debug.LogWarning("Evergrove line bank " + locale + " unreadable: " + error.Message); }
            }
            return lineBank;
        }

        // ---- SC02: device-local memory of what was said (PlayerPrefs; never the save or the garden file).
        private readonly System.Collections.Generic.Dictionary<int, string> spokenThisVisit =
            new System.Collections.Generic.Dictionary<int, string>();
        private readonly System.Collections.Generic.Dictionary<int, int> spokenVariant =
            new System.Collections.Generic.Dictionary<int, int>(); // SC03: the wording said this visit
        private int visitGarden = int.MinValue;
        private int visitDay = int.MinValue; // CP42: the companion's day the visit lock belongs to

        private Fact CompanionPick(int villager)
        {
            // SC06: a language without a reviewed bank keeps the story's own lines (LocalizedVillage), as before.
            if (world == null || world.Mode != PlayMode.Living || !CompanionLocale.Speaks(GameLocalization.Current, LineBankNow())) return null;
            // CP42: the visit lock is kept for the companion's day, which turns at 4 a.m., not the calendar day.
            CompanionContext context = CompanionNow();
            if (visitGarden != world.Garden || visitDay != context.DayNumber) { spokenThisVisit.Clear(); spokenVariant.Clear(); visitGarden = world.Garden; visitDay = context.DayNumber; }
            spokenThisVisit.TryGetValue(villager, out string spoken);
            return CompanionRanker.Pick(CompanionFactsNow(), CompanionMemoryNow(), villager,
                context.DayNumber, world.Garden, spoken, CompanionCanSay); // SC04/SC06
        }

        // CP40: the sayable check the pick and the QA explain share, so the log reads the same rule.
        private bool CompanionCanSay(Fact fact, int who)
        {
            return CompanionLocale.CanSay(GameLocalization.Current, LineBankNow(), fact, who);
        }

        // CP38: the parsed memory is reused while its storage key and raw PlayerPrefs string are unchanged.
        // CompanionMemory is immutable, so sharing the instance is safe. A redraw asks for it several times.
        private CompanionMemory companionMemory;
        private string companionMemoryKey;
        private string companionMemoryRaw;

        private CompanionMemory CompanionMemoryNow()
        {
            string key = CompanionMemory.StorageKey(world.Garden, GameLocalization.Current);
            string raw = PlayerPrefs.GetString(key, "");
            if (companionMemory != null && key == companionMemoryKey && raw == companionMemoryRaw) return companionMemory;
            companionMemory = CompanionMemory.Parse(raw);
            companionMemoryKey = key;
            companionMemoryRaw = raw;
            return companionMemory;
        }

        // Called when a conversation opens: the remark is now heard, and is theirs for this visit.
        private void CompanionHeard(int villager, Fact said)
        {
            if (said == null || world == null) return;
            CompanionWords(said, villager, out int variant);
            spokenThisVisit[villager] = said.Key;
            spokenVariant[villager] = variant;
            CompanionMemory memory = CompanionMemoryNow().With(
                new Heard(CompanionDayNow(), villager, said.Kind, variant, said.Key)); // CP42
            string key = CompanionMemory.StorageKey(world.Garden, GameLocalization.Current);
            string raw = memory.Serialize();
            PlayerPrefs.SetString(key, raw);
            // CP38: the next read hits the cache rather than re-parsing what was just written.
            companionMemory = memory;
            companionMemoryKey = key;
            companionMemoryRaw = raw;
        }

        // CP41: a reset forgets this garden's companion memory in every language, and the visit state
        // and parsed-memory cache that would otherwise still remember it.
        private void ForgetCompanion(int garden)
        {
            var locales = new System.Collections.Generic.List<string>();
            foreach (GameLanguage language in GameLocalization.Available) locales.Add(language.Code);
            foreach (string key in CompanionMemory.StorageKeysFor(garden, locales)) PlayerPrefs.DeleteKey(key);
            PlayerPrefs.Save();
            companionMemory = null;
            companionMemoryKey = null;
            companionMemoryRaw = null;
            spokenThisVisit.Clear();
            spokenVariant.Clear();
        }

        // CP37: the fact list, built once per rebuild. A redraw asks for it 6-8 times (CompanionPick runs
        // once per neighbour), so the last list is kept while its signature and the frame are unchanged.
        // The signature covers every input For reads, so a stale list cannot be served.
        private System.Collections.Generic.IReadOnlyList<Fact> companionFacts;
        private string companionFactsKey;

        private System.Collections.Generic.IReadOnlyList<Fact> CompanionFactsNow()
        {
            CompanionContext context = CompanionNow();
            string key = CompanionFacts.Signature(world, context) + "#" + Time.frameCount;
            if (companionFacts == null || key != companionFactsKey)
            {
                companionFacts = CompanionFacts.For(world, context);
                companionFactsKey = key;
            }
            return companionFacts;
        }

        // What the companion needs that the save doesn't hold (SC01).
        private CompanionContext CompanionNow()
        {
            bool living = world != null && world.Mode == PlayMode.Living;
            return new CompanionContext(CompanionToday(), localNow: CompanionClock(), // CP42: facts read the date, the companion the clock
                lastSeen: sinceSeen,
                lettersRead: PlayerPrefs.GetInt(LettersReadKey, 0),
                weather: TodayWeather(world),
                lightOn: DayLightOn && living,
                phase: CurrentPhase(),
                fruitJustGivenTo: fruitThanks,
                hourCounts: living ? HourCounts() : null,
                togetherKeeper: togetherKeeper,
                togetherKind: togetherKind,
                togetherPlayers: togetherPlayers,
                map: living ? LoadGardenMap() : null);
        }

        // How much of the valley the restored water has greened (I180): the
        // cottage's top rows from the start, then a twelfth per stream restored.
        // `back` counts that many streams fewer (the edge before a choice).
        private float GreenedShare(int back)
        {
            // The river's open reaches are all green; new land arrives as a new
            // reach (I192) rather than as the dry edge drawing back.
            if (world.Completed || world.Mode == PlayMode.Living) return 1f;
            int restored = Mathf.Max(0, world.Choices.Length - back);
            return Mathf.Clamp01(0.14f + 0.86f * restored / (float)GardenCatalog.ChaptersPerGarden);
        }

        // Right after a choice the new thing is in your hand until you put it down.
        private int ItemInHand()
        {
            if (mapInHand >= 0) return mapInHand;
            int[] placed = world.Placements;
            int newest = world.Choices.Length - 1;
            bool justMade = showingMemory && newest >= 0 && (newest >= placed.Length || placed[newest] < 0);
            return justMade ? newest : -1;
        }
    }
}
