using System;
using System.Linq;
using Evergrove.Core;

/// <summary>
/// I263 Solved together: a garden remembers which things were solved by taking turns on one device.
/// A private memory only; it never changes seeds, fruit, solves or anything else.
/// </summary>
internal static class TogetherTests
{
    private static int assertions;

    private static void Expect(bool ok, string message)
    {
        assertions++;
        if (!ok) throw new Exception("Together test failed: " + message);
    }

    private static WorldModel Grown(int count)
    {
        WorldModel world = WorldModel.New(PlayMode.Living);
        for (int chapter = 0; chapter < count; chapter++)
        {
            Puzzle board = world.CurrentPuzzle;
            for (int i = 0; i < board.CellCount; i++)
                while (world.Rotations[i] != board.Solution[i]) world.Rotate(i);
            world.Choose((VillageChoice)(chapter % 3));
        }
        return world;
    }

    /// <summary>Six grown things (0 and 4 solved) and two bought plants (the first solved).</summary>
    private static WorldModel Garden()
    {
        WorldModel w = Grown(6);
        WorldSnapshot s = w.Export();
        s.seeds = 500;
        Expect(WorldModel.TryRestore(s, out w), "rich garden restores");
        w.SolveTree(0);
        w.SolveTree(4);
        Expect(w.Buy(2, 60) && w.Buy(0, 61), "buy two plants");
        w.SolveBought(0);
        return w;
    }

    private static string Strip(string text)
    {
        int start = text.IndexOf(",\"together\":", StringComparison.Ordinal);
        Expect(start >= 0, "text has the together key to strip");
        return text.Substring(0, start) + "}";
    }

    private static string Snapshot(string envelope)
    {
        const string marker = "\"snapshot\":";
        int start = envelope.IndexOf(marker, StringComparison.Ordinal) + marker.Length;
        return envelope.Substring(start, envelope.Length - start - 1);
    }

    private static string Envelope(string snapshot, string checksum)
    {
        return "{\"format\":\"mosselora-garden\",\"formatVersion\":1,\"exportedAtUtc\":\"2026-11-27T12:00:00Z" +
            "\",\"checksumSha256\":\"" + checksum + "\",\"snapshot\":" + snapshot + "}";
    }

    private static WorldSnapshot Tweak(WorldSnapshot s, Action<WorldSnapshot> change) { change(s); return s; }

    public static int Run()
    {
        Marking();
        Refusing();
        Untouched();
        Saving();
        Tampering();
        Older();
        // I271: a neighbour's note kept in the letterbox (through the same letters as family).
        WorldModel notes = WorldModel.New(PlayMode.Living);
        Expect(notes.KeepNote("Mira", "The river runs further now. Come and see the new bank."), "Mira's note kept");
        Expect(notes.Letters.Length == 1 && notes.LetterFrom[0] == "Mira", "it is in the letterbox, from Mira");
        Expect(!notes.KeepNote("", "words") && !notes.KeepNote("Mira", "") && !notes.KeepNote("Mira", new string('a', 400)),
            "an empty sender, an empty or overlong note is refused");
        Expect(!WorldModel.New(PlayMode.PuzzleOnly).KeepNote("Mira", "hello"), "only a living garden keeps notes");
        int seedsBefore = notes.Seeds;
        for (int n = 0; n < WorldModel.MaxLetters + 5; n++) notes.KeepNote("Orin", "Note " + n);
        Expect(notes.Letters.Length == WorldModel.MaxLetters, "notes share the letterbox's cap, oldest leave first");
        Expect(notes.Seeds == seedsBefore, "a note moves nothing");
        WorldModel back;
        Expect(WorldModel.TryRestore(notes.Export(), out back) && back.Letters.Length == WorldModel.MaxLetters,
            "notes survive a save");
        foreach (Season season in new[] { Season.Spring, Season.Summer, Season.Autumn, Season.Winter })
            Expect(WorldModel.New(PlayMode.Living).KeepNote("Mira", NeighbourNotice.RiverNote(season)),
                "Mira's river note is a valid letter in " + season);
        return assertions;
    }

    private static void Marking()
    {
        WorldModel w = Garden();
        Expect(w.TogetherCount == 0 && w.Together.Length == 0 && !w.SolvedTogether(0), "a new garden has none");
        Expect(WorldModel.BoughtBase == 2000, "bought plants start at 2000");
        Expect(w.MarkTogether(4) && w.SolvedTogether(4) && w.TogetherCount == 1, "a solved story thing can be marked");
        Expect(w.MarkTogether(WorldModel.BoughtBase + 0) && w.SolvedTogether(2000), "a solved bought plant can be marked");
        Expect(w.MarkTogether(0), "another story thing");
        Expect(w.Together.SequenceEqual(new[] { 0, 4, 2000 }), "kept sorted ascending");
        Expect(w.MarkTogether(4) && w.TogetherCount == 3, "marking twice keeps one entry and says yes");
        int[] copy = w.Together;
        copy[0] = 99;
        Expect(w.Together[0] == 0, "Together is a copy");
        Expect(!w.SolvedTogether(1) && !w.SolvedTogether(-1) && !w.SolvedTogether(2001), "unmarked things are not together");
    }

    private static void Refusing()
    {
        WorldModel w = Garden();
        Expect(!w.MarkTogether(1) && !w.MarkTogether(2000 + 1), "unsolved story thing and unsolved bought plant refused");
        Expect(!w.MarkTogether(-1) && !w.MarkTogether(6) && !w.MarkTogether(999), "out of range story indices refused");
        Expect(!w.MarkTogether(2002) && !w.MarkTogether(1999) && !w.MarkTogether(2000 + 5000), "out of range bought indices refused");
        Expect(w.TogetherCount == 0, "refusals change nothing");
        WorldModel puzzle = WorldModel.New(PlayMode.PuzzleOnly);
        Expect(!puzzle.MarkTogether(0) && puzzle.TogetherCount == 0, "puzzle-only refuses");
        Expect(!WorldModel.NewMatched(Enumerable.Repeat(1, WorldModel.ScheduleLength).ToArray()).MarkTogether(0), "matched refuses");
    }

    private static void Untouched()
    {
        WorldModel w = Garden();
        WorldSnapshot before = w.Export();
        Expect(w.MarkTogether(0) && w.MarkTogether(2000) && w.MarkTogether(0), "mark things");
        WorldSnapshot after = w.Export();
        Expect(after.together.Length == 2, "the new field changed");
        string a = GardenTransfer.SnapshotJson(Tweak(before, s => s.together = Array.Empty<int>()));
        string b = GardenTransfer.SnapshotJson(Tweak(after, s => s.together = Array.Empty<int>()));
        Expect(a == b, "nothing else changes: seeds, fruit, solves, river, all equal");
        Expect(w.PuzzlesSolved == Garden().PuzzlesSolved && w.Seeds == Garden().Seeds, "solve count and seeds unchanged");
    }

    private static void Saving()
    {
        WorldModel w = Garden();
        w.MarkTogether(4);
        w.MarkTogether(2000);
        Expect(WorldModel.TryRestore(w.Export(), out WorldModel back), "restores");
        Expect(back.Together.SequenceEqual(new[] { 4, 2000 }) && back.SolvedTogether(2000), "survives a save");
        Expect(GardenTransfer.SnapshotJson(back.Export()) == GardenTransfer.SnapshotJson(w.Export()), "identical state");

        string file = GardenTransfer.Export(w, "2026-11-27T12:00:00Z");
        Expect(file.Contains("\"together\":[4,2000]"), "the garden file carries the key");
        Expect(GardenTransfer.TryImport(file, out WorldModel imported, out GardenImportError error) &&
               imported.Together.SequenceEqual(new[] { 4, 2000 }), "garden round trip (" + error + ")");
        Expect(GardenTransfer.Export(imported, "2026-11-27T12:00:00Z") == file, "byte-identical export");
        Expect(!GardenTransfer.TryImport(file.Replace("\"together\":[4,2000]", "\"together\":[0,4,2000]"), out _, out error) &&
               error == GardenImportError.ChecksumMismatch, "edited together fails its checksum");
        Expect(!GardenTransfer.TryImport(file.Replace("\"together\":[4,2000]", "\"together\":\"4\""), out _, out _), "wrong type refused");
    }

    private static void Tampering()
    {
        WorldModel g = Garden();
        g.MarkTogether(0);
        g.MarkTogether(4);
        Expect(WorldModel.TryRestore(g.Export(), out _), "setup restores");
        Expect(!WorldModel.TryRestore(Tweak(g.Export(), s => s.together = new[] { 4, 0 }), out _), "unsorted refused");
        Expect(!WorldModel.TryRestore(Tweak(g.Export(), s => s.together = new[] { 4, 4 }), out _), "duplicate refused");
        Expect(!WorldModel.TryRestore(Tweak(g.Export(), s => s.together = new[] { -1 }), out _), "negative refused");
        Expect(!WorldModel.TryRestore(Tweak(g.Export(), s => s.together = new[] { 6 }), out _), "story index at Choices.Length refused");
        Expect(WorldModel.TryRestore(Tweak(g.Export(), s => s.together = new[] { 5 }), out _), "last story index accepted");
        Expect(!WorldModel.TryRestore(Tweak(g.Export(), s => s.together = new[] { 100 }), out _), "between the ranges refused");
        Expect(!WorldModel.TryRestore(Tweak(g.Export(), s => s.together = new[] { 1999 }), out _), "just below the bought range refused");
        Expect(WorldModel.TryRestore(Tweak(g.Export(), s => s.together = new[] { 2000, 2001 }), out _), "both bought plants accepted");
        Expect(!WorldModel.TryRestore(Tweak(g.Export(), s => s.together = new[] { 2002 }), out _), "bought index past BoughtCount refused");
        Expect(WorldModel.TryRestore(Tweak(g.Export(), s => s.together = Enumerable.Range(0, 6).ToArray()), out _), "all story things accepted");
        // Too many entries: needs 200+ plants' worth of range, so a long list is refused whatever it holds.
        Expect(!WorldModel.TryRestore(Tweak(g.Export(), s => s.together = Enumerable.Range(2000, 201).ToArray()), out _), "more than 200 refused");
        Expect(WorldModel.TryRestore(Tweak(g.Export(), s => s.together = null), out WorldModel bare) && bare.TogetherCount == 0,
            "null restores empty");
        Expect(!WorldModel.TryRestore(Tweak(WorldModel.New(PlayMode.PuzzleOnly).Export(), s => s.together = new[] { 0 }), out _),
            "puzzle-only holding any refused");
        Expect(!WorldModel.TryRestore(Tweak(WorldModel.NewMatched(Enumerable.Repeat(1, WorldModel.ScheduleLength).ToArray()).Export(),
            s => s.together = new[] { 0 }), out _), "matched holding any refused");
    }

    private static void Older()
    {
        WorldModel w = Garden();
        w.MarkTogether(4);
        string file = GardenTransfer.Export(w, "2026-11-27T12:00:00Z");
        string snapshot = Snapshot(file);

        // A garden file from before this change (fruit keys present, no together key) imports empty.
        WorldModel older = Garden();
        string oldText = Strip(Snapshot(GardenTransfer.Export(older, "2026-11-27T12:00:00Z")));
        Expect(!oldText.Contains("together") && oldText.Contains("\"fruit\":"), "the older form keeps fruit keys, lacks together");
        Expect(GardenTransfer.TryImport(Envelope(oldText, GardenTransfer.Sha256Hex(oldText)), out WorldModel fromOld, out GardenImportError error) &&
               fromOld.TogetherCount == 0 && fromOld.Fruit == older.Fruit && fromOld.BoughtCount == 2,
            "a pre-together garden file imports with none (" + error + ")");

        // Together cannot ride under this or any older checksum.
        string[] olderForms = {
            Strip(snapshot),
            UndoHistoryTests.RemoveMapKeys(snapshot),
        };
        foreach (string stripped in olderForms)
        {
            string forged = Envelope(snapshot, GardenTransfer.Sha256Hex(stripped));
            Expect(!GardenTransfer.TryImport(forged, out _, out error) && error == GardenImportError.ChecksumMismatch,
                "together data cannot ride under an older checksum");
        }
        // Nor under a pre-fruit or pre-help checksum, even for a garden with no fruit.
        WorldSnapshot bare = WorldModel.New(PlayMode.Living).Export();
        bare.together = Array.Empty<int>();
        string clean = GardenTransfer.SnapshotJson(bare);
        string preFruit = clean.Substring(0, clean.IndexOf(",\"fruit\":", StringComparison.Ordinal)) + "}";
        WorldSnapshot smuggle = Garden().Export();
        Expect(GardenTransfer.TryImport(Envelope(preFruit, GardenTransfer.Sha256Hex(preFruit)), out _, out _),
            "a clean pre-fruit file still imports");
        string smuggled = GardenTransfer.SnapshotJson(Tweak(smuggle, s => s.together = new[] { 0 }));
        string smuggledOld = smuggled.Substring(0, smuggled.IndexOf(",\"fruit\":", StringComparison.Ordinal)) + "}";
        Expect(!GardenTransfer.TryImport(Envelope(smuggled, GardenTransfer.Sha256Hex(smuggledOld)), out _, out _),
            "together cannot ride under a pre-fruit checksum");
    }
}
