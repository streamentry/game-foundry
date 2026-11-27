using System;
using System.IO;
using System.Text.RegularExpressions;
using Evergrove.Core;

// Checklist 43: travel by boat. Any open stretch of the river is two taps away, only open stretches
// are stops, and the Unity side (menu entry, sheet, glide, reduced motion, tour) is checked by source audit.
internal static class RiverTravelTests
{
    private static int assertions;

    private static void Expect(bool ok, string message)
    {
        assertions++;
        if (!ok) throw new Exception("RiverTravelTests failed: " + message);
    }

    public static int Run()
    {
        assertions = 0;
        StopsAreTheOpenStretches();
        EveryStopIsTwoTaps();
        BoatIsInTheCatalogue();
        MenuEntryNeedsTwoStretches();
        MenuHookExists();
        SheetUsesTheSheetPattern();
        ReducedMotionJumps();
        GlideIsShortAndUnscaled();
        TourSavesTheShots();
        SheetEdgeKnowsTheSheet();
        return assertions;
    }

    // Stops(n) lists exactly reaches 0..n-1 with their names: never the misted stretch n, for 1..200 open reaches.
    private static void StopsAreTheOpenStretches()
    {
        Expect(RiverTravel.Stops(0).Count == 0, "no stops before any stretch is open");
        for (int open = 1; open <= 200; open++)
        {
            var stops = RiverTravel.Stops(open);
            Expect(stops.Count == open, "one stop per open stretch at " + open);
            for (int reach = 0; reach < open; reach++)
            {
                Expect(stops[reach].Reach == reach, "stop " + reach + " is that stretch at " + open);
                Expect(stops[reach].Name == RiverNames.For(reach), "stop " + reach + " carries its name at " + open);
            }
            Expect(stops[open - 1].Reach < open, "no stop reaches into the mist at " + open);
        }
        Expect(RiverTravel.Stops(3)[0].Name == "Cottage Reach", "home's stretch is the cottage reach");
    }

    private static void EveryStopIsTwoTaps()
    {
        for (int reach = 0; reach < 200; reach++)
        {
            string[] steps = RiverTravel.Steps(reach);
            Expect(RiverTravel.TapsTo(reach) == 2, "two taps to stretch " + reach);
            Expect(steps.Length == 2 && steps[0] == RiverTravel.MenuLabel && steps[1] == RiverNames.For(reach),
                "menu entry then the stretch's own button for " + reach);
        }
        bool threw = false;
        try { RiverTravel.Steps(-1); } catch (ArgumentOutOfRangeException) { threw = true; }
        Expect(threw, "negative stretches are refused");
    }

    private static void BoatIsInTheCatalogue()
    {
        int index = RiverTravel.BoatItemIndex();
        Expect(index >= 0, "the rowing boat is a catalogue item");
        Expect(GardenCatalog.Get(VillageChoice.Bridge, index).Name == RiverTravel.BoatName, "the boat's name is found");
    }

    private static void MenuEntryNeedsTwoStretches()
    {
        Expect(!RiverTravel.CanTravel(0) && !RiverTravel.CanTravel(1), "one stretch is no journey");
        Expect(RiverTravel.CanTravel(2) && RiverTravel.CanTravel(200), "two or more stretches offer the boat");
    }

    private static void MenuHookExists()
    {
        string menu = UnityAppSource.ReadAll();
        Expect(Regex.IsMatch(menu, @"BuildStallEntries\(\);[^\n]*\n\s*BuildTravelEntry\(\);"), "the menu build adds the travel entry after the stall");
        string boat = ReadEvergrove("Assets/Scripts/Unity/EvergroveApp.Boat.cs");
        Expect(boat.Contains("RiverTravel.MenuLabel") && boat.Contains("RiverTravel.CanTravel(OpenReaches())"),
            "the entry is labelled and offered only with two or more open stretches");
        Expect(boat.Contains("world.Mode != PlayMode.Living"), "travel is for living gardens only");
        Expect(boat.Contains("internal void QaOpenTravel()") && boat.Contains("internal void QaTravel(int reach)"),
            "the tour can open the sheet and travel");
    }

    private static void SheetUsesTheSheetPattern()
    {
        string boat = ReadEvergrove("Assets/Scripts/Unity/EvergroveApp.Boat.cs");
        Expect(boat.Contains("BuildSheet(titleScreen.transform, \"Travel by boat\""), "the sheet is built with BuildSheet");
        Expect(boat.Contains("SheetMotion.Show(travelSheet, SheetEdge.For(\"Travel by boat\"), ReducedMotion)"),
            "the sheet opens through SheetMotion from its edge");
        Expect(boat.Contains("SheetClose(content, \"Close travel\", CloseTravelSheet)"), "the close button is placed with SheetClose");
        Expect(boat.Contains("RetireSheet(travelSheet, \"Travel by boat\", replacing)"), "the sheet leaves through RetireSheet");
        Expect(Regex.IsMatch(boat, @"Button\(content, ""Stretch ""[^;]*\(\) => TravelTo\(reach\)"), "each stretch is a button of at least 48 pt");
        Expect(boat.Contains("MosseloraArt.StateMark(row.transform, current, 18f)"), "the current stretch wears a check, not only a colour");
    }

    private static void ReducedMotionJumps()
    {
        string boat = ReadEvergrove("Assets/Scripts/Unity/EvergroveApp.Boat.cs");
        Regex jump = new Regex(@"if \(!gentleMotion\)\s*\{[^}]*KeepAt\([^}]*return;\s*\}", RegexOptions.Singleline);
        Expect(jump.IsMatch(boat), "reduced motion jumps straight to the stretch");
        Expect(boat.IndexOf("if (!gentleMotion)", StringComparison.Ordinal) < boat.IndexOf("StartCoroutine(GlideTo(", StringComparison.Ordinal),
            "the jump is decided before any glide starts");
    }

    private static void GlideIsShortAndUnscaled()
    {
        Expect(RiverTravel.GlideSeconds > 0f && RiverTravel.GlideSeconds <= 1.2f, "the glide is at most 1.2 seconds");
        string boat = ReadEvergrove("Assets/Scripts/Unity/EvergroveApp.Boat.cs");
        Expect(boat.Contains("Time.unscaledDeltaTime") && !boat.Contains("Time.deltaTime"), "the glide runs on unscaled time");
        Expect(!boat.Contains("localScale"), "the glide moves transforms only, never scale");
    }

    private static void TourSavesTheShots()
    {
        string tour = ReadEvergrove("Assets/Scripts/Unity/DevQaTour.cs") + ReadEvergrove("Assets/Scripts/Unity/DevQaTour.Travel.cs"); // the boat and map steps live in the tour partial
        int river = tour.IndexOf("SavePng(\"17x-river-opened.png\")", StringComparison.Ordinal);
        int zc = tour.IndexOf("SavePng(\"17zc-boat-travel.png\")", StringComparison.Ordinal);
        int zd = tour.IndexOf("SavePng(\"17zd-boat-arrived.png\")", StringComparison.Ordinal);
        Expect(river >= 0 && zc > river && zd > zc, "the boat shots follow the river shots");
        Expect(tour.Contains("app.QaOpenTravel();") && tour.Contains("app.QaTravel(app.QaOpenReaches() - 1);"),
            "the tour opens the sheet and travels to the furthest open stretch");
    }

    private static void SheetEdgeKnowsTheSheet()
    {
        Expect(SheetEdge.For("Travel by boat") == SheetEdge.Side.Bottom, "the travel sheet rises from the bottom");
        string program = ReadEvergrove("Tests/Program.cs");
        Expect(program.Contains("assertions += KitchenTests.Run();\n") && program.Contains("assertions += RiverTravelTests.Run();"),
            "the travel tests are registered in the runner");
    }

    // Reads a file under games/evergrove, found from the working or base directory.
    private static string ReadEvergrove(string relative)
    {
        foreach (string start in new[] { Directory.GetCurrentDirectory(), AppContext.BaseDirectory })
        {
            for (var dir = new DirectoryInfo(start); dir != null; dir = dir.Parent)
            {
                string nested = Path.Combine(dir.FullName, "games", "evergrove", relative);
                if (File.Exists(nested)) return File.ReadAllText(nested);
                string direct = Path.Combine(dir.FullName, relative);
                if (File.Exists(direct)) return File.ReadAllText(direct);
            }
        }
        throw new Exception("Could not find " + relative + " from " + Directory.GetCurrentDirectory());
    }
}
