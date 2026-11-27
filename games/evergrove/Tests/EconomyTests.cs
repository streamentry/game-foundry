using System;
using System.Linq;
using Evergrove.Core;

/// <summary>
/// The seed economy: every change to seeds is recorded in the ledger with its reason (v1 #78), the daily
/// replay cap pays the first solves of a day in full and later replays less (v1 #80), and giving a cutting
/// costs half the nursery price (v1 #73). Seeds only move or are earned by play; nothing is made from nothing.
/// </summary>
internal static class EconomyTests
{
    private static int assertions;
    private const int Day1 = 20261127;
    private const int Day2 = 20261128;
    private const string Stamp = "2026-11-27T12:00:00Z";

    private static void Expect(bool ok, string message)
    {
        assertions++;
        if (!ok) throw new Exception("Economy test failed: " + message);
    }

    private static string State(WorldModel w) { return GardenTransfer.SnapshotJson(w.Export()); }
    private static WorldSnapshot Tweak(WorldSnapshot s, Action<WorldSnapshot> change) { change(s); return s; }

    private static WorldModel Restored(WorldSnapshot s)
    {
        Expect(WorldModel.TryRestore(s, out WorldModel w), "snapshot restores");
        return w;
    }

    // A living garden with `count` grown things (none solved yet) and the given seeds; the ledger starts empty.
    private static WorldModel Grown(int count, int seeds)
    {
        WorldModel world = WorldModel.New(PlayMode.Living);
        for (int chapter = 0; chapter < count; chapter++)
        {
            Puzzle board = world.CurrentPuzzle;
            for (int i = 0; i < board.CellCount; i++)
                while (world.Rotations[i] != board.Solution[i]) world.Rotate(i);
            world.Choose((VillageChoice)(chapter % 3));
        }
        WorldSnapshot s = world.Export();
        s.seeds = seeds;
        return Restored(s);
    }

    // Twelve grown things with the given nursery pairs [kind, cell, ...] already bought.
    private static WorldModel WithBought(int seeds, params int[] bought)
    {
        WorldSnapshot s = Grown(3, 0).Export();
        s.seeds = seeds;
        s.bought = bought;
        return Restored(s);
    }

    private static string Lines(WorldModel w)
    {
        return string.Join(";", w.Ledger.Select(l => ((int)l.Reason) + ":" + l.Delta));
    }

    // Writes the first ledger line of a fresh garden with the given seeds after one action, then checks it.
    private static void OneLine(WorldModel w, int before, SeedReason reason, int delta, string what)
    {
        Expect(w.Seeds == before + delta, what + ": seeds moved by " + delta);
        Expect(w.Ledger.Length == 1 && w.Ledger[0].Reason == reason && w.Ledger[0].Delta == delta,
            what + ": exactly one " + reason + " line of " + delta + " (got " + Lines(w) + ")");
    }

    public static int Run()
    {
        Recording();
        RecordingRefusals();
        LedgerCap();
        SaveRoundTrip();
        SaveForgeries();
        ReplayCap();
        Grinding();
        GiftPrices();
        PairingLine();
        return assertions;
    }

    // A pairing found for the first time pays through ChangeSeeds and writes one Pairing line; never twice.
    private static void PairingLine()
    {
        WorldModel w = Grown(3, 100);
        int paid = w.DiscoverPairing(Pairing.Sheltered);
        Expect(paid == Pairings.Reward && w.Ledger.Length == 1 && w.Ledger[0].Reason == SeedReason.Pairing &&
               w.Ledger[0].Delta == Pairings.Reward, "a discovered pairing writes one Pairing line");
        Expect(w.DiscoverPairing(Pairing.Sheltered) == 0 && w.Ledger.Length == 1, "a pairing found again writes nothing");
        WorldModel full = Grown(3, WorldModel.MaxSeeds);
        Expect(full.DiscoverPairing(Pairing.Grove) == 0 && full.Ledger.Length == 0, "a pairing at the seed cap applies nothing and writes nothing");
    }

    private static void Recording()
    {
        Expect(WorldModel.New(PlayMode.Living).Ledger.Length == 0, "a new garden has an empty ledger");

        WorldModel first = Grown(3, 0);
        Expect(first.SolveTree(0) == WorldModel.TreeReward(TreePuzzles.TierFor(0), true), "first solve pays in full");
        OneLine(first, 0, SeedReason.FirstSolve, WorldModel.TreeReward(TreePuzzles.TierFor(0), true), "first solve");

        WorldModel replay = Grown(3, 0);
        replay.SolveTree(0);
        int replayed = replay.SolveTree(0);
        Expect(replayed == WorldModel.TreeReward(TreePuzzles.TierFor(0), false), "a replay on day 0 pays as before");
        Expect(replay.Ledger.Length == 2 && replay.Ledger[0].Reason == SeedReason.Replay && replay.Ledger[0].Delta == replayed,
            "replay writes one Replay line, newest first");

        WorldModel bought = WithBought(100);
        int seeds = bought.Seeds;
        Expect(bought.Buy(0, 9) && bought.Seeds == seeds - 50, "buying spends the price");
        OneLine(bought, seeds, SeedReason.Nursery, -50, "buy");

        WorldModel bought2 = WithBought(100);
        Expect(bought2.Buy(0, 3), "buy a plant");
        Expect(bought2.Ledger.Length == 1, "buy wrote one line before the solve");
        int bought2Earned = bought2.SolveBought(0);
        Expect(bought2.Ledger.Length == 2 && bought2.Ledger[0].Reason == SeedReason.FirstSolve && bought2.Ledger[0].Delta == bought2Earned,
            "first solve of a bought plant writes a FirstSolve line");

        WorldModel keep = Grown(0, 100);
        Expect(keep.TakeKeepsake(0), "keepsake taken");
        OneLine(keep, 100, SeedReason.Keepsake, -Nursery.Price(0), "keepsake");

        WorldModel sender = Grown(0, 100);
        ParcelSendResult seedGift = sender.SendParcel("Anh", "", ParcelGift.Seeds, 20, "e-seed");
        Expect(seedGift.Ok, "seeds parcel sent");
        OneLine(sender, 100, SeedReason.ParcelSent, -20, "seeds parcel sent");

        WorldModel cutter = WithBought(100, 0, 1);
        ParcelSendResult cut = cutter.SendParcel("Anh", "", ParcelGift.Cutting, 0, "e-cut");
        Expect(cut.Ok && cut.Cost == Nursery.GiftPrice(0), "cutting sent at the gift price");
        OneLine(cutter, 100, SeedReason.ParcelSent, -Nursery.GiftPrice(0), "cutting sent");

        WorldModel giver = Grown(0, 100);
        string parcel = giver.SendParcel("Anh", "", ParcelGift.Seeds, 30, "e-open").Text;
        WorldModel receiver = Grown(0, 0);
        ParcelOpenResult opened = receiver.Open(parcel);
        Expect(opened.Ok && opened.SeedsGained == 30, "seeds parcel opened");
        OneLine(receiver, 0, SeedReason.ParcelOpened, 30, "seeds parcel opened");
    }

    private static void RecordingRefusals()
    {
        WorldModel poor = WithBought(10, 0, 1);
        string before = State(poor);
        Expect(!poor.Buy(0, 9) && State(poor) == before && poor.Ledger.Length == 0, "an unaffordable buy writes nothing");

        WorldModel low = Grown(0, 10);
        string lowBefore = State(low);
        Expect(!low.TakeKeepsake(0) && State(low) == lowBefore && low.Ledger.Length == 0, "an unaffordable keepsake writes nothing");

        WorldModel cheap = Grown(0, 5);
        ParcelSendResult refused = cheap.SendParcel("Anh", "", ParcelGift.Seeds, 20, "e-refuse");
        Expect(!refused.Ok && refused.Error == ParcelError.NotEnoughSeeds && cheap.Ledger.Length == 0 && cheap.Seeds == 5,
            "a refused send writes nothing");

        WorldModel repeat = Grown(0, 100);
        string parcel = repeat.SendParcel("Anh", "", ParcelGift.Seeds, 30, "e-dup").Text;
        WorldModel once = Grown(0, 0);
        Expect(once.Open(parcel).Ok, "first open works");
        int lines = once.Ledger.Length;
        Expect(!once.Open(parcel).Ok && once.Ledger.Length == lines, "a refused open writes nothing");

        // A change that applies nothing (the cap) writes no line.
        WorldModel full = Grown(3, WorldModel.MaxSeeds);
        Expect(full.SolveTree(0) == WorldModel.TreeReward(TreePuzzles.TierFor(0), true), "first solve reports its reward");
        Expect(full.Seeds == WorldModel.MaxSeeds && full.Ledger.Length == 0, "at the cap the reward applies nothing and writes no line");

        WorldModel near = Grown(3, WorldModel.MaxSeeds - 4);
        near.SolveTree(0);
        Expect(near.Seeds == WorldModel.MaxSeeds && near.Ledger.Length == 1 && near.Ledger[0].Delta == 4,
            "a reward past the cap records only the amount applied");
        near.SolveTree(1);
        Expect(near.Ledger.Length == 1, "a capped reward adds no line");
    }

    private static void LedgerCap()
    {
        WorldModel w = WithBought(2000);
        Expect(w.Buy(0, 7), "buy the first plant (oldest line)");
        Expect(w.SolveTree(0) > 0, "first solve");
        for (int i = 0; i < 18; i++) Expect(w.TakeKeepsake(0), "keepsake " + i);
        Expect(w.Ledger.Length == 20, "twenty lines held (got " + w.Ledger.Length + ")");
        int replayed = w.SolveTree(0);
        Expect(w.Ledger.Length == WorldModel.MaxLedger && w.Ledger[0].Reason == SeedReason.Replay && w.Ledger[0].Delta == replayed,
            "the newest line is first");
        Expect(w.Ledger[19].Reason == SeedReason.FirstSolve && w.Ledger.All(l => l.Reason != SeedReason.Nursery),
            "the 21st line dropped the oldest (the plant purchase)");
        Expect(w.Ledger[18].Reason == SeedReason.Keepsake, "the lines between are kept in order");
    }

    private static void SaveRoundTrip()
    {
        WorldModel w = WithBought(2000);
        Expect(w.Buy(0, 7), "a plant to save with");
        w.SolveTree(0, Day1);
        w.SolveTree(0, Day1);
        string ledger = Lines(w);
        WorldSnapshot s = w.Export();
        Expect(s.ledger.Length == 6 && s.payDay == Day1 && s.replaysPaid == 1, "the snapshot carries ledger and replay cap");

        Expect(WorldModel.TryRestore(w.Export(), out WorldModel back) && Lines(back) == ledger &&
               back.Export().payDay == Day1 && back.Export().replaysPaid == 1, "ledger and cap survive a snapshot round trip");

        string file = GardenTransfer.Export(w, Stamp);
        Expect(file.Contains("\"ledger\":[") && file.Contains("\"payDay\":" + Day1) && file.Contains("\"replaysPaid\":1"),
            "the garden file carries ledger and cap");
        Expect(GardenTransfer.TryImport(file, out WorldModel imported, out GardenImportError error) && Lines(imported) == ledger &&
               imported.Export().payDay == Day1 && imported.Export().replaysPaid == 1, "garden file round trip (" + error + ")");
        Expect(GardenTransfer.Export(imported, Stamp) == file, "byte-identical export after import");

        // Older saves have none of these fields: they restore empty, and a fresh garden writes no keys.
        Expect(WorldModel.TryRestore(Tweak(Grown(3, 50).Export(), x => { x.ledger = null; x.payDay = 0; x.replaysPaid = 0; }),
            out WorldModel older) && older.Ledger.Length == 0 && older.Export().payDay == 0, "a snapshot without the fields restores empty");
        string plain = GardenTransfer.Export(Grown(3, 50), Stamp);
        Expect(!plain.Contains("\"ledger\"") && !plain.Contains("\"payDay\"") && !plain.Contains("\"replaysPaid\""),
            "no ledger, no cap: older files stay byte-identical");
        Expect(GardenTransfer.TryImport(plain, out WorldModel plainBack, out error) && plainBack.Ledger.Length == 0,
            "a file without the fields imports (" + error + ")");
    }

    private static void SaveForgeries()
    {
        WorldModel busy = WithBought(2000, 0, 1);
        busy.Buy(0, 7);
        busy.SolveTree(0, Day1);
        busy.SolveTree(0, Day1);
        string before = State(busy);

        Refused("odd ledger length", Tweak(busy.Export(), s => s.ledger = new[] { 1, 10, 2 }));
        Refused("unknown reason 0", Tweak(busy.Export(), s => s.ledger = new[] { 0, 10 }));
        Refused("unknown reason 99", Tweak(busy.Export(), s => s.ledger = new[] { 99, 10 }));
        Refused("negative reason", Tweak(busy.Export(), s => s.ledger = new[] { -1, 10 }));
        Refused("zero delta", Tweak(busy.Export(), s => s.ledger = new[] { 1, 0 }));
        Refused("delta past the cap", Tweak(busy.Export(), s => s.ledger = new[] { 1, WorldModel.MaxSeeds + 1 }));
        Refused("delta at int minimum", Tweak(busy.Export(), s => s.ledger = new[] { 1, int.MinValue }));
        Refused("oversize ledger", Tweak(busy.Export(), s => s.ledger = Enumerable.Repeat(new[] { 1, 10 }, 21).SelectMany(p => p).ToArray()));
        Refused("ledger in a puzzle garden", Tweak(WorldModel.New(PlayMode.PuzzleOnly).Export(), s => s.ledger = new[] { 1, 10 }));
        Refused("bad payDay (negative)", Tweak(busy.Export(), s => s.payDay = -1));
        Refused("bad payDay (before 1900)", Tweak(busy.Export(), s => s.payDay = 19000100));
        Refused("bad payDay (too large)", Tweak(busy.Export(), s => s.payDay = 100000000));
        Refused("negative replaysPaid", Tweak(busy.Export(), s => s.replaysPaid = -1));
        Refused("replaysPaid past the cap", Tweak(busy.Export(), s => s.replaysPaid = WorldModel.MaxReplaysPaid + 1));
        Refused("cap in a puzzle garden", Tweak(WorldModel.New(PlayMode.PuzzleOnly).Export(), s => s.payDay = Day1));

        // A forged line in a file is refused by the checksum or by validation, and the garden is untouched.
        string file = GardenTransfer.Export(busy, Stamp);
        Expect(!GardenTransfer.TryImport(file.Replace("\"replaysPaid\":1", "\"replaysPaid\":-1"), out _, out _), "an edited cap fails import");
        Expect(State(busy) == before, "forgeries change nothing");

        // Reasons 7 (Pairing) and 8 (Season) are defined and restore; reason 99 is refused.
        Expect(WorldModel.TryRestore(Tweak(Grown(3, 0).Export(), s => s.ledger = new[] { 7, 5 }), out WorldModel paired) &&
               paired.Ledger.Length == 1 && paired.Ledger[0].Reason == SeedReason.Pairing && paired.Ledger[0].Delta == 5,
            "a Pairing line restores");
        Expect(WorldModel.TryRestore(Tweak(Grown(3, 0).Export(), s => s.ledger = new[] { 8, 5 }), out WorldModel seasonal) &&
               seasonal.Ledger.Length == 1 && seasonal.Ledger[0].Reason == SeedReason.Season && (int)SeedReason.Season == 8,
            "a Season line restores (v1 #78 reserved reason 8)");
        Refused("reason 99", Tweak(Grown(3, 0).Export(), s => s.ledger = new[] { 99, 5 }));
    }

    private static void Refused(string what, WorldSnapshot forged)
    {
        Expect(!WorldModel.TryRestore(forged, out _), "forged " + what + " is refused");
    }

    private static void ReplayCap()
    {
        WorldModel w = Grown(3, 0);
        int tier = TreePuzzles.TierFor(0);
        int full = WorldModel.TreeReward(tier, false);
        Expect(w.SolveTree(0, Day1) == WorldModel.TreeReward(tier, true), "first solve pays in full, whatever the day");
        Expect(w.Export().payDay == 0 && w.Export().replaysPaid == 0, "a first solve does not touch the cap");

        int[] expected = { full, full, full, 1, 1, 1, 1, 1, 0, 0 };
        for (int i = 0; i < expected.Length; i++)
        {
            string before = State(w);
            int predicted = w.ReplayRewardOn(tier, Day1);
            Expect(State(w) == before, "ReplayRewardOn changes nothing (replay " + i + ")");
            int paid = w.SolveTree(0, Day1);
            Expect(predicted == paid && paid == expected[i], "replay " + i + " on day one pays " + expected[i] + " (paid " + paid + ")");
        }
        Expect(w.Ledger.Length == 9, "only the paid replays write lines (got " + w.Ledger.Length + ")");
        Expect(w.Export().replaysPaid == 10, "every replay counts, paid or not");

        Expect(w.ReplayRewardOn(tier, Day2) == full && w.SolveTree(0, Day2) == full, "a new day resets the cap");
        Expect(w.Export().payDay == Day2 && w.Export().replaysPaid == 1, "the new day is recorded");

        // Setting the clock back to day one counts against day two: switching dates cannot reset the cap.
        Expect(w.ReplayRewardOn(tier, Day1) == full && w.SolveTree(0, Day1) == full &&
               w.Export().payDay == Day2 && w.Export().replaysPaid == 2, "an earlier day counts against the latest paid day");
        for (int i = 0; i < 20; i++) { w.SolveTree(0, Day1); w.SolveTree(0, Day2); }
        Expect(w.ReplayRewardOn(tier, Day1) == 0 && w.ReplayRewardOn(tier, Day2) == 0,
               "alternating two dates still runs the cap dry");
        WorldModel back = Grown(3, 0);
        back.SolveTree(0);
        for (int i = 0; i < 3; i++) back.SolveTree(0, Day2);
        Expect(back.SolveTree(0, Day1) == WorldModel.SmallReplayReward, "a replay dated before the paid day pays from that day's count");

        int paidBefore = w.Export().replaysPaid;
        Expect(w.ReplayRewardOn(tier, 0) == full && w.SolveTree(0) == full, "day 0 keeps the uncapped replay reward");
        Expect(w.Export().payDay == Day2 && w.Export().replaysPaid == paidBefore, "day 0 leaves the cap alone");

        WorldModel first = Grown(3, 0);
        first.SolveTree(0);
        int firstOfOther = first.SolveTree(1, Day1);
        Expect(firstOfOther == WorldModel.TreeReward(TreePuzzles.TierFor(1), true), "a first solve on a new day pays in full");
        Expect(first.Export().replaysPaid == 0, "a first solve does not count toward the cap");

        // Replays of trees and bought plants share one day's cap.
        WorldModel shared = WithBought(200, 0, 1);
        shared.SolveTree(0, Day1);
        shared.SolveBought(0, Day1);
        int boughtRow = WorldModel.TreeReward(0, false);
        for (int i = 0; i < WorldModel.FullReplaysPerDay; i++)
            Expect(shared.SolveBought(0, Day1) == boughtRow, "bought replay " + i + " pays in full");
        Expect(shared.SolveTree(0, Day1) == WorldModel.SmallReplayReward, "a tree replay after the full ones pays the small reward");
    }

    private static void Grinding()
    {
        for (int tier = 0; tier < TreePuzzles.Tiers; tier++)
        {
            int item = -1;
            for (int candidate = 0; candidate < 12 && item < 0; candidate++)
                if (TreePuzzles.TierFor(candidate) == tier) item = candidate;
            Expect(item >= 0, "a grown thing of tier " + tier + " exists");

            WorldModel w = Grown(12, 0);
            w.SolveTree(item, Day1);
            int total = 0;
            for (int i = 0; i < 200; i++) total += w.SolveTree(item, Day1);

            int normalDay = WorldModel.FullReplaysPerDay * WorldModel.TreeReward(tier, false);
            int extra = WorldModel.SmallReplaysPerDay * WorldModel.SmallReplayReward;
            Expect(total == normalDay + extra, "tier " + tier + ": 200 replays pay one full day plus the small tail (got " + total + ")");
            Expect(total < 2 * normalDay, "tier " + tier + ": 200 replays earn less than two full days");
            Expect(extra < WorldModel.FullReplaysPerDay * WorldModel.TreeReward(0, false),
                "the tail beyond a normal day is less than one tier-0 replay day");
        }
    }

    private static void GiftPrices()
    {
        for (int kind = 0; kind < Nursery.Kinds; kind++)
        {
            int gift = Nursery.GiftPrice(kind);
            Expect(gift > 0 && gift < Nursery.Price(kind) && gift == Nursery.Price(kind) / 2,
                "kind " + kind + ": a gift costs half, above zero");
        }

        // Open all three rows: kind 0 (row 0), kind 3 (row 1), kind 6 (row 2).
        WorldModel shelfGarden = WithBought(1000, 0, 1, 3, 2, 6, 4);
        WorldSnapshot shelf = shelfGarden.Export();
        for (int kind = 0; kind < Nursery.Kinds; kind++)
        {
            WorldModel sender = Restored(shelf);
            ParcelSendResult sent = sender.SendParcel("Anh", "", ParcelGift.Cutting, kind, "gift" + kind);
            Expect(sent.Ok && sent.Cost == Nursery.GiftPrice(kind) && sender.Seeds == 1000 - Nursery.GiftPrice(kind),
                "a cutting of kind " + kind + " costs its gift price");
            Expect(sender.Ledger[0].Reason == SeedReason.ParcelSent && sender.Ledger[0].Delta == -Nursery.GiftPrice(kind),
                "the gift is one ParcelSent line");

            WorldModel keeper = Restored(shelf);
            Expect(keeper.TakeKeepsake(kind) && keeper.Seeds == 1000 - Nursery.Price(kind),
                "a keepsake of kind " + kind + " costs the full price");
        }
    }
}
