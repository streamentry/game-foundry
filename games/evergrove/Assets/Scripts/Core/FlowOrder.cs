using System.Collections.Generic;

namespace Evergrove.Core
{
    /// <summary>
    /// Orders tiles by how far water travels from the spring along the pipes the player joined.
    /// Pure and deterministic: the inputs are read, never changed.
    /// </summary>
    public static class FlowOrder
    {
        /// <summary>
        /// Steps from the spring (the first End in row-major order) along joined pipes, where a
        /// joined edge needs facing ports on both tiles. Spring = 0, unreached = -1. Null, wrong-length
        /// or invalid rotations give all -1 of length puzzle.CellCount.
        /// </summary>
        public static int[] StepsFromSpring(Puzzle puzzle, int[] rotations)
        {
            if (puzzle == null) return new int[0];
            int[] steps = Unreached(puzzle.CellCount);
            if (!PipeRules.ValidRotations(puzzle, rotations)) return steps;

            int spring = -1;
            for (int i = 0; i < puzzle.CellCount && spring < 0; i++)
                if (puzzle.Kinds[i] == PipeKind.End) spring = i;
            if (spring < 0) return steps;

            var queue = new Queue<int>();
            steps[spring] = 0;
            queue.Enqueue(spring);
            while (queue.Count > 0)
            {
                int tile = queue.Dequeue();
                for (int d = 0; d < 4; d++)
                {
                    int next = PipeRules.Neighbor(tile, d, puzzle.Size);
                    if (next < 0 || steps[next] >= 0) continue;
                    if (!IsJoined(puzzle, rotations, tile, next, d)) continue;
                    steps[next] = steps[tile] + 1;
                    queue.Enqueue(next);
                }
            }
            return steps;
        }

        private static bool IsJoined(Puzzle puzzle, int[] rotations, int from, int to, int direction)
        {
            return PipeRules.HasPort(puzzle.Kinds[from], rotations[from], direction) &&
                   PipeRules.HasPort(puzzle.Kinds[to], rotations[to], (direction + 2) % 4);
        }

        private static int[] Unreached(int count)
        {
            var steps = new int[count];
            for (int i = 0; i < count; i++) steps[i] = -1;
            return steps;
        }
    }
}
