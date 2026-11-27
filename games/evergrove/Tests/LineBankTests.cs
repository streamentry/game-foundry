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
        RulesCatchBadLines(rules);
        LongestIsLongest(rules);
        FactsFillTheirSlots(rules);
        int shipped = ShippedBanks(rules);
        EnglishSpeaks(rules);
        int drafts = ReviewDrafts(rules);
        Console.WriteLine("Line bank SC03: " + shipped + " shipped lines and " + drafts + " draft lines pass every rule.");
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
        Expect(rules.Check("Hour", "Dusk.").Count > 0, "a kind without rules is refused");
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
        foreach (var kind in rules.Kinds)
            foreach (string slot in kind.Value)
                Expect(rules.Longest.ContainsKey(slot), "a longest value for {" + slot + "}");
    }

    // Every slot a kind's lines may use is really filled by facts of that kind, non-empty.
    private static void FactsFillTheirSlots(Rules rules)
    {
        GardenMap map = GardenMap.River(10, 15, CompanionFactsTests.Mask(), 2);
        var seen = new HashSet<FactKind>();
        var today = new DateTime(2026, 11, 27);
        foreach (WorldModel world in KindsSaves())
            foreach (CompanionContext context in new[] {
                new CompanionContext(today, map: map),
                new CompanionContext(today, fruitJustGivenTo: 1, togetherKeeper: 2, togetherKind: PuzzleKind.Pairs, map: map) })
                foreach (Fact fact in CompanionFacts.For(world, context))
                {
                    if (!rules.Kinds.TryGetValue(fact.Kind.ToString(), out List<string> slots)) continue;
                    seen.Add(fact.Kind);
                    foreach (string slot in slots)
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
        foreach (WorldModel world in KindsSaves())
            foreach (CompanionContext context in new[] {
                new CompanionContext(today, map: map),
                new CompanionContext(today, fruitJustGivenTo: 1, togetherKeeper: 2, togetherKind: PuzzleKind.Pairs, map: map) })
                foreach (Fact fact in CompanionFacts.For(world, context))
                {
                    if (!en.Kinds.Contains(fact.Kind)) continue;
                    int speaker = fact.Keeper < 0 ? 0 : fact.Keeper;
                    string text = en.Speak(fact, speaker, world.Garden, context.DayNumber, CompanionMemory.Empty, out int variant);
                    Expect(text != null && variant >= 0 && !text.Contains("{") && !text.Contains("}"), "English speaks " + fact + ": " + text);
                    Expect(text.Length <= rules.MaxChars, "within " + rules.MaxChars + " characters: " + text);
                    spoken.Add(fact.Kind);
                }
        foreach (FactKind kind in en.Kinds) Expect(spoken.Contains(kind), "the English bank speaks for " + kind);
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
            count += CheckBank(Path.GetFileName(path), rules, bank.Kinds.ToDictionary(k => k.ToString(),
                k => bank.Voices(k).ToDictionary(v => v, v => bank.Lines(k, v).ToList())));
        }
        return count;
    }

    private static readonly Regex VoiceHeading = new Regex(@"^##\s+.*\(voice\s+(\d|any)\)\s*$");
    private static readonly Regex Item = new Regex(@"^\d+\.\s+(.*\S)\s*$");

    private static int ReviewDrafts(Rules rules)
    {
        string dir = Path.Combine(Root(), "docs/improvements/review");
        if (!Directory.Exists(dir)) return 0;
        int count = 0;
        foreach (string path in Directory.GetFiles(dir, "companion-lines-*.md"))
        {
            string kind = Path.GetFileNameWithoutExtension(path).Replace("companion-lines-", "");
            var voices = new Dictionary<string, List<string>>();
            string voice = null;
            foreach (string raw in File.ReadAllLines(path))
            {
                Match heading = VoiceHeading.Match(raw);
                if (heading.Success) { voice = heading.Groups[1].Value; voices[voice] = new List<string>(); continue; }
                if (raw.StartsWith("#")) { voice = null; continue; }
                Match item = Item.Match(raw);
                if (item.Success && voice != null) voices[voice].Add(item.Groups[1].Value);
            }
            count += CheckBank(Path.GetFileName(path), rules, new Dictionary<string, Dictionary<string, List<string>>> { { kind, voices } });
        }
        return count;
    }

    private static int CheckBank(string name, Rules rules, Dictionary<string, Dictionary<string, List<string>>> bank)
    {
        int count = 0;
        foreach (var kind in bank)
        {
            Expect(rules.Kinds.ContainsKey(kind.Key), name + ": " + kind.Key + " has rules");
            foreach (string voice in rules.Voices)
                Expect(kind.Value.TryGetValue(voice, out var lines) && lines.Count >= rules.MinVariants,
                    name + ": " + kind.Key + "/" + voice + " has at least " + rules.MinVariants + " lines");
            foreach (var voice in kind.Value)
            {
                var seen = new HashSet<string>();
                foreach (string line in voice.Value)
                {
                    Expect(seen.Add(line.Trim().ToLowerInvariant()), name + ": duplicate " + line);
                    List<string> problems = rules.Check(kind.Key, line);
                    Expect(problems.Count == 0, name + ": " + kind.Key + "/" + voice.Key + " " + string.Join("; ", problems) + ": " + line);
                    count++;
                }
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
    private sealed class Rules
    {
        public int MaxChars;
        public int MinVariants;
        public List<string> Voices;
        public Dictionary<string, List<string>> Kinds;
        public Dictionary<string, string> Longest;
        public List<string> Banned, ChildSafe, TimeClaims, SubjectVerbs;

        public static Rules Load()
        {
            using var doc = JsonDocument.Parse(File.ReadAllText(Path.Combine(Root(), "games/evergrove/docs/specs/companion-line-rules.json")));
            JsonElement r = doc.RootElement;
            List<string> List(string name) => r.GetProperty(name).EnumerateArray().Select(e => e.GetString()).ToList();
            return new Rules {
                MaxChars = r.GetProperty("maxChars").GetInt32(),
                MinVariants = r.GetProperty("minVariants").GetInt32(),
                Voices = List("voices"),
                Kinds = r.GetProperty("kinds").EnumerateObject().ToDictionary(p => p.Name, p => p.Value.EnumerateArray().Select(e => e.GetString()).ToList()),
                Longest = r.GetProperty("longest").EnumerateObject().ToDictionary(p => p.Name, p => p.Value.GetString()),
                Banned = List("banned"), ChildSafe = List("childSafe"), TimeClaims = List("timeClaims"), SubjectVerbs = List("subjectVerbs"),
            };
        }

        private static bool Hit(string text, string phrase)
        {
            return Regex.IsMatch(text, "(?<![a-z])" + Regex.Escape(phrase) + "(?![a-z])");
        }

        public List<string> Check(string kind, string line)
        {
            var problems = new List<string>();
            if (!Kinds.TryGetValue(kind, out List<string> allowed)) { problems.Add("unknown kind"); return problems; }
            if (string.IsNullOrEmpty(line) || line != line.Trim() || line.Contains("\n")) problems.Add("empty, untrimmed or broken");
            List<string> used = LineBank.SlotsIn(line);
            if (line.Count(c => c == '{') != used.Count || line.Count(c => c == '}') != used.Count) problems.Add("unbalanced braces");
            foreach (string slot in used)
                if (!allowed.Contains(slot)) problems.Add("slot {" + slot + "} not filled by " + kind);
            string filled = Regex.Replace(line, @"\{([^{}]*)\}", m => Longest.TryGetValue(m.Groups[1].Value, out string v) ? v : m.Value);
            if (filled.Length > MaxChars) problems.Add(filled.Length + " chars when filled");
            if (line.Contains("!")) problems.Add("exclamation mark");
            if (Regex.IsMatch(line, @"\d")) problems.Add("a number");
            string lower = filled.ToLowerInvariant();
            foreach (string p in Banned) if (Hit(lower, p.ToLowerInvariant())) problems.Add("banned: " + p);
            foreach (string p in ChildSafe) if (Hit(lower, p.ToLowerInvariant())) problems.Add("not for a 7-year-old: " + p);
            foreach (string p in TimeClaims) if (Hit(lower, p.ToLowerInvariant())) problems.Add("claims time: " + p);
            if (Regex.IsMatch(line, @"(^|[.;?]\s+)\{")) problems.Add("a slot starts a sentence");
            if (Regex.IsMatch(line, @"(?i)\ban? \{")) problems.Add("an article before a slot");
            if (Regex.IsMatch(line, @"\{thing\}(?: \{where\})?,? (?:" + string.Join("|", SubjectVerbs) + @")\b"))
                problems.Add("{thing} as a verb's subject");
            return problems;
        }
    }
}
