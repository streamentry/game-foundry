using System;
using System.Collections.Generic;
using Evergrove.Core;

/// <summary>
/// Item 65: food shared with family appears on the cottage table. Round trip: garden A cooks and sends
/// fruit and jam, garden B opens the parcels, and FamilyTable lists exactly what B holds.
/// </summary>
internal static class SharedFoodTests
{
    private static int assertions;

    private static void Expect(bool ok, string message)
    {
        assertions++;
        if (!ok) throw new Exception("SharedFoodTests failed: " + message);
    }

    private static WorldModel WithFood(int fruit, int jam)
    {
        WorldSnapshot s = WorldModel.New(PlayMode.Living).Export();
        s.fruit = fruit;
        s.jam = jam;
        Expect(WorldModel.TryRestore(s, out WorldModel world), "restore with " + fruit + " fruit and " + jam + " jam");
        return world;
    }

    private static string Describe(IReadOnlyList<FamilyTableItem> items)
    {
        var parts = new List<string>();
        foreach (FamilyTableItem item in items)
            parts.Add(item.Kind + ":" + item.Shown + "/" + item.Total + ":" + item.Words);
        return string.Join(" | ", parts.ToArray());
    }

    private static void RoundTrip()
    {
        // Garden A: 10 fruit cooked into 2 jars leaves 4 fruit and 2 jars.
        WorldModel a = WithFood(10, 0);
        Expect(a.MakeJam() && a.MakeJam(), "garden A cooks two jars");
        Expect(a.Fruit == 4 && a.Jam == 2, "A holds 4 fruit and 2 jars before sending");

        ParcelSendResult fruitSent = a.SendParcel("Anh", "Pears from my tree", ParcelGift.Fruit, 3, "f1");
        Expect(fruitSent.Ok && fruitSent.Text != null, "A sends 3 fruit");
        ParcelSendResult jamSent = a.SendParcel("Anh", "", ParcelGift.Jam, 2, "j1");
        Expect(jamSent.Ok && jamSent.Text != null, "A sends 2 jars");
        Expect(a.Fruit == 1 && a.Jam == 0, "A's counts drop by exactly the sent amounts");

        // Garden B starts empty, so what the table shows is exactly what arrived.
        WorldModel b = WorldModel.New(PlayMode.Living);
        Expect(FamilyTable.Items(b).Count == 0, "an empty garden shows nothing on the table");
        Expect(b.Open(fruitSent.Text).FruitGained == 3, "B opens the fruit parcel");
        Expect(b.Open(jamSent.Text).JamGained == 2, "B opens the jam parcel");

        IReadOnlyList<FamilyTableItem> items = FamilyTable.Items(b);
        Expect(items.Count == 2, "the table has a fruit group and a jam group: " + Describe(items));
        Expect(items[0].Kind == FamilyTableKind.Fruit && items[0].Total == 3 && items[0].Shown == 3,
            "3 fruit on the table");
        Expect(items[0].Words == "3 fruit in your basket", "fruit caption: " + items[0].Words);
        Expect(items[1].Kind == FamilyTableKind.Jam && items[1].Total == 2 && items[1].Shown == 2,
            "2 jars on the table");
        Expect(items[1].Words == "2 jars of jam in your pantry", "jam caption: " + items[1].Words);
        string described = Describe(items);
        Expect(!described.Contains("Anh"), "senders are not recorded per item, so no name appears");

        // Deterministic: the same garden gives the same list every time.
        Expect(Describe(FamilyTable.Items(b)) == described, "the table list is deterministic");

        // A replayed parcel is refused and changes nothing on the table.
        Expect(b.Open(fruitSent.Text).Error == ParcelError.AlreadyOpened, "a replayed parcel is refused");
        Expect(Describe(FamilyTable.Items(b)) == described, "a refused replay leaves the table unchanged");

        // Save round trip, both the snapshot and the text export.
        WorldSnapshot snap = b.Export();
        Expect(WorldModel.TryRestore(snap, out WorldModel fromSnapshot), "snapshot restores");
        Expect(Describe(FamilyTable.Items(fromSnapshot)) == described, "the table survives a snapshot round trip");

        string saved = GardenTransfer.Export(b, "2026-11-27T00:00:00Z");
        Expect(GardenTransfer.TryImport(saved, out WorldModel imported, out GardenImportError importError),
            "the export imports: " + importError);
        Expect(Describe(FamilyTable.Items(imported)) == described, "the table survives an export and import");
    }

    private static void Capping()
    {
        IReadOnlyList<FamilyTableItem> big = FamilyTable.Items(WithFood(15, 10));
        Expect(big.Count == 2, "a large garden still has two groups");
        Expect(big[0].Shown == FamilyTable.MaxFruitShown && big[0].Total == 15, "fruit drawn up to 9 of 15");
        Expect(big[0].Words == "15 fruit in your basket", "the caption states the true 15: " + big[0].Words);
        Expect(big[1].Shown == FamilyTable.MaxJarsShown && big[1].Total == 10, "jars drawn up to 6 of 10");
        Expect(big[1].Words == "10 jars of jam in your pantry", "the caption states the true 10: " + big[1].Words);

        Expect(FamilyTable.Items(WithFood(1, 1))[1].Words == "1 jar of jam in your pantry", "one jar is singular");
        Expect(FamilyTable.Items(null).Count == 0, "no world, no table");
    }

    public static int Run()
    {
        RoundTrip();
        Capping();
        return assertions;
    }
}
