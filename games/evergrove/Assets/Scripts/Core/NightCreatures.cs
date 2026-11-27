using System;

namespace Evergrove.Core
{
    /// <summary>
    /// Checklist 49: night creatures, one per open stretch of river. Moths and frogs come out
    /// in the evening only (dusk and night), and every stretch has one: even stretches have
    /// moths, odd stretches frogs. Pure: no UI types, nothing saved. Who has been seen is kept
    /// on the device (see the app's "who visits" log), never in the garden file.
    /// </summary>
    public static class NightCreatures
    {
        /// <summary>The evening: dusk and night. Never morning, day or golden hour.</summary>
        public static bool Dark(DayPhase phase)
        {
            return phase == DayPhase.Dusk || phase == DayPhase.Night;
        }

        /// <summary>True for the creatures this rule owns (the owl keeps its grove, I374).</summary>
        public static bool IsNightKind(Visitor visitor)
        {
            return visitor == Visitor.Moths || visitor == Visitor.Frogs;
        }

        /// <summary>The night creature of a stretch, or null when it is not evening or the stretch is not real.</summary>
        public static Visitor? For(int reach, DayPhase phase)
        {
            if (reach < 0 || !Dark(phase)) return null;
            return reach % 2 == 0 ? Visitor.Moths : Visitor.Frogs;
        }
    }
}
