using System;
using System.IO;
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

        return assertions;
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
