using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Evergrove.Core;

// Checklist 14: one back. Every close / back control sits in the same top corner, at the same
// inset and size, with the same cancel icon, and one back gesture closes the topmost open layer.
// This is a static audit of the Unity sources; the placement numbers live in Core BackPlacement.
internal static class BackPlacementTests
{
    private static int assertions;

    private static void Expect(bool ok, string message)
    {
        assertions++;
        if (!ok) throw new Exception("BackPlacementTests failed: " + message);
    }

    // Sheet sites: each passes its name to EvergroveApp.SheetClose, which places it with BackPlacement.
    private static readonly string[] SheetSites =
    {
        "Close seed book", "Close the stall", "Close naming", "Close dates", "Close deeper boards",
        "Close menu", "Close journal", "Close travel",
    };

    // Card and panel sites: each builds its own token and places it with BackPlacement.
    private static readonly (string File, string Name)[] CardSites =
    {
        ("FamilySheet.cs", "Close family"),
        ("TreePlay.cs", "Close tree puzzle"),
        ("EvergroveApp.cs", "Close letter"),
        ("EvergroveApp.ChoicePreview.cs", "Close choice preview"),
    };

    // Close-like names that are not a layer's back control, each with its reason (audited, not hidden).
    private static readonly (string Name, string Reason)[] Exceptions =
    {
        ("Close basket", "garden-corner toggle that opens and shuts the planting basket, not a layer"),
        ("Close language picker", "continue step inside the menu's language list, placed at the bottom"),
        ("Close matched control", "dev-only matched-control entry, not shipped to players"),
    };

    // Every IconCancel use per file. Each is a back control site or a named decision (keep, not now,
    // basket toggle); the MosseloraArt definition is the one in the art catalogue.
    private static readonly Dictionary<string, int> CancelIcons = new Dictionary<string, int>
    {
        ["MosseloraArt.cs"] = 1,                    // the definition
        ["EvergroveApp.cs"] = 6,                    // SheetClose, Close letter, basket toggle, Keep (reset),
                                                    // Cancel garden restore (Keep), Not now (notes)
        ["FamilySheet.cs"] = 1,                     // Close family
        ["TreePlay.cs"] = 1,                        // Close tree puzzle
        ["EvergroveApp.ChoicePreview.cs"] = 1,      // Close choice preview
    };

    // The one back gesture, top layer first. Each name is that layer's own close method.
    private static readonly string[] BackOrder =
    {
        "CloseLetter()", "familySheet.Close()", "CloseSeedBook()", "CloseMarketSheet()",
        "CloseNameSheet()", "CloseDatesSheet()", "CloseWeeklyBoardSheet()", "CloseTravelSheet()", "SetMenu(false)",
        "CloseJournal()", "CloseChoicePreview()", "openCard.Close()",
    };

    private static readonly Regex CloseName = new Regex("\"(Close [^\"]*)\"");
    private static readonly Regex SheetCloseCall = new Regex(@"SheetClose\(");
    private static readonly Regex CancelIcon = new Regex(@"\bIconCancel\b");

    public static int Run()
    {
        // Placement numbers (the one rule).
        Expect(BackPlacement.Corner == BackCorner.TopRight, "back controls sit top-right");
        Expect(BackPlacement.Inset == 12f, "back inset is 12 pt");
        Expect(BackPlacement.Size == 56f, "back token is 56 pt, the size every site passes");
        Expect(BackPlacement.Reserve == BackPlacement.Size + BackPlacement.Inset, "reserve is size plus inset");

        var texts = LoadSources();
        string evergrove = Get(texts, "EvergroveApp.cs");

        // Sheet sites share one helper, and it places the control with the rule.
        int defAt = evergrove.IndexOf("private void SheetClose(", StringComparison.Ordinal);
        int endAt = evergrove.IndexOf("private void HandleBackGesture()", defAt, StringComparison.Ordinal);
        Expect(defAt >= 0 && endAt > defAt, "SheetClose helper found");
        string sheetBody = evergrove.Substring(defAt, endAt - defAt);
        Expect(sheetBody.Contains("MosseloraArt.IconCancel"), "SheetClose uses IconCancel");
        Expect(sheetBody.Contains("PlaceBack("), "SheetClose places with PlaceBack");
        Expect(!sheetBody.Contains("PlaceChrome(") && !sheetBody.Contains("-12f"), "SheetClose has no ad-hoc corner");

        int sheetCalls = texts.Values.Sum(t => SheetCloseCall.Matches(t).Count) - 1; // minus the definition
        Expect(sheetCalls == SheetSites.Length, "every sheet site calls SheetClose (" + sheetCalls + ")");
        foreach (string name in SheetSites)
        {
            bool called = texts.Values.Any(t => t.Split('\n').Any(l => l.Contains("SheetClose(") && l.Contains("\"" + name + "\"")));
            Expect(called, "sheet site uses SheetClose: " + name);
        }

        // Card and panel sites: the token uses the rule's size and cancel icon, then is placed.
        foreach (var site in CardSites)
        {
            string text = Get(texts, site.File);
            int at = text.IndexOf("\"" + site.Name + "\"", StringComparison.Ordinal);
            Expect(at >= 0, "card site present: " + site.Name);
            int place = text.IndexOf("PlaceBack(", at, StringComparison.Ordinal);
            Expect(place > at && place - at < 900, "card site placed with PlaceBack: " + site.Name);
            string window = text.Substring(at, place - at);
            Expect(window.Contains("MosseloraArt.IconCancel"), "card site uses IconCancel: " + site.Name);
            Expect(window.Contains(", 56"), "card site passes the rule size: " + site.Name);
            Expect(!window.Contains("IconConfirm"), "card site has no confirm icon: " + site.Name);
        }

        // No other close icon: every IconCancel use is counted per file.
        foreach (var kv in texts)
        {
            int n = CancelIcon.Matches(kv.Value).Count;
            int want;
            CancelIcons.TryGetValue(kv.Key, out want);
            Expect(n == want, "IconCancel uses in " + kv.Key + " = " + n + ", expected " + want);
        }

        // No other close-like name: every "Close ..." literal is a site or a named exception.
        var found = new HashSet<string>();
        foreach (var kv in texts)
        {
            if (kv.Key == "DevQaTour.cs") continue; // the tour presses these names; it does not place them
            foreach (Match m in CloseName.Matches(kv.Value)) found.Add(m.Groups[1].Value);
        }
        var expected = new HashSet<string>(SheetSites.Concat(CardSites.Select(s => s.Name))
            .Concat(Exceptions.Select(e => e.Name)));
        foreach (string name in found)
            Expect(expected.Contains(name), "unexpected close-like name: " + name);
        foreach (string name in expected)
            Expect(found.Contains(name), "expected close name present: " + name);

        // One back gesture: the Update hook, the Escape / back key, and the layer order.
        int backAt = evergrove.IndexOf("HandleBackGesture();", StringComparison.Ordinal);
        int tickAt = evergrove.IndexOf("TickSoundscape();", StringComparison.Ordinal);
        Expect(backAt >= 0 && backAt < tickAt, "Update runs the back gesture first");
        Expect(evergrove.Contains("KeyCode.Escape") && evergrove.Contains("escapeKey"),
            "back gesture reads Escape on both input paths");
        int start = evergrove.IndexOf("private void HandleBackGesture()", StringComparison.Ordinal);
        Expect(start >= 0, "back gesture handler found");
        int stop = evergrove.IndexOf("\n        }", start, StringComparison.Ordinal);
        string handler = evergrove.Substring(start, stop - start);
        int last = -1;
        foreach (string call in BackOrder)
        {
            int at = handler.IndexOf(call, StringComparison.Ordinal);
            Expect(at > last, "back gesture closes in order at " + call);
            last = at;
        }

        Console.WriteLine("Back controls: " + (SheetSites.Length + CardSites.Length) + " sites, one corner");
        return assertions;
    }

    private static Dictionary<string, string> LoadSources()
    {
        string unity = FindUnityDir();
        var texts = new Dictionary<string, string>();
        foreach (string file in Directory.GetFiles(unity, "*.cs", SearchOption.AllDirectories))
            texts[Path.GetFileName(file)] = StripComments(File.ReadAllText(file));
        return texts;
    }

    private static string Get(Dictionary<string, string> texts, string file)
    {
        Expect(texts.ContainsKey(file), "source present: " + file);
        return texts[file];
    }

    private static string StripComments(string text)
    {
        text = Regex.Replace(text, @"/\*.*?\*/", "", RegexOptions.Singleline);
        return Regex.Replace(text, @"//[^\n]*", "");
    }

    private static string FindUnityDir()
    {
        foreach (string start in new[] { Directory.GetCurrentDirectory(), AppContext.BaseDirectory })
        {
            for (var dir = new DirectoryInfo(start); dir != null; dir = dir.Parent)
            {
                string nested = Path.Combine(dir.FullName, "games", "evergrove", "Assets", "Scripts", "Unity");
                if (Directory.Exists(nested)) return nested;
                string direct = Path.Combine(dir.FullName, "Assets", "Scripts", "Unity");
                if (Directory.Exists(direct)) return direct;
            }
        }
        throw new Exception("BackPlacementTests: Unity sources not found");
    }
}
