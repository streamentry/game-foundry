using System;
using System.Text;
using Evergrove.Core;

/// <summary>
/// Layer 5, file-based: a gift travels as a small checksummed text parcel shared through any
/// messaging app. Gifts MOVE seeds; they never create them. A parcel opens once per garden.
/// </summary>
internal static class FamilyParcelTests
{
    private static int assertions;

    private static void Expect(bool ok, string message)
    {
        assertions++;
        if (!ok) throw new Exception("Family parcel test failed: " + message);
    }

    private static WorldModel WithSeeds(int seeds)
    {
        WorldSnapshot s = WorldModel.New(PlayMode.Living).Export();
        s.seeds = seeds;
        Expect(WorldModel.TryRestore(s, out WorldModel w), "a garden with " + seeds + " seeds restores");
        return w;
    }

    private static string State(WorldModel w) { return GardenTransfer.SnapshotJson(w.Export()); }

    private static WorldSnapshot Tweak(WorldSnapshot s, Action<WorldSnapshot> change) { change(s); return s; }

    public static int Run()
    {
        WrapTests();
        Format();
        Sending();
        Opening();
        Economics();
        Basket();
        Letters();
        Tampering();
        Limits();
        Saving();
        Restoring();
        return assertions;
    }

    private static void Format()
    {
        Expect(FamilyParcel.TryCreate("  Nan  ", "Thinking of you.", ParcelGift.Seeds, 50, "a1", out string text,
                   out ParcelContents made, out ParcelError error) && error == ParcelError.None, "a parcel is made");
        Expect(made.From == "Nan" && made.Letter == "Thinking of you." && made.Gift == ParcelGift.Seeds &&
               made.Amount == 50 && made.Kind == -1 && made.Nonce == "a1", "its contents are normalised");
        Expect(text.StartsWith("{\"format\":\"mosselora-parcel\",\"formatVersion\":1,", StringComparison.Ordinal) &&
               text.Contains("\"checksumSha256\":\"") && Encoding.UTF8.GetByteCount(text) < FamilyParcel.MaxParcelBytes,
               "it is small text with a format name, version and checksum");
        Expect(made.Id.Length == 24 && FamilyParcel.IsParcelId(made.Id), "its id is 24 hex characters");
        Expect(FamilyParcel.TryRead(text, out ParcelContents read, out error) && read.Id == made.Id &&
               read.From == "Nan" && read.Amount == 50 && read.Gift == ParcelGift.Seeds, "it reads back");
        Expect(FamilyParcel.TryRead("\n  " + text + "  \n", out read, out error), "stray spaces around pasted text are fine");
        FamilyParcel.TryCreate("Nan", "Thinking of you.", ParcelGift.Seeds, 50, "a1", out string again, out ParcelContents c2, out error);
        Expect(again == text && c2.Id == made.Id, "same contents and nonce give the same text and id");
        FamilyParcel.TryCreate("Nan", "Thinking of you.", ParcelGift.Seeds, 50, "a2", out _, out ParcelContents c3, out error);
        Expect(c3.Id != made.Id, "a different nonce gives a different id");
        FamilyParcel.TryCreate("Nan", "Thinking of you.", ParcelGift.Seeds, 51, "a1", out _, out ParcelContents c4, out error);
        Expect(c4.Id != made.Id, "a different amount gives a different id");
        Expect(FamilyParcel.TryCreate("Joe", "Love\nfrom the garden", ParcelGift.Cutting, 4, "n", out text, out made, out error) &&
               made.Kind == 4 && made.Amount == 0 && FamilyParcel.TryRead(text, out read, out error) &&
               read.Letter == "Love\nfrom the garden" && read.Kind == 4 && read.Gift == ParcelGift.Cutting,
               "a cutting parcel keeps its kind and a two-line letter");
        Expect(FamilyParcel.TryCreate("Joe", "Hello", ParcelGift.None, 0, "n", out text, out made, out error) &&
               made.Gift == ParcelGift.None && made.Amount == 0 && made.Kind == -1 && FamilyParcel.TryRead(text, out read, out error),
               "a letter-only parcel");
    }

    private static void Sending()
    {
        WorldModel w = WithSeeds(300);
        ParcelSendResult sent = w.SendParcel("Anh", "Happy birthday", ParcelGift.Seeds, 120, "k1");
        Expect(sent.Ok && sent.Error == ParcelError.None && sent.Cost == 120 && w.Seeds == 180, "sending 120 seeds subtracts exactly 120");
        Expect(FamilyParcel.TryRead(sent.Text, out ParcelContents c, out _) && c.Amount == 120 && c.Id == sent.Id,
            "the returned text is a readable parcel");

        WorldModel poor = WithSeeds(100);
        string before = State(poor);
        Expect(!poor.SendParcel("Anh", "", ParcelGift.Seeds, 101, "k").Ok && State(poor) == before, "cannot send more seeds than you have");
        Expect(poor.SendParcel("Anh", "", ParcelGift.Seeds, 101, "k").Error == ParcelError.NotEnoughSeeds, "and the reason is named");
        Expect(poor.SendParcel("Anh", "", ParcelGift.Seeds, 100, "k").Ok && poor.Seeds == 0, "sending all of them leaves exactly zero");
        Expect(!poor.SendParcel("Anh", "", ParcelGift.Seeds, 1, "k2").Ok && poor.Seeds == 0, "and never below zero");

        WorldModel rich = WithSeeds(5000);
        before = State(rich);
        foreach (int bad in new[] { 0, -1, 501, int.MaxValue, int.MinValue })
            Expect(!rich.SendParcel("Anh", "", ParcelGift.Seeds, bad, "k").Ok && State(rich) == before, "seed amount " + bad + " refused");
        Expect(rich.SendParcel("Anh", "", ParcelGift.Seeds, 500, "k").Ok && rich.Seeds == 4500, "500 is the most one parcel may carry");

        // A cutting costs its nursery price; it removes no plant; the row must be open.
        WorldModel g = WithSeeds(1000);
        Expect(g.Buy(0, 5) && g.Seeds == 950, "setup: one row 0 plant");
        ParcelSendResult cut = g.SendParcel("Anh", "", ParcelGift.Cutting, 1, "c1");
        Expect(cut.Ok && cut.Cost == 50 && g.Seeds == 900 && g.BoughtCount == 1, "a row 0 cutting costs 50 and removes nothing");
        cut = g.SendParcel("Anh", "", ParcelGift.Cutting, 4, "c2");
        Expect(cut.Ok && cut.Cost == 100 && g.Seeds == 800, "a row 1 cutting costs 100 (row 1 is open)");
        before = State(g);
        Expect(!g.SendParcel("Anh", "", ParcelGift.Cutting, 7, "c3").Ok && State(g) == before, "a row 2 cutting is refused while row 2 is closed");
        Expect(g.SendParcel("Anh", "", ParcelGift.Cutting, 7, "c3").Error == ParcelError.CuttingNotAvailable, "with its own reason");
        foreach (int badKind in new[] { -1, Nursery.Kinds, 100 })
            Expect(!g.SendParcel("Anh", "", ParcelGift.Cutting, badKind, "c4").Ok && State(g) == before, "kind " + badKind + " refused");
        WorldModel low = WithSeeds(49);
        Expect(!low.SendParcel("Anh", "", ParcelGift.Cutting, 0, "c").Ok && low.Seeds == 49, "a cutting you cannot afford is refused");
        WorldModel lowRow = WithSeeds(60);
        Expect(!lowRow.SendParcel("Anh", "", ParcelGift.Cutting, 3, "c").Ok && lowRow.Seeds == 60, "row 1 is closed before any row 0 plant");

        // Letter-only costs nothing.
        WorldModel l = WithSeeds(7);
        ParcelSendResult note = l.SendParcel("Anh", "Missing you", ParcelGift.None, 0, "l1");
        Expect(note.Ok && note.Cost == 0 && l.Seeds == 7, "a letter costs nothing");
        Expect(WithSeeds(0).SendParcel("Anh", "Missing you", ParcelGift.None, 0, "l1").Ok, "even with no seeds at all");
        Expect(!l.SendParcel("Anh", "", ParcelGift.None, 0, "l2").Ok && !l.SendParcel("Anh", "  \n ", ParcelGift.None, 0, "l2").Ok,
            "an empty letter with no gift is refused");

        // Names, letters and nonces.
        before = State(l);
        string[] badNames = { "", "   ", new string('a', 25), "Ann\nBob", "Ann\tBob", "Ann\u0000", "A[n]n", "<b>Ann</b>", "Ann‮Bob", "Ann\u0007", "​" };
        foreach (string name in badNames)
            Expect(!l.SendParcel(name, "Hi", ParcelGift.None, 0, "n").Ok && State(l) == before, "name refused: " + name.Length + " chars");
        Expect(l.SendParcel(new string('a', 24), "Hi", ParcelGift.None, 0, "n1").Ok, "24 characters is the longest name");
        Expect(l.SendParcel("Nguyễn Thị Hoa", "Hi", ParcelGift.None, 0, "n2").Ok, "names may use any letters");
        Expect(l.SendParcel("Mrs. O'Neil-Jo, (Gran)!", "Hi", ParcelGift.None, 0, "n3").Ok, "basic punctuation in a name is fine");
        string[] badLetters = { new string('x', 281), "1\n2\n3\n4\n5\n6\n7", "bad\u0000char", "bad\u0007bell", "tab\there", "a[b]", "x{y}", "x<y>", "\\", "line\r\nfeed" + new string('x', 300) };
        foreach (string letter in badLetters)
            Expect(!l.SendParcel("Ann", letter, ParcelGift.None, 0, "m").Ok, "letter refused: " + letter.Length + " chars");
        Expect(l.SendParcel("Ann", new string('x', 280), ParcelGift.None, 0, "m1").Ok, "280 characters is the longest letter");
        Expect(l.SendParcel("Ann", "1\n2\n3\n4\n5\n6", ParcelGift.None, 0, "m2").Ok, "six lines is the most");
        Expect(l.SendParcel("Ann", "a \"quoted\" word, and: it's (fine) & fair; ok? yes!", ParcelGift.None, 0, "m3").Ok, "ordinary punctuation and quotes pass");
        Expect(l.SendParcel("Ann", "Windows\r\nline ends\r\nare tidied", ParcelGift.None, 0, "m4").Ok, "CRLF line ends are normalised");
        foreach (string nonce in new[] { "", null, new string('n', 33), "a b", "a\"b", "é" })
            Expect(!l.SendParcel("Ann", "Hi", ParcelGift.None, 0, nonce).Ok, "nonce refused");
        Expect(l.SendParcel("Ann", "Hi", ParcelGift.None, 0, "dup").Ok, "a parcel is sent");
        Expect(!l.SendParcel("Ann", "Hi", ParcelGift.None, 0, "dup").Ok && l.SendParcel("Ann", "Hi", ParcelGift.None, 0, "dup").Error == ParcelError.AlreadyUsed,
            "the same parcel (same contents and nonce) cannot be sent twice from one garden");
        Expect(!l.SendParcel("Ann", "Hi", (ParcelGift)7, 0, "z").Ok, "an unknown gift kind is refused");

        // Only a Living garden can send.
        WorldModel puzzle = WorldModel.New(PlayMode.PuzzleOnly);
        Expect(!puzzle.SendParcel("Ann", "Hi", ParcelGift.None, 0, "p").Ok &&
               puzzle.SendParcel("Ann", "Hi", ParcelGift.None, 0, "p").Error == ParcelError.NotLiving, "a puzzle-only garden cannot send");

        // The sender cannot open their own parcel (that would create seeds once the relative opens it too).
        WorldModel me = WithSeeds(200);
        string mine = me.SendParcel("Ann", "", ParcelGift.Seeds, 100, "own").Text;
        Expect(me.Seeds == 100 && !me.Open(mine).Ok && me.Open(mine).Error == ParcelError.AlreadyOpened && me.Seeds == 100,
            "the sender cannot open their own parcel");
    }

    private static void Opening()
    {
        WorldModel from = WithSeeds(400);
        string seedParcel = from.SendParcel("Anh", "Buy something pretty", ParcelGift.Seeds, 150, "s1").Text;
        WorldModel to = WithSeeds(10);
        ParcelOpenResult r = to.Open(seedParcel);
        Expect(r.Ok && r.Error == ParcelError.None && r.SeedsGained == 150 && to.Seeds == 160, "opening adds exactly the seeds sent");
        Expect(r.Contents.From == "Anh" && r.Contents.Gift == ParcelGift.Seeds && r.Contents.Amount == 150 && r.Contents.Letter == "Buy something pretty",
            "the result says who sent what");
        Expect(to.Letters.Length == 1 && to.Letters[0] == "Buy something pretty" && to.LetterFrom[0] == "Anh", "the letter is kept");
        string after = State(to);
        ParcelOpenResult again = to.Open(seedParcel);
        Expect(!again.Ok && again.Error == ParcelError.AlreadyOpened && again.SeedsGained == 0 && State(to) == after,
            "opening the same parcel twice gives nothing the second time");
        Expect(to.HasSeenParcel(r.Contents.Id), "the garden remembers the id");
        WorldModel saved;
        Expect(WorldModel.TryRestore(to.Export(), out saved) && !saved.Open(seedParcel).Ok, "even after a save and reload");
        Expect(WorldModel.TryRestore(WorldModel.New(PlayMode.Living).Export(), out WorldModel other) && other.Open(seedParcel).Ok,
            "a different garden may open it (a parcel is not addressed)");

        string cutParcel = WithSeedsAndRow0(500).SendParcel("Anh", "", ParcelGift.Cutting, 2, "c1").Text;
        WorldModel getter = WithSeeds(5);
        r = getter.Open(cutParcel);
        Expect(r.Ok && r.SeedsGained == 0 && getter.Seeds == 5 && getter.Gifts.Length == 1 && getter.Gifts[0] == 2 && r.GiftIndex == 0,
            "a cutting lands in the basket and changes no seeds");
        Expect(getter.Letters.Length == 0, "no letter, nothing kept");
        Expect(getter.BoughtCount == 0, "it is not a plant until it is planted");

        string noteParcel = WithSeeds(0).SendParcel("Oma", "Just a note", ParcelGift.None, 0, "l").Text;
        WorldModel reader = WithSeeds(3);
        r = reader.Open(noteParcel);
        Expect(r.Ok && r.SeedsGained == 0 && reader.Seeds == 3 && reader.Gifts.Length == 0 && reader.Letters.Length == 1, "a letter-only parcel keeps only the letter");

        // Only a Living garden can open.
        WorldModel puzzle = WorldModel.New(PlayMode.PuzzleOnly);
        string pb = State(puzzle);
        r = puzzle.Open(seedParcel);
        Expect(!r.Ok && r.Error == ParcelError.NotLiving && State(puzzle) == pb, "a puzzle-only garden cannot open a parcel");
    }

    private static WorldModel WithSeedsAndRow0(int seeds)
    {
        WorldModel w = WithSeeds(seeds + 50);
        Expect(w.Buy(0, 1), "setup plant");
        return w;
    }

    private static void Economics()
    {
        // Seeds are conserved: the sender loses exactly what the recipient gains.
        foreach (int amount in new[] { 1, 37, 250, 500 })
        {
            WorldModel a = WithSeeds(1000), b = WithSeeds(321);
            int total = a.Seeds + b.Seeds;
            Expect(b.Open(a.SendParcel("A", "", ParcelGift.Seeds, amount, "e" + amount).Text).Ok, "parcel of " + amount + " opens");
            Expect(a.Seeds + b.Seeds == total && a.Seeds == 1000 - amount && b.Seeds == 321 + amount, "amount " + amount + " is conserved");
        }
        // A cutting is paid for by the sender.
        for (int kind = 0; kind < Nursery.Kinds; kind++)
        {
            WorldModel a = WithSeeds(5000);
            for (int row = 0; row < Nursery.Rows - 1; row++) Expect(a.Buy(row * 3, row), "setup row " + row);
            int seeds = a.Seeds;
            ParcelSendResult s = a.SendParcel("A", "", ParcelGift.Cutting, kind, "k" + kind);
            Expect(s.Ok && a.Seeds == seeds - Nursery.Price(kind) && s.Cost == Nursery.Price(kind), "kind " + kind + " costs its price");
            WorldModel b = WithSeeds(0);
            Expect(b.Open(s.Text).Ok && b.Seeds == 0 && b.Gifts[0] == kind, "kind " + kind + " arrives free of charge to the recipient");
        }
        // The recipient's seed total is capped, never exceeded.
        WorldModel full = WithSeeds(WorldModel.MaxSeeds - 10);
        ParcelOpenResult capped = full.Open(WithSeeds(500).SendParcel("A", "", ParcelGift.Seeds, 500, "cap").Text);
        Expect(capped.Ok && full.Seeds == WorldModel.MaxSeeds && capped.SeedsGained == 10, "seeds stop at the cap and the result says how many arrived");
        // Never negative anywhere: send everything, then nothing is left to send.
        WorldModel zero = WithSeeds(500);
        Expect(zero.SendParcel("A", "", ParcelGift.Seeds, 500, "z").Ok && zero.Seeds == 0 && !zero.SendParcel("A", "", ParcelGift.Seeds, 1, "z2").Ok,
            "the sender is never driven below zero");
        // A planted gift costs nothing and earns nothing by itself.
        WorldModel b2 = WithSeeds(100);
        b2.Open(WithSeedsAndRow0(50).SendParcel("A", "", ParcelGift.Cutting, 1, "pg").Text);
        Expect(b2.Seeds == 100 && b2.PlantGift(0, 9) && b2.Seeds == 100, "planting a gift does not touch seeds");
    }

    private static void Basket()
    {
        WorldModel b = WithSeeds(0);
        WorldModel a = WithSeedsAndRow0(100000);
        for (int i = 0; i < WorldModel.MaxGifts; i++)
            Expect(b.Open(a.SendParcel("A", "", ParcelGift.Cutting, i % 3, "g" + i).Text).Ok, "gift " + i + " arrives");
        Expect(b.Gifts.Length == WorldModel.MaxGifts, "the basket holds " + WorldModel.MaxGifts);
        string full = a.SendParcel("A", "extra", ParcelGift.Cutting, 0, "gx").Text;
        string before = State(b);
        ParcelOpenResult r = b.Open(full);
        Expect(!r.Ok && r.Error == ParcelError.BasketFull && State(b) == before, "a full basket refuses, changing nothing (not even the letter)");
        Expect(b.PlantGift(0, 3), "make room");
        Expect(b.Open(full).Ok, "the refused parcel opens later, because it was never marked opened");
        // Seeds still arrive when the basket is full.
        WorldModel full2 = WithSeeds(0);
        WorldModel a2 = WithSeedsAndRow0(100000);
        for (int i = 0; i < WorldModel.MaxGifts; i++) full2.Open(a2.SendParcel("A", "", ParcelGift.Cutting, 0, "h" + i).Text);
        Expect(full2.Open(a2.SendParcel("A", "", ParcelGift.Seeds, 20, "hs").Text).Ok && full2.Seeds == 20, "a full basket still takes seeds");

        // Planting a gift.
        WorldModel w = WithSeeds(0);
        w.Open(WithSeedsAndRow0(900).SendParcel("A", "", ParcelGift.Cutting, 1, "pa").Text);
        w.Open(WithSeedsAndRow0(900).SendParcel("A", "", ParcelGift.Cutting, 2, "pb").Text);
        w.Open(WithSeedsAndRow0(900).SendParcel("A", "", ParcelGift.Cutting, 0, "pc").Text);
        Expect(string.Join(",", w.Gifts) == "1,2,0", "gifts wait in arrival order");
        before = State(w);
        Expect(!w.PlantGift(-1, 5) && !w.PlantGift(3, 5) && !w.PlantGift(0, -1) && !w.PlantGift(0, WorldModel.MaxMapCell) && State(w) == before,
            "bad index or cell is refused and changes nothing");
        Expect(w.PlantGift(1, 44) && string.Join(",", w.Gifts) == "1,0" && string.Join(",", w.Bought) == "2,44" && w.BoughtCount == 1,
            "planting gift 1 moves it to a bought plant at that cell");
        Expect(w.RowOpen(1) && !w.RowOpen(2), "a gifted row 0 plant opens row 1 once it is planted");
        Expect(w.SolveBought(0) == WorldModel.TreeReward(0, true) && w.BoughtSolved(0), "a gifted plant holds a puzzle like any bought plant");
        Expect(w.MoveBought(0, 45), "and can be moved");
        WorldModel puzzle = WorldModel.New(PlayMode.PuzzleOnly);
        Expect(!puzzle.PlantGift(0, 1), "a puzzle-only garden has no gifts to plant");
        // The bought list is full: gifts cannot be planted.
        WorldSnapshot snap = WithSeeds(0).Export();
        snap.bought = new int[WorldModel.MaxBought * 2];
        snap.gifts = new[] { 1 };
        Expect(WorldModel.TryRestore(snap, out WorldModel crowded) && !crowded.PlantGift(0, 3) && crowded.Gifts.Length == 1,
            "a garden with the most plants cannot plant a gift");
        // Accessors hand out copies.
        int[] copy = w.Gifts; copy[0] = 8;
        Expect(w.Gifts[0] != 8, "Gifts is a copy");
    }

    private static void Letters()
    {
        WorldModel a = WithSeeds(100000);
        WorldModel b = WithSeeds(0);
        for (int i = 0; i < WorldModel.MaxLetters + 5; i++)
            Expect(b.Open(a.SendParcel("From" + i, "Letter " + i, ParcelGift.None, 0, "L" + i).Text).Ok, "letter " + i + " opens");
        Expect(b.Letters.Length == WorldModel.MaxLetters && b.LetterFrom.Length == WorldModel.MaxLetters, "only the newest " + WorldModel.MaxLetters + " are kept");
        Expect(b.Letters[WorldModel.MaxLetters - 1] == "Letter " + (WorldModel.MaxLetters + 4) && b.LetterFrom[WorldModel.MaxLetters - 1] == "From" + (WorldModel.MaxLetters + 4),
            "newest last");
        Expect(b.Letters[0] == "Letter 5" && b.LetterFrom[0] == "From5", "the oldest five fell away");
        string[] copy = b.Letters; copy[0] = "changed";
        Expect(b.Letters[0] == "Letter 5", "Letters is a copy");
        copy = b.LetterFrom; copy[0] = "changed";
        Expect(b.LetterFrom[0] == "From5", "LetterFrom is a copy");
        Expect(WorldModel.TryRestore(b.Export(), out WorldModel back) && string.Join("|", back.Letters) == string.Join("|", b.Letters) &&
               string.Join("|", back.LetterFrom) == string.Join("|", b.LetterFrom), "letters survive a save");
        // Old parcel ids fall away after 200, newest kept.
        WorldModel c = WithSeeds(0);
        WorldModel s = WithSeeds(0);
        string first = null;
        for (int i = 0; i < WorldModel.MaxParcelIds + 3; i++)
        {
            string text = FamilyParcelFor("W", "id" + i);
            if (i == 0) first = text;
            Expect(c.Open(text).Ok, "parcel " + i + " opens");
        }
        Expect(c.Export().parcelIds.Length == WorldModel.MaxParcelIds, "the garden remembers the newest " + WorldModel.MaxParcelIds + " parcels");
        Expect(!c.Open(FamilyParcelFor("W", "id" + (WorldModel.MaxParcelIds + 2))).Ok, "a recent parcel still cannot be reopened");
        Expect(s != null && first != null, "setup");
    }

    private static string FamilyParcelFor(string from, string nonce)
    {
        Expect(FamilyParcel.TryCreate(from, "Hello " + nonce, ParcelGift.None, 0, nonce, out string text, out _, out _), "parcel made");
        return text;
    }

    private static void Tampering()
    {
        WorldModel a = WithSeedsAndRow0(900);
        string seeds = a.SendParcel("Anh", "Hello there", ParcelGift.Seeds, 100, "t1").Text;
        string cut = a.SendParcel("Anh", "Hello there", ParcelGift.Cutting, 2, "t2").Text;
        string[] tampered = {
            seeds.Replace("\"amount\":100", "\"amount\":400"),
            seeds.Replace("\"amount\":100", "\"amount\":101"),
            seeds.Replace("\"from\":\"Anh\"", "\"from\":\"Ben\""),
            seeds.Replace("Hello there", "Hello thare"),
            seeds.Replace("\"gift\":\"seeds\"", "\"gift\":\"cutting\""),
            cut.Replace("\"kind\":2", "\"kind\":8"),
            cut.Replace("\"kind\":2", "\"kind\":1"),
            seeds.Replace("\"id\":\"", "\"id\":\"0"),
            seeds.Replace("\"nonce\":\"t1\"", "\"nonce\":\"t9\""),
            ChangeChecksum(seeds),
            ChangeChecksum(cut)
        };
        for (int i = 0; i < tampered.Length; i++)
        {
            Expect(tampered[i] != seeds && tampered[i] != cut, "tamper " + i + " really changed the text");
            WorldModel b = WithSeeds(40);
            string before = State(b);
            ParcelOpenResult r = b.Open(tampered[i]);
            Expect(!r.Ok && r.SeedsGained == 0 && State(b) == before, "tamper " + i + " is refused and changes nothing (" + r.Error + ")");
        }
        Expect(WithSeeds(1).Open(tampered[0]).Error == ParcelError.ChecksumMismatch, "a changed amount fails its checksum");
        Expect(WithSeeds(1).Open(tampered[9]).Error == ParcelError.ChecksumMismatch, "a changed checksum fails");

        // A forger can recompute the checksum, so the rules must also hold the line.
        Forged("amount over the cap", Raw("Anh", "", ParcelGift.Seeds, 501, -1, "f1"));
        Forged("huge amount", Raw("Anh", "", ParcelGift.Seeds, 9999999, -1, "f2"));
        Forged("negative amount", Raw("Anh", "", ParcelGift.Seeds, -50, -1, "f3"));
        Forged("zero seeds", Raw("Anh", "", ParcelGift.Seeds, 0, -1, "f4"));
        Forged("seeds with a kind", Raw("Anh", "", ParcelGift.Seeds, 10, 3, "f5"));
        Forged("cutting with an amount", Raw("Anh", "", ParcelGift.Cutting, 500, 3, "f6"));
        Forged("cutting of unknown kind", Raw("Anh", "", ParcelGift.Cutting, 0, Nursery.Kinds, "f7"));
        Forged("cutting of negative kind", Raw("Anh", "", ParcelGift.Cutting, 0, -1, "f8"));
        Forged("letter-only with seeds", Raw("Anh", "Hi", ParcelGift.None, 300, -1, "f9"));
        Forged("empty letter-only", Raw("Anh", "", ParcelGift.None, 0, -1, "f10"));
        Forged("control character in name", Raw("An\u0001h", "Hi", ParcelGift.None, 0, -1, "f11"));
        Forged("overlong letter", Raw("Anh", new string('x', 281), ParcelGift.None, 0, -1, "f12"));
        Forged("seven lines", Raw("Anh", "1\n2\n3\n4\n5\n6\n7", ParcelGift.None, 0, -1, "f13"));
        Forged("untrimmed name", Raw(" Anh", "Hi", ParcelGift.None, 0, -1, "f14"));
        Forged("bad nonce", Raw("Anh", "Hi", ParcelGift.None, 0, -1, "f 15"));
        // Valid contents with an id that does not belong to them.
        ParcelContents ok = new ParcelContents { From = "Anh", Letter = "Hi", Gift = ParcelGift.None, Amount = 0, Kind = -1, Nonce = "f16" };
        ok.Id = new string('a', 24);
        Forged("id that does not match the contents", FamilyParcel.BuildText(ok));
        Expect(WithSeeds(0).Open(Raw("Anh", "Hi", ParcelGift.None, 0, -1, "fine")).Ok, "the forging helper itself builds openable parcels when the rules are met");
    }

    private static string Raw(string from, string letter, ParcelGift gift, int amount, int kind, string nonce)
    {
        var c = new ParcelContents { From = from, Letter = letter, Gift = gift, Amount = amount, Kind = kind, Nonce = nonce };
        c.Id = FamilyParcel.MakeId(c);
        return FamilyParcel.BuildText(c);
    }

    private static void Forged(string what, string text)
    {
        WorldModel b = WithSeeds(40);
        string before = State(b);
        ParcelOpenResult r = b.Open(text);
        Expect(!r.Ok && r.Error == ParcelError.Invalid && State(b) == before && b.Seeds == 40, "forged parcel refused (" + what + "): " + r.Error);
        Expect(!FamilyParcel.TryRead(text, out _, out ParcelError e) && e == ParcelError.Invalid, "TryRead agrees (" + what + ")");
    }

    private static string ChangeChecksum(string text)
    {
        const string key = "\"checksumSha256\":\"";
        int i = text.IndexOf(key, StringComparison.Ordinal) + key.Length;
        char c = text[i] == '0' ? '1' : '0';
        return text.Substring(0, i) + c + text.Substring(i + 1);
    }

    private static void Limits()
    {
        WorldModel b = WithSeeds(5);
        string before = State(b);
        WorldModel a = WithSeeds(100);
        string good = a.SendParcel("Anh", "Hi", ParcelGift.Seeds, 10, "lim").Text;
        Expect(b.Open(null).Error == ParcelError.Empty && b.Open("").Error == ParcelError.Empty && b.Open("  \n ").Error == ParcelError.Empty, "nothing pasted");
        Expect(b.Open(good + new string(' ', 5000)).Error == ParcelError.TooLarge, "a parcel over 4 KB is too large (even with only spaces)");
        Expect(b.Open("{\"a\":\"" + new string('x', FamilyParcel.MaxParcelBytes) + "\"}").Error == ParcelError.TooLarge, "long junk is too large");
        string multibyte = "{\"a\":\"" + new string('é', FamilyParcel.MaxParcelBytes / 2 - 3) + "\"}";
        Expect(multibyte.Length < FamilyParcel.MaxParcelBytes && b.Open(multibyte).Error == ParcelError.TooLarge, "size is counted in bytes");
        Expect(b.Open("not json at all").Error == ParcelError.Malformed, "words are not a parcel");
        Expect(b.Open("[1,2,3]").Error == ParcelError.Malformed, "an array is not a parcel");
        Expect(b.Open("{\"format\":\"mosselora-parcel\"").Error == ParcelError.Malformed, "cut-off text");
        Expect(b.Open(good + "x").Error == ParcelError.Malformed, "trailing junk");
        Expect(b.Open(good.Replace("\"formatVersion\":1", "\"formatVersion\":1,\"formatVersion\":1")).Error == ParcelError.Malformed, "duplicate keys");
        Expect(b.Open(new string('[', 40) + new string(']', 40)).Error == ParcelError.Malformed, "deep nesting is refused");
        string deep = "{\"format\":\"mosselora-parcel\",\"formatVersion\":1,\"checksumSha256\":\"x\",\"parcel\":" + new string('[', 30) + new string(']', 30) + "}";
        Expect(b.Open(deep).Error == ParcelError.Malformed, "deep nesting inside the parcel is refused");
        Expect(b.Open(good.Replace("mosselora-parcel", "mosselora-garden")).Error == ParcelError.WrongFormat, "another app's format");
        Expect(b.Open(good.Replace("\"formatVersion\":1", "\"formatVersion\":2")).Error == ParcelError.UnsupportedFutureVersion, "a newer parcel version");
        Expect(b.Open(good.Replace("\"formatVersion\":1", "\"formatVersion\":0")).Error == ParcelError.WrongFormat, "version zero");
        Expect(b.Open(good.Replace("\"formatVersion\":1", "\"formatVersion\":1.5")).Error == ParcelError.WrongFormat, "fractional version");
        Expect(b.Open(good.Replace("\"amount\":10", "\"amount\":10.0")).Error == ParcelError.Malformed, "a fractional amount is not an integer");
        Expect(b.Open(good.Replace("\"amount\":10", "\"amount\":\"10\"")).Error == ParcelError.Malformed, "an amount in quotes");
        Expect(b.Open(good.Replace("\"amount\":10", "\"amount\":99999999999")).Error == ParcelError.Malformed, "an amount beyond int");
        Expect(b.Open(good.Replace("\"parcel\":{", "\"parcel\":{\"extra\":1,")).Error == ParcelError.Malformed, "unknown parcel key");
        Expect(b.Open(good.Replace("{\"format\"", "{\"extra\":1,\"format\"")).Error == ParcelError.Malformed, "unknown envelope key");
        Expect(b.Open(good.Replace("\"nonce\":\"lim\",", "")).Error == ParcelError.Malformed, "missing key");
        Expect(b.Open(good.Replace("\"from\":\"Anh\"", "\"from\":null")).Error == ParcelError.Malformed, "null name");
        Expect(b.Open(good.Replace("\"from\":\"Anh\"", "\"from\":5")).Error == ParcelError.Malformed, "a number for the name");
        Expect(b.Open(good.Replace("\"parcel\":{", "\"parcel\":[{")).Error == ParcelError.Malformed, "parcel must be an object");
        Expect(State(b) == before && b.Seeds == 5, "every refusal changed nothing");
        Expect(b.Open(good).Ok && b.Seeds == 15, "the good parcel still opens");
    }

    private static void Saving()
    {
        WorldModel a = WithSeedsAndRow0(1000);
        WorldModel w = WithSeeds(100);
        string sv1 = a.SendParcel("Anh", "Dear you,\nlove Anh", ParcelGift.Cutting, 2, "sv1").Text;
        Expect(w.Open(sv1).Ok, "cutting opened");
        Expect(w.Open(a.SendParcel("Bà \"Ba\"", "Hi", ParcelGift.Seeds, 25, "sv2").Text).Ok, "seeds opened");
        Expect(WorldModel.TryRestore(w.Export(), out WorldModel back), "a garden with family data restores");
        Expect(string.Join(",", back.Gifts) == "2" && back.Seeds == 125 && back.Letters.Length == 2 && back.LetterFrom[1] == "Bà \"Ba\"" &&
               back.Letters[0] == "Dear you,\nlove Anh", "gifts, letters and seeds survive a save");
        Expect(!back.Open(sv1).Ok, "opened parcel ids survive the save");

        string file = GardenTransfer.Export(w, "2026-11-27T12:00:00Z");
        Expect(file.Contains("\"gifts\":[2]") && file.Contains("\"parcelIds\":[\"") && file.Contains("\"letterFrom\":[\"Anh\",\"B"), "the garden file carries the family keys");
        Expect(GardenTransfer.TryImport(file, out WorldModel imported, out GardenImportError error) && string.Join(",", imported.Gifts) == "2" &&
               imported.Letters.Length == 2 && imported.Letters[0] == "Dear you,\nlove Anh" && imported.LetterFrom[1] == "Bà \"Ba\"" &&
               imported.Seeds == 125, "garden file round trip keeps gifts and letters (" + error + ")");
        Expect(GardenTransfer.Export(imported, "2026-11-27T12:00:00Z") == file, "and exports byte-identically");
        Expect(!GardenTransfer.TryImport(file.Replace("\"gifts\":[2]", "\"gifts\":[3]"), out _, out error) && error == GardenImportError.ChecksumMismatch,
            "an edited gift fails its checksum");
        Expect(!GardenTransfer.TryImport(file.Replace("Dear you", "Dear yoo"), out _, out error) && error == GardenImportError.ChecksumMismatch,
            "an edited letter fails its checksum");

        // A garden file from before parcels (no family keys) still imports.
        WorldModel plain = WithSeeds(77);
        Expect(plain.Buy(0, 3), "setup");
        string older = GardenTransfer.Export(plain, "2026-11-27T12:00:00Z");
        string text = Snapshot(older);
        string oldText = StripFamilyKeys(text);
        Expect(oldText != text && !oldText.Contains("gifts") && !oldText.Contains("letters"), "the older form really lacks the family keys");
        string legacy = Envelope(oldText, GardenTransfer.Sha256Hex(oldText));
        Expect(GardenTransfer.TryImport(legacy, out WorldModel fromOld, out error) && fromOld.Gifts.Length == 0 && fromOld.Letters.Length == 0 &&
               fromOld.Seeds == plain.Seeds && fromOld.BoughtCount == 1, "a pre-parcel garden file imports (" + error + ")");
        // The old checksum must not cover a garden that carries family data.
        string snapshot = Snapshot(file);
        string forged = Envelope(snapshot, GardenTransfer.Sha256Hex(StripFamilyKeys(snapshot)));
        Expect(!GardenTransfer.TryImport(forged, out _, out error) && error == GardenImportError.ChecksumMismatch,
            "family data cannot ride under a pre-parcel checksum");
        // Family keys with data are rejected inside a wrong-typed value.
        Expect(!GardenTransfer.TryImport(file.Replace("\"gifts\":[2]", "\"gifts\":\"2\""), out _, out _), "a gifts value of the wrong type is refused");
        Expect(!GardenTransfer.TryImport(file.Replace("\"letters\":[\"Dear you,\\nlove Anh\",\"Hi\"]", "\"letters\":[1,2]"), out _, out _), "letters of the wrong type are refused");
    }

    private static void Restoring()
    {
        WorldModel a = WithSeedsAndRow0(1000);
        WorldModel w = WithSeeds(0);
        w.Open(a.SendParcel("Anh", "Hello", ParcelGift.Cutting, 1, "r1").Text);
        WorldSnapshot good = w.Export();
        Expect(WorldModel.TryRestore(good, out _), "setup restores");
        string id = good.parcelIds[0];
        Expect(!WorldModel.TryRestore(Tweak(w.Export(), s => s.gifts = new[] { Nursery.Kinds }), out _), "unknown gift kind refused");
        Expect(!WorldModel.TryRestore(Tweak(w.Export(), s => s.gifts = new[] { -1 }), out _), "negative gift kind refused");
        Expect(!WorldModel.TryRestore(Tweak(w.Export(), s => s.gifts = new int[WorldModel.MaxGifts + 1]), out _), "too many gifts refused");
        Expect(WorldModel.TryRestore(Tweak(w.Export(), s => s.gifts = new int[WorldModel.MaxGifts]), out _), "the most gifts is fine");
        Expect(!WorldModel.TryRestore(Tweak(w.Export(), s => s.parcelIds = new[] { "XYZ" }), out _), "a short id refused");
        Expect(!WorldModel.TryRestore(Tweak(w.Export(), s => s.parcelIds = new[] { id.ToUpperInvariant() }), out _), "upper-case id refused");
        Expect(!WorldModel.TryRestore(Tweak(w.Export(), s => s.parcelIds = new[] { id, id }), out _), "duplicate ids refused");
        Expect(!WorldModel.TryRestore(Tweak(w.Export(), s => s.parcelIds = new string[] { null }), out _), "null id refused");
        Expect(!WorldModel.TryRestore(Tweak(w.Export(), s => s.parcelIds = NewIds(WorldModel.MaxParcelIds + 1)), out _), "too many ids refused");
        Expect(WorldModel.TryRestore(Tweak(w.Export(), s => s.parcelIds = NewIds(WorldModel.MaxParcelIds)), out _), "the most ids is fine");
        Expect(!WorldModel.TryRestore(Tweak(w.Export(), s => { s.letters = new[] { "Hi" }; s.letterFrom = new string[0]; }), out _), "letters and senders must match in length");
        Expect(!WorldModel.TryRestore(Tweak(w.Export(), s => { s.letters = new string[0]; s.letterFrom = new[] { "Anh" }; }), out _), "senders without letters refused");
        Expect(!WorldModel.TryRestore(Tweak(w.Export(), s => { s.letters = new[] { "" }; s.letterFrom = new[] { "Anh" }; }), out _), "an empty kept letter refused");
        Expect(!WorldModel.TryRestore(Tweak(w.Export(), s => { s.letters = new[] { "Hi\u0000" }; s.letterFrom = new[] { "Anh" }; }), out _), "a control character in a letter refused");
        Expect(!WorldModel.TryRestore(Tweak(w.Export(), s => { s.letters = new[] { "Hi" }; s.letterFrom = new[] { "" }; }), out _), "an empty sender refused");
        Expect(!WorldModel.TryRestore(Tweak(w.Export(), s => { s.letters = new[] { "Hi" }; s.letterFrom = new[] { "A\nB" }; }), out _), "a newline in a sender refused");
        Expect(!WorldModel.TryRestore(Tweak(w.Export(), s => { s.letters = new[] { new string('x', 281) }; s.letterFrom = new[] { "A" }; }), out _), "an overlong letter refused");
        Expect(!WorldModel.TryRestore(Tweak(w.Export(), s => { s.letters = new[] { "Hi" }; s.letterFrom = new string[] { null }; }), out _), "a null sender refused");
        Expect(!WorldModel.TryRestore(Tweak(w.Export(), s => { s.letters = new string[WorldModel.MaxLetters + 1]; s.letterFrom = new string[WorldModel.MaxLetters + 1]; }), out _), "too many letters refused");
        Expect(WorldModel.TryRestore(Tweak(w.Export(), s => { s.letters = null; s.letterFrom = null; s.gifts = null; s.parcelIds = null; }), out WorldModel bare) &&
               bare.Gifts.Length == 0 && bare.Letters.Length == 0, "a save without the family fields loads");
        WorldSnapshot puzzle = WorldModel.New(PlayMode.PuzzleOnly).Export();
        Expect(!WorldModel.TryRestore(Tweak(puzzle, s => s.gifts = new[] { 1 }), out _), "gifts need the living garden");
        puzzle = WorldModel.New(PlayMode.PuzzleOnly).Export();
        Expect(!WorldModel.TryRestore(Tweak(puzzle, s => s.parcelIds = new[] { id }), out _), "parcel ids need the living garden");
        puzzle = WorldModel.New(PlayMode.PuzzleOnly).Export();
        Expect(!WorldModel.TryRestore(Tweak(puzzle, s => { s.letters = new[] { "Hi" }; s.letterFrom = new[] { "A" }; }), out _), "letters need the living garden");
        Expect(WorldModel.New(PlayMode.Living).Export().gifts.Length == 0 && WorldModel.New(PlayMode.Living).Export().parcelIds.Length == 0,
            "a new garden has no family data");
    }

    private static string[] NewIds(int count)
    {
        var ids = new string[count];
        for (int i = 0; i < count; i++) ids[i] = i.ToString("x24");
        return ids;
    }

    private static string Snapshot(string envelope)
    {
        const string marker = "\"snapshot\":";
        int start = envelope.IndexOf(marker, StringComparison.Ordinal) + marker.Length;
        return envelope.Substring(start, envelope.Length - start - 1);
    }

    private static string StripFamilyKeys(string text)
    {
        int start = text.IndexOf(",\"gifts\":", StringComparison.Ordinal);
        if (start < 0) return text;
        int end = text.IndexOf(']', text.IndexOf(",\"letterFrom\":", start, StringComparison.Ordinal));
        int helpedFrom = text.IndexOf(",\"helpedFrom\":", start, StringComparison.Ordinal);
        if (helpedFrom >= 0) end = text.IndexOf(']', helpedFrom); // I226 help keys follow the family keys
        int fruitKeys = text.IndexOf(",\"fruit\":", start, StringComparison.Ordinal);
        if (fruitKeys >= 0) end = text.IndexOf('}', fruitKeys) - 1; // I228 fruit keys come last
        return text.Substring(0, start) + text.Substring(end + 1);
    }

    private static string Envelope(string snapshot, string checksum)
    {
        return "{\"format\":\"mosselora-garden\",\"formatVersion\":1,\"exportedAtUtc\":\"2026-11-27T12:00:00Z" +
            "\",\"checksumSha256\":\"" + checksum + "\",\"snapshot\":" + snapshot + "}";
    }

    // I214: the parcel travels as a friendly line plus one token that messaging
    // apps can't "smarten" (no quotes, no spaces), and is found anywhere in a paste.
    private static void WrapTests()
    {
        Expect(FamilyParcel.TryCreate("Nan", "Thinking of you.", ParcelGift.Seeds, 50, "w1",
            out string raw, out ParcelContents made, out ParcelError e0), "create a parcel to wrap (" + e0 + ")");
        string shared = FamilyParcel.Wrap(raw, "Nan sent you a parcel from Mosselora.");
        Expect(shared.StartsWith("Nan sent you a parcel from Mosselora."), "the friendly line comes first");
        Expect(shared.IndexOf('"') < 0 && shared.IndexOf('{') < 0, "no quotes or braces for an app to change");
        Expect(FamilyParcel.Unwrap(shared) == raw, "unwraps to the same parcel");
        string pasted = "Fwd: \u201csee below\u201d\n\n" + shared.Replace("\n", "\r\n") + "\n\nSent from my phone";
        Expect(FamilyParcel.Unwrap(pasted) == raw, "found inside a forwarded message with other text around it");
        Expect(FamilyParcel.TryRead(FamilyParcel.Unwrap(shared), out ParcelContents back, out ParcelError e1) &&
               back.Id == made.Id, "and still reads, checksum intact (" + e1 + ")");
        Expect(FamilyParcel.Unwrap(raw) == raw, "a raw parcel passes straight through");
        Expect(FamilyParcel.Unwrap("hello there") == null && FamilyParcel.Unwrap(null) == null, "no token, no parcel");
        string broken = shared.Substring(0, shared.Length - 6);
        string got = FamilyParcel.Unwrap(broken);
        Expect(got == null || !FamilyParcel.TryRead(got, out _, out _), "a cut-off token is refused, never half-read");
    }
}
