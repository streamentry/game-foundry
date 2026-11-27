using System;

namespace Evergrove.Core
{
    /// <summary>
    /// The picture library for planting by numbers: small original garden drawings,
    /// one set per tier, each a square mask ('#' = plant, '.' = bare). Every picture
    /// is solvable by row and column logic alone (the tests prove it) and has no
    /// empty row or column, so the finished bed always draws something you can name.
    /// </summary>
    public static class NonogramPictures
    {
        private struct Picture
        {
            public readonly string Name;
            public readonly string[] Rows;
            public Picture(string name, string rows) { Name = name; Rows = rows.Split('/'); }
        }

        // Tier 0: 5 x 5.
        private static readonly Picture[] Small =
        {
            new Picture("a tulip", "#.#.#/#####/.###./..#../..#.."),
            new Picture("an apple", "..#../.###./#####/#####/.#.#."),
            new Picture("a mushroom", ".###./#####/..#../..#../.###."),
            new Picture("a heart-shaped leaf", "##.##/#####/#####/.###./..#.."),
            new Picture("a pot with a sprout", "##.##/.#.#./..#../#####/.###."),
            new Picture("a cup", "####./#####/#####/####./.##.."),
            new Picture("a bell", "..#../.###./.###./#####/..#.."),
            new Picture("a sun", "#.#.#/.###./#####/.###./#.#.#"),
            new Picture("a key", "###../#.#../###../.#.../.#.##"),
        };

        // Tier 1: 6 x 6.
        private static readonly Picture[] Medium =
        {
            new Picture("a pear", "...#../..##../..##../.####./######/.####."),
            new Picture("a tulip", "#.##.#/######/.####./..##../.###../..##.."),
            new Picture("a mushroom", ".####./######/######/..##../..##../.####."),
            new Picture("a heart-shaped leaf", "##..##/######/######/.####./..##../..##.."),
            new Picture("a bell", "..##../.####./.####./######/######/..##.."),
            new Picture("a sun", "..##../#.##.#/.####./.####./#.##.#/..##.."),
            new Picture("a cup", ".#.#../#####./######/#####./.###../######"),
            new Picture("a cottage", "..##../.####./######/######/##..##/##..##"),
            new Picture("a fish", "#...../..##../.###.#/#.####/.###.#/..##.."),
            new Picture("a pot with a sprout", "##..##/.#..#./..##../.####./######/.####."),
        };

        // Tier 2: 8 x 8.
        private static readonly Picture[] Large =
        {
            new Picture("a pear", "....##../....#.../...##.../..####../.######./########/########/.######."),
            new Picture("an apple", "....#.../..##.##./.#######/########/########/########/.######./..#..#.."),
            new Picture("a tulip", "#.#..#.#/########/.######./..####../...##.../.#.##.../..###.##/...##..#"),
            new Picture("a mushroom", "..####../.######./##.##.##/########/...##.../...##.../..####../.######."),
            new Picture("a snail", "..###.../.#...#.#/#.##..##/#.#...##/#.#..###/.#...##./..###.##/########"),
            new Picture("a bird", "..###.../##.##.../..#####./..######/..######/...####./...#.#../..##.##."),
            new Picture("a cottage", "...##.#./..#####./.#######/########/.######./.##..##./.##..##./.##..##."),
            new Picture("a fish", "...##.../..####.#/.#.#####/########/.#######/..####.#/...##.../#......."),
            new Picture("a heart-shaped leaf", ".##..##./########/########/########/.######./..####../...##.../...##..."),
            new Picture("a bell", "...##.../..####../..####../.######./.######./########/########/...##..."),
            new Picture("a cup", ".#.#.#../########/######.#/######.#/######.#/######.#/.######./########"),
            new Picture("a pot with a sprout", "##....##/.##..##./..####../...##.../...##.../.######./..####../..####.."),
        };

        private static Picture[] Library(int tier)
        {
            if (tier < 0 || tier >= TreePuzzles.Tiers) throw new ArgumentOutOfRangeException(nameof(tier));
            return tier == 0 ? Small : tier == 1 ? Medium : Large;
        }

        /// <summary>How many pictures this tier draws from.</summary>
        public static int Count(int tier) { return Library(tier).Length; }

        /// <summary>The picture's short name, e.g. "a pear".</summary>
        public static string Name(int tier, int index) { return Pick(tier, index).Name; }

        /// <summary>A copy of the picture's rows, top to bottom, '#' for a plant.</summary>
        public static string[] Rows(int tier, int index) { return (string[])Pick(tier, index).Rows.Clone(); }

        private static Picture Pick(int tier, int index)
        {
            var lib = Library(tier);
            if (index < 0 || index >= lib.Length) throw new ArgumentOutOfRangeException(nameof(index));
            return lib[index];
        }
    }
}
