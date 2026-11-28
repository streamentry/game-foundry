using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Evergrove.Core;

// Checklist 42, each stretch has a character: a palette, its own neighbour and its own puzzle flavour. The
// pure rules (StretchCharacter) are tested here; the painted tint, the flavour marks on the name plank, the
// neighbour placed in the stretch and the tour are checked by source audit, since UnityEngine is not part
// of the headless Core build.
internal static class StretchCharacterTests
{
    private static int assertions;

    private static void Expect(bool ok, string message)
    {
        assertions++;
        if (!ok) throw new Exception("StretchCharacterTests failed: " + message);
    }

    public static int Run()
    {
        assertions = 0;
        KindsCycleInOrder();
        StandCellPrefersTheView();
        EachKindHasItsOwnNeighbour();
        FlavourIsWhatTheNeighbourKeeps();
        FlavoursDoNotOverlap();
        TintsAreDistinctSubtleAndDeterministic();
        StandCellIsLandInTheStretchNearItsMiddle();
        GardenMapViewLineCountUnchanged();
        TintAndFlavourAreDrawnInTheReachLoopAndOnThePlank();
        NeighbourStandsInTheShownStretch();
        TourSavesTheShotsAndLogsEachStretch();
        RegisteredInTheSuite();
        return assertions;
    }

    // A stretch's character follows its name (RiverNames): mills, fords and falls are mills; pools and runs
    // of birds and otters are heron pools; the rest are water meadows. Later laps reuse the names.
    private static void KindsCycleInOrder()
    {
        Expect(StretchCharacter.NamedKinds == 20, "one kind for each of the twenty river names");
        for (int reach = 0; reach < StretchCharacter.NamedKinds; reach++)
        {
            string name = RiverNames.For(reach);
            StretchKind kind = StretchCharacter.KindOf(reach);
            if (name.Contains("Mill") || name.Contains("Ford") || name.Contains("Falls"))
                Expect(kind == StretchKind.Mill, name + " is a mill stretch");
            if (name.Contains("Pool") || name.Contains("Heron") || name.Contains("Kingfisher") || name.Contains("Swan") || name.Contains("Otter"))
                Expect(kind == StretchKind.HeronPool, name + " is a heron pool stretch");
            if (name.Contains("Meadow") || name.Contains("Bank") || name.Contains("Beds"))
                Expect(kind == StretchKind.WaterMeadow, name + " is a water meadow stretch");
        }
        for (int reach = 0; reach < 60; reach++)
            Expect(StretchCharacter.KindOf(reach + StretchCharacter.NamedKinds) == StretchCharacter.KindOf(reach),
                "the kinds follow the names round on later laps at " + reach);
        Expect(StretchCharacter.KindOf(0) == StretchKind.WaterMeadow, "home's stretch is the water meadow");
        Expect(StretchCharacter.KindOf(-4) == StretchKind.WaterMeadow, "a negative reach keeps to the first kind");
        // The tour's three-stretch view (stretches 1 to 3) shows all three characters side by side.
        var three = new System.Collections.Generic.HashSet<StretchKind> { StretchCharacter.KindOf(1), StretchCharacter.KindOf(2), StretchCharacter.KindOf(3) };
        Expect(three.Count == 3, "stretches 1, 2 and 3 are three different characters");
        int[] counts = new int[3];
        for (int reach = 0; reach < StretchCharacter.NamedKinds; reach++) counts[(int)StretchCharacter.KindOf(reach)]++;
        Expect(counts[0] > 0 && counts[1] > 0 && counts[2] > 0, "every character appears along the river");
    }

    // The neighbour stands nearest the middle of the view, within the stretch, when that row is in the stretch.
    private static void StandCellPrefersTheView()
    {
        GardenMap map = GardenMap.River(10, 15, CompanionFactsTests.Mask(), 4);
        int mid = StretchCharacter.StandCell(map, 2, null);
        int low = StretchCharacter.StandCell(map, 2, null, 2 * map.ReachRows + map.ReachRows - 2);
        Expect(mid >= 0 && low >= 0, "both find land");
        Expect(low / map.Columns > mid / map.Columns, "asked for a lower row, the neighbour stands lower");
        int outside = StretchCharacter.StandCell(map, 2, null, 0);
        Expect(outside == mid, "a row outside the stretch falls back to its middle");
    }

    private static void EachKindHasItsOwnNeighbour()
    {
        Expect(StretchCharacter.Neighbour(StretchKind.WaterMeadow) == 0, "the water meadow is Mira's (0)");
        Expect(StretchCharacter.Neighbour(StretchKind.HeronPool) == 1, "the heron pool is Orin's (1)");
        Expect(StretchCharacter.Neighbour(StretchKind.Mill) == 2, "the mill is Fen's (2)");
        var seen = new HashSet<int>();
        foreach (StretchKind kind in Enum.GetValues(typeof(StretchKind))) seen.Add(StretchCharacter.Neighbour(kind));
        Expect(seen.Count == 3, "each of the three stretch kinds has a different neighbour");
    }

    private static void FlavourIsWhatTheNeighbourKeeps()
    {
        foreach (StretchKind kind in Enum.GetValues(typeof(StretchKind)))
        {
            PuzzleKind[] flavour = StretchCharacter.Flavour(kind);
            PuzzleKind[] kept = NeighbourNotice.KeptBy(StretchCharacter.Neighbour(kind));
            Expect(flavour.Length > 0, kind + " has puzzle kinds to show");
            Expect(string.Join(",", flavour) == string.Join(",", kept), kind + " shows exactly the kinds its neighbour keeps, in order");
            Expect(!ReferenceEquals(flavour, StretchCharacter.Flavour(kind)), "the flavour is a copy, so callers cannot change the rule");
        }
    }

    private static void FlavoursDoNotOverlap()
    {
        var all = new HashSet<PuzzleKind>();
        foreach (StretchKind kind in Enum.GetValues(typeof(StretchKind)))
            foreach (PuzzleKind puzzle in StretchCharacter.Flavour(kind))
                Expect(all.Add(puzzle), puzzle + " belongs to one stretch's flavour only");
    }

    private static void TintsAreDistinctSubtleAndDeterministic()
    {
        var tints = new Dictionary<StretchKind, StretchTint>();
        foreach (StretchKind kind in Enum.GetValues(typeof(StretchKind)))
        {
            StretchTint tint = StretchCharacter.Tint(kind);
            StretchTint again = StretchCharacter.Tint(kind);
            Expect(tint.R == again.R && tint.G == again.G && tint.B == again.B && tint.A == again.A, kind + " tint is deterministic");
            Expect(tint.A >= 0.08f && tint.A <= 0.22f, kind + " tint is subtle (alpha 0.08 to 0.22 so the painting still reads)");
            Expect(new[] { tint.R, tint.G, tint.B, tint.A }.All(v => v >= 0f && v <= 1f), kind + " tint channels are 0 to 1");
            tints[kind] = tint;
        }
        StretchTint meadow = tints[StretchKind.WaterMeadow], mill = tints[StretchKind.Mill], pool = tints[StretchKind.HeronPool];
        Expect(meadow.G > meadow.R && meadow.G > meadow.B, "the meadow is a green-gold");
        Expect(mill.R > mill.G && mill.G > mill.B, "the mill is a warm wheat ochre");
        Expect(pool.B > pool.R && pool.B > pool.G, "the heron pool is a cool blue-grey");
        Expect(Distance(meadow, mill) > 0.3f && Distance(mill, pool) > 0.3f && Distance(meadow, pool) > 0.3f,
            "the three tints are clearly different from one another");
    }

    private static float Distance(StretchTint a, StretchTint b)
    {
        float dr = a.R - b.R, dg = a.G - b.G, db = a.B - b.B;
        return (float)Math.Sqrt(dr * dr + dg * dg + db * db);
    }

    private static void StandCellIsLandInTheStretchNearItsMiddle()
    {
        // Three stretches of 4 rows and 10 columns, all land: stretch 1 is rows 4 to 7, its middle row 6, middle column 5.
        GardenMap map = GardenMap.River(10, 4, new string(GardenMap.Free, 40), 3);
        Expect(StretchCharacter.StandCell(map, 1, null) == 65, "the neighbour stands at the middle of the stretch (row 6, column 5)");
        Expect(StretchCharacter.StandCell(map, 1, c => c == 65) == 64, "when the middle is taken, the next nearest free land is chosen");
        int cell = StretchCharacter.StandCell(map, 2, null);
        Expect(cell / 10 >= 8 && cell / 10 <= 11, "a stretch's neighbour stands within that stretch's own rows");
        Expect(StretchCharacter.StandCell(map, 3, null) == -1, "a stretch that is not on the river has no cell");
        Expect(StretchCharacter.StandCell(null, 0, null) == -1, "no map, no cell");
        Expect(StretchCharacter.StandCell(map, 1, c => true) == -1, "a stretch with no free land has no cell");
    }

    // The map view keeps its exact line count: the checklist 42 calls were appended to existing lines.
    private static void GardenMapViewLineCountUnchanged()
    {
        string view = ReadEvergrove("Assets/Scripts/Unity/GardenMapView.cs");
        Expect(view.Count(c => c == '\n') == 2362, "GardenMapView.cs stays at 2362 lines (a file-size cap)");
    }

    private static void TintAndFlavourAreDrawnInTheReachLoopAndOnThePlank()
    {
        string view = ReadEvergrove("Assets/Scripts/Unity/GardenMapView.cs");
        string stretch = ReadEvergrove("Assets/Scripts/Unity/GardenMapView.Stretch.cs");
        Expect(view.Contains("copyRect.offsetMin = copyRect.offsetMax = Vector2.zero; PaintStretch(copyRect, reach);"),
            "each reach's painting is tinted by its kind inside the reach loop of Build");
        Expect(view.Contains("if (reaches == 1) { ground.sprite = plot; PaintStretch(plotRect, 0); }"), "a single stretch is tinted as home's");
        Expect(view.Contains("label.text = RiverNames.For(reach); FlavourMarks(signRect, reach, signW);"),
            "the name plank carries the stretch's flavour marks");
        Expect(stretch.Contains("StretchCharacter.Tint(") && stretch.Contains("StretchCharacter.Flavour("),
            "the tint and flavour come from StretchCharacter");
        Expect(stretch.Contains("KindBadge(disc.transform, kinds[i], box * 0.62f)"), "the flavour marks are the play mark glyphs");
        Expect(stretch.Contains("nodes.Add(rowRect)"), "the flavour row is cleared with the other map nodes");
        Expect(stretch.Contains("internal static int StretchShown") && view.Contains("StretchShown = StretchInView(reachHeight);"),
            "the stretch the view shows is recorded before the neighbours are placed");
    }

    private static void NeighbourStandsInTheShownStretch()
    {
        string neighbours = ReadEvergrove("Assets/Scripts/Unity/EvergroveApp.Neighbours.cs");
        Expect(neighbours.Contains("StretchCharacter.Neighbour(StretchCharacter.KindOf(shownStretch)) == villager"),
            "the neighbour of the shown stretch is the one placed in it");
        Expect(neighbours.Contains("StretchCharacter.StandCell(map, shownStretch, taken.Contains, GardenMapView.ViewRow)"),
            "the neighbour stands on the stretch's free land nearest the middle of the view (StandCell with ViewRow)");
        Expect(neighbours.IndexOf("StretchCharacter.StandCell", StringComparison.Ordinal) <
               neighbours.IndexOf("// I402: in the golden hour", StringComparison.Ordinal),
            "the stretch spot comes before the golden-hour and evening spots, which keep precedence");
        Expect(neighbours.IndexOf("StretchCharacter.StandCell", StringComparison.Ordinal) <
               neighbours.IndexOf("// Never stand on another neighbour (I184)", StringComparison.Ordinal),
            "the stretch spot comes before the clash check, which still moves anyone standing on another");
    }

    private static void TourSavesTheShotsAndLogsEachStretch()
    {
        string tour = ReadEvergrove("Assets/Scripts/Unity/DevQaTour.Stretches.cs");
        string main = ReadEvergrove("Assets/Scripts/Unity/DevQaTour.cs");
        Expect(tour.StartsWith("#if UNITY_EDITOR || DEVELOPMENT_BUILD"), "the stretch tour is dev-only");
        Expect(tour.Contains("private IEnumerator StretchShots()"), "the tour has its StretchShots session");
        Expect(tour.Contains("SavePng(\"32-three-stretches.png\")") && tour.Contains("SavePng(\"32-stretch-\" + reach + \".png\")"),
            "the tour saves the three-stretch shot and each stretch's shot");
        Expect(tour.Contains("\"MOSSELORA_STRETCH \" + reach + \" \" + kind + \" neighbour \""), "the tour logs MOSSELORA_STRETCH per stretch");
        Expect(tour.Contains("app.QaLongRiver(0)") && tour.Contains("app.QaQuiet(false)") && tour.Contains("app.QaSetClock(13)"),
            "the tour puts the river length, the lookout and the clock back");
        Expect(main.Contains("yield return StretchShots(); // checklist 42 (DevQaTour.Stretches.cs)"), "the tour runs the stretch session");
        Expect(main.Split('\n').Length <= 1177, "DevQaTour.cs stays within 1177 lines");
    }

    private static void RegisteredInTheSuite()
    {
        Expect(ReadEvergrove("Tests/Program.cs").Contains("StretchCharacterTests.Run()"), "StretchCharacterTests is registered");
    }

    private static string ReadEvergrove(string relative)
    {
        string dir = AppContext.BaseDirectory;
        while (dir != null && !File.Exists(Path.Combine(dir, relative)))
            dir = Path.GetDirectoryName(dir);
        if (dir == null) throw new Exception(relative + " not found");
        return File.ReadAllText(Path.Combine(dir, relative));
    }
}
