using System;
using Evergrove.Core;

/// <summary>
/// I226 Help: a relative solves one of your things' puzzles while visiting; it blooms in your
/// garden when you open the parcel. Costs nothing, earns nothing, never touches your own solves.
/// </summary>
internal static class FamilyHelpTests
{
    private static int assertions;

    private static void Expect(bool ok, string message)
    {
        assertions++;
        if (!ok) throw new Exception("Family help test failed: " + message);
    }

    private static string State(WorldModel w) { return GardenTransfer.SnapshotJson(w.Export()); }

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

    private static string Help(WorldModel sender, string from, string letter, int item, string nonce)
    {
        ParcelSendResult r = sender.SendParcel(from, letter, ParcelGift.Help, item, nonce);
        Expect(r.Ok, "a help parcel is sent (" + r.Error + ")");
        return r.Text;
    }

    public static int Run()
    {
        Format();
        Sending();
        Opening();
        Replacing();
        Saving();
        Notices();
        return assertions;
    }

    private static void Format()
    {
        Expect((int)ParcelGift.Help == 3, "Help is gift 3");
        Expect(FamilyParcel.TryCreate("Nan", "Mind the roots.", ParcelGift.Help, 4, "h1", out string text,
                   out ParcelContents made, out ParcelError error) && error == ParcelError.None, "a help parcel is made");
        Expect(made.Gift == ParcelGift.Help && made.Kind == 4 && made.Amount == 0, "the item is stored in Kind, amount is 0");
        Expect(text.Contains("\"kind\":4") && text.Contains("\"formatVersion\":1,"), "same JSON keys and format version");
        Expect(FamilyParcel.TryRead(text, out ParcelContents read, out error) && read.Id == made.Id &&
               read.Gift == ParcelGift.Help && read.Kind == 4 && read.Amount == 0 && read.Letter == "Mind the roots.", "it reads back");
        Expect(FamilyParcel.TryCreate("Nan", "", ParcelGift.Help, 0, "h2", out _, out _, out _), "item 0 is valid, no letter needed");
        Expect(FamilyParcel.TryCreate("Nan", "", ParcelGift.Help, PuzzleCatalog.Count - 1, "h3", out _, out _, out _), "the last item is valid");
        Expect(!FamilyParcel.TryCreate("Nan", "", ParcelGift.Help, -1, "h4", out _, out _, out error) && error == ParcelError.Invalid, "negative item refused");
        Expect(!FamilyParcel.TryCreate("Nan", "", ParcelGift.Help, PuzzleCatalog.Count, "h5", out _, out _, out error) && error == ParcelError.Invalid, "item 12 refused");
        // Existing gifts are untouched.
        Expect(FamilyParcel.TryCreate("Nan", "Hi", ParcelGift.None, 7, "h6", out _, out ParcelContents none, out _) && none.Kind == -1 && none.Amount == 0, "None still ignores value");
        Expect(FamilyParcel.TryCreate("Nan", "", ParcelGift.Seeds, 5, "h7", out _, out ParcelContents seeds, out _) && seeds.Kind == -1 && seeds.Amount == 5, "Seeds unchanged");
        Expect(FamilyParcel.TryCreate("Nan", "", ParcelGift.Cutting, 2, "h8", out _, out ParcelContents cut, out _) && cut.Kind == 2 && cut.Amount == 0, "Cutting unchanged");
        // A forged help parcel with an amount is refused on read.
        ParcelContents forged = new ParcelContents { From = "Nan", Letter = "", Gift = ParcelGift.Help, Amount = 9, Kind = 1, Nonce = "f1" };
        forged.Id = FamilyParcel.MakeId(forged);
        Expect(!FamilyParcel.TryRead(FamilyParcel.BuildText(forged), out _, out error) && error == ParcelError.Invalid, "help carrying an amount refused");
    }

    private static void Sending()
    {
        WorldModel sender = Grown(3);
        WorldSnapshot s = sender.Export();
        s.seeds = 40;
        Expect(WorldModel.TryRestore(s, out sender), "restore sender");
        string before = State(sender);
        ParcelSendResult r = sender.SendParcel("Anh", "Look at this one.", ParcelGift.Help, 1, "s1");
        Expect(r.Ok && r.Cost == 0 && sender.Seeds == 40, "sending help costs nothing");
        Expect(sender.HasSeenParcel(r.Id), "the id is recorded as seen");
        Expect(sender.SendParcel("Anh", "Look at this one.", ParcelGift.Help, 1, "s1").Error == ParcelError.AlreadyUsed, "same help twice is AlreadyUsed");
        Expect(!sender.SendParcel("Anh", "", ParcelGift.Help, 99, "s2").Ok, "bad item refused");
        Expect(sender.Helped.Length == 0, "sending never marks the sender's own garden");
        Expect(WorldModel.New(PlayMode.PuzzleOnly).SendParcel("Anh", "", ParcelGift.Help, 0, "s3").Error == ParcelError.NotLiving, "help needs a living garden");
        Expect(before != State(sender), "the id was added to the sender's list");
    }

    private static void Opening()
    {
        WorldModel sender = Grown(1);
        string text = Help(sender, "Anh", "I did the one by the gate.", 1, "o1");

        WorldModel small = Grown(1);
        string beforeSmall = State(small);
        ParcelOpenResult refused = small.Open(text);
        Expect(!refused.Ok && refused.Error == ParcelError.NotInThisGarden, "a garden with fewer things refuses it");
        Expect(State(small) == beforeSmall && small.Letters.Length == 0 && !small.HasSeenParcel(
            FamilyParcel.TryRead(text, out ParcelContents c, out _) ? c.Id : ""), "the refusal changed nothing");

        WorldModel w = Grown(6);
        WorldSnapshot snap = w.Export();
        snap.seeds = 25;
        Expect(WorldModel.TryRestore(snap, out w), "restore recipient");
        int solvedBefore = w.PuzzlesSolved;
        ParcelOpenResult open = w.Open(text);
        Expect(open.Ok && open.HelpedItem == 1 && open.Contents.Gift == ParcelGift.Help, "it opens and says which item");
        Expect(w.IsHelped(1) && w.HelpedBy(1) == "Anh" && !w.IsHelped(0) && w.HelpedBy(0) == null, "the item is helped by Anh");
        Expect(w.Helped.Length == 1 && w.Helped[0] == 1 && w.HelpedFrom[0] == "Anh", "arrays hold item and sender");
        Expect(w.Seeds == 25 && open.SeedsGained == 0, "seeds unchanged by opening");
        Expect(!w.TreeSolved(1) && w.PuzzlesSolved == solvedBefore, "never marks solved or counts as a solve");
        Expect(w.Letters.Length == 1 && w.Letters[0] == "I did the one by the gate." && w.LetterFrom[0] == "Anh", "the letter is kept");
        Expect(w.Open(text).Error == ParcelError.AlreadyOpened, "open twice refused");
        Expect(w.Helped.Length == 1, "and nothing changed");
        Expect(open.GiftIndex == -1, "no basket gift");
        // The owner still earns the full first-solve reward.
        Expect(w.SolveTree(1) == WorldModel.TreeReward(TreePuzzles.TierFor(1), true) && w.TreeSolved(1), "the owner's own first solve still pays");
        Expect(w.IsHelped(1), "the help stays on record");
        // Clones: the arrays are copies.
        w.Helped[0] = 9;
        Expect(w.Helped[0] == 1, "Helped is a copy");

        // A forged parcel with a bad item is refused at open without changing anything.
        ParcelContents forged = new ParcelContents { From = "Nan", Letter = "", Gift = ParcelGift.Help, Amount = 0, Kind = 99, Nonce = "o9" };
        forged.Id = FamilyParcel.MakeId(forged);
        string before = State(w);
        Expect(w.Open(FamilyParcel.BuildText(forged)).Error == ParcelError.Invalid && State(w) == before, "a forged item is Invalid and changes nothing");

        WorldModel puzzle = WorldModel.New(PlayMode.PuzzleOnly);
        Expect(puzzle.Open(text).Error == ParcelError.NotLiving, "opening needs a living garden");
    }

    private static void Replacing()
    {
        WorldModel w = Grown(6);
        Expect(w.Open(Help(Grown(1), "Anh", "", 2, "r1")).Ok, "first help");
        Expect(w.Open(Help(Grown(1), "Bà", "", 0, "r2")).Ok, "second item");
        Expect(w.Open(Help(Grown(1), "Chú Tư", "Again!", 2, "r3")).Ok, "same item again");
        Expect(w.Helped.Length == 2 && string.Join(",", w.Helped) == "0,2" && string.Join("|", w.HelpedFrom) == "Bà|Chú Tư",
            "re-help replaces the sender and moves to newest: " + string.Join(",", w.Helped));
        Expect(w.HelpedBy(2) == "Chú Tư", "newest sender wins");
        // Cap.
        Expect(WorldModel.MaxHelped >= PuzzleCatalog.Count, "the cap covers every item");
    }

    private static string Snapshot(string envelope)
    {
        const string marker = "\"snapshot\":";
        int start = envelope.IndexOf(marker, StringComparison.Ordinal) + marker.Length;
        return envelope.Substring(start, envelope.Length - start - 1);
    }

    private static string StripHelpKeys(string text)
    {
        int start = text.IndexOf(",\"helped\":", StringComparison.Ordinal);
        if (start < 0) return text;
        int end = text.IndexOf(']', text.IndexOf(",\"helpedFrom\":", start, StringComparison.Ordinal));
        int fruitKeys = text.IndexOf(",\"fruit\":", start, StringComparison.Ordinal);
        if (fruitKeys >= 0) end = text.IndexOf('}', fruitKeys) - 1; // I228 fruit keys come last
        return text.Substring(0, start) + text.Substring(end + 1);
    }

    private static string Envelope(string snapshot, string checksum)
    {
        return "{\"format\":\"mosselora-garden\",\"formatVersion\":1,\"exportedAtUtc\":\"2026-11-27T12:00:00Z" +
            "\",\"checksumSha256\":\"" + checksum + "\",\"snapshot\":" + snapshot + "}";
    }

    private static WorldSnapshot Tweak(WorldSnapshot s, Action<WorldSnapshot> change) { change(s); return s; }

    private static void Saving()
    {
        WorldModel w = Grown(6);
        w.Open(Help(Grown(1), "Anh", "Hello", 4, "v1"));
        w.Open(Help(Grown(1), "Bà \"Ba\"", "", 1, "v2"));
        Expect(WorldModel.TryRestore(w.Export(), out WorldModel back), "a garden with help restores");
        Expect(string.Join(",", back.Helped) == "4,1" && back.HelpedBy(1) == "Bà \"Ba\"" && back.HelpedBy(4) == "Anh", "help survives a save");

        string file = GardenTransfer.Export(w, "2026-11-27T12:00:00Z");
        Expect(file.Contains("\"helped\":[4,1]") && file.Contains("\"helpedFrom\":[\"Anh\""), "the garden file carries the help keys");
        Expect(GardenTransfer.TryImport(file, out WorldModel imported, out GardenImportError error) &&
               string.Join(",", imported.Helped) == "4,1" && imported.HelpedBy(4) == "Anh", "garden file round trip (" + error + ")");
        Expect(GardenTransfer.Export(imported, "2026-11-27T12:00:00Z") == file, "byte-identical export");
        Expect(!GardenTransfer.TryImport(file.Replace("\"helped\":[4,1]", "\"helped\":[5,1]"), out _, out error) &&
               error == GardenImportError.ChecksumMismatch, "an edited item fails its checksum");

        // A garden file from before Help (no help keys, but with family data) still imports.
        string text = Snapshot(GardenTransfer.Export(Grown(6), "2026-11-27T12:00:00Z"));
        WorldModel older = Grown(6);
        string oldText = StripHelpKeys(Snapshot(GardenTransfer.Export(older, "2026-11-27T12:00:00Z")));
        Expect(oldText != text && !oldText.Contains("helped"), "the older form lacks the help keys");
        string legacy = Envelope(oldText, GardenTransfer.Sha256Hex(oldText));
        Expect(GardenTransfer.TryImport(legacy, out WorldModel fromOld, out error) && fromOld.Helped.Length == 0 && fromOld.HelpedFrom.Length == 0,
            "a pre-help garden file imports with nothing helped (" + error + ")");
        // And one that has family data.
        WorldModel withFamily = Grown(6);
        WorldSnapshot fs = withFamily.Export();
        fs.seeds = 10;
        WorldModel.TryRestore(fs, out withFamily);
        withFamily.Open(WorldModel.New(PlayMode.Living).SendParcel("Anh", "A note", ParcelGift.None, 0, "fam1").Text);
        string famText = StripHelpKeys(Snapshot(GardenTransfer.Export(withFamily, "2026-11-27T12:00:00Z")));
        Expect(famText.Contains("letters\":[\"A note"), "family data stays in the older form");
        Expect(GardenTransfer.TryImport(Envelope(famText, GardenTransfer.Sha256Hex(famText)), out WorldModel fam2, out error) &&
               fam2.Letters.Length == 1 && fam2.Helped.Length == 0, "pre-help file with letters imports (" + error + ")");
        // Help data cannot ride under the pre-help checksum.
        string snapshot = Snapshot(file);
        string forged = Envelope(snapshot, GardenTransfer.Sha256Hex(StripHelpKeys(snapshot)));
        Expect(!GardenTransfer.TryImport(forged, out _, out error) && error == GardenImportError.ChecksumMismatch,
            "help data cannot ride under a pre-help checksum");
        Expect(!GardenTransfer.TryImport(file.Replace("\"helped\":[4,1]", "\"helped\":\"4\""), out _, out _), "wrong type refused");

        // Tampered snapshots fail closed.
        WorldModel g = Grown(6);
        g.Open(Help(Grown(1), "Anh", "", 2, "t1"));
        Expect(WorldModel.TryRestore(g.Export(), out _), "setup restores");
        Expect(!WorldModel.TryRestore(Tweak(g.Export(), s => s.helped = new[] { 1, 2 }), out _), "mismatched lengths refused");
        Expect(!WorldModel.TryRestore(Tweak(g.Export(), s => { s.helped = new[] { 6 }; s.helpedFrom = new[] { "A" }; }), out _), "item beyond grown things refused");
        Expect(!WorldModel.TryRestore(Tweak(g.Export(), s => { s.helped = new[] { -1 }; s.helpedFrom = new[] { "A" }; }), out _), "negative item refused");
        Expect(!WorldModel.TryRestore(Tweak(g.Export(), s => { s.helped = new[] { 2, 2 }; s.helpedFrom = new[] { "A", "B" }; }), out _), "duplicate item refused");
        Expect(!WorldModel.TryRestore(Tweak(g.Export(), s => s.helpedFrom = new[] { "" }), out _), "empty name refused");
        Expect(!WorldModel.TryRestore(Tweak(g.Export(), s => s.helpedFrom = new[] { "A\nB" }), out _), "newline in name refused");
        Expect(!WorldModel.TryRestore(Tweak(g.Export(), s => s.helpedFrom = new string[] { null }), out _), "null name refused");
        Expect(!WorldModel.TryRestore(Tweak(g.Export(), s => { s.helped = new int[WorldModel.MaxHelped + 1]; s.helpedFrom = new string[WorldModel.MaxHelped + 1]; }), out _), "too many refused");
        Expect(WorldModel.TryRestore(Tweak(g.Export(), s => { s.helped = null; s.helpedFrom = null; }), out WorldModel bare) && bare.Helped.Length == 0,
            "absent arrays restore as empty");
        WorldSnapshot puzzle = WorldModel.New(PlayMode.PuzzleOnly).Export();
        Expect(!WorldModel.TryRestore(Tweak(puzzle, s => { s.helped = new[] { 0 }; s.helpedFrom = new[] { "A" }; }), out _), "help needs the living garden");
        Expect(!WorldModel.TryRestore(Tweak(WorldModel.New(PlayMode.Living).Export(), s => { s.helped = new[] { 0 }; s.helpedFrom = new[] { "A" }; }), out _),
            "no grown things, nothing to help");
    }

    private static void Notices()
    {
        WorldModel w = Grown(9); // items 0..8 cycle tree, building, flowers
        for (int v = 0; v < 3; v++) Expect(NeighbourNotice.Line(v, w) == null, "nothing yet");
        w.Open(Help(Grown(1), "Anh", "", 1, "n1")); // item 1: building -> Orin
        string orin = NeighbourNotice.Line(1, w);
        Expect(orin != null && orin.Contains("Anh") && orin.Contains("painting"), "Orin names Anh: " + orin);
        Expect(NeighbourNotice.Line(0, w) == null && NeighbourNotice.Line(2, w) == null, "only the right neighbour says it");
        w.Open(Help(Grown(1), "Bà", "", 2, "n2")); // flowers -> Fen, newest
        string fen = NeighbourNotice.Line(2, w);
        Expect(fen != null && fen.Contains("Bà") && fen.Contains("flower"), "Fen names Bà: " + fen);
        Expect(NeighbourNotice.Line(1, w) == null, "only the newest helped item speaks");
        w.Open(Help(Grown(1), "Chú", "", 0, "n3")); // trees -> Mira
        string mira = NeighbourNotice.Line(0, w);
        Expect(mira != null && mira.Contains("Chú") && mira.Contains("tree"), "Mira names Chú: " + mira);
        foreach (string line in new[] { orin, fen, mira })
        {
            string lower = line.ToLowerInvariant();
            foreach (string word in new[] { "miss", "lonely", "come back", "streak", "don't forget", "hurry", "only today", "waiting for you", "love you" })
                Expect(!lower.Contains(word), "no pressure or fake intimacy (" + word + "): " + line);
        }
        Expect(NeighbourNotice.Line(0, w) == mira, "deterministic");
        w.SolveTree(0);
        string after = NeighbourNotice.Line(0, w);
        Expect(after == null || !after.Contains("Chú"), "once the owner solves it, the help line gives way: " + after);
        // A waiting gift still comes first.
        WorldSnapshot gifted = w.Export();
        gifted.gifts = new[] { 2 };
        WorldModel.TryRestore(gifted, out WorldModel withGift);
        withGift.Open(Help(Grown(1), "Bà", "", 5, "n4")); // flowers
        string waiting = NeighbourNotice.Line(2, withGift);
        Expect(waiting != null && waiting.ToLowerInvariant().Contains("basket"), "a waiting gift is said first: " + waiting);
    }
}
