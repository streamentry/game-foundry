using System;
using System.Collections.Generic;

namespace Evergrove.Core
{
    /// <summary>
    /// Boards drawn from the player's own garden (checklist item 37). A planting-by-numbers
    /// bed draws a picture the garden has actually grown or bought: each catalogue item
    /// maps to a library picture by name (ItemPictures), and a bed's picture is chosen from
    /// those by seed. Only pictures whose clues have exactly one solution are eligible, so
    /// every garden bed is unique as well as logic-solvable. Deterministic: the same garden
    /// and seed always give the same bed.
    /// </summary>
    public static class OwnGardenBoards
    {
        // Catalogue item name -> library picture name (NonogramPictures). Item names are
        // exact GardenCatalog names: trees, flowers, then things to build or mend. Items with
        // no matching picture simply contribute nothing; a garden with no match at all draws
        // from the general library.
        private static readonly string[][] ItemPictures =
        {
            new[] { "apple tree", "an apple" },
            new[] { "crab apple", "an apple" },
            new[] { "pear tree", "a pear" },
            new[] { "tulip tree", "a tulip" },
            new[] { "linden", "a heart-shaped leaf" },
            new[] { "catalpa", "a heart-shaped leaf" },
            new[] { "tulips", "a tulip" },
            new[] { "sunflowers", "a sun" },
            new[] { "bluebells", "a bell" },
            new[] { "foxgloves", "a bell" },
            new[] { "snowdrops", "a bell" },
            new[] { "lily of the valley", "a bell" },
            new[] { "school bell", "a bell" },
            new[] { "sundial", "a sun" },
            new[] { "cottage roof", "a cottage" },
            new[] { "garden shed", "a cottage" },
            new[] { "tea house", "a cup" },
            new[] { "bird bath", "a bird" },
            new[] { "bird hide", "a bird" },
            new[] { "duck house", "a bird" },
            new[] { "hen house", "a bird" },
            new[] { "dovecote", "a bird" },
            new[] { "lily pond", "a fish" },
            new[] { "frog pond", "a fish" },
            new[] { "potting bench", "a pot with a sprout" },
            new[] { "herb spiral", "a pot with a sprout" },
            new[] { "pottery kiln", "a pot with a sprout" },
            new[] { "raised beds", "a pot with a sprout" },
        };

        private static bool[][] unique; // [tier][library index], computed once

        /// <summary>Library names of the garden's grown or bought things, in the order they were grown (distinct). Empty when none maps.</summary>
        public static string[] MappedPictures(WorldModel world)
        {
            var names = new List<string>();
            if (world == null) return names.ToArray();
            int[] choices = world.Choices;
            for (int i = 0; i < choices.Length; i++)
            {
                if (choices[i] < 0 || choices[i] > 2) continue;
                AddPicture(names, GardenCatalog.For(world.Garden,
                    Math.Min(i, GardenCatalog.ChaptersPerGarden - 1), (VillageChoice)choices[i]));
            }
            int[] bought = world.Bought;
            for (int i = 0; i + 1 < bought.Length; i += 2)
                if (bought[i] >= 0 && bought[i] < Nursery.Kinds) AddPicture(names, Nursery.Item(bought[i]));
            return names.ToArray();
        }

        /// <summary>The picture names this garden draws from: its mapped pictures, or the general library when none maps.</summary>
        public static string[] PicturesFor(WorldModel world)
        {
            string[] mapped = MappedPictures(world);
            return mapped.Length > 0 ? mapped : GeneralPictures();
        }

        /// <summary>The library picture a catalogue item draws, or null when none maps.</summary>
        public static string PictureFor(GardenItem item)
        {
            if (item == null) return null;
            foreach (var pair in ItemPictures)
                if (pair[0] == item.Name) return pair[1];
            return null;
        }

        /// <summary>Every picture name in the library, tier by tier, in library order (distinct).</summary>
        public static string[] GeneralPictures()
        {
            var names = new List<string>();
            for (int tier = 0; tier < TreePuzzles.Tiers; tier++)
                for (int i = 0; i < NonogramPictures.Count(tier); i++)
                    if (!names.Contains(NonogramPictures.Name(tier, i))) names.Add(NonogramPictures.Name(tier, i));
            return names.ToArray();
        }

        /// <summary>
        /// The bed for a garden and seed in a tier. Its picture comes from the garden's mapped
        /// pictures that the tier has and that are unique; the seed picks one and its lap flips it.
        /// If the tier has garden pictures but none is unique, the unique library picture nearest
        /// the first of them is used. If the tier has no garden picture at all, the unique general
        /// library is used.
        /// </summary>
        public static NonogramGame NonogramFor(WorldModel world, int seed, int tier)
        {
            return NonogramFor(MappedPictures(world), seed, tier);
        }

        /// <summary><see cref="NonogramFor(WorldModel, int, int)"/> for a list of picture names (an empty list means the general library).</summary>
        public static NonogramGame NonogramFor(string[] names, int seed, int tier)
        {
            int count = NonogramPictures.Count(tier);
            bool[] uniq = Uniques()[tier];
            List<int> mapped = IndexesOf(names ?? new string[0], tier);
            var pool = new List<int>();
            foreach (int i in mapped) if (uniq[i]) pool.Add(i);
            if (pool.Count == 0 && mapped.Count > 0)
            {
                // Garden pictures exist in this tier but none is unique: the nearest unique picture.
                int anchor = mapped[0], best = -1;
                for (int i = 0; i < count; i++)
                    if (uniq[i] && (best < 0 || Math.Abs(i - anchor) < Math.Abs(best - anchor))) best = i;
                if (best >= 0) pool.Add(best);
            }
            if (pool.Count == 0)
                for (int i = 0; i < count; i++) if (uniq[i]) pool.Add(i);
            if (pool.Count == 0) throw new InvalidOperationException("No unique nonogram picture in tier " + tier + ".");
            long s = seed;
            long lap = FloorDiv(s, pool.Count);
            int pick = (int)(s - lap * pool.Count);
            return BedFor(tier, pool[pick], (lap & 1) != 0);
        }

        /// <summary>True when the garden has a unique picture of its own in this tier (not the general fallback).</summary>
        public static bool HasGardenPicture(WorldModel world, int tier)
        {
            bool[] uniq = Uniques()[tier];
            foreach (int i in IndexesOf(MappedPictures(world), tier)) if (uniq[i]) return true;
            return false;
        }

        /// <summary>
        /// The standard seed whose board a relative can play as the same puzzle (checklist 37): the
        /// standard generator (new NonogramGame(seed, tier)) with identical row and column clues and
        /// solution. Every bed the library can deal is one of the 2 x count standard boards (seeds
        /// 0..2*count-1; a further lap repeats a pose), so that range is searched exhaustively in order.
        /// Returns -1 when no standard seed matches, and the caller then offers no send for that bed.
        /// </summary>
        public static int SharedSeedFor(NonogramGame ownBoard, int tier)
        {
            if (ownBoard == null || tier < 0 || tier >= TreePuzzles.Tiers) return -1;
            int limit = 2 * NonogramPictures.Count(tier);
            for (int seed = 0; seed < limit; seed++)
                if (SameBoard(new NonogramGame(seed, tier), ownBoard)) return seed;
            return -1;
        }

        /// <summary>
        /// The own bed a chosen leaves level deals (item 34): the first seed from the given one whose
        /// own bed measures that level in the level's tier, as DifficultyChoice.DealAt does for standard
        /// beds. If none is found within the search, the seed's own bed is kept, and its leaves are shown.
        /// </summary>
        public static (int seed, int tier) DealAt(WorldModel world, int seed, int level)
        {
            int chosen = DifficultyChoice.Clamp(level);
            int tier = DifficultyChoice.TierFor(PuzzleKind.Nonogram, chosen);
            for (int offset = 0; offset < DealSearchLimit; offset++)
            {
                int candidate = unchecked(seed + offset);
                if (NonogramFor(world, candidate, tier).Leaves == chosen) return (candidate, tier);
            }
            return (seed, tier);
        }

        // Seeds tried after the given one by DealAt (the same bound DifficultyChoice uses).
        private const int DealSearchLimit = 200;

        private static bool SameBoard(NonogramGame a, NonogramGame b)
        {
            if (a.Size != b.Size) return false;
            for (int i = 0; i < a.Size; i++)
                if (!SameClues(a.RowClues[i], b.RowClues[i]) || !SameClues(a.ColumnClues[i], b.ColumnClues[i])) return false;
            for (int cell = 0; cell < a.Size * a.Size; cell++)
                if (a.Solution(cell) != b.Solution(cell)) return false;
            return true;
        }

        private static bool SameClues(int[] a, int[] b)
        {
            if (a.Length != b.Length) return false;
            for (int i = 0; i < a.Length; i++) if (a[i] != b[i]) return false;
            return true;
        }

        /// <summary>Whether a library picture's clues have exactly one solution.</summary>
        public static bool IsUnique(int tier, int index)
        {
            return Uniques()[tier][index];
        }

        /// <summary>How many library pictures are unique, out of all of them.</summary>
        public static int CountUniqueLibrary(out int total)
        {
            int unique = 0;
            total = 0;
            for (int tier = 0; tier < TreePuzzles.Tiers; tier++)
                for (int i = 0; i < NonogramPictures.Count(tier); i++)
                {
                    total++;
                    if (Uniques()[tier][i]) unique++;
                }
            return unique;
        }

        // A bed of a given library picture, by the seed that the library would deal it at.
        private static NonogramGame BedFor(int tier, int index, bool mirrored)
        {
            int count = NonogramPictures.Count(tier);
            return new NonogramGame(index + (mirrored ? count : 0), tier);
        }

        private static bool[][] Uniques()
        {
            if (unique != null) return unique;
            var all = new bool[TreePuzzles.Tiers][];
            for (int tier = 0; tier < TreePuzzles.Tiers; tier++)
            {
                all[tier] = new bool[NonogramPictures.Count(tier)];
                for (int i = 0; i < all[tier].Length; i++)
                {
                    int[][] rowClues, colClues;
                    CluesOf(NonogramPictures.Rows(tier, i), out rowClues, out colClues);
                    bool[] first;
                    all[tier][i] = NonogramUniqueness.Count(rowClues, colClues, 2, out first) == 1;
                }
            }
            unique = all;
            return unique;
        }

        private static List<int> IndexesOf(string[] names, int tier)
        {
            var found = new List<int>();
            for (int i = 0; i < NonogramPictures.Count(tier); i++)
                if (Array.IndexOf(names, NonogramPictures.Name(tier, i)) >= 0) found.Add(i);
            return found;
        }

        private static void AddPicture(List<string> names, GardenItem item)
        {
            string picture = PictureFor(item);
            if (picture != null && !names.Contains(picture)) names.Add(picture);
        }

        private static void CluesOf(string[] rows, out int[][] rowClues, out int[][] colClues)
        {
            int n = rows.Length;
            rowClues = new int[n][];
            colClues = new int[n][];
            for (int i = 0; i < n; i++)
            {
                var row = new List<int>();
                var col = new List<int>();
                int rr = 0, cr = 0;
                for (int k = 0; k < n; k++)
                {
                    if (rows[i][k] == '#') rr++; else if (rr > 0) { row.Add(rr); rr = 0; }
                    if (rows[k][i] == '#') cr++; else if (cr > 0) { col.Add(cr); cr = 0; }
                }
                if (rr > 0) row.Add(rr);
                if (cr > 0) col.Add(cr);
                rowClues[i] = row.ToArray();
                colClues[i] = col.ToArray();
            }
        }

        private static long FloorDiv(long a, long b)
        {
            return a >= 0 ? a / b : -((-a + b - 1) / b);
        }
    }

    /// <summary>
    /// Exact solution counter for nonogram clues: depth-first search over the rows, each row
    /// trying every placement its clue allows, with every column's clue checked as the rows
    /// fill in. Stops once <c>cap</c> solutions are found, so a count of 1 proves uniqueness.
    /// </summary>
    public static class NonogramUniqueness
    {
        /// <summary>Counts the solutions of the clues, stopping at <paramref name="cap"/>; <paramref name="first"/> is the first solution found (null if none).</summary>
        public static int Count(int[][] rowClues, int[][] colClues, int cap, out bool[] first)
        {
            if (rowClues == null || colClues == null || rowClues.Length != colClues.Length)
                throw new ArgumentException("Clues must cover a square.");
            var counter = new Counter(rowClues, colClues, cap);
            counter.Search(0);
            first = counter.First;
            return counter.Found;
        }

        private sealed class Counter
        {
            private readonly int[][] rowClues, colClues;
            private readonly int cap, n;
            private readonly List<bool[]>[] placements;
            private readonly bool[,] grid;
            private readonly int[] runs;
            public int Found;
            public bool[] First;

            public Counter(int[][] rowClues, int[][] colClues, int cap)
            {
                this.rowClues = rowClues;
                this.colClues = colClues;
                this.cap = cap;
                n = rowClues.Length;
                grid = new bool[n, n];
                runs = new int[n + 1];
                placements = new List<bool[]>[n];
                for (int r = 0; r < n; r++)
                {
                    placements[r] = new List<bool[]>();
                    Place(rowClues[r], 0, 0, new bool[n], placements[r]);
                }
            }

            // Every way to lay the clue's runs along a line of n cells, in order.
            private void Place(int[] clue, int k, int from, bool[] line, List<bool[]> outList)
            {
                if (k == clue.Length)
                {
                    for (int i = from; i < n; i++) line[i] = false;
                    outList.Add((bool[])line.Clone());
                    return;
                }
                int rest = 0;
                for (int j = k + 1; j < clue.Length; j++) rest += clue[j] + 1;
                for (int start = from; start + clue[k] + rest <= n; start++)
                {
                    for (int i = from; i < start; i++) line[i] = false;
                    int end = start + clue[k];
                    for (int i = start; i < end; i++) line[i] = true;
                    int next = end;
                    if (end < n) { line[end] = false; next = end + 1; }
                    Place(clue, k + 1, next, line, outList);
                }
            }

            public void Search(int r)
            {
                if (Found >= cap) return;
                if (r == n)
                {
                    Found++;
                    if (First == null)
                    {
                        First = new bool[n * n];
                        for (int i = 0; i < n; i++)
                            for (int c = 0; c < n; c++) First[i * n + c] = grid[i, c];
                    }
                    return;
                }
                foreach (bool[] pattern in placements[r])
                {
                    for (int c = 0; c < n; c++) grid[r, c] = pattern[c];
                    if (!ColumnsFit(r + 1)) continue;
                    Search(r + 1);
                    if (Found >= cap) return;
                }
            }

            // Whether every column can still match its clue given rows 0..rowsDone-1.
            private bool ColumnsFit(int rowsDone)
            {
                bool final = rowsDone == n;
                for (int c = 0; c < n; c++)
                {
                    int[] clue = colClues[c];
                    int count = 0, run = 0;
                    for (int r = 0; r < rowsDone; r++)
                    {
                        if (grid[r, c]) { run++; continue; }
                        if (run > 0) { runs[count++] = run; run = 0; }
                    }
                    bool open = run > 0;
                    if (final)
                    {
                        if (open) runs[count++] = run;
                        if (count != clue.Length) return false;
                        for (int i = 0; i < count; i++) if (runs[i] != clue[i]) return false;
                        continue;
                    }
                    if (count > clue.Length) return false;
                    for (int i = 0; i < count; i++) if (runs[i] != clue[i]) return false;
                    if (open && (count >= clue.Length || run > clue[count])) return false;
                }
                return true;
            }
        }
    }
}
