using System;

namespace Evergrove.Core
{
    /// <summary>
    /// Board tile size for the phone's orientation (I114-I116, pure since I118).
    /// Portrait sizes tiles by the column width. Sideways the screen is short, so
    /// tiles are sized by the height left after the stream stones, board rim,
    /// water drops, spacing and margins. While the board is unsolved, Undo and
    /// Hint stand in a rail beside it, which frees height but takes width.
    /// Never below a 44-point touch target.
    /// </summary>
    public static class BoardFit
    {
        public const int MinTouch = 44;
        public const float RailWidth = 84f;
        public const float RailGap = 12f;
        /// <summary>Height taken by everything but the tiles, tools in the rail.</summary>
        public const float BelowUnsolved = 136f;
        /// <summary>Height taken by everything but the tiles once solved (no rail).</summary>
        public const float BelowSolved = 212f;

        public static int MaxTile(int size)
        {
            return size == 3 ? 90 : 67;
        }

        /// <summary>Total width or height of a size x size grid of tiles.</summary>
        public static int Span(int tile, int size, int gap)
        {
            return (tile + gap) * size - gap;
        }

        /// <summary>Portrait: the column width decides (the EG-007 sizing).</summary>
        public static int Portrait(float available, int size, int gap)
        {
            int byWidth = (int)Math.Floor((available - (size - 1) * gap) / size);
            return Clamp(byWidth, 48, MaxTile(size));
        }

        /// <summary>Sideways: the height decides, and the rail's width when unsolved.</summary>
        public static int Landscape(float logicalHeight, float available, int size, int gap, bool solved)
        {
            float below = solved ? BelowSolved : BelowUnsolved;
            int byHeight = (int)Math.Floor((logicalHeight - below) / size) - gap;
            if (!solved)
            {
                int byWidth = (int)Math.Floor((available - RailWidth - RailGap - (size - 1) * gap) / size);
                byHeight = Math.Min(byHeight, byWidth);
            }
            return Clamp(byHeight, MinTouch, MaxTile(size));
        }

        private static int Clamp(int value, int low, int high)
        {
            return value < low ? low : value > high ? high : value;
        }
    }
}
