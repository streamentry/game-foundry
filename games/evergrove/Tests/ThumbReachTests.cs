using System;
using System.IO;
using Evergrove.Core;

// Checklist 94, one-hand landscape and portrait: every action reachable with one thumb in both
// orientations. The reach rule is pure (ThumbReach); the menu's bottom-left placement, the upward fan and
// the reach tour are checked by source audit, and the tour itself runs in the dev build.
internal static class ThumbReachTests
{
    private static int assertions;

    private static void Expect(bool ok, string message)
    {
        assertions++;
        if (!ok) throw new Exception("ThumbReachTests failed: " + message);
    }

    public static int Run()
    {
        assertions = 0;
        PortraitLowerTwoThirdsIn();
        LandscapeLowerCornersIn();
        BadSizesAreSafe();
        RightHandDoesNotChangeTheModel();
        RuleIsPure();
        MenuSitsBottomLeft();
        FanOpensUpward();
        TourLogsReachForHubAndFan();
        TourIsCalledFromTheMainTour();
        RegisteredInTheSuite();
        return assertions;
    }

    private static void PortraitLowerTwoThirdsIn()
    {
        // 390 x 844: the top third ends at 281.3 points.
        Expect(!ThumbReach.Reachable(195f, 100f, 390f, 844f), "portrait: the top-centre is out of reach");
        Expect(!ThumbReach.Reachable(195f, 280f, 390f, 844f), "portrait: just above the lower two thirds is out");
        Expect(ThumbReach.Reachable(195f, 282f, 390f, 844f), "portrait: just inside the lower two thirds is in");
        Expect(ThumbReach.Reachable(195f, 800f, 390f, 844f), "portrait: the bottom-centre is in reach");
        Expect(ThumbReach.Reachable(30f, 800f, 390f, 844f) && ThumbReach.Reachable(360f, 800f, 390f, 844f),
            "portrait: the bottom corners are in reach");
        Expect(ThumbReach.Reachable(30f, 400f, 390f, 844f), "portrait: a left-middle target is in reach");
    }

    private static void LandscapeLowerCornersIn()
    {
        // 844 x 390: the top quarter ends at 97.5 points; the reachable sides are x <= 379.8 or x >= 464.2.
        Expect(ThumbReach.Reachable(50f, 380f, 844f, 390f), "landscape: the lower-left corner is in reach");
        Expect(ThumbReach.Reachable(800f, 380f, 844f, 390f), "landscape: the lower-right corner is in reach");
        Expect(ThumbReach.Reachable(100f, 250f, 844f, 390f), "landscape: a left-side target in the lower three quarters is in");
        Expect(!ThumbReach.Reachable(422f, 50f, 844f, 390f), "landscape: the top-centre is out of reach");
        Expect(!ThumbReach.Reachable(422f, 300f, 844f, 390f), "landscape: the bottom-centre is out (no thumb reaches it)");
        Expect(!ThumbReach.Reachable(50f, 80f, 844f, 390f), "landscape: a top-left target is out");
    }

    private static void BadSizesAreSafe()
    {
        Expect(!ThumbReach.Reachable(10f, 10f, 0f, 0f), "a zero screen reaches nothing");
        Expect(!ThumbReach.Reachable(10f, 10f, -390f, 844f), "a negative width reaches nothing");
        Expect(!ThumbReach.Reachable(10f, 10f, float.NaN, 844f), "a NaN width reaches nothing");
        Expect(!ThumbReach.Reachable(10f, 10f, float.PositiveInfinity, 844f), "an infinite width reaches nothing");
        Expect(!ThumbReach.Reachable(float.NaN, 500f, 390f, 844f), "a NaN centre is not reachable");
        Expect(!ThumbReach.Reachable(-5f, 500f, 390f, 844f), "a centre left of the screen is not reachable");
        Expect(!ThumbReach.Reachable(195f, 900f, 390f, 844f), "a centre below the screen is not reachable");
    }

    private static void RightHandDoesNotChangeTheModel()
    {
        foreach (float x in new[] { 30f, 195f, 360f })
            foreach (float y in new[] { 100f, 400f, 800f })
                Expect(ThumbReach.Reachable(x, y, 390f, 844f, true) == ThumbReach.Reachable(x, y, 390f, 844f, false),
                    "portrait is the same for either hand at " + x + "," + y);
        Expect(ThumbReach.Reachable(50f, 380f, 844f, 390f, false) && ThumbReach.Reachable(800f, 380f, 844f, 390f, true),
            "landscape reaches both lower corners for either hand");
    }

    private static void RuleIsPure()
    {
        string core = ReadEvergrove("Assets/Scripts/Core/ThumbReach.cs");
        Expect(core.Contains("namespace Evergrove.Core") && !core.Contains("UnityEngine"), "ThumbReach is pure: no engine types");
        Expect(core.Contains("Steven Hoober") && core.Contains("heuristic"), "the model is documented as a heuristic from Hoober's studies");
        Expect(core.Contains("public static bool Reachable(float x, float y, float width, float height, bool rightHanded = true)"),
            "Reachable keeps the agreed signature");
    }

    private static void MenuSitsBottomLeft()
    {
        string menu = ReadEvergrove("Assets/Scripts/Unity/EvergroveApp.Menu.cs");
        string hub = ReadEvergrove("Assets/Scripts/Unity/EvergroveApp.Hub.cs");
        string controls = ReadEvergrove("Assets/Scripts/Unity/EvergroveApp.Controls.cs");
        string layout = ReadEvergrove("Assets/Scripts/Unity/EvergroveApp.Layout.cs");
        Expect(menu.Contains("PlaceChrome(menu, MenuCornerAnchor, new Vector2(MenuSideOffset, MenuInset));"),
            "the game's menu button is anchored at its hand's bottom corner (bottom-left unless right-handed), inset from the safe area");
        Expect(menu.Contains("internal const float MenuInset = 14f, MenuSize = 54f;"), "the inset is 14 pt and the button 54 pt");
        Expect(hub.Contains("PlaceChrome(menu, MenuCornerAnchor, new Vector2(MenuSideOffset, MenuInset));") &&
               hub.Contains("rect.anchorMin = new Vector2(safe.xMin / w, safe.yMin / h);"),
            "the hub's menu button sits in a bar on the bottom safe edge, at the bottom-left");
        Expect(!hub.Contains("PlaceChrome(menu, new Vector2(0f, 1f)") && !menu.Contains("PlaceChrome(menu, new Vector2(0f, 1f)"),
            "no menu button is left at the top-left");
        Expect(controls.Contains("chrome.anchorMin = Vector2.zero;") && controls.Contains("chrome.anchorMax = Vector2.one;"),
            "the game chrome spans the safe area so the bottom-left button can anchor to it");
        Expect(layout.Contains("chrome.offsetMax = new Vector2(0f, -topInset);"), "the chrome keeps its top edge at the status-bar inset");
        Expect(menu.Contains("MenuReserve"), "the bottom controls keep MenuReserve clear of the button");
        Expect(ReadEvergrove("Assets/Scripts/Unity/EvergroveApp.MapActions.cs").Contains("new Vector2(-MenuReserve, basketOpen ? 104f : 76f)")
               || ReadEvergrove("Assets/Scripts/Unity/EvergroveApp.MapActions.cs").Contains("cornerRect.sizeDelta = new Vector2(-MenuReserve"),
            "the open basket tray starts right of the menu button");
        Expect(ReadEvergrove("Assets/Scripts/Unity/EvergroveApp.Chrome.cs").Contains("new Vector2(-34f - MenuReserve, rowHeight)"),
            "the pinned action row starts right of the menu button");
    }

    private static void FanOpensUpward()
    {
        string menu = ReadEvergrove("Assets/Scripts/Unity/EvergroveApp.Menu.cs");
        Expect(menu.Contains("stack.startCorner = onRight ? GridLayoutGroup.Corner.LowerRight : GridLayoutGroup.Corner.LowerLeft;") &&
               menu.Contains("stack.childAlignment = onRight ? TextAnchor.LowerRight : TextAnchor.LowerLeft;"),
            "the fan's grid fills from the lower corner on the menu's side, so the first token is nearest the button");
        Expect(menu.Contains("columnRect.pivot = new Vector2(onRight ? 1f : 0f, 0f);"), "the fan's pivot is its bottom corner on the menu's side");
        Expect(menu.Contains("columnRect.position = corners[onRight ? 2 : 1] + new Vector3(0f, 12f * canvasRoot.lossyScale.y, 0f);"),
            "the fan hangs one gap above the button's top corner on the menu's side");
        Expect(menu.Contains("float fromFoot = (screenCorners[1].y - columnRect.position.y)") &&
               menu.Contains("float room = Mathf.Min(fromFoot - headMargin, fromFoot - reachTop);"),
            "the room is measured from the fan's foot up to the top of the screen, then limited to the one-thumb band");
        int arrange = menu.IndexOf("tokens = ArrangeFan(tokens);", StringComparison.Ordinal);
        int rows = menu.IndexOf("stack.constraintCount = FanOrder.RowsThatFit(room, FanCell, FanGap, tokens.Count);", StringComparison.Ordinal);
        Expect(arrange > 0 && rows > arrange, "the fan is ordered before its rows are fitted");
        Expect(!menu.Contains("columnRect.pivot = new Vector2(0f, 1f);") && !menu.Contains("stack.startCorner = GridLayoutGroup.Corner.UpperLeft;"),
            "the old downward fan (top-left pivot, upper-left start) is gone");
    }

    private static void TourLogsReachForHubAndFan()
    {
        string tour = ReadEvergrove("Assets/Scripts/Unity/DevQaTour.Reach.cs");
        Expect(tour.StartsWith("#if UNITY_EDITOR || DEVELOPMENT_BUILD"), "the reach tour is dev-only");
        Expect(tour.Contains("public sealed partial class DevQaTour") && tour.Contains("private IEnumerator ReachShots()"),
            "ReachShots is a DevQaTour partial");
        Expect(tour.Contains("app.QaShowHub(true);") && tour.Contains("WaitForSecondsRealtime(0.4f)"),
            "the hub is shown and settled first");
        Expect(tour.Contains("LogReach(\"hub\"") && tour.Contains("LogReach(\"fan\"") && tour.Contains("LogReach(\"card\""),
            "the hub, the fan and the puzzle card are logged");
        Expect(tour.Contains("Debug.Log(\"MOSSELORA_REACH \" + scope + \" \" + control.name + \" \" + status);") &&
               tour.Contains("status = \"swipe\";") && tour.Contains("status = \"out\";"),
            "every control is logged as MOSSELORA_REACH reachable, out or swipe");
        Expect(tour.Contains("Debug.Log(\"MOSSELORA_REACH summary out \" + missed);"), "the summary is logged");
        Expect(tour.Contains("SavePng(\"29-reach-hub.png\")") && tour.Contains("SavePng(\"29-reach-fan.png\")"),
            "the tour saves the hub and the fan");
        Expect(tour.Contains("Press(\"Open menu\");") && tour.Contains("ThumbReach.Reachable("),
            "the tour opens the fan with the real button and uses the pure rule");
        Expect(tour.Contains("IsRestControl(control.transform)"), "the hub list reuses the rest count's garden exclusions");
    }

    private static void TourIsCalledFromTheMainTour()
    {
        string main = ReadEvergrove("Assets/Scripts/Unity/DevQaTour.cs");
        Expect(main.Contains("yield return ReachShots(); // checklist 94 (DevQaTour.Reach.cs)"), "DevQaTour.cs calls ReachShots");
        Expect(main.Split('\n').Length - 1 <= 1177, "DevQaTour.cs stays within its 1177-line budget");
    }

    private static void RegisteredInTheSuite()
    {
        Expect(ReadEvergrove("Tests/Program.cs").Contains("ThumbReachTests.Run()"), "ThumbReachTests is registered");
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
