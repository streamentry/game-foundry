using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

// Checklist 92: a colour-blind pass. Every place a colour carries state also carries a shape,
// a pattern or a mark (WCAG 1.4.1). This is a static audit of the Unity sources: each listed site
// must still exist and still set its paired non-colour cue inside the same method.
internal static class ColourCueTests
{
    private static int assertions;

    private static void Expect(bool ok, string message)
    {
        assertions++;
        if (!ok) throw new Exception("ColourCueTests failed: " + message);
    }

    // File, method (or the method that paints the state), and the non-colour cue it must call or name.
    private static readonly (string File, string Method, string Cue)[] Sites =
    {
        ("PipeGraphic.cs", "OnPopulateMesh", "AddDashes("),                 // dry pipe: broken stripe, not just brown
        ("BoardKit.cs", "ClashLens", "StateMark("),                         // show-clashes lens: check when on
        ("BoardKit.cs", "ToggleClashes", "StateMark("),
        ("BoardKit.cs", "MarkClash", "BoldRing()"),                         // conflict: ring, never colour alone
        ("BoardKit.cs", "Style", "interactable"),                           // undo/redo off: refuses taps, not just faded
        ("StreamBoard.cs", "Paint", "MarkRing("),                           // on-path cells: channel bars + ring on hint
        ("StreamBoard.cs", "Hint", "MarkRing("),
        ("SumsBoard.cs", "Paint", "MarkRing("),                             // selected / hinted cell
        ("SumsBoard.cs", "Paint", "MarkClash("),
        ("SudokuBoard.cs", "Paint", "MarkRing("),                           // selected cell
        ("SudokuBoard.cs", "Paint", "\"Given mark\""),                      // given vs entered: a mark, not tan alone
        ("SudokuBoard.cs", "Paint", "MarkClash("),
        ("NonogramBoard.cs", "Paint", "sprouts["),                          // filled: sprout glyph
        ("NonogramBoard.cs", "Paint", "marks["),                            // marked: cross glyph
        ("NonogramBoard.cs", "Hint", "MarkRing("),
        ("MahjongBoard.cs", "Paint", "MarkRing("),                          // selected tile
        ("MahjongBoard.cs", "Hint", "MarkRing("),                           // hinted pair
        ("PlotsBoard.cs", "Paint", "MarkRing("),                            // corner pick
        ("PlotsBoard.cs", "Paint", "PaintBedPictures("),                    // bed tones: each bed has its own picture
        ("PlotsBoard.cs", "Hint", "MarkRing("),
        ("SowBoard.cs", "Paint", "MarkRing("),                              // shown bed
        ("SowBoard.cs", "Paint", "StateMark("),                             // ready bed: check
        ("DoorstepBoard.cs", "Paint", "MarkRing("),                         // shown cell
        ("DoorstepBoard.cs", "Paint", "MarkClash("),
        ("PiecesBoard.cs", "Paint", "FitMarkName"),                         // snug-fit glow: ring
        ("PiecesBoard.cs", "Paint", "PieceTag("),                           // placed piece: its number
        ("PiecesBoard.cs", "Paint", "FocusMarkName"),                       // chosen tray piece: ring
        ("TreePlay.cs", "PaintTurn", "icon.localScale"),                    // whose turn: icon flips (I268)
        ("TreePlay.cs", "PaintPairs", "StateMark("),                        // matched pairs: check
        ("TreePlay.cs", "Hint", "MarkRing("),                               // pairs hint
        ("TreePlay.cs", "PaintMosaic", "MarkRing("),                        // picked mosaic tile
        ("TreePlay.cs", "PaintMosaic", "StateMark("),                       // tile in place: check
        ("EvergroveApp.Difficulty.cs", "AddLevelButton", "StateMark("),     // chosen leaves level: check
        ("EvergroveApp.Controls.cs", "IconButton", "StateMark("),                    // every settings token with a stateOn
    };

    // Settings on/off tokens: each must change its icon with the state, or carry a stateOn mark.
    private static readonly (string Label, string Cue)[] Toggles =
    {
        ("Gentle motion setting", "IconMotionOn"),
        ("Quiet audio setting", "IconChimeOn"),
        ("Haptics setting", "stateOn: Haptics.Enabled"),
        ("Day light setting", "stateOn: DayLightOn"),
        ("Garden notes setting", "stateOn: GardenNotes.On"),
        ("Speak less setting", "stateOn: SpeakLessOn"),
    };

    public static int Run()
    {
        string unity = FindUnityDir();
        var files = new Dictionary<string, string>();
        foreach (string file in Directory.GetFiles(unity, "*.cs", SearchOption.AllDirectories))
            files[Path.GetFileName(file)] = StripComments(File.ReadAllText(file));

        foreach (var site in Sites)
        {
            Expect(files.ContainsKey(site.File), "source present: " + site.File);
            string body = MethodBody(files[site.File], site.Method);
            Expect(body != null, site.File + " still has " + site.Method);
            Expect(body.Contains(site.Cue), site.File + ":" + site.Method + " still sets its cue " + site.Cue);
        }

        foreach (var toggle in Toggles)
        {
            string window = ToggleWindow(files, toggle.Label);
            Expect(window != null, "settings token present: " + toggle.Label);
            Expect(window.Contains(toggle.Cue), toggle.Label + " changes its icon or mark with state");
        }

        Console.WriteLine("Colour cues: " + Sites.Length + " sites, all paired");
        return assertions;
    }

    // The method's text: from its declaration (one with a body, not an interface line) to the next member at the same (8-space) indent.
    private static string MethodBody(string text, string method)
    {
        var decl = new Regex(@"(?m)^\s+(?:(?:private|public|internal|protected|static|override|virtual|async)\s+)*[\w<>\[\],\.]+\s+" +
                             Regex.Escape(method) + @"\s*\([^;{)]*\)\s*\{");
        Match m = decl.Match(text);
        if (!m.Success) return null;
        var next = new Regex(@"(?m)^        (?:private|public|internal|protected|static)\b");
        Match end = next.Match(text, m.Index + m.Length);
        return text.Substring(m.Index, (end.Success ? end.Index : text.Length) - m.Index);
    }

    // The settings call that carries a label: from the label to the next button or member.
    private static string ToggleWindow(Dictionary<string, string> files, string label)
    {
        foreach (string text in files.Values)
        {
            int at = text.IndexOf("\"" + label, StringComparison.Ordinal);
            if (at < 0) continue;
            int end = text.Length;
            foreach (string stop in new[] { "Button(", "\n        private " })
            {
                int i = text.IndexOf(stop, at + 1, StringComparison.Ordinal);
                if (i >= 0 && i < end) end = i;
            }
            return text.Substring(at, end - at);
        }
        return null;
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
        throw new Exception("ColourCueTests: Unity sources not found");
    }
}
