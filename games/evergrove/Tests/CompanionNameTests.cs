using System;
using System.Collections.Generic;
using System.Globalization;
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

        Console.WriteLine("Companion names CP33: " + assertions + " assertions, typed names tidy in slots, keys unchanged.");
        return assertions;
    }
}
