using System;
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

        /// <summary>The game opened (by a note or not): the unopened run resets, the pending note goes.</summary>
        public static void Opened()
        {
            PlayerPrefs.SetInt(UnopenedKey, 0);
            PlayerPrefs.Save();
            Cancel();
        }

        /// <summary>The game is closing: queue tomorrow's (or today's) one note, if anything is waiting.</summary>
        public static GardenNote Schedule(WorldModel world, bool unreadLetter)
        {
            if (!On) return null;
            int unopened = PlayerPrefs.GetInt(UnopenedKey, 0);
            if (!GardenNote.MaySchedule(unopened)) { Cancel(); return null; }
            GardenNote note = GardenNote.For(world, unreadLetter);
            if (note == null) { Cancel(); return null; }
            DateTime when = GardenNote.NextTime(DateTime.Now, Hour);
            double seconds = (when - DateTime.Now).TotalSeconds;
            // Counted now; reset when the game opens. Three in a row unopened and it stops.
            PlayerPrefs.SetInt(UnopenedKey, unopened + 1);
            PlayerPrefs.Save();
#if UNITY_IOS && !UNITY_EDITOR
            try { _MosseloraNotesSchedule(note.From, note.Body, seconds, note.Target.ToString()); }
            catch (Exception e) { Debug.LogWarning("Garden notes unavailable: " + e.Message); }
#else
            Debug.Log("MOSSELORA_GARDEN_NOTE " + when.ToString("HH:mm") + " " + note.From + ": " + note.Body);
#endif
            return note;
        }

        private static void Cancel()
        {
#if UNITY_IOS && !UNITY_EDITOR
            try { _MosseloraNotesCancel(); } catch (Exception) { }
#endif
        }
    }
}
