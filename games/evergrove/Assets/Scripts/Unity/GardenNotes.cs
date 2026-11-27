using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Evergrove.Core;
using UnityEngine;

namespace Evergrove.UnityClient
{
    /// <summary>
    /// I275: the one daily note from the garden, on the device (docs/specs/proposals/
    /// notifications-2026-11-27.md). Off until the player says yes in the game; then, each time
    /// the game closes, the single pending note is replaced with what's truly waiting
    /// (Core GardenNote), at the hour they chose, never at night, and not at all after three
    /// unopened in a row. iOS only for now; elsewhere it logs what it would schedule.
    /// </summary>
    internal static class GardenNotes
    {
        private const string OnKey = "evergrove:notes-on";
        private const string HourKey = "evergrove:notes-hour";
        private const string UnopenedKey = "evergrove:notes-unopened";
        private const string AskedKey = "evergrove:notes-asked"; // 0 never, 1 once "not now", 2 twice: never again
        private const string DueKey = "evergrove:notes-due";       // when the queued note was due (ticks)
        private const string TargetKey = "evergrove:notes-target"; // what it pointed at
        private const string ThingKey = "evergrove:notes-thing";

#if UNITY_IOS && !UNITY_EDITOR
        [DllImport("__Internal")] private static extern void _MosseloraNotesRequest();
        [DllImport("__Internal")] private static extern int _MosseloraNotesPermission();
        [DllImport("__Internal")] private static extern bool _MosseloraNotesSchedule(string title, string body, double seconds, string target);
        [DllImport("__Internal")] private static extern void _MosseloraNotesCancel();
#endif

        public static bool Supported
        {
#if UNITY_IOS || UNITY_EDITOR
            get { return true; }
#else
            get { return false; }
#endif
        }

        public static bool On { get { return PlayerPrefs.GetInt(OnKey, 0) == 1; } }
        public static int Hour { get { return Mathf.Clamp(PlayerPrefs.GetInt(HourKey, 19), GardenNote.EarliestHour, GardenNote.LatestHour); } }
        public static int Asked { get { return PlayerPrefs.GetInt(AskedKey, 0); } }

        /// <summary>The player said yes (soft ask or Settings): ask iOS, remember the hour.</summary>
        public static void TurnOn(int hour)
        {
            PlayerPrefs.SetInt(OnKey, 1);
            PlayerPrefs.SetInt(HourKey, Mathf.Clamp(hour, GardenNote.EarliestHour, GardenNote.LatestHour));
            PlayerPrefs.SetInt(UnopenedKey, 0);
            PlayerPrefs.Save();
#if UNITY_IOS && !UNITY_EDITOR
            try { _MosseloraNotesRequest(); } catch (Exception e) { Debug.LogWarning("Garden notes unavailable: " + e.Message); }
#endif
        }

        public static void TurnOff()
        {
            PlayerPrefs.SetInt(OnKey, 0);
            PlayerPrefs.Save();
            Cancel();
        }

        public static void NotNow()
        {
            PlayerPrefs.SetInt(AskedKey, Mathf.Min(2, Asked + 1));
            PlayerPrefs.Save();
        }

        /// <summary>
        /// The game opened (by a note or not): the unopened run resets and the pending note goes.
        /// If the note had already come (its hour has passed), returns what it pointed at, so the
        /// garden can show it (I283, rule 8: "opens where it points"); otherwise null.
        /// </summary>
        public static GardenNote Opened()
        {
            GardenNote came = null;
            string due = PlayerPrefs.GetString(DueKey, "");
            if (On && long.TryParse(due, out long ticks) && DateTime.Now.Ticks >= ticks &&
                Enum.TryParse(PlayerPrefs.GetString(TargetKey, ""), out NoteTarget target))
                came = new GardenNote { Target = target, Thing = PlayerPrefs.GetInt(ThingKey, -1) };
            PlayerPrefs.DeleteKey(DueKey);
            PlayerPrefs.SetInt(UnopenedKey, 0);
            PlayerPrefs.Save();
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (devTestActive) return came; // I284: Unity reports a resume right after launch
#endif
            Cancel();
            return came;
        }

        /// <summary>
        /// The game is closing: queue the one note the Core plan (GardenNote.ForWeek) gives for the
        /// next due day, if something has arrived and is waiting. Nothing else is a source of notes.
        /// </summary>
        public static GardenNote Schedule(WorldModel world, bool unreadLetter)
        {
            if (!On) return null;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (devTestActive) return null; // I284: leave the test note queued while backgrounded
#endif
            int unopened = PlayerPrefs.GetInt(UnopenedKey, 0);
            DateTime when = GardenNote.NextTime(DateTime.Now, Hour);
            List<PlannedNote> plan = GardenNote.ForWeek(world, GardenNote.DayNumber(when), unreadLetter, On, unopened);
            if (plan.Count == 0) { Cancel(); return null; }
            GardenNote note = plan[0].Note;
            double seconds = (when - DateTime.Now).TotalSeconds;
            // Counted now; reset when the game opens. Three in a row unopened and it stops.
            PlayerPrefs.SetInt(UnopenedKey, unopened + 1);
            PlayerPrefs.SetString(DueKey, when.Ticks.ToString());
            PlayerPrefs.SetString(TargetKey, note.Target.ToString());
            PlayerPrefs.SetInt(ThingKey, note.Thing);
            PlayerPrefs.Save();
#if UNITY_IOS && !UNITY_EDITOR
            try { _MosseloraNotesSchedule(note.From, note.Body, seconds, note.Target.ToString()); }
            catch (Exception e) { Debug.LogWarning("Garden notes unavailable: " + e.Message); }
#else
            Debug.Log("MOSSELORA_GARDEN_NOTE " + when.ToString("HH:mm") + " " + note.From + ": " + note.Body);
#endif
            return note;
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        /// <summary>
        /// I284, development builds only: launched with MosseloraNoteTest set (on the simulator,
        /// SIMCTL_CHILD_MosseloraNoteTest=1), turn notes on and queue the real note the garden would
        /// send (or a clearly marked test line when nothing is waiting) 75 seconds from now, so
        /// delivery can be seen on a simulator or device.
        /// </summary>
        private static bool devTestActive;

        public static void DevTest(WorldModel world)
        {
            if (DevArgs.Read("-MosseloraNoteTest") == null) return;
            devTestActive = true;
            TurnOn(19);
            // Only the Core plan's note: with nothing arrived there is nothing to send, and the test says so.
            List<PlannedNote> plan = GardenNote.ForWeek(world, GardenNote.DayNumber(DateTime.Now), false, true, 0);
            if (plan.Count == 0) { Debug.Log("MOSSELORA_NOTE_TEST nothing has arrived: no note to send"); return; }
            GardenNote note = plan[0].Note;
#if UNITY_IOS && !UNITY_EDITOR
            try { _MosseloraNotesSchedule(note.From, note.Body, 75, note.Target.ToString()); }
            catch (Exception e) { Debug.LogWarning("Garden notes unavailable: " + e.Message); }
#endif
            Debug.Log("MOSSELORA_NOTE_TEST in 75 s: " + note.From + ": " + note.Body);
        }
#endif

        private static void Cancel()
        {
#if UNITY_IOS && !UNITY_EDITOR
            try { _MosseloraNotesCancel(); } catch (Exception) { }
#endif
        }
    }
}
