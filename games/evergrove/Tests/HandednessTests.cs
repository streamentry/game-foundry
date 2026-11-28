using System;
using System.Collections.Generic;
using System.IO;
using Evergrove.Core;

// Checklist 5, remember the hand: asked once on the first long-press, stored on the device, and the menu,
// its fan, the trays and the sideways panel mirror to suit. The rules are pure (Handedness, SheetEdge); the
// ask, the mirror, the settings token and the hand tour are checked by source audit of the Unity sources.
internal static class HandednessTests
{
    private static int assertions;

    private static void Expect(bool ok, string message)
    {
        assertions++;
        if (!ok) throw new Exception("HandednessTests failed: " + message);
    }

    public static int Run()
    {
        assertions = 0;
        MenuCornerPerHand();
        PanelSidePerHand();
        AskOnlyOnce();
        StoredTextRoundTrips();
        GarbageIsUnknown();
        SettingsTokenCycles();
        HandSheetRisesFromTheFoot();
        AskTriggeredByFirstLongPress();
        AnswerStaysOnTheDevice();
        AskSheetHasItsChoicesAndBack();
        MenuFanAndTraysMirror();
        SidewaysPanelFollowsTheHand();
        SettingsTokenExists();
        TourShotsAndLogsBothHands();
        TourCalledFromTheMainTour();
        HeatmapToolMatchesTheReachModel();
        RegisteredInTheSuite();
        RightHandHeatmapFixes();
        return assertions;
    }

    // The 6.7-inch landscape heatmap (batch 31) found, for a right hand: the fan hung from the page's
    // hidden left-corner button and ran off the screen, and the right-corner menu sat on home's Play token.
    private static void RightHandHeatmapFixes()
    {
        string menu = ReadEvergrove("Assets/Scripts/Unity/EvergroveApp.Menu.cs");
        string hub = ReadEvergrove("Assets/Scripts/Unity/EvergroveApp.Hub.cs");
        Expect(menu.Contains("Transform cornerHost = titleScreen != null ? titleScreen.transform : (Transform)canvasRoot;"),
            "with home open, the fan hangs from home's own corner button");
        Expect(hub.Contains("if (MenuOnRight && Screen.width > Screen.height)") &&
               hub.Contains("token.GetComponent<HorizontalLayoutGroup>().padding.right = (int)(MenuReserve * 2f);"),
            "sideways with the menu on the right, home's play token keeps clear of it");
    }

    private static void MenuCornerPerHand()
    {
        Expect(Handedness.MenuCorner(Hand.Right) == HandCorner.BottomRight, "a right hand's menu sits bottom-right");
        Expect(Handedness.MenuCorner(Hand.Left) == HandCorner.BottomLeft, "a left hand's menu sits bottom-left");
        Expect(Handedness.MenuCorner(Hand.Unknown) == HandCorner.BottomLeft, "unknown keeps the bottom-left menu from before the question");
    }

    private static void PanelSidePerHand()
    {
        Expect(Handedness.PanelOnRight(Hand.Right, true), "sideways, a right hand's control panel is on the right");
        Expect(!Handedness.PanelOnRight(Hand.Left, true), "sideways, a left hand's control panel is on the left");
        Expect(Handedness.PanelOnRight(Hand.Unknown, true), "sideways, unknown keeps the panel on the right as now");
        Expect(Handedness.PanelOnRight(Hand.Left, false) && Handedness.PanelOnRight(Hand.Unknown, false),
            "upright there is no side panel, so the side answer is the default");
    }

    private static void AskOnlyOnce()
    {
        Expect(Handedness.ShouldAsk(Hand.Unknown, false), "an unknown hand that was never asked is asked");
        Expect(!Handedness.ShouldAsk(Hand.Unknown, true), "an unknown hand already asked (declined) is not asked again");
        Expect(!Handedness.ShouldAsk(Hand.Left, false), "a known left hand is never asked");
        Expect(!Handedness.ShouldAsk(Hand.Right, true), "a known right hand is never asked");
    }

    private static void StoredTextRoundTrips()
    {
        foreach (Hand hand in new[] { Hand.Unknown, Hand.Left, Hand.Right })
            Expect(Handedness.Parse(Handedness.Serialise(hand)) == hand, "round trip keeps " + hand);
        Expect(Handedness.Serialise(Hand.Left) == "left" && Handedness.Serialise(Hand.Right) == "right",
            "the stored words are left and right");
        Expect(Handedness.Parse("  RIGHT ") == Hand.Right, "parsing ignores case and spaces");
    }

    private static void GarbageIsUnknown()
    {
        foreach (string text in new[] { null, "", "both", "middle", "1", "lefty" })
            Expect(Handedness.Parse(text) == Hand.Unknown, "garbage reads as unknown: " + (text ?? "null"));
    }

    private static void SettingsTokenCycles()
    {
        Expect(Handedness.Next(Hand.Right) == Hand.Left, "right moves to left");
        Expect(Handedness.Next(Hand.Left) == Hand.Right, "left moves back to right");
        Expect(Handedness.Next(Hand.Unknown) == Hand.Right, "unknown moves to right");
    }

    private static void HandSheetRisesFromTheFoot()
    {
        Expect(SheetEdge.For("Hand") == SheetEdge.Side.Bottom, "the hand question rises from the bottom, with its own SheetEdge entry");
    }

    private static void AskTriggeredByFirstLongPress()
    {
        string caption = ReadEvergrove("Assets/Scripts/Unity/LongPressCaption.cs");
        string app = ReadEvergrove("Assets/Scripts/Unity/EvergroveApp.cs");
        string sheet = ReadEvergrove("Assets/Scripts/Unity/EvergroveApp.Handedness.cs");
        Expect(caption.Contains("internal static event System.Action Shown;"), "a caption announces itself through Shown");
        Expect(caption.Contains("var shown = Shown;") && caption.Contains("if (shown != null) shown();"),
            "showing a caption raises Shown, which is the first long-press");
        Expect(app.Contains("LongPressCaption.Shown += OnLongPressShown;") && app.Contains("LongPressCaption.Shown -= OnLongPressShown;"),
            "the app subscribes on start and unsubscribes when destroyed");
        Expect(sheet.Contains("if (!Handedness.ShouldAsk(hand, HandAsked) || handSheet != null) return;"),
            "the question is asked only while ShouldAsk holds and no sheet is open");
        Expect(sheet.Contains("world.Choices.Length == 0") && sheet.Contains("if (AnyLayerOpen()) return;"),
            "the question never opens in the first minutes or over a puzzle, card, fan or sheet");
    }

    private static void AnswerStaysOnTheDevice()
    {
        string sheet = ReadEvergrove("Assets/Scripts/Unity/EvergroveApp.Handedness.cs");
        Expect(ReadEvergrove("Assets/Scripts/Core/Handedness.cs").Contains("public const string HandKey = \"evergrove:hand\";"),
            "the hand is kept under evergrove:hand");
        Expect(ReadEvergrove("Assets/Scripts/Core/Handedness.cs").Contains("public const string AskedKey = \"evergrove:hand-asked\";"),
            "the asked flag is kept under evergrove:hand-asked");
        Expect(sheet.Contains("PlayerPrefs.SetString(Handedness.HandKey") && sheet.Contains("PlayerPrefs.SetInt(Handedness.AskedKey"),
            "both are written to PlayerPrefs");
        foreach (string file in new[] { "Assets/Scripts/Core/WorldModel.cs", "Assets/Scripts/Unity/EvergroveApp.Saving.cs" })
            Expect(!ReadEvergrove(file).Contains("Handedness") && !ReadEvergrove(file).Contains("evergrove:hand"),
                "the save never holds the hand: " + file);
    }

    private static void AskSheetHasItsChoicesAndBack()
    {
        string sheet = ReadEvergrove("Assets/Scripts/Unity/EvergroveApp.Handedness.cs");
        Expect(sheet.Contains("\"Which hand do you hold your phone with?\""), "the question is the one asked");
        Expect(sheet.Contains("\"Left hand\"") && sheet.Contains("\"Right hand\""), "two hand tokens: Left hand and Right hand");
        Expect(sheet.Contains("HandTokenSize = 64") && sheet.Contains("MosseloraArt.IconTap"), "the tokens are 64 pt with the tap icon");
        Expect(sheet.Contains("glyph.localScale = new Vector3(-1f, 1f, 1f);"), "the left token's icon is mirrored");
        Expect(sheet.Contains("Button(content, \"Not now\", \"Not now\", DeclineHand, 52);"), "a quiet Not now records asked and stays unknown");
        Expect(sheet.Contains("BuildSheet(titleScreen.transform, HandSheetName, DeclineHand") &&
               sheet.Contains("SheetClose(content, \"Close hand\", DeclineHand);") &&
               sheet.Contains("SheetMotion.Show(handSheet, SheetEdge.For(HandSheetName), ReducedMotion);") &&
               sheet.Contains("RetireSheet(handSheet, HandSheetName, false);"),
            "the sheet uses BuildSheet, SheetClose, SheetMotion and RetireSheet");
        string menu = ReadEvergrove("Assets/Scripts/Unity/EvergroveApp.Menu.cs");
        Expect(menu.Contains("if (handSheet != null) DeclineHand();"), "the one back gesture closes the hand sheet first");
        string sheets = ReadEvergrove("Assets/Scripts/Unity/EvergroveApp.Sheets.cs");
        Expect(sheets.Contains("PlaceBack(button.GetComponent<RectTransform>());"), "its close token is placed with the one back placement");
    }

    private static void MenuFanAndTraysMirror()
    {
        string menu = ReadEvergrove("Assets/Scripts/Unity/EvergroveApp.Menu.cs");
        string hub = ReadEvergrove("Assets/Scripts/Unity/EvergroveApp.Hub.cs");
        string map = ReadEvergrove("Assets/Scripts/Unity/EvergroveApp.MapActions.cs");
        string chrome = ReadEvergrove("Assets/Scripts/Unity/EvergroveApp.Chrome.cs");
        Expect(menu.Contains("PlaceChrome(menu, MenuCornerAnchor, new Vector2(MenuSideOffset, MenuInset));"),
            "the menu button follows the hand's corner in BuildChrome");
        Expect(hub.Contains("PlaceChrome(menu, MenuCornerAnchor, new Vector2(MenuSideOffset, MenuInset));"),
            "the hub's menu button follows the hand's corner");
        Expect(ReadEvergrove("Assets/Scripts/Unity/EvergroveApp.Handedness.cs").Contains("Handedness.MenuCorner(hand) == HandCorner.BottomRight"),
            "the menu side comes from Handedness.MenuCorner");
        Expect(menu.Contains("stack.startCorner = onRight ? GridLayoutGroup.Corner.LowerRight : GridLayoutGroup.Corner.LowerLeft;") &&
               menu.Contains("stack.childAlignment = onRight ? TextAnchor.LowerRight : TextAnchor.LowerLeft;"),
            "the fan fills up from the lower corner on the menu's side");
        Expect(menu.Contains("columnRect.pivot = new Vector2(onRight ? 1f : 0f, 0f);") &&
               menu.Contains("columnRect.position = corners[onRight ? 2 : 1] +"),
            "the fan hangs from the menu's side: pivot and top corner follow the hand");
        Expect(map.Contains("cornerRect.anchoredPosition = new Vector2(MenuReserveOffset, 10f);"),
            "the open basket tray keeps its reserve on the far side from the menu");
        Expect(chrome.Contains("rect.anchoredPosition = new Vector2(MenuReserveOffset, 14f);"),
            "the pinned action row keeps its reserve on the far side from the menu");
        Expect(ReadEvergrove("Assets/Scripts/Unity/EvergroveApp.Handedness.cs").Contains("internal float MenuReserveOffset"),
            "the reserve side is defined once, in the hand file");
    }

    private static void SidewaysPanelFollowsTheHand()
    {
        string side = ReadEvergrove("Assets/Scripts/Unity/TreePlay.Sideways.cs");
        Expect(side.Contains("bool panelRight = Handedness.PanelOnRight(HandHolding, true);"),
            "the sideways split asks Handedness which side the panel is on");
        Expect(side.Contains("if (!panelRight) panel.SetAsFirstSibling();"), "a left hand's panel is the first side, on the left");
        Expect(side.Contains("internal static Hand HandHolding = Hand.Unknown;"), "the hand reaches the card through one field set by the app");
        Expect(side.Contains("\"Card controls\"") && side.Contains("\"Board side\""), "the two sides keep their names");
    }

    private static void SettingsTokenExists()
    {
        string settings = ReadEvergrove("Assets/Scripts/Unity/EvergroveApp.Settings.cs");
        Expect(settings.Contains("HandSettingButton(row.transform);"), "Settings holds the hand token");
        string sheet = ReadEvergrove("Assets/Scripts/Unity/EvergroveApp.Handedness.cs");
        Expect(sheet.Contains("\"Hand setting\"") && sheet.Contains("ChooseHand(Handedness.Next(hand))") &&
               sheet.Contains("MosseloraArt.IconTap") && sheet.Contains("stateOn: hand == Hand.Left"),
            "the token cycles the hand with its icon and state, and applies at once");
    }

    private static void TourShotsAndLogsBothHands()
    {
        string tour = ReadEvergrove("Assets/Scripts/Unity/DevQaTour.Hand.cs");
        Expect(tour.StartsWith("#if UNITY_EDITOR || DEVELOPMENT_BUILD"), "the hand tour is dev-only");
        Expect(tour.Contains("foreach (Hand holding in new[] { Hand.Left, Hand.Right })"), "the tour runs both hands");
        Expect(tour.Contains("SavePng(\"31-hand-\" + side + \"-hub.png\")") && tour.Contains("SavePng(\"31-hand-\" + side + \"-fan.png\")"),
            "the hub and the open fan are saved for each hand");
        Expect(tour.Contains("Debug.Log(\"MOSSELORA_HANDMAP \" + side + \" \" + name + \" \""),
            "every control is logged as MOSSELORA_HANDMAP");
        Expect(tour.Contains("HubReachControls()") && tour.Contains("FanReachControls()"),
            "the controls are the same active, non-garden set the reach tour uses");
        Expect(tour.Contains("app.QaSetHand(Hand.Unknown, asked);") && tour.Contains("bool asked = app.QaHandAsked;"),
            "the hand and its asked flag are restored after the tour");
        Expect(ReadEvergrove("Assets/Scripts/Unity/EvergroveApp.HandQa.cs").StartsWith("#if UNITY_EDITOR || DEVELOPMENT_BUILD"),
            "the QA seam is dev-only");
    }

    private static void TourCalledFromTheMainTour()
    {
        string main = ReadEvergrove("Assets/Scripts/Unity/DevQaTour.cs");
        int reach = main.IndexOf("yield return ReachShots();", StringComparison.Ordinal);
        int hand = main.IndexOf("yield return HandShots(); // checklist 5 (DevQaTour.Hand.cs)", StringComparison.Ordinal);
        Expect(hand > reach && reach > 0, "the hand tour runs after the reach tour in DevQaTour.cs");
        Expect(main.Split('\n').Length <= 1177, "DevQaTour.cs stays at or under 1177 lines");
    }

    private static void HeatmapToolMatchesTheReachModel()
    {
        string tool = ReadRepo("tools/evergrove_hand_heatmap.py");
        string reach = ReadEvergrove("Assets/Scripts/Core/ThumbReach.cs");
        Expect(tool.Contains("LANDSCAPE_SIDE_SHARE = 0.45") && reach.Contains("LandscapeSideShare = 0.45f"),
            "the heatmap's side share is ThumbReach's");
        Expect(tool.Contains("LANDSCAPE_TOP_SHARE = 1 / 4") && reach.Contains("LandscapeTopShare = 1f / 4f"),
            "the heatmap's top share is ThumbReach's");
        Expect(tool.Contains("PORTRAIT_TOP_SHARE = 1 / 3") && reach.Contains("PortraitTopShare = 1f / 3f"),
            "the heatmap's portrait share is ThumbReach's");
        Expect(tool.Contains("MOSSELORA_HANDMAP (left|right)"), "the heatmap reads the hand log lines");
    }

    private static void RegisteredInTheSuite()
    {
        Expect(ReadEvergrove("Tests/Program.cs").Contains("HandednessTests.Run()"), "HandednessTests is registered");
    }

    private static string ReadEvergrove(string relative)
    {
        string dir = AppContext.BaseDirectory;
        while (dir != null && !File.Exists(Path.Combine(dir, relative)))
            dir = Path.GetDirectoryName(dir);
        if (dir == null) throw new Exception(relative + " not found");
        return File.ReadAllText(Path.Combine(dir, relative));
    }

    // The heatmap tool lives beside the game folder, at the repository root.
    private static string ReadRepo(string relative)
    {
        string dir = AppContext.BaseDirectory;
        while (dir != null && !File.Exists(Path.Combine(dir, relative)))
            dir = Path.GetDirectoryName(dir);
        if (dir == null) throw new Exception(relative + " not found");
        return File.ReadAllText(Path.Combine(dir, relative));
    }
}
