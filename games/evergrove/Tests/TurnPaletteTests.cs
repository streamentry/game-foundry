using System;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using Evergrove.Core;

/// <summary>
/// Pins the take-turns palette: every pair of colours keeps a WCAG contrast of at least 1.3
/// (so they differ in greyscale), each colour keeps its hue family, Rgb returns fresh copies
/// and wraps, and TreePlay.cs reads its colours from TurnPalette rather than the old fields.
/// </summary>
internal static class TurnPaletteTests
{
    private static int assertions;

    private static void Expect(bool ok, string message)
    {
        assertions++;
        if (!ok) throw new Exception("TurnPaletteTests failed: " + message);
    }

    public static int Run()
    {
        assertions = 0;

        // Pairwise greyscale separation.
        double[] lum = new double[TurnPalette.Count];
        for (int i = 0; i < TurnPalette.Count; i++)
        {
            float[] c = TurnPalette.Rgb(i);
            lum[i] = TurnPalette.Luminance(c[0], c[1], c[2]);
        }
        for (int i = 0; i < TurnPalette.Count; i++)
        {
            for (int j = i + 1; j < TurnPalette.Count; j++)
            {
                Expect(TurnPalette.Contrast(lum[i], lum[j]) >= 1.3,
                    "colours " + i + " and " + j + " keep contrast 1.3 or more");
            }
        }

        // Colour-vision separation: every pair under Normal, protanopia, deuteranopia and tritanopia
        // (Machado 2009, severity 1.0). Rows are pairs, columns are vision types; the minimum is locked.
        ColourVision.Vision[] visions =
        {
            ColourVision.Vision.Normal, ColourVision.Vision.Protanopia,
            ColourVision.Vision.Deuteranopia, ColourVision.Vision.Tritanopia,
        };
        const double MeasuredMinUnderVision = 1.3648;
        double minUnderVision = double.MaxValue;
        string minPair = "";
        Console.WriteLine("TurnPalette contrast under vision (WCAG ratio):");
        Console.WriteLine("  pair         Normal  Protan  Deutan  Tritan");
        for (int i = 0; i < TurnPalette.Count; i++)
        {
            for (int j = i + 1; j < TurnPalette.Count; j++)
            {
                float[] fa = TurnPalette.Rgb(i);
                float[] fb = TurnPalette.Rgb(j);
                double[] a = { fa[0], fa[1], fa[2] };
                double[] b = { fb[0], fb[1], fb[2] };
                string row = "  " + i + "-" + j + "         ";
                foreach (ColourVision.Vision v in visions)
                {
                    double ratio = ColourVision.ContrastUnder(v, a, b);
                    row += ratio.ToString("0.000", System.Globalization.CultureInfo.InvariantCulture) + "   ";
                    Expect(ratio >= 1.36, "turn colours " + i + " and " + j + " keep 1.36 under " + v);
                    if (ratio < minUnderVision)
                    {
                        minUnderVision = ratio;
                        minPair = i + "-" + j + " " + v;
                    }
                }
                Console.WriteLine(row);
            }
        }
        Console.WriteLine("  minimum " + minUnderVision.ToString("0.0000", System.Globalization.CultureInfo.InvariantCulture) + " (" + minPair + ")");
        Expect(Math.Abs(minUnderVision - MeasuredMinUnderVision) < 0.005,
            "measured minimum under vision is locked at " + MeasuredMinUnderVision);

        // Hue families: sage green, honey warm, sky blue, rose red.
        float[] sage = TurnPalette.Rgb(0);
        Expect(sage[1] > sage[0] && sage[1] > sage[2], "sage is green-led");
        float[] honey = TurnPalette.Rgb(1);
        Expect(honey[0] >= honey[1] && honey[1] > honey[2], "honey is warm");
        float[] sky = TurnPalette.Rgb(2);
        Expect(sky[2] > sky[0] && sky[2] > sky[1], "sky is blue-led");
        float[] rose = TurnPalette.Rgb(3);
        Expect(rose[0] > rose[1] && rose[0] > rose[2], "rose is red-led");

        // Copies and wrap-around.
        float[] first = TurnPalette.Rgb(0);
        first[0] = 0f;
        Expect(TurnPalette.Rgb(0)[0] == 0.42f, "Rgb returns a copy");
        float[] wrapped = TurnPalette.Rgb(4);
        float[] zero = TurnPalette.Rgb(0);
        Expect(wrapped[0] == zero[0] && wrapped[1] == zero[1] && wrapped[2] == zero[2], "Rgb(4) equals Rgb(0)");
        float[] minusOne = TurnPalette.Rgb(-1);
        float[] three = TurnPalette.Rgb(3);
        Expect(minusOne[0] == three[0] && minusOne[1] == three[1] && minusOne[2] == three[2], "Rgb(-1) equals Rgb(3)");

        // Paint names: each seat is named by the colour its pip shows, and wraps like Rgb.
        Expect(TurnPalette.Name(0) == "sage", "seat 0 is sage");
        Expect(TurnPalette.Name(1) == "honey", "seat 1 is honey");
        Expect(TurnPalette.Name(2) == "sky", "seat 2 is sky");
        Expect(TurnPalette.Name(3) == "rose", "seat 3 is rose");
        Expect(TurnPalette.Name(4) == "sage", "Name(4) wraps to sage");
        Expect(TurnPalette.Name(-1) == "rose", "Name(-1) wraps to rose");
        Expect(TurnPalette.Name(5) == "honey", "Name(5) wraps to honey");
        Expect(TurnPalette.Name(int.MinValue).Length > 0, "the smallest turn names a colour without throwing");
        Expect(TurnPalette.Name(int.MaxValue).Length > 0, "the largest turn names a colour without throwing");
        for (int turn = -9; turn <= 9; turn++)
        {
            int index = ((turn % TurnPalette.Count) + TurnPalette.Count) % TurnPalette.Count;
            float[] named = TurnPalette.Rgb(turn);
            float[] byIndex = TurnPalette.Rgb(index);
            Expect(TurnPalette.Name(turn) == TurnPalette.Name(index), "Name(" + turn + ") matches its index " + index);
            Expect(named[0] == byIndex[0] && named[1] == byIndex[1] && named[2] == byIndex[2],
                "Name(" + turn + ") names the same index Rgb uses");
        }

        // Luminance and contrast.
        Expect(Math.Abs(TurnPalette.Luminance(1f, 1f, 1f) - 1.0) < 1e-9, "white has luminance 1");
        Expect(TurnPalette.Luminance(0f, 0f, 0f) == 0.0, "black has luminance 0");
        Expect(TurnPalette.Contrast(0.2, 0.7) == TurnPalette.Contrast(0.7, 0.2), "contrast is symmetric");

        // Idea 4: the fade. Blend endpoints are exact, t clamps, and each channel moves monotonically
        // toward its target across every ordered pair of turn colours.
        string[] colourNames = { "sage", "honey", "sky", "rose" };
        for (int i = 0; i < TurnPalette.Count; i++)
        {
            for (int j = 0; j < TurnPalette.Count; j++)
            {
                if (i == j) continue;
                float[] from = TurnPalette.Rgb(i);
                float[] to = TurnPalette.Rgb(j);
                float[] at0 = TurnPalette.Blend(from, to, 0f);
                float[] at1 = TurnPalette.Blend(from, to, 1f);
                string pair = colourNames[i] + " to " + colourNames[j];
                Expect(at0[0] == from[0] && at0[1] == from[1] && at0[2] == from[2], pair + " starts exactly at from");
                Expect(at1[0] == to[0] && at1[1] == to[1] && at1[2] == to[2], pair + " ends exactly at to");
                Expect(TurnPalette.Blend(from, to, -0.5f)[0] == from[0], pair + " clamps t below 0 to from");
                Expect(TurnPalette.Blend(from, to, 3f)[2] == to[2], pair + " clamps t above 1 to to");

                float[] previous = TurnPalette.Blend(from, to, 0f);
                for (int step = 1; step <= 20; step++)
                {
                    float[] now = TurnPalette.Blend(from, to, step * 0.05f);
                    for (int c = 0; c < 3; c++)
                    {
                        bool up = to[c] >= from[c];
                        Expect(up ? now[c] >= previous[c] - 1e-6f : now[c] <= previous[c] + 1e-6f,
                            pair + " channel " + c + " moves monotonically at step " + step);
                        Expect(now[c] >= 0f && now[c] <= 1f, pair + " channel " + c + " stays in 0..1");
                    }
                    previous = now;
                }
            }
        }
        Expect(TurnPalette.Ease(0.5f) == 0.5f, "smoothstep is symmetric at one half");
        Expect(TurnPalette.Ease(float.NaN) == 0f, "easing NaN gives the start");

        // The fade length is short: long enough to see, short enough not to lag.
        Expect(TurnPalette.FadeSeconds >= 0.1f && TurnPalette.FadeSeconds <= 0.3f, "FadeSeconds is between 0.1 and 0.3");

        // Source check: TintDisc assigns the target at once under reduced motion (not gentle), before any fade starts.
        string turnsSource = Script("Unity/TreePlay.Turns.cs");
        int tint = turnsSource.IndexOf("private void TintDisc(", StringComparison.Ordinal);
        Expect(tint >= 0, "TintDisc exists in TreePlay.Turns.cs");
        string body = tint >= 0 ? turnsSource.Substring(tint, Math.Min(600, turnsSource.Length - tint)) : "";
        int gate = body.IndexOf("if (!gentle", StringComparison.Ordinal);
        int instant = body.IndexOf("disc.color = target;", StringComparison.Ordinal);
        int fade = body.IndexOf("StartCoroutine(", StringComparison.Ordinal);
        Expect(gate >= 0 && instant > gate && fade > instant,
            "TintDisc sets the target at once when not gentle, before it starts a fade");
        Expect(treePlayUsesTint(), "PaintTurn tints the token through TintDisc");

        // Source check: TreePlay uses the palette and not the removed fields.
        string treePlay = Script("Unity/TreePlay.cs");
        Expect(treePlay.Contains("TurnPalette.Rgb("), "TreePlay reads colours from TurnPalette");
        Expect(!treePlay.Contains("TurnSage"), "TreePlay no longer has the old TurnSage field");

        // Idea 3: each seat's pip wears its own ink on the card, darkened only as far as it needs to read.
        float[] pipCard = ParseCardFill(File.ReadAllText(ThemePath()));
        double pipCardLum = TurnPalette.Luminance(pipCard[0], pipCard[1], pipCard[2]);
        string pipLine = "Pip inks on CardFill (WCAG ratio, opaque; pips are never faded):";
        for (int t = 0; t < TurnPalette.Count; t++)
        {
            float[] ink = TurnPalette.PipInk(t, pipCard);
            double ratio = TurnPalette.Contrast(TurnPalette.Luminance(ink[0], ink[1], ink[2]), pipCardLum);
            Expect(ratio >= TurnPalette.PipContrast, "pip ink " + t + " reaches 3:1 on CardFill");

            pipLine += " seat " + t + " " + Rgb3(ink) + " " + ratio.ToString("0.00", CultureInfo.InvariantCulture) + ";";

            // Hue: every channel pair above 0.01 keeps the seat colour's ratio.
            float[] seat = TurnPalette.Rgb(t);
            for (int a = 0; a < 3; a++)
            {
                for (int b = a + 1; b < 3; b++)
                {
                    if (seat[a] <= 0.01f || seat[b] <= 0.01f) continue;
                    double want = (double)seat[a] / seat[b];
                    double got = (double)ink[a] / ink[b];
                    Expect(Math.Abs(got - want) < 1e-4, "pip ink " + t + " keeps the hue ratio of channels " + a + " and " + b);
                }
            }

            // Never all-zero, on the card or on black.
            Expect(ink[0] + ink[1] + ink[2] > 0f, "pip ink " + t + " is never all-zero");
            float[] onBlack = TurnPalette.PipInk(t, new float[] { 0f, 0f, 0f });
            Expect(onBlack[0] + onBlack[1] + onBlack[2] > 0f, "pip ink " + t + " is never all-zero on black");
        }
        Console.WriteLine(pipLine);

        // A colour already at 3:1 is returned unchanged (sage on the card, and on black).
        float[] sageInk = TurnPalette.PipInk(0, pipCard);
        float[] sageSeat = TurnPalette.Rgb(0);
        Expect(sageInk[0] == sageSeat[0] && sageInk[1] == sageSeat[1] && sageInk[2] == sageSeat[2],
            "sage is already 3:1 on CardFill, so it is returned unchanged");
        float[] sageBlack = TurnPalette.PipInk(0, new float[] { 0f, 0f, 0f });
        Expect(sageBlack[0] == sageSeat[0] && sageBlack[1] == sageSeat[1] && sageBlack[2] == sageSeat[2],
            "sage is already 3:1 on black, so it is returned unchanged");

        // Wrap-around, like Rgb.
        float[] pipWrap5 = TurnPalette.PipInk(5, pipCard);
        float[] pipWrap1 = TurnPalette.PipInk(1, pipCard);
        Expect(pipWrap5[0] == pipWrap1[0] && pipWrap5[1] == pipWrap1[1] && pipWrap5[2] == pipWrap1[2], "PipInk(5) equals PipInk(1)");
        float[] pipWrapMinus = TurnPalette.PipInk(-1, pipCard);
        float[] pipWrap3 = TurnPalette.PipInk(3, pipCard);
        Expect(pipWrapMinus[0] == pipWrap3[0] && pipWrapMinus[1] == pipWrap3[1] && pipWrapMinus[2] == pipWrap3[2], "PipInk(-1) equals PipInk(3)");

        // The background is not mutated, and results are fresh arrays.
        float[] pipBefore = { pipCard[0], pipCard[1], pipCard[2] };
        float[] pipOut = TurnPalette.PipInk(1, pipCard);
        Expect(pipCard[0] == pipBefore[0] && pipCard[1] == pipBefore[1] && pipCard[2] == pipBefore[2], "PipInk does not mutate its background");
        pipOut[0] = 0f;
        Expect(TurnPalette.PipInk(1, pipCard)[0] > 0f, "PipInk returns a fresh array");

        // Bad backgrounds throw ArgumentException.
        Expect(ThrowsArgument(() => TurnPalette.PipInk(0, null)), "null background throws ArgumentException");
        Expect(ThrowsArgument(() => TurnPalette.PipInk(0, new float[] { 1f, 1f })), "two-channel background throws");
        Expect(ThrowsArgument(() => TurnPalette.PipInk(0, new float[] { 1f, 1f, 1f, 1f })), "four-channel background throws");
        Expect(TurnPalette.PipContrast == 3.0, "PipContrast is 3:1");

        // Source checks: TreePlay paints pips with PipInk; TableSolved no longer uses EarthInk for them.
        Expect(treePlay.Contains("PipInk"), "TreePlay.cs paints its pips with TurnPalette.PipInk");
        int solvedStart = turnsSource.IndexOf("private void TableSolved()", StringComparison.Ordinal);
        int solvedEnd = turnsSource.IndexOf("Idea 5: the hint token", solvedStart < 0 ? 0 : solvedStart, StringComparison.Ordinal);
        Expect(solvedStart >= 0 && solvedEnd > solvedStart, "TableSolved exists in TreePlay.Turns.cs");
        string tableSolved = solvedStart >= 0 && solvedEnd > solvedStart ? turnsSource.Substring(solvedStart, solvedEnd - solvedStart) : "";
        Expect(!tableSolved.Contains("EarthInk"), "TableSolved no longer paints pips with EarthInk");
        Expect(tableSolved.Contains("PipInks["), "TableSolved paints pips in their own seat ink");

        return assertions;
    }

    private static string Rgb3(float[] c)
    {
        return "(" + c[0].ToString("0.000", CultureInfo.InvariantCulture) + ", " +
            c[1].ToString("0.000", CultureInfo.InvariantCulture) + ", " +
            c[2].ToString("0.000", CultureInfo.InvariantCulture) + ")";
    }

    private static bool ThrowsArgument(Func<float[]> call)
    {
        try
        {
            call();
            return false;
        }
        catch (ArgumentException)
        {
            return true;
        }
    }

    private static float[] ParseCardFill(string source)
    {
        Match m = Regex.Match(source, @"Color CardFill = Hex\(""#([0-9A-Fa-f]{6})""\)");
        Expect(m.Success, "CardFill is defined in MosseloraTheme.cs");
        string hex = m.Groups[1].Value;
        return new float[]
        {
            int.Parse(hex.Substring(0, 2), NumberStyles.HexNumber) / 255f,
            int.Parse(hex.Substring(2, 2), NumberStyles.HexNumber) / 255f,
            int.Parse(hex.Substring(4, 2), NumberStyles.HexNumber) / 255f,
        };
    }

    private static string ThemePath()
    {
        foreach (string start in new[] { Directory.GetCurrentDirectory(), AppContext.BaseDirectory })
        {
            string dir = Path.GetFullPath(start);
            while (dir != null)
            {
                foreach (string rel in new[] { "games/evergrove/Assets/Scripts/Unity", "Assets/Scripts/Unity" })
                {
                    string candidate = Path.Combine(dir, rel, "MosseloraTheme.cs");
                    if (File.Exists(candidate)) return candidate;
                }
                dir = Path.GetDirectoryName(dir);
            }
        }
        throw new Exception("TurnPaletteTests could not locate MosseloraTheme.cs.");
    }

    private static bool treePlayUsesTint()
    {
        return Script("Unity/TreePlay.cs").Contains("TintDisc(disc, !together");
    }

    private static string Script(string relative)
    {
        foreach (string start in new[] { Directory.GetCurrentDirectory(), AppContext.BaseDirectory })
        {
            for (var dir = new DirectoryInfo(start); dir != null; dir = dir.Parent)
            {
                string path = Path.Combine(dir.FullName, "games", "evergrove", "Assets", "Scripts", relative);
                if (File.Exists(path)) return File.ReadAllText(path);
            }
        }
        throw new Exception("TurnPaletteTests: " + relative + " not found from " + Directory.GetCurrentDirectory());
    }
}
