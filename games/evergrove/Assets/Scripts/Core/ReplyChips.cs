using System;
using System.Collections.Generic;
using System.Globalization;

namespace Evergrove.Core
{
    /// <summary>The three things a player can say back to a neighbour's letter. Icons, not free text.</summary>
    public enum ReplyChip { Warm = 0, Curious = 1, Thanks = 2 }

    /// <summary>One reply kept for one neighbour: which chip, on which day, and whether the neighbour has answered it.</summary>
    public readonly struct ReplyNote
    {
        public readonly int Villager;
        public readonly ReplyChip Chip;
        public readonly int Day;
        public readonly bool Echoed;

        public ReplyNote(int villager, ReplyChip chip, int day, bool echoed)
        {
            Villager = villager;
            Chip = chip;
            Day = day;
            Echoed = echoed;
        }
    }

    /// <summary>
    /// Checklist 81: device-local memory of the replies sent to neighbours' letters. At most one
    /// reply per neighbour (a new reply replaces the old). Immutable; every change returns a copy.
    /// Conversation state, not garden state: it lives in PlayerPrefs, never in the save or the garden file.
    /// </summary>
    public sealed class ReplyMemory
    {
        public static readonly ReplyMemory Empty = new ReplyMemory(Array.Empty<ReplyNote>());

        private readonly ReplyNote[] notes;

        private ReplyMemory(ReplyNote[] entries) { notes = entries; }

        public int Count { get { return notes.Length; } }

        public IReadOnlyList<ReplyNote> Entries { get { return Array.AsReadOnly(notes); } }

        /// <summary>The reply kept for this neighbour, if any.</summary>
        public bool TryGet(int villager, out ReplyNote note)
        {
            foreach (ReplyNote n in notes)
                if (n.Villager == villager)
                {
                    note = n;
                    return true;
                }
            note = default(ReplyNote);
            return false;
        }

        /// <summary>A copy with this neighbour's reply replaced (or added); the others are untouched.</summary>
        public ReplyMemory With(ReplyNote note)
        {
            var kept = new List<ReplyNote>(notes.Length + 1);
            foreach (ReplyNote n in notes)
                if (n.Villager != note.Villager) kept.Add(n);
            kept.Add(note);
            kept.Sort((a, b) => a.Villager.CompareTo(b.Villager));
            return new ReplyMemory(kept.ToArray());
        }

        // ---- PlayerPrefs text: one reply per line, "villager|chip|day|echoed" with echoed 0 or 1.
        public string Serialize()
        {
            var text = new System.Text.StringBuilder();
            foreach (ReplyNote n in notes)
            {
                if (text.Length > 0) text.Append('\n');
                text.Append(n.Villager.ToString(CultureInfo.InvariantCulture)).Append('|')
                    .Append(n.Chip.ToString()).Append('|')
                    .Append(n.Day.ToString(CultureInfo.InvariantCulture)).Append('|')
                    .Append(n.Echoed ? '1' : '0');
            }
            return text.ToString();
        }

        /// <summary>Reads what <see cref="Serialize"/> wrote. Unreadable lines are skipped, never thrown on.</summary>
        public static ReplyMemory Parse(string text)
        {
            if (string.IsNullOrEmpty(text)) return Empty;
            var memory = Empty;
            foreach (string line in text.Split('\n'))
            {
                string[] parts = line.Trim().Split('|');
                if (parts.Length != 4) continue;
                if (!int.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out int villager) ||
                    !ReplyChips.ValidVillager(villager)) continue;
                if (!Enum.TryParse(parts[1], out ReplyChip chip) || !Enum.IsDefined(typeof(ReplyChip), chip)) continue;
                if (!int.TryParse(parts[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out int day) || day < 0) continue;
                if (parts[3] != "0" && parts[3] != "1") continue;
                memory = memory.With(new ReplyNote(villager, chip, day, parts[3] == "1"));
            }
            return memory;
        }
    }

    /// <summary>
    /// Checklist 81: "Neighbours remember what you said." A reply chip sent back to a neighbour's letter
    /// is echoed in that neighbour's next remarks, within <see cref="EchoWindowDays"/> days, once.
    /// A reply that is never echoed simply lapses. Pure rules; no UnityEngine.
    /// Villagers: 0 Mira, 1 Orin, 2 Fen.
    /// </summary>
    public static class ReplyChips
    {
        public const int Villagers = 3;
        public const int EchoWindowDays = 3;

        private static readonly ReplyChip[] All = { ReplyChip.Warm, ReplyChip.Curious, ReplyChip.Thanks };

        private static readonly string[] Labels = { "That made me smile", "Tell me more", "Thank you" };

        // Painted icons that exist in Resources/Evergrove/Art (MosseloraArt.Load).
        private static readonly string[] Icons = { "ui-sun", "ui-chat", "glyph-sprig" };

        // Nine short echoes, one per chip per neighbour: warm, adult, never guilt or loss.
        private static readonly string[,] Echoes =
        {
            { // Mira
                "I'm glad that reached you. Hearing it made my morning.",
                "A little more, then: the shade tree was here long before any of us.",
                "Thank you back. Little words like yours keep the garden growing.",
            },
            { // Orin
                "Good to hear it made you smile. That's the best news on the path today.",
                "A little more, then: every plank of my boat is measured twice.",
                "You're very welcome. It's a pleasure to have you on the path.",
            },
            { // Fen
                "That makes me glad too. Kind words carry well over water.",
                "A little more, then: the old mill wheel was hiding in the reeds.",
                "Thank you for saying so. Kind words go a long way out here.",
            },
        };

        /// <summary>The three chips, in the order they are shown.</summary>
        public static IReadOnlyList<ReplyChip> Chips { get { return Array.AsReadOnly(All); } }

        public static bool ValidVillager(int villager) { return villager >= 0 && villager < Villagers; }

        /// <summary>The accessible label of a chip: short, warm, adult.</summary>
        public static string Label(ReplyChip chip) { return Labels[(int)chip]; }

        /// <summary>The painted icon name of a chip.</summary>
        public static string Icon(ReplyChip chip) { return Icons[(int)chip]; }

        /// <summary>Keep this reply to the neighbour's letter, from day <paramref name="day"/>. It replaces their earlier reply.</summary>
        public static ReplyMemory Choose(ReplyMemory memory, int villager, ReplyChip chip, int day)
        {
            if (memory == null) memory = ReplyMemory.Empty;
            if (!ValidVillager(villager) || (int)chip < 0 || (int)chip >= All.Length || day < 0) return memory;
            return memory.With(new ReplyNote(villager, chip, day, false));
        }

        /// <summary>A reply to this neighbour was chosen 0 to <see cref="EchoWindowDays"/> days ago and is not echoed yet.</summary>
        public static bool EchoDue(ReplyMemory memory, int villager, int today)
        {
            if (memory == null || !ValidVillager(villager)) return false;
            if (!memory.TryGet(villager, out ReplyNote note) || note.Echoed) return false;
            int ago = today - note.Day;
            return ago >= 0 && ago <= EchoWindowDays;
        }

        /// <summary>The neighbour's line that answers this reply. Empty for an unknown villager or chip.</summary>
        public static string Echo(int villager, ReplyChip chip)
        {
            if (!ValidVillager(villager) || (int)chip < 0 || (int)chip >= All.Length) return "";
            return Echoes[villager, (int)chip];
        }

        /// <summary>The neighbour has answered their reply: it is used up and is not echoed again.</summary>
        public static ReplyMemory MarkEchoed(ReplyMemory memory, int villager)
        {
            if (memory == null) return ReplyMemory.Empty;
            if (!memory.TryGet(villager, out ReplyNote note) || note.Echoed) return memory;
            return memory.With(new ReplyNote(villager, note.Chip, note.Day, true));
        }
    }
}
