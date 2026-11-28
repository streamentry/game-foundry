using System;

namespace Evergrove.Core
{
    /// <summary>
    /// Checklist 18: the corner chrome (menu, seed purse, open basket tray) fades back while the
    /// garden is left alone, so the garden fills the screen. Pure: the Unity layer supplies the
    /// seconds since the last touch and whether reduced motion is on.
    /// </summary>
    public static class IdleFade
    {
        /// <summary>Seconds with no touch before the chrome starts to fade.</summary>
        public const float IdleAfterSeconds = 6f;

        /// <summary>How long the fade takes once it starts.</summary>
        public const float FadeSeconds = 0.8f;

        /// <summary>The faded opacity: still visible and still tappable.</summary>
        public const float FadedAlpha = 0.2f;

        /// <summary>
        /// Opacity of the chrome after <paramref name="secondsSinceTouch"/> seconds without a touch.
        /// Full until 6 s, then eases (smoothstep) to 0.2 over 0.8 s. Reduced motion jumps straight
        /// to 0.2 at 6 s. Negative or NaN input counts as a fresh touch.
        /// </summary>
        public static float AlphaAt(float secondsSinceTouch, bool reducedMotion)
        {
            if (float.IsNaN(secondsSinceTouch) || secondsSinceTouch < IdleAfterSeconds) return 1f;
            if (reducedMotion) return FadedAlpha;
            float t = (secondsSinceTouch - IdleAfterSeconds) / FadeSeconds;
            if (t >= 1f) return FadedAlpha;
            float eased = t * t * (3f - 2f * t);
            return 1f - (1f - FadedAlpha) * eased;
        }
    }
}
