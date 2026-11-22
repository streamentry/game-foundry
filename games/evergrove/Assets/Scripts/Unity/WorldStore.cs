using System;
using System.IO;
using Evergrove.Core;
using UnityEngine;

namespace Evergrove.UnityClient
{
    /// <summary>
    /// Offline-first, versioned snapshots with a recoverable backup.
    /// Old PlayerPrefs v1 progress is migrated once when this build loads.
    /// This is not encrypted storage or a multiplayer authority.
    /// </summary>
    public static class WorldStore
    {
        private const string LegacyKey = "evergrove:unity:world:v1";
        private const string FileName = "evergrove-world-v2.json";

        private static string SaveFile { get { return Path.Combine(Application.persistentDataPath, FileName); } }
        private static string BackupFile { get { return SaveFile + ".bak"; } }

        private static bool TryRead(string path, out WorldModel model)
        {
            model = null;
            if (!File.Exists(path)) return false;
            try
            {
                string raw = File.ReadAllText(path);
                WorldSnapshot candidate = JsonUtility.FromJson<WorldSnapshot>(raw);
                return WorldModel.TryRestore(candidate, out model);
            }
            catch (Exception ex)
            {
                Debug.LogWarning("Evergrove save file unreadable: " + ex.GetType().Name);
                return false;
            }
        }

        public static WorldModel Load()
        {
            WorldModel world;
            if (TryRead(SaveFile, out world)) return world;
            if (TryRead(BackupFile, out world))
            {
                Debug.LogWarning("Evergrove recovered a previous valid save.");
                try { File.Copy(BackupFile, SaveFile, true); }
                catch (Exception ex) {
                    Debug.LogWarning("Could not restore backup to primary: " + ex.GetType().Name);
                }
                return world;
            }
            if (PlayerPrefs.HasKey(LegacyKey))
            {
                try
                {
                    WorldSnapshot old = JsonUtility.FromJson<WorldSnapshot>(PlayerPrefs.GetString(LegacyKey));
                    if (WorldModel.TryRestore(old, out world))
                    {
                        if (Save(world))
                        {
                            PlayerPrefs.DeleteKey(LegacyKey);
                            PlayerPrefs.Save();
                        }
                        return world;
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogWarning("Legacy Evergrove save could not be upgraded: " + ex.GetType().Name);
                }
            }
            return WorldModel.New();
        }

        public static bool Save(WorldModel world)
        {
            string destination = SaveFile;
            string temporary = destination + ".tmp";
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(destination));
                string payload = JsonUtility.ToJson(world.Export());
                // Refuse a round-trip that would destroy the current session.
                WorldModel check;
                if (!WorldModel.TryRestore(JsonUtility.FromJson<WorldSnapshot>(payload), out check))
                    return false;
                File.WriteAllText(temporary, payload);
                if (File.Exists(destination))
                {
                    try
                    {
                        File.Replace(temporary, destination, BackupFile);
                    }
                    catch (PlatformNotSupportedException)
                    {
                        File.Copy(destination, BackupFile, true);
                        File.Copy(temporary, destination, true);
                        File.Delete(temporary);
                    }
                    catch (IOException)
                    {
                        // Some mobile filesystems don't support replacement;
                        // retain a valid backup before the copy.
                        File.Copy(destination, BackupFile, true);
                        File.Copy(temporary, destination, true);
                        File.Delete(temporary);
                    }
                }
                else File.Move(temporary, destination);
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogWarning("Evergrove could not write progress: " + ex.GetType().Name);
                return false;
            }
        }

        public static void Clear()
        {
            foreach (string path in new[] { SaveFile, BackupFile, SaveFile + ".tmp" })
            {
                try { if (File.Exists(path)) File.Delete(path); }
                catch (Exception ex) { Debug.LogWarning("Could not clear Evergrove file: " + ex.GetType().Name); }
            }
            PlayerPrefs.DeleteKey(LegacyKey);
            PlayerPrefs.Save();
        }
    }
}
