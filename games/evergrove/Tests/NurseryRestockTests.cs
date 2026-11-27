using System;
using System.Collections.Generic;
using Evergrove.Core;

/// <summary>
/// I240: the nursery restocks. Buying a slot's plant brings that slot's next edition,
/// a new catalogue item, so buying feeds the collection. Edition 0 is exactly the old nursery.
/// </summary>
internal static class NurseryRestockTests
{
    private static int assertions;

    private static void Expect(bool ok, string message)
    {
        assertions++;
        if (!ok) throw new Exception("Nursery restock test failed: " + message);
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
        EditionZero();
        Distinct();
        Stock();
        Buying();
        Persistence();
        Parcels();
        Fruit();
        // I241: an almanac silhouette can tell you it grows in the nursery.
        for (int kind = 0; kind < Nursery.Kinds; kind++)
            Expect(Nursery.KindOf(Nursery.Item(kind)) == kind, "every nursery plant is found by its item (" + kind + ")");
        int notSold = 0;
        foreach (VillageChoice fam in new[] { VillageChoice.ShadeTree, VillageChoice.Bridge, VillageChoice.Flowers })
            for (int n = 0; n < GardenCatalog.CountOf(fam); n++)
                if (Nursery.KindOf(GardenCatalog.Get(fam, n)) < 0) notSold++;
        Expect(notSold == GardenCatalog.CountOf(VillageChoice.ShadeTree) + GardenCatalog.CountOf(VillageChoice.Bridge) +
            GardenCatalog.CountOf(VillageChoice.Flowers) - Nursery.Kinds, "every other catalogue item isn't sold");
        Expect(Nursery.KindOf(null) == -1, "nothing isn't sold");
        return assertions;
    }

    private static void EditionZero()
    {
        Expect(Nursery.Editions == 5 && Nursery.Slots == 9 && Nursery.Kinds == 45, "five editions of nine slots");
        for (int kind = 0; kind < Nursery.Slots; kind++)
        {
            int row = kind / 3;
            Expect(Nursery.Edition(kind) == 0 && Nursery.Slot(kind) == kind, "kind " + kind + " is edition 0 of its own slot");
            Expect(Nursery.Row(kind) == row && (int)Nursery.Family(kind) == kind % 3, "kind " + kind + " row and family unchanged");
            Expect(Nursery.Price(kind) == new[] { 50, 100, 200 }[row], "kind " + kind + " price unchanged");
            GardenItem old = GardenCatalog.For(row + 1, 2 + row * 4, (VillageChoice)(kind % 3));
            Expect(Nursery.Item(kind).Art == old.Art && Nursery.Item(kind).Name == old.Name, "kind " + kind + " item is the old formula");
        }
        for (int kind = 0; kind < Nursery.Kinds; kind++)
        {
            Expect(Nursery.Edition(kind) == kind / 9 && Nursery.Slot(kind) == kind % 9, "kind " + kind + " edition and slot");
            Expect(Nursery.Row(kind) == (kind % 9) / 3 && (int)Nursery.Family(kind) == kind % 3, "kind " + kind + " row and family follow the slot");
            Expect(Nursery.Price(kind) == Nursery.Price(kind % 9), "kind " + kind + " costs what its slot costs");
            Expect(Nursery.Item(kind).Family == Nursery.Family(kind), "kind " + kind + " item is of its family");
        }
        foreach (int bad in new[] { -1, Nursery.Kinds, 1000 })
        {
            Expect(Throws(() => Nursery.Edition(bad)) && Throws(() => Nursery.Slot(bad)) && Throws(() => Nursery.Item(bad)),
                "kind " + bad + " is refused");
        }
    }

    private static bool Throws(Action a)
    {
        try { a(); } catch (ArgumentOutOfRangeException) { return true; }
        return false;
    }

    private static void Distinct()
    {
        for (int family = 0; family < Nursery.Families; family++)
        {
            var seen = new HashSet<string>();
            int count = 0;
            for (int kind = 0; kind < Nursery.Kinds; kind++)
            {
                if (kind % 3 != family) continue;
                count++;
                Expect(seen.Add(Nursery.Item(kind).Art), "family " + family + " kind " + kind + " has art no other kind has");
            }
            Expect(count == Nursery.Editions * Nursery.Rows && seen.Count == count, "family " + family + " has fifteen distinct items");
        }
    }

    private static int[] Pairs(params int[] kinds)
    {
        var list = new int[kinds.Length * 2];
        for (int i = 0; i < kinds.Length; i++) { list[i * 2] = kinds[i]; list[i * 2 + 1] = i; }
        return list;
    }

    private static void Stock()
    {
        for (int slot = 0; slot < Nursery.Slots; slot++)
        {
            var bought = new List<int>();
            for (int n = 0; n <= 7; n++)
            {
                int expected = Math.Min(n, Nursery.Editions - 1) * Nursery.Slots + slot;
                Expect(Nursery.InStock(slot, bought.ToArray()) == expected, "slot " + slot + " after " + n + " buys offers " + expected);
                bought.Add(expected); bought.Add(n);
            }
        }
        int[] one = Pairs(4);
        Expect(Nursery.InStock(4, one) == 13 && Nursery.InStock(0, one) == 0 && Nursery.InStock(5, one) == 5, "other slots are unaffected");
        Expect(Nursery.InStock(4, new int[0]) == 4 && Nursery.InStock(4, null) == 4, "no buys, edition 0");
        Expect(Throws(() => Nursery.InStock(-1, one)) && Throws(() => Nursery.InStock(9, one)), "unknown slots are refused");
        // Buying a later edition of a slot counts the same as buying its first.
        Expect(Nursery.InStock(4, Pairs(31)) == 13, "any bought kind of a slot restocks it");
    }

    private static void Buying()
    {
        WorldModel w = WithSeeds(5000);
        Expect(w.NurseryStock(0) == 0 && w.NurseryStock(8) == 8, "a new garden stocks edition 0");
        Expect(!w.Buy(9, 1) && !w.Buy(18, 1) && w.BoughtCount == 0 && w.Seeds == 5000, "a later edition is refused before it is in stock");
        Expect(w.Buy(0, 1) && w.Seeds == 4950, "buy edition 0 for 50");
        Expect(w.NurseryStock(0) == 9 && w.NurseryStock(1) == 1, "buying restocks that slot only");
        Expect(!w.Buy(0, 2) && w.Seeds == 4950 && w.BoughtCount == 1, "the sold-out kind is refused");
        Expect(!w.Buy(18, 2) && w.BoughtCount == 1, "so is a kind further ahead");
        Expect(w.Buy(9, 2) && w.Seeds == 4900 && w.NurseryStock(0) == 18, "edition 1 costs the row price and brings edition 2");
        Expect(w.Buy(3, 3) && w.Seeds == 4800 && w.NurseryStock(3) == 12, "row 1 slot costs 100");
        Expect(!w.Buy(21, 4) && w.Buy(12, 4) && w.Seeds == 4700, "row 1 edition 1 follows edition 0");
        Expect(w.Buy(6, 5) && w.Seeds == 4500 && w.Buy(15, 6) && w.Seeds == 4300, "row 2 costs 200 in every edition");
        // Walk a slot to the end: it keeps offering the last edition.
        WorldModel e = WithSeeds(5000);
        for (int n = 0; n < 8; n++)
        {
            int stock = e.NurseryStock(2);
            Expect(stock == Math.Min(n, 4) * 9 + 2, "slot 2 offers " + stock + " at buy " + n);
            Expect(e.Buy(stock, n), "buy " + stock);
        }
        Expect(e.NurseryStock(2) == 38 && e.Buy(38, 20) && e.NurseryStock(2) == 38, "the last edition stays on the shelf");
        Expect(e.Seeds == 5000 - 9 * 50, "nine buys at 50");
        Expect(e.NurseryStock(-1) == -1 && e.NurseryStock(9) == -1, "an unknown slot has no stock");
        // Rows still open in order.
        WorldModel r = WithSeeds(1000);
        Expect(!r.Buy(3, 1) && r.Buy(9, 1) == false, "row 1 closed, later edition refused");
        Expect(r.Buy(0, 1) && r.RowOpen(1) && !r.RowOpen(2), "a row 0 plant opens row 1");
        Expect(r.Buy(9, 2) && r.RowOpen(1), "a later edition keeps rows as they were");
        Expect(!r.Buy(Nursery.Kinds, 1) && !r.Buy(-1, 1), "kinds outside the range are refused");
    }

    private static void Persistence()
    {
        WorldModel w = WithSeeds(5000);
        Expect(w.Buy(0, 1) && w.Buy(9, 2) && w.Buy(3, 3) && w.Buy(18, 4), "buys across editions");
        w.SolveBought(1);
        Expect(WorldModel.TryRestore(w.Export(), out WorldModel back), "later-edition kinds restore");
        Expect(string.Join(",", back.Bought) == string.Join(",", w.Bought) && back.NurseryStock(0) == 27 && back.NurseryStock(3) == 12,
            "and the stock follows from them");
        string file = GardenTransfer.Export(w, "2026-11-27T12:00:00Z");
        Expect(GardenTransfer.TryImport(file, out WorldModel imported, out GardenImportError error) &&
               string.Join(",", imported.Bought) == string.Join(",", w.Bought) && imported.NurseryStock(0) == 27,
               "later editions travel in the garden file (" + error + ")");
        Expect(GardenTransfer.Export(imported, "2026-11-27T12:00:00Z") == file, "and export byte-identically");
        foreach (int kind in new[] { Nursery.Kinds, Nursery.Kinds + 1, -1 })
        {
            WorldSnapshot s = w.Export();
            s.bought = new[] { kind, 1 };
            Expect(!WorldModel.TryRestore(s, out _), "bought kind " + kind + " refused");
            s = w.Export();
            s.gifts = new[] { kind };
            Expect(!WorldModel.TryRestore(s, out _), "gift kind " + kind + " refused");
        }
        WorldSnapshot top = w.Export();
        top.bought = new[] { Nursery.Kinds - 1, 1 };
        top.boughtSolved = new int[0];
        Expect(WorldModel.TryRestore(top, out _), "the last kind restores");
        WorldSnapshot gift = w.Export();
        gift.gifts = new[] { Nursery.Kinds - 1 };
        Expect(WorldModel.TryRestore(gift, out _), "a gift of the last kind restores");
    }

    private static void Parcels()
    {
        int last = Nursery.Kinds - 1;
        foreach (int kind in new[] { 9, 22, last })
        {
            WorldModel a = WithSeeds(5000);
            Expect(a.Buy(0, 1) && a.Buy(3, 2), "setup rows 0 and 1");
            int seeds = a.Seeds;
            ParcelSendResult s = a.SendParcel("Nan", "For you", ParcelGift.Cutting, kind, "r" + kind);
            Expect(s.Ok && a.Seeds == seeds - Nursery.GiftPrice(kind), "a cutting of kind " + kind + " is sent at its gift price (v1 #73)");
            WorldModel b = WithSeeds(0);
            ParcelOpenResult o = b.Open(s.Text);
            Expect(o.Ok && b.Gifts.Length == 1 && b.Gifts[0] == kind, "and arrives as kind " + kind);
            Expect(WorldModel.TryRestore(b.Export(), out WorldModel again) && again.Gifts[0] == kind, "the gift survives a save");
        }
        WorldModel c = WithSeeds(5000);
        Expect(c.Buy(0, 1) && c.Buy(3, 2), "setup");
        int before = c.Seeds;
        Expect(!c.SendParcel("Nan", "", ParcelGift.Cutting, Nursery.Kinds, "x").Ok && c.Seeds == before, "a cutting beyond the range is refused");
        Expect(!c.SendParcel("Nan", "", ParcelGift.Cutting, -1, "y").Ok && c.Seeds == before, "and a negative one");
        Expect(!FamilyParcel.TryCreate("Nan", "", ParcelGift.Cutting, Nursery.Kinds, "z", out _, out _, out _), "the parcel format refuses it too");
        Expect(FamilyParcel.TryCreate("Nan", "", ParcelGift.Cutting, Nursery.Kinds - 1, "z", out _, out _, out _), "and accepts the last kind");
        // Row 2 cuttings still need row 2 open, whatever the edition.
        WorldModel d = WithSeeds(5000);
        Expect(d.Buy(0, 1) && !d.SendParcel("Nan", "", ParcelGift.Cutting, 33, "w").Ok, "a later row 2 cutting needs row 1 bought");
    }

    private static void Fruit()
    {
        WorldModel w = WithSeeds(1000);
        Expect(w.Buy(0, 1) && w.Buy(9, 2) && w.Buy(1, 3) && w.Buy(10, 4), "a tree, a later tree, a building and a later building");
        int fruit = w.Fruit;
        w.SolveBought(1);
        Expect(w.Fruit == fruit + 1, "a later-edition tree gives fruit");
        w.SolveBought(3);
        Expect(w.Fruit == fruit + 1, "a later-edition building gives none");
        Expect(w.SolveBought(1) == WorldModel.TreeReward(Nursery.Row(9), false) && w.Fruit == fruit + 2, "its replay gives fruit and the row's replay seeds");
    }
}
