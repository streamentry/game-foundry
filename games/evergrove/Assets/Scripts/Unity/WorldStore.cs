using System;
using System.IO;
using Evergrove.Core;
using UnityEngine;

namespace Evergrove.UnityClient
{
    /// <summary>
    /// Offline-first versioned snapshots. The filename stays v2 for upgrade
    /// compatibility although the active data schema is v4.
    /// Failed validation never authorizes silently erasing old player files.
    /// </summary>
    public static class WorldStore
    {
        private const string LegacyKey = "evergrove:unity:world:v1";
        private const string FileName = "evergrove-world-v2.json";

        private static string SaveFile { get { return Path.Combine(Application.persistentDataPath, FileName); } }
        private static string BackupFile { get { return SaveFile + ".bak"; } }

        /// <summary>
        /// True when files exist but no slot can be restored. A deliberate
        /// user reset is required before replacing them with a new game.
        /// </summary>
        public static bool RecoveryBlocked { get; private set; }

        // Local time of the last successful primary write this session, or the primary file's
        // write time when the game loaded from the primary. Null when unknown (loaded from backup
        // or staging, or the file cannot be read). Used only for the "saved on this phone" line.
        private static DateTime? lastSavedLocal;
        public static DateTime? LastSavedLocal { get { return lastSavedLocal; } }

        private static DateTime? PrimaryWriteTime()
        {
            try { return File.Exists(SaveFile) ? File.GetLastWriteTime(SaveFile) : (DateTime?)null; }
            catch (Exception) { return null; }
        }

        private static bool ValidSnapshot(string json)
        {
            try
            {
                WorldModel candidate;
                return !string.IsNullOrEmpty(json) &&
                    WorldModel.TryRestore(JsonUtility.FromJson<WorldSnapshot>(json), out candidate);
            }
            catch (Exception) { return false; }
        }

        public static WorldModel Load()
        {
            RecoveryBlocked = false;
            lastSavedLocal = null;
            string payload, origin;
            if (SaveJournal.TryLoad(SaveFile, ValidSnapshot, out payload, out origin))
            {
                WorldModel restored;
                if (WorldModel.TryRestore(JsonUtility.FromJson<WorldSnapshot>(payload), out restored))
                {
                    if (origin != "primary")
                        Debug.LogWarning("Evergrove recovered a " + origin +
                            " save. Recent progress may have been lost; verify before continuing.");
                    else
                        lastSavedLocal = PrimaryWriteTime();
                    // Do not copy fallback bytes to the primary during load.
                    // A later validated save repairs the primary while keeping
                    // a good backup when the previous primary is corrupt.
                    return restored;
                }
            }

            // Distinguish a fresh install from an unrecoverable older save.
            // Blocking prevents the next move from overwriting corrupt slots.
            RecoveryBlocked = SaveJournal.HasAnySlot(SaveFile);
            if (PlayerPrefs.HasKey(LegacyKey))
            {
                try
                {
                    var old = JsonUtility.FromJson<WorldSnapshot>(PlayerPrefs.GetString(LegacyKey));
                    WorldModel imported;
                    if (WorldModel.TryRestore(old, out imported))
                    {
                        // When newer files exist but cannot be read, keep them
                        // untouched; the old PlayerPrefs snapshot is still
                        // playable in memory, but writing remains blocked.
                        if (!RecoveryBlocked && Save(imported))
                        {
                            PlayerPrefs.DeleteKey(LegacyKey);
                            PlayerPrefs.Save();
                        }
                        return imported;
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogWarning("Legacy Evergrove save could not be upgraded: " +
                        ex.GetType().Name);
                }
            }

            if (RecoveryBlocked)
                Debug.LogError("Unreadable Evergrove save retained. Saving is disabled until an explicit reset.");
            return WorldModel.New();
        }

        public static bool Save(WorldModel world)
        {
            if (RecoveryBlocked || world == null) return false;
            try
            {
                string payload = JsonUtility.ToJson(world.Export());
                bool committed = SaveJournal.Commit(SaveFile, payload, ValidSnapshot);
                if (committed) lastSavedLocal = DateTime.Now;
                return committed;
            }
            catch (Exception ex)
            {
                Debug.LogWarning("Evergrove could not write progress: " + ex.GetType().Name);
                return false;
            }
        }

        /// <summary>
        /// Return false if *any* file or preference could not be deleted.
        /// Callers must not display a successful reset on partial deletion.
        /// </summary>
        public static bool Clear()
        {
            bool success = true;
            foreach (string path in new[] { SaveFile, BackupFile, SaveFile + ".tmp" })
            {
                try { if (File.Exists(path)) File.Delete(path); }
                catch (Exception ex)
                {
                    Debug.LogWarning("Could not clear Evergrove file: " + ex.GetType().Name);
                    success = false;
                }
            }
            if (success)
            {
                try
                {
                    PlayerPrefs.DeleteKey(LegacyKey);
                    PlayerPrefs.Save();
                }
                catch (Exception ex)
                {
                    Debug.LogWarning("Could not clear legacy save: " + ex.GetType().Name);
                    success = false;
                }
            }
            lastSavedLocal = null;
            RecoveryBlocked = !success;
            return success;
        }
    }
}
