using System;

namespace Evergrove.Core
{
    /// <summary>
    /// Source-derived facts about an authored board presentation.
    /// Human difficulty, timing and enjoyment must be measured separately.
    /// </summary>
    [Serializable]
    public sealed class PuzzleProfile
    {
        public string id;
        public int chapter;
        public int variant;
        public string name;
        public int size;
        public string tileKinds;
        public int[] initialOrientations;
        public int[] solutionOrientations;
        public int? anchoredTile;
        public string ecologicalConstraint;
        public int solutionCountAtMostTwo;
        public int minimumClockwiseTaps;
        public int initiallyMisalignedTiles;
        public int startingConnectedTiles;
        public int startingLeaks;
        public int? forcedMoveCount;
        public double? observedMedianSeconds;
        public double? observedP90Seconds;
        public double? observedHintRate;
        public string humanReviewer;
        public string reviewDate;
    }

    public static class PuzzleDiagnostics
    {
        public static PuzzleProfile Analyze(int chapter, int variant)
        {
            if (chapter < 0 || chapter >= PuzzleCatalog.Count ||
                variant < 0 || variant > 3 || (chapter == 0 && variant != 0))
                throw new ArgumentOutOfRangeException(nameof(chapter));
            Puzzle puzzle = ReactiveWatercourses.ForChapter(chapter, variant);
            if (!PipeRules.ValidRotations(puzzle, puzzle.Initial))
                throw new InvalidOperationException("Broken source puzzle rotations.");
            int taps = 0, misaligned = 0;
            var tileKinds = new char[puzzle.CellCount];
            for (int i = 0; i < puzzle.CellCount; i++)
            {
                PipeKind kind = puzzle.Kinds[i];
                tileKinds[i] = kind == PipeKind.End ? 'E' :
                    kind == PipeKind.Straight ? 'I' : 'L';
                int distance = (puzzle.Solution[i] - puzzle.Initial[i] +
                    PipeRules.OrientationCount(kind)) % PipeRules.OrientationCount(kind);
                taps += distance;
                if (distance > 0) misaligned++;
                if (puzzle.IsAnchored(i) && distance != 0)
                    throw new InvalidOperationException("Protected tile cannot require a turn.");
            }
            PipeInsight initial = PipeRules.Inspect(puzzle, puzzle.Initial);
            string suffix = variant == 0 ? "base" : variant == 1 ? "tree" :
                variant == 2 ? "bridge" : "flowers";
            return new PuzzleProfile {
                id = "stream-" + (chapter + 1).ToString("D2",
                    System.Globalization.CultureInfo.InvariantCulture) + "-" + suffix,
                chapter = chapter + 1,
                variant = variant,
                name = puzzle.Name,
                size = puzzle.Size,
                tileKinds = new string(tileKinds),
                initialOrientations = (int[])puzzle.Initial.Clone(),
                solutionOrientations = (int[])puzzle.Solution.Clone(),
                anchoredTile = puzzle.AnchoredTile < 0 ? (int?)null : puzzle.AnchoredTile,
                ecologicalConstraint = variant == 0 ? null : puzzle.EnvironmentalRule,
                solutionCountAtMostTwo = PipeRules.CountSolutions(puzzle, 2),
                minimumClockwiseTaps = taps,
                initiallyMisalignedTiles = misaligned,
                startingConnectedTiles = initial.ConnectedCount,
                startingLeaks = initial.LeakCount,
                forcedMoveCount = null,
                observedMedianSeconds = null,
                observedP90Seconds = null,
                observedHintRate = null,
                humanReviewer = null,
                reviewDate = null
            };
        }

        public static PuzzleProfile[] All()
        {
            var profiles = new PuzzleProfile[12 + 11 * 3];
            int next = 0;
            for (int chapter = 0; chapter < PuzzleCatalog.Count; chapter++)
            {
                profiles[next++] = Analyze(chapter, 0);
                if (chapter == 0) continue;
                for (int variant = 1; variant <= 3; variant++)
                    profiles[next++] = Analyze(chapter, variant);
            }
            return profiles;
        }
    }
}
