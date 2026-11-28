using System;
using System.IO;

// Checklist 50: the map view builds only the stretches in its window, and the dev tour measures memory and
// layout time. The build is Unity code, so it is checked by source audit here; the window rule itself has
// numeric tests in StretchWindowTests.
internal static class LazyStretchTests
{
    private static int assertions;

    private static void Expect(bool ok, string message)
    {
        assertions++;
        if (!ok) throw new Exception("LazyStretchTests failed: " + message);
    }

    public static int Run()
    {
        assertions = 0;
        ViewIsPartialAndDidNotGrow();
        OrderKeepsOnlyTheWindow();
        ReachLoopsStartAndEndInTheWindow();
        CellNodesSkipFarCells();
        NightCreaturesTakeTheNearFilter();
        WindowRelaysOutOnlyOnChangeAndHoldsTheView();
        StretchWindowDrivesTheView();
        TourLogsBuiltAndLayoutTime();
        FirstBuildIsAlreadyWindowed();
        return assertions;
    }

    private const string ViewPath = "Assets/Scripts/Unity/GardenMapView.cs";
    private const string WindowPath = "Assets/Scripts/Unity/GardenMapView.Window.cs";

    // The first layout must not build every stretch: the window is preset to the top before it.
    private static void FirstBuildIsAlreadyWindowed()
    {
        string window = ReadEvergrove("Assets/Scripts/Unity/StretchWindow.cs");
        string view = ReadEvergrove("Assets/Scripts/Unity/GardenMapView.Window.cs");
        Expect(window.Contains("if (Enabled && window.view != null) window.view.PresetWindow(0, StretchWindowRule.Margin);"),
            "attaching the window presets it to the top stretches, before the first layout");
        Expect(view.Contains("internal void PresetWindow(int first, int last)"), "the preset stores the window without laying out");
    }

    private static void ViewIsPartialAndDidNotGrow()
    {
        string view = ReadEvergrove(ViewPath);
        Expect(view.Contains("internal sealed partial class GardenMapView : MonoBehaviour"), "the view is partial");
        Expect(CountLines(view) == 2362, "GardenMapView.cs stays at 2362 lines (file-size allowlist), got " + CountLines(view));
        Expect(ReadEvergrove(WindowPath).Contains("internal sealed partial class GardenMapView"), "the window lives in its own partial file");
    }

    private static void OrderKeepsOnlyTheWindow()
    {
        string view = ReadEvergrove(ViewPath);
        Expect(view.Contains("order.RemoveAll(placed => !NearCell(placed.Cell) && !placed.Newest && placed.Index != inHand);"),
            "items outside the window are not built; the newest and the one in hand always are, for the focus");
        Expect(view.Contains("foreach (Placed item in order)"), "the item loop builds from the windowed order");
    }

    private static void ReachLoopsStartAndEndInTheWindow()
    {
        string view = ReadEvergrove(ViewPath);
        Expect(view.Contains("for (int reach = FirstReach(); LampLit && reach < EndReach(reaches); reach++)"), "lamplight loops over the window");
        Expect(view.Contains("for (int reach = FirstReach(); SeasonLook.SnowCaps(SeasonNow) && reach < EndReach(reaches); reach++)"), "snow caps loop over the window");
        Expect(view.Contains("for (int reach = FirstReach(); plank != null && reach < EndReach(reaches); reach++)"), "river name planks loop over the window");
        Expect(view.Contains("if (SmokeNow && NearReach(0))") && view.Contains("for (int l = 0; l < Mathf.Min(lanterns, 3) && NearReach(0); l++)"),
            "the chimney and the lanterns stand on the top stretch and are built only there");
    }

    private static void CellNodesSkipFarCells()
    {
        string view = ReadEvergrove(ViewPath);
        Expect(view.Contains("if (seen++ % mistStride != 0 || !NearCell(c)) continue;"),
            "mist counts over the whole river (same spacing) but builds near the window");
        Expect(view.Contains("if (!NearCell(c)) { drawn++; continue; }"), "river marks keep the same first 70 cells, built near the window");
        Expect(view.Contains("int cell = tuftCells[t]; if (!NearCell(cell)) continue;"), "wild flowers skip far cells");
        Expect(view.Contains("if (!NearCell(a) && !NearCell(b)) continue;"), "sprigs between neighbours skip pairs out of view");
        Expect(view.Contains("if (seatLantern >= 0 && seatLantern < map.CellCount && NearCell(seatLantern))"), "the shady seat's lantern skips when far");
        Expect(view.Contains("int cell = creatureCells[k]; if (!NearCell(cell)) continue;"), "visitor creatures skip far cells");
        Expect(view.Contains("int cell = pairSpots[sp]; if (!NearCell(cell)) continue;"), "pairing glows skip far cells");
        Expect(view.Contains("if (!map.IsLand(cell)) continue; if (!NearCell(cell)) { rng.NextDouble(); rng.NextDouble(); continue; }"),
            "dew glints skip far cells but draw the same random numbers, so the near ones stay put");
        Expect(view.Contains("GardenNightCreatures.Place(land, nodes, map, cellW, cellH, nightCells, NearCell);"), "night creatures get the near filter");
    }

    private static void NightCreaturesTakeTheNearFilter()
    {
        string night = ReadEvergrove("Assets/Scripts/Unity/GardenNightCreatures.cs");
        Expect(night.Contains("int[] cells, System.Func<int, bool> near = null)"), "Place takes an optional near filter");
        Expect(night.Contains("(near != null && !near(cell))) continue;"), "Place skips far cells and keeps every other one");
    }

    private static void WindowRelaysOutOnlyOnChangeAndHoldsTheView()
    {
        string window = ReadEvergrove(WindowPath);
        Expect(window.Contains("if (first == windowFirst && last == windowLast) return;"), "SetWindow relays out only when the range changes");
        Expect(window.Contains("keep = new Vector2(Math.Max(0f, Position.x), Position.y);"), "a window change holds the view where it is");
        Expect(window.Contains("internal void SetWindow(int first, int last)"), "the window is set from outside");
        Expect(window.Contains("windowLast = int.MaxValue;"), "every stretch is built until a window is set");
        string view = ReadEvergrove(ViewPath);
        Expect(view.Contains("if (!windowing) scroll.StopMovement(); // checklist 50: a window change keeps a fling going"),
            "a window change does not stop a fling");
        Expect(window.Contains("private bool NearReach(int reach) { return reach >= windowFirst && reach <= windowLast; }"),
            "a stretch is near when it lies between the window's first and last");
        Expect(window.Contains("return reach >= windowFirst") && window.Contains("windowLast >= reaches ? reaches : windowLast + 1"),
            "the end of the window never overflows, even when every stretch is built");
    }

    private static void StretchWindowDrivesTheView()
    {
        string window = ReadEvergrove("Assets/Scripts/Unity/StretchWindow.cs");
        Expect(window.Contains("window.view = viewport.GetComponent<GardenMapView>();"), "the window finds its map view");
        Expect(window.Contains("if (view != null) view.SetWindow(first, last);"), "the stretch window sets the map's window as the view scrolls");
        Expect(window.Contains("int first = 0, last = int.MaxValue;") && window.Contains("if (Enabled) StretchWindowRule.Range("),
            "with the window off every stretch is built and shown");
        Expect(window.Contains("if (first == lastFirst && last == lastLast && painting.childCount == lastPaintings && Enabled == lastEnabled) return;"),
            "work is done only when the range or the painting's children change, not every frame");
    }

    private static void TourLogsBuiltAndLayoutTime()
    {
        string tour = ReadEvergrove("Assets/Scripts/Unity/DevQaTour.Window.cs");
        Expect(tour.Contains("\" built \" + app.QaHubBuilt()"), "each step logs how many things were built");
        Expect(tour.Contains("\" layoutMs \""), "the hub rebuild time is logged once per pass");
        Expect(tour.Contains("var clock = System.Diagnostics.Stopwatch.StartNew();") && tour.Contains("clock.Stop();"),
            "the rebuild is timed around the hub rebuild");
        string hooks = ReadEvergrove("Assets/Scripts/Unity/EvergroveApp.WindowQa.cs");
        Expect(hooks.Contains("internal int QaHubBuilt()"), "the app exposes the built count to the tour");
    }

    private static int CountLines(string text)
    {
        int lines = 0;
        foreach (char ch in text) if (ch == '\n') lines++;
        return lines;
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
