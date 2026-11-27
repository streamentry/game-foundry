using System;
using System.Linq;
using Evergrove.Core;

/// <summary>
/// The jam kitchen: three fruit become one jar of jam, and jam travels in parcels. Fruit and jam
/// only ever move or are cooked down: nothing is made from nothing.
/// </summary>
internal static class KitchenTests
{
    private static int assertions;
    private const string Stamp = "2026-11-27T12:00:00Z";

    private static void Expect(bool ok, string message)
    {
        assertions++;
        if (!ok) throw new Exception("Kitchen test failed: " + message);
    }

    private static WorldModel Garden(int fruit, int jam = 0, int seeds = 0)
    {
        WorldSnapshot s = WorldModel.New(PlayMode.Living).Export();
        s.fruit = fruit;
        s.jam = jam;
        s.seeds = seeds;
        Expect(WorldModel.TryRestore(s, out WorldModel w), "garden restores (fruit " + fruit + ", jam " + jam + ")");
        return w;
    }

    private static string State(WorldModel w) { return GardenTransfer.SnapshotJson(w.Export()); }
    private static WorldSnapshot Tweak(WorldSnapshot s, Action<WorldSnapshot> change) { change(s); return s; }

    private static string Raw(int amount, int kind, string nonce, int blessing = 0, string letter = "")
    {
        var c = new ParcelContents { From = "Anh", Letter = letter, Gift = ParcelGift.Jam, Amount = amount, Kind = kind, Nonce = nonce, Blessing = blessing };
        c.Id = FamilyParcel.MakeId(c);
        return FamilyParcel.BuildText(c);
    }

    private static void Invalid(string what, string text)
    {
        WorldModel w = Garden(5, 2, 40);
        string before = State(w);
        ParcelOpenResult r = w.Open(text);
        Expect(!r.Ok && r.Error == ParcelError.Invalid && State(w) == before, "forged jam parcel refused (" + what + "): " + r.Error);
        Expect(!FamilyParcel.TryRead(text, out _, out ParcelError e) && e == ParcelError.Invalid, "TryRead agrees (" + what + ")");
    }

    public static int Run()
    {
        Constants();
        Making();
        Saving();
        FileCompatibility();
        ParcelShape();
        ParcelForgeries();
        Sending();
        Opening();
        return assertions;
    }

    private static void Constants()
    {
        Expect(WorldModel.JamPerJar == 3 && WorldModel.MaxJam == 10 && WorldModel.MaxJarsPerParcel == 3, "the kitchen's numbers");
        Expect((int)ParcelGift.Jam == 7, "jam is gift 7");
        Expect(WorldModel.New(PlayMode.Living).Jam == 0, "a new garden has no jam");
    }

    private static void Making()
    {
        WorldModel w = Garden(7, 0, 12);
        Expect(w.MakeJam() && w.Fruit == 4 && w.Jam == 1 && w.Seeds == 12, "three fruit become one jar, seeds untouched");
        Expect(w.MakeJam() && w.Fruit == 1 && w.Jam == 2, "again");
        string before = State(w);
        Expect(!w.MakeJam() && State(w) == before, "one fruit is not enough, nothing changes");
        WorldModel two = Garden(2);
        Expect(!two.MakeJam() && two.Fruit == 2 && two.Jam == 0, "two fruit is not enough");
        Expect(!Garden(0).MakeJam(), "no fruit, no jam");
        WorldModel three = Garden(3);
        Expect(three.MakeJam() && three.Jam == 1 && three.Fruit == 0, "exactly three is enough");

        WorldModel full = Garden(30, WorldModel.MaxJam);
        string fullBefore = State(full);
        Expect(!full.MakeJam() && State(full) == fullBefore, "a full pantry refuses and keeps the fruit");
        WorldModel nearly = Garden(30, WorldModel.MaxJam - 1);
        Expect(nearly.MakeJam() && nearly.Jam == WorldModel.MaxJam && nearly.Fruit == 27 && !nearly.MakeJam(), "the tenth jar fits, the eleventh does not");

        Expect(!WorldModel.New(PlayMode.PuzzleOnly).MakeJam(), "only a living garden cooks");

        // Fruit is conserved: every jar cost exactly JamPerJar fruit.
        WorldModel c = Garden(30);
        int start = c.Fruit;
        while (c.MakeJam()) { }
        Expect(c.Jam == 10 && c.Fruit == start - c.Jam * WorldModel.JamPerJar, "fruit spent equals jars times three");
    }

    private static void Saving()
    {
        WorldModel w = Garden(9, 4);
        Expect(WorldModel.TryRestore(w.Export(), out WorldModel back) && back.Jam == 4 && back.Fruit == 9 && State(back) == State(w), "jam survives a save");
        Expect(w.Export().jam == 4, "the snapshot carries jam");
        Expect(!WorldModel.TryRestore(Tweak(w.Export(), s => s.jam = -1), out _), "negative jam refused");
        Expect(!WorldModel.TryRestore(Tweak(w.Export(), s => s.jam = WorldModel.MaxJam + 1), out _), "more than the cap refused");
        Expect(WorldModel.TryRestore(Tweak(w.Export(), s => s.jam = WorldModel.MaxJam), out _), "exactly the cap accepted");
        Expect(!WorldModel.TryRestore(Tweak(WorldModel.New(PlayMode.PuzzleOnly).Export(), s => s.jam = 1), out _), "puzzle-only holding jam refused");
    }

    private static void FileCompatibility()
    {
        WorldModel w = Garden(9, 4);
        string file = GardenTransfer.Export(w, Stamp);
        Expect(file.Contains("\"jam\":4"), "the garden file carries jam");
        Expect(GardenTransfer.TryImport(file, out WorldModel imported, out GardenImportError error) && imported.Jam == 4 && imported.Fruit == 9,
            "garden round trip (" + error + ")");
        Expect(GardenTransfer.Export(imported, Stamp) == file, "byte-identical export");
        Expect(!GardenTransfer.TryImport(file.Replace("\"jam\":4", "\"jam\":5"), out _, out error) && error == GardenImportError.ChecksumMismatch, "edited jam fails the checksum");
        Expect(!GardenTransfer.TryImport(file.Replace("\"jam\":4", "\"jam\":\"4\""), out _, out _), "wrong type refused");
        Expect(!GardenTransfer.TryImport(file.Replace("}}", ",\"jamm\":1}}"), out _, out _), "an unknown key is refused");

        // A garden with no jam writes exactly what it wrote before: no jam key at all.
        string plain = GardenTransfer.Export(Garden(9), Stamp);
        Expect(!plain.Contains("\"jam\""), "no jam, no key: older files stay byte-identical");
        Expect(GardenTransfer.TryImport(plain, out WorldModel older, out error) && older.Jam == 0 && older.Fruit == 9, "a file without the key imports (" + error + ")");

        // Jam cannot ride under a checksum computed without it.
        string snapshot = plain.Substring(plain.IndexOf("\"snapshot\":", StringComparison.Ordinal) + 11);
        snapshot = snapshot.Substring(0, snapshot.Length - 1);
        string smuggled = snapshot.Substring(0, snapshot.Length - 1) + ",\"jam\":3}";
        Expect(!GardenTransfer.TryImport("{\"format\":\"mosselora-garden\",\"formatVersion\":1,\"exportedAtUtc\":\"" + Stamp + "\",\"checksumSha256\":\"" +
            GardenTransfer.Sha256Hex(snapshot) + "\",\"snapshot\":" + smuggled + "}", out _, out _), "jam cannot ride under the older checksum");
        // An explicit zero is not the canonical text either.
        string zero = snapshot.Substring(0, snapshot.Length - 1) + ",\"jam\":0}";
        Expect(!GardenTransfer.TryImport("{\"format\":\"mosselora-garden\",\"formatVersion\":1,\"exportedAtUtc\":\"" + Stamp + "\",\"checksumSha256\":\"" +
            GardenTransfer.Sha256Hex(zero) + "\",\"snapshot\":" + zero + "}", out _, out _), "a jam key reading 0 is not canonical");
    }

    private static void ParcelShape()
    {
        Expect(FamilyParcel.TryCreate("Nan", "Plum, made today", ParcelGift.Jam, 2, "j1", out string text, out ParcelContents made, out ParcelError error) &&
               error == ParcelError.None && made.Gift == ParcelGift.Jam && made.Amount == 2 && made.Kind == -1, "a jam parcel is made");
        Expect(text.Contains("\"gift\":\"jam\"") && text.Contains("\"amount\":2") && text.Contains("\"formatVersion\":1,"), "plain version 1 text");
        Expect(FamilyParcel.TryRead(text, out ParcelContents read, out _) && read.Gift == ParcelGift.Jam && read.Amount == 2 && read.Id == made.Id, "it reads back");
        Expect(FamilyParcel.TryCreate("Nan", "", ParcelGift.Jam, 1, "j2", out _, out _, out _) &&
               FamilyParcel.TryCreate("Nan", "", ParcelGift.Jam, 3, "j3", out _, out _, out _), "1 and 3 jars are allowed");
        Expect(!FamilyParcel.TryCreate("Nan", "", ParcelGift.Jam, 0, "j4", out _, out _, out _) &&
               !FamilyParcel.TryCreate("Nan", "", ParcelGift.Jam, 4, "j5", out _, out _, out _) &&
               !FamilyParcel.TryCreate("Nan", "", ParcelGift.Jam, -1, "j6", out _, out _, out _), "0, 4 and -1 jars are refused");
        Expect(FamilyParcel.TryCreate("Nan", "Sweet", ParcelGift.Jam, 2, "j7", out text, out made, out _, 5) && made.Blessing == 5 &&
               text.Contains("\"formatVersion\":2,") && text.Contains(",\"kind\":-1,\"blessing\":5}") &&
               FamilyParcel.TryRead(text, out read, out _) && read.Gift == ParcelGift.Jam && read.Blessing == 5, "a blessing is allowed on jam");
        Expect(!FamilyParcel.TryCreate("Nan", "", ParcelGift.Jam, 2, "j8", out _, out _, out _, 7), "blessing 7 refused on jam");
        Expect(!FamilyParcel.TryCreate("Nan", "", ParcelGift.Cutting, 2, "j9", out _, out _, out _, 1) &&
               !FamilyParcel.TryCreate("Nan", "", ParcelGift.Lantern, 0, "j10", out _, out _, out _, 1), "blessings still refused elsewhere");
    }

    private static void ParcelForgeries()
    {
        Invalid("jam amount 0", Raw(0, -1, "jf1"));
        Invalid("jam amount 4", Raw(4, -1, "jf2"));
        Invalid("jam amount negative", Raw(-3, -1, "jf3"));
        Invalid("jam amount huge", Raw(int.MaxValue, -1, "jf4"));
        Invalid("jam with a kind", Raw(1, 0, "jf5"));
        Invalid("jam blessing 7", Raw(1, -1, "jf6", 7));
        string text = Raw(1, -1, "jf7");
        Expect(!FamilyParcel.TryRead(text.Replace("\"amount\":1", "\"amount\":3"), out _, out ParcelError e) && e == ParcelError.ChecksumMismatch, "an edited amount fails the checksum");
    }

    private static void Sending()
    {
        WorldModel sender = Garden(5, 3, 40);
        string before = State(sender);
        ParcelSendResult r = sender.SendParcel("Anh", "For your toast", ParcelGift.Jam, 2, "js1");
        Expect(r.Ok && r.Cost == 0 && sender.Jam == 1 && sender.Fruit == 5 && sender.Seeds == 40, "sending removes the jars and nothing else");
        Expect(sender.HasSeenParcel(r.Id) && State(sender) != before, "the parcel is remembered");
        Expect(sender.SendParcel("Anh", "For your toast", ParcelGift.Jam, 2, "js1").Error == ParcelError.AlreadyUsed && sender.Jam == 1, "the same parcel is not sent twice");
        string afterOne = State(sender);
        ParcelSendResult short1 = sender.SendParcel("Anh", "", ParcelGift.Jam, 2, "js2");
        Expect(!short1.Ok && short1.Error == ParcelError.NotEnoughJam && sender.Jam == 1 && State(sender) == afterOne, "not enough jam: refused, nothing changes");
        Expect(sender.SendParcel("Anh", "", ParcelGift.Jam, 1, "js3").Ok && sender.Jam == 0, "the last jar goes");
        Expect(sender.SendParcel("Anh", "", ParcelGift.Jam, 1, "js4").Error == ParcelError.NotEnoughJam, "none left");
        Expect(sender.SendParcel("Anh", "", ParcelGift.Jam, 0, "js5").Error == ParcelError.Invalid &&
               Garden(0, 9).SendParcel("Anh", "", ParcelGift.Jam, 4, "js6").Error == ParcelError.Invalid, "a bad amount is invalid, not a shortage");
        Expect(!WorldModel.New(PlayMode.PuzzleOnly).SendParcel("Anh", "", ParcelGift.Jam, 1, "js7").Ok, "only a living garden sends");

        WorldModel blessed = Garden(0, 3);
        ParcelSendResult b = blessed.SendParcel("Anh", "", ParcelGift.Jam, 3, "js8", 6);
        Expect(b.Ok && blessed.Jam == 0 && b.Text.Contains("\"blessing\":6"), "a blessed jam parcel moves the jars");
    }

    private static void Opening()
    {
        WorldModel sender = Garden(0, 5, 10);
        ParcelSendResult sent = sender.SendParcel("Anh", "From my kitchen", ParcelGift.Jam, 3, "jo1", 4);
        WorldModel receiver = Garden(2, 1, 7);
        ParcelOpenResult o = receiver.Open(sent.Text);
        Expect(o.Ok && o.JamGained == 3 && receiver.Jam == 4 && receiver.Fruit == 2 && receiver.Seeds == 7, "opening adds the jars only");
        Expect(o.SeedsGained == 0 && o.FruitGained == 0 && o.Contents.Blessing == 4 && o.Contents.Gift == ParcelGift.Jam, "reports the blessing, no other gain");
        Expect(receiver.Letters.SequenceEqual(new[] { "From my kitchen" }), "the letter is kept");
        Expect(sender.Jam == 2 && receiver.Jam == 4, "jars moved: two stay, three arrived (nothing made)");
        string after = State(receiver);
        Expect(receiver.Open(sent.Text).Error == ParcelError.AlreadyOpened && State(receiver) == after, "opens once");
        Expect(sender.Open(sent.Text).Error == ParcelError.AlreadyOpened && sender.Jam == 2, "the sender cannot open its own parcel");
        Expect(!WorldModel.New(PlayMode.PuzzleOnly).Open(sent.Text).Ok, "only a living garden receives");

        // v1 #76: a parcel that would not fit in full is refused with LarderFull, changing nothing.
        WorldModel nearlyFull = Garden(0, WorldModel.MaxJam - 1);
        FamilyParcel.TryCreate("Nan", "", ParcelGift.Jam, 2, "jo2", out string big, out _, out _);
        string nearlyBefore = State(nearlyFull);
        ParcelOpenResult refused = nearlyFull.Open(big);
        Expect(!refused.Ok && refused.Error == ParcelError.LarderFull && refused.JamGained == 0 && State(nearlyFull) == nearlyBefore,
            "two jars do not fit in one: refused with LarderFull, nothing changes");
        Expect(nearlyFull.SendParcel("Anh", "", ParcelGift.Jam, 1, "jo4").Ok && nearlyFull.Jam == WorldModel.MaxJam - 2, "a jar sent makes room");
        ParcelOpenResult opened = nearlyFull.Open(big);
        Expect(opened.Ok && opened.JamGained == 2 && nearlyFull.Jam == WorldModel.MaxJam, "the same parcel opens once there is room");
        FamilyParcel.TryCreate("Nan", "", ParcelGift.Jam, 1, "jo3", out string more, out _, out _);
        ParcelOpenResult none = Garden(0, WorldModel.MaxJam).Open(more);
        Expect(!none.Ok && none.Error == ParcelError.LarderFull, "a full pantry refuses even one jar");
        FamilyParcel.TryCreate("Nan", "", ParcelGift.Seeds, 5, "jo5", out string seedsText, out _, out _);
        Expect(Garden(0).Open(seedsText).JamGained == 0, "other parcels report no jam");
    }
}
