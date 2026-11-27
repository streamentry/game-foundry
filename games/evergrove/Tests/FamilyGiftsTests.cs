using System;
using System.Linq;
using Evergrove.Core;

/// <summary>
/// Three more family gifts: the very board (Board), a festive envelope with a blessing, and a
/// lantern left for another. None creates seeds or fruit from nothing.
/// </summary>
internal static class FamilyGiftsTests
{
    private static int assertions;

    private const string PinnedText =
        "{\"format\":\"mosselora-parcel\",\"formatVersion\":1,\"checksumSha256\":\"1b8cfe03364c5f9fd500feb9fdf745b86950e2b4239aea0b7ce20dde86697fe1\"," +
        "\"parcel\":{\"id\":\"2ce2d19b10dbe1963ec25732\",\"nonce\":\"a1\",\"from\":\"Nan\",\"letter\":\"Thinking of you.\",\"gift\":\"seeds\",\"amount\":50,\"kind\":-1}}";

    private static void Expect(bool ok, string message)
    {
        assertions++;
        if (!ok) throw new Exception("Family gifts test failed: " + message);
    }

    private static WorldModel Rich(int seeds, int fruit)
    {
        WorldSnapshot s = WorldModel.New(PlayMode.Living).Export();
        s.seeds = seeds;
        s.fruit = fruit;
        Expect(WorldModel.TryRestore(s, out WorldModel w), "garden restores");
        return w;
    }

    private static string State(WorldModel w) { return GardenTransfer.SnapshotJson(w.Export()); }

    private static string Raw(ParcelGift gift, int amount, int kind, string nonce, int blessing = 0, string letter = "")
    {
        var c = new ParcelContents { From = "Anh", Letter = letter, Gift = gift, Amount = amount, Kind = kind, Nonce = nonce, Blessing = blessing };
        c.Id = FamilyParcel.MakeId(c);
        return FamilyParcel.BuildText(c);
    }

    private static void Invalid(string what, string text)
    {
        WorldModel w = Rich(40, 5);
        string before = State(w);
        ParcelOpenResult r = w.Open(text);
        Expect(!r.Ok && r.Error == ParcelError.Invalid && State(w) == before, "forged parcel refused (" + what + "): " + r.Error);
        Expect(!FamilyParcel.TryRead(text, out _, out ParcelError e) && e == ParcelError.Invalid, "TryRead agrees (" + what + ")");
    }

    public static int Run()
    {
        Pinned();
        Boards();
        BoardForgeries();
        Blessings();
        BlessingForgeries();
        Lanterns();
        LanternSaving();
        return assertions;
    }

    private static void Pinned()
    {
        Expect(FamilyParcel.CurrentFormatVersion == 2, "the format is version 2");
        FamilyParcel.TryCreate("Nan", "Thinking of you.", ParcelGift.Seeds, 50, "a1", out string text, out _, out _);
        Expect(text == PinnedText, "an existing parcel's text, checksum and id are byte-identical");
        Expect(!text.Contains("blessing"), "no blessing key when there is none");
        Expect(FamilyParcel.TryRead(PinnedText, out ParcelContents read, out _) && read.Blessing == 0 && read.Id == "2ce2d19b10dbe1963ec25732",
            "version 1 text is still read");
    }

    private static void Boards()
    {
        int kind = (int)PuzzleKind.Sudoku * TreePuzzles.Tiers + 2;
        Expect(FamilyParcel.TryCreate("Nan", "Try this one", ParcelGift.Board, kind, "b1", out string text, out ParcelContents made,
                   out ParcelError error, 0, 123456) && error == ParcelError.None, "a board parcel is made");
        Expect(made.Gift == ParcelGift.Board && made.Kind == kind && made.Amount == 123456, "kind encodes puzzle and tier; amount is the seed");
        Expect(text.Contains("\"gift\":\"board\"") && text.Contains("\"formatVersion\":1,"), "it is plain version 1 text with no new keys");
        Expect(FamilyParcel.TryRead(text, out ParcelContents read, out _) && read.Kind == kind && read.Amount == 123456 && read.Id == made.Id,
            "it reads back");
        Expect(FamilyParcel.TryCreate("Nan", "", ParcelGift.Board, 0, "b2", out _, out _, out _, 0, 0), "kind 0 tier 0 seed 0 is allowed");
        Expect(FamilyParcel.TryCreate("Nan", "", ParcelGift.Board, 8 * TreePuzzles.Tiers - 1, "b3", out _, out _, out _, 0, int.MaxValue),
            "the last kind and tier with the largest seed is allowed");

        WorldModel sender = Rich(100, 7);
        string before = State(sender);
        ParcelSendResult sent = sender.SendBoard("Anh", "I just solved this", (int)PuzzleKind.Nonogram, 1, 777, "bs1");
        Expect(sent.Ok && sent.Cost == 0 && sender.Seeds == 100 && sender.Fruit == 7, "sending a board costs nothing");
        Expect(sender.HasSeenParcel(sent.Id) && State(sender) != before, "only the parcel memory changed");
        Expect(sender.SendParcel("Anh", "I just solved this", ParcelGift.Board, (int)PuzzleKind.Nonogram * TreePuzzles.Tiers + 1, "bs1", 0, 777).Error
               == ParcelError.AlreadyUsed, "the same board and nonce is already used");
        Expect(!sender.SendBoard("Anh", "", 8, 0, 1, "bs2").Ok && !sender.SendBoard("Anh", "", 0, TreePuzzles.Tiers, 1, "bs3").Ok &&
               !sender.SendBoard("Anh", "", -1, 0, 1, "bs4").Ok && !sender.SendBoard("Anh", "", 0, -1, 1, "bs5").Ok &&
               !sender.SendBoard("Anh", "", 0, 0, -1, "bs6").Ok, "a bad kind, tier or seed is not sent");
        Expect(!WorldModel.New(PlayMode.PuzzleOnly).SendBoard("Anh", "", 0, 0, 1, "x").Ok, "only a living garden sends");

        WorldModel receiver = Rich(10, 2);
        string rBefore = GardenTransfer.SnapshotJson(Tweak(receiver.Export(), s => s.parcelIds = Array.Empty<string>()));
        ParcelOpenResult opened = receiver.Open(sent.Text);
        Expect(opened.Ok && opened.BoardKind == (int)PuzzleKind.Nonogram && opened.BoardTier == 1 && opened.BoardSeed == 777,
            "opening reports the kind, tier and seed");
        Expect(opened.SeedsGained == 0 && opened.FruitGained == 0 && opened.GiftIndex == -1 && opened.HelpedItem == -1,
            "opening gains nothing");
        Expect(receiver.Seeds == 10 && receiver.Fruit == 2 && receiver.Gifts.Length == 0, "seeds, fruit and basket unchanged");
        Expect(GardenTransfer.SnapshotJson(Tweak(receiver.Export(), s => s.parcelIds = Array.Empty<string>())) ==
               rBefore.Replace("\"letters\":[]", "\"letters\":[\"I just solved this\"]").Replace("\"letterFrom\":[]", "\"letterFrom\":[\"Anh\"]"),
            "only the letter and the parcel memory changed");
        string afterOpen = State(receiver);
        ParcelOpenResult twice = receiver.Open(sent.Text);
        Expect(!twice.Ok && twice.Error == ParcelError.AlreadyOpened && twice.BoardKind == -1 && State(receiver) == afterOpen,
            "the same board parcel opens once");
        Expect(sender.Open(sent.Text).Error == ParcelError.AlreadyOpened, "a garden cannot open the board it sent");
        ParcelOpenResult seeds = Rich(0, 0).Open(Raw(ParcelGift.Seeds, 5, -1, "o1"));
        Expect(seeds.BoardKind == -1 && seeds.BoardTier == -1 && seeds.BoardSeed == -1, "other parcels report -1 for the board");
    }

    private static WorldSnapshot Tweak(WorldSnapshot s, Action<WorldSnapshot> change) { change(s); return s; }

    private static void BoardForgeries()
    {
        int tiers = TreePuzzles.Tiers;
        Invalid("board kind too large", Raw(ParcelGift.Board, 5, 8 * tiers, "bf1"));
        Invalid("board kind negative", Raw(ParcelGift.Board, 5, -1, "bf2"));
        Invalid("board with a negative seed", Raw(ParcelGift.Board, -1, 3, "bf3"));
        Invalid("board with a hugely negative seed", Raw(ParcelGift.Board, int.MinValue, 3, "bf4"));
        Invalid("board with a blessing", Raw(ParcelGift.Board, 5, 3, "bf5", 2));
        Expect(!FamilyParcel.TryCreate("A", "", ParcelGift.Board, 8 * tiers, "z", out _, out _, out _, 0, 5), "TryCreate refuses a bad kind");
        Expect(!FamilyParcel.TryCreate("A", "", ParcelGift.Board, 3, "z", out _, out _, out _, 0, -5), "TryCreate refuses a negative seed");
    }

    private static void Blessings()
    {
        string[] list = FamilyParcel.Blessings;
        Expect(list.Length == 6 && list.All(b => b.Length > 0 && b.Length <= 24 && !b.Contains("!")), "six short blessings, none with an exclamation mark");
        Expect(list.Distinct().Count() == 6 && list[0] == "Good health" && list[5] == "With love", "distinct and in the agreed order");

        Expect(FamilyParcel.TryCreate("Nan", "Lucky money", ParcelGift.Seeds, 20, "e1", out string text, out ParcelContents made,
                   out _, 4) && made.Blessing == 4, "a seeds parcel takes a blessing");
        Expect(text.Contains("\"formatVersion\":2,") && text.Contains(",\"kind\":-1,\"blessing\":4}"), "with a blessing the format is version 2");
        Expect(FamilyParcel.TryRead(text, out ParcelContents read, out _) && read.Blessing == 4 && read.Amount == 20 && read.Id == made.Id,
            "it reads back");
        FamilyParcel.TryCreate("Nan", "Lucky money", ParcelGift.Seeds, 20, "e1", out _, out ParcelContents plain, out _);
        Expect(plain.Id != made.Id, "a blessing changes the id");
        Expect(FamilyParcel.TryCreate("Nan", "", ParcelGift.Fruit, 3, "e2", out text, out made, out _, 6) && made.Blessing == 6 &&
               FamilyParcel.TryRead(text, out read, out _) && read.Gift == ParcelGift.Fruit && read.Blessing == 6, "fruit takes a blessing too");

        WorldModel sender = Rich(100, 5);
        ParcelSendResult s = sender.SendParcel("Anh", "", ParcelGift.Seeds, 30, "e3", 2);
        Expect(s.Ok && s.Cost == 30 && sender.Seeds == 70 && s.Text.Contains("\"blessing\":2"), "SendParcel passes the blessing through; seeds move as ever");
        ParcelSendResult f = sender.SendParcel("Anh", "", ParcelGift.Fruit, 2, "e4", 1);
        Expect(f.Ok && sender.Fruit == 3, "a blessed fruit parcel costs the fruit only");
        WorldModel receiver = Rich(0, 0);
        ParcelOpenResult o = receiver.Open(s.Text);
        Expect(o.Ok && o.SeedsGained == 30 && receiver.Seeds == 30 && o.Contents.Blessing == 2, "opening shows the blessing and moves the seeds");
        Expect(receiver.Open(f.Text).FruitGained == 2 && receiver.Fruit == 2, "the fruit arrives");
        Expect(!sender.SendParcel("Anh", "", ParcelGift.Seeds, 1, "e5", 0).Text?.Contains("blessing") ?? true, "blessing 0 sends no key");
        Expect(!sender.SendParcel("Anh", "", ParcelGift.Cutting, 0, "e6", 3).Ok, "a cutting cannot carry a blessing");
        Expect(!sender.SendParcel("Anh", "", ParcelGift.Seeds, 1, "e7", 7).Ok && !sender.SendParcel("Anh", "", ParcelGift.Seeds, 1, "e8", -1).Ok,
            "blessing 7 or -1 is refused when sending");
    }

    private static string Version2WithKey(string text, string keyText)
    {
        string t = text.Replace("\"formatVersion\":1", "\"formatVersion\":2");
        Expect(t.EndsWith("\"kind\":-1}}", StringComparison.Ordinal), "forge helper: text shape");
        return t.Substring(0, t.Length - 2) + "," + keyText + "}}";
    }

    private static void BlessingForgeries()
    {
        Invalid("blessing on a cutting", Raw(ParcelGift.Cutting, 0, 3, "bl1", 2));
        Invalid("blessing on help", Raw(ParcelGift.Help, 0, 0, "bl2", 2));
        Invalid("blessing on a letter", Raw(ParcelGift.None, 0, -1, "bl3", 2, "Hello"));
        Invalid("blessing 7", Raw(ParcelGift.Seeds, 5, -1, "bl4", 7));
        Invalid("blessing negative", Raw(ParcelGift.Seeds, 5, -1, "bl5", -2));
        // Blessing 0 written as a key: the no-key checksum still matches, so the rules must refuse it.
        Invalid("blessing 0 in the text", Version2WithKey(Raw(ParcelGift.Seeds, 5, -1, "bl6"), "\"blessing\":0"));
        // A blessing key inside a version 1 parcel.
        Expect(FamilyParcel.TryCreate("Nan", "", ParcelGift.Seeds, 5, "bl7", out string blessed, out _, out _, 3), "setup");
        string v1 = blessed.Replace("\"formatVersion\":2", "\"formatVersion\":1");
        Expect(!FamilyParcel.TryRead(v1, out _, out _), "a version 1 parcel with a blessing key is refused");
        string v2NoKey = Raw(ParcelGift.Seeds, 5, -1, "bl8").Replace("\"formatVersion\":1", "\"formatVersion\":2");
        Expect(!FamilyParcel.TryRead(v2NoKey, out _, out _), "version 2 without a blessing is refused");
        Expect(FamilyParcel.TryRead(blessed.Replace("\"formatVersion\":2", "\"formatVersion\":3"), out _, out ParcelError e) == false &&
               e == ParcelError.UnsupportedFutureVersion, "version 3 is a future version");
        Expect(!FamilyParcel.TryRead(blessed.Replace("\"blessing\":3", "\"blessing\":\"3\""), out _, out _), "a text blessing is refused");
        Expect(!FamilyParcel.TryRead(blessed.Replace("\"blessing\":3", "\"blessing\":2"), out _, out e) && e == ParcelError.ChecksumMismatch,
            "an edited blessing fails the checksum");
        Expect(!FamilyParcel.TryCreate("A", "", ParcelGift.Seeds, 5, "z", out _, out _, out _, 7), "TryCreate refuses 7");
        Expect(!FamilyParcel.TryCreate("A", "", ParcelGift.Cutting, 3, "z", out _, out _, out _, 1), "TryCreate refuses a blessed cutting");
    }

    private static void Lanterns()
    {
        Expect(FamilyParcel.TryCreate("Nan", "", ParcelGift.Lantern, 0, "l1", out string text, out ParcelContents made, out _) &&
               made.Gift == ParcelGift.Lantern && made.Amount == 0 && made.Kind == -1, "a lantern parcel is made with no amount or kind");
        Expect(text.Contains("\"gift\":\"lantern\"") && FamilyParcel.TryRead(text, out ParcelContents read, out _) && read.Gift == ParcelGift.Lantern,
            "it reads back");
        Invalid("lantern with an amount", Raw(ParcelGift.Lantern, 5, -1, "lf1"));
        Invalid("lantern with a kind", Raw(ParcelGift.Lantern, 0, 2, "lf2"));
        Invalid("lantern with a blessing", Raw(ParcelGift.Lantern, 0, -1, "lf3", 1));

        WorldModel sender = Rich(50, 4);
        ParcelSendResult sent = sender.SendParcel("Anh", "", ParcelGift.Lantern, 0, "ls1");
        Expect(sent.Ok && sent.Cost == 0 && sender.Seeds == 50 && sender.Fruit == 4, "sending a lantern is free");
        Expect(sender.Lanterns.Length == 0, "the sender's own garden lights nothing");

        WorldModel receiver = Rich(5, 1);
        Expect(receiver.Lanterns.Length == 0, "none at first");
        ParcelOpenResult o = receiver.Open(sent.Text);
        Expect(o.Ok && o.SeedsGained == 0 && receiver.Seeds == 5 && receiver.Fruit == 1, "opening a lantern gains nothing");
        Expect(receiver.Lanterns.SequenceEqual(new[] { "Anh" }), "the sender's name is recorded");
        string[] copy = receiver.Lanterns;
        copy[0] = "x";
        Expect(receiver.Lanterns[0] == "Anh", "Lanterns is a copy");
        string after = State(receiver);
        Expect(receiver.Open(sent.Text).Error == ParcelError.AlreadyOpened && State(receiver) == after, "the same lantern opens once");
        Expect(!WorldModel.New(PlayMode.PuzzleOnly).Open(sent.Text).Ok, "only a living garden receives");

        for (int i = 0; i < 14; i++)
        {
            string from = "Friend" + i;
            FamilyParcel.TryCreate(from, "", ParcelGift.Lantern, 0, "ln" + i, out string t, out _, out _);
            Expect(receiver.Open(t).Ok, "lantern " + i + " opens");
        }
        string[] names = receiver.Lanterns;
        Expect(names.Length == 12 && names[11] == "Friend13" && names[0] == "Friend2", "capped at 12, newest last, oldest dropped");
    }

    private static void LanternSaving()
    {
        WorldModel w = Rich(5, 1);
        foreach (string n in new[] { "Anh", "Nan" })
        {
            FamilyParcel.TryCreate(n, "", ParcelGift.Lantern, 0, "ls-" + n, out string t, out _, out _);
            w.Open(t);
        }
        Expect(WorldModel.TryRestore(w.Export(), out WorldModel back) && back.Lanterns.SequenceEqual(new[] { "Anh", "Nan" }), "lanterns survive a save");
        Expect(State(back) == State(w), "identical state");
        string file = GardenTransfer.Export(w, "2026-11-27T12:00:00Z");
        Expect(file.Contains("\"lanterns\":[\"Anh\",\"Nan\"]"), "the garden file carries the key");
        Expect(GardenTransfer.TryImport(file, out WorldModel imported, out GardenImportError error) &&
               imported.Lanterns.SequenceEqual(new[] { "Anh", "Nan" }), "garden round trip (" + error + ")");
        Expect(GardenTransfer.Export(imported, "2026-11-27T12:00:00Z") == file, "byte-identical export");
        Expect(!GardenTransfer.TryImport(file.Replace("[\"Anh\",\"Nan\"]", "[\"Anh\",\"Nan\",\"Joe\"]"), out _, out error) &&
               error == GardenImportError.ChecksumMismatch, "edited lanterns fail the checksum");
        Expect(!GardenTransfer.TryImport(file.Replace("\"lanterns\":[\"Anh\",\"Nan\"]", "\"lanterns\":\"Anh\""), out _, out _), "wrong type refused");
        Expect(!GardenTransfer.TryImport(file.Replace("}}", ",\"lanternz\":[]}}"), out _, out _), "an unknown key is refused");

        // An older file: same snapshot without the lanterns key, checksummed as before.
        WorldModel older = Rich(5, 1);
        string full = GardenTransfer.Export(older, "2026-11-27T12:00:00Z");
        int at = full.IndexOf(",\"lanterns\":[]", StringComparison.Ordinal);
        Expect(at > 0, "an empty garden still writes the lanterns key");
        string snapshot = full.Substring(full.IndexOf("\"snapshot\":", StringComparison.Ordinal) + 11);
        snapshot = snapshot.Substring(0, snapshot.Length - 1).Replace(",\"lanterns\":[]", "");
        string envelope = "{\"format\":\"mosselora-garden\",\"formatVersion\":1,\"exportedAtUtc\":\"2026-11-27T12:00:00Z\",\"checksumSha256\":\"" +
            GardenTransfer.Sha256Hex(snapshot) + "\",\"snapshot\":" + snapshot + "}";
        Expect(GardenTransfer.TryImport(envelope, out WorldModel fromOld, out error) && fromOld.Lanterns.Length == 0 && fromOld.Seeds == 5,
            "a file without the key still imports (" + error + ")");
        // And one that predates the together key as well.
        string noTogether = snapshot.Replace(",\"together\":[]", "");
        Expect(noTogether != snapshot && GardenTransfer.TryImport(
            "{\"format\":\"mosselora-garden\",\"formatVersion\":1,\"exportedAtUtc\":\"2026-11-27T12:00:00Z\",\"checksumSha256\":\"" +
            GardenTransfer.Sha256Hex(noTogether) + "\",\"snapshot\":" + noTogether + "}", out _, out error), "an even older file imports (" + error + ")");
        // Lanterns cannot ride under the older checksum.
        string smuggled = snapshot.Substring(0, snapshot.Length - 1) + ",\"lanterns\":[\"Anh\"]}";
        Expect(!GardenTransfer.TryImport("{\"format\":\"mosselora-garden\",\"formatVersion\":1,\"exportedAtUtc\":\"2026-11-27T12:00:00Z\",\"checksumSha256\":\"" +
            GardenTransfer.Sha256Hex(snapshot) + "\",\"snapshot\":" + smuggled + "}", out _, out _), "lanterns cannot ride under the older checksum");

        WorldSnapshot good = w.Export();
        Expect(!WorldModel.TryRestore(Tweak(w.Export(), s => s.lanterns = new[] { "" }), out _), "an empty name refused");
        Expect(!WorldModel.TryRestore(Tweak(w.Export(), s => s.lanterns = new[] { " Anh" }), out _), "an untrimmed name refused");
        Expect(!WorldModel.TryRestore(Tweak(w.Export(), s => s.lanterns = new[] { new string('a', 25) }), out _), "an overlong name refused");
        Expect(!WorldModel.TryRestore(Tweak(w.Export(), s => s.lanterns = new[] { "A\u0001" }), out _), "a control character refused");
        Expect(!WorldModel.TryRestore(Tweak(w.Export(), s => s.lanterns = new string[] { null }), out _), "a null name refused");
        Expect(!WorldModel.TryRestore(Tweak(w.Export(), s => s.lanterns = Enumerable.Repeat("Anh", 13).ToArray()), out _), "more than 12 refused");
        Expect(WorldModel.TryRestore(Tweak(w.Export(), s => s.lanterns = Enumerable.Repeat("Anh", 12).ToArray()), out _), "exactly 12 accepted");
        Expect(WorldModel.TryRestore(Tweak(w.Export(), s => s.lanterns = null), out WorldModel bare) && bare.Lanterns.Length == 0, "null restores empty");
        Expect(!WorldModel.TryRestore(Tweak(WorldModel.New(PlayMode.PuzzleOnly).Export(), s => s.lanterns = new[] { "Anh" }), out _),
            "puzzle-only holding any refused");
        Expect(good.lanterns.Length == 2, "setup kept");
    }
}
