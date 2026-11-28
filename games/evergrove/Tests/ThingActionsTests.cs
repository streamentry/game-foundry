using System;
using System.IO;
using System.Text.RegularExpressions;
using Evergrove.Core;

// Checklist 13: a tapped thing shows its two actions right beside it. The tap counts (ThingTaps) are pure;
// the placement, tokens, hub wiring and tour are checked by source audit.
internal static class ThingActionsTests
{
    private static int assertions;

    private static void Expect(bool ok, string message)
    {
        assertions++;
        if (!ok) throw new Exception("ThingActionsTests failed: " + message);
    }

    public static int Run()
    {
        assertions = 0;
        TapCountsAreNoWorse();
        PlayAndMoveTapCounts();
        MoveTokenIsOptionalGuidance();
        TokensSitBesideTheThing();
        MapAsksForTheActionsOnlyOnTheHub();
        HubDropsTheBottomPlayToken();
        TourLogsDistanceAndShots();
        RegisteredInTheSuite();
        return assertions;
    }

    private static void TapCountsAreNoWorse()
    {
        foreach (ThingAction action in new[] { ThingAction.Play, ThingAction.Move })
            Expect(ThingTaps.After(action) <= ThingTaps.Before(action), "after is never more taps than before: " + action);
    }

    private static void PlayAndMoveTapCounts()
    {
        Expect(ThingTaps.Before(ThingAction.Play) == 2, "before, play is thing then bottom token (2)");
        Expect(ThingTaps.After(ThingAction.Play) == 2, "after, play is thing then the token beside it (2)");
        Expect(ThingTaps.Before(ThingAction.Move) == 2, "before, move is thing then free land (2)");
        Expect(ThingTaps.After(ThingAction.Move) == 2, "after, move is thing then free land (2)");
    }

    private static void MoveTokenIsOptionalGuidance()
    {
        Expect(ThingTaps.AfterWithMoveToken(ThingAction.Move) == 3, "using the Move token adds one tap (3)");
        Expect(ThingTaps.AfterWithMoveToken(ThingAction.Play) == ThingTaps.After(ThingAction.Play),
            "the Move token does not change play");
    }

    private static void TokensSitBesideTheThing()
    {
        string app = ReadEvergrove("Assets/Scripts/Unity/EvergroveApp.ThingActions.cs");
        Expect(app.Contains("Mathf.Clamp(x, 4f"), "the row is kept inside the land");
        Expect(app.Contains("onLeft = cell % map.Columns >= map.Columns - 2"), "rightmost two columns put the row on the left");
        Expect(app.Contains("middle.x + beside"), "otherwise the row sits to the right of the thing");
        Expect(app.Contains("\"Thing actions\""), "the row is named Thing actions");
        Expect(app.Contains("heldName = held >= 0"), "the Play token keeps the thing's name as its caption");
        Expect(app.Contains("\"Move \" + heldName"), "the Move token's caption names the thing");
    }

    private static void MapAsksForTheActionsOnlyOnTheHub()
    {
        string map = ReadEvergrove("Assets/Scripts/Unity/EvergroveApp.Map.cs");
        Expect(Regex.IsMatch(map, @"\(land,\s*point\)\s*=>\s*\{[^}]*var standing = NeighborsOnMap\(land,\s*point,\s*map,\s*true\);[^}]*ThingActionsOnMap\(land,\s*point\);[^}]*return standing;"),
            "the hub's decorate places the thing actions after the neighbours (drawn on top), and returns only the neighbours");
        // The map treats what decorate returns as people (footprints, stepping off signs): the tokens stay out
        // of that list and replace their own previous row each layout instead.
        string app = ReadEvergrove("Assets/Scripts/Unity/EvergroveApp.ThingActions.cs");
        Expect(!map.Contains("standing.AddRange(ThingActionsOnMap"), "the tokens are not counted as neighbours");
        Expect(app.Contains("Transform old = land != null ? land.Find(ThingActionsName) : null;") &&
               app.Contains("old.gameObject.SetActive(false); Destroy(old.gameObject);"),
            "each layout removes the last row before placing a new one (no pile-up)");
        Expect(app.Contains("world.Mode != PlayMode.Living") && app.Contains("held < 0 && heldPlant < 0"),
            "nothing is placed unless a thing is held in a Living garden");
        Expect(app.Contains("Tap where it should go."), "Move gives a one-line hint and keeps the thing in hand");
    }

    private static void HubDropsTheBottomPlayToken()
    {
        string hub = ReadEvergrove("Assets/Scripts/Unity/EvergroveApp.Hub.cs");
        int at = hub.IndexOf("checklist 13: a thing you've grown", StringComparison.Ordinal);
        Expect(at >= 0, "the held block is marked for checklist 13");
        int end = hub.IndexOf("titleScreen.transform.SetAsLastSibling();", at, StringComparison.Ordinal);
        Expect(end > at, "the held block still ends with the hub's chrome");
        end = hub.IndexOf("return;", end, StringComparison.Ordinal) + "return;".Length;
        string held = hub.Substring(at, end - at);
        Expect(!held.Contains("Play row") && !held.Contains("Play its puzzle") && !held.Contains("IconButton("),
            "the held block builds no bottom Play row");
        foreach (string keep in new[] { "SeedPurse();", "DayRhythmTokens(titleScreen.transform)", "BasketControls(", "HubChrome();", "MaybeAskNotes();", "FollowNote();", "return;" })
            Expect(held.Contains(keep), "the held block keeps " + keep);
    }

    private static void TourLogsDistanceAndShots()
    {
        string tour = ReadEvergrove("Assets/Scripts/Unity/DevQaTour.ThingActions.cs");
        Expect(tour.Contains("MOSSELORA_THING play-distance"), "the tour logs the Play distance");
        Expect(tour.Contains("SavePng(\"30-thing-actions.png\")") && tour.Contains("SavePng(\"30-thing-move.png\")"),
            "the tour saves both shots");
        Expect(tour.Contains("Press(\"Move it\")"), "the tour presses Move it");
        Expect(tour.Contains("app.QaPutDownThing()"), "the tour puts the thing down afterwards");
        string main = ReadEvergrove("Assets/Scripts/Unity/DevQaTour.cs");
        Expect(main.Contains("yield return ThingActionShots();"), "DevQaTour calls ThingActionShots");
        Expect(main.Split('\n').Length <= 1177, "DevQaTour.cs stays within 1177 lines");
    }

    private static void RegisteredInTheSuite()
    {
        Expect(ReadEvergrove("Tests/Program.cs").Contains("ThingActionsTests.Run()"), "ThingActionsTests is registered");
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
