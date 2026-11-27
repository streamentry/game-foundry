using System;
using System.Globalization;
using System.Text;
using Evergrove.Core;

/// <summary>
/// Checklist 3: the optional garden name. Set, trimmed and limited by text elements; refused when it
/// could be markup or a line break; kept in the garden file; suggested deterministically; and shown
/// on postcards only when set.
/// </summary>
internal static class GardenNameTests
{
    private static int assertions;
    private const string Stamp = "2026-11-27T12:00:00Z";

    private static void Expect(bool ok, string message)
    {
        assertions++;
        if (!ok) throw new Exception("GardenNameTests failed: " + message);
    }

    private static string Repeat(string unit, int count)
    {
        var sb = new StringBuilder();
        for (int i = 0; i < count; i++) sb.Append(unit);
        return sb.ToString();
    }

    private static int Graphemes(string text)
    {
        return new StringInfo(text).LengthInTextElements;
    }

    private static WorldModel Named(string name)
    {
        WorldModel world = WorldModel.New(PlayMode.Living);
        Expect(world.SetGardenName(name), "a valid name is accepted: " + name);
        return world;
    }

    public static int Run()
    {
        DefaultIsEmpty();
        SetTrimsAndCollapses();
        LengthCountsTextElements();
        RefusesUnsafeText();
        EmptyClears();
        PuzzleOnlyRefuses();
        SaveRoundTrip();
        OldSavesRestoreEmpty();
        ForgedValuesRefused();
        SuggestionIsDeterministic();
        PostcardShowsNameOnlyWhenSet();
        return assertions;
    }

    private static void DefaultIsEmpty()
    {
        Expect(WorldModel.New(PlayMode.Living).GardenName == "", "a new garden has no name");
        Expect(WorldModel.New(PlayMode.PuzzleOnly).GardenName == "", "a puzzle garden has no name");
    }

    private static void SetTrimsAndCollapses()
    {
        WorldModel world = Named("  Willow    Bend  ");
        Expect(world.GardenName == "Willow Bend", "trimmed and collapsed: " + world.GardenName);
        Expect(world.SetGardenName("Otter\u00A0\u00A0Pool") && world.GardenName == "Otter Pool", "non-breaking spaces collapse");
    }

    private static void LengthCountsTextElements()
    {
        WorldModel world = WorldModel.New(PlayMode.Living);
        string ascii = Repeat("a", 24);
        Expect(world.SetGardenName(ascii) && world.GardenName == ascii, "24 letters are allowed");
        Expect(!world.SetGardenName(Repeat("a", 25)) && world.GardenName == ascii, "25 letters are refused and change nothing");

        string emoji = Repeat("\U0001F333", 24);
        Expect(world.SetGardenName(emoji) && world.GardenName == emoji, "24 emoji (surrogate pairs) are allowed");
        Expect(!world.SetGardenName(Repeat("\U0001F333", 25)) && world.GardenName == emoji, "25 emoji are refused");

        // Devanagari: each "कि" is one text element (consonant plus vowel sign) of two code units.
        string indic = Repeat("\u0915\u093F", 24);
        Expect(Graphemes(indic) == 24 && indic.Length == 48, "the Indic sample is 24 text elements");
        Expect(world.SetGardenName(indic) && world.GardenName == indic, "24 Indic text elements are allowed");
        Expect(!world.SetGardenName(Repeat("\u0915\u093F", 25)) && world.GardenName == indic, "25 Indic text elements are refused");

        // A combined emoji sequence (family ZWJ) is one text element and is never split.
        string family = "\U0001F469\u200D\U0001F467";
        Expect(world.SetGardenName(family) && Graphemes(world.GardenName) == 1, "a ZWJ emoji stays whole");
    }

    private static void RefusesUnsafeText()
    {
        WorldModel world = Named("Heron Pool");
        string[] refused = {
            "a\nb", "a\rb", "a\tb", "a\u0007b", "a\u2028b", "a\u2029b",
            "<b>x", "x>", "Pool <i>", "<", ">", "\uD83D", "a\uDC00b"
        };
        foreach (string bad in refused)
        {
            Expect(!world.SetGardenName(bad) && world.GardenName == "Heron Pool",
                "refused and unchanged: " + bad.Replace("\n", "\\n").Replace("\t", "\\t"));
        }
        Expect(!world.SetGardenName(null) && world.GardenName == "Heron Pool", "null is refused");
    }

    private static void EmptyClears()
    {
        WorldModel world = Named("Lily Reach");
        Expect(world.SetGardenName("") && world.GardenName == "", "empty text clears the name");
        Expect(world.SetGardenName("Lily Reach") && world.SetGardenName("   ") && world.GardenName == "", "spaces only clears the name");
    }

    private static void PuzzleOnlyRefuses()
    {
        WorldModel puzzle = WorldModel.New(PlayMode.PuzzleOnly);
        Expect(!puzzle.SetGardenName("Willow Bend") && puzzle.GardenName == "", "a puzzle garden refuses a name");
        Expect(!puzzle.SetGardenName("") || puzzle.GardenName == "", "a puzzle garden stays unnamed");
    }

    private static void SaveRoundTrip()
    {
        WorldModel world = Named("Kingfisher Run");
        WorldSnapshot snapshot = world.Export();
        Expect(snapshot.gardenName == "Kingfisher Run", "the snapshot carries the name");
        WorldModel restored;
        Expect(WorldModel.TryRestore(snapshot, out restored) && restored.GardenName == "Kingfisher Run", "TryRestore keeps the name");

        string text = GardenTransfer.Export(world, Stamp);
        Expect(text.Contains("\"gardenName\":\"Kingfisher Run\""), "the garden file has the name");
        WorldModel imported;
        GardenImportError error;
        Expect(GardenTransfer.TryImport(text, out imported, out error), "the garden file imports: " + error);
        Expect(imported.GardenName == "Kingfisher Run", "the imported garden keeps the name");
        Expect(GardenTransfer.Export(imported, Stamp) == text, "re-export is byte-identical");

        WorldModel unnamed = WorldModel.New(PlayMode.Living);
        Expect(!GardenTransfer.Export(unnamed, Stamp).Contains("gardenName"), "an unnamed garden writes no name key");
    }

    private static void OldSavesRestoreEmpty()
    {
        WorldSnapshot snapshot = WorldModel.New(PlayMode.Living).Export();
        snapshot.gardenName = null;
        WorldModel restored;
        Expect(WorldModel.TryRestore(snapshot, out restored) && restored.GardenName == "", "a save without the field restores unnamed");
        WorldSnapshot older = WorldModel.New(PlayMode.Living).Export();
        older.gardenName = "";
        Expect(WorldModel.TryRestore(older, out restored) && restored.GardenName == "", "an empty name restores unnamed");
    }

    private static void ForgedValuesRefused()
    {
        WorldModel living = WorldModel.New(PlayMode.Living);
        string[] forged = { " Willow", "Willow ", "Wil\nlow", "<script>", Repeat("a", 25), "a\u0001b", "Pool  Bend" };
        foreach (string name in forged)
        {
            WorldSnapshot snapshot = living.Export();
            snapshot.gardenName = name;
            WorldModel restored;
            Expect(!WorldModel.TryRestore(snapshot, out restored), "a forged name is refused: " + name.Replace("\n", "\\n"));
        }

        WorldSnapshot puzzle = WorldModel.New(PlayMode.PuzzleOnly).Export();
        puzzle.gardenName = "Willow Bend";
        WorldModel ignored;
        Expect(!WorldModel.TryRestore(puzzle, out ignored), "a puzzle garden cannot hold a name");

        string text = GardenTransfer.Export(Named("Willow Bend"), Stamp);
        string forgedText = text.Replace("\"gardenName\":\"Willow Bend\"", "\"gardenName\":\"<Willow Bend>\"");
        WorldModel imported;
        GardenImportError error;
        Expect(forgedText != text && !GardenTransfer.TryImport(forgedText, out imported, out error), "a forged garden file is refused");
    }

    private static void SuggestionIsDeterministic()
    {
        var known = new System.Collections.Generic.HashSet<string>();
        for (int reach = 0; reach < 20; reach++) known.Add(RiverNames.For(reach));
        for (int garden = 0; garden < 2000; garden++)
        {
            string suggestion = WorldModel.SuggestedName(garden);
            Expect(suggestion.Length > 0 && known.Contains(suggestion), "a suggestion is a river name for garden " + garden);
            Expect(WorldModel.SuggestedName(garden) == suggestion, "the suggestion is deterministic for garden " + garden);
            WorldModel world = WorldModel.New(PlayMode.Living);
            Expect(world.SetGardenName(suggestion), "every suggestion is a valid name: " + suggestion);
        }
        Expect(WorldModel.SuggestedName(1) != WorldModel.SuggestedName(2) || WorldModel.SuggestedName(2) != WorldModel.SuggestedName(3),
            "suggestions vary between gardens");
    }

    private static void PostcardShowsNameOnlyWhenSet()
    {
        const string stream = "Stream 1 / 12";
        WorldModel plain = WorldModel.New(PlayMode.Living);
        string before = GardenPostcard.Build(plain, stream);
        Expect(!before.Contains("From "), "an unnamed postcard has no name line");
        Expect(GardenPostcard.Build(plain, stream) == before, "the unnamed postcard is unchanged");

        WorldModel named = Named("Willow Bend");
        string card = GardenPostcard.Build(named, stream);
        Expect(card.Contains("From Willow Bend\n"), "the named postcard shows the name");
        Expect(card == before.Replace("Mosselora\n", "Mosselora\nFrom Willow Bend\n"), "only the name line differs");

        named.SetGardenName("");
        Expect(GardenPostcard.Build(named, stream) == before, "clearing the name restores the postcard exactly");
    }
}
