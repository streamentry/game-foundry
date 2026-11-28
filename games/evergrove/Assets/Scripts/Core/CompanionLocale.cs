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

        /// <summary>
        /// CP31: translates one slot's English value into the locale, or returns null when it has no
        /// translation. Optional: with none, only <see cref="LocalizedSlots"/> may appear in a line.
        /// </summary>
        public delegate string SlotTranslator(string slot, string englishValue);

        /// <summary>Slots whose values are already right in any language: a name typed by family.</summary>
        public static readonly IReadOnlyList<string> LocalizedSlots = Array.AsReadOnly(new[] { "name" });

        /// <summary>The companion speaks in this locale at all (else LocalizedVillage, as before SC01).</summary>
        public static bool Speaks(string locale, LineBank bank)
        {
            return locale == English || (bank != null && !bank.IsEmpty);
        }

        /// <summary>This voice has words for this fact in this locale.</summary>
        public static bool CanSay(string locale, LineBank bank, Fact fact, int speaker, SlotTranslator translate = null)
        {
            if (fact == null) return false;
            if (locale == English) return CompanionSpeech.Legacy(fact, speaker) != null || (bank != null && bank.CanSay(fact, speaker));
            if (bank == null || !bank.CanSay(fact, speaker)) return false;
            foreach (string line in bank.For(fact, speaker))
                foreach (string slot in LineBank.SlotsIn(line))
                    if (!IsLocalized(slot) && !IsTranslated(fact, slot, translate)) return false;
            return true;
        }

        /// <summary>
        /// The words, or null (the caller then lets the story's line speak). <paramref name="keep"/> is
        /// the wording already said this visit; <paramref name="variant"/> the wording used (-1: today's sentence).
        /// </summary>
        public static string Words(string locale, LineBank bank, Fact fact, int speaker, int garden, int day,
            CompanionMemory memory, int keep, out int variant, SlotTranslator translate = null)
        {
            variant = -1;
            if (!CanSay(locale, bank, fact, speaker, translate)) return null;
            // CP31: a new fact with translated slot values; the original is never changed.
            Fact spoken = locale == English || translate == null ? fact : Translated(fact, translate);
            string banked = bank != null ? bank.Speak(spoken, speaker, garden, day, memory, keep, out variant) : null;
            if (banked != null) return CompanionSpeech.Compose(fact, banked);
            variant = -1;
            return locale == English ? CompanionSpeech.Legacy(fact, speaker) : null;
        }

        public static bool IsLocalized(string slot)
        {
            foreach (string s in LocalizedSlots) if (s == slot) return true;
            return false;
        }

        /// <summary>CP31: the slot has a non-empty translation of the fact's English value.</summary>
        private static bool IsTranslated(Fact fact, string slot, SlotTranslator translate)
        {
            if (translate == null) return false;
            string translation = translate(slot, fact.Slot(slot));
            return !string.IsNullOrEmpty(translation);
        }

        /// <summary>CP31: a copy of the fact whose translatable slots hold their translated values.</summary>
        private static Fact Translated(Fact fact, SlotTranslator translate)
        {
            var slots = new Dictionary<string, string>();
            foreach (var pair in fact.Slots)
            {
                string value = pair.Value;
                if (!IsLocalized(pair.Key))
                {
                    string translation = translate(pair.Key, pair.Value);
                    if (!string.IsNullOrEmpty(translation)) value = translation;
                }
                slots[pair.Key] = value;
            }
            return new Fact(fact.Kind, fact.Keeper, fact.Key, slots, fact.Weight, fact.Waiting, fact.Fresh, fact.LegacyOrder, fact.Once);
        }
    }
}
