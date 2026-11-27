using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Evergrove.Core;

/// <summary>
/// Checklist 4 (Mira explains once: under 40 words before the first puzzle, at most three
/// dialogue lines) and checklist 16 (badges mean one thing and clear when looked at).
/// Core strings are read through GameLocalization; Unity literals are read from source.
/// The lists below are exact: a new literal, a moved literal or a new badge fails the audit
/// until this file is updated on purpose.
/// </summary>
internal static class FirstMinutesAuditTests
{
    private static int assertions;

    private const int WordBudget = 40;      // checklist 4: goal is under 40 words shown first
    private const int DialogueLineBudget = 3;

    private static void Expect(bool ok, string message)
    {
        assertions++;
        if (!ok) throw new Exception("FirstMinutesAuditTests failed: " + message);
    }

    public static int Run()
    {
        FirstRunAudit();
        BadgeAudit();
        return assertions;
    }

    // ---- Checklist 4: first run, launch to the first interactive puzzle (English).

    // Core keys shown on screen before the first puzzle is interactive. Each is "key -> where it shows".
    private static readonly string[] FirstRunShownKeys =
    {
        "title",    // EvergroveApp.cs GardenHeader: painted wordmark; spoken text beside it. Name counted once.
        "subtitle", // EvergroveApp.cs title screen tagline (line ~1716). Hidden on puzzle screens since I160.
        "first",    // EvergroveApp.cs BuildPuzzle -> CoachBubble: Mira's one opening line (I16). Words show at >100% text (I163).
        "text_size", // EvergroveApp.FirstSize.cs chooser heading (checklist 6): "Text size", shown before the first puzzle.
    };

    // Checklist 6: one visible "A" per TextScale step (100, 125, 150), each a one-word glyph.
    private const int FirstSizeGlyphs = 3;

    // Core keys spoken only (transparent Text or screen-reader hint), not shown. Not counted.
    private static readonly string[] FirstRunSpokenOnlyKeys =
    {
        "stream", // EvergroveApp.cs StreamStones "Stream position": STREAM 1 / 12, alpha 0.
        "how",    // EvergroveApp.cs tile speech hint: "Tap to turn a pipe..." (the on-board line was removed, I16).
        "menu",   // EvergroveApp.cs chrome buttons: icon-first, caption kept hidden for screen readers.
        "undo",   // EvergroveApp.cs BuildPuzzle tools: icon-first, caption hidden.
        "hint",   // EvergroveApp.cs BuildPuzzle tools: icon-first, caption hidden.
        "close",  // EvergroveApp.FirstSize.cs chooser Done: icon-first (tick), caption hidden.
    };

    // Unity literal lines in source that must still exist, so moved copy fails until listed again.
    private static readonly string[] FirstRunSourceCanaries =
    {
        "GameLocalization.Text(\"title\")",
        "GameLocalization.Text(\"subtitle\")",
        "CoachBubble(GameLocalization.Text(\"first\"))",
        "OfferFirstSize(firstPuzzle);",
        "GameLocalization.Text(\"stream\", world.Round + 1",
        "GameLocalization.Text(\"how\")",
        "world.CurrentPuzzle.Name",
    };

    // Puzzle-name text shown before the first puzzle, from Core PuzzleCatalog stream 1 (PipeRules.cs).
    // Shown twice before the first move: the Play button (EvergroveApp.cs ~1812, English only)
    // and the stream sign on the first board (StreamStones, I162).
    private const int FirstPuzzleNameShownCount = 2;

    private static void FirstRunAudit()
    {
        string root = EvergroveRoot();
        string app = File.ReadAllText(Path.Combine(root, "Assets", "Scripts", "Unity", "EvergroveApp.cs"));

        foreach (string canary in FirstRunSourceCanaries)
            Expect(app.Contains(canary), "first-run literal moved or removed: " + canary);

        string previous = GameLocalization.Current;
        try
        {
            Expect(GameLocalization.SetCurrent("en"), "English locale must be selectable for the audit");

            int words = 0;
            foreach (string key in FirstRunShownKeys)
            {
                string text = GameLocalization.Text(key);
                Expect(!text.StartsWith("[missing:", StringComparison.Ordinal), "missing Core key " + key);
                words += CountWords(text);
            }

            string puzzleName = PuzzleCatalog.Get(0).Name;
            Expect(puzzleName == "First Bend", "stream 1 name changed; update the first-run list: " + puzzleName);
            words += FirstPuzzleNameShownCount * CountWords(puzzleName);

            // Checklist 6: the text-size chooser shows one "A" glyph per size step before the first puzzle.
            string firstSize = File.ReadAllText(Path.Combine(root, "Assets", "Scripts", "Unity", "EvergroveApp.FirstSize.cs"));
            Expect(firstSize.Contains("\"A\""), "text-size chooser glyph literal missing from EvergroveApp.FirstSize.cs");
            words += FirstSizeGlyphs * CountWords("A");

            Expect(words < WordBudget,
                "first-run words shown before the first puzzle must be under " + WordBudget + ", counted " + words);

            // Spoken-only keys must stay spoken-only: they must not be counted above.
            foreach (string key in FirstRunSpokenOnlyKeys)
                Expect(!GameLocalization.Text(key).StartsWith("[missing:", StringComparison.Ordinal), "missing Core key " + key);

            // Mira's opening dialogue: one bubble, at most three short lines.
            string coach = GameLocalization.Text("first");
            Expect(!coach.Contains("\n"), "coach dialogue must be a single line");
            int sentences = coach.Split(new[] { '.', '!', '?' }, StringSplitOptions.RemoveEmptyEntries)
                .Count(part => part.Trim().Length > 0);
            Expect(sentences <= DialogueLineBudget, "coach dialogue must be at most " + DialogueLineBudget + " lines");
            int bubbles = Regex.Matches(app, @"CoachBubble\(GameLocalization\.Text\(""first""\)\)").Count;
            Expect(bubbles == 1, "Mira must speak once before the first puzzle, found " + bubbles);

            // Coach copy for the record: sentence and word count.
            Expect(CountWords(coach) <= WordBudget, "coach dialogue unexpectedly long");
        }
        finally
        {
            if (previous != null) GameLocalization.SetCurrent(previous);
        }
    }

    private static int CountWords(string text)
    {
        return Regex.Matches(text, "[A-Za-z0-9]+").Count;
    }

    // ---- Checklist 16: every badge source clears when looked at.

    // Each badge: the source symbol, its file, its literal, and a literal that must appear in the
    // file to prove the clear path (the write that happens when the player looks at it).
    private sealed class Badge
    {
        public readonly string Symbol, Literal, ClearPath;
        public Badge(string symbol, string literal, string clearPath)
        {
            Symbol = symbol; Literal = literal; ClearPath = clearPath;
        }
    }

    private static readonly Badge[] BadgeSources =
    {
        // B1 Journal dot on the book token in the fan (MarkUnreadMemory -> MemoryJournal clears).
        new Badge("MarkUnreadMemory", "Has a new memory", "SetInt(JournalSeenKey, world.Choices.Length)"),
        // B1b Menu-button dot (MarkUnreadMemory with MenuSeenKey -> ToggleFan clears).
        new Badge("MarkUnreadMemory", "Has a new memory", "SetInt(MenuSeenKey, world.Choices.Length)"),
        // B2 "New to your almanac" disc on a nursery token: clears when the plant enters the almanac.
        new Badge("NewToAlmanac", "New to your almanac", "almanac.Add("),
        // B3 Neighbour speech mark on the hub: clears when the player talks to them (HeardKey written).
        new Badge("PortraitButton", "Has something new to say", "SetString(HeardKey + villagerIndex"),
        // B4 Unread letter envelope on the letterbox: clears when the letter is opened.
        new Badge("LetterBox", "Unread letter", "SetInt(LettersReadKey, letters.Length)"),
        // B5 Gift ribbon on a relative's cutting in the basket: clears when it is planted (PlantGift).
        new Badge("GiftTokens", "Ribbon", "world.PlantGift("),
    };

    // Named nodes that look badge-like but are not notification badges. Each has a reason.
    private static readonly string[] NotBadgeNames =
    {
        "Level dots", "Level on", "Level off",          // IconButton pips helper (no caller shows them now)
        "Saved mark", "Saved sprig", "Saved words",     // transient save confirmation, fades (Saving.cs)
        "Speech mark", "Almanac mark", "Sprig mark",    // icons inside B3 and B2 and B1
        "Sprig left", "Sprig right", "Coach disc",      // painted ornaments and the coach portrait
        "Sealed letter", "letter-sealed",               // icon inside B4
        "Newest thing, peeking out", "Newer letter", "Older letter",
        "Mosselora wordmark", "Mark", "Share mark", "Chat mark", "Undo mark", "Turn back", "Play mark", // icons on tokens
        "Keep unreadable files",
    };

    // Colours that read as red: a red badge is forbidden on any badge source.
    private static bool IsRed(float r, float g, float b)
    {
        return r >= 0.7f && g <= 0.45f && b <= 0.45f;
    }

    private static void BadgeAudit()
    {
        // Pure Core: a new almanac says "new" (badge on); adding the plant clears it (badge off).
        var almanac = new Almanac();
        GardenItem plant = Nursery.Item(0);
        Expect(!almanac.Has(plant), "a plant not yet in the almanac must show its new mark");
        almanac.Add(plant);
        Expect(almanac.Has(plant), "adding the plant to the almanac must clear its new mark");

        string root = EvergroveRoot();
        string unity = Path.Combine(root, "Assets", "Scripts", "Unity");
        string app = File.ReadAllText(Path.Combine(unity, "EvergroveApp.cs"));
        string sources = AllUnitySource(unity);

        foreach (Badge badge in BadgeSources)
        {
            Expect(app.Contains(badge.Literal), "badge literal missing: " + badge.Literal);
            Expect(Regex.IsMatch(app, @"\b" + badge.Symbol + @"\s*\("), "badge symbol missing: " + badge.Symbol);
            Expect(sources.Contains(badge.ClearPath), "no clear path for " + badge.Literal + ": " + badge.ClearPath);

            string body = MethodBody(app, badge.Symbol);
            Expect(!Regex.IsMatch(body, @"ToString\(\)"), "badge " + badge.Symbol + " must not show a count");
            foreach (Match m in Regex.Matches(body, @"new Color\(\s*([\d.]+)f?\s*,\s*([\d.]+)f?\s*,\s*([\d.]+)f?"))
            {
                float r = float.Parse(m.Groups[1].Value, System.Globalization.CultureInfo.InvariantCulture);
                float g = float.Parse(m.Groups[2].Value, System.Globalization.CultureInfo.InvariantCulture);
                float b = float.Parse(m.Groups[3].Value, System.Globalization.CultureInfo.InvariantCulture);
                Expect(!IsRed(r, g, b), "red badge colour in " + badge.Symbol + ": " + m.Value);
            }
        }

        // The hint button no longer draws level dots (a dot means something new).
        Expect(!app.Contains("world.HintStep > 0 ? 2 : 0"), "hint button must not draw level dots");

        // Scan: every badge-like node name in the Unity UI must be a listed badge or an explicit non-badge.
        var badgeWord = new Regex(@"(?i)(badge|ribbon|\bunread\b|\bnew to\b|\bnew memory\b|\bhas\b|\bdots?\b|\bdisc\b|speech|sealed|\bmark\b)");
        var allowed = new HashSet<string>(StringComparer.Ordinal);
        foreach (Badge badge in BadgeSources) allowed.Add(badge.Literal);
        foreach (string name in NotBadgeNames) allowed.Add(name);
        foreach (string file in Directory.GetFiles(unity, "EvergroveApp*.cs"))
        {
            foreach (string line in File.ReadAllLines(file))
            {
                if (!Regex.IsMatch(line, @"\b(Node|Ornament|Label|IconButton|FanToken|Image)\(")) continue;
                foreach (Match m in Regex.Matches(line, "\"([^\"]+)\""))
                {
                    string name = m.Groups[1].Value;
                    // Node names are short labels; sentences (spoken or shown copy) are not names.
                    if (name.Length > 40 || name.Contains(".")) continue;
                    if (!badgeWord.IsMatch(name)) continue;
                    Expect(allowed.Contains(name),
                        "new badge-like node \"" + name + "\" in " + Path.GetFileName(file) + " is not in the badge list");
                }
            }
        }
    }

    private static string AllUnitySource(string unity)
    {
        var parts = new List<string>();
        foreach (string file in Directory.GetFiles(unity, "*.cs", SearchOption.AllDirectories))
            parts.Add(File.ReadAllText(file));
        return string.Join("\n", parts);
    }

    // Returns the brace-matched body of the first method named symbol.
    private static string MethodBody(string text, string symbol)
    {
        Match header = Regex.Match(text, @"\b" + symbol + @"\s*\([^)]*\)\s*\{");
        Expect(header.Success, "method not found: " + symbol);
        int depth = 0;
        for (int i = header.Index + header.Length - 1; i < text.Length; i++)
        {
            if (text[i] == '{') depth++;
            else if (text[i] == '}' && --depth == 0) return text.Substring(header.Index, i - header.Index + 1);
        }
        throw new Exception("unbalanced braces in " + symbol);
    }

    // Walks up from the working directory and from the assembly's directory. Returns .../games/evergrove.
    private static string EvergroveRoot()
    {
        string[] starts = { Directory.GetCurrentDirectory(), AppContext.BaseDirectory };
        foreach (string start in starts)
        {
            DirectoryInfo current = new DirectoryInfo(start);
            while (current != null)
            {
                string nested = Path.Combine(current.FullName, "games", "evergrove");
                if (Directory.Exists(Path.Combine(nested, "Assets", "Scripts", "Core"))) return nested;
                if (Directory.Exists(Path.Combine(current.FullName, "Assets", "Scripts", "Core"))) return current.FullName;
                current = current.Parent;
            }
        }
        throw new Exception("Could not find games/evergrove from " + Directory.GetCurrentDirectory());
    }
}
