using System;

namespace Evergrove.Core
{
    /// <summary>What a decoration looks like; the number is saved.</summary>
    public enum Decoration { Stones = 0, Bench = 1, Fence = 2, LanternPost = 3 }

    /// <summary>
    /// Free decorations (ideas 14 and 16): a living garden may set stepping stones, a bench, a fence or a
    /// lantern post on any free land, for nothing. They create and cost nothing, and a decorated cell
    /// is taken: no plant, bought plant or thing may be put on it. They travel in the garden file.
    /// </summary>
    public sealed partial class WorldModel
    {
        public const int MaxDecor = 60;
        private const int DecorKinds = 4;

        /// <summary>Decorations as pairs [kind, cell, ...], in the order they were set down (a copy).</summary>
        public int[] Decor { get { return (int[])state.decor.Clone(); } }
        public int DecorCount { get { return state.decor.Length / 2; } }

        /// <summary>
        /// Every cell a plant, bought plant or decoration stands on, as pairs [kind, cell, ...] ready to
        /// pass to <see cref="GardenMap.Layout(int[], int[], int[])"/> as its reserved cells, so the
        /// story things' default places keep clear of all of them.
        /// </summary>
        public int[] Occupied
        {
            get
            {
                int[] all = new int[state.plantings.Length + state.bought.Length + state.decor.Length];
                Array.Copy(state.plantings, 0, all, 0, state.plantings.Length);
                Array.Copy(state.bought, 0, all, state.plantings.Length, state.bought.Length);
                Array.Copy(state.decor, 0, all, state.plantings.Length + state.bought.Length, state.decor.Length);
                return all;
            }
        }

        /// <summary>Index of the decoration standing on <paramref name="cell"/>, or -1.</summary>
        private int DecorAt(int cell, int except = -1)
        {
            for (int k = 0; k * 2 + 1 < state.decor.Length; k++)
                if (k != except && state.decor[k * 2 + 1] == cell) return k;
            return -1;
        }

        /// <summary>
        /// True when a decoration may stand on <paramref name="cell"/>: land, and nothing there already
        /// (not a story thing as the garden lays it out now, nor a planting, a bought plant or another
        /// decoration). <paramref name="except"/> is a decoration being moved, which does not block itself.
        /// </summary>
        private bool DecorCellFree(GardenMap map, int cell, int except)
        {
            if (map == null || !map.IsLand(cell) || cell >= MaxMapCell) return false;
            int[] others = (int[])state.decor.Clone();
            if (except >= 0)
            {
                others = new int[state.decor.Length - 2];
                Array.Copy(state.decor, 0, others, 0, except * 2);
                Array.Copy(state.decor, except * 2 + 2, others, except * 2, others.Length - except * 2);
            }
            int[] reserved = new int[state.plantings.Length + state.bought.Length + others.Length];
            Array.Copy(state.plantings, 0, reserved, 0, state.plantings.Length);
            Array.Copy(state.bought, 0, reserved, state.plantings.Length, state.bought.Length);
            Array.Copy(others, 0, reserved, state.plantings.Length + state.bought.Length, others.Length);
            int[] layout = map.Layout(state.choices, state.placements, reserved);
            return map.IsFree(layout, reserved, cell);
        }

        /// <summary>
        /// Sets a decoration on a free land cell of <paramref name="map"/>. Free: it costs and creates nothing.
        /// Living gardens only, at most <see cref="MaxDecor"/> pieces; refused, changing nothing, otherwise.
        /// </summary>
        public bool PlaceDecor(Decoration kind, int cell, GardenMap map)
        {
            if (Mode != PlayMode.Living || (int)kind < 0 || (int)kind >= DecorKinds || DecorCount >= MaxDecor ||
                !DecorCellFree(map, cell, -1))
                return false;
            state.decor = Append(Append(state.decor, (int)kind), cell);
            return true;
        }

        /// <summary>Takes decoration <paramref name="index"/> away. Later pieces keep their order.</summary>
        public bool RemoveDecor(int index)
        {
            if (index < 0 || index >= DecorCount) return false;
            int[] next = new int[state.decor.Length - 2];
            Array.Copy(state.decor, 0, next, 0, index * 2);
            Array.Copy(state.decor, index * 2 + 2, next, index * 2, next.Length - index * 2);
            state.decor = next;
            return true;
        }

        /// <summary>Moves decoration <paramref name="index"/> to another free land cell, keeping its kind.</summary>
        public bool MoveDecor(int index, int cell, GardenMap map)
        {
            if (Mode != PlayMode.Living || index < 0 || index >= DecorCount || !DecorCellFree(map, cell, index)) return false;
            int[] next = (int[])state.decor.Clone();
            next[index * 2 + 1] = cell;
            state.decor = next;
            return true;
        }

        /// <summary>
        /// Fail-closed validation of the decorations; empty or null means "none" (an older save). The map is
        /// not known here, so land and story things are the player's flow; the pieces must be well
        /// formed, at most 60, one to a cell, and clear of every planting and bought plant.
        /// </summary>
        private static bool ReadDecor(WorldSnapshot source, out int[] decor)
        {
            decor = null;
            int[] list = source.decor ?? Array.Empty<int>();
            if (list.Length == 0) { decor = Array.Empty<int>(); return true; }
            if (list.Length % 2 != 0 || list.Length > MaxDecor * 2 || source.mode != "living") return false;
            int[] plantings = source.plantings ?? Array.Empty<int>();
            int[] bought = source.bought ?? Array.Empty<int>();
            for (int i = 0; i < list.Length; i += 2)
            {
                int cell = list[i + 1];
                if (list[i] < 0 || list[i] >= DecorKinds || cell < 0 || cell >= MaxMapCell) return false;
                for (int j = 1; j < list.Length; j += 2)
                    if (j != i + 1 && list[j] == cell) return false;
                for (int j = 1; j < plantings.Length; j += 2)
                    if (plantings[j] == cell) return false;
                for (int j = 1; j < bought.Length; j += 2)
                    if (bought[j] == cell) return false;
            }
            decor = (int[])list.Clone();
            return true;
        }
    }
}
