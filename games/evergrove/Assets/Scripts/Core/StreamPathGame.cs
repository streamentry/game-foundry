using System;

namespace Evergrove.Core
{
    /// <summary>
    /// The puzzle water meadows hold (depth spine, Layer 2): lead the stream.
    /// Draw one unbroken stream from the spring through every open field,
    /// each exactly once, and let it end in the pond. Rocks block the way.
    /// Every meadow has exactly one way through, so it is a puzzle of
    /// reasoning, never of luck. No timer, no limit on tries; tapping a field
    /// already on the stream pulls the stream back to it. Dealt from a seed.
    /// </summary>
    public sealed class StreamPathGame
    {
        /// <summary>Ignored: nothing happened. Extended: one field further. Trimmed: pulled back. TooSoon: the pond waits until every field is watered.</summary>
        public enum PathMove { Ignored, Extended, Trimmed, TooSoon }

        /// <summary>Meadow size by tier: 4, 5, 6.</summary>
        public static int SizeFor(int tier)
        {
            if (tier < 0 || tier >= TreePuzzles.Tiers) throw new ArgumentOutOfRangeException(nameof(tier));
            return 4 + tier;
        }

        private static int MinRocks(int tier) { return tier == 0 ? 0 : tier == 1 ? 1 : 2; }
        private static int MaxRocks(int tier) { return tier == 0 ? 1 : tier == 1 ? 3 : 4; }

        private const int MaxAttempts = 20000;
        private const int BuildBudget = 300;
        private const long CountBudget = 2000000;

        private readonly bool[] rock;
        private readonly int[] solution;
        private readonly int[] path;
        private int length;
        private readonly int[] scratchStack;
        private readonly bool[] scratchSeen;
        private readonly int[] scratchNb = new int[4];
        private readonly bool[] onPath;

        public int Size { get; private set; }
        public int Spring { get; private set; }
        public int Pond { get; private set; }
        public int OpenCount { get; private set; }
        public int Moves { get; private set; }
        public bool Solved { get; private set; }

        public StreamPathGame(int seed, int tier)
        {
            Size = SizeFor(tier);
            int n = Size * Size;
            rock = new bool[n];
            onPath = new bool[n];
            scratchStack = new int[n];
            scratchSeen = new bool[n];
            uint state = unchecked((uint)seed * 0x9E3779B1u + (uint)tier * 0x85EBCA6Bu + 0x27D4EB2Fu);
            if (state == 0) state = 0x6d2b79f5u;

            int[] found = null;
            for (int attempt = 0; attempt < MaxAttempts && found == null; attempt++)
            {
                Array.Clear(rock, 0, n);
                int rocks = MinRocks(tier) + (int)(Next(ref state) % (uint)(MaxRocks(tier) - MinRocks(tier) + 1));
                for (int placed = 0; placed < rocks;)
                {
                    int c = (int)(Next(ref state) % (uint)n);
                    if (!rock[c]) { rock[c] = true; placed++; }
                }
                OpenCount = n - rocks;
                int spring;
                do { spring = (int)(Next(ref state) % (uint)n); } while (rock[spring]);
                if (!Connected(spring)) continue;
                int[] candidate = BuildPath(spring, ref state);
                if (candidate == null) continue;
                if (CountPaths(candidate[0], candidate[candidate.Length - 1]) != 1) continue;
                found = candidate;
            }
            if (found == null) throw new InvalidOperationException("no unique meadow found for seed " + seed);
            solution = found;
            Spring = solution[0];
            Pond = solution[solution.Length - 1];
            path = new int[OpenCount];
            path[0] = Spring;
            length = 1;
            onPath[Spring] = true;
        }

        private static uint Next(ref uint state)
        {
            state ^= state << 13;
            state ^= state >> 17;
            state ^= state << 5;
            return state;
        }

        // Orthogonal neighbours of a cell, written into buffer; returns the count.
        private int Neighbours(int cell, int[] buffer)
        {
            int r = cell / Size, c = cell % Size, k = 0;
            if (r > 0) buffer[k++] = cell - Size;
            if (c > 0) buffer[k++] = cell - 1;
            if (c < Size - 1) buffer[k++] = cell + 1;
            if (r < Size - 1) buffer[k++] = cell + Size;
            return k;
        }

        private bool Connected(int start)
        {
            var seen = new bool[rock.Length];
            var stack = new int[rock.Length];
            var nb = new int[4];
            int top = 0, count = 0;
            stack[top++] = start; seen[start] = true;
            while (top > 0)
            {
                int cell = stack[--top];
                count++;
                int k = Neighbours(cell, nb);
                for (int i = 0; i < k; i++)
                    if (!rock[nb[i]] && !seen[nb[i]]) { seen[nb[i]] = true; stack[top++] = nb[i]; }
            }
            return count == OpenCount;
        }

        // Randomized depth-first Hamiltonian path; fewest onward moves first (Warnsdorff), ties shuffled.
        private int[] BuildPath(int spring, ref uint state)
        {
            var visited = new bool[rock.Length];
            var order = new int[OpenCount];
            int budget = BuildBudget;
            order[0] = spring; visited[spring] = true;
            return Extend(order, 1, visited, ref state, ref budget) ? order : null;
        }

        private bool Extend(int[] order, int depth, bool[] visited, ref uint state, ref int budget)
        {
            if (depth == OpenCount) return true;
            if (--budget < 0) return false;
            var nb = new int[4];
            var nn = new int[4];
            var options = new int[4];
            var degree = new int[4];
            int count = 0;
            int k = Neighbours(order[depth - 1], nb);
            for (int i = 0; i < k; i++)
            {
                int cell = nb[i];
                if (rock[cell] || visited[cell]) continue;
                int onward = 0;
                int m = Neighbours(cell, nn);
                for (int j = 0; j < m; j++) if (!rock[nn[j]] && !visited[nn[j]]) onward++;
                options[count] = cell;
                degree[count] = onward * 8 + (int)(Next(ref state) & 7); // random tie-break
                count++;
            }
            for (int a = 1; a < count; a++)
                for (int b = a; b > 0 && degree[b] < degree[b - 1]; b--)
                {
                    int t = degree[b]; degree[b] = degree[b - 1]; degree[b - 1] = t;
                    t = options[b]; options[b] = options[b - 1]; options[b - 1] = t;
                }
            for (int i = 0; i < count; i++)
            {
                order[depth] = options[i];
                visited[options[i]] = true;
                if (Extend(order, depth + 1, visited, ref state, ref budget)) return true;
                visited[options[i]] = false;
                if (budget < 0) return false;
            }
            return false;
        }

        // Number of Hamiltonian paths spring→pond over the open cells, stopping at 2
        // (a budget overrun also reports 2, which rejects the meadow).
        private int CountPaths(int spring, int pond)
        {
            var visited = new bool[rock.Length];
            visited[spring] = true;
            long budget = CountBudget;
            int result = Count(spring, 1, pond, visited, ref budget);
            return budget < 0 ? 2 : result;
        }

        private int Count(int head, int depth, int pond, bool[] visited, ref long budget)
        {
            if (--budget < 0) return 2;
            if (depth == OpenCount) return head == pond ? 1 : 0;
            if (head == pond) return 0;
            if (!Viable(head, pond, visited)) return 0;
            var nb = new int[4];
            int k = Neighbours(head, nb);
            int total = 0;
            for (int i = 0; i < k && total < 2; i++)
            {
                int cell = nb[i];
                if (rock[cell] || visited[cell]) continue;
                if (cell == pond && depth + 1 != OpenCount) continue;
                visited[cell] = true;
                total += Count(cell, depth + 1, pond, visited, ref budget);
                visited[cell] = false;
                if (budget < 0) return 2;
            }
            return Math.Min(total, 2);
        }

        // Pruning: every unvisited field must still be reachable from the head, and
        // must keep enough free neighbours (the pond needs 1 way in, others need 2).
        private bool Viable(int head, int pond, bool[] visited)
        {
            var seen = scratchSeen;
            var stack = scratchStack;
            var nb = scratchNb;
            Array.Clear(seen, 0, seen.Length);
            int top = 0, reached = 0, remaining = 0;
            for (int c = 0; c < rock.Length; c++) if (!rock[c] && !visited[c]) remaining++;
            stack[top++] = head; seen[head] = true;
            while (top > 0)
            {
                int cell = stack[--top];
                int k = Neighbours(cell, nb);
                for (int i = 0; i < k; i++)
                {
                    int o = nb[i];
                    if (rock[o] || visited[o] || seen[o]) continue;
                    seen[o] = true; reached++; stack[top++] = o;
                }
            }
            if (reached != remaining) return false;
            if (remaining <= 1) return true;
            for (int c = 0; c < rock.Length; c++)
            {
                if (rock[c] || visited[c]) continue;
                int free = 0;
                int k = Neighbours(c, nb);
                for (int i = 0; i < k; i++) if (!rock[nb[i]] && (!visited[nb[i]] || nb[i] == head)) free++;
                if (free < (c == pond ? 1 : 2)) return false;
            }
            return true;
        }

        public bool IsRock(int cell) { return cell >= 0 && cell < rock.Length && rock[cell]; }

        /// <summary>A copy of the current stream, starting at the spring.</summary>
        public int[] Path
        {
            get
            {
                var copy = new int[length];
                Array.Copy(path, copy, length);
                return copy;
            }
        }

        public int PathLength { get { return length; } }
        public bool OnPath(int cell) { return cell >= 0 && cell < onPath.Length && onPath[cell]; }

        public PathMove Step(int cell)
        {
            if (Solved || cell < 0 || cell >= rock.Length || rock[cell]) return PathMove.Ignored;
            int head = path[length - 1];
            if (onPath[cell])
            {
                if (cell == head) return PathMove.Ignored;
                while (path[length - 1] != cell) onPath[path[--length]] = false;
                Moves++;
                return PathMove.Trimmed;
            }
            int dr = Math.Abs(cell / Size - head / Size), dc = Math.Abs(cell % Size - head % Size);
            if (dr + dc != 1) return PathMove.Ignored;
            if (cell == Pond && length + 1 != OpenCount) return PathMove.TooSoon;
            path[length++] = cell;
            onPath[cell] = true;
            Moves++;
            Solved = length == OpenCount && cell == Pond;
            return PathMove.Extended;
        }

        /// <summary>
        /// The next hint. If the stream so far follows the solution, the next field of the
        /// solution (-1 when already complete). If it strays, the last correct field: tap
        /// it to pull the stream back to where it was still right.
        /// </summary>
        public int NextHint()
        {
            int k = 0;
            while (k < length && path[k] == solution[k]) k++;
            if (k == length) return length < solution.Length ? solution[length] : -1;
            return solution[k - 1];
        }

        public int Solution(int index) { return solution[index]; }
        public int[] SolutionPath() { return (int[])solution.Clone(); }
    }
}
