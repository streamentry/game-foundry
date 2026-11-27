using System;
using System.IO;
using System.Linq;
using Evergrove.Core;

/// <summary>
/// Picture stamps on letters: any parcel may carry one of twelve garden pictures. A stamp moves
/// nothing and costs nothing; it travels with the letter and is kept beside it.
/// </summary>
internal static class LetterStampTests
{
    private static int assertions;
    private const string Stamp = "2026-11-27T12:00:00Z";

    private const string PinnedText =
        "{\"format\":\"mosselora-parcel\",\"formatVersion\":1,\"checksumSha256\":\"1b8cfe03364c5f9fd500feb9fdf745b86950e2b4239aea0b7ce20dde86697fe1\"," +
        "\"parcel\":{\"id\":\"2ce2d19b10dbe1963ec25732\",\"nonce\":\"a1\",\"from\":\"Nan\",\"letter\":\"Thinking of you.\",\"gift\":\"seeds\",\"amount\":50,\"kind\":-1}}";

    private static void Expect(bool ok, string message)
    {
        assertions++;
        if (!ok) throw new Exception("Letter stamp test failed: " + message);
    }

    private static WorldModel Garden(int seeds = 0, int fruit = 0, int jam = 0)
    {
        WorldSnapshot s = WorldModel.New(PlayMode.Living).Export();
        s.seeds = seeds;
        s.fruit = fruit;
        s.jam = jam;
        Expect(WorldModel.TryRestore(s, out WorldModel w), "garden restores");
        return w;
    }

    private static string State(WorldModel w) { return GardenTransfer.SnapshotJson(w.Export()); }
    private static WorldSnapshot Tweak(WorldSnapshot s, Action<WorldSnapshot> change) { change(s); return s; }

    private static string Raw(ParcelGift gift, int amount, int kind, string nonce, int blessing, int stamp, string letter)
    {
        var c = new ParcelContents { From = "Anh", Letter = letter, Gift = gift, Amount = amount, Kind = kind, Nonce = nonce, Blessing = blessing, Stamp = stamp };
        c.Id = FamilyParcel.MakeId(c);
        return FamilyParcel.BuildText(c);
    }

    private static void Invalid(string what, string text)
    {
        WorldModel w = Garden(40, 5, 2);
        string before = State(w);
        ParcelOpenResult r = w.Open(text);
        Expect(!r.Ok && r.Error == ParcelError.Invalid && State(w) == before, "forged parcel refused (" + what + "): " + r.Error);
        Expect(!FamilyParcel.TryRead(text, out _, out ParcelError e) && e == ParcelError.Invalid, "TryRead agrees (" + what + ")");
    }

    public static int Run()
    {
        TheTwelve();
        Pinned();
        Shape();
        Forgeries();
        Sending();
        Keeping();
        Saving();
        FileCompatibility();
        return assertions;
    }

    private static string ArtFolder()
    {
        string dir = AppContext.BaseDirectory;
        while (dir != null && !Directory.Exists(Path.Combine(dir, "Assets", "Resources", "Evergrove", "Art")))
            dir = Path.GetDirectoryName(dir);
        return dir == null ? null : Path.Combine(dir, "Assets", "Resources", "Evergrove", "Art");
    }

    private static void TheTwelve()
    {
        Expect(FamilyParcel.StampCount == 12 && FamilyParcel.Stamps.Length == 12, "twelve stamps");
        Expect(FamilyParcel.Stamps.Distinct().Count() == 12, "all distinct");
        Expect(FamilyParcel.Stamps[0] == "flower-00" && FamilyParcel.Stamps[7] == "food-jam" && FamilyParcel.Stamps[11] == "build-96", "in the agreed order");
        string art = ArtFolder();
        Expect(art != null, "the art folder is found from the test run");
        foreach (string key in FamilyParcel.Stamps)
            Expect(File.Exists(Path.Combine(art, key + ".png")), "stamp art exists: " + key);
        Expect(FamilyParcel.CurrentFormatVersion == 2, "no format bump needed");
    }

    private static void Pinned()
    {
        FamilyParcel.TryCreate("Nan", "Thinking of you.", ParcelGift.Seeds, 50, "a1", out string text, out ParcelContents made, out _);
        Expect(text == PinnedText && made.Stamp == 0, "an existing parcel's text, checksum and id are byte-identical");
        Expect(!text.Contains("stamp"), "no stamp key when there is none");
        Expect(FamilyParcel.TryRead(PinnedText, out ParcelContents read, out _) && read.Stamp == 0 && read.Id == "2ce2d19b10dbe1963ec25732", "version 1 text still reads");
        FamilyParcel.TryCreate("Nan", "Lucky", ParcelGift.Seeds, 20, "e1", out string blessed, out _, out _, 4);
        Expect(blessed.Contains(",\"kind\":-1,\"blessing\":4}") && !blessed.Contains("stamp"), "a blessed parcel without a stamp is unchanged");
        FamilyParcel.TryCreate("Nan", "Hello", ParcelGift.None, 0, "n1", out string letter, out _, out _);
        Expect(letter.Contains("\"formatVersion\":1,") && !letter.Contains("stamp"), "a plain letter is unchanged");
    }

    private static void Shape()
    {
        Expect(FamilyParcel.TryCreate("Nan", "With a pear", ParcelGift.None, 0, "s1", out string text, out ParcelContents made, out ParcelError error, 0, 0, 6) &&
               error == ParcelError.None && made.Stamp == 6, "a letter takes a stamp");
        Expect(text.Contains("\"formatVersion\":2,") && text.Contains(",\"kind\":-1,\"stamp\":6}") && !text.Contains("blessing"), "a stamp alone makes version 2");
        Expect(FamilyParcel.TryRead(text, out ParcelContents read, out _) && read.Stamp == 6 && read.Blessing == 0 && read.Id == made.Id && read.Letter == "With a pear", "it reads back");
        FamilyParcel.TryCreate("Nan", "With a pear", ParcelGift.None, 0, "s1", out _, out ParcelContents plain, out _);
        Expect(plain.Id != made.Id, "a stamp changes the id");

        Expect(FamilyParcel.TryCreate("Nan", "", ParcelGift.Seeds, 20, "s2", out text, out made, out _, 3, 0, 12) &&
               text.Contains("\"formatVersion\":2,") && text.Contains(",\"kind\":-1,\"blessing\":3,\"stamp\":12}") &&
               FamilyParcel.TryRead(text, out read, out _) && read.Blessing == 3 && read.Stamp == 12, "blessing and stamp together, blessing first");

        ParcelGift[] gifts = { ParcelGift.Seeds, ParcelGift.Fruit, ParcelGift.Jam, ParcelGift.Lantern };
        for (int i = 0; i < gifts.Length; i++)
        {
            int amount = gifts[i] == ParcelGift.Lantern ? 0 : 1;
            Expect(FamilyParcel.TryCreate("Nan", "", gifts[i], amount, "sg" + i, out text, out _, out _, 0, 0, 1) &&
                   FamilyParcel.TryRead(text, out read, out _) && read.Gift == gifts[i] && read.Stamp == 1, "a stamp rides on " + gifts[i]);
        }
        Expect(FamilyParcel.TryCreate("Nan", "", ParcelGift.Cutting, 0, "sc", out text, out _, out _, 0, 0, 2) &&
               FamilyParcel.TryRead(text, out read, out _) && read.Stamp == 2, "and on a cutting");
        Expect(FamilyParcel.TryCreate("Nan", "", ParcelGift.Help, 0, "sh", out _, out _, out _, 0, 0, 2) &&
               FamilyParcel.TryCreate("Nan", "", ParcelGift.Board, 3, "sb", out _, out _, out _, 0, 99, 2), "and on help and a board");
        Expect(FamilyParcel.TryCreate("A", "x", ParcelGift.None, 0, "z1", out _, out _, out _, 0, 0, 1) &&
               FamilyParcel.TryCreate("A", "x", ParcelGift.None, 0, "z2", out _, out _, out _, 0, 0, 12), "stamps 1 and 12 are allowed");
        Expect(!FamilyParcel.TryCreate("A", "x", ParcelGift.None, 0, "z3", out _, out _, out _, 0, 0, 13) &&
               !FamilyParcel.TryCreate("A", "x", ParcelGift.None, 0, "z4", out _, out _, out _, 0, 0, -1), "13 and -1 are refused");
        Expect(!FamilyParcel.TryCreate("A", "", ParcelGift.None, 0, "z5", out _, out _, out _, 0, 0, 3), "a stamp does not excuse an empty letter");
    }

    private static string WithKey(string text, string keyText)
    {
        string t = text.Replace("\"formatVersion\":1", "\"formatVersion\":2");
        Expect(t.EndsWith("}}", StringComparison.Ordinal), "forge helper: text shape");
        return t.Substring(0, t.Length - 2) + "," + keyText + "}}";
    }

    private static void Forgeries()
    {
        Invalid("stamp 13", Raw(ParcelGift.None, 0, -1, "f1", 0, 13, "Hi"));
        Invalid("stamp negative", Raw(ParcelGift.None, 0, -1, "f2", 0, -4, "Hi"));
        Invalid("stamp huge", Raw(ParcelGift.None, 0, -1, "f3", 0, int.MaxValue, "Hi"));
        // A key reading 0 matches the no-key checksum, so the rules must refuse it.
        Invalid("stamp 0 in the text", WithKey(Raw(ParcelGift.None, 0, -1, "f4", 0, 0, "Hi"), "\"stamp\":0"));
        Invalid("blessing present and stamp 0", WithKey(Raw(ParcelGift.Seeds, 5, -1, "f5", 3, 0, ""), "\"stamp\":0"));

        Expect(FamilyParcel.TryCreate("Nan", "Hi", ParcelGift.None, 0, "f6", out string stamped, out _, out _, 0, 0, 5), "setup");
        Expect(!FamilyParcel.TryRead(stamped.Replace("\"formatVersion\":2", "\"formatVersion\":1"), out _, out _), "a version 1 parcel with a stamp key is refused");
        string v2NoKey = Raw(ParcelGift.None, 0, -1, "f7", 0, 0, "Hi").Replace("\"formatVersion\":1", "\"formatVersion\":2");
        Expect(!FamilyParcel.TryRead(v2NoKey, out _, out _), "version 2 with neither key is refused");
        Expect(!FamilyParcel.TryRead(stamped.Replace("\"formatVersion\":2", "\"formatVersion\":3"), out _, out ParcelError e) && e == ParcelError.UnsupportedFutureVersion, "version 3 is a future version");
        Expect(!FamilyParcel.TryRead(stamped.Replace("\"stamp\":5", "\"stamp\":\"5\""), out _, out _), "a text stamp is refused");
        Expect(!FamilyParcel.TryRead(stamped.Replace("\"stamp\":5", "\"stamp\":6"), out _, out e) && e == ParcelError.ChecksumMismatch, "an edited stamp fails the checksum");
        Expect(!FamilyParcel.TryRead(stamped.Replace(",\"stamp\":5", ",\"stamp\":5,\"extra\":1"), out _, out _), "an unknown key is refused");
        Expect(!FamilyParcel.TryRead(stamped.Replace(",\"stamp\":5", ",\"stamp\":5,\"stamp\":5"), out _, out _), "a duplicated key is refused");
    }

    private static void Sending()
    {
        WorldModel sender = Garden(100, 6, 3);
        ParcelSendResult s = sender.SendParcel("Anh", "A pear for you", ParcelGift.None, 0, "ss1", 0, 0, 6);
        Expect(s.Ok && s.Cost == 0 && s.Text.Contains("\"stamp\":6") && sender.Seeds == 100 && sender.Fruit == 6 && sender.Jam == 3, "a stamped letter costs nothing");
        ParcelSendResult seeds = sender.SendParcel("Anh", "", ParcelGift.Seeds, 30, "ss2", 2, 0, 9);
        Expect(seeds.Ok && sender.Seeds == 70 && seeds.Text.Contains("\"blessing\":2,\"stamp\":9}"), "seeds move as ever; stamp and blessing pass through");
        ParcelSendResult jam = sender.SendParcel("Anh", "", ParcelGift.Jam, 2, "ss3", 0, 0, 8);
        Expect(jam.Ok && sender.Jam == 1 && jam.Text.Contains("\"stamp\":8"), "a stamp on jam moves the jars only");
        Expect(!sender.SendParcel("Anh", "x", ParcelGift.None, 0, "ss4", 0, 0, 13).Ok && !sender.SendParcel("Anh", "x", ParcelGift.None, 0, "ss5", 0, 0, -1).Ok, "a bad stamp is not sent");
        Expect(sender.SendParcel("Anh", "A pear for you", ParcelGift.None, 0, "ss1", 0, 0, 6).Error == ParcelError.AlreadyUsed, "the same stamped letter is used once");
        Expect(sender.SendParcel("Anh", "A pear for you", ParcelGift.None, 0, "ss1").Ok, "the same words without the stamp are a different parcel");
        Expect(!sender.SendParcel("Anh", "", ParcelGift.Seeds, 1, "ss6", 0, 0, 0).Text.Contains("stamp"), "stamp 0 sends no key");
        Expect(!WorldModel.New(PlayMode.PuzzleOnly).SendParcel("Anh", "x", ParcelGift.None, 0, "ss7", 0, 0, 1).Ok, "only a living garden sends");
        // SendBoard has no stamp parameter and is unchanged.
        Expect(sender.SendBoard("Anh", "", 0, 0, 5, "ss8").Ok, "SendBoard still works");
    }

    private static string Parcel(string from, string letter, ParcelGift gift, int amount, string nonce, int stamp)
    {
        FamilyParcel.TryCreate(from, letter, gift, amount, nonce, out string text, out _, out _, 0, 0, stamp);
        return text;
    }

    private static void Keeping()
    {
        WorldModel w = Garden();
        Expect(w.LetterStamps.Length == 0, "no letters, no stamps");
        Expect(w.Open(Parcel("Nan", "With a pear", ParcelGift.None, 0, "k1", 6)).Ok, "a stamped letter opens");
        Expect(w.Letters.SequenceEqual(new[] { "With a pear" }) && w.LetterStamps.SequenceEqual(new[] { 6 }), "the stamp is kept with the letter");
        Expect(w.Open(Parcel("Ann", "Plain words", ParcelGift.None, 0, "k2", 0)).Ok, "a plain letter opens");
        Expect(w.LetterStamps.SequenceEqual(new[] { 6, 0 }) && w.LetterStamps.Length == w.Letters.Length, "stamps run parallel to letters");
        Expect(w.KeepNote("Mira", "Seen your tree"), "a neighbour's note is kept");
        Expect(w.LetterStamps.SequenceEqual(new[] { 6, 0, 0 }), "KeepNote keeps stamp 0");
        Expect(w.Open(Parcel("Bo", "Seeds and a lantern picture", ParcelGift.Seeds, 5, "k3", 12)).Ok && w.LetterStamps.SequenceEqual(new[] { 6, 0, 0, 12 }) && w.Seeds == 5, "a stamped seeds parcel keeps its stamp, seeds move as ever");

        string before = State(w);
        ParcelOpenResult noLetter = w.Open(Parcel("Cy", "", ParcelGift.Seeds, 4, "k4", 3));
        Expect(noLetter.Ok && noLetter.Contents.Stamp == 3 && w.Letters.Length == 4 && w.LetterStamps.Length == 4 && w.Seeds == 9, "with no letter there is nothing to keep it beside; the open result still shows it");
        Expect(State(w) != before, "(the parcel memory changed)");
        string after = State(w);
        Expect(w.Open(Parcel("Cy", "", ParcelGift.Seeds, 4, "k4", 3)).Error == ParcelError.AlreadyOpened && State(w) == after, "opens once");
        int[] copy = w.LetterStamps;
        copy[0] = 9;
        Expect(w.LetterStamps[0] == 6, "LetterStamps is a copy");

        // A garden whose letters never carried a stamp keeps no stamp list at all.
        WorldModel plain = Garden();
        plain.Open(Parcel("Nan", "Hello", ParcelGift.None, 0, "k5", 0));
        plain.KeepNote("Mira", "Hi");
        Expect(plain.Letters.Length == 2 && plain.LetterStamps.SequenceEqual(new[] { 0, 0 }) && plain.Export().letterStamps.Length == 0, "unstamped letters store nothing extra");

        // The cap: 20 letters, oldest leave first and take their stamps with them.
        WorldModel many = Garden();
        for (int i = 0; i < 23; i++)
            Expect(many.Open(Parcel("Friend" + i, "Letter " + i, ParcelGift.None, 0, "kc" + i, i % 13)).Ok, "letter " + i + " opens");
        Expect(many.Letters.Length == 20 && many.LetterStamps.Length == 20 && many.Letters[0] == "Letter 3" && many.LetterFrom[19] == "Friend22", "capped at 20");
        Expect(many.LetterStamps[0] == 3 && many.LetterStamps[19] == 22 % 13 && many.LetterStamps.SequenceEqual(Enumerable.Range(3, 20).Select(i => i % 13)), "stamps stay with their letters through the cap");

        // When the stamped letters have all scrolled away, the list is empty again.
        WorldModel scroll = Garden();
        scroll.Open(Parcel("Nan", "Stamped", ParcelGift.None, 0, "kd0", 4));
        for (int i = 0; i < 20; i++) scroll.KeepNote("Mira", "Note " + i);
        Expect(scroll.Letters.Length == 20 && scroll.Export().letterStamps.Length == 0 && scroll.LetterStamps.All(x => x == 0), "no stamps left, nothing stored");
        Expect(State(scroll) == State(Restored(scroll)), "and it round trips");
    }

    private static WorldModel Restored(WorldModel w)
    {
        Expect(WorldModel.TryRestore(w.Export(), out WorldModel back), "restores");
        return back;
    }

    private static void Saving()
    {
        WorldModel w = Garden();
        w.Open(Parcel("Nan", "With a pear", ParcelGift.None, 0, "v1", 6));
        w.Open(Parcel("Ann", "Plain", ParcelGift.None, 0, "v2", 0));
        w.Open(Parcel("Bo", "Tree", ParcelGift.None, 0, "v3", 4));
        WorldModel back = Restored(w);
        Expect(back.LetterStamps.SequenceEqual(new[] { 6, 0, 4 }) && State(back) == State(w), "stamps survive a save");
        Expect(w.Export().letterStamps.SequenceEqual(new[] { 6, 0, 4 }), "the snapshot carries them");
        Expect(!WorldModel.TryRestore(Tweak(w.Export(), s => s.letterStamps = new[] { 6, 0 }), out _), "too short refused");
        Expect(!WorldModel.TryRestore(Tweak(w.Export(), s => s.letterStamps = new[] { 6, 0, 4, 1 }), out _), "too long refused");
        Expect(!WorldModel.TryRestore(Tweak(w.Export(), s => s.letterStamps = new[] { 6, 13, 4 }), out _), "13 refused");
        Expect(!WorldModel.TryRestore(Tweak(w.Export(), s => s.letterStamps = new[] { 6, -1, 4 }), out _), "negative refused");
        Expect(WorldModel.TryRestore(Tweak(w.Export(), s => s.letterStamps = new[] { 12, 1, 0 }), out _), "the full range is accepted");
        Expect(WorldModel.TryRestore(Tweak(w.Export(), s => s.letterStamps = null), out WorldModel bare) && bare.LetterStamps.SequenceEqual(new[] { 0, 0, 0 }), "null restores as no stamps");
        Expect(WorldModel.TryRestore(Tweak(w.Export(), s => s.letterStamps = new[] { 0, 0, 0 }), out WorldModel zeros) && zeros.Export().letterStamps.Length == 0 &&
               State(zeros) == State(bare), "all zeros are stored as none");
        Expect(!WorldModel.TryRestore(Tweak(WorldModel.New(PlayMode.Living).Export(), s => s.letterStamps = new[] { 1 }), out _), "stamps with no letters refused");
        Expect(!WorldModel.TryRestore(Tweak(WorldModel.New(PlayMode.PuzzleOnly).Export(), s => s.letterStamps = new[] { 1 }), out _), "puzzle-only holding any refused");
    }

    private static string Envelope(string snapshot)
    {
        return "{\"format\":\"mosselora-garden\",\"formatVersion\":1,\"exportedAtUtc\":\"" + Stamp + "\",\"checksumSha256\":\"" +
            GardenTransfer.Sha256Hex(snapshot) + "\",\"snapshot\":" + snapshot + "}";
    }

    private static void FileCompatibility()
    {
        WorldModel w = Garden(0, 0, 3);
        w.Open(Parcel("Nan", "With a pear", ParcelGift.None, 0, "g1", 6));
        w.Open(Parcel("Ann", "Plain", ParcelGift.None, 0, "g2", 0));
        string file = GardenTransfer.Export(w, Stamp);
        Expect(file.Contains("\"jam\":3,\"letterStamps\":[6,0]"), "the file carries the stamps, after jam");
        Expect(GardenTransfer.TryImport(file, out WorldModel imported, out GardenImportError error) && imported.LetterStamps.SequenceEqual(new[] { 6, 0 }) && imported.Jam == 3,
            "garden round trip (" + error + ")");
        Expect(GardenTransfer.Export(imported, Stamp) == file, "byte-identical export");
        Expect(!GardenTransfer.TryImport(file.Replace("[6,0]", "[7,0]"), out _, out error) && error == GardenImportError.ChecksumMismatch, "edited stamps fail the checksum");
        Expect(!GardenTransfer.TryImport(file.Replace("\"letterStamps\":[6,0]", "\"letterStamps\":6"), out _, out _), "wrong type refused");
        Expect(!GardenTransfer.TryImport(file.Replace("}}", ",\"letterStampz\":[]}}"), out _, out _), "an unknown key is refused");

        // Letters with no stamps write exactly what they wrote before.
        WorldModel plainGarden = Garden();
        plainGarden.Open(Parcel("Ann", "Plain", ParcelGift.None, 0, "g3", 0));
        string plain = GardenTransfer.Export(plainGarden, Stamp);
        Expect(!plain.Contains("letterStamps"), "no stamps, no key: older files stay byte-identical");
        Expect(GardenTransfer.TryImport(plain, out WorldModel older, out error) && older.Letters.Length == 1 && older.LetterStamps.SequenceEqual(new[] { 0 }), "so an older file imports (" + error + ")");

        string snapshot = plain.Substring(plain.IndexOf("\"snapshot\":", StringComparison.Ordinal) + 11);
        snapshot = snapshot.Substring(0, snapshot.Length - 1);
        string body = snapshot.Substring(0, snapshot.Length - 1);
        Expect(GardenTransfer.TryImport(Envelope(body + ",\"letterStamps\":[5]}"), out WorldModel honest, out _) && honest.LetterStamps.SequenceEqual(new[] { 5 }),
            "a file that checksums its stamps is fine");
        string smuggled = body + ",\"letterStamps\":[5]}";
        Expect(!GardenTransfer.TryImport("{\"format\":\"mosselora-garden\",\"formatVersion\":1,\"exportedAtUtc\":\"" + Stamp + "\",\"checksumSha256\":\"" +
            GardenTransfer.Sha256Hex(snapshot) + "\",\"snapshot\":" + smuggled + "}", out _, out _), "stamps cannot ride under the older checksum");
        Expect(GardenTransfer.TryImport(Envelope(body + ",\"letterStamps\":[0]}"), out WorldModel zeros, out _) && zeros.Export().letterStamps.Length == 0,
            "a checksummed stamps key of zeros is harmless and is stored as none");
        Expect(!GardenTransfer.TryImport(Envelope(body + ",\"letterStamps\":[]}"), out _, out _), "an empty stamps key is not canonical");
    }
}
