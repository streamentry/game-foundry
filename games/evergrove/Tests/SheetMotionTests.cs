using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Evergrove.Core;

// Checklist 17: sheets slide from their own edge, move only transform and opacity, and
// reduced motion swaps every slide for a fade. The Unity side is checked by source audit,
// since UnityEngine is not part of the headless Core build.
internal static class SheetMotionTests
{
    private static int assertions;

    private static void Expect(bool ok, string message)
    {
        assertions++;
        if (!ok) throw new Exception("SheetMotionTests failed: " + message);
    }

    private static readonly string[] SheetRootFiles =
    {
        "EvergroveApp.cs", "EvergroveApp.Market.cs", "EvergroveApp.Dates.cs", "EvergroveApp.Name.cs",
        "EvergroveApp.Economy.cs", "FamilySheet.cs",
    };

    // Each sheet's named call site. A sheet not routed through SheetMotion fails here.
    private static readonly Dictionary<string, string> RoutedIn = new Dictionary<string, string>
    {
        { "Menu", "EvergroveApp.cs" },
        { "Journal", "EvergroveApp.cs" },
        { "Letter", "EvergroveApp.cs" },
        { "The stall", "EvergroveApp.Market.cs" },
        { "Deeper boards", "EvergroveApp.Market.cs" },
        { "Dates sheet", "EvergroveApp.Dates.cs" },
        { "Name sheet", "EvergroveApp.Name.cs" },
        { "Seed book", "EvergroveApp.Economy.cs" },
        { "Family", "FamilySheet.cs" },
    };

    // Direct SetActive calls on a sheet root, each with its reason. Only build-time hiding is allowed:
    // BuildSheet creates every sheet inactive, and SheetMotion is the one place that shows or hides it.
    private static readonly string[] SetActiveExceptions =
    {
        "EvergroveApp.cs: sheet.SetActive(false)", // BuildSheet's initial state, before any motion starts
    };

    // The only members SheetMotion may assign while animating: opacity and the panel's position.
    // Fade is a reference set once when the player is created, not a visual change.
    private static readonly string[] AllowedAssignments =
    {
        "alpha", "anchoredPosition", "blocksRaycasts", "interactable", "Fade",
    };

    private static readonly string[] ForbiddenMotionTokens =
    {
        "sizeDelta", "offsetMin", "offsetMax", "anchorMin", "anchorMax", "LayoutElement",
        "preferredWidth", "preferredHeight", "minWidth", "minHeight", "rect =", "fontSize", "localPosition",
    };

    public static int Run()
    {
        assertions = 0;
        EdgeTableCoversEverySheet();
        EdgeTableValues();
        EasingIsMonotonicAndEnds();
        MotionTouchesOnlyTransformAndOpacity();
        ReducedMotionNeverMovesThePanel();
        EverySheetShowAndHideGoesThroughSheetMotion();
        EverySheetIsRoutedWithItsEdge();
        return assertions;
    }

    private static void EdgeTableCoversEverySheet()
    {
        var buildNames = new SortedSet<string>();
        var routedNames = new SortedSet<string>();
        foreach (string file in SheetRootFiles)
        {
            string text = ReadUnity(file);
            foreach (Match m in Regex.Matches(text, @"BuildSheet\(\s*[^,]+,\s*""([^""]+)"""))
                buildNames.Add(m.Groups[1].Value);
            foreach (Match m in Regex.Matches(text, @"SheetEdge\.For\(""([^""]+)""\)"))
                routedNames.Add(m.Groups[1].Value);
        }

        var expected = new SortedSet<string> { "Menu", "Journal", "Seed book", "The stall", "Deeper boards", "Name sheet", "Dates sheet" };
        Expect(buildNames.SetEquals(expected), "BuildSheet names are the known seven: " + string.Join(", ", buildNames));
        foreach (string name in buildNames)
            Expect(SheetEdgeNoThrow(name), "every BuildSheet name has an edge: " + name);
        foreach (string name in routedNames)
            Expect(SheetEdgeNoThrow(name), "every SheetEdge.For name has an edge: " + name);
        foreach (string name in new[] { "Family", "Letter" })
            Expect(routedNames.Contains(name), "non-BuildSheet sheet routed with an edge: " + name);
        Expect(!SheetEdgeNoThrow("Nursery"), "unknown sheet names are refused, not defaulted");
    }

    private static void EdgeTableValues()
    {
        Expect(SheetEdge.For("Menu") == SheetEdge.Side.Bottom, "menu keeps its bottom rise");
        Expect(SheetEdge.For("Journal") == SheetEdge.Side.Right, "journal comes from the book corner (right)");
        Expect(SheetEdge.For("Letter") == SheetEdge.Side.Top, "letters come from the letterbox (top right)");
        Expect(SheetEdge.For("Family") == SheetEdge.Side.Bottom, "family card rises from the bottom");
        Expect(SheetEdge.For("Seed book") == SheetEdge.Side.Bottom && SheetEdge.For("The stall") == SheetEdge.Side.Bottom &&
               SheetEdge.For("Deeper boards") == SheetEdge.Side.Bottom && SheetEdge.For("Name sheet") == SheetEdge.Side.Bottom &&
               SheetEdge.For("Dates sheet") == SheetEdge.Side.Bottom, "seed book, stall, boards, name and dates rise from the bottom");
    }

    private static void EasingIsMonotonicAndEnds()
    {
        Expect(SheetEdge.Ease(0f) == 0f, "ease starts at 0");
        Expect(SheetEdge.Ease(1f) == 1f, "ease ends exactly at 1");
        Expect(SheetEdge.Ease(-3f) == 0f && SheetEdge.Ease(7f) == 1f, "ease clamps outside [0, 1]");
        float previous = SheetEdge.Ease(0f);
        for (int i = 1; i <= 2000; i++)
        {
            float value = SheetEdge.Ease(i / 2000f);
            Expect(value >= previous, "ease never decreases at step " + i);
            Expect(value >= 0f && value <= 1f, "ease stays within [0, 1] at step " + i);
            previous = value;
        }
        Expect(SheetEdge.Ease(0.5f) > 0.5f, "ease-out: more than halfway at the midpoint");
    }

    private static void MotionTouchesOnlyTransformAndOpacity()
    {
        string text = ReadUnity("SheetMotion.cs");
        foreach (string token in ForbiddenMotionTokens)
            Expect(!text.Contains(token), "SheetMotion never touches layout: " + token);

        var assigned = new SortedSet<string>();
        foreach (Match m in Regex.Matches(text, @"\b[A-Za-z_][\w]*\.(\w+)\s*(\+=|-=|=)(?!=)"))
            assigned.Add(m.Groups[1].Value);
        foreach (string member in assigned)
            Expect(AllowedAssignments.Contains(member), "SheetMotion assigns only opacity and position, not: " + member);
        Expect(assigned.Contains("alpha") && assigned.Contains("anchoredPosition"), "SheetMotion animates both opacity and position");
        Expect(!Regex.IsMatch(text, @"\bscale\b|localScale"), "SheetMotion does not scale sheets");
    }

    private static void ReducedMotionNeverMovesThePanel()
    {
        string text = ReadUnity("SheetMotion.cs");
        string apply = Between(text, "private void Apply(float p)", "private float Distance()");
        Expect(apply.Contains("shift = reduced ? Vector2.zero : Direction(side) * ((1f - p) * Distance());"),
            "reduced motion computes no slide offset in Apply");
        Expect(apply.Contains("panel.anchoredPosition = rest + shift;"), "Apply writes only rest plus the guarded shift");
        Expect(apply.IndexOf("reduced", StringComparison.Ordinal) < apply.IndexOf("Distance()", StringComparison.Ordinal),
            "the reduced check comes before any distance is read");

        string play = Between(text, "internal void Play(float target, bool instant, Action done)", "private IEnumerator Run");
        Expect(play.Contains("(reduced ? SheetMotion.FadeSeconds : SheetMotion.SlideSeconds)"),
            "reduced motion times a fade, not a slide");
        Expect(text.Contains("SlideSeconds = 0.28f") && text.Contains("FadeSeconds = 0.18f"),
            "slide 0.28 s and reduced fade 0.18 s");
        Expect(text.Contains("Fade.alpha = p"), "opacity follows the eased progress in both modes");
        Expect(text.Contains("#if UNITY_EDITOR || DEVELOPMENT_BUILD") && text.Contains("if (SheetMotion.Settled) instant = true;"),
            "QA screenshots settle sheets at once");
    }

    private static void EverySheetShowAndHideGoesThroughSheetMotion()
    {
        var found = new List<string>();
        foreach (string file in SheetRootFiles)
        {
            string text = ReadUnity(file);
            foreach (Match m in Regex.Matches(text,
                         @"(\b\w*[Ss]heet\w*|\bletterCard)(\.gameObject)?\.SetActive\(([^)]*)\)"))
                found.Add(file + ": " + m.Value);
        }
        var unexpected = found.Where(f => !SetActiveExceptions.Contains(f)).ToList();
        Expect(unexpected.Count == 0, "sheet roots are shown and hidden only by SheetMotion: " + string.Join("; ", unexpected));
        Expect(found.Count == SetActiveExceptions.Length, "SetActive exceptions are exactly the listed ones");

        string app = ReadUnity("EvergroveApp.cs");
        Expect(app.Contains("SheetMotion.Set(menuSheet, menuOpen,") && app.Contains("SheetMotion.Set(journalSheet, showJournal && !menuOpen,"),
            "menu and journal open and close through SheetMotion.Set in Rebuild");
    }

    private static void EverySheetIsRoutedWithItsEdge()
    {
        foreach (KeyValuePair<string, string> route in RoutedIn)
        {
            string text = ReadUnity(route.Value);
            Expect(text.Contains("SheetEdge.For(\"" + route.Key + "\")"),
                "sheet '" + route.Key + "' is routed with its edge in " + route.Value);
        }
        Expect(ReadUnity("FamilySheet.cs").Contains("SheetMotion.Hide(self, SheetEdge.For(\"Family\")"), "family dismiss hides before destroying");
    }

    private static bool SheetEdgeNoThrow(string name)
    {
        try
        {
            SheetEdge.For(name);
            return true;
        }
        catch (ArgumentException)
        {
            return false;
        }
    }

    private static string Between(string text, string start, string end)
    {
        int from = text.IndexOf(start, StringComparison.Ordinal);
        Expect(from >= 0, "found " + start);
        int to = text.IndexOf(end, from, StringComparison.Ordinal);
        Expect(to > from, "found " + end + " after " + start);
        return text.Substring(from, to - from);
    }

    private static string ReadUnity(string name)
    {
        return File.ReadAllText(Path.Combine(EvergroveRoot(), "Assets", "Scripts", "Unity", name));
    }

    private static string EvergroveRoot()
    {
        foreach (string start in new[] { Directory.GetCurrentDirectory(), AppContext.BaseDirectory })
        {
            DirectoryInfo dir = new DirectoryInfo(start);
            while (dir != null)
            {
                string candidate = Path.Combine(dir.FullName, "games", "evergrove");
                if (Directory.Exists(Path.Combine(candidate, "Assets", "Scripts", "Unity"))) return candidate;
                if (Directory.Exists(Path.Combine(dir.FullName, "Assets", "Scripts", "Unity"))) return dir.FullName;
                dir = dir.Parent;
            }
        }
        throw new Exception("Could not find games/evergrove from " + Directory.GetCurrentDirectory());
    }
}
