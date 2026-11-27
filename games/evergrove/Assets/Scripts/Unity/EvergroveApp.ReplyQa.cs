#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using Evergrove.Core;
using UnityEngine;

namespace Evergrove.UnityClient
{
    /// <summary>Checklist 81 tour hooks: a neighbour's letter to reply to, a chip chosen, and the echo that follows.</summary>
    public sealed partial class EvergroveApp
    {
        // Keeps a note from Mira and opens it (the newest letter). No reply saved yet.
        internal void QaOpenNeighbourLetter()
        {
            if (world == null || world.Mode != PlayMode.Living) return;
            // Always a fresh note from Mira, so the tour knows whose answer to look for.
            world.KeepNote("Mira", "The river runs clear this morning. The shade tree is full of birds.");
            PlayerPrefs.DeleteKey(ReplyKeyPrefix + world.Garden);
            ShowLetter(0);
        }

        // Chooses a chip (ReplyChips order: 0 warm, 1 curious, 2 thanks) for the neighbour who wrote the newest letter.
        internal void QaChooseReply(int chip)
        {
            if (world == null || world.Mode != PlayMode.Living || chip < 0 || chip >= ReplyChips.Chips.Count) return;
            string[] from = world.LetterFrom;
            if (from.Length == 0) return;
            int villager = Array.IndexOf(VillagerNames, from[from.Length - 1]);
            if (villager >= 0) ChooseReplyChip(villager, (ReplyChip)chip);
        }
    }
}
#endif
