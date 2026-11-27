using System.Collections.Generic;

namespace Evergrove.Core
{
    /// <summary>
    /// The words for a fact. Today's English sentences (the ones the neighbours said before the
    /// smart companion), unchanged: SC01 proves the feed reproduces them exactly. The reviewed line
    /// bank (SC03) speaks instead where it has lines for the fact's kind.
    /// </summary>
    public static class CompanionSpeech
    {
        /// <summary>Today's sentence for a fact in a speaker's voice; null if that voice has none.</summary>
        public static string Legacy(Fact fact, int speaker)
        {
            if (fact == null || speaker < 0 || speaker > 2 || !fact.For(speaker)) return null;
            switch (fact.Kind)
            {
                case FactKind.FruitGiven: return NeighbourNotice.Thanked(speaker);
                case FactKind.SolvedTogether: return NeighbourNotice.TogetherLine(fact.EnumSlot<PuzzleKind>("puzzle"));
                case FactKind.WeatherWhileAway:
                    string since = DayWeather.SinceSentence(fact.Slot("weather"), fact.IntSlot("days"));
                    return fact.Slot("welcome") == null ? since : fact.Slot("welcome") + " " + since; // I420
                case FactKind.LetterWaiting:
                    return "There's a letter in your box from " + fact.Slot("name") + ". I saw the flag go up this morning.";
                case FactKind.GiftWaiting: return NeighbourNotice.Waiting(speaker, fact.Slot("thing"));
                case FactKind.HelpedBy:
                    return fact.Slot("name") + " came by and " + NeighbourNotice.HelpedWith(fact.EnumSlot<PuzzleKind>("puzzle")) +
                        " Look how it blooms.";
                case FactKind.NewArrival: return NeighbourNotice.Arrival(speaker, fact.Slot("thing"));
                case FactKind.RiverClose: return "The mist downstream is thinning. One more thing tended and it will lift.";
                case FactKind.RiverFurther:
                    return "The river runs further than it ever did. " + NeighbourNotice.SeasonCame(fact.EnumSlot<Season>("season"));
                case FactKind.KindSolved:
                    return NeighbourNotice.Tended(fact.EnumSlot<PuzzleKind>("puzzle"), fact.IntSlot("count"));
                case FactKind.WeatherToday: return DayWeather.Remark(fact.EnumSlot<Weather>("weather"), speaker);
                case FactKind.PairingToFind: return PairingHint(fact.EnumSlot<Pairing>("pairing"));
                case FactKind.Hour:
                    return DayLight.Remark(speaker, fact.EnumSlot<DayPhase>("phase"), fact.EnumSlot<Season>("season"));
                case FactKind.NewestThing: return Newest(fact, speaker);
                case FactKind.VisitorCame: return VisitorRemark(fact.Slot("visitor"));
                case FactKind.HourHabit: return HabitRemark(fact.EnumSlot<DayPhase>("phase"), speaker);
                default: return null;
            }
        }

        /// <summary>
        /// The old hand-ordered chain (EvergroveApp.NeighborSays up to SC01) over the feed: the first
        /// fact in its order that this speaker can voice. Null means the story's own line speaks.
        /// </summary>
        public static Fact LegacyPick(IReadOnlyList<Fact> facts, int speaker)
        {
            Fact best = null;
            if (facts == null) return null;
            foreach (Fact fact in facts)
                if (fact.For(speaker) && fact.Slot(CompanionFacts.LegacyOffSlot) == null &&
                    (best == null || fact.LegacyOrder < best.LegacyOrder) && Legacy(fact, speaker) != null)
                    best = fact;
            return best;
        }

        /// <summary>I370: a keeper's hint at a good neighbour not yet found.</summary>
        public static string PairingHint(Pairing pairing)
        {
            switch (pairing)
            {
                case Pairing.Grove: return "Two trees side by side keep each other company. A grove, my grandmother called it.";
                case Pairing.ByTheWater: return "Things built beside the water last. The river keeps an eye on them.";
                case Pairing.Sheltered: return "Flowers love a little shade. Try one beside a tree.";
                case Pairing.Orchard: return "Three trees together and it's an orchard. They'd crop better for the company.";
                case Pairing.ShadySeat: return "A seat's no good in full sun. Build something beside a tree and you'll see.";
                case Pairing.Waterside: return "Some flowers like their feet wet. Try one right by the water.";
                default: return "Two different flowers side by side make a bed. They show each other off.";
            }
        }

        // I419 (Companion.HabitLine), the same words: a clear habit at that hour, on its noticer's day.
        private static string HabitRemark(DayPhase phase, int speaker)
        {
            int[] counts = new int[Companion.PhaseCount];
            counts[(int)phase] = Companion.HabitNeeds;
            return Companion.HabitLine(counts, speaker, speaker * 2);
        }

        // I377 (Visitors.KeeperRemark), the same words.
        private static string VisitorRemark(string visitor)
        {
            switch (visitor)
            {
                case "owl": return "There's an owl in your grove. I heard it last night, calling to the far trees.";
                case "heron": return "A heron's taken to fishing by what we built at the water. Patient bird. Good sign.";
                case "hedgehog": return "Something snuffles round your sheltered flowers at dusk. A hedgehog, I'd say. They know a safe spot.";
                default: return null;
            }
        }

        private static string Newest(Fact fact, int speaker)
        {
            string thing = fact.Slot("thing");
            string where = fact.Slot("where");
            where = string.IsNullOrEmpty(where) ? "" : " " + where;
            int count = fact.IntSlot("count");
            string more = count >= 3 ? " That makes " + count + " now." : "";
            switch (speaker)
            {
                case 0: return "Your " + thing + where + " is taking root nicely." + more;
                case 1: return "I walked past your " + thing + where + " this morning. Good, sturdy work." + more;
                default: return "The bees found your " + thing + where + " before I did." + more;
            }
        }
    }
}
