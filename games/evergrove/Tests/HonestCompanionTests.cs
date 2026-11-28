using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Evergrove.Core;

// Checklist item 88 (honest companions): a line audit of neighbour and companion text against
// vision section 1a "What we must not claim" and AGENTS.md (no memory, cognition, health or
// loneliness claims; neighbours never imply they are real people).
//
// Coverage of English player-facing text in Evergrove.Core:
//   1. Source scan: every C# string literal in Assets/Scripts/Core/*.cs (complete backstop for
//      text written inside method bodies, including the English GameLocalization catalogue).
//   2. Reflection: every static string, string[], jagged array, and dictionary field in the
//      Evergrove.Core assembly, walked recursively.
//   3. API enumeration: neighbour generators run over generated Living worlds, so composed text
//      (for example "Mira  /  ..." lines and chapter journals) is audited as the game produces it.
//   4. GameLocalization.Source(key) for every catalogue key (English is the source).
//
// Translations in Assets/Resources/Evergrove/Locales/*.json are NOT audited here. Only English is.
// Each translation needs native review (checklist item 86).
//
// Not covered: neighbour dialogue written in Assets/Scripts/Unity/*.cs (see the report).
internal static class HonestCompanionTests
{
    private static int assertions;

    private static void Expect(bool ok, string message)
    {
        assertions++;
        if (!ok) throw new Exception("HonestCompanionTests failed: " + message);
    }

    private sealed class Rule
    {
        public readonly string Name;
        public readonly Regex Pattern;
        public readonly string[] Bad;

        public Rule(string name, string pattern, string[] bad)
        {
            Name = name;
            Pattern = new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
            Bad = bad;
        }
    }

    private sealed class Hit
    {
        public string Where;
        public string Text;
    }

    // Offending lines, exact text as it appears in the source or runtime output.
    // The test asserts this list equals the set of violations found: a NEW violation fails the
    // build, and a fixed one must be removed from this list. Do not add lines to hide a failure;
    // only add a line after the founder has decided it stays. Empty means no violations found.
    private static readonly string[] KnownViolations = { };

    // Rule 1: no health, cognition or benefit claims, and no diagnosis language.
    // Rule 2: no loneliness claims and no emotional-dependence pressure (guilt, absence, need).
    // Rule 3: neighbours never imply they are real people, and never claim personal affection.
    // Keepsake wording ("Mira remembers the shade you planted", "a keepsake") is allowed.
    private static readonly Rule[] Rules =
    {
        new Rule("R1 health/cognition claim",
            @"\b(dementia|alzheimer\w*|cognitive|cognition|diagnos\w*|therap(y|ist|eutic)|cures?|cured|symptoms?|medical|disease|clinical|brain|wellbeing|well-being|wellness|mental health|memory (loss|decline|problems?|training|exercises?))\b" +
            @"|\b(improves?|boosts?|sharpens?|trains?|strengthens?)\s+(your|the|our)?\s*(memory|memories|mind|brain|cognition|cognitive|health|mood)\b" +
            @"|\bhelps?\s+(your|you)\s+(memory|memories|mind|brain|health|mood)\b" +
            @"|\b(keeps?|keeping|stays?|makes?)\s+you\s+(healthy|fit|sharp|young|well)\b" +
            @"|\b(keeps?|keeping)\s+your\s+(mind|brain)\b" +
            @"|\byour\s+(health|mood)\b" +
            @"|\b(good|great|healthy) for your (memory|mind|brain|health|body|heart)\b",
            new[]
            {
                "This garden is good for your memory, so keep coming back.",
                "Tending trees keeps your mind sharp.",
                "Working the bridges will improve your memory.",
                "It can help with dementia and memory loss.",
                "This is a kind of brain training for your wellbeing.",
                "Gardening here is a cure for a low mood.",
                "Tell your doctor: it's a therapy for worried minds.",
            }),

        new Rule("R2 loneliness or dependence pressure",
            @"\blonel(y|iness)\b|\blonesome\b" +
            @"|\byou\b[^.!?]{0,25}\balone\b|\bfeel(ing)? alone\b" +
            @"|\bkeeps? you company\b|\bkeep you company\b" +
            @"|\b(miss|missed|missing)\s+you\b" +
            @"|\bhaven'?t (seen|heard from) you\b" +
            @"|\byou (haven'?t|have not|hadn'?t) (played|visited|been|come|returned|written|tended|watered)\b" +
            @"|\bsince you('ve| have)? (last )?(been|played|visited|left|gone)\b" +
            @"|\bdon'?t (leave|abandon|forget) (us|me)\b" +
            @"|\bwe need you\b|\bI need you\b|\bneed you back\b" +
            @"|\b(is|are|was|were) waiting (for|on) you\b" +
            @"|\byou'?ll lose\b|\byou will lose\b" +
            @"|\byou (should|ought to|must|need to) (come back|come by|visit|return)\b",
            new[]
            {
                "You're not alone in this garden, friend.",
                "Nobody should feel lonely, so come sit with us.",
                "We missed you so much while you were away.",
                "Don't leave us with the weeds.",
                "We need you back at the bridge.",
                "Your grandchild is waiting for you.",
                "If you stop tending, you'll lose the shade.",
                "It's been days since you last visited the river.",
                "You should come back and water the beds.",
                "Mira will keep you company while you rest.",
            }),

        new Rule("R3 implies a real person or personal affection",
            @"\bI'?m (a |just a )?real\b|\bI am (a |just a )?real\b|\bwe'?re (a |just )?real\b|\bwe are (a |just )?real\b" +
            @"|\b(Mira|Orin|Fen) (is|was) (a )?real\b" +
            @"|\breal (person|people|human|humans|being|beings|neighbou?rs?)\b" +
            @"|\b(I'?m|I am|we'?re|we are) (a |just a )?(person|human|human being)\b" +
            @"|\b(always|right) here for you\b|\bI'?m (always )?(here|there|around) for you\b" +
            @"|\btalk to me (any ?time|whenever|anytime|day or night)\b" +
            @"|\b(call|text|message|write to|write) me (any ?time|whenever|anytime)\b" +
            @"|\bI love you\b|\bI (really )?care about you\b|\bI (really )?miss you\b",
            new[]
            {
                "Don't worry, I'm real, I promise.",
                "We're real people, not a program.",
                "I'm a real person who lives down the lane.",
                "Talk to me anytime you like, I'm always here for you.",
                "I love you, you know that.",
                "Write to me whenever you need company.",
            }),
    };

    // Near misses and real game-voice lines that must NOT match any rule.
    private static readonly string[] Allowed =
    {
        "Mind the steps by the creek.",
        "The beech keeps the shade cool.",
        "I can see a real canopy growing now.",
        "The real stones were laid by my uncle.",
        "The bees keep the flowers healthy.",
        "A healthy tree sheds its leaves in autumn.",
        "The garden keeps your memories in the book.",
        "Mira remembers the shade you planted by the stream.",
        "Orin remembers the crossing you made possible.",
        "I remember the trees you planted.",
        "Standalone puzzles are calm.",
        "The beech stood alone by the well.",
        "Good to see you.",
        "The first memory is still waiting.",
        "Your tree is turning gold. It will lose its leaves and come back greener.",
        "Come back to the garden whenever you like.",
        "I love the way the creek sounds now.",
        "Some of those flowers are older than I am.",
        // Fictional family backstory is allowed: characters having histories is not a claim to be
        // real people talking to the player (founder decision, 2026-11-27).
        "Spring is in the garden. When I was small, my grandmother tied a ribbon to the first tree to leaf each year. For luck, she said, but really it was so we'd stop and look. I've tied one on yours.",
        "My father kept the first fallen leaf of every year pressed in a book. Forty leaves, forty autumns, and the book still smells of them.",
        "Nobody ever thanked him, and he never minded.",
        // Family blessing on a parcel: a wish, not a claim about the player's health (founder decision, 2026-11-27).
        "Good health",
        "A happy year",
        "The villagers are beginning to notice your care.",
        "Fen counted the blossoms twice, just for the pleasure of it.",
        "I'm really pleased with how the bridge turned out.",
        "The bank will be fuller next spring.",
    };

    public static int Run()
    {
        SelfTests();

        string coreDir = FindCoreDir();
        Expect(coreDir != null, "Assets/Scripts/Core found from the test directory");

        string scriptsDir = Path.GetDirectoryName(coreDir);
        string unityDir = Path.Combine(scriptsDir, "Unity");
        Expect(Directory.Exists(unityDir), "Assets/Scripts/Unity found");
        var files = new List<string>(Directory.GetFiles(coreDir, "*.cs"));
        files.AddRange(UnityFiles(unityDir));
        List<Hit> literals = ScanLiterals(files, scriptsDir);
        Expect(literals.Count >= 300, "source scan sees the literal set (" + literals.Count + ")");
        Expect(Contains(literals, "Mira remembers the shade you planted by the stream."),
            "canary: source scan reads VillageStory literals");
        Expect(ContainsIn(literals, "Unity/EvergroveApp.Map.cs", "You found what was under the mossy stone: "),
            "canary: Unity scan reaches a real KeepNote literal in EvergroveApp.Map.cs");

        var reflected = new List<Hit>();
        SweepStaticFields(reflected);
        Expect(ContainsPrefix(reflected, "Spring is in the garden."),
            "canary: reflection reaches SeasonTales jagged arrays");

        var generated = new List<Hit>();
        EnumerateGenerators(generated);
        Expect(Contains(generated, "Mira remembers the shade you planted by the stream."),
            "canary: API enumeration reaches VillageStory.Memory");
        Expect(generated.Count >= 200, "API enumeration produces generated lines (" + generated.Count + ")");

        var all = new List<Hit>();
        all.AddRange(literals);
        all.AddRange(reflected);
        all.AddRange(generated);
        AuditAll(all);
        return assertions;
    }

    private static void SelfTests()
    {
        foreach (Rule rule in Rules)
        {
            Expect(rule.Bad.Length >= 5, rule.Name + " has at least five bad examples");
            foreach (string bad in rule.Bad)
                Expect(rule.Pattern.IsMatch(Normalize(bad)), rule.Name + " catches: " + bad);
        }
        foreach (string good in Allowed)
            foreach (Rule rule in Rules)
                Expect(!rule.Pattern.IsMatch(Normalize(good)), rule.Name + " passes allowed line: " + good);
    }

    private static void AuditAll(List<Hit> all)
    {
        var firstWhere = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (Hit hit in all)
        {
            if (string.IsNullOrWhiteSpace(hit.Text)) continue;
            if (!firstWhere.ContainsKey(hit.Text)) firstWhere[hit.Text] = hit.Where;
        }
        Expect(firstWhere.Count >= 200, "distinct English lines audited (" + firstWhere.Count + ")");
        Console.WriteLine("HonestCompanionTests: distinct literal lines audited = " + firstWhere.Count);

        assertions += firstWhere.Count * Rules.Length; // one check per line per rule
        var problems = new StringBuilder();
        var offending = new HashSet<string>(StringComparer.Ordinal);
        foreach (KeyValuePair<string, string> pair in firstWhere)
        {
            foreach (Rule rule in Rules)
            {
                if (!rule.Pattern.IsMatch(Normalize(pair.Key))) continue;
                offending.Add(pair.Key);
                if (Array.IndexOf(KnownViolations, pair.Key) < 0)
                    problems.AppendLine("NEW [" + rule.Name + "] at " + pair.Value + ": " + pair.Key);
            }
        }
        foreach (string known in KnownViolations)
        {
            if (!offending.Contains(known))
                problems.AppendLine("KnownViolations entry no longer violates, remove it: " + known);
        }
        if (problems.Length > 0)
            throw new Exception("HonestCompanionTests failed: honest-companion audit\n" + problems);
    }

    private static string Normalize(string text)
    {
        return text.Replace('’', '\'').Replace('‘', '\'');
    }

    private static bool Contains(List<Hit> hits, string text)
    {
        foreach (Hit hit in hits) if (hit.Text == text) return true;
        return false;
    }

    private static bool ContainsPrefix(List<Hit> hits, string prefix)
    {
        foreach (Hit hit in hits) if (hit.Text != null && hit.Text.StartsWith(prefix, StringComparison.Ordinal)) return true;
        return false;
    }

    // Works from games/evergrove/Tests (local runs), the repo root (CI: dotnet run --project ...)
    // and the build output folder: walk up from both the working directory and the binary.
    private static string FindCoreDir()
    {
        foreach (string start in new[] { Directory.GetCurrentDirectory(), AppContext.BaseDirectory })
        {
            string dir = start;
            for (int up = 0; up < 8 && dir != null; up++)
            {
                foreach (string candidate in new[] {
                    Path.Combine(dir, "Assets", "Scripts", "Core"),
                    Path.Combine(dir, "games", "evergrove", "Assets", "Scripts", "Core") })
                    if (Directory.Exists(candidate)) return candidate;
                dir = Path.GetDirectoryName(dir);
            }
        }
        return null;
    }

    // Unity sources except QA-only ones: any file under a Dev/ folder, or whose name starts with "Dev".
    private static List<string> UnityFiles(string unityDir)
    {
        var result = new List<string>();
        foreach (string file in Directory.GetFiles(unityDir, "*.cs", SearchOption.AllDirectories))
        {
            string rel = Path.GetRelativePath(unityDir, file);
            string[] parts = rel.Split(Path.DirectorySeparatorChar);
            bool devFolder = false;
            for (int p = 0; p < parts.Length - 1; p++)
                if (string.Equals(parts[p], "Dev", StringComparison.OrdinalIgnoreCase)) devFolder = true;
            if (devFolder || Path.GetFileName(file).StartsWith("Dev", StringComparison.Ordinal)) continue;
            result.Add(file);
        }
        result.Sort(StringComparer.Ordinal);
        return result;
    }

    private static bool ContainsIn(List<Hit> hits, string whereStart, string text)
    {
        foreach (Hit hit in hits)
            if (hit.Text == text && hit.Where.StartsWith(whereStart, StringComparison.Ordinal)) return true;
        return false;
    }

    // Reads every C# string literal (regular, verbatim and interpolated) from the given files.
    // Comments are skipped while scanning, so quoted lines inside // and /* */ are never extracted.
    private static List<Hit> ScanLiterals(List<string> files, string root)
    {
        var result = new List<Hit>();
        files.Sort(StringComparer.Ordinal);
        foreach (string file in files)
        {
            string src = File.ReadAllText(file);
            string name = Path.GetRelativePath(root, file).Replace('\\', '/');
            int line = 1;
            int i = 0;
            while (i < src.Length)
            {
                char c = src[i];
                if (c == '\n') { line++; i++; continue; }
                if (c == '/' && i + 1 < src.Length && src[i + 1] == '/')
                {
                    while (i < src.Length && src[i] != '\n') i++;
                    continue;
                }
                if (c == '/' && i + 1 < src.Length && src[i + 1] == '*')
                {
                    int end = src.IndexOf("*/", i + 2, StringComparison.Ordinal);
                    int stop = end < 0 ? src.Length : end + 2;
                    for (int k = i; k < stop; k++) if (src[k] == '\n') line++;
                    i = stop;
                    continue;
                }
                if (c == '\'')
                {
                    // Char literal, for example '"' or '\''. Skip it whole.
                    i++;
                    if (i < src.Length && src[i] == '\\') i++;
                    i++;
                    while (i < src.Length && src[i] != '\'' && src[i] != '\n') i++;
                    i++;
                    continue;
                }
                if (c == '"')
                {
                    char prev = i > 0 ? src[i - 1] : ' ';
                    bool verbatim = prev == '@' || (prev == '$' && i > 1 && src[i - 2] == '@');
                    int startLine = line;
                    var text = new StringBuilder();
                    i++;
                    if (verbatim)
                    {
                        while (i < src.Length)
                        {
                            if (src[i] == '"')
                            {
                                if (i + 1 < src.Length && src[i + 1] == '"') { text.Append('"'); i += 2; continue; }
                                break;
                            }
                            if (src[i] == '\n') line++;
                            text.Append(src[i]);
                            i++;
                        }
                    }
                    else
                    {
                        while (i < src.Length && src[i] != '"' && src[i] != '\n')
                        {
                            if (src[i] == '\\' && i + 1 < src.Length)
                            {
                                char e = src[i + 1];
                                if (e == 'u' && i + 6 <= src.Length)
                                {
                                    text.Append((char)Convert.ToInt32(src.Substring(i + 2, 4), 16));
                                    i += 6;
                                }
                                else if (e == 'U' && i + 10 <= src.Length)
                                {
                                    text.Append(char.ConvertFromUtf32(Convert.ToInt32(src.Substring(i + 2, 8), 16)));
                                    i += 10;
                                }
                                else
                                {
                                    switch (e)
                                    {
                                        case 'n': text.Append('\n'); break;
                                        case 't': text.Append('\t'); break;
                                        case 'r': text.Append('\r'); break;
                                        default: text.Append(e); break; // \" \\ and the rest
                                    }
                                    i += 2;
                                }
                                continue;
                            }
                            text.Append(src[i]);
                            i++;
                        }
                    }
                    i++; // closing quote
                    if (text.Length > 0)
                        result.Add(new Hit { Where = name + ":" + startLine, Text = text.ToString() });
                    continue;
                }
                i++;
            }
        }
        return result;
    }

    // Walks every static field of every type in the Evergrove.Core assembly (private and public).
    private static void SweepStaticFields(List<Hit> hits)
    {
        Assembly asm = typeof(VillageStory).Assembly;
        foreach (Type type in asm.GetTypes())
        {
            // Only Core types: the test assembly also holds test classes and their fixtures.
            if (type.Namespace != "Evergrove.Core") continue;
            foreach (FieldInfo field in type.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
            {
                if (!typeof(IEnumerable).IsAssignableFrom(field.FieldType) && field.FieldType != typeof(string)) continue;
                // Non-English catalogues filled at runtime by Register(); English is audited via Keys/Source.
                if (type == typeof(GameLocalization) && (field.Name == "Translations" || field.Name == "current")) continue;
                Walk(field.GetValue(null), type.Name + "." + field.Name, hits, 0);
            }
        }
    }

    private static void Walk(object value, string where, List<Hit> hits, int depth)
    {
        if (value == null || depth > 5) return;
        string s = value as string;
        if (s != null) { hits.Add(new Hit { Where = where, Text = s }); return; }
        IDictionary dict = value as IDictionary;
        if (dict != null)
        {
            foreach (DictionaryEntry entry in dict)
            {
                Walk(entry.Key, where, hits, depth + 1);
                Walk(entry.Value, where, hits, depth + 1);
            }
            return;
        }
        IEnumerable seq = value as IEnumerable;
        if (seq != null)
        {
            foreach (object item in seq) Walk(item, where, hits, depth + 1);
        }
    }

    private static void Add(List<Hit> hits, string where, string text)
    {
        if (text != null) hits.Add(new Hit { Where = where, Text = text });
    }

    // Runs the neighbour generators over Living worlds grown to every chapter count, with three
    // village-choice rotations, so composed and conditional lines are produced as players see them.
    private static void EnumerateGenerators(List<Hit> hits)
    {
        foreach (string key in GameLocalization.Keys)
            Add(hits, "GameLocalization.Source(" + key + ")", GameLocalization.Source(key));

        foreach (Season season in Enum.GetValues(typeof(Season)))
        {
            Add(hits, "Season.RiverNote", NeighbourNotice.RiverNote(season));
            for (int turn = 0; turn < 6; turn++)
            {
                SeasonNote note = SeasonTales.For(season, turn);
                Add(hits, "SeasonTales.From", note.From);
                Add(hits, "SeasonTales.Body", note.Body);
            }
        }

        foreach (PuzzleKind kind in Enum.GetValues(typeof(PuzzleKind)))
        {
            Add(hits, "NeighbourNotice.TogetherLine", NeighbourNotice.TogetherLine(kind));
            Add(hits, "NeighbourNotice.TogetherLine(3)", NeighbourNotice.TogetherLine(kind, 3));
            Add(hits, "NeighbourNotice.TogetherLine(4)", NeighbourNotice.TogetherLine(kind, 4));
            Add(hits, "NeighbourNotice.KindWord", NeighbourNotice.KindWord(kind));
        }

        foreach (Visitor visitor in Enum.GetValues(typeof(Visitor)))
        {
            Add(hits, "Visitors.Why", Visitors.Why(visitor));
        }

        for (int villager = 0; villager < 3; villager++)
            Add(hits, "VillageStory.VillagerName", VillageStory.VillagerName(villager));

        for (int choice = 0; choice < 3; choice++)
            Add(hits, "VillageStory.ChoiceLabel", VillageStory.ChoiceLabel((VillageChoice)choice));

        for (int milestone = 0; milestone < NeighbourGifts.Milestones.Length; milestone++)
            Add(hits, "NeighbourGifts.From", NeighbourGifts.From(NeighbourGifts.Milestones[milestone]));

        for (int offset = 0; offset < 3; offset++)
        {
            for (int count = 0; count <= PuzzleCatalog.Count; count++)
            {
                WorldModel world = Grow(offset, count);
                string where = "generated(offset " + offset + ", chapters " + count + ")";

                Add(hits, where + " ChapterPrompt", VillageStory.ChapterPrompt(world));
                Add(hits, where + " Memory", VillageStory.Memory(world));
                Add(hits, where + " Consequence", VillageStory.Consequence(world));
                Add(hits, where + " MemoryEvent", VillageStory.MemoryEvent(world));
                Add(hits, where + " Ending", VillageStory.Ending(world));
                AddAll(hits, where + " Characters", VillageStory.Characters(world));
                AddAll(hits, where + " Journal", VillageStory.Journal(world));
                for (int villager = 0; villager < 3; villager++)
                {
                    Add(hits, where + " Speak", VillageStory.Speak(world, villager));
                    Add(hits, where + " NeighbourNotice.Line", NeighbourNotice.Line(villager, world));
                    Add(hits, where + " NeighbourNotice.ThanksFor", NeighbourNotice.ThanksFor(villager, world));
                    NeighbourWish wish = NeighbourWish.For(world, villager);
                    if (wish != null)
                    {
                        Add(hits, where + " NeighbourWish.Hope", wish.Hope);
                        Add(hits, where + " NeighbourWish.Thanks", wish.Thanks);
                    }
                    for (int day = 0; day < 6; day++)
                        Add(hits, where + " Visitors.KeeperRemark", Visitors.KeeperRemark(world, villager, day));
                }
                for (int choice = 0; choice < 3; choice++)
                    Add(hits, where + " ChoicePromise", VillageStory.ChoicePromise((VillageChoice)choice, world));
                foreach (Keepsake keepsake in MemoryShelf.For(world))
                    Add(hits, where + " MemoryShelf", keepsake.Label);
            }
        }
    }

    private static void AddAll(List<Hit> hits, string where, string[] lines)
    {
        if (lines == null) return;
        foreach (string line in lines) Add(hits, where, line);
    }

    // Plays chapters with the reference solution, choosing villages in a rotating order.
    private static WorldModel Grow(int offset, int count)
    {
        WorldModel world = WorldModel.New(PlayMode.Living);
        for (int chapter = 0; chapter < count; chapter++)
        {
            Puzzle board = world.CurrentPuzzle;
            for (int i = 0; i < board.CellCount; i++)
                while (world.Rotations[i] != board.Solution[i]) world.Rotate(i);
            world.Choose((VillageChoice)((chapter + offset) % 3));
        }
        return world;
    }
}
