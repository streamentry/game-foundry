using System;
using System.Collections.Generic;

namespace Evergrove.Core
{
    /// <summary>
    /// Separate internal research catalogs from player-visible launch locales.
    /// A translation file with all keys is NOT a locale certification. Keep
    /// the consumer allowlist deliberately small until native/device/store
    /// reviewers sign off an actual candidate build.
    /// </summary>
    public static class LocaleShippingPolicy
    {
        // Safety default for a future non-development build, NOT a statement
        // that English has passed accessibility or store review.
        private static readonly GameLanguage[] ConsumerCandidates = {
            new GameLanguage("en", "English")
        };

        public static IReadOnlyList<GameLanguage> Selectable(bool researchBuild)
        {
            return researchBuild ? GameLocalization.Available : ConsumerCandidates;
        }

        public static bool Allows(string code, bool researchBuild)
        {
            if (string.IsNullOrEmpty(code)) return false;
            if (researchBuild) return GameLocalization.IsSupported(code);
            foreach (GameLanguage language in ConsumerCandidates)
                if (string.Equals(code, language.Code, StringComparison.Ordinal))
                    return true;
            return false;
        }

        public static string Resolve(string detectedCode, bool researchBuild)
        {
            return Allows(detectedCode, researchBuild)
                ? detectedCode
                : GameLocalization.Fallback;
        }
    }
}
