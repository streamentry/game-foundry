using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

/// <summary>
/// Checklist 19 coverage audit: every control a player can press must have a caption that a long
/// press explains (LongPressCaption). The Unity scene cannot run under .NET, so this reads the
/// Unity sources. It checks the shared helpers attach the component, that every IconButton/Button
/// call has a caption, and that every raw interactive builder (AddComponent&lt;Button&gt; and tap
/// catchers) attaches it too. The allowlists pin exact keys (file:method:variable): a new
/// uncaptioned control fails, and a fixed one must be removed from its list.
/// </summary>
internal static class LongPressCaptionTests
{
    private static int assertions;

    private static void Expect(bool ok, string message)
    {
        assertions++;
        if (!ok) throw new Exception("LongPressCaptionTests failed: " + message);
    }

    // Legit non-control targets: full-screen veils and backdrops that dismiss a sheet, a letter or a
    // fan, and a letter's own paper that only blocks taps. Nothing here performs a game action.
    private static readonly Dictionary<string, int> KnownNonControls = new Dictionary<string, int>
    {
        // Full-screen or sheet veils that close what is open on a tap.
        { "EvergroveApp.Sheets.cs:BuildSheet:dimButton", 1 },
        { "EvergroveApp.Menu.cs:EnterQuiet:back", 1 },
        { "EvergroveApp.Journal.cs:JournalPageOpen:back", 1 },
        { "EvergroveApp.Choices.cs:NeighborConversation:neighborLayer", 1 },
        { "EvergroveApp.Cottage.cs:OpenCottage:backdrop", 1 },
        { "EvergroveApp.Letters.cs:ShowLetter:letterCard", 1 },
        { "EvergroveApp.Menu.cs:ToggleFan:away", 1 },
        // A letter's own paper: it only swallows taps so they do not reach the veil behind it.
        { "EvergroveApp.Letters.cs:LetterPaper:paper", 1 },
        // The garden map surface: a tap on land asks which landmark is there (GardenMapView.TapLand).
        { "GardenMapView.cs:Build:plotNode", 1 },
    };

    // Real controls built outside the shared helpers must attach LongPressCaption in their builder.
    // Empty: every known gap is captioned, so any new uncaptioned raw control fails this audit.
    private static readonly Dictionary<string, int> KnownBypassDebt = new Dictionary<string, int>
    {
    };

    private static readonly Regex RawBuilder = new Regex(@"AddComponent<(Button|TapCatcher|MapTap|GardenCreature)>\(\)");
    private static readonly Regex CallSite = new Regex(@"(?<![\w.])(IconButton|Button)\s*\(");
    private static readonly Regex DefinitionLine = new Regex(@"\bButton\s+(Button|IconButton)\s*\(");
    private static readonly Regex MethodHeader = new Regex(@"^ {8}(?:private|public|internal|protected|static)\b[^=;]*?\b(\w+)\s*\(");
    private static readonly Regex MethodEnd = new Regex(@"^ {8}\}\s*$");
    private static readonly Regex AssignedName = new Regex(@"(\w+)\s*=\s*[^;]*?AddComponent<");
    private static readonly Regex ChainedName = new Regex(@"(\w+)\.AddComponent<");

    public static int Run()
    {
        string unity = FindUnityDir();
        Expect(unity != null, "could not locate games/evergrove/Assets/Scripts/Unity from the working or base directory");

        var files = new List<string>(Directory.GetFiles(unity, "*.cs"));
        files.Sort(StringComparer.Ordinal);

        // 1. The shared helpers must attach the component (the item's core requirement).
        string appText = UnityAppSource.ReadAll();
        var helperAttach = HelperAttachStatus(appText);
        Expect(helperAttach.TryGetValue("Button", out bool buttonAttaches) && buttonAttaches,
            "the Button(…, Color tone) helper must call LongPressCaption.Attach");
        Expect(helperAttach.TryGetValue("IconButton", out bool iconAttaches) && iconAttaches,
            "the IconButton helper must call LongPressCaption.Attach (it replaced IconTip)");

        int controls = 0;
        var emptyCaptions = new List<string>();
        var nonControlHits = new Dictionary<string, int>();
        var debtHits = new Dictionary<string, int>();

        foreach (string path in files)
        {
            string file = Path.GetFileName(path);
            string text = File.ReadAllText(path);
            string[] lines = text.Split('\n');

            // 2. Every IconButton/Button call must carry a non-empty caption (its third argument).
            foreach (Match match in CallSite.Matches(text))
            {
                int line = LineOf(text, match.Index);
                if (DefinitionLine.IsMatch(lines[line - 1])) continue;
                List<string> args = Arguments(text, match.Index + match.Length - 1);
                controls++;
                string caption = args.Count >= 3 ? args[2].Trim() : "";
                if (caption == "" || caption == "\"\"" || caption == "null")
                    emptyCaptions.Add(file + ":" + MethodAt(lines, line) + " line " + line);
            }

            // 3. Raw interactive builders outside the helpers must attach the component in their method.
            for (int i = 0; i < lines.Length; i++)
            {
                Match raw = RawBuilder.Match(lines[i]);
                if (!raw.Success) continue;
                string method = MethodAt(lines, i + 1);
                string key = file + ":" + method + ":" + VariableOf(lines[i]);
                if (method == "Button" || method == "IconButton") continue; // the helpers, audited above
                controls++;
                if (AttachedInMethod(lines, i)) continue;
                Count(KnownNonControls.ContainsKey(key) ? nonControlHits : debtHits, key);
            }
        }

        Expect(emptyCaptions.Count == 0,
            "IconButton/Button calls without a caption: " + string.Join("; ", emptyCaptions));

        Expect(SameCounts(KnownNonControls, nonControlHits),
            "non-control allowlist drifted. Found: " + Describe(nonControlHits) + " expected: " + Describe(KnownNonControls));
        Expect(SameCounts(KnownBypassDebt, debtHits),
            "uncaptioned raw controls changed. Found: " + Describe(debtHits) + " expected: " + Describe(KnownBypassDebt));

        int allowlisted = 0;
        foreach (int n in nonControlHits.Values) allowlisted += n;
        int uncaptioned = emptyCaptions.Count;
        foreach (int n in debtHits.Values) uncaptioned += n;

        Console.WriteLine("Long-press coverage: " + controls + " controls, " + allowlisted + " allowlisted, "
            + uncaptioned + " uncaptioned");
        return assertions;
    }

    // Reads the body of each helper overload and reports whether it calls LongPressCaption.Attach.
    private static Dictionary<string, bool> HelperAttachStatus(string appText)
    {
        var status = new Dictionary<string, bool>();
        string[] lines = appText.Split('\n');
        for (int i = 0; i < lines.Length; i++)
        {
            Match header = Regex.Match(lines[i], @"private Button (Button|IconButton)\(");
            if (!header.Success) continue;
            // The wrapper overload delegates to the tone overload; only bodies with a real build count.
            bool isWrapper = lines[i].Contains("bool primary");
            if (isWrapper) continue;
            bool attaches = false;
            for (int j = i; j < lines.Length && !MethodEnd.IsMatch(lines[j]); j++)
                if (lines[j].Contains("LongPressCaption.Attach(")) attaches = true;
            status[header.Groups[1].Value] = attaches;
        }
        return status;
    }

    private static bool AttachedInMethod(string[] lines, int start)
    {
        for (int j = start; j < lines.Length; j++)
        {
            if (lines[j].Contains("LongPressCaption.Attach(")) return true;
            if (j > start && MethodEnd.IsMatch(lines[j])) return false;
        }
        return false;
    }

    private static string VariableOf(string line)
    {
        Match assigned = AssignedName.Match(line);
        if (assigned.Success) return assigned.Groups[1].Value;
        Match chained = ChainedName.Match(line);
        return chained.Success ? chained.Groups[1].Value : "?";
    }

    private static string MethodAt(string[] lines, int oneBased)
    {
        for (int i = oneBased - 1; i >= 0; i--)
        {
            Match header = MethodHeader.Match(lines[i]);
            if (header.Success) return header.Groups[1].Value;
        }
        return "?";
    }

    // Top-level comma-separated arguments of the call whose '(' is at openIndex.
    private static List<string> Arguments(string text, int openIndex)
    {
        var args = new List<string>();
        int depth = 0;
        var current = new System.Text.StringBuilder();
        for (int i = openIndex + 1; i < text.Length; i++)
        {
            char c = text[i];
            if (c == '"' || c == '\'')
            {
                int end = i + 1;
                while (end < text.Length && text[end] != c)
                {
                    if (text[end] == '\\') end++;
                    end++;
                }
                current.Append(text, i, Math.Min(end + 1, text.Length) - i);
                i = end;
                continue;
            }
            if (c == '(' || c == '[' || c == '{') depth++;
            if (c == ')' || c == ']' || c == '}')
            {
                if (depth == 0)
                {
                    args.Add(current.ToString());
                    return args;
                }
                depth--;
            }
            if (c == ',' && depth == 0)
            {
                args.Add(current.ToString());
                current.Clear();
                continue;
            }
            current.Append(c);
        }
        args.Add(current.ToString());
        return args;
    }

    private static int LineOf(string text, int index)
    {
        int line = 1;
        for (int i = 0; i < index && i < text.Length; i++)
            if (text[i] == '\n') line++;
        return line;
    }

    private static void Count(Dictionary<string, int> into, string key)
    {
        into.TryGetValue(key, out int n);
        into[key] = n + 1;
    }

    private static bool SameCounts(Dictionary<string, int> expected, Dictionary<string, int> found)
    {
        if (expected.Count != found.Count) return false;
        foreach (var pair in expected)
            if (!found.TryGetValue(pair.Key, out int n) || n != pair.Value) return false;
        return true;
    }

    private static string Describe(Dictionary<string, int> counts)
    {
        var parts = new List<string>();
        foreach (var pair in counts) parts.Add(pair.Key + " x" + pair.Value);
        parts.Sort(StringComparer.Ordinal);
        return "{" + string.Join("; ", parts) + "}";
    }

    private static string FindUnityDir()
    {
        string[] starts = { Directory.GetCurrentDirectory(), AppContext.BaseDirectory };
        foreach (string start in starts)
        {
            string dir = Path.GetFullPath(start);
            for (int up = 0; up < 12 && dir != null; up++)
            {
                string candidate = Path.Combine(dir, "games", "evergrove", "Assets", "Scripts", "Unity");
                if (Directory.Exists(candidate)) return candidate;
                dir = Directory.GetParent(dir)?.FullName;
            }
        }
        return null;
    }
}
