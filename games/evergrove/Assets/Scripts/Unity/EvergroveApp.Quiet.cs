using System.Collections.Generic;
using Evergrove.Core;
using UnityEngine;

namespace Evergrove.UnityClient
{
    public sealed partial class EvergroveApp
    {
        /// <summary>
        /// "Speak less" (idea 85) and neighbour life arcs (idea 83). Both are device settings or
        /// device memory only: nothing here is written to the save.
        /// </summary>
        private const string SpeakLessKey = "evergrove:speakLess";
        private const string ArcShownKey = "evergrove:arc-shown:";

        // Speak less is read straight from PlayerPrefs so the toggle always matches what is saved.
        private bool SpeakLessOn { get { return PlayerPrefs.GetInt(SpeakLessKey, 0) == 1; } }

        // True when this remark opportunity is quietened by "speak less". Letters, gifts and
        // essential notices never call this. Opportunities are keyed by day, villager and
        // puzzles solved, so one opportunity always gives the same answer.
        private bool SpeakLessSilences(int villager)
        {
            if (!SpeakLessOn || world == null) return false;
            int day = CompanionContext.DayNumberOf(CompanionToday());
            return !QuietRemarks.Keep(day, villager, world.PuzzlesSolved);
        }

        // Idea 83: the arc line for Orin or Fen, when its step is news. It is said once while the
        // neighbour is being spoken to (the same line holds for that whole visit), and after that
        // only when the step has moved on. Device-only PlayerPrefs record the last step said.
        private readonly Dictionary<int, int> arcSpokenNow = new Dictionary<int, int>();

        private string ArcRemark(int villager)
        {
            int step = NeighbourArcs.Step(world, villager);
            if (step < 0) return null;
            string line = NeighbourArcs.Line(world, villager);
            if (speakingWith != villager)
            {
                arcSpokenNow.Remove(villager);
                // Outside the conversation the arc line counts as heard, so the badge stays quiet.
                bool heard = PlayerPrefs.GetInt(ArcShownKey + villager, -1) == step &&
                             PlayerPrefs.GetString(HeardKey + villager, "") == line;
                return heard ? line : null;
            }
            if (arcSpokenNow.TryGetValue(villager, out int during) && during == step) return line;
            if (PlayerPrefs.GetInt(ArcShownKey + villager, -1) == step) return null;
            arcSpokenNow[villager] = step;
            PlayerPrefs.SetInt(ArcShownKey + villager, step);
            return line;
        }

        // The Speak less toggle, in the settings row next to Gentle motion (same pattern).
        private void SpeakLessButton(Transform row)
        {
            IconButton(row, "Speak less setting",
                SpeakLessOn ? "Speak less: on" : "Speak less: off",
                MosseloraArt.IconChat, () => {
                    PlayerPrefs.SetInt(SpeakLessKey, SpeakLessOn ? 0 : 1);
                    PlayerPrefs.Save();
                    Rebuild();
                }, SettingToken, SpeakLessOn ? MosseloraTheme.ForestAction : MosseloraTheme.QuietAction,
                false, null, false, 0, 0, true, stateOn: SpeakLessOn);
        }
    }
}
