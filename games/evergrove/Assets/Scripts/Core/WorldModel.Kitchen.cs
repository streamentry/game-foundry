using System;

namespace Evergrove.Core
{
    /// <summary>
    /// The jam kitchen: three fruit are cooked down into one jar of jam, which can be given in a parcel.
    /// Fruit and jam are only ever moved or cooked down, never created.
    /// </summary>
    public sealed partial class WorldModel
    {
        public const int JamPerJar = 3;
        public const int MaxJam = 10;
        public const int MaxJarsPerParcel = 3;

        /// <summary>Jars of jam in the pantry, 0..<see cref="MaxJam"/>.</summary>
        public int Jam { get { return state.jam; } }

        /// <summary>
        /// Cooks <see cref="JamPerJar"/> fruit into one jar. Living gardens only; refused, changing nothing,
        /// when there is too little fruit or the pantry is full (so no fruit is wasted).
        /// </summary>
        public bool MakeJam()
        {
            if (Mode != PlayMode.Living || state.fruit < JamPerJar || state.jam >= MaxJam) return false;
            state.fruit -= JamPerJar;
            state.jam++;
            return true;
        }

        /// <summary>Fail-closed validation of the jam count; Living gardens only may hold any.</summary>
        private static bool ReadJam(WorldSnapshot source)
        {
            return source.jam >= 0 && source.jam <= MaxJam && (source.mode == "living" || source.jam == 0);
        }
    }
}
