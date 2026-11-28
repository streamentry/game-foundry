using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Evergrove.Core;

// Checklist 94: a downward swipe closes any sheet or card, as iOS sheets do. The pure rule is checked
// here; the Unity side (SwipeToClose and its attach sites) is checked by source audit, since UnityEngine
// is not part of the headless Core build.
internal static class SwipeCloseTests
{
    private static int assertions;

    private static void Expect(bool ok, string message)
    {
        assertions++;
        if (!ok) throw new Exception("SwipeCloseTests failed: " + message);
    }

    // Every file that calls PlaceBack( must attach the swipe with the same action its close token uses.
    // Each entry lists the attach call and the close token's action; a new site fails until listed here.
    private static readonly Dictionary<string, string[]> AttachSites = new Dictionary<string, string[]>
    {
        ["EvergroveApp.Sheets.cs"] = new[]
        {
            "SwipeToClose.Attach(panel.GetComponent<RectTransform>(), close, gentleMotion)",
            "() => close(), 56",
        },
        ["FamilySheet.cs"] = new[]
        {
            "SwipeToClose.Attach(node.GetComponent<RectTransform>(), Close, !ReducedMotion())",
            "MosseloraTheme.QuietAction, Close, true);",
        },
        ["EvergroveApp.ChoicePreview.cs"] = new[]
        {
            "SwipeToClose.Attach(choicePreviewPanel, closePreviewAction, gentleMotion)",
            "MosseloraArt.IconCancel, closePreviewAction, 56",
        },
        ["EvergroveApp.Letters.cs"] = new[]
        {
            "SwipeToClose.Attach(paperRect, CloseLetter, gentleMotion)",
            "MosseloraArt.IconCancel, CloseLetter, 56",
        },
        ["TreePlay.cs"] = new[]
        {
            "AttachSwipeClose();",
            "MosseloraTheme.QuietAction, Close, true);",
        },
    };

    public static int Run()
    {
        assertions = 0;
        DownwardSwipeCloses();
        SidewaysUpwardAndShortDoNotClose();
        ThresholdIsAdjustable();
        FollowIsClampedAndDownwardOnly();
        SwipeRuleSourceMatches();
        SwipeNeverMovesLayout();
        EveryPlaceBackSiteAttachesTheSwipe();
        PuzzleCardAttachesWithItsCloseAction();
        EverySheetRoutesThroughSheetClose();
        ClosableFindsTheSwipeAboveAControl();
        RegisteredInTheSuite();
        return assertions;
    }

    private static void DownwardSwipeCloses()
    {
        Expect(SwipeClose.Closes(0f, 120f), "a straight downward swipe closes");
        Expect(SwipeClose.Closes(0f, SwipeClose.Threshold), "exactly the threshold closes");
        Expect(SwipeClose.Closes(40f, 100f), "a downward swipe that leans a little closes");
        Expect(SwipeClose.Closes(-60f, 100f), "leaning left and mostly down closes");
        Expect(SwipeClose.Closes(60f, 100f), "leaning right and mostly down closes");
    }

    private static void SidewaysUpwardAndShortDoNotClose()
    {
        Expect(!SwipeClose.Closes(200f, 0f), "a sideways swipe does not close");
        Expect(!SwipeClose.Closes(-300f, 50f), "a fast sideways swipe does not close");
        Expect(!SwipeClose.Closes(0f, -200f), "an upward swipe does not close");
        Expect(!SwipeClose.Closes(0f, 0f), "no movement does not close");
        Expect(!SwipeClose.Closes(0f, 89.9f), "just short of the threshold does not close");
        Expect(!SwipeClose.Closes(0f, 30f), "a short tug does not close");
        Expect(!SwipeClose.Closes(80f, 100f), "a diagonal that is mostly sideways does not close");
        Expect(!SwipeClose.Closes(150f, 100f), "sideways-dominant motion does not close");
        Expect(!SwipeClose.Closes(float.NaN, 200f) && !SwipeClose.Closes(0f, float.NaN), "NaN never closes");
        Expect(!SwipeClose.Closes(0f, float.PositiveInfinity), "infinity never closes");
    }

    private static void ThresholdIsAdjustable()
    {
        Expect(SwipeClose.Threshold == 90f, "the swipe threshold is 90 canvas points");
        Expect(SwipeClose.Closes(0f, 50f, 40f), "a caller's smaller threshold is honoured");
        Expect(!SwipeClose.Closes(0f, 120f, 200f), "a caller's larger threshold is honoured");
    }

    private static void FollowIsClampedAndDownwardOnly()
    {
        Expect(SwipeClose.FollowOffset(100f) == 50f, "the panel follows half the travel");
        Expect(SwipeClose.FollowOffset(10000f) == SwipeClose.MaxFollow, "the follow is clamped");
        Expect(SwipeClose.FollowOffset(-80f) == 0f, "an upward drag does not move the panel");
        Expect(SwipeClose.FollowOffset(0f) == 0f, "a still finger does not move the panel");
        Expect(SwipeClose.FollowOffset(float.NaN) == 0f, "NaN does not move the panel");
    }

    private static void SwipeRuleSourceMatches()
    {
        string core = File.ReadAllText(Path.Combine(Root(), "Assets", "Scripts", "Core", "SwipeClose.cs"));
        Expect(core.Contains("public const float Threshold = 90f;"), "the threshold constant is in Core");
        Expect(core.Contains("public static bool Closes(float dx, float dy, float threshold = Threshold)"),
            "Closes takes dx, dy and a threshold");
        Expect(core.Contains("dy is positive DOWNWARD"), "the sign convention is documented");
        Expect(!core.Contains("UnityEngine"), "the rule stays pure (no UnityEngine)");
    }

    private static void SwipeNeverMovesLayout()
    {
        string text = File.ReadAllText(Path.Combine(UnityDir(), "SwipeToClose.cs"));
        Expect(text.Contains("IBeginDragHandler, IDragHandler, IEndDragHandler"), "it implements the three drag handlers");
        Expect(text.Contains("public void OnBeginDrag(") && text.Contains("public void OnDrag(") && text.Contains("public void OnEndDrag("),
            "the drag handler methods exist");
        Expect(text.Contains("public System.Action Close;"), "it holds the close action");
        Expect(!Regex.IsMatch(text, @"\.(sizeDelta|anchorMin|anchorMax|pivot|offsetMin|offsetMax|preferredWidth|preferredHeight|minWidth|minHeight|flexibleWidth|localScale)\b\s*="),
            "it never writes a layout or size property");
        Expect(!Regex.IsMatch(text, @"LayoutElement|LayoutGroup|ContentSizeFitter|SetSizeWithCurrentAnchors"),
            "it adds no layout component");
        foreach (Match m in Regex.Matches(text, @"\.(\w*[Pp]osition)\s*=(?!=)"))
            Expect(m.Groups[1].Value == "localPosition", "only localPosition is written, found " + m.Value);
        Expect(text.Contains("if (!Follow || rect == null) return;"), "reduced motion (Follow false) never moves the panel");
        Expect(Regex.IsMatch(text, @"rect\.localPosition = restPosition;"), "a cancel or a close restores the rest position");
        Expect(text.Contains("internal static bool Closable(Transform control)"), "Closable is the reach hook the tour calls");
        Expect(text.Contains("isActiveAndEnabled"), "Closable counts only an enabled swipe");
        Expect(!text.Contains("DevQaTour") && !text.Contains("DEVELOPMENT_BUILD"), "no dev-only names in the shipping component");
    }

    private static void EveryPlaceBackSiteAttachesTheSwipe()
    {
        var call = new Regex(@"(?<!void )\bPlaceBack\(");
        var found = new List<string>();
        foreach (string file in Directory.GetFiles(UnityDir(), "*.cs").OrderBy(f => f, StringComparer.Ordinal))
        {
            string text = File.ReadAllText(file);
            if (!call.IsMatch(text)) continue;
            string name = Path.GetFileName(file);
            found.Add(name);
            Expect(AttachSites.ContainsKey(name), name + " calls PlaceBack but has no swipe-to-close entry");
            if (!AttachSites.ContainsKey(name)) continue;
            foreach (string fragment in AttachSites[name])
                Expect(text.Contains(fragment), name + " must contain: " + fragment);
        }
        Expect(found.Count == AttachSites.Count && AttachSites.Keys.All(found.Contains),
            "every PlaceBack site is audited: " + string.Join(", ", found));
    }

    private static void PuzzleCardAttachesWithItsCloseAction()
    {
        string swipe = File.ReadAllText(Path.Combine(UnityDir(), "TreePlay.Swipe.cs"));
        Expect(swipe.Contains("private void AttachSwipeClose()"), "the puzzle card's stub is filled in");
        Expect(swipe.Contains("SwipeToClose.Attach(cardRect, Close, gentle);"),
            "the puzzle card swipes to its own Close, following only with gentle motion");
    }

    private static void EverySheetRoutesThroughSheetClose()
    {
        string app = UnityAppSource.ReadAll();
        int builds = Count(app, "BuildSheet(") - Count(app, "GameObject BuildSheet(");
        int closes = Count(app, "SheetClose(") - Count(app, "void SheetClose(");
        Expect(builds > 0, "the shared sheet builder is used");
        Expect(closes >= builds, "every built sheet gets its close button (and the swipe) through SheetClose");
        int attaches = 0;
        foreach (string file in Directory.GetFiles(UnityDir(), "*.cs"))
            attaches += Count(File.ReadAllText(file), "SwipeToClose.Attach(");
        Expect(attaches == 5, "exactly the audited sites attach the swipe, found " + attaches);
    }

    private static void ClosableFindsTheSwipeAboveAControl()
    {
        string text = File.ReadAllText(Path.Combine(UnityDir(), "SwipeToClose.cs"));
        Expect(text.Contains("for (Transform t = control; t != null; t = t.parent)"),
            "Closable walks up from the control to its ancestors");
        Expect(text.Contains("GetComponent<SwipeToClose>()"), "Closable looks for the component on each ancestor");
        Expect(text.Contains("GetComponent<Graphic>() == null"),
            "an attach to an empty panel adds a clear graphic so a drag on it still reaches the swipe");
    }

    private static void RegisteredInTheSuite()
    {
        Expect(File.ReadAllText(Path.Combine(Root(), "Tests", "Program.cs")).Contains("SwipeCloseTests.Run()"),
            "SwipeCloseTests is registered");
    }

    private static int Count(string text, string needle)
    {
        return Regex.Matches(text, Regex.Escape(needle)).Count;
    }

    private static string UnityDir()
    {
        return Path.Combine(Root(), "Assets", "Scripts", "Unity");
    }

    private static string Root()
    {
        string dir = AppContext.BaseDirectory;
        while (dir != null && !Directory.Exists(Path.Combine(dir, "Assets", "Scripts", "Unity")))
            dir = Path.GetDirectoryName(dir);
        if (dir == null) throw new Exception("games/evergrove root not found from " + AppContext.BaseDirectory);
        return dir;
    }
}
