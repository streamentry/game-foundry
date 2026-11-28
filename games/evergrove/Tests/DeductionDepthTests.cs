using System;
using System.Text;
using Evergrove.Core;

/// <summary>
/// Pins the forced-move measure: deterministic, bounded by rotatable tiles, never removes the
/// true solution, invariant under board rotation when the landmark is ignored, and reported by
/// PuzzleDiagnostics.
/// </summary>
internal static class DeductionDepthTests
{
    private static int assertions;

    private static void Expect(bool ok, string message)
    {
        assertions++;
        if (!ok) throw new Exception("DeductionDepthTests failed: " + message);
    }

    public static int Run()
    {
        assertions = 0;
        for (int chapter = 0; chapter < PuzzleCatalog.Count; chapter++)
        {
            int maxVariant = chapter == 0 ? 0 : 3;
            int baseForced = DeductionDepth.ForcedMoves(ReactiveWatercourses.ForChapter(chapter, 0), false);
            for (int variant = 0; variant <= maxVariant; variant++)
            {
                Puzzle puzzle = ReactiveWatercourses.ForChapter(chapter, variant);
                int forced = DeductionDepth.ForcedMoves(puzzle);
                int rotatable = DeductionDepth.RotatableTiles(puzzle);
                Expect(forced == DeductionDepth.ForcedMoves(puzzle), "forced count is deterministic");
                Expect(rotatable == DeductionDepth.RotatableTiles(puzzle), "rotatable count is deterministic");
                Expect(forced >= 0 && forced <= rotatable, "forced count is bounded by rotatable tiles");

                bool[][] candidates = DeductionDepth.Candidates(puzzle);
                for (int i = 0; i < puzzle.CellCount; i++)
                    Expect(candidates[i][puzzle.Solution[i]], "propagation keeps the solution orientation");

                Expect(DeductionDepth.ForcedMoves(puzzle, false) == baseForced,
                    "rotated board matches base board with anchor ignored");

                Expect(PuzzleDiagnostics.Analyze(chapter, variant).forcedMoveCount == forced,
                    "diagnostics report the same forced count");
            }
        }
        return assertions;
    }

    /// <summary>Forced count of variant 0 of every chapter, one line per chapter (chapter index is 0-based).</summary>
    public static string Curve()
    {
        var sb = new StringBuilder();
        for (int chapter = 0; chapter < PuzzleCatalog.Count; chapter++)
        {
            Puzzle puzzle = ReactiveWatercourses.ForChapter(chapter, 0);
            if (chapter > 0) sb.Append('\n');
            sb.Append("chapter ").Append(chapter).Append(" variant 0: forced ")
              .Append(DeductionDepth.ForcedMoves(puzzle)).Append(" of ")
              .Append(DeductionDepth.RotatableTiles(puzzle));
        }
        return sb.ToString();
    }
}
