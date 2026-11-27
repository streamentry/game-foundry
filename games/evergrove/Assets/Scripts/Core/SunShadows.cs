using System;

namespace Evergrove.Core
{
    /// <summary>
    /// Checklist 22: the shadow one thing casts on the ground at a given part of the day.
    /// Angle is the direction the shadow falls in the screen plane, in degrees (0 = right/east,
    /// 90 = up the screen, 180 = left/west, 270 = down toward the viewer). Length is a multiple of
    /// the thing's own width. Alpha is the shadow's opacity, 0..1.
    /// </summary>
    public readonly struct SunShadowLook
    {
        public readonly bool Shows;
        public readonly float AngleDegrees, Length, Alpha;

        public SunShadowLook(bool shows, float angleDegrees, float length, float alpha)
        {
            Shows = shows;
            AngleDegrees = angleDegrees;
            Length = length;
            Alpha = alpha;
        }

        /// <summary>True when the shadow falls toward the left of the screen.</summary>
        public bool FallsLeft { get { return Shows && Math.Cos(AngleDegrees * Math.PI / 180.0) < -0.01; } }

        /// <summary>True when the shadow falls toward the right of the screen.</summary>
        public bool FallsRight { get { return Shows && Math.Cos(AngleDegrees * Math.PI / 180.0) > 0.01; } }
    }

    /// <summary>
    /// Checklist 22: a sun that moves with the day. Dawn is low in the east, so the shadow is long and
    /// faint and falls west (left). Morning leans left and shortens. Noon sits high, so the shadow is
    /// short, darkest and almost under the thing (it falls toward the viewer). Golden hour and dusk are
    /// low in the west, so the shadow stretches east (right). Night casts none. Cosmetic only.
    /// </summary>
    public static class SunShadows
    {
        public const float MaxAlpha = 0.4f;

        public static SunShadowLook For(DayPhase phase)
        {
            switch (phase)
            {
                case DayPhase.Dawn:    return new SunShadowLook(true, 180f, 1.4f, 0.20f);
                case DayPhase.Morning: return new SunShadowLook(true, 160f, 0.85f, 0.26f);
                case DayPhase.Day:     return new SunShadowLook(true, 270f, 0.35f, 0.32f);
                case DayPhase.Golden:  return new SunShadowLook(true, 10f, 1.2f, 0.22f);
                case DayPhase.Dusk:    return new SunShadowLook(true, 20f, 1.4f, 0.20f);
                default:               return new SunShadowLook(false, 0f, 0f, 0f);
            }
        }
    }
}
