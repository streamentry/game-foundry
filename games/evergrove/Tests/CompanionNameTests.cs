using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Evergrove.Core;

/// <summary>
/// CP33: a family member's typed name is tidy before it fills a {name} slot. The fact keys keep the
/// raw name (memory stores them), only the slot value is tidied.
/// </summary>
internal static class CompanionNameTests
{
    private static int assertions;

    private static void Expect(bool ok, string message)
    {
        assertions++;
        if (!ok) throw new Exception("Companion name test failed: " + message);
    }

    private static int TextElements(string s)
    {
        return new StringInfo(s).LengthInTextElements;
    }

    public static int Run()
    {
        assertions = 0;
        Expect(CompanionFacts.TidyName("  Anh  ") == "Anh", "surrounding spaces are trimmed");
        Expect(CompanionFacts.TidyName("Bà\tNội") == "Bà Nội", "a tab is a space: " + CompanionFacts.TidyName("Bà\tNội"));
        Expect(CompanionFacts.TidyName("a   b \t\t c") == "a b c", "space runs collapse to one");
        Expect(CompanionFacts.TidyName("{thing}") == "thing", "braces are removed");
        Expect(CompanionFacts.TidyName("a|b\nc") == "ab c", "a bar is dropped, a newline is a space: " + CompanionFacts.TidyName("a|b\nc"));
        Expect(CompanionFacts.TidyName("a\u0001b\u007fc") == "abc", "other control characters are dropped");
        Expect(CompanionFacts.TidyName(null) == "", "null is empty");
        Expect(CompanionFacts.TidyName("   \t\n ") == "", "only whitespace is empty");
        Expect(CompanionFacts.TidyName("{|}") == "", "only braces and bars is empty");
        Expect(CompanionFacts.TidyName("Anh") == "Anh", "a clean name is unchanged");

        string forty = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMN";
        string cut = CompanionFacts.TidyName(forty);
        Expect(TextElements(cut) == 24 && cut == forty.Substring(0, 24), "a 40-character name keeps 24 text elements: " + cut);

        // CP33: an emoji on the boundary is one text element, never split; no ellipsis is added.
        string emojiEdge = new string('a', 23) + "\U0001F600\U0001F600";
        string kept = CompanionFacts.TidyName(emojiEdge);
        Expect(TextElements(kept) == 24 && kept == new string('a', 23) + "\U0001F600", "an emoji at the boundary is kept whole");

        // An accented letter written as base plus combining mark is one text element.
        string accented = new string('a', 23) + "éx";
        Expect(TextElements(CompanionFacts.TidyName(accented)) == 24, "a combining mark stays with its letter");

        // A cut that would end in a space is trimmed.
        string spaced = new string('a', 23) + " bb";
        Expect(CompanionFacts.TidyName(spaced) == new string('a', 23), "no trailing space after the cut");

        // Facts: a typed name fills the slot tidied, and the key keeps what was typed.
        var today = new DateTime(2026, 11, 27);
        WorldModel world = WorldModel.New(PlayMode.Living);
        Expect(world.KeepNote("Anh", "The plums are early this year."), "a note from Anh");
        string raw = world.LetterFrom[world.LetterFrom.Length - 1];
        Fact letter = null;
        foreach (Fact f in CompanionFacts.For(world, new CompanionContext(today)))
            if (f.Kind == FactKind.LetterWaiting) letter = f;
        Expect(letter != null && letter.Slot("name") == "Anh", "the letter's name slot is the tidy name");
        Expect(letter != null && letter.Key == "letter:" + raw + ":" + world.Letters.Length,
            "the letter key keeps the raw name: " + (letter == null ? "none" : letter.Key));

        // A missing name is an empty slot, never a null that the words would print.
        Expect(CompanionFacts.TidyName(null).Length == 0, "a missing name tidies to empty");

        AnEmptyNameIsNotSaid();

        Console.WriteLine("Companion names CP33: " + assertions + " assertions, typed names tidy in slots, keys unchanged.");
        return assertions;
    }

    /// <summary>The reviewed English bank, found by walking up from the test binary (as CompanionFuzzTests does).</summary>
    private static LineBank LoadEnglishBank()
    {
        string dir = AppContext.BaseDirectory;
        while (dir != null && !File.Exists(Path.Combine(dir, "Assets/Resources/Evergrove/Lines/en.json")))
            dir = Path.GetDirectoryName(dir);
        if (dir == null) throw new Exception("Companion name test failed: en.json not found");
        return LineBank.Parse(File.ReadAllText(Path.Combine(dir, "Assets/Resources/Evergrove/Lines/en.json")));
    }

    /// <summary>
    /// CP35: a name that tidies to nothing (spaces, braces) fills no {name} slot, so its fact can't be said:
    /// no bank line, no "from  is waiting" fallback. The fact stays in the feed (the letter is still in the
    /// box, and the attention arrow uses the feed), and the ranker gives the neighbour their next fact.
    /// </summary>
    private static void AnEmptyNameIsNotSaid()
    {
        LineBank bank = LoadEnglishBank();
        int day = CompanionContext.DayNumberOf(new DateTime(2026, 11, 27));
        string blank = CompanionFacts.TidyName("   ");
        Expect(blank == "" && CompanionFacts.TidyName("{}") == "", "the names under test tidy to nothing");

        var letterSlots = new Dictionary<string, string> { { "name", blank } };
        Fact letter = new Fact(FactKind.LetterWaiting, 1, "letter:   :1", letterSlots, CompanionFacts.WaitingWeight, true, false, 3 * 100000);
        var helpSlots = new Dictionary<string, string>
        {
            { "name", CompanionFacts.TidyName("{}") }, { "puzzle", PuzzleKind.Water.ToString() }, { "work", "the water" }, { "item", "4" }
        };
        Fact helped = new Fact(FactKind.HelpedBy, 0, "helped:4", helpSlots, CompanionFacts.WaitingWeight, true, false, 5 * 100000);

        // (a) every LetterWaiting and HelpedBy line uses {name}, so with no name there is nothing to say.
        Expect(!CompanionLocale.CanSay(CompanionLocale.English, bank, letter, 1), "CP35: a blank letter sender is not sayable");
        Expect(!CompanionLocale.CanSay(CompanionLocale.English, bank, helped, 0), "CP35: a blank helper is not sayable");

        // (b) the words are null (never "from  ." or " came by"), and the old fixed sentence is null too.
        Expect(CompanionLocale.Words(CompanionLocale.English, bank, letter, 1, 1, day, null, -1, out int v1) == null && v1 == -1,
            "CP35: no words for a blank letter");
        Expect(CompanionLocale.Words(CompanionLocale.English, bank, helped, 0, 1, day, null, -1, out int v2) == null && v2 == -1,
            "CP35: no words for a blank helper");
        Expect(CompanionSpeech.Legacy(letter, 1) == null && CompanionSpeech.Legacy(helped, 0) == null,
            "CP35: the English fallback is null for a blank name");
        Expect(CompanionSpeech.LegacyPick(new[] { letter }, 1) == null, "CP35: the old chain does not pick a blank letter");

        // Clean names are unchanged: the same fact with a real name is sayable, in the bank and as the fallback.
        var cleanSlots = new Dictionary<string, string> { { "name", "Anh" } };
        Fact clean = new Fact(FactKind.LetterWaiting, 1, "letter:Anh:1", cleanSlots, CompanionFacts.WaitingWeight, true, false, 3 * 100000);
        Expect(CompanionLocale.CanSay(CompanionLocale.English, bank, clean, 1), "CP35: a named letter is sayable");
        Expect(CompanionSpeech.Legacy(clean, 1) == "There's a letter in your box from Anh. I saw the flag go up this morning.",
            "CP35: a clean name keeps today's sentence");
        string words = CompanionLocale.Words(CompanionLocale.English, bank, clean, 1, 1, day, null, -1, out _);
        Expect(words != null && words.Contains("Anh") && !words.Contains("  ") && !words.Contains("{"),
            "CP35: a named letter says its name cleanly: " + words);

        // (c) the ranker skips the blank letter and picks the neighbour's next fact.
        var fruitSlots = new Dictionary<string, string>();
        Fact fruit = new Fact(FactKind.FruitGiven, 1, "fruit:1:2", fruitSlots, CompanionFacts.OwnDoingWeight, false, false, 8 * 100000);
        var feed = new List<Fact> { letter, fruit };
        Func<Fact, int, bool> canSay = (f, who) => CompanionLocale.CanSay(CompanionLocale.English, bank, f, who);
        Fact picked = CompanionRanker.Pick(feed, CompanionMemory.Empty, 1, day, 1, null, canSay);
        Expect(picked != null && picked.Kind == FactKind.FruitGiven, "CP35: the next fact is said when the letter's name is blank");
        Fact[] allotted = CompanionRanker.Allot(feed, CompanionMemory.Empty, day, 1, canSay);
        Expect(allotted[1] != null && allotted[1].Kind == FactKind.FruitGiven, "CP35: Allot gives the neighbour the next fact");
    }
}
