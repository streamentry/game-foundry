using System;

namespace Evergrove.Core
{
    /// <summary>
    /// One genuinely changed watercourse after every village choice.
    /// Board topology, endpoint positions and one immutable environmental
    /// landmark depend on the prior choice. Rotational symmetry preserves
    /// the original board's unique solution and low content-authoring cost.
    /// This is a limited deterministic system, NOT a full ecosystem simulation.
    /// </summary>
    public static class ReactiveWatercourses
    {
        // Zero is the unchanged control / legacy board; 1..3 represent
        // tree, bridge and meadow decisions from the previous chapter.
        public static Puzzle ForChapter(int chapter, int variant)
        {
            Puzzle source = PuzzleCatalog.Get(chapter);
            if (variant == 0) return source;
            if (variant < 1 || variant > 3 || chapter == 0)
                throw new ArgumentOutOfRangeException(nameof(variant));

            int turns = variant;
            int size = source.Size;
            PipeKind[] transformedKinds = new PipeKind[source.CellCount];
            int[] initial = new int[source.CellCount];
            int[] solution = new int[source.CellCount];
            for (int i = 0; i < source.CellCount; i++)
            {
                int row = i / size, column = i % size;
                for (int turn = 0; turn < turns; turn++)
                {
                    int formerRow = row;
                    row = column;
                    column = size - 1 - formerRow;
                }
                int newIndex = row * size + column;
                transformedKinds[newIndex] = source.Kinds[i];
                initial[newIndex] = RotatedOrientation(source.Kinds[i], source.Initial[i], turns);
                solution[newIndex] = RotatedOrientation(source.Kinds[i], source.Solution[i], turns);
            }

            int chosenTile = -1;
            int offset = (chapter * 7 + variant * 5) % source.CellCount;
            for (int step = 0; step < source.CellCount; step++)
            {
                int candidate = (offset + step) % source.CellCount;
                if (transformedKinds[candidate] != PipeKind.End)
                {
                    chosenTile = candidate;
                    break;
                }
            }
            if (chosenTile < 0) throw new InvalidOperationException("Cannot anchor an endpoint-only board.");
            initial[chosenTile] = solution[chosenTile];

            char[] layout = new char[source.CellCount];
            for (int i = 0; i < layout.Length; i++)
                layout[i] = transformedKinds[i] == PipeKind.End ? 'E' :
                    transformedKinds[i] == PipeKind.Straight ? 'I' : 'L';
            string[] ruleNames = { "", "Living roots", "Bridge foundation", "Wildflower sanctuary" };
            return new Puzzle(
                source.Name + " · " + ruleNames[variant],
                new string(layout),
                initial,
                solution,
                chosenTile,
                ruleNames[variant] + " preserve one connection. That tile cannot rotate.");
        }

        private static int RotatedOrientation(PipeKind kind, int orientation, int turns)
        {
            for (int candidate = 1; candidate <= PipeRules.OrientationCount(kind); candidate++)
            {
                bool matches = true;
                for (int dir = 0; dir < 4; dir++)
                {
                    int from = (dir - turns + 4) % 4;
                    if (PipeRules.HasPort(kind, orientation, from) !=
                        PipeRules.HasPort(kind, candidate, dir))
                    {
                        matches = false;
                        break;
                    }
                }
                if (matches) return candidate;
            }
            throw new InvalidOperationException("Unable to rotate an authored channel.");
        }

        public static int VariantAfterChoice(VillageChoice decision)
        {
            int variant = (int)decision + 1;
            if (variant < 1 || variant > 3)
                throw new ArgumentOutOfRangeException(nameof(decision));
            return variant;
        }
    }
}
