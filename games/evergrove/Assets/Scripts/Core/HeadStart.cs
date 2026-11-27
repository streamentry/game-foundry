using System;

namespace Evergrove.Core
{
    /// <summary>
    /// I322 (idea 6, after the handicap stones of go in China, Japan and Korea: the stronger
    /// side gives a head start and nobody is shamed): when a kind's last board needed two or
    /// more hints, its next board opens with a move or two already made, by the same moves the
    /// hint would make. Never on the smallest boards, and never more than a few moves.
    /// </summary>
    public static class HeadStart
    {
        public const int MaxMoves = 2;

        public static int Moves(int hintsLastBoard, int tier)
        {
            if (tier <= 0 || hintsLastBoard < 2) return 0;
            return Math.Min(MaxMoves, hintsLastBoard - 1);
        }
    }
}
