using System;
using System.IO;
using Evergrove.Core;

// Checklist 94, the card foot and the fan's reach band: upright, the puzzle card's hint, take turns and
// send move to a foot row beside undo; the menu fan climbs only as far as the one-thumb band. The band
// is pure (ThumbReach.TopOfReach); the wiring and the reach tour are checked by source audit, and the
// tour itself runs in the dev build.
internal static class CardFootTests
{
    private static int assertions;

    private static void Expect(bool ok, string message)
    {
        assertions++;
        if (!ok) throw new Exception("CardFootTests failed: " + message);
    }

    public static int Run()
    {
        assertions = 0;
        TopOfReachIsTheBandEdge();
        TopOfReachMatchesTheReachRule();
        BadHeightsCutNothingOff();
        FootMovesControlsUprightOnly();
        FootKeepsTheHintForTheSendOffer();
        ClashLensFindsItsTokenAfterTheMove();
        FanClimbsOnlyToTheReachBand();
        ZoomTourClosesEachCard();
        ReachTourAuditsTheCard();
        RegisteredInTheSuite();
        return assertions;
    }

    private static void TopOfReachIsTheBandEdge()
    {
        // 844 points tall: the band starts a third of the way down, at 281.33 points.
        Expect(Math.Abs(ThumbReach.TopOfReach(844f, true) - 844f / 3f) < 0.001f, "portrait: the band starts at a third of the height");
        // 390 points tall: landscape starts a quarter of the way down, at 97.5 points.
        Expect(Math.Abs(ThumbReach.TopOfReach(390f, false) - 97.5f) < 0.001f, "landscape: the band starts at a quarter of the height");
        Expect(ThumbReach.TopOfReach(300f, false) < ThumbReach.TopOfReach(300f, true),
            "a landscape band starts higher on the screen than a portrait one of the same height (a quarter, not a third)");
    }

    private static void TopOfReachMatchesTheReachRule()
    {
        float portraitTop = ThumbReach.TopOfReach(844f, true);
        Expect(ThumbReach.Reachable(195f, portraitTop, 390f, 844f), "portrait: a centre exactly on the band edge is in reach");
        Expect(!ThumbReach.Reachable(195f, portraitTop - 1f, 390f, 844f), "portrait: a centre just above the band edge is out");
        float landscapeTop = ThumbReach.TopOfReach(390f, false);
        Expect(ThumbReach.Reachable(50f, landscapeTop, 844f, 390f), "landscape: a left-corner centre on the band edge is in reach");
        Expect(!ThumbReach.Reachable(50f, landscapeTop - 1f, 844f, 390f), "landscape: a left-corner centre just above the band edge is out");
    }

    private static void BadHeightsCutNothingOff()
    {
        Expect(ThumbReach.TopOfReach(0f, true) == 0f, "a zero height cuts nothing off");
        Expect(ThumbReach.TopOfReach(-844f, false) == 0f, "a negative height cuts nothing off");
        Expect(ThumbReach.TopOfReach(float.NaN, true) == 0f, "a NaN height cuts nothing off");
        Expect(ThumbReach.TopOfReach(float.PositiveInfinity, false) == 0f, "an infinite height cuts nothing off");
        string core = ReadEvergrove("Assets/Scripts/Core/ThumbReach.cs");
        Expect(core.Contains("public static float TopOfReach(float height, bool portrait)") && !core.Contains("UnityEngine"),
            "TopOfReach keeps its agreed signature and the rule stays pure");
    }

    private static void FootMovesControlsUprightOnly()
    {
        string foot = ReadEvergrove("Assets/Scripts/Unity/TreePlay.Foot.cs");
        Expect(!foot.Contains("{ }"), "MoveControlsToFoot is no longer a stub");
        Expect(foot.Contains("if (Sideways(root) || cardRect == null) return;"),
            "the foot only applies upright: sideways, the top row already sits in the panel");
        Expect(foot.Contains("Transform top = cardRect.Find(\"Tree puzzle top\");"), "the foot reads the card's top row");
        Expect(foot.Contains("if (part.name == \"The thing\") continue;") &&
               foot.Contains("if (rewardPill != null && part.gameObject == rewardPill) continue;"),
            "the thing and the seed pill stay at the top");
        Expect(foot.Contains("Transform foot = Row(cardRect, \"Card foot\", FootRowHeight);"),
            "the controls go to a row named Card foot, appended to the card's end");
        Expect(foot.Contains("const float FootRowHeight = 60f;"), "the foot row is 60 points");
        Expect(foot.Contains("foreach (Transform part in top)") && foot.Contains("moving.Add(part);") &&
               foot.Contains("foreach (Transform part in moving) part.SetParent(foot, false);"),
            "the tokens are collected in order, then moved, so their order holds");
        Expect(foot.Contains("if (moving.Count == 0) { Destroy(foot.gameObject); return; }"),
            "no empty foot row is left behind");
        string tree = ReadEvergrove("Assets/Scripts/Unity/TreePlay.cs");
        int split = tree.IndexOf("SplitSideways(root); MoveControlsToFoot(root); AttachSwipeClose();", StringComparison.Ordinal);
        Expect(split > 0, "the foot is moved once the card settles, right after SplitSideways(root)");
        Expect(tree.Contains("Transform top = transform.Find(\"Tree puzzle card/Tree puzzle top\");"),
            "TreePlay.cs is not edited for the foot (the top row is still built as before)");
    }

    private static void FootKeepsTheHintForTheSendOffer()
    {
        string tree = ReadEvergrove("Assets/Scripts/Unity/TreePlay.cs");
        Expect(tree.Contains("Transform top = hintToken.transform.parent;") && tree.Contains("send.SetSiblingIndex(at);"),
            "OfferToSend reads the hint's parent when it runs, so the send token lands in the foot row where the hint moved");
        Expect(tree.Contains("hintToken = hintNode != null ? hintNode.GetComponent<Image>() : null;"),
            "the hint token reference is kept as the card builds, and moving it does not break it");
    }

    private static void ClashLensFindsItsTokenAfterTheMove()
    {
        string kit = ReadEvergrove("Assets/Scripts/Unity/BoardKit.cs");
        Expect(kit.Contains("() => ToggleClashes(lens, repaint), true);") && kit.Contains("lens = top.Find(caption);"),
            "the clash lens holds its own reference, so a tap still finds it after MoveControlsToFoot moves it");
        Expect(!kit.Contains("Transform lens = top.Find(ShowClashes ? HideCaption : ShowCaption);"),
            "the clash lens no longer looks itself up on the top row");
    }

    private static void FanClimbsOnlyToTheReachBand()
    {
        string menu = ReadEvergrove("Assets/Scripts/Unity/EvergroveApp.Menu.cs");
        Expect(menu.Contains("float reachTop = ThumbReach.TopOfReach(canvasRoot.rect.height, portrait);"),
            "the fan's band comes from ThumbReach.TopOfReach, portrait or landscape");
        Expect(menu.Contains("bool portrait = canvasRoot.rect.height > canvasRoot.rect.width;"),
            "the band follows the screen's orientation");
        Expect(menu.Contains("float room = Mathf.Min(fromFoot - headMargin, fromFoot - reachTop);"),
            "the room is the smaller of the safe-area room and the room up to the band");
        int arrange = menu.IndexOf("tokens = ArrangeFan(tokens);", StringComparison.Ordinal);
        int rows = menu.IndexOf("stack.constraintCount = FanOrder.RowsThatFit(room, FanCell, FanGap, tokens.Count);", StringComparison.Ordinal);
        Expect(arrange > 0 && rows > arrange, "the remembered token still goes first before the rows are fitted");
        string rest = ReadEvergrove("Assets/Scripts/Unity/DevQaTour.Rest.cs");
        Expect(rest.Contains("corners[1].y > Screen.height") && rest.Contains("MOSSELORA_FAN_FIT"),
            "LogFanFit's on-screen check is unchanged");
    }

    private static void ZoomTourClosesEachCard()
    {
        string zoom = ReadEvergrove("Assets/Scripts/Unity/DevQaTour.Zoom.cs");
        int leave = zoom.IndexOf("app.QaZoomTreePlay(false);", StringComparison.Ordinal);
        int close = zoom.IndexOf("Press(\"Close tree puzzle\");", StringComparison.Ordinal);
        int forget = zoom.IndexOf("app.QaEndTreePlay();", StringComparison.Ordinal);
        Expect(leave > 0 && close > leave && forget > close,
            "each zoomed card is closed with its own close button, then forgotten, before the next opens");
    }

    private static void ReachTourAuditsTheCard()
    {
        string tour = ReadEvergrove("Assets/Scripts/Unity/DevQaTour.Reach.cs");
        Expect(tour.Contains("private const string PuzzleCardName = \"Tree puzzle card\";"), "the card is named for the audit");
        Expect(tour.Contains("app.QaOpenKind(PuzzleKind.Doorstep, 1);"), "the card audit opens a board with a clash lens");
        Expect(tour.Contains("missed += LogReach(\"card\", CardReachControls());") && tour.Contains("SavePng(\"29-reach-card.png\")"),
            "every control on the card is logged and shot");
        Expect(tour.Contains("Press(\"Close tree puzzle\");") && tour.Contains("app.QaEndTreePlay();"),
            "the card is closed after the audit");
        Expect(tour.Contains("!UnderPuzzleCard(control.transform)"), "the hub list skips anything under a puzzle card");
        Expect(tour.Contains("control.name.StartsWith(\"Close\", System.StringComparison.Ordinal) && SwipeToClose.Closable(control.transform)") &&
               tour.Contains("status = \"swipe\";"),
            "a close button is logged as swipe when SwipeToClose can close it");
        Expect(tour.Contains("else { status = \"out\"; missed++; }"), "only an out-of-reach control counts as missed");
        Expect(tour.Contains("Debug.Log(\"MOSSELORA_REACH summary out \" + missed);"), "the summary counts only out");
        string swipe = ReadEvergrove("Assets/Scripts/Unity/SwipeToClose.cs");
        Expect(swipe.Contains("internal static bool Closable(Transform control)"), "the swipe helper the audit calls exists");
        string main = ReadEvergrove("Assets/Scripts/Unity/DevQaTour.cs");
        Expect(main.Split('\n').Length - 1 <= 1177, "DevQaTour.cs stays within its 1177-line budget");
    }

    private static void RegisteredInTheSuite()
    {
        Expect(ReadEvergrove("Tests/Program.cs").Contains("CardFootTests.Run()"), "CardFootTests is registered");
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
