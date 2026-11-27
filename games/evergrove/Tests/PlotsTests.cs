using System;
using System.Collections.Generic;
using Evergrove.Core;

/// <summary>Garden plots, the puzzle gardens hold (depth spine, Layer 2): divide the field into numbered beds.</summary>
internal static class PlotsTests
{
    private static int assertions;

    private static void Expect(bool ok, string message)
    {
        assertions++;
        if (!ok) throw new Exception("Plots test failed: " + message);
    }

    private static string Key(PlotsGame g)
    {
        var parts = new List<string>();
        for (int c = 0; c < g.Size * g.Size; c++) parts.Add(g.Clue(c).ToString());
        return string.Join(",", parts);
    }

    private static void Place(PlotsGame g, int[] r)
    {
        int from = r[1] * g.Size + r[0];
        int to = (r[1] + r[3] - 1) * g.Size + r[0] + r[2] - 1;
        Expect(g.Fits(from, to), "hint bed fits");
        Expect(g.Draw(from, to) == PlotMove.Placed, "hint bed placed");
    }

    public static int Run()
    {
        assertions = 0;
        Expect(PlotsGame.SizeFor(0) == 5 && PlotsGame.SizeFor(1) == 6 && PlotsGame.SizeFor(2) == 7, "5, 6, 7 by tier");
        bool threw = false;
        try { PlotsGame.SizeFor(3); } catch (ArgumentOutOfRangeException) { threw = true; }
        Expect(threw, "unknown tier refused");

        for (int tier = 0; tier < TreePuzzles.Tiers; tier++)
        {
            var keys = new HashSet<string>();
            int singles = 0;
            for (int seed = 0; seed < 200; seed++)
            {
                var g = new PlotsGame(seed, tier);
                var again = new PlotsGame(seed, tier);
                Expect(Key(g) == Key(again), "same seed deals the same field");
                keys.Add(Key(g));
                int n = g.Size * g.Size, sum = 0;
                for (int c = 0; c < n; c++) sum += g.Clue(c);
                Expect(sum == n, "clues sum to the field area");
                Expect(g.Markers.Length > 0, "has markers");
                foreach (int m in g.Markers)
                {
                    Expect(g.Clue(m) >= 1 && g.Clue(m) <= 8, "clue area in range");
                    if (g.Clue(m) == 1) singles++;
                }
                if (tier == 0) Expect(g.SolvableByDeduction, "tier 0 falls to plain deduction");

                // The generator accepts only fields with exactly one answer; the hints walk to it.
                Expect(!g.Solved && g.BedCount == 0 && g.Moves == 0, "starts open");
                int guard = 0;
                while (!g.Solved && guard++ < 100) Place(g, g.HintBed());
                Expect(g.Solved, "hints solve it");
                Expect(g.BedCount == g.Markers.Length, "one bed per marker");
                Expect(g.HintBed().Length == 0, "no hint once solved");
                Expect(g.Moves == g.Markers.Length, "moves count placements");
            }
            Expect(keys.Count > 150, "seeds differ");
            Expect(singles < 200 * 2, "area-1 beds are rare");
        }

        var game = new PlotsGame(7, 1);
        int size = game.Size, total = size * size;
        int first = game.Markers[0];
        Expect(game.Draw(0, total - 1) == PlotMove.Invalid && game.Moves == 0 && game.BedCount == 0, "two markers invalid");
        int empty = -1;
        for (int c = 0; c < total; c++) if (game.Clue(c) == 0) { empty = c; break; }
        Expect(game.Draw(empty, empty) == PlotMove.Invalid, "zero markers invalid");
        Expect(game.Clue(first) > 1 && game.Draw(first, first) == PlotMove.Invalid, "wrong area invalid");
        Expect(!game.Fits(first, first) && game.Draw(-1, 3) == PlotMove.Invalid && game.Moves == 0, "invalid changes nothing");

        int[] hint = game.HintBed();
        Place(game, hint);
        int corner = hint[1] * size + hint[0];
        Expect(game.BedCount == 1 && game.BedAt(corner) == 0 && game.Moves == 1, "bed placed");
        Expect(game.BedLeft(0) == hint[0] && game.BedTop(0) == hint[1] &&
               game.BedWidth(0) == hint[2] && game.BedHeight(0) == hint[3] &&
               game.Clue(game.BedMarker(0)) == hint[2] * hint[3], "bed accessors");
        Expect(game.Draw(corner, corner) == PlotMove.Removed, "tap removes");
        Expect(game.BedCount == 0 && game.BedAt(corner) == -1 && game.Moves == 2 && game.LastReplaced == 0, "bed lifted");

        Place(game, hint);
        Place(game, hint);
        Expect(game.BedCount == 1 && game.LastReplaced == 1, "same bed replaces itself");
        bool replacedOther = false;
        for (int seed = 0; seed < 60 && !replacedOther; seed++)
        {
            var g = new PlotsGame(seed, 2);
            int[] h = g.HintBed();
            Place(g, h);
            for (int a = 0; a < g.Size * g.Size && !replacedOther; a++)
                for (int b = a; b < g.Size * g.Size && !replacedOther; b++)
                {
                    if (!g.Fits(a, b)) continue;
                    int x0 = Math.Min(a % g.Size, b % g.Size), y0 = Math.Min(a / g.Size, b / g.Size);
                    int w = Math.Abs(a % g.Size - b % g.Size) + 1, hh = Math.Abs(a / g.Size - b / g.Size) + 1;
                    bool same = x0 == h[0] && y0 == h[1] && w == h[2] && hh == h[3];
                    bool overlap = x0 <= h[0] + h[2] - 1 && h[0] <= x0 + w - 1 &&
                                   y0 <= h[1] + h[3] - 1 && h[1] <= y0 + hh - 1;
                    if (same || !overlap) continue;
                    Expect(g.Draw(a, b) == PlotMove.Placed && g.LastReplaced == 1 && g.BedCount == 1, "overlap replaced");
                    replacedOther = true;
                }
        }
        Expect(replacedOther, "a different overlapping bed was exercised");

        var probe = new PlotsGame(3, 0);
        for (int a = 0; a < probe.Size * probe.Size; a++)
            for (int b = 0; b < probe.Size * probe.Size; b++)
            {
                var copy = new PlotsGame(3, 0);
                Expect((copy.Draw(a, b) == PlotMove.Placed) == probe.Fits(a, b), "Fits agrees with Draw");
            }

        var gap = new PlotsGame(11, 0);
        int count = gap.Markers.Length;
        for (int i = 0; i < count - 1; i++) Place(gap, gap.HintBed());
        Expect(!gap.Solved && gap.BedCount == count - 1, "a gap means not solved");
        Place(gap, gap.HintBed());
        Expect(gap.Solved, "last bed solves");
        Expect(gap.Draw(gap.Markers[0], gap.Markers[0]) == PlotMove.Removed && !gap.Solved, "lifting a bed reopens it");
        // Independent uniqueness check (integration review): a plain exact-cover
        // count that shares no code with the generator, on 60 seeds per tier.
        for (int tier = 0; tier < 3; tier++)
            for (int seed = 0; seed < 60; seed++)
            {
                var field = new PlotsGame(seed * 31 + 5, tier);
                Expect(CountCovers(field, new int[field.Size * field.Size], 0, 2) == 1,
                    "exactly one way to plant field " + seed + " tier " + tier);
            }
        return assertions;
    }

    // Counts full covers (up to `limit`): the first uncovered cell must be covered by some
    // marker's rectangle of the marker's area containing that cell and exactly one marker.
    private static int CountCovers(PlotsGame g, int[] owner, int found, int limit)
    {
        int n = g.Size, first = -1;
        for (int c = 0; c < n * n; c++) if (owner[c] == 0) { first = c; break; }
        if (first < 0) return found + 1;
        int fr = first / n, fc = first % n;
        foreach (int marker in g.Markers)
        {
            int area = g.Clue(marker);
            for (int w = 1; w <= area; w++)
            {
                if (area % w != 0) continue;
                int h = area / w;
                for (int top = fr - h + 1; top <= fr; top++)
                    for (int left = fc - w + 1; left <= fc; left++)
                    {
                        if (top < 0 || left < 0 || top + h > n || left + w > n) continue;
                        // `first` is the first free cell in reading order, so it must be the rectangle's top-left
                        if (top != fr || left != fc) continue;
                        bool ok = true; int markers = 0, mine = 0;
                        for (int r = top; r < top + h && ok; r++)
                            for (int c = left; c < left + w; c++)
                            {
                                int cell = r * n + c;
                                if (owner[cell] != 0) { ok = false; break; }
                                if (g.Clue(cell) > 0) { markers++; if (cell == marker) mine++; }
                            }
                        if (!ok || markers != 1 || mine != 1) continue;
                        for (int r = top; r < top + h; r++) for (int c = left; c < left + w; c++) owner[r * n + c] = marker + 1;
                        found = CountCovers(g, owner, found, limit);
                        for (int r = top; r < top + h; r++) for (int c = left; c < left + w; c++) owner[r * n + c] = 0;
                        if (found >= limit) return found;
                    }
            }
        }
        return found;
    }
}
