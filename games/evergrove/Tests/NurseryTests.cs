using System;
using Evergrove.Core;

/// <summary>
/// I205: the nursery. Seeds buy plants from three rows that open in order
/// (50, then 100, then 200: an idea from the founder's daughter). Each bought
/// plant holds its own puzzle, so spending creates new play, and new play earns seeds.
/// </summary>
internal static class NurseryTests
{
    private static int assertions;

    private static void Expect(bool ok, string message)
    {
        assertions++;
        if (!ok) throw new Exception("Nursery test failed: " + message);
    }

    // A garden with twelve grown things, all solved once: 280 seeds.
    private static WorldModel RichWorld()
    {
        WorldModel world = WorldModel.New(PlayMode.Living);
        for (int chapter = 0; chapter < 12; chapter++)
        {
            Puzzle board = world.CurrentPuzzle;
            for (int i = 0; i < board.CellCount; i++)
                while (world.Rotations[i] != board.Solution[i]) world.Rotate(i);
            Expect(world.Choose((VillageChoice)(chapter % 3)), "choice " + chapter + " made");
        }
        for (int item = 0; item < 12; item++) world.SolveTree(item);
        return world;
    }

    private static WorldModel WithSeeds(int seeds)
    {
        WorldSnapshot s = WorldModel.New(PlayMode.Living).Export();
        s.seeds = seeds;
        Expect(WorldModel.TryRestore(s, out WorldModel w), "a garden with " + seeds + " seeds restores");
        return w;
    }

    public static int Run()
    {
        Catalogue();
        Buying();
        Rewards();
        Saving();
        Refusals();
        return assertions;
    }

    private static void Catalogue()
    {
        Expect(Nursery.Kinds == 9, "nine kinds: three rows of three families");
        Expect(Nursery.Price(0) == 50 && Nursery.Price(2) == 50, "row 0 costs 50");
        Expect(Nursery.Price(3) == 100 && Nursery.Price(5) == 100, "row 1 costs 100");
        Expect(Nursery.Price(6) == 200 && Nursery.Price(8) == 200, "row 2 costs 200");
        for (int kind = 0; kind < Nursery.Kinds; kind++)
        {
            Expect(Nursery.Row(kind) == kind / 3, "kind " + kind + " row");
            Expect((int)Nursery.Family(kind) == kind % 3, "kind " + kind + " family");
            GardenItem item = Nursery.Item(kind);
            Expect(item != null && item.Family == Nursery.Family(kind) && item.Art.Length > 0, "kind " + kind + " has art");
            Expect(Nursery.Item(kind).Name == Nursery.Item(kind).Name, "kind " + kind + " is deterministic");
        }
        bool threw = false;
        try { Nursery.Price(9); } catch (ArgumentOutOfRangeException) { threw = true; }
        Expect(threw, "an unknown kind is refused");
    }

    private static void Buying()
    {
        WorldModel world = WithSeeds(49);
        WorldSnapshot before = world.Export();
        Expect(!world.Buy(0, 10), "49 seeds cannot buy a 50-seed plant");
        Expect(world.Seeds == 49 && world.BoughtCount == 0, "a refused buy changes nothing");
        Expect(world.Export().seeds == before.seeds && world.Export().bought.Length == 0, "and the save is unchanged");

        world = WithSeeds(1000);
        Expect(world.RowOpen(0) && !world.RowOpen(1) && !world.RowOpen(2), "only row 0 is open at first");
        Expect(!world.Buy(3, 5) && world.Seeds == 1000, "row 1 is closed until you own a row 0 plant");
        Expect(!world.Buy(6, 5) && world.Seeds == 1000, "row 2 is closed too");
        Expect(world.Buy(1, 5) && world.Seeds == 950 && world.BoughtCount == 1, "buying row 0 subtracts exactly 50");
        Expect(world.RowOpen(1) && !world.RowOpen(2), "a row 0 plant opens row 1");
        Expect(!world.Buy(6, 6), "row 2 is still closed");
        Expect(world.Buy(4, 6) && world.Seeds == 850, "row 1 costs exactly 100");
        Expect(world.RowOpen(2), "a row 1 plant opens row 2");
        Expect(world.Buy(8, 7) && world.Seeds == 650, "row 2 costs exactly 200");
        Expect(world.Bought.Length == 6 && world.Bought[0] == 1 && world.Bought[1] == 5 && world.Bought[4] == 8,
            "bought plants are kept as [kind, cell] pairs in order");
        Expect(!world.Buy(-1, 1) && !world.Buy(9, 1), "unknown kinds refused");
        Expect(!world.Buy(0, -1) && !world.Buy(0, WorldModel.MaxMapCell), "cells off the map refused");
        Expect(world.Seeds == 650 && world.BoughtCount == 3, "refusals changed nothing");

        WorldModel poor = WithSeeds(60);
        Expect(poor.Buy(0, 1) && poor.Seeds == 10, "60 buys one 50");
        Expect(!poor.Buy(0, 2) && poor.Seeds == 10, "and then no more: seeds never go negative");

        Expect(world.MoveBought(0, 99) && world.Bought[1] == 99 && world.Bought[0] == 1, "a bought plant can be moved");
        Expect(!world.MoveBought(3, 1) && !world.MoveBought(-1, 1) && !world.MoveBought(0, -1) &&
               !world.MoveBought(0, WorldModel.MaxMapCell), "bad moves refused");

        WorldModel cap = WithSeeds(WorldModel.MaxSeeds);
        for (int i = 0; i < WorldModel.MaxBought; i++) Expect(cap.Buy(0, i), "buy " + i);
        Expect(!cap.Buy(0, 5) && cap.BoughtCount == WorldModel.MaxBought, "there is a cap on bought plants");

        WorldSnapshot puzzleOnly = WorldModel.New(PlayMode.PuzzleOnly).Export();
        Expect(!WorldModel.TryRestore(Tweak(puzzleOnly, s => s.bought = new[] { 0, 1 }), out _), "bought plants belong to the living garden");
        WorldModel notLiving = WorldModel.New(PlayMode.PuzzleOnly);
        Expect(!notLiving.Buy(0, 1), "buying needs the living garden");
    }

    private static WorldSnapshot Tweak(WorldSnapshot s, Action<WorldSnapshot> edit) { edit(s); return s; }

    private static void Rewards()
    {
        WorldModel world = WithSeeds(1000);
        Expect(world.Buy(0, 1) && world.Buy(3, 2) && world.Buy(6, 3), "one of each row (rows open in order)");
        for (int row = 0; row < 3; row++)
        {
            Expect(!world.BoughtSolved(row), "not solved before playing");
            int first = world.SolveBought(row);
            Expect(first == WorldModel.TreeReward(row, true) && world.BoughtSolved(row), "row " + row + " first solve pays the tier's first reward");
            int replay = world.SolveBought(row);
            Expect(replay == WorldModel.TreeReward(row, false) && first > replay, "a first solve pays more than a replay at row " + row);
        }
        int seeds = world.Seeds;
        Expect(world.SolveBought(3) == 0 && world.SolveBought(-1) == 0 && world.Seeds == seeds, "only things you bought hold puzzles");

        // Opening every row comes from the twelve grown things alone.
        int firstSolves = 0;
        for (int item = 0; item < 12; item++) firstSolves += WorldModel.TreeReward(TreePuzzles.TierFor(item), true);
        Expect(firstSolves >= Nursery.Price(0) + Nursery.Price(3), "twelve first solves open every row (" + firstSolves + ")");
        Expect(RichWorld().Seeds == firstSolves, "the rich test garden holds exactly those seeds");
        WorldModel path = RichWorld();
        Expect(path.Buy(0, 1) && path.Buy(3, 2) && path.RowOpen(2), "50 then 100 opens row 2 from first solves alone");

        // Reconsidering a choice keeps bought plants and seeds.
        WorldModel keep = RichWorld();
        Expect(keep.Buy(0, 1) && keep.SolveBought(0) == 10, "bought and solved");
        int seedsBefore = keep.Seeds;
        keep.ReconsiderLatestChoice();
        Expect(keep.BoughtCount == 1 && keep.BoughtSolved(0) && keep.Seeds >= seedsBefore - 40, "reconsidering leaves bought plants alone");
    }

    private static void Saving()
    {
        WorldModel world = RichWorld();
        Expect(world.Buy(0, 11) && world.Buy(4, 12), "buys");
        world.SolveBought(1);
        Expect(WorldModel.TryRestore(world.Export(), out WorldModel back), "a save with bought plants restores");
        Expect(string.Join(",", back.Bought) == string.Join(",", world.Bought) && back.BoughtSolved(1) && !back.BoughtSolved(0) &&
               back.Seeds == world.Seeds, "bought plants, what is solved and seeds survive a save");

        string file = GardenTransfer.Export(world, "2026-11-26T12:00:00Z");
        Expect(GardenTransfer.TryImport(file, out WorldModel imported, out GardenImportError error) &&
               string.Join(",", imported.Bought) == string.Join(",", world.Bought) && imported.BoughtSolved(1),
               "bought plants travel in the garden file (" + error + ")");
        Expect(GardenTransfer.Export(imported, "2026-11-26T12:00:00Z") == file, "and export byte-identically");

        // A garden file from before the nursery (no new keys) still imports, with an empty nursery.
        WorldModel plain = RichWorld();
        string older = GardenTransfer.Export(plain, "2026-11-26T12:00:00Z");
        string text = Snapshot(older);
        string oldText = StripNurseryKeys(text);
        Expect(oldText != text, "the older form really lacks the new keys");
        string legacy = Envelope(oldText, GardenTransfer.Sha256Hex(oldText));
        Expect(GardenTransfer.TryImport(legacy, out WorldModel fromOld, out error) && fromOld.BoughtCount == 0 &&
               fromOld.Seeds == plain.Seeds, "a pre-nursery garden file imports (" + error + ")");
        // The old checksum must not cover a garden that holds bought plants.
        string snapshot = Snapshot(file);
        string forged = Envelope(snapshot, GardenTransfer.Sha256Hex(StripNurseryKeys(snapshot)));
        Expect(!GardenTransfer.TryImport(forged, out _, out error) && error == GardenImportError.ChecksumMismatch,
            "bought plants cannot ride under a pre-nursery checksum");
    }

    private static void Refusals()
    {
        WorldModel world = RichWorld();
        Expect(world.Buy(0, 1) && world.Buy(0, 2), "setup");
        WorldSnapshot good = world.Export();
        Expect(WorldModel.TryRestore(good, out _), "the setup restores");
        Expect(!WorldModel.TryRestore(Tweak(world.Export(), s => s.bought = new[] { 0, 1, 2 }), out _), "odd length refused");
        Expect(!WorldModel.TryRestore(Tweak(world.Export(), s => s.bought = new[] { 9, 1 }), out _), "unknown kind refused");
        Expect(!WorldModel.TryRestore(Tweak(world.Export(), s => s.bought = new[] { -1, 1 }), out _), "negative kind refused");
        Expect(!WorldModel.TryRestore(Tweak(world.Export(), s => s.bought = new[] { 0, -1 }), out _), "negative cell refused");
        Expect(!WorldModel.TryRestore(Tweak(world.Export(), s => s.bought = new[] { 0, WorldModel.MaxMapCell }), out _), "cell off the map refused");
        Expect(!WorldModel.TryRestore(Tweak(world.Export(), s => s.bought = new int[(WorldModel.MaxBought + 1) * 2]), out _), "too many refused");
        Expect(!WorldModel.TryRestore(Tweak(world.Export(), s => s.boughtSolved = new[] { 0, 0 }), out _), "duplicate solved refused");
        Expect(!WorldModel.TryRestore(Tweak(world.Export(), s => s.boughtSolved = new[] { 2 }), out _), "solved index beyond the plants refused");
        Expect(!WorldModel.TryRestore(Tweak(world.Export(), s => s.boughtSolved = new[] { -1 }), out _), "negative solved index refused");
        Expect(WorldModel.TryRestore(Tweak(world.Export(), s => s.boughtSolved = new[] { 1, 0 }), out _), "solved in any order is fine");
        Expect(WorldModel.TryRestore(Tweak(world.Export(), s => s.bought = new[] { 6, 1 }), out _), "a closed row is structurally fine on restore");
        WorldSnapshot matched = WorldModel.New(PlayMode.PuzzleOnly).Export();
        Expect(!WorldModel.TryRestore(Tweak(matched, s => s.boughtSolved = new[] { 0 }), out _), "solved list needs the living garden");
    }

    private static string Snapshot(string envelope)
    {
        const string marker = "\"snapshot\":";
        int start = envelope.IndexOf(marker, StringComparison.Ordinal) + marker.Length;
        return envelope.Substring(start, envelope.Length - start - 1);
    }

    private static string StripNurseryKeys(string text)
    {
        int start = text.IndexOf(",\"bought\":", StringComparison.Ordinal);
        if (start < 0) return text;
        int end = text.IndexOf(']', text.IndexOf(",\"boughtSolved\":", start, StringComparison.Ordinal));
        int letterFrom = text.IndexOf(",\"letterFrom\":", start, StringComparison.Ordinal);
        if (letterFrom >= 0) end = text.IndexOf(']', letterFrom); // family keys follow boughtSolved
        return text.Substring(0, start) + text.Substring(end + 1);
    }

    private static string Envelope(string snapshot, string checksum)
    {
        return "{\"format\":\"mosselora-garden\",\"formatVersion\":1,\"exportedAtUtc\":\"2026-11-26T12:00:00Z" +
            "\",\"checksumSha256\":\"" + checksum + "\",\"snapshot\":" + snapshot + "}";
    }
}
