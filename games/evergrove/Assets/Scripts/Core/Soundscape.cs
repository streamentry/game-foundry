namespace Evergrove.Core
{
    public enum AmbientVoice { Birds, Crickets, Wind, Water, Rain }

    /// <summary>
    /// I316 (idea 49, after ambient Japanese puzzle games where sound is a place): what you
    /// hear over home, by the garden's season and your hour. Birds by day, crickets on summer
    /// and autumn evenings, wind through winter, the stream otherwise. One or two voices,
    /// sparse. Cosmetic, like the light (I272); the sound switch silences it all.
    /// </summary>
    public static class Soundscape
    {
        public static AmbientVoice[] For(Season season, DayPhase phase)
        {
            bool dark = phase == DayPhase.Dusk || phase == DayPhase.Night;
            if (season == Season.Winter)
                return dark || phase == DayPhase.Dawn ? new[] { AmbientVoice.Wind }
                    : new[] { AmbientVoice.Wind, AmbientVoice.Birds };
            if (dark)
                return season == Season.Spring ? new[] { AmbientVoice.Water }
                    : new[] { AmbientVoice.Crickets, AmbientVoice.Water };
            if (season == Season.Autumn && phase == DayPhase.Golden)
                return new[] { AmbientVoice.Birds, AmbientVoice.Wind };
            return new[] { AmbientVoice.Birds, AmbientVoice.Water };
        }

        /// <summary>Average quiet between two sounds of a voice, in seconds.</summary>
        /// <summary>I354–I356: the day's weather leads the voice half the time or more.</summary>
        public static AmbientVoice? WeatherVoice(Weather weather, DayPhase phase)
        {
            bool dark = phase == DayPhase.Dusk || phase == DayPhase.Night;
            switch (weather)
            {
                case Weather.Rainy: return AmbientVoice.Rain;
                case Weather.Windy: case Weather.Snowy: return AmbientVoice.Wind;
                case Weather.Hot: return dark ? (AmbientVoice?)null : AmbientVoice.Crickets;
                default: return null;
            }
        }

        public static float SecondsBetween(AmbientVoice voice)
        {
            switch (voice)
            {
                case AmbientVoice.Birds: return 7f;
                case AmbientVoice.Crickets: return 4f;
                case AmbientVoice.Wind: return 11f;
                case AmbientVoice.Rain: return 2.6f; // I354: patter, close together
                default: return 9f;
            }
        }
    }
}
