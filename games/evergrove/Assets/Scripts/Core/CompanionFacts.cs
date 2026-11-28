using System;
using System.Collections.Generic;
using System.Globalization;

namespace Evergrove.Core
{
    /// <summary>
    /// Smart companion (spec: docs/specs/proposals/smart-companion-2026-11-27.md): the kinds of
    /// true thing a neighbour can say. Every kind is built only from the save and the context.
    /// </summary>
    public enum FactKind
    {
        FruitGiven,       // you gave this neighbour fruit (I228/I231)
        SolvedTogether,   // a puzzle of their kind solved taking turns (I266)
        WeatherWhileAway, // the weather since your last visit (I359, Mira)
        LetterWaiting,    // a letter in the box, unread on this device (I236, Orin)
        GiftWaiting,      // a cutting from family waiting in the basket (I222)
        HelpedBy,         // a relative helped with one of their kind's puzzles (I226)
        NewArrival,       // a nursery plant of their kind, puzzle still unsolved
        RiverClose,       // one more thing tended and the mist lifts (I225, Mira)
        RiverFurther,     // the river runs further than ever, and the season it brought (I250)
        KindSolved,       // how many puzzles of their kind you have solved (I257)
        WeatherToday,     // rain, wind, heat or snow today (I357/I358)
        PairingToFind,    // a good-neighbour pairing of their kind not yet found (I370)
        Hour,             // the light at dawn, golden hour, dusk or night (I281)
        NewestThing,      // the newest thing of their kind and where it stands (I184)
        VisitorCame,      // the visitor their good neighbours brought: owl, heron, hedgehog (I377)
        HourHabit,        // the hour you usually come, by its noticer (I419)
        // SC04: true sources the old chain never voiced. They speak only through the reviewed bank.
        WishProgress,     // a neighbour's hope, partly come true, with its true count (I313)
        SeasonTurned,     // the season the garden is in now, once per season turn (I250/I251)
        YieldGiven,       // what this season gave: seedling, seeds, fruit or jam (I388-I391)
        PairingFound,     // a good-neighbour pairing you have found (I369/I383)
        RainComing,       // rain upstream today, reaching the garden tomorrow: Orin (checklist 48)
    }

    /// <summary>
    /// One true fact a neighbour could mention. Immutable. <see cref="Keeper"/> is 0 Mira (trees),
    /// 1 Orin (built things), 2 Fen (flowers) or -1 for anyone. <see cref="Key"/> is a stable
    /// identity for the device-local memory; <see cref="Slots"/> are the values the words use.
    /// </summary>
    public sealed class Fact
    {
        public const int AnyKeeper = -1;

        public FactKind Kind { get; }
        public int Keeper { get; }
        public string Key { get; }
        public IReadOnlyDictionary<string, string> Slots { get; }
        /// <summary>Base weight from the spec's table, before the ranker adds "new" and "their own line".</summary>
        public int Weight { get; }
        /// <summary>Still waiting for the player (an unread letter, a cutting in the basket): the 3-day rule doesn't hide it.</summary>
        public bool Waiting { get; }
        /// <summary>Caused just now, this visit (fruit handed over, a puzzle solved together): said straight away.</summary>
        public bool Fresh { get; }
        /// <summary>Where today's hand-ordered chain (EvergroveApp.NeighborSays before SC01) put it: lower first.</summary>
        public int LegacyOrder { get; }
        /// <summary>SC04: news that is said once (while remembered), not every few days: a season turned, a pairing found.</summary>
        public bool Once { get; }

        public Fact(FactKind kind, int keeper, string key, IReadOnlyDictionary<string, string> slots, int weight,
            bool waiting, bool fresh, int legacyOrder, bool once = false)
        {
            Once = once;
            if (keeper < AnyKeeper || keeper > 2) throw new ArgumentOutOfRangeException(nameof(keeper));
            if (string.IsNullOrEmpty(key)) throw new ArgumentException("A fact needs a key.", nameof(key));
            Kind = kind;
            Keeper = keeper;
            Key = key;
            Slots = slots ?? new Dictionary<string, string>();
            Weight = weight;
            Waiting = waiting;
            Fresh = fresh;
            LegacyOrder = legacyOrder;
        }

        public string Slot(string name)
        {
            return Slots.TryGetValue(name, out string value) ? value : null;
        }

        public int IntSlot(string name)
        {
            string value = Slot(name);
            return value != null && int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int n) ? n : 0;
        }

        public int IntSlotOr(string name, int otherwise)
        {
            string value = Slot(name);
            return value != null && int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int n) ? n : otherwise;
        }

        public T EnumSlot<T>(string name) where T : struct
        {
            return Enum.TryParse(Slot(name), out T value) ? value : default(T);
        }

        public bool For(int speaker)
        {
            return Keeper == AnyKeeper || Keeper == speaker;
        }

        public override string ToString()
        {
            return Kind + "(" + Keeper + "," + Key + ")";
        }
    }

    /// <summary>
    /// What the save doesn't hold: the local day, today's weather and light, the last return,
    /// letters read on this device, and what just happened this visit. Immutable.
    /// </summary>
    public sealed class CompanionContext
    {
        public DateTime Today { get; }
        /// <summary>The last day this garden was opened before today, while Mira hasn't yet said what the weather did. Null otherwise.</summary>
        public DateTime? LastSeen { get; }
        public int LettersRead { get; }
        public Weather Weather { get; }
        public bool LightOn { get; }
        public DayPhase Phase { get; }
        /// <summary>The neighbour just handed fruit this visit, or -1.</summary>
        public int FruitJustGivenTo { get; }
        /// <summary>The keeper of a puzzle just solved taking turns, or -1.</summary>
        public int TogetherKeeper { get; }
        public PuzzleKind TogetherKind { get; }
        /// <summary>The painted map, for where things stand; null when unavailable.</summary>
        public GardenMap Map { get; }
        /// <summary>I419: device-only counts of the hour (phase) you come, or null.</summary>
        public IReadOnlyList<int> HourCounts { get; }

        public CompanionContext(DateTime today, DateTime? lastSeen = null, int lettersRead = 0,
            Weather weather = Weather.Fair, bool lightOn = false, DayPhase phase = DayPhase.Day,
            int fruitJustGivenTo = -1, int togetherKeeper = -1, PuzzleKind togetherKind = PuzzleKind.Water,
            GardenMap map = null, int[] hourCounts = null)
        {
            HourCounts = hourCounts == null ? null : Array.AsReadOnly((int[])hourCounts.Clone());
            Today = today.Date;
            LastSeen = lastSeen;
            LettersRead = Math.Max(0, lettersRead);
            Weather = weather;
            LightOn = lightOn;
            Phase = phase;
            FruitJustGivenTo = fruitJustGivenTo;
            TogetherKeeper = togetherKeeper;
            TogetherKind = togetherKind;
            Map = map;
        }

        /// <summary>Days since 2000-01-01 for the local date: the ranker's and memory's day number.</summary>
        public int DayNumber { get { return DayNumberOf(Today); } }

        public static int DayNumberOf(DateTime date)
        {
            return (int)(date.Date - new DateTime(2000, 1, 1)).TotalDays;
        }
    }

    /// <summary>
    /// The fact feed (SC01): every true thing the neighbours could say right now, from the save and
    /// the context only. Pure, deterministic, no clock and no randomness. Ordering and wording are
    /// left to <see cref="CompanionRanker"/> and <see cref="CompanionSpeech"/>.
    /// </summary>
    public static class CompanionFacts
    {
        // Base weights (spec §2). "New" (+30) and "their own line of work" (+10) are the ranker's.
        public const int WaitingWeight = 50;
        public const int RareWeight = 20;
        public const int OwnDoingWeight = 15;
        public const int AmbientWeight = 5;

        // The old chain's order, kept so SC01 can prove nothing visible changed.
        private const int Step = 100000; // room for an order within a step (KindSolved: up to 10,000)

        public static IReadOnlyList<Fact> For(WorldModel world, CompanionContext context)
        {
            var facts = new List<Fact>();
            if (world == null || context == null || world.Mode != PlayMode.Living) return facts;
            for (int v = 0; v < 3; v++)
            {
                FruitGiven(world, context, v, facts);
                if (context.TogetherKeeper == v)
                    facts.Add(Make(FactKind.SolvedTogether, v, "together:" + context.TogetherKind + ":" + world.PuzzlesSolved,
                        OwnDoingWeight + WaitingWeight, false, true, 1 * Step, "puzzle", context.TogetherKind.ToString(),
                        "work", WorkOf(context.TogetherKind)));
                Gifts(world, v, facts);
                Arrival(world, v, facts);
                Solved(world, v, facts);
                Pairings(world, v, facts);
                Visitor(world, context, v, facts);
                Habit(context, v, facts);
                Newest(world, context, v, facts);
            }
            WhileAway(world, context, facts);
            for (int v = 0; v < 3; v++) Wish(world, v, facts);
            SeasonTurn(world, facts);
            Yield(world, facts);
            Found(world, facts);
            Letter(world, context, facts);
            Helped(world, facts);
            River(world, facts);
            if (context.Weather != Weather.Fair)
                facts.Add(Make(FactKind.WeatherToday, Fact.AnyKeeper,
                    "weather:" + context.Weather, AmbientWeight, false, false, 10 * Step,
                    "weather", context.Weather.ToString()));
            if (RiverWeather.RainComing(context.Today, world.Garden, GardenSeason.For(world)))
                facts.Add(Make(FactKind.RainComing, 1, "rain-coming:" + context.Today.ToString("yyyyMMdd", CultureInfo.InvariantCulture),
                    AmbientWeight, false, false, 10 * Step + 60, LegacyOffSlot, "1"));
            if (context.LightOn && DayLight.Remark(0, context.Phase, GardenSeason.For(world)) != null)
                facts.Add(Make(FactKind.Hour, Fact.AnyKeeper, "hour:" + context.Phase, AmbientWeight, false, false, 12 * Step,
                    "phase", context.Phase.ToString(), "season", GardenSeason.For(world).ToString()));
            return facts;
        }

        private static void FruitGiven(WorldModel world, CompanionContext context, int v, List<Fact> facts)
        {
            if (world.LastFruitTo != v || world.FruitGivenTo(v) <= 0) return;
            bool fresh = context.FruitJustGivenTo == v;
            facts.Add(Make(FactKind.FruitGiven, v, "fruit:" + v + ":" + world.FruitGivenTo(v),
                OwnDoingWeight + (fresh ? WaitingWeight : 0), false, fresh, fresh ? 0 : 8 * Step));
        }

        private static void WhileAway(WorldModel world, CompanionContext context, List<Fact> facts)
        {
            if (context.LastSeen == null || world.Choices.Length == 0) return;
            DateTime last = context.LastSeen.Value.Date;
            string weather = DayWeather.WhileAway(last, context.Today, GardenSeason.For(world), world.Garden, out int days);
            if (weather == null) return;
            // I420: after a long time away, reassurance first, never guilt.
            string welcome = Companion.WelcomeBack(days);
            var slots = new List<string> { "weather", weather, "days", Num(days) };
            if (welcome != null) { slots.Add("welcome"); slots.Add(welcome); }
            // v1 #90: after 90 days or more, a short letter about this garden replaces the weather report.
            string[] letter = Companion.AbsenceLetter(world, days);
            if (letter != null) { slots.Add("letter"); slots.Add(string.Join(" ", letter)); }
            facts.Add(Make(FactKind.WeatherWhileAway, 0, "away:" + last.ToString("yyyyMMdd", CultureInfo.InvariantCulture),
                RareWeight, false, false, 2 * Step, slots.ToArray()));
        }

        private static void Letter(WorldModel world, CompanionContext context, List<Fact> facts)
        {
            string[] mail = world.Letters;
            if (mail.Length == 0 || context.LettersRead >= mail.Length) return;
            string name = world.LetterFrom[mail.Length - 1];
            facts.Add(Make(FactKind.LetterWaiting, 1, "letter:" + name + ":" + mail.Length, WaitingWeight, true, false,
                3 * Step, "name", name));
        }

        private static void Gifts(WorldModel world, int v, List<Fact> facts)
        {
            int[] gifts = world.Gifts;
            var said = new HashSet<int>();
            for (int i = 0; i < gifts.Length; i++)
            {
                int kind = gifts[i];
                if ((int)Nursery.Family(kind) != v || !said.Add(kind)) continue;
                facts.Add(Make(FactKind.GiftWaiting, v, "gift:" + kind, WaitingWeight, true, false, 4 * Step + i,
                    "thing", Nursery.Item(kind).Name));
            }
        }

        private static void Helped(WorldModel world, List<Fact> facts)
        {
            int[] helped = world.Helped;
            if (helped.Length == 0) return;
            int item = helped[helped.Length - 1];
            int[] made = world.Choices;
            if (item >= made.Length || world.TreeSolved(item)) return;
            PuzzleKind kind = ThingPuzzle.ForStory(item, (VillageChoice)Math.Max(0, Math.Min(2, made[item])));
            facts.Add(Make(FactKind.HelpedBy, NeighbourNotice.OwnerOf(kind), "helped:" + item, WaitingWeight, true, false,
                5 * Step, "name", world.HelpedBy(item), "puzzle", kind.ToString(), "work", WorkOf(kind), "item", Num(item)));
        }

        private static void Arrival(WorldModel world, int v, List<Fact> facts)
        {
            int[] bought = world.Bought;
            for (int k = bought.Length / 2 - 1; k >= 0; k--)
            {
                int kind = bought[k * 2];
                if ((int)Nursery.Family(kind) != v) continue;
                if (!world.BoughtSolved(k))
                    facts.Add(Make(FactKind.NewArrival, v, "arrival:" + k, OwnDoingWeight, false, false, 6 * Step,
                        "thing", Nursery.Item(kind).Name, "bought", Num(k)));
                return;
            }
        }

        private static void River(WorldModel world, List<Fact> facts)
        {
            if (!world.Completed) return;
            int all = world.PuzzlesSolved;
            int reaches = GardenMap.ReachesFor(world.Choices.Length, true, all, world.RiverReplays);
            if (world.TowardNextStretch(out int have, out int need) && have == need - 1)
                facts.Add(Make(FactKind.RiverClose, 0, "mist:" + reaches, RareWeight, false, false, 7 * Step));
            else if (have == 0 && reaches > GardenMap.MaxReaches)
            {
                Season season = GardenSeason.AfterStory(all);
                facts.Add(Make(FactKind.RiverFurther, 0, "river:" + reaches, RareWeight, false, false, 7 * Step,
                    "season", season.ToString()));
            }
        }

        // I257: by puzzle kind. The old chain said only the kind with most solves (first on ties).
        private static void Solved(WorldModel world, int v, List<Fact> facts)
        {
            PuzzleKind[] kept = NeighbourNotice.KeptBy(v);
            for (int i = 0; i < kept.Length; i++)
            {
                int n = NeighbourNotice.SolvedOf(world, kept[i]);
                if (n <= 0) continue;
                int rank = (Math.Max(0, 499 - Math.Min(n, 499))) * 20 + i;
                facts.Add(Make(FactKind.KindSolved, v, "solved:" + kept[i] + ":" + n, OwnDoingWeight, false, false,
                    9 * Step + rank, "puzzle", kept[i].ToString(), "count", Num(n), "work", WorkOf(kept[i])));
            }
        }

        // I370/I383: their own kind's good neighbours open and not yet found, in the old order.
        private static readonly Pairing[][] PairingsOf = {
            new[] { Pairing.Grove, Pairing.Orchard },
            new[] { Pairing.ByTheWater, Pairing.ShadySeat },
            new[] { Pairing.Sheltered, Pairing.FlowerBed, Pairing.Waterside },
        };

        private static void Pairings(WorldModel world, int v, List<Fact> facts)
        {
            Pairing[] theirs = PairingsOf[v];
            int open = Evergrove.Core.Pairings.Open(world);
            for (int i = 0; i < theirs.Length; i++)
                if (!world.HasPairing(theirs[i]) && (open & PairingMask.Of(theirs[i])) != 0)
                    facts.Add(Make(FactKind.PairingToFind, v, "pairing-hint:" + theirs[i], AmbientWeight, false, false,
                        11 * Step + i, "pairing", theirs[i].ToString()));
        }

        /// <summary>Slot marking a fact the old chain would not have said today (it kept its own "one day in three").</summary>
        public const string LegacyOffSlot = "_legacy-off";

        // I377: their visitor has come. The old chain said it one day in three; the ranker's
        // 3-day rule now keeps it from becoming a refrain.
        private static void Visitor(WorldModel world, CompanionContext context, int v, List<Fact> facts)
        {
            Visitor visitor = Visitors.KeeperVisitor(v);
            if (!Visitors.Welcomes(world, visitor)) return;
            bool legacyDay = Visitors.KeeperRemark(world, v, context.DayNumber) != null;
            var slots = new List<string> { "visitor", VisitorName(visitor) };
            if (!legacyDay) { slots.Add(LegacyOffSlot); slots.Add("1"); }
            facts.Add(Make(FactKind.VisitorCame, v, "visitor:" + visitor, RareWeight, false, false, 11 * Step + 50,
                slots.ToArray()));
        }

        // I419: the hour you usually come, said by its noticer. The old chain said it one day in seven.
        private static void Habit(CompanionContext context, int v, List<Fact> facts)
        {
            if (context.HourCounts == null) return;
            int[] counts = new int[context.HourCounts.Count];
            for (int i = 0; i < counts.Length; i++) counts[i] = context.HourCounts[i];
            DayPhase? habit = Companion.Habit(counts);
            if (!habit.HasValue || Companion.Noticer(habit.Value) != v) return;
            var slots = new List<string> { "phase", habit.Value.ToString() };
            if (Companion.HabitLine(counts, v, context.DayNumber) == null) { slots.Add(LegacyOffSlot); slots.Add("1"); }
            facts.Add(Make(FactKind.HourHabit, v, "habit:" + habit.Value, AmbientWeight, false, false, 11 * Step + 80,
                slots.ToArray()));
        }

        // ---- SC04: kinds the old chain never said (LegacyOff, so SC01's parity is untouched).
        private const int NewKindsOrder = 14 * Step;

        // I313: a hope partly come true, with its true count, said once per step.
        private static void Wish(WorldModel world, int v, List<Fact> facts)
        {
            NeighbourWish wish = NeighbourWish.For(world, v);
            if (wish == null || wish.Have <= 0 || wish.Granted) return;
            facts.Add(Once(Make(FactKind.WishProgress, v, "wish:" + v + ":" + wish.Have, OwnDoingWeight, false, false, NewKindsOrder,
                "have", Words(wish.Have), "need", Words(wish.Need), LegacyOffSlot, "1")));
        }

        // The season now, once per season turn (never the garden's first season: that wasn't a turn).
        private static void SeasonTurn(WorldModel world, List<Fact> facts)
        {
            int key = SeasonalYields.Key(world);
            if (key <= 0 || world.Choices.Length == 0) return;
            facts.Add(Once(Make(FactKind.SeasonTurned, Fact.AnyKeeper, "season-turn:" + key, RareWeight, false, false, NewKindsOrder,
                "season", GardenSeason.For(world).ToString(), LegacyOffSlot, "1")));
        }

        // I388-I391: this season's yield was taken (the save proves it): its keeper says so, once.
        // Winter's jam came from whoever you gave fruit to; the save doesn't keep who, so anyone says it.
        private static void Yield(WorldModel world, List<Fact> facts)
        {
            int key = SeasonalYields.Key(world);
            if (key < 0 || world.YieldTaken != key + 1) return;
            Season season = GardenSeason.For(world);
            int keeper = season == Evergrove.Core.Season.Spring ? 2 : season == Evergrove.Core.Season.Winter ? Fact.AnyKeeper : 0;
            facts.Add(Once(Make(FactKind.YieldGiven, keeper, "yield:" + key, RareWeight, false, false, NewKindsOrder,
                "season", season.ToString(), LegacyOffSlot, "1")));
        }

        // I369/I383: each good neighbour found, by the keeper of its kind, once.
        private static void Found(WorldModel world, List<Fact> facts)
        {
            for (int v = 0; v < PairingsOf.Length; v++)
                foreach (Pairing p in PairingsOf[v])
                    if (world.HasPairing(p))
                        facts.Add(Once(Make(FactKind.PairingFound, v, "pairing-found:" + p, RareWeight, false, false, NewKindsOrder,
                            "pairing", p.ToString(), LegacyOffSlot, "1")));
        }

        private static Fact Once(Fact f)
        {
            return new Fact(f.Kind, f.Keeper, f.Key, f.Slots, f.Weight, f.Waiting, f.Fresh, f.LegacyOrder, true);
        }

        private static readonly string[] NumberWords = { "no", "one", "two", "three", "four", "five", "six", "seven", "eight", "nine", "ten" };

        /// <summary>Small counts as words ("five"), larger as digits: the bank's {have}/{need} slots.</summary>
        public static string Words(int n)
        {
            return n >= 0 && n < NumberWords.Length ? NumberWords[n] : Num(n);
        }

        public static string VisitorName(Visitor visitor)
        {
            switch (visitor)
            {
                case Evergrove.Core.Visitor.Owl: return "owl";
                case Evergrove.Core.Visitor.Heron: return "heron";
                case Evergrove.Core.Visitor.Hedgehog: return "hedgehog";
                case Evergrove.Core.Visitor.Kingfisher: return "kingfisher";
                case Evergrove.Core.Visitor.Bees: return "bees";
                case Evergrove.Core.Visitor.Birds: return "birds";
                case Evergrove.Core.Visitor.Moths: return "moths";
                case Evergrove.Core.Visitor.Frogs: return "frogs";
                default: return "butterflies";
            }
        }

        private static void Newest(WorldModel world, CompanionContext context, int v, List<Fact> facts)
        {
            int[] made = world.Choices;
            int newest = -1, count = 0;
            for (int i = 0; i < made.Length; i++)
                if (made[i] == v) { newest = i; count++; }
            if (newest < 0) return;
            int chapter = Math.Max(0, Math.Min(GardenCatalog.ChaptersPerGarden - 1, newest));
            string thing = GardenCatalog.For(world.Garden, chapter, (VillageChoice)v).Name;
            facts.Add(Make(FactKind.NewestThing, v, "newest:" + newest + ":" + count, 0, false, false, 13 * Step,
                "thing", thing, "where", Where(context.Map, world, newest), "count", Num(count)));
        }

        /// <summary>The line bank's {work} slot: a puzzle kind as a plain noun phrase ("the seed sums").</summary>
        public static string WorkOf(PuzzleKind kind)
        {
            switch (kind)
            {
                case PuzzleKind.Water: return "the water";
                case PuzzleKind.StreamPath: return "the stream";
                case PuzzleKind.Nonogram: return "the numbered bed";
                case PuzzleKind.Mosaic: return "the old painting";
                case PuzzleKind.Plots: return "the plots";
                case PuzzleKind.Mahjong: return "the garden tiles";
                case PuzzleKind.Pairs: return "the flower beds";
                case PuzzleKind.Sowing: return "the sowing";
                case PuzzleKind.Sums: return "the seed sums";
                case PuzzleKind.Pieces: return "the garden pieces";
                case PuzzleKind.Doorstep: return "the doorstep pattern";
                default: return "the seed squares";
            }
        }

        /// <summary>Where a grown thing stands on the painted map (I184): "by the creek", "by the cottage", ...; "" when unknown.</summary>
        public static string Where(GardenMap map, WorldModel world, int index)
        {
            if (map == null) return "";
            int[] cells = map.Layout(world.Choices, world.Placements);
            if (index < 0 || index >= cells.Length || cells[index] < 0) return "";
            int cell = cells[index];
            int row = cell / map.Columns, column = cell % map.Columns;
            return map.DistanceToWater(cell) <= 1 ? "by the creek"
                : row <= 3 && column >= 5 ? "by the cottage"
                : row >= map.Rows - 4 ? "down the valley" : "in the meadow";
        }

        private static string Num(int n)
        {
            return n.ToString(CultureInfo.InvariantCulture);
        }

        private static Fact Make(FactKind kind, int keeper, string key, int weight, bool waiting, bool fresh, int legacy,
            params string[] slots)
        {
            var map = new Dictionary<string, string>();
            for (int i = 0; i + 1 < slots.Length; i += 2) map[slots[i]] = slots[i + 1];
            return new Fact(kind, keeper, key, map, weight, waiting, fresh, legacy);
        }
    }
}
