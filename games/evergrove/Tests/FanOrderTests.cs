using System;
using System.Collections.Generic;
using System.IO;
using Evergrove.Core;

// Checklist 15, the menu remembers: the fan opens with the token used last first, right under the
// corner button (nearest the thumb), and every other token keeps its usual order. The rule is pure;
// the wiring (remembered on the device, applied when the fan opens) and the logged tour session are
// checked by source audit.
internal static class FanOrderTests
{
    private static int assertions;

    private static void Expect(bool ok, string message)
    {
        assertions++;
        if (!ok) throw new Exception("FanOrderTests failed: " + message);
    }

    private static readonly string[] Usual = { "Memories", "Almanac", "Family", "Just look at the garden", "Settings" };

    public static int Run()
    {
        assertions = 0;
        LastUsedComesFirst();
        OthersKeepTheirOrder();
        UnknownOrBlankChangesNothing();
        InputIsNeverChanged();
        EveryTokenWhenUsedComesFirst();
        RowsThatFitWrapTheFan();
        AppRemembersAndArranges();
        TourLogsTheSecondTap();
        RegisteredInTheSuite();
        return assertions;
    }

    private static void LastUsedComesFirst()
    {
        List<string> order = FanOrder.Arrange(Usual, "Just look at the garden");
        Expect(order[0] == "Just look at the garden", "the token used last is first, under the corner button");
        Expect(FanOrder.Place(Usual, "Just look at the garden", "Just look at the garden") == 0, "its place is 0");
        Expect(FanOrder.Place(Usual, "", "Just look at the garden") == 3, "before it was used it sat fourth");
    }

    private static void OthersKeepTheirOrder()
    {
        List<string> order = FanOrder.Arrange(Usual, "Family");
        Expect(string.Join("|", order) == "Family|Memories|Almanac|Just look at the garden|Settings",
            "only the used token moves; the rest keep their usual order");
        Expect(order.Count == Usual.Length, "nothing is lost or added");
    }

    private static void UnknownOrBlankChangesNothing()
    {
        foreach (string last in new[] { null, "", "Home", "settings" })
            Expect(string.Join("|", FanOrder.Arrange(Usual, last)) == string.Join("|", Usual),
                "an unknown or blank last-used name leaves the usual order: " + (last ?? "null"));
        Expect(FanOrder.Arrange(null, "Family").Count == 0, "no tokens, no order");
        Expect(FanOrder.Place(Usual, "Family", "Home") == -1, "a token not in the fan has no place");
    }

    private static void InputIsNeverChanged()
    {
        var usual = new List<string>(Usual);
        FanOrder.Arrange(usual, "Settings");
        Expect(string.Join("|", usual) == string.Join("|", Usual), "arranging never changes the list it is given");
    }

    private static void EveryTokenWhenUsedComesFirst()
    {
        foreach (string token in Usual)
        {
            int before = FanOrder.Place(Usual, "", token);
            int after = FanOrder.Place(Usual, token, token);
            Expect(after == 0 && after <= before, token + " is never further after being used");
        }
    }

    // Checklist 11: a short screen wraps the fan into more columns instead of pushing tokens off screen.
    private static void RowsThatFitWrapTheFan()
    {
        Expect(FanOrder.RowsThatFit(1000f, 64f, 12f, 7) == 7, "a tall screen keeps one column of all seven");
        Expect(FanOrder.RowsThatFit(64f, 64f, 12f, 7) == 1, "room for exactly one token gives one row");
        Expect(FanOrder.RowsThatFit(215f, 64f, 12f, 7) == 2 && FanOrder.RowsThatFit(216f, 64f, 12f, 7) == 3,
            "rows step exactly where the next token (plus its gap) fits");
        Expect(FanOrder.RowsThatFit(-50f, 64f, 12f, 7) == 1, "no room still shows one row (never zero)");
        Expect(FanOrder.RowsThatFit(float.NaN, 64f, 12f, 7) == 7 && FanOrder.RowsThatFit(300f, 0f, 12f, 7) == 7,
            "bad input keeps a single column");
        Expect(FanOrder.RowsThatFit(300f, 64f, 12f, 0) == 1, "no tokens, one row");
        string menu = ReadEvergrove("Assets/Scripts/Unity/EvergroveApp.Menu.cs");
        // Checklist 94 and 5: the fan opens upward from the menu button (bottom-left, or bottom-right for a right hand).
        Expect(menu.Contains("stack.startAxis = GridLayoutGroup.Axis.Vertical;") &&
               menu.Contains("stack.startCorner = onRight ? GridLayoutGroup.Corner.LowerRight : GridLayoutGroup.Corner.LowerLeft;") &&
               menu.Contains("stack.constraintCount = FanOrder.RowsThatFit(room, FanCell, FanGap, tokens.Count);"),
            "the fan fills up from the menu's bottom corner, then wraps, by the rows that fit");
    }

    private static void AppRemembersAndArranges()
    {
        string chrome = ReadEvergrove("Assets/Scripts/Unity/EvergroveApp.Chrome.cs");
        string menu = ReadEvergrove("Assets/Scripts/Unity/EvergroveApp.Menu.cs");
        Expect(chrome.Contains("() => { RememberFanToken(name); action(); }"), "every fan token remembers itself when used");
        Expect(chrome.Contains("private const string FanLastKey = \"evergrove:fan-last\";") &&
               chrome.Contains("PlayerPrefs.SetString(FanLastKey, name);"), "the last token is kept on the device (PlayerPrefs)");
        Expect(chrome.Contains("FanOrder.Arrange(names, PlayerPrefs.GetString(FanLastKey, \"\"))"), "the fan is arranged by FanOrder");
        int arrange = menu.IndexOf("tokens = ArrangeFan(tokens);", StringComparison.Ordinal);
        int fadeIn = menu.IndexOf("StartCoroutine(FanIn(tokens[i]", StringComparison.Ordinal);
        Expect(arrange > 0 && fadeIn > arrange, "the fan is arranged before its tokens fade in, so the fade follows the new order");
    }

    private static void TourLogsTheSecondTap()
    {
        string tour = ReadEvergrove("Assets/Scripts/Unity/DevQaTour.Fan.cs");
        string main = ReadEvergrove("Assets/Scripts/Unity/DevQaTour.cs");
        Expect(tour.StartsWith("#if UNITY_EDITOR || DEVELOPMENT_BUILD"), "the fan tour is dev-only");
        Expect(tour.Contains("LogFan(\"before\")") && tour.Contains("LogFan(\"after\")") && tour.Contains("MOSSELORA_FAN"),
            "the session logs every token's distance before and after a use");
        Expect(tour.Contains("SavePng(\"24-fan-before.png\")") && tour.Contains("SavePng(\"24-fan-remembers.png\")"),
            "the tour saves the fan before and after");
        Expect(main.Contains("yield return FanShots();"), "the tour runs the fan session");
    }

    private static void RegisteredInTheSuite()
    {
        Expect(ReadEvergrove("Tests/Program.cs").Contains("FanOrderTests.Run()"), "FanOrderTests is registered");
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
