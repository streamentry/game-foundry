using System;
using System.IO;
using System.Text.RegularExpressions;
using Evergrove.Core;

// Checklist 45: the river map. A painted scroll of the open stretches, one tap from the menu. Core rows
// are tested headless; the Unity sheet (menu hook, sheet pattern, travel tap, family gate, back order and
// the tour's screenshot) is checked by source audit, since UnityEngine is not part of the headless build.
// The class is named RiverMapSheetTests because Program.cs already has a private RiverMapTests() method.
internal static class RiverMapSheetTests
{
    private static int assertions;

    private static void Expect(bool ok, string message)
    {
        assertions++;
        if (!ok) throw new Exception("RiverMapSheetTests failed: " + message);
    }

    public static int Run()
    {
        assertions = 0;
        RowsListExactlyTheOpenStretches();
        RowsMarkTheCurrentStretchOnly();
        FamilyNamesAreOptionalDedupedAndCapped();
        FamilyNamesSkipNeighboursAndBlanks();
        MenuHookSitsAfterTravel();
        SheetUsesTheSheetPattern();
        SheetEdgeKnowsTheSheet();
        TappingAStretchTravels();
        FamilyPinsAreGated();
        BackGestureClosesTheMap();
        TourSavesTheShot();
        RegisteredInTheRunner();
        return assertions;
    }

    private static void RowsListExactlyTheOpenStretches()
    {
        Expect(RiverMap.Rows(0, 0, null).Count == 0, "no stretches before any is open");
        for (int open = 1; open <= 200; open++)
        {
            var rows = RiverMap.Rows(open, open - 1, null);
            Expect(rows.Count == open, "one row per open stretch at " + open);
            for (int reach = 0; reach < open; reach++)
            {
                Expect(rows[reach].Kind == RiverMapKind.Stretch && rows[reach].Reach == reach,
                    "row " + reach + " is that stretch at " + open);
                Expect(rows[reach].Name == RiverNames.For(reach), "row " + reach + " carries its name at " + open);
            }
            Expect(rows[open - 1].Reach < open, "no row reaches into the mist at " + open);
        }
        Expect(RiverMap.Rows(3, 0, null)[0].Name == "Cottage Reach", "home's stretch is the cottage reach");
        Expect(RiverMap.Rows(200, 0, null).Count == 200, "200 open stretches are all listed");
    }

    private static void RowsMarkTheCurrentStretchOnly()
    {
        for (int open = 1; open <= 200; open++)
        {
            for (int current = 0; current < open; current++)
            {
                var rows = RiverMap.Rows(open, current, null);
                int marked = 0;
                foreach (RiverMapRow row in rows)
                {
                    if (row.Current) marked++;
                    Expect(row.Current == (row.Reach == current), "only stretch " + current + " is current at " + open);
                }
                Expect(marked == 1, "exactly one current stretch at " + open + "/" + current);
            }
        }
        foreach (int none in new[] { -1, 5, 200 })
        {
            foreach (RiverMapRow row in RiverMap.Rows(3, none, null))
                Expect(!row.Current, "no stretch is current for index " + none);
        }
    }

    private static void FamilyNamesAreOptionalDedupedAndCapped()
    {
        foreach (RiverMapRow row in RiverMap.Rows(4, 0, null))
            Expect(row.Kind == RiverMapKind.Stretch, "no family lines when family is not given");
        var names = new[] { "Anh", " Anh ", "", null, "Bea", "Bea", "Cal" };
        var rows = RiverMap.Rows(2, 1, names);
        Expect(rows.Count == 2 + 3, "family names are deduplicated, trimmed and blanks dropped");
        Expect(rows[2].Kind == RiverMapKind.Family && rows[2].Name == "Anh" && rows[2].Reach == -1 && !rows[2].Current,
            "family line carries the trimmed name, no stretch, not current");
        Expect(rows[3].Name == "Bea" && rows[4].Name == "Cal", "family names keep their order");

        var many = new string[40];
        for (int i = 0; i < many.Length; i++) many[i] = "Relative " + i;
        for (int open = 1; open <= 200; open++)
        {
            var capped = RiverMap.Rows(open, 0, many);
            int family = 0;
            foreach (RiverMapRow row in capped)
                if (row.Kind == RiverMapKind.Family) family++;
            Expect(family == RiverMap.MaxFamilyNames && RiverMap.MaxFamilyNames == 12, "family names are capped at 12 at " + open);
            Expect(capped.Count == open + 12, "stretch rows stay exact beside the family at " + open);
        }
    }

    private static void FamilyNamesSkipNeighboursAndBlanks()
    {
        string[] letters = { "Mira", "Dad", "" };
        string[] helped = { "Orin", "Nan" };
        string[] lanterns = { null, "Fen", "Dad", "Sam" };
        string[] found = RiverMap.FamilyNamesFrom(letters, helped, lanterns);
        Expect(found.Length == 4 && found[0] == "Dad" && found[1] == "Nan" && found[2] == "Dad" && found[3] == "Sam",
            "neighbours and blanks are left out; order kept (dedupe happens in Rows)");
        Expect(RiverMap.FamilyNamesFrom(null, null, null).Length == 0, "nothing known, nothing listed");
        Expect(RiverMap.Rows(1, 0, found).Count == 1 + 3, "dedupe leaves Dad once");
    }

    private static void MenuHookSitsAfterTravel()
    {
        string menu = ReadEvergrove("Assets/Scripts/Unity/EvergroveApp.Rebuild.cs");
        Expect(Regex.IsMatch(menu, @"BuildTravelEntry\(\);[^\n]*\n\s*BuildRiverMapEntry\(\);"),
            "the menu hook is right after BuildTravelEntry");
        string sheet = ReadEvergrove("Assets/Scripts/Unity/EvergroveApp.RiverMap.cs");
        Expect(sheet.Contains("RiverMap.MenuLabel") && sheet.Contains("world.Mode != PlayMode.Living"),
            "the entry is labelled and for living gardens only");
        Expect(sheet.Contains("internal void OpenRiverMap()"), "the sheet opens from the menu entry");
    }

    private static void SheetUsesTheSheetPattern()
    {
        string sheet = ReadEvergrove("Assets/Scripts/Unity/EvergroveApp.RiverMap.cs");
        Expect(sheet.Contains("BuildSheet(titleScreen.transform, \"River map\""), "the sheet is built with BuildSheet");
        Expect(sheet.Contains("SheetMotion.Show(riverMapSheet, SheetEdge.For(\"River map\"), ReducedMotion)"),
            "the sheet opens through SheetMotion from its edge");
        Expect(sheet.Contains("SheetClose(content, \"Close river map\", CloseRiverMap)"), "the close button is placed with SheetClose");
        Expect(sheet.Contains("RetireSheet(riverMapSheet, \"River map\", replacing)"), "the sheet leaves through RetireSheet");
        Expect(sheet.Contains("MosseloraArt.StateMark(stretch.transform, row.Current, 18f)"), "the current stretch wears a check");
        Expect(sheet.Contains("\", you are here\""), "the current stretch says so in words, not only by colour");
        Expect(sheet.Contains("PaintedSign(content)"), "the heading rides on the existing stream-sign plank");
    }

    private static void SheetEdgeKnowsTheSheet()
    {
        Expect(SheetEdge.For("River map") == SheetEdge.Side.Bottom, "the river map rises from the bottom");
    }

    private static void TappingAStretchTravels()
    {
        string sheet = ReadEvergrove("Assets/Scripts/Unity/EvergroveApp.RiverMap.cs");
        Expect(Regex.IsMatch(sheet, @"Button\(line\.transform, ""Stretch ""[\s\S]*?TravelTo\(reach\);"),
            "tapping a stretch calls TravelTo");
        Expect(sheet.Contains("RetireRiverMapSheet(false);\n                TravelTo(reach);"),
            "the map closes before the boat travels");
    }

    private static void FamilyPinsAreGated()
    {
        string sheet = ReadEvergrove("Assets/Scripts/Unity/EvergroveApp.RiverMap.cs");
        Expect(sheet.Contains("FamilyShown() ? RiverMap.FamilyNamesFrom(world.LetterFrom, world.HelpedFrom, world.Lanterns) : null"),
            "family names come from this device and only when family is shown");
        Expect(sheet.Contains("where they are is not known") || sheet.Contains("Where they are is not known"),
            "the family line says honestly that their stretches are not known");
    }

    private static void BackGestureClosesTheMap()
    {
        string menu = ReadEvergrove("Assets/Scripts/Unity/EvergroveApp.Menu.cs");
        int travel = menu.IndexOf("CloseTravelSheet();", StringComparison.Ordinal);
        int river = menu.IndexOf("CloseRiverMap();", StringComparison.Ordinal);
        int setMenu = menu.IndexOf("SetMenu(false);", travel, StringComparison.Ordinal);
        Expect(travel >= 0 && river > travel && setMenu > river, "the back gesture closes the river map above the menu");
    }

    private static void TourSavesTheShot()
    {
        string tour = ReadEvergrove("Assets/Scripts/Unity/DevQaTour.cs") + ReadEvergrove("Assets/Scripts/Unity/DevQaTour.Travel.cs"); // the boat and map steps live in the tour partial
        int boat = tour.IndexOf("SavePng(\"17zd-boat-arrived.png\")", StringComparison.Ordinal);
        int shot = tour.IndexOf("SavePng(\"17ze-river-map.png\")", StringComparison.Ordinal);
        Expect(boat >= 0 && shot > boat, "the river map shot follows the boat shots");
        Expect(tour.Contains("app.QaOpenRiverMap();") && tour.Contains("Press(\"Close river map\");"),
            "the tour opens the map over the hub and closes it with its own button");
        string sheet = ReadEvergrove("Assets/Scripts/Unity/EvergroveApp.RiverMap.cs");
        Expect(sheet.Contains("internal void QaOpenRiverMap()") && sheet.Contains("OpenRiverMap();"),
            "the QA hook opens the map through the same OpenRiverMap the menu entry calls");
    }

    private static void RegisteredInTheRunner()
    {
        string program = ReadEvergrove("Tests/Program.cs");
        int kitchen = program.IndexOf("assertions += KitchenTests.Run();", StringComparison.Ordinal);
        int river = program.IndexOf("assertions += RiverMapSheetTests.Run();", StringComparison.Ordinal);
        Expect(kitchen >= 0 && river > kitchen, "the river map tests are registered after the kitchen tests");
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
