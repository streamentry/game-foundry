using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Evergrove.Core;

// Checklist 91: every interactive element has a touch area of at least 48 pt. This is a static
// audit of the Unity sources: each Button / IconButton / Token size literal is checked, every raw
// interactive component is registered with its reason, and the runtime scan
// (TouchTargets.Undersized, called from the DevQa tour) is verified to exist.
internal static class TouchTargetTests
{
    private static int assertions;
    private const float MinPt = 48f;

    private static void Expect(bool ok, string message)
    {
        assertions++;
        if (!ok) throw new Exception("TouchTargetTests failed: " + message);
    }

    // Size expressions that are computed at run time. Each needs an audited reason.
    private static readonly (string File, string Expr, string Reason)[] DynamicSizes =
    {
        ("SudokuBoard.cs", "token",
            "TrayToken(width, perRow): the tray wraps into two rows whenever one row would fall below 48 pt " +
            "(v1 #91), so a 6x6 tray is 3 per row on a 375 pt phone"),
        ("SumsBoard.cs", "token",
            "EXCEPTION: min(58, (width - pad - 32) / 5); reaches 48 only when the card is about 300 pt wide. " +
            "Verify on a 320 pt phone in the tour"),
    };

    // Raw interactive components (AddComponent of Button, InputField, or a click-handler class),
    // per file. Count must match exactly, so a new site fails until it is audited here.
    private static readonly Dictionary<string, string[]> RawSites = new Dictionary<string, string[]>
    {
        ["BoardKit.cs"] = new[]
        {
            "board cell (BoardKit.Cell): sized by the board; BoardFit.MinTouch is now 48 (v1 #91)",
        },
        ["EvergroveApp.Difficulty.cs"] = new[] { "level leaf: 56 pt literal LayoutElement" },
        ["EvergroveApp.Name.cs"] = new[] { "name text field: height from NameField layout (verify in tour)" },
        ["FamilySheet.cs"] = new[] { "text field: height from the Field helper's callers (48f)" },
        ["GardenNightCreatures.cs"] = new[] { "night creature (GardenCreature): its 48 x 48 tap area is asserted in GardenCreature.cs" },
        ["TreePlay.Zoom.cs"] = new[] { "board zoom picture (ZoomTap): covers the whole board view, which is far over 48 pt (v1 #91)" },
        ["GardenMapView.cs"] = new[]
        {
            "map item Button: size from the item's art rect (verify in tour)",
            "map plot tap (MapTap): size from the map cell (verify in tour)",
            "GardenCreature instance: its tap area is asserted in GardenCreature.cs",
        },
        ["PiecesBoard.cs"] = new[] { "piece tray slot: thumb * 4 (48 pt once thumb >= 12; verify in tour)" },
        ["TreePlay.cs"] = new[]
        {
            "EXCEPTION tree puzzle tile (board cell): unclamped cell from width; sideways may fall below 48",
            "EXCEPTION pairs card (board cell): unclamped cell from width and height",
            "EXCEPTION mosaic piece (board cell): unclamped cell from width and height",
            "Token builder: its size is checked at every call site",
        },
        ["EvergroveApp.cs"] = new[]
        {
            "Button helper: height from callers (default 52)",
            "dew board / lantern letter token (EvergroveApp.DayRhythm.cs): 52 x 52 literal sizeDelta",
            "IconButton helper: height from callers",
            "reading paper: page-sized",
            "dismiss scrim: full screen",
            "fan dismiss: full screen",
            "quiet layer dismiss: full screen",
            "medallion and name: layout-sized (verify in tour)",
            "journal page: page-sized image",
            "almanac grid cell (hint): >= 48 by the 7-or-6 column rule",
            "almanac grid cell (name): >= 48 by the 7-or-6 column rule",
            "memory shelf cell: min(64, (page - 40) / 4 - 8), 48+ for pages of 320 pt and up",
            "open memory card: page-sized",
            "EXCEPTION water tile (board cell): BoardFit clamps 48 portrait, 44 sideways",
            "tall card: MinHeight 96 or 150 literal",
            "cottage backdrop: full screen",
            "name field InputField: NameField height (verify in tour)",
            "letter tap-to-close layer: full screen",
            "neighbour tap-to-close layer: full screen",
        },
    };

    private static readonly string[] ClickHandlerClasses = { "GardenCreature", "MapTap", "TapCatcher", "ZoomTap" };

    private static readonly Regex BuilderCall = new Regex(@"(?<![A-Za-z_])(IconButton|Button|Token)\(");
    private static readonly Regex RawBuilder =
        new Regex(@"AddComponent<(Button|InputField|Toggle|TapCatcher|MapTap|GardenCreature|ZoomTap)>\(");
    private static readonly Regex ClickClass = new Regex(@"class\s+(\w+)[^{;]*IPointerClickHandler");

    public static int Run()
    {
        string unity = FindUnityDir();
        var files = Directory.GetFiles(unity, "*.cs", SearchOption.AllDirectories)
            .OrderBy(f => f, StringComparer.Ordinal).ToArray();
        var texts = new Dictionary<string, string>();
        foreach (string file in files) texts[Path.GetFileName(file)] = StripComments(File.ReadAllText(file));

        int checkedBuilders = 0, exceptions = 0;
        var dynamicHits = new HashSet<string>();
        foreach (var pair in texts)
        {
            string file = pair.Key, src = pair.Value;
            foreach (Match m in BuilderCall.Matches(src))
            {
                if (IsDefinition(src, m.Index)) continue;
                string callee = m.Groups[1].Value;
                int index = callee == "IconButton" ? 5 : callee == "Button" ? 4 : 3;
                List<string> args = ArgsAt(src, m.Index + m.Length - 1);
                checkedBuilders++;
                if (args.Count <= index)
                {
                    // Button(...) without a height uses the helper default of 52 pt.
                    Expect(callee == "Button", file + ": " + callee + " call has no size argument");
                    continue;
                }
                string expr = args[index];
                if (expr == "height") continue; // pass-through inside a helper; its callers are checked
                List<float> sizes = Sizes(expr, texts);
                if (sizes != null)
                {
                    foreach (float size in sizes)
                        Expect(size >= MinPt, file + ": " + callee + " size " + expr + " is " + size + " pt (< 48)");
                    continue;
                }
                string key = file + "|" + expr;
                Expect(DynamicSizes.Any(d => d.File + "|" + d.Expr == key),
                    file + ": computed " + callee + " size '" + expr + "' has no audited reason");
                dynamicHits.Add(key);
                exceptions++;
            }
        }
        foreach (var dynamic in DynamicSizes)
            Expect(dynamicHits.Contains(dynamic.File + "|" + dynamic.Expr),
                "audited dynamic size " + dynamic.File + " '" + dynamic.Expr + "' is no longer used");

        // The EvergroveApp partials are audited together as "EvergroveApp.cs" (checklist 20), except
        // files with their own RawSites entry.
        var rawCounts = new Dictionary<string, int>();
        foreach (var pair in texts)
        {
            string group = !RawSites.ContainsKey(pair.Key) && IsAppPartial(pair.Key) ? "EvergroveApp.cs" : pair.Key;
            int seen;
            rawCounts.TryGetValue(group, out seen);
            rawCounts[group] = seen + RawBuilder.Matches(pair.Value).Count;
        }
        int raw = 0;
        foreach (var pair in rawCounts)
        {
            string file = pair.Key;
            int count = pair.Value;
            string[] reasons;
            bool known = RawSites.TryGetValue(file, out reasons);
            Expect(count == (known ? reasons.Length : 0),
                file + ": " + count + " raw interactive sites, audited " + (known ? reasons.Length : 0));
            raw += count;
            exceptions += known ? reasons.Count(r => r.StartsWith("EXCEPTION", StringComparison.Ordinal)) : 0;
        }
        foreach (var audited in RawSites.Keys)
            Expect(texts.ContainsKey(audited), "audited file " + audited + " is missing");

        // Click-handler classes must be exactly the audited set.
        var handlers = texts.Values.SelectMany(s => ClickClass.Matches(s).Cast<Match>().Select(m => m.Groups[1].Value))
            .OrderBy(n => n, StringComparer.Ordinal).ToArray();
        Expect(handlers.SequenceEqual(ClickHandlerClasses.OrderBy(n => n, StringComparer.Ordinal)),
            "click-handler classes changed: " + string.Join(", ", handlers));

        // The visitor tap area stays at the 48 pt minimum.
        Expect(texts["GardenCreature.cs"].Contains("sizeDelta = new Vector2(48f, 48f)"),
            "GardenCreature tap area is not 48 x 48");

        // The runtime scan the DevQa tour calls.
        string touch = texts.ContainsKey("TouchTargets.cs") ? texts["TouchTargets.cs"] : "";
        Expect(touch.Length > 0, "TouchTargets.cs is missing");
        Expect(Regex.IsMatch(touch, @"MinPt\s*=\s*48f"), "TouchTargets.MinPt is not 48");
        Expect(Regex.IsMatch(touch, @"List<RectTransform>\s+Undersized\(RectTransform root,\s*float minPt\s*=\s*MinPt\)"),
            "TouchTargets.Undersized(root, minPt = MinPt) is missing");
        // ...and the tour really calls it: every shot is scanned, and the total is reported at the end.
        string scan = texts.ContainsKey("DevQaTour.Touch.cs") ? texts["DevQaTour.Touch.cs"] : "";
        string tourMain = texts.ContainsKey("DevQaTour.cs") ? texts["DevQaTour.cs"] : "";
        Expect(scan.Contains("TouchTargets.Undersized(root, TouchTargets.MinPt - ScanTolerance)") && scan.Contains("ScanTolerance = 0.05f") && scan.Contains("touch-scan.tsv"),
            "the tour's touch scan measures every root canvas and writes touch-scan.tsv");
        Expect(tourMain.Contains("ScanTouchTargets(fileName);") && tourMain.Contains("ReportTouchScan();"),
            "every tour shot is scanned and the total reported before MOSSELORA_QA_TOUR_DONE");

        Console.WriteLine("Touch targets: " + (checkedBuilders + raw) + " builders checked, " + exceptions + " exceptions");
        return assertions;
    }

    private static bool IsAppPartial(string file)
    {
        return file.StartsWith("EvergroveApp", StringComparison.Ordinal) && file.EndsWith(".cs", StringComparison.Ordinal);
    }

    // Literal, ternary of literals, or a const/var declared from literals. Null means computed.
    private static List<float> Sizes(string expr, Dictionary<string, string> texts)
    {
        if (Regex.IsMatch(expr, @"[A-Za-z_]\s*\.|\(|\[")) return null;
        if (Regex.IsMatch(expr, @"^[A-Za-z_]\w*$"))
        {
            foreach (string src in texts.Values)
            {
                Match decl = Regex.Match(src, @"\b(const\s+)?(int|float)\s+" + expr + @"\s*=\s*([^;]+);");
                if (decl.Success) return Sizes(decl.Groups[3].Value.Trim(), texts);
            }
            return null;
        }
        var values = new List<float>();
        foreach (Match m in Regex.Matches(expr, @"\d+(\.\d+)?"))
            values.Add(float.Parse(m.Value, CultureInfo.InvariantCulture));
        return values.Count == 0 ? null : values;
    }

    private static bool IsDefinition(string src, int index)
    {
        int lineStart = src.LastIndexOf('\n', Math.Max(0, index - 1)) + 1;
        string prefix = src.Substring(lineStart, index - lineStart);
        return Regex.IsMatch(prefix, @"\b(private|public|internal|static|protected)\b") &&
               Regex.IsMatch(prefix, @"\b(void|Button|Transform|GameObject)\s+$");
    }

    // Arguments of the call whose '(' is at index open, split at top-level commas.
    private static List<string> ArgsAt(string src, int open)
    {
        var args = new List<string>();
        int depth = 0, start = open + 1;
        bool inString = false;
        char quote = '\0';
        for (int i = open; i < src.Length; i++)
        {
            char c = src[i];
            if (inString)
            {
                if (c == '\\') i++;
                else if (c == quote) inString = false;
                continue;
            }
            if (c == '"' || c == '\'') { inString = true; quote = c; continue; }
            if (c == '(' || c == '[' || c == '{') depth++;
            else if (c == ')' || c == ']' || c == '}')
            {
                depth--;
                if (depth == 0)
                {
                    args.Add(src.Substring(start, i - start).Trim());
                    return args;
                }
            }
            else if (c == ',' && depth == 1)
            {
                args.Add(src.Substring(start, i - start).Trim());
                start = i + 1;
            }
        }
        return args;
    }

    // Removes // and /* */ comments and keeps string literals intact; newlines are preserved.
    private static string StripComments(string src)
    {
        var sb = new StringBuilder(src.Length);
        int i = 0;
        while (i < src.Length)
        {
            char c = src[i];
            if (c == '/' && i + 1 < src.Length && src[i + 1] == '/')
            {
                while (i < src.Length && src[i] != '\n') i++;
                continue;
            }
            if (c == '/' && i + 1 < src.Length && src[i + 1] == '*')
            {
                int end = src.IndexOf("*/", i + 2, StringComparison.Ordinal);
                int stop = end < 0 ? src.Length : end + 2;
                for (int k = i; k < stop; k++) if (src[k] == '\n') sb.Append('\n');
                i = stop;
                continue;
            }
            if (c == '"' || c == '\'')
            {
                char quote = c;
                sb.Append(c);
                i++;
                while (i < src.Length && src[i] != quote)
                {
                    if (src[i] == '\\' && i + 1 < src.Length) { sb.Append(src[i]); i++; }
                    sb.Append(src[i]);
                    i++;
                }
                if (i < src.Length) { sb.Append(src[i]); i++; }
                continue;
            }
            sb.Append(c);
            i++;
        }
        return sb.ToString();
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
        throw new Exception("TouchTargetTests: Unity sources not found");
    }
}
