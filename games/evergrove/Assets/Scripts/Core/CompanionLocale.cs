using System;
using System.Collections.Generic;

namespace Evergrove.Core
{
    /// <summary>
    /// Smart companion SC06: which language the companion may speak, and with what words.
    /// <list type="bullet">
    /// <item>English: the reviewed bank, else today's English sentence.</item>
    /// <item>Another language with a reviewed bank: that bank only. Never an English sentence, and
    /// only lines whose slots need no translation (<see cref="LocalizedSlots"/>): an English item
    /// name inside a French sentence would be wrong.</item>
    /// <item>Another language without a bank: the companion stays silent and the story's own
    /// localized lines (LocalizedVillage) speak, exactly as before.</item>
    /// </list>
    /// Banks ship only after a native speaker's review; machine drafts never go in Resources.
    /// </summary>
    public static class CompanionLocale
    {
        public const string English = "en";

        /// <summary>Slots whose values are already right in any language: a name typed by family.</summary>
        public static readonly IReadOnlyList<string> LocalizedSlots = Array.AsReadOnly(new[] { "name" });

        /// <summary>The companion speaks in this locale at all (else LocalizedVillage, as before SC01).</summary>
        public static bool Speaks(string locale, LineBank bank)
        {
            return locale == English || (bank != null && !bank.IsEmpty);
        }

        /// <summary>This voice has words for this fact in this locale.</summary>
        public static bool CanSay(string locale, LineBank bank, Fact fact, int speaker)
        {
            if (fact == null) return false;
            if (locale == English) return CompanionSpeech.Legacy(fact, speaker) != null || (bank != null && bank.CanSay(fact, speaker));
            if (bank == null || !bank.CanSay(fact, speaker)) return false;
            foreach (string line in bank.For(fact, speaker))
                foreach (string slot in LineBank.SlotsIn(line))
                    if (!IsLocalized(slot)) return false;
            return true;
        }

        /// <summary>
        /// The words, or null (the caller then lets the story's line speak). <paramref name="keep"/> is
        /// the wording already said this visit; <paramref name="variant"/> the wording used (-1: today's sentence).
        /// </summary>
        public static string Words(string locale, LineBank bank, Fact fact, int speaker, int garden, int day,
            CompanionMemory memory, int keep, out int variant)
        {
            variant = -1;
            if (!CanSay(locale, bank, fact, speaker)) return null;
            string banked = bank != null ? bank.Speak(fact, speaker, garden, day, memory, keep, out variant) : null;
            if (banked != null) return CompanionSpeech.Compose(fact, banked);
            variant = -1;
            return locale == English ? CompanionSpeech.Legacy(fact, speaker) : null;
        }

        public static bool IsLocalized(string slot)
        {
            foreach (string s in LocalizedSlots) if (s == slot) return true;
            return false;
        }
    }
}
