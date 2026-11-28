using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using Evergrove.Core;

/// <summary>
/// Smart companion SC03: the line bank's format and loader, and every rule in the spec, enforced
/// on each shipped bank (Assets/Resources/Evergrove/Lines/*.json) and on the review drafts
/// (docs/improvements/review/companion-lines-*.md). The rules live in
/// docs/specs/companion-line-rules.json, shared with tools/evergrove_line_bank_check.py.
/// </summary>
internal static class LineBankTests
{
    private static int assertions;

    private static void Expect(bool ok, string message)
    {
        assertions++;
        if (!ok) throw new Exception("Line bank test failed: " + message);
    }

    public static int Run()
    {
        assertions = 0;
        Rules rules = Rules.Load();
        Loader();
        Filling();
        Wording();
        LineIds();
        RulesCatchBadLines(rules);
        LongestIsLongest(rules);
        FactsFillTheirSlots(rules);
        int shipped = ShippedBanks(rules);
        EnglishSpeaks(rules);
        int drafts = ReviewDrafts(rules);
        Console.WriteLine("Line bank SC03/SC04: " + shipped + " shipped lines and " + drafts + " draft lines pass every rule.");
        return assertions;
    }

    private static void Loader()
    {
        LineBank bank = LineBank.Parse("{ \"FruitGiven\": { \"0\": [\"Thank you.\", \"Thanks, truly.\"], \"any\": [\"Fruit\\u0021\"] }, \"NewestThing\": {} }");
        Expect(bank.Lines(FactKind.FruitGiven, "0").Count == 2, "two lines in Mira's voice");
        Expect(bank.Lines(FactKind.FruitGiven, "any")[0] == "Fruit!", "\\u escapes are read");
        Expect(bank.For(FactKind.FruitGiven, 0)[0] == "Thank you.", "own voice first");
        Expect(bank.For(FactKind.FruitGiven, 2)[0] == "Fruit!", "anyone's voice otherwise");
        Expect(bank.For(FactKind.Hour, 1).Count == 0, "no lines for a kind not in the bank");
        Expect(bank.Lines(FactKind.NewestThing, "1").Count == 0, "an empty kind has no lines");
        Expect(LineBank.Parse("{}").IsEmpty && LineBank.Parse("\uFEFF { } ").IsEmpty, "an empty bank, with or without a BOM");
        // SC04: kinds keyed by a qualifier ("WeatherToday.Rainy"): the fact's slot picks the list.
        LineBank weather = LineBank.Parse("{\"WeatherToday.Rainy\": {\"1\": [\"Rain on the bridge.\"]}, \"WeatherToday.Snowy\": {\"any\": [\"Snow.\"]}}");
        Fact rain = new Fact(FactKind.WeatherToday, -1, "weather:Rainy", new Dictionary<string, string> { { "weather", "Rainy" } }, 5, false, false, 0);
        Fact snow = new Fact(FactKind.WeatherToday, -1, "weather:Snowy", new Dictionary<string, string> { { "weather", "Snowy" } }, 5, false, false, 0);
        Fact wind = new Fact(FactKind.WeatherToday, -1, "weather:Windy", new Dictionary<string, string> { { "weather", "Windy" } }, 5, false, false, 0);
        Expect(weather.Speak(rain, 1, 1, 1, null, out _) == "Rain on the bridge.", "Orin's rain line for rain");
        Expect(!weather.CanSay(rain, 0) && weather.CanSay(snow, 0) && !weather.CanSay(wind, 1), "only the qualifier's own lines, else anyone's");
        Expect(weather.Kinds.Contains(FactKind.WeatherToday) && weather.Keys.Contains("WeatherToday.Snowy"), "qualified keys listed");
        foreach (string text in new[] { "{\"WeatherToday\": {}}x", "{\"FruitGiven.Rainy\": {}}", "{\"WeatherToday.\": {}}", "{\"WeatherToday.Rainy\": {}, \"WeatherToday.Rainy\": {}}" })
        {
            bool threw = false;
            try { LineBank.Parse(text); } catch (FormatException) { threw = true; }
            Expect(threw, "a bad qualified key is refused: " + text);
        }
        Fact away = new Fact(FactKind.WeatherWhileAway, 0, "away:x", new Dictionary<string, string> { { "weather", "rain" }, { "welcome", "It's good to see you." } }, 20, false, false, 0);
        Expect(CompanionSpeech.Compose(away, "We had rain.") == "It's good to see you. We had rain.", "the welcome comes first, as before (I420)");
        Expect(CompanionSpeech.Compose(rain, "Rain.") == "Rain." && CompanionSpeech.Compose(rain, null) == null, "other kinds are spoken as written");
        Expect(LineBank.Empty.IsEmpty, "the empty bank");
        string[] broken = {
            "", "[]", "{", "{\"FruitGiven\": {\"0\": [\"a\"]}", "{\"NotAKind\": {}}", "{\"fruitgiven\": {}}",
            "{\"FruitGiven\": {\"3\": []}}", "{\"FruitGiven\": {\"0\": [1]}}", "{\"FruitGiven\": {}, \"FruitGiven\": {}}",
            "{\"FruitGiven\": {\"0\": [], \"0\": []}}", "{\"FruitGiven\": {}} extra", "{\"FruitGiven\": {\"0\": [\"\\q\"]}}",
        };
        foreach (string text in broken)
        {
            bool threw = false;
            try { LineBank.Parse(text); } catch (FormatException) { threw = true; }
            Expect(threw, "a malformed bank is refused, not half-read: " + text);
        }
    }

    private static void Filling()
    {
        var slots = new Dictionary<string, string> { { "thing", "quince tree" }, { "where", "by the creek" }, { "empty", "" } };
        Expect(LineBank.Fill("Your {thing} {where}.", slots) == "Your quince tree by the creek.", "slots filled");
        Expect(LineBank.Fill("No slots here.", slots) == "No slots here.", "no slots");
        Expect(LineBank.Fill("Your {name}.", slots) == null, "a missing slot refuses the line");
        Expect(LineBank.Fill("Your {empty}.", slots) == null, "an empty slot refuses the line");
        Expect(LineBank.Fill("Your {thing.", slots) == null && LineBank.Fill("Your }thing.", slots) == null, "stray braces refuse the line");
        Expect(string.Join(",", LineBank.SlotsIn("{a} and {b}, {a}")) == "a,b,a", "slots listed in order");
    }

    private static void Wording()
    {
        var lines = new List<string>();
        for (int i = 0; i < 20; i++) lines.Add("Thank you, " + (char)('a' + i) + ".");
        LineBank bank = LineBank.Parse("{\"FruitGiven\": {\"1\": [" + string.Join(",", lines.Select(l => "\"" + l + "\"")) + "]}}");
        Fact fruit = new Fact(FactKind.FruitGiven, 1, "fruit:1:1", new Dictionary<string, string>(), 15, false, false, 0);
        CompanionMemory memory = CompanionMemory.Empty;
        var said = new HashSet<string>();
        for (int day = 0; day < 14; day++)
        {
            string text = bank.Speak(fruit, 1, 7, 2000 + day, memory, out int variant);
            Expect(text != null && variant >= 0, "the bank speaks");
            Expect(bank.Speak(fruit, 1, 7, 2000 + day, memory, out int again) == text && again == variant, "deterministic");
            Expect(said.Add(text), "no wording twice in 14 days: " + text);
            memory = memory.With(new Heard(2000 + day, 1, FactKind.FruitGiven, variant, fruit.Key));
            Expect(bank.Speak(fruit, 1, 7, 2000 + day, memory, variant, out int kept) == text && kept == variant,
                "the wording said this visit is kept");
        }
        Expect(bank.Speak(fruit, 0, 7, 2000, memory, out int none) == null && none == -1, "no lines in Mira's voice: none (today's sentence)");
        LineBank slotted = LineBank.Parse("{\"FruitGiven\": {\"1\": [\"For {thing}.\"]}}");
        Expect(slotted.Speak(fruit, 1, 7, 2000, memory, out _) == null, "a line whose slot the fact lacks never shows");
    }

    // CP21: every line has a stable ID for translators and analytics; it is a pure hash of group, voice and text.
    private static void LineIds()
    {
        // The pinned value is FNV-1a over the UTF-16 code units of "FruitGiven|0|Thank you, Crème.", computed in Python.
        Expect(LineBank.LineId("FruitGiven", 0, "Thank you, Crème.") == "cl_f8f7c620", "the line ID is FNV-1a over UTF-16 code units");
        Expect(LineBank.LineId("FruitGiven", 0, "Thank you, Crème.") == LineBank.LineId("FruitGiven", 0, "Thank you, Crème."), "the line ID is deterministic");
        Expect(LineBank.LineId("FruitGiven", 0, "Thank you.") != LineBank.LineId("FruitGiven", 0, "Thank you!"), "changing one character changes the ID");

        LineBank en = LineBank.Parse(File.ReadAllText(Path.Combine(Root(), "games/evergrove/Assets/Resources/Evergrove/Lines/en.json")));
        var ids = new HashSet<string>();
        int total = 0;
        foreach (var line in en.All())
        {
            total++;
            Expect(ids.Add(LineBank.LineId(line.Group, line.Voice, line.Text)), "a line ID is unique: " + line.Group + " " + line.Text);
        }
        Expect(total > 0 && ids.Count == total, "every shipped English line has an ID");
        Console.WriteLine("Line IDs: " + total + " shipped English lines, " + ids.Count + " unique IDs.");
    }

    // Each rule must catch a line that breaks it, or a passing bank proves nothing.
    private static void RulesCatchBadLines(Rules rules)
    {
        var bad = new Dictionary<string, string> {
            { "Thank you!", "exclamation" },
            { "I missed you, and the fruit.", "banned" },
            { "Come back soon for more fruit.", "banned" },
            { "Fruit keeps your brain young.", "banned" },
            { "That's three days in a row.", "time" },
            { "I ate it yesterday.", "time" },
            { "I'd kill for fruit like this.", "child" },
            { "Thank you for 3 pears.", "number" },
            { "Thank you for the {thing}.", "slot" },
            { "Thank you. " + new string('o', 140), "length" },
            { "Thanks {a", "braces" },
        };
        foreach (var pair in bad)
            Expect(rules.Check("FruitGiven", pair.Key).Count > 0, "the " + pair.Value + " rule catches: " + pair.Key);
        Expect(rules.Check("NewestThing", "Your {thing} is lovely.").Count > 0, "{thing} as a verb's subject is caught");
        Expect(rules.Check("NewestThing", "I saw a {thing}.").Count > 0, "an article before a slot is caught");
        Expect(rules.Check("KindSolved", "Well done. {work} looks right.").Count > 0, "a slot starting a sentence is caught");
        Expect(rules.Check("NewestThing", "I saw your {thing} {where}. Lovely.").Count == 0, "a good line passes");
        Expect(rules.Check("NoSuchKind", "Dusk.").Count > 0, "a kind without rules is refused");
        Expect(rules.Check("WeatherWhileAway", "While you were away we had {weather}.").Count == 0,
            "the weather while away may say 'while you were away': it is its fact");
        Expect(rules.Check("WeatherToday", "While you were away it rained.").Count > 0, "other kinds may not");
    }

    // The "longest" values the length rule uses really are the longest the game can produce.
    private static void LongestIsLongest(Rules rules)
    {
        int thing = 0;
        for (int g = 0; g < 64; g++)
            for (int c = 0; c < GardenCatalog.ChaptersPerGarden; c++)
                for (int f = 0; f < 3; f++) thing = Math.Max(thing, GardenCatalog.For(g, c, (VillageChoice)f).Name.Length);
        for (int k = 0; k < Nursery.Kinds; k++) thing = Math.Max(thing, Nursery.Item(k).Name.Length);
        Expect(rules.Longest["thing"].Length >= thing, "the longest {thing} covers every catalogue and nursery name (" + thing + ")");
        int work = Enum.GetValues(typeof(PuzzleKind)).Cast<PuzzleKind>().Max(k => CompanionFacts.WorkOf(k).Length);
        Expect(rules.Longest["work"].Length >= work, "the longest {work} covers every puzzle kind");
        int where = new[] { "by the creek", "by the cottage", "down the valley", "in the meadow" }.Max(w => w.Length);
        Expect(rules.Longest["where"].Length >= where, "the longest {where} covers every place");
        Expect(rules.Longest["name"].Length >= FamilyParcel.MaxNameLength, "the longest {name} is a full-length family name");
        int have = 0, need = 0;
        for (int v = 0; v < 3; v++)
        {
            int target = NeighbourWish.For(WorldModel.New(PlayMode.Living), v).Need;
            need = Math.Max(need, CompanionFacts.Words(target).Length);
            for (int n = 1; n < target; n++) have = Math.Max(have, CompanionFacts.Words(n).Length);
        }
        Expect(rules.Longest["have"].Length >= have && rules.Longest["need"].Length >= need, "the longest {have}/{need} cover every wish count");
        int weather = 0;
        for (int garden = 0; garden < 24; garden++)
            foreach (Season season in Enum.GetValues(typeof(Season)))
                for (int days = 2; days <= 31; days++)
                {
                    DateTime today = new DateTime(2026, 1, 1).AddDays(garden * 37 + days);
                    string phrase = DayWeather.WhileAway(today.AddDays(-days), today, season, garden, out _);
                    if (phrase != null) weather = Math.Max(weather, phrase.Length);
                }
        Expect(rules.Longest["weather"].Length >= weather, "the longest {weather} covers the phrases seen (" + weather + ")");
        foreach (var kind in rules.Kinds)
            foreach (string slot in kind.Value.Slots)
                Expect(rules.Longest.ContainsKey(slot), "a longest value for {" + slot + "}");
    }

    // Every slot a kind's lines may use is really filled by facts of that kind, non-empty.
    private static void FactsFillTheirSlots(Rules rules)
    {
        GardenMap map = GardenMap.River(10, 15, CompanionFactsTests.Mask(), 2);
        var seen = new HashSet<FactKind>();
        var today = new DateTime(2026, 11, 27);
        foreach (WorldModel world in CompanionFactsTests.Saves())
            foreach (CompanionContext context in CompanionFactsTests.Contexts(map).Where(c => c.Map != null)) // without the map, {where} is unknown and today's sentence speaks
                foreach (Fact fact in CompanionFacts.For(world, context))
                {
                    if (!rules.Kinds.TryGetValue(fact.Kind.ToString(), out KindRule rule)) continue;
                    seen.Add(fact.Kind);
                    foreach (string slot in rule.Slots)
                        Expect(!string.IsNullOrEmpty(fact.Slot(slot)), fact + " fills {" + slot + "}");
                }
        foreach (string kind in rules.Kinds.Keys)
            Expect(seen.Contains((FactKind)Enum.Parse(typeof(FactKind), kind)), "a constructed save produces " + kind);
    }

    // The shipped English bank speaks, slots filled, for every fact of every kind it holds.
    private static void EnglishSpeaks(Rules rules)
    {
        LineBank en = LineBank.Parse(File.ReadAllText(Path.Combine(Root(), "games/evergrove/Assets/Resources/Evergrove/Lines/en.json")));
        GardenMap map = GardenMap.River(10, 15, CompanionFactsTests.Mask(), 2);
        var today = new DateTime(2026, 11, 27);
        var spoken = new HashSet<FactKind>();
        var pending = new HashSet<FactKind>();
        foreach (WorldModel world in CompanionFactsTests.Saves())
            foreach (CompanionContext context in CompanionFactsTests.Contexts(map).Where(c => c.Map != null)) // without the map, {where} is unknown and today's sentence speaks
                foreach (Fact fact in CompanionFacts.For(world, context))
                {
                    int speaker = fact.Keeper < 0 ? 0 : fact.Keeper;
                    if (!rules.Kinds.ContainsKey(fact.Kind.ToString()) || rules.Kinds[fact.Kind.ToString()].Pending)
                    {
                        // A kind newer than its reviewed lines speaks its own fixed sentence until they ship.
                        Expect(CompanionSpeech.Legacy(fact, speaker) != null, "a kind awaiting review still has words: " + fact);
                        pending.Add(fact.Kind);
                        continue;
                    }
                    string text = en.Speak(fact, speaker, world.Garden, context.DayNumber, CompanionMemory.Empty, out int variant);
                    Expect(text != null && variant >= 0 && !text.Contains("{") && !text.Contains("}"), "English speaks " + fact + ": " + text);
                    Expect(text.Length <= rules.MaxChars, "within " + rules.MaxChars + " characters: " + text);
                    spoken.Add(fact.Kind);
                }
        foreach (var kind in rules.Kinds)
            if (!kind.Value.Pending)
                Expect(spoken.Contains((FactKind)Enum.Parse(typeof(FactKind), kind.Key)), "the English bank speaks for " + kind.Key);
        foreach (FactKind kind in Enum.GetValues(typeof(FactKind)))
            Expect(rules.Kinds.ContainsKey(kind.ToString()), "every fact kind has rules (reviewed lines, or pending review): " + kind);
        if (pending.Count > 0) Console.WriteLine("Line bank: awaiting reviewed lines (fixed sentence meanwhile): " + string.Join(", ", pending));
        Expect(pending.Count == 0, "every fact kind on main has a reviewed English bank: " + string.Join(", ", pending));
    }

    private static IEnumerable<WorldModel> KindsSaves()
    {
        WorldModel grown = CompanionFactsTests.Grown(9);
        grown.SolveTree(0);
        grown.SolveTree(1);
        grown.SolveTree(2);
        WorldSnapshot s = grown.Export();
        s.fruit = 2;
        WorldModel.TryRestore(s, out WorldModel fruity);
        fruity.GiveFruit(1);
        fruity.ReceiveNeighbourCutting(0);
        fruity.ReceiveNeighbourCutting(1);
        fruity.ReceiveNeighbourCutting(2);
        yield return grown;
        yield return fruity;
    }

    private static int ShippedBanks(Rules rules)
    {
        string dir = Path.Combine(Root(), "games/evergrove/Assets/Resources/Evergrove/Lines");
        int count = 0;
        Expect(File.Exists(Path.Combine(dir, "en.json")), "an English bank ships (empty until reviewed)");
        foreach (string path in Directory.GetFiles(dir, "*.json"))
        {
            LineBank bank = LineBank.Parse(File.ReadAllText(path));
            count += CheckBank(Path.GetFileName(path), rules, bank.Keys.ToDictionary(k => k,
                k => bank.Voices(k).ToDictionary(v => v, v => bank.Lines(k, v).ToList())),
                complete: Path.GetFileName(path) == "en.json"); // SC04: English covers every kind
        }
        return count;
    }

    private static readonly Regex VoiceHeading = new Regex(@"^##\s+.*\(voice\s+(\d|any)\)(?:\s*·\s*(\w+))?\s*$");
    private static readonly Regex Item = new Regex(@"^\d+\.\s+(.*\S)\s*$");

    private static int ReviewDrafts(Rules rules)
    {
        string dir = Path.Combine(Root(), "docs/improvements/review");
        if (!Directory.Exists(dir)) return 0;
        int count = 0;
        foreach (string path in Directory.GetFiles(dir, "companion-lines-*.md"))
        {
            string kind = Path.GetFileNameWithoutExtension(path).Replace("companion-lines-", "");
            var bank = new Dictionary<string, Dictionary<string, List<string>>>();
            List<string> target = null;
            foreach (string raw in File.ReadAllLines(path))
            {
                Match heading = VoiceHeading.Match(raw);
                if (heading.Success)
                {
                    string key = kind + (heading.Groups[2].Success ? "." + heading.Groups[2].Value : "");
                    if (!bank.ContainsKey(key)) bank[key] = new Dictionary<string, List<string>>();
                    if (!bank[key].ContainsKey(heading.Groups[1].Value)) bank[key][heading.Groups[1].Value] = new List<string>();
                    target = bank[key][heading.Groups[1].Value];
                    continue;
                }
                if (raw.StartsWith("#")) { target = null; continue; }
                Match item = Item.Match(raw);
                if (item.Success && target != null) target.Add(item.Groups[1].Value);
            }
            count += CheckBank(Path.GetFileName(path), rules, bank);
        }
        return count;
    }

    /// <summary>bank: {"Kind" or "Kind.Value": {voice: [lines]}}. Counts are checked for each kind present; complete: every kind.</summary>
    private static int CheckBank(string name, Rules rules, Dictionary<string, Dictionary<string, List<string>>> bank, bool complete = false)
    {
        int count = 0;
        var present = new Dictionary<string, Dictionary<string, Dictionary<string, int>>>();
        foreach (var entry in bank)
        {
            int dot = entry.Key.IndexOf('.');
            string kind = dot < 0 ? entry.Key : entry.Key.Substring(0, dot);
            string qualifier = dot < 0 ? "" : entry.Key.Substring(dot + 1);
            Expect(rules.Kinds.TryGetValue(kind, out KindRule rule), name + ": " + kind + " has rules");
            Expect((qualifier.Length == 0) == (rule.Qualifier == null), name + ": " + entry.Key + " is keyed by its qualifier when it has one");
            foreach (var voice in entry.Value)
            {
                Expect(rule.Voices.ContainsKey(voice.Key), name + ": voice " + voice.Key + " speaks " + kind);
                if (qualifier.Length > 0)
                    Expect(rule.Voices[voice.Key].Contains(qualifier), name + ": voice " + voice.Key + " needs " + entry.Key);
                if (!present.ContainsKey(kind)) present[kind] = new Dictionary<string, Dictionary<string, int>>();
                if (!present[kind].ContainsKey(voice.Key)) present[kind][voice.Key] = new Dictionary<string, int>();
                present[kind][voice.Key][qualifier] = voice.Value.Count;
                var seen = new HashSet<string>();
                foreach (string line in voice.Value)
                {
                    Expect(seen.Add(line.Trim().ToLowerInvariant()), name + ": duplicate " + line);
                    List<string> problems = rules.Check(kind, line);
                    Expect(problems.Count == 0, name + ": " + entry.Key + "/" + voice.Key + " " + string.Join("; ", problems) + ": " + line);
                    count++;
                }
            }
        }
        foreach (var kind in rules.Kinds)
        {
            if (!present.TryGetValue(kind.Key, out var got)) { Expect(!complete || kind.Value.Pending, name + ": has lines for " + kind.Key); continue; }
            foreach (var voice in kind.Value.Voices)
            {
                got.TryGetValue(voice.Key, out var counts);
                counts = counts ?? new Dictionary<string, int>();
                Expect(counts.Values.Sum() >= kind.Value.Min, name + ": " + kind.Key + "/" + voice.Key + " has at least " + kind.Value.Min + " lines");
                foreach (string value in voice.Value ?? new List<string>())
                    Expect(counts.TryGetValue(value, out int n) && n >= kind.Value.MinEach,
                        name + ": " + kind.Key + "." + value + "/" + voice.Key + " has at least " + kind.Value.MinEach + " lines");
            }
        }
        return count;
    }

    private static string Root()
    {
        string dir = AppContext.BaseDirectory;
        while (dir != null && !File.Exists(Path.Combine(dir, "games/evergrove/docs/specs/companion-line-rules.json")))
            dir = Path.GetDirectoryName(dir);
        if (dir == null) throw new Exception("companion-line-rules.json not found");
        return dir;
    }

    /// <summary>The same rules as tools/evergrove_line_bank_check.py, from the shared JSON.</summary>
    private sealed class KindRule
    {
        public List<string> Slots;
        public Dictionary<string, List<string>> Voices; // voice -> qualifier values (null when unqualified)
        public int Min;
        public string Qualifier;
        public int MinEach;
        public bool AllowTimeClaims;
        public bool Pending;
    }

    private sealed class Rules
    {
        public int MaxChars;
        public Dictionary<string, KindRule> Kinds;
        public Dictionary<string, string> Longest;
        public List<string> Banned, ChildSafe, TimeClaims, SubjectVerbs;

        public static Rules Load()
        {
            using var doc = JsonDocument.Parse(File.ReadAllText(Path.Combine(Root(), "games/evergrove/docs/specs/companion-line-rules.json")));
            JsonElement r = doc.RootElement;
            List<string> List(JsonElement e) => e.EnumerateArray().Select(x => x.GetString()).ToList();
            var kinds = new Dictionary<string, KindRule>();
            foreach (JsonProperty k in r.GetProperty("kinds").EnumerateObject())
            {
                JsonElement v = k.Value;
                kinds[k.Name] = new KindRule {
                    Slots = List(v.GetProperty("slots")),
                    Voices = v.GetProperty("voices").EnumerateObject().ToDictionary(p => p.Name,
                        p => p.Value.ValueKind == JsonValueKind.Null ? null : List(p.Value)),
                    Min = v.GetProperty("min").GetInt32(),
                    Qualifier = v.TryGetProperty("qualifier", out JsonElement q) ? q.GetString() : null,
                    MinEach = v.TryGetProperty("minEach", out JsonElement m) ? m.GetInt32() : 0,
                    AllowTimeClaims = v.TryGetProperty("allowTimeClaims", out JsonElement t) && t.GetBoolean(),
                    Pending = v.TryGetProperty("pending", out JsonElement pend) && pend.GetBoolean(),
                };
            }
            return new Rules {
                MaxChars = r.GetProperty("maxChars").GetInt32(),
                Kinds = kinds,
                Longest = r.GetProperty("longest").EnumerateObject().ToDictionary(p => p.Name, p => p.Value.GetString()),
                Banned = List(r.GetProperty("banned")), ChildSafe = List(r.GetProperty("childSafe")),
                TimeClaims = List(r.GetProperty("timeClaims")), SubjectVerbs = List(r.GetProperty("subjectVerbs")),
            };
        }

        private static bool Hit(string text, string phrase)
        {
            return Regex.IsMatch(text, "(?<![a-z])" + Regex.Escape(phrase) + "(?![a-z])");
        }

        public List<string> Check(string kind, string line)
        {
            var problems = new List<string>();
            if (!Kinds.TryGetValue(kind, out KindRule rule)) { problems.Add("unknown kind"); return problems; }
            if (string.IsNullOrEmpty(line) || line != line.Trim() || line.Contains("\n")) problems.Add("empty, untrimmed or broken");
            List<string> used = LineBank.SlotsIn(line);
            if (line.Count(c => c == '{') != used.Count || line.Count(c => c == '}') != used.Count) problems.Add("unbalanced braces");
            foreach (string slot in used)
                if (!rule.Slots.Contains(slot)) problems.Add("slot {" + slot + "} not filled by " + kind);
            string filled = Regex.Replace(line, @"\{([^{}]*)\}", m => Longest.TryGetValue(m.Groups[1].Value, out string v) ? v : m.Value);
            if (filled.Length > MaxChars) problems.Add(filled.Length + " chars when filled");
            if (line.Contains("!")) problems.Add("exclamation mark");
            if (Regex.IsMatch(line, @"\d")) problems.Add("a number");
            string lower = filled.ToLowerInvariant();
            foreach (string p in Banned) if (Hit(lower, p.ToLowerInvariant())) problems.Add("banned: " + p);
            foreach (string p in ChildSafe) if (Hit(lower, p.ToLowerInvariant())) problems.Add("not for a 7-year-old: " + p);
            if (!rule.AllowTimeClaims)
                foreach (string p in TimeClaims) if (Hit(lower, p.ToLowerInvariant())) problems.Add("claims time: " + p);
            if (Regex.IsMatch(line, @"(^|[.;?]\s+)\{")) problems.Add("a slot starts a sentence");
            if (Regex.IsMatch(line, @"(?i)\ban? \{")) problems.Add("an article before a slot");
            if (Regex.IsMatch(line, @"\{thing\}(?: \{where\})?,? (?:" + string.Join("|", SubjectVerbs) + @")\b"))
                problems.Add("{thing} as a verb's subject");
            return problems;
        }
    }
}
