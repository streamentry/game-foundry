using System;
using System.Globalization;
using System.Text;

namespace Evergrove.Core
{
    /// <summary>
    /// Short-lived, consent-first facilitator notes about a single play session.
    /// It collects no names, device IDs, IP, location, chat text, timestamps,
    /// hidden background events or third-party analytics. It is never persisted
    /// or transmitted by the game.
    ///
    /// For directional usability findings only. Does not measure D1/D7 return
    /// or establish causality; a researcher must recruit and follow up.
    /// </summary>
    public sealed class LocalPlaytest
    {
        private readonly double startSeconds;
        private readonly int initialChapter;
        private string condition;
        private string visual;
        private double? firstMove;
        private double? firstSolve;
        private int rotates;
        private int hints;
        private int undos;
        private int restarts;
        private int solved;
        private int choices;
        private int corrections;
        private int visualSwitches;
        private int modeSwitches;
        private int npcTalks;

        public LocalPlaytest(PlayMode mode, bool miniature, int chapter, double nowSeconds)
        {
            if (!ValidTime(nowSeconds) || chapter < 0 || chapter > PuzzleCatalog.Count)
                throw new ArgumentOutOfRangeException(nameof(nowSeconds));
            startSeconds = nowSeconds;
            initialChapter = chapter;
            condition = ConditionName(mode);
            visual = miniature ? "Miniature" : "Storybook";
        }

        private static string ConditionName(PlayMode mode)
        {
            if (mode == PlayMode.Living) return "LivingWorld";
            return mode == PlayMode.PuzzleMatched ? "PuzzleMatched" : "PuzzleOnly";
        }

        private static bool ValidTime(double seconds)
        {
            return !double.IsNaN(seconds) && !double.IsInfinity(seconds) && seconds >= 0;
        }

        private void Check(double now)
        {
            if (!ValidTime(now) || now < startSeconds)
                throw new ArgumentOutOfRangeException(nameof(now));
        }

        public void Rotate(double now)
        {
            Check(now);
            rotates++;
            if (!firstMove.HasValue) firstMove = now - startSeconds;
        }

        public void Hint(double now) { Check(now); hints++; }
        public void Undo(double now) { Check(now); undos++; }
        public void Restart(double now) { Check(now); restarts++; }

        public void Solve(double now)
        {
            Check(now);
            solved++;
            if (!firstSolve.HasValue) firstSolve = now - startSeconds;
        }

        public void Choose(double now) { Check(now); choices++; }
        public void Reconsider(double now) { Check(now); corrections++; }
        public void Talk(double now) { Check(now); npcTalks++; }

        public void ChangeMode(PlayMode mode, double now)
        {
            Check(now);
            string next = ConditionName(mode);
            if (next != condition) modeSwitches++;
            condition = next;
        }

        public void ChangeVisual(bool miniature, double now)
        {
            Check(now);
            string next = miniature ? "Miniature" : "Storybook";
            if (next != visual) visualSwitches++;
            visual = next;
        }

        public string Report(double now)
        {
            Check(now);
            var result = new StringBuilder();
            result.AppendLine("EVERGROVE LOCAL PLAYTEST v1");
            result.AppendLine("CONSENT: facilitator-selected; no automatic upload or persistence");
            result.AppendLine("condition=" + condition);
            result.AppendLine("visual=" + visual);
            result.AppendLine("initial_chapter=" + (initialChapter + 1));
            result.AppendLine("session_seconds=" + Seconds(now - startSeconds));
            result.AppendLine("first_tap_seconds=" + (firstMove.HasValue ? Seconds(firstMove.Value) : "not_observed"));
            result.AppendLine("first_solve_seconds=" + (firstSolve.HasValue ? Seconds(firstSolve.Value) : "not_observed"));
            result.AppendLine("rotations=" + rotates);
            result.AppendLine("puzzles_solved=" + solved);
            result.AppendLine("hints=" + hints);
            result.AppendLine("undos=" + undos);
            result.AppendLine("puzzle_restarts=" + restarts);
            result.AppendLine("village_choices=" + choices);
            result.AppendLine("choice_corrections=" + corrections);
            result.AppendLine("npc_conversations_opened=" + npcTalks);
            result.AppendLine("visual_mode_switches=" + visualSwitches);
            result.AppendLine("experiment_condition_switches=" + modeSwitches);
            result.AppendLine("LIMIT: this report excludes identity, cohort retention and satisfaction");
            return result.ToString();
        }

        private static string Seconds(double seconds)
        {
            return seconds.ToString("F1", CultureInfo.InvariantCulture);
        }
    }
}
