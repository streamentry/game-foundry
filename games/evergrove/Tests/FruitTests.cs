using System;
using System.Linq;
using Evergrove.Core;

/// <summary>
/// I228 Fruit: food picked by tending trees, never by time, shared with family and neighbours.
/// Fruit comes only from solving puzzles under tree things; gifts MOVE it; nothing converts to or from seeds.
/// </summary>
internal static class FruitTests
{
    private static int assertions;

    private static void Expect(bool ok, string message)
    {
        assertions++;
        if (!ok) throw new Exception("Fruit test failed: " + message);
    }

    private static string State(WorldModel w) { return GardenTransfer.SnapshotJson(w.Export()); }

    // Items 0..n-1 cycle tree, building, flowers (Choices[i] == i % 3).
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

    private static WorldModel WithFruit(WorldModel w, int fruit)
    {
        WorldSnapshot s = w.Export();
        s.fruit = fruit;
        Expect(WorldModel.TryRestore(s, out WorldModel back), "restore with " + fruit + " fruit");
        return back;
    }

    private static WorldSnapshot Tweak(WorldSnapshot s, Action<WorldSnapshot> change) { change(s); return s; }

    public static int Run()
    {
        Picking();
        Bought();
        Capping();
        Sending();
        Opening();
        Conservation();
        Giving();
        Saving();
        Tampering();
        Notices();
        // I231: the thank-you right after giving, only from the neighbour who got the fruit.
        WorldModel giver = WorldModel.New(PlayMode.Living);
        Expect(NeighbourNotice.ThanksFor(0, giver) == null, "no fruit given, no thanks");
        WorldSnapshot ripe = giver.Export();
        ripe.fruit = 3;
        Expect(WorldModel.TryRestore(ripe, out WorldModel withFruit), "restore with fruit");
        Expect(withFruit.GiveFruit(2), "give Fen a fruit");
        string jam = NeighbourNotice.ThanksFor(2, withFruit);
        Expect(jam != null && jam.Contains("jam"), "Fen thanks you at once: " + jam);
        Expect(NeighbourNotice.ThanksFor(0, withFruit) == null && NeighbourNotice.ThanksFor(1, withFruit) == null,
            "only Fen thanks you");
        Expect(withFruit.GiveFruit(0) && NeighbourNotice.ThanksFor(2, withFruit) == null &&
            NeighbourNotice.ThanksFor(0, withFruit) != null, "the newest gift's neighbour thanks you");
        return assertions;
    }

    private static void Picking()
    {
        WorldModel w = Grown(9);
        Expect(w.Fruit == 0 && WorldModel.MaxFruit == 99, "a fresh garden has no fruit; cap is 99");
        Expect(WorldModel.New(PlayMode.Living).Fruit == 0, "a new garden has none");
        int seeds0 = w.Seeds;
        int reward = w.SolveTree(0);
        Expect(reward == WorldModel.TreeReward(TreePuzzles.TierFor(0), true), "seeds for a first tree solve are unchanged");
        Expect(w.Fruit == 1 && w.Seeds == seeds0 + reward, "first solve of a tree gives one fruit");
        int replay = w.SolveTree(0);
        Expect(replay == WorldModel.TreeReward(TreePuzzles.TierFor(0), false), "replay seeds are unchanged");
        Expect(w.Fruit == 2, "a replay gives one fruit too");
        w.SolveTree(3);
        Expect(w.Fruit == 3, "another tree gives one more");
        // Buildings (1, 4) and flowers (2, 5) give none, first or replay.
        foreach (int item in new[] { 1, 4, 2, 5, 1, 2 })
        {
            int before = w.Fruit;
            int s = w.SolveTree(item);
            Expect(s > 0 && w.Fruit == before, "item " + item + " (not a tree) gives no fruit");
        }
        // Invalid solves give none.
        Expect(w.SolveTree(-1) == 0 && w.SolveTree(99) == 0 && w.Fruit == 3, "bad index gives nothing");
        // Not Living: no fruit and no change.
        WorldModel puzzle = WorldModel.New(PlayMode.PuzzleOnly);
        Expect(puzzle.SolveTree(0) == 0 && puzzle.Fruit == 0, "puzzle-only picks nothing");
        // Fruit and seeds never convert: spending seeds leaves fruit alone.
        WorldSnapshot rich = w.Export();
        rich.seeds = 500;
        WorldModel.TryRestore(rich, out WorldModel buyer);
        int fruit = buyer.Fruit;
        Expect(buyer.Buy(0, 40) && buyer.Fruit == fruit, "buying a plant does not touch fruit");
        Expect(buyer.GiveFruit(0) && buyer.Seeds == 450, "giving fruit does not touch seeds");
    }

    private static void Bought()
    {
        WorldModel w = Grown(3);
        WorldSnapshot rich = w.Export();
        rich.seeds = 1000;
        WorldModel.TryRestore(rich, out w);
        Expect(w.Buy(0, 50) && w.Buy(1, 51) && w.Buy(2, 52), "bought a tree, a building and a flower plant");
        Expect(Nursery.Family(0) == VillageChoice.ShadeTree, "kind 0 is a tree plant");
        int reward = w.SolveBought(0);
        Expect(reward == WorldModel.TreeReward(Nursery.Row(0), true), "bought seeds unchanged");
        Expect(w.Fruit == 1, "a bought tree plant gives fruit");
        w.SolveBought(0);
        Expect(w.Fruit == 2, "its replay gives fruit");
        w.SolveBought(1);
        w.SolveBought(2);
        w.SolveBought(1);
        Expect(w.Fruit == 2, "bought buildings and flowers give none");
        Expect(w.SolveBought(7) == 0 && w.SolveBought(-1) == 0 && w.Fruit == 2, "bad index gives nothing");
        // Fruit comes only from solving: buying and restoring never add it.
        WorldModel.TryRestore(w.Export(), out WorldModel back);
        Expect(back.Fruit == 2, "restore does not change fruit");
    }

    private static void Capping()
    {
        WorldModel w = Grown(1);
        for (int i = 0; i < 99; i++) w.SolveTree(0);
        Expect(w.Fruit == 99, "99 solves, 99 fruit");
        int seeds = w.Seeds;
        int reward = w.SolveTree(0);
        Expect(w.Fruit == 99, "the 100th is simply not picked");
        Expect(reward == WorldModel.TreeReward(TreePuzzles.TierFor(0), false) && w.Seeds == seeds + reward, "seeds still earned at the cap");
        Expect(WorldModel.TryRestore(w.Export(), out _), "99 restores");
    }

    private static WorldModel Gardener(int fruit) { return WithFruit(Grown(3), fruit); }

    private static void Sending()
    {
        Expect((int)ParcelGift.Fruit == 4, "Fruit is gift 4");
        Expect(FamilyParcel.TryCreate("Nan", "Pears!", ParcelGift.Fruit, 3, "p1", out string text, out ParcelContents made, out ParcelError error) &&
               made.Gift == ParcelGift.Fruit && made.Amount == 3 && made.Kind == -1, "a fruit parcel is made, amount holds the count");
        Expect(text.Contains("\"amount\":3") && text.Contains("\"formatVersion\":1,") && text.Contains("\"gift\":\"fruit\""), "same keys and format version");
        Expect(FamilyParcel.TryRead(text, out ParcelContents read, out error) && read.Gift == ParcelGift.Fruit && read.Amount == 3 && read.Id == made.Id, "it reads back");
        Expect(FamilyParcel.TryCreate("Nan", "", ParcelGift.Fruit, 1, "p2", out _, out _, out _), "1 is allowed");
        Expect(FamilyParcel.TryCreate("Nan", "", ParcelGift.Fruit, 10, "p3", out _, out _, out _), "10 is allowed");
        Expect(!FamilyParcel.TryCreate("Nan", "", ParcelGift.Fruit, 0, "p4", out _, out _, out error) && error == ParcelError.Invalid, "0 refused");
        Expect(!FamilyParcel.TryCreate("Nan", "", ParcelGift.Fruit, 11, "p5", out _, out _, out error) && error == ParcelError.Invalid, "11 refused");
        Expect(!FamilyParcel.TryCreate("Nan", "", ParcelGift.Fruit, -3, "p6", out _, out _, out _), "negative refused");
        ParcelContents forged = new ParcelContents { From = "Nan", Letter = "", Gift = ParcelGift.Fruit, Amount = 11, Kind = -1, Nonce = "f1" };
        forged.Id = FamilyParcel.MakeId(forged);
        Expect(!FamilyParcel.TryRead(FamilyParcel.BuildText(forged), out _, out error) && error == ParcelError.Invalid, "a forged 11 is refused on read");
        forged = new ParcelContents { From = "Nan", Letter = "", Gift = ParcelGift.Fruit, Amount = 2, Kind = 1, Nonce = "f2" };
        forged.Id = FamilyParcel.MakeId(forged);
        Expect(!FamilyParcel.TryRead(FamilyParcel.BuildText(forged), out _, out error) && error == ParcelError.Invalid, "fruit with a kind is refused");
        // Old gifts are untouched.
        Expect(FamilyParcel.TryCreate("Nan", "", ParcelGift.Seeds, 5, "o1", out _, out ParcelContents seeds, out _) && seeds.Amount == 5 && seeds.Kind == -1, "seeds unchanged");
        Expect(FamilyParcel.TryCreate("Nan", "", ParcelGift.Cutting, 2, "o2", out _, out ParcelContents cut, out _) && cut.Kind == 2 && cut.Amount == 0, "cutting unchanged");
        Expect(FamilyParcel.TryCreate("Nan", "Hi", ParcelGift.None, 7, "o3", out _, out ParcelContents none, out _) && none.Amount == 0, "letter unchanged");

        // Sending subtracts from the sender.
        WorldModel sender = Gardener(7);
        ParcelSendResult r = sender.SendParcel("Nan", "", ParcelGift.Fruit, 3, "s1");
        Expect(r.Ok && r.Cost == 0 && sender.Fruit == 4, "sending 3 leaves 4");
        Expect(sender.Seeds == 0, "seeds untouched");
        // Not enough: nothing changes.
        string before = State(sender);
        r = sender.SendParcel("Nan", "", ParcelGift.Fruit, 5, "s2");
        Expect(!r.Ok && r.Error == ParcelError.NotEnoughFruit && State(sender) == before, "not enough fruit refused, nothing changed");
        Expect((int)ParcelError.NotEnoughFruit > (int)ParcelError.NotInThisGarden, "new error value appended");
        r = sender.SendParcel("Nan", "", ParcelGift.Fruit, 0, "s3");
        Expect(!r.Ok && r.Error == ParcelError.Invalid && State(sender) == before, "zero refused");
        r = sender.SendParcel("Nan", "", ParcelGift.Fruit, 11, "s4");
        Expect(!r.Ok && State(sender) == before, "eleven refused");
        // All of it can go.
        r = sender.SendParcel("Nan", "", ParcelGift.Fruit, 4, "s5");
        Expect(r.Ok && sender.Fruit == 0, "can send the last of it");
        r = sender.SendParcel("Nan", "", ParcelGift.Fruit, 1, "s6");
        Expect(!r.Ok && r.Error == ParcelError.NotEnoughFruit, "then none left");
        // Same nonce and contents is refused.
        WorldModel again = Gardener(9);
        Expect(again.SendParcel("Nan", "", ParcelGift.Fruit, 2, "dup").Ok, "first send");
        r = again.SendParcel("Nan", "", ParcelGift.Fruit, 2, "dup");
        Expect(!r.Ok && r.Error == ParcelError.AlreadyUsed && again.Fruit == 7, "a repeat send is refused and costs nothing");
        // Not Living.
        r = WorldModel.New(PlayMode.PuzzleOnly).SendParcel("Nan", "", ParcelGift.Fruit, 1, "nl");
        Expect(!r.Ok && r.Error == ParcelError.NotLiving, "puzzle-only cannot send");
    }

    private static string Parcel(WorldModel sender, int n, string nonce)
    {
        ParcelSendResult r = sender.SendParcel("Nan", "Eat well", ParcelGift.Fruit, n, nonce);
        Expect(r.Ok, "send " + n + " (" + r.Error + ")");
        return r.Text;
    }

    private static void Opening()
    {
        WorldModel sender = Gardener(10);
        string text = Parcel(sender, 4, "o1");
        WorldModel to = Gardener(5);
        ParcelOpenResult o = to.Open(text);
        Expect(o.Ok && o.FruitGained == 4 && to.Fruit == 9, "opening adds the fruit");
        Expect(o.SeedsGained == 0 && o.GiftIndex == -1 && o.HelpedItem == -1, "nothing else gained");
        Expect(to.Letters.Length == 1 && to.Letters[0] == "Eat well", "the letter is kept");
        string before = State(to);
        o = to.Open(text);
        Expect(!o.Ok && o.Error == ParcelError.AlreadyOpened && o.FruitGained == 0 && State(to) == before, "opening twice is refused");
        o = sender.Open(text);
        Expect(!o.Ok && o.Error == ParcelError.AlreadyOpened, "a garden cannot open what it sent");
        // Capped.
        WorldModel full = Gardener(97);
        o = full.Open(Parcel(Gardener(10), 5, "o2"));
        Expect(o.Ok && o.FruitGained == 2 && full.Fruit == 99, "capped at 99, reports what arrived");
        o = full.Open(Parcel(Gardener(10), 5, "o3"));
        Expect(o.Ok && o.FruitGained == 0 && full.Fruit == 99, "a full garden gains none but the parcel is spent");
        Expect(full.HasSeenParcel(o.Contents.Id), "it is remembered");
        // Other gifts report no fruit.
        WorldModel seedSender = WithFruit(Grown(3), 0);
        WorldSnapshot ss = seedSender.Export(); ss.seeds = 50; WorldModel.TryRestore(ss, out seedSender);
        string seedText = seedSender.SendParcel("Nan", "", ParcelGift.Seeds, 10, "o4").Text;
        WorldModel any = Gardener(1);
        o = any.Open(seedText);
        Expect(o.Ok && o.FruitGained == 0 && o.SeedsGained == 10 && any.Fruit == 1, "seed parcels give no fruit");
        // Not Living.
        o = WorldModel.New(PlayMode.PuzzleOnly).Open(Parcel(Gardener(10), 1, "o5"));
        Expect(!o.Ok && o.Error == ParcelError.NotLiving, "puzzle-only cannot open");
        // Old parcels read exactly as before: format and ids of existing gifts are unchanged.
        Expect(FamilyParcel.TryCreate("Nan", "x", ParcelGift.Seeds, 5, "n1", out string oldText, out _, out _) &&
               oldText.Contains("\"gift\":\"seeds\",\"amount\":5,\"kind\":-1"), "seed parcel text is unchanged");
    }

    private static void Conservation()
    {
        WorldModel a = Gardener(20), b = Gardener(95);
        int total = a.Fruit + b.Fruit;
        int lost = 0;
        for (int i = 0; i < 4; i++)
        {
            int send = Math.Min(6, a.Fruit);
            if (send == 0) break;
            ParcelOpenResult o = b.Open(Parcel(a, send, "c" + i));
            Expect(o.Ok, "opened");
            lost += send - o.FruitGained;
            Expect(a.Fruit + b.Fruit == total - lost, "sum conserved minus cap losses after parcel " + i);
        }
        Expect(b.Fruit == 99 && lost > 0, "the cap lost some");
        // Back the other way.
        ParcelOpenResult back = a.Open(Parcel(b, 10, "c-back"));
        Expect(back.Ok && back.FruitGained == 10 && a.Fruit + b.Fruit == total - lost, "it moves back too");
        // Fruit never converts to seeds.
        Expect(a.Seeds == 0 && b.Seeds == 0, "no seeds appeared");
    }

    private static void Giving()
    {
        WorldModel w = Gardener(3);
        Expect(w.FruitGivenTo(0) == 0 && w.FruitGivenTo(1) == 0 && w.FruitGivenTo(2) == 0, "nothing given yet");
        Expect(w.GiveFruit(1) && w.Fruit == 2 && w.FruitGivenTo(1) == 1, "Orin gets one");
        Expect(w.GiveFruit(1) && w.Fruit == 1 && w.FruitGivenTo(1) == 2, "and another");
        Expect(w.GiveFruit(2) && w.Fruit == 0 && w.FruitGivenTo(2) == 1 && w.FruitGivenTo(0) == 0, "Fen gets one");
        string before = State(w);
        Expect(!w.GiveFruit(0) && State(w) == before && w.FruitGivenTo(0) == 0, "refused at zero, nothing changes");
        w = Gardener(5);
        Expect(!w.GiveFruit(-1) && !w.GiveFruit(3) && w.Fruit == 5, "only our three neighbours");
        Expect(w.FruitGivenTo(-1) == 0 && w.FruitGivenTo(3) == 0, "out of range reads zero");
        Expect(w.Export().fruitGiven.Length == 3, "three counts in the snapshot");
        WorldModel puzzle = WorldModel.New(PlayMode.PuzzleOnly);
        Expect(!puzzle.GiveFruit(0) && puzzle.Fruit == 0, "no giving in puzzle-only");
        // Giving does not create or convert anything.
        WorldModel g = Gardener(4);
        int seeds = g.Seeds;
        g.GiveFruit(0);
        Expect(g.Seeds == seeds && g.Fruit == 3, "seeds untouched");
        // The record cannot be bought or sent: a parcel's fruit goes to the garden, not to the count.
        WorldModel recv = Gardener(0);
        recv.Open(Parcel(Gardener(5), 2, "g1"));
        Expect(recv.FruitGivenTo(0) == 0 && recv.Fruit == 2, "a parcel changes fruit, never the neighbours' counts");
    }

    private static string Snapshot(string envelope)
    {
        const string marker = "\"snapshot\":";
        int start = envelope.IndexOf(marker, StringComparison.Ordinal) + marker.Length;
        return envelope.Substring(start, envelope.Length - start - 1);
    }

    /// <summary>Drops only the I228 keys (they come last), leaving a pre-fruit snapshot.</summary>
    private static string StripFruitKeys(string text)
    {
        int start = text.IndexOf(",\"fruit\":", StringComparison.Ordinal);
        Expect(start >= 0, "text has fruit keys to strip");
        return text.Substring(0, start) + "}";
    }

    private static string Envelope(string snapshot, string checksum)
    {
        return "{\"format\":\"mosselora-garden\",\"formatVersion\":1,\"exportedAtUtc\":\"2026-11-27T12:00:00Z" +
            "\",\"checksumSha256\":\"" + checksum + "\",\"snapshot\":" + snapshot + "}";
    }

    private static void Saving()
    {
        WorldModel w = Gardener(12);
        w.GiveFruit(0);
        w.GiveFruit(2);
        w.GiveFruit(2);
        Expect(WorldModel.TryRestore(w.Export(), out WorldModel back), "a garden with fruit restores");
        Expect(back.Fruit == 9 && back.FruitGivenTo(0) == 1 && back.FruitGivenTo(1) == 0 && back.FruitGivenTo(2) == 2, "fruit and counts survive a save");
        Expect(back.LastFruitTo == 2, "who got the last one survives too");
        Expect(State(back) == State(w), "identical state");
        Expect(WorldModel.New(PlayMode.Living).LastFruitTo == -1, "nobody yet");

        string file = GardenTransfer.Export(w, "2026-11-27T12:00:00Z");
        Expect(file.Contains("\"fruit\":9") && file.Contains("\"fruitGiven\":[1,0,2]") && file.Contains("\"lastFruitTo\":2"), "the garden file carries the keys");
        Expect(GardenTransfer.TryImport(file, out WorldModel imported, out GardenImportError error) &&
               imported.Fruit == 9 && imported.FruitGivenTo(2) == 2 && imported.LastFruitTo == 2, "garden round trip (" + error + ")");
        Expect(GardenTransfer.Export(imported, "2026-11-27T12:00:00Z") == file, "byte-identical export");
        Expect(!GardenTransfer.TryImport(file.Replace("\"fruit\":9", "\"fruit\":50"), out _, out error) &&
               error == GardenImportError.ChecksumMismatch, "edited fruit fails its checksum");
        Expect(!GardenTransfer.TryImport(file.Replace("\"fruitGiven\":[1,0,2]", "\"fruitGiven\":[1,0,3]"), out _, out error) &&
               error == GardenImportError.ChecksumMismatch, "edited counts fail the checksum");
        Expect(!GardenTransfer.TryImport(file.Replace("\"fruit\":9", "\"fruit\":\"9\""), out _, out _), "wrong type refused");

        // A garden file from before fruit (but with help and family data) imports with none.
        WorldModel older = Grown(6);
        older.Open(WorldModel.New(PlayMode.Living).SendParcel("Anh", "A note", ParcelGift.None, 0, "fr1").Text);
        string oldFile = GardenTransfer.Export(older, "2026-11-27T12:00:00Z");
        string oldText = StripFruitKeys(Snapshot(oldFile));
        Expect(!oldText.Contains("fruit") && oldText.Contains("\"helped\":"), "the older form keeps help keys, lacks fruit keys");
        Expect(GardenTransfer.TryImport(Envelope(oldText, GardenTransfer.Sha256Hex(oldText)), out WorldModel fromOld, out error) &&
               fromOld.Fruit == 0 && fromOld.LastFruitTo == -1 && fromOld.FruitGivenTo(0) == 0 && fromOld.Letters.Length == 1,
            "a pre-fruit garden file imports with fruit 0 (" + error + ")");
        // The same, with help data present.
        WorldModel helped = Grown(6);
        helped.Open(Grown(1).SendParcel("Anh", "", ParcelGift.Help, 4, "fr2").Text);
        string helpText = StripFruitKeys(Snapshot(GardenTransfer.Export(helped, "2026-11-27T12:00:00Z")));
        Expect(GardenTransfer.TryImport(Envelope(helpText, GardenTransfer.Sha256Hex(helpText)), out WorldModel h2, out error) &&
               h2.Helped.Length == 1 && h2.Fruit == 0, "pre-fruit file with help imports (" + error + ")");

        // Fruit cannot ride under any older checksum.
        string snapshot = Snapshot(file);
        string[] older2 = {
            StripFruitKeys(snapshot),
            UndoHistoryTests.RemoveMapKeys(snapshot),
        };
        foreach (string stripped in older2)
        {
            string forged = Envelope(snapshot, GardenTransfer.Sha256Hex(stripped));
            Expect(!GardenTransfer.TryImport(forged, out _, out error) && error == GardenImportError.ChecksumMismatch,
                "fruit data cannot ride under an older checksum");
        }
        // Even if the forger drops fruit but still claims counts.
        Expect(!GardenTransfer.TryImport(Envelope(snapshot, GardenTransfer.Sha256Hex(oldText)), out _, out _), "mixed forgery refused");
        // A file that only lists counts (fruit 0) is not smuggled either: counts set, fruit 0 under the pre-fruit sum.
        WorldSnapshot countsOnly = Gardener(0).Export();
        countsOnly.fruitGiven = new[] { 0, 0, 1 };
        countsOnly.lastFruitTo = 2;
        string countsText = GardenTransfer.SnapshotJson(countsOnly);
        string preCounts = StripFruitKeys(countsText);
        Expect(!GardenTransfer.TryImport(Envelope(countsText, GardenTransfer.Sha256Hex(preCounts)), out _, out _), "counts cannot ride under a pre-fruit checksum");
    }

    private static void Tampering()
    {
        WorldModel g = Gardener(5);
        g.GiveFruit(1);
        Expect(WorldModel.TryRestore(g.Export(), out _), "setup restores");
        Expect(!WorldModel.TryRestore(Tweak(g.Export(), s => s.fruit = -1), out _), "negative fruit refused");
        Expect(!WorldModel.TryRestore(Tweak(g.Export(), s => s.fruit = 100), out _), "more than 99 refused");
        Expect(WorldModel.TryRestore(Tweak(g.Export(), s => s.fruit = 99), out _), "exactly 99 accepted");
        Expect(WorldModel.TryRestore(Tweak(g.Export(), s => s.fruit = 0), out _), "0 accepted");
        Expect(!WorldModel.TryRestore(Tweak(g.Export(), s => s.fruitGiven = new[] { 0, 1 }), out _), "length 2 refused");
        Expect(!WorldModel.TryRestore(Tweak(g.Export(), s => s.fruitGiven = new[] { 0, 1, 0, 0 }), out _), "length 4 refused");
        Expect(!WorldModel.TryRestore(Tweak(g.Export(), s => s.fruitGiven = new[] { 0, -1, 0 }), out _), "negative count refused");
        Expect(!WorldModel.TryRestore(Tweak(g.Export(), s => s.fruitGiven = new[] { 0, 100001, 0 }), out _), "huge count refused");
        Expect(WorldModel.TryRestore(Tweak(g.Export(), s => s.fruitGiven = new[] { 0, 100000, 0 }), out _), "100000 accepted");
        Expect(!WorldModel.TryRestore(Tweak(g.Export(), s => s.lastFruitTo = 3), out _), "last recipient 3 refused");
        Expect(!WorldModel.TryRestore(Tweak(g.Export(), s => s.lastFruitTo = -2), out _), "last recipient -2 refused");
        Expect(!WorldModel.TryRestore(Tweak(g.Export(), s => s.lastFruitTo = 0), out _), "last recipient with a zero count refused");
        Expect(!WorldModel.TryRestore(Tweak(g.Export(), s => s.lastFruitTo = -1), out _), "counts without a last recipient refused");
        Expect(!WorldModel.TryRestore(Tweak(Gardener(5).Export(), s => s.lastFruitTo = 1), out _), "last recipient set while all zero refused");
        Expect(WorldModel.TryRestore(Tweak(g.Export(), s => { s.fruitGiven = null; s.fruit = 0; s.lastFruitTo = -1; }), out WorldModel bare) &&
               bare.FruitGivenTo(1) == 0 && bare.Export().fruitGiven.Length == 3, "absent counts restore as zeros");
        Expect(WorldModel.TryRestore(Tweak(g.Export(), s => s.fruitGiven = new int[0]), out _) == false,
            "empty counts with a set last recipient are refused");
        Expect(WorldModel.TryRestore(Tweak(Gardener(5).Export(), s => s.fruitGiven = new int[0]), out WorldModel zero) && zero.FruitGivenTo(0) == 0,
            "empty counts are zeros");
        // Non-Living modes hold no fruit.
        Expect(!WorldModel.TryRestore(Tweak(WorldModel.New(PlayMode.PuzzleOnly).Export(), s => s.fruit = 1), out _), "puzzle-only with fruit refused");
        Expect(!WorldModel.TryRestore(Tweak(WorldModel.New(PlayMode.PuzzleOnly).Export(), s => { s.fruitGiven = new[] { 1, 0, 0 }; s.lastFruitTo = 0; }), out _),
            "puzzle-only with counts refused");
        Expect(!WorldModel.TryRestore(Tweak(WorldModel.NewMatched(Enumerable.Repeat(1, WorldModel.ScheduleLength).ToArray()).Export(), s => s.fruit = 1), out _), "matched with fruit refused");
    }

    private static readonly string[] Banned = {
        "miss", "lonely", "come back", "streak", "don't forget", "hurry", "only today", "waiting for you", "love you"
    };

    private static void Kind(string line, string label)
    {
        string lower = line.ToLowerInvariant();
        foreach (string word in Banned) Expect(!lower.Contains(word), label + ": no pressure or fake intimacy (" + word + "): " + line);
    }

    private static void Notices()
    {
        WorldModel w = Gardener(6);
        for (int v = 0; v < 3; v++) Expect(NeighbourNotice.Line(v, w) == null, "holding fruit alone says nothing");
        Expect(w.GiveFruit(0), "give to Mira");
        string mira = NeighbourNotice.Line(0, w);
        Expect(mira == "Thank you for the fruit. I'll dry some for the winter.", "Mira's line: " + mira);
        Expect(NeighbourNotice.Line(1, w) == null && NeighbourNotice.Line(2, w) == null, "only the last one given speaks");
        Kind(mira, "Mira");
        Expect(w.GiveFruit(1), "give to Orin");
        string orin = NeighbourNotice.Line(1, w);
        Expect(orin == "The fruit you left on my bench was the best part of the day.", "Orin's line: " + orin);
        Expect(NeighbourNotice.Line(0, w) == null, "Mira stops once someone else got the latest");
        Kind(orin, "Orin");
        Expect(w.GiveFruit(2), "give to Fen");
        string fen = NeighbourNotice.Line(2, w);
        Expect(fen == "I made jam with your fruit. There's a jar with your name on it.", "Fen's line: " + fen);
        Kind(fen, "Fen");
        Expect(NeighbourNotice.Line(2, w) == fen, "deterministic");
        Expect(NeighbourNotice.Line(1, w) == null, "Orin quiet again");

        // The fruit line gives way to the tended count when someone else was the last.
        WorldModel t = Grown(9);
        t.SolveTree(0);
        t.SolveTree(3);
        string tended = NeighbourNotice.Line(0, t);
        Expect(tended != null && tended.Contains("2"), "Mira counts two trees first: " + tended);
        t.GiveFruit(0);
        Expect(NeighbourNotice.Line(0, t) == "Thank you for the fruit. I'll dry some for the winter.", "the fruit line is said now");
        t.GiveFruit(2);
        Expect(NeighbourNotice.Line(0, t) == tended, "and the count returns once Fen was the last");

        // Priority: waiting gift > help > arrival > river > fruit.
        WorldModel p = Grown(9);
        p.SolveTree(0);
        p.SolveTree(3);
        WorldSnapshot ps = p.Export();
        ps.seeds = 500;
        WorldModel.TryRestore(ps, out p);
        p.GiveFruit(2);
        Expect(NeighbourNotice.Line(2, p).Contains("jam"), "Fen says the fruit line when nothing outranks it");
        WorldSnapshot gifted = p.Export();
        gifted.gifts = new[] { 2 };
        WorldModel.TryRestore(gifted, out WorldModel withGift);
        string waiting = NeighbourNotice.Line(2, withGift);
        Expect(waiting != null && waiting.ToLowerInvariant().Contains("basket"), "a waiting gift outranks fruit: " + waiting);
        WorldModel helpedGarden = Grown(9);
        helpedGarden.SolveTree(0);
        WorldSnapshot hs = helpedGarden.Export();
        WorldModel.TryRestore(hs, out helpedGarden);
        helpedGarden.Open(Grown(1).SendParcel("Anh", "", ParcelGift.Help, 2, "pr1").Text); // flowers: Fen
        helpedGarden.GiveFruit(2);
        string helpLine = NeighbourNotice.Line(2, helpedGarden);
        Expect(helpLine != null && helpLine.Contains("Anh"), "help outranks fruit: " + helpLine);
        // Arrival outranks fruit.
        Expect(p.Buy(2, 60), "buy a flower plant");
        string arrival = NeighbourNotice.Line(2, p);
        Expect(arrival != null && arrival.Contains(Nursery.Item(2).Name), "a new arrival outranks fruit: " + arrival);
        p.SolveBought(0);
        Expect(NeighbourNotice.Line(2, p).Contains("jam"), "once solved, fruit speaks again");
        // River line outranks fruit for Mira.
        WorldModel done = Grown(12);
        done.SolveTree(0);
        done.SolveTree(1);
        done.SolveTree(2);
        Expect(done.GiveFruit(0), "Mira gets fruit");
        string near = NeighbourNotice.Line(0, done);
        Expect(near != null && near.ToLowerInvariant().Contains("mist"), "the river line outranks fruit: " + near);
        done.SolveTree(3);
        done.SolveTree(4);
        Expect(NeighbourNotice.Line(0, done) == "Thank you for the fruit. I'll dry some for the winter.", "after the river line, fruit speaks");
        // Non-Living neighbours say nothing.
        Expect(NeighbourNotice.Line(0, WorldModel.New(PlayMode.PuzzleOnly)) == null, "no neighbours in puzzle-only");
    }
}
