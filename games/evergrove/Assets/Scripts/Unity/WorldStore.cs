using System;
using Evergrove.Core;
using UnityEngine;

namespace Evergrove.UnityClient
{
    /// <summary>Local-first demo persistence. No cloud sync or telemetry.</summary>
    public static class WorldStore
    {
        private const string SaveKey = "evergrove:unity:world:v1";

        public static WorldModel Load()
        {
            if (!PlayerPrefs.HasKey(SaveKey)) return WorldModel.New();
            try
            {
                string raw = PlayerPrefs.GetString(SaveKey);
                var restored = JsonUtility.FromJson<WorldSnapshot>(raw);
                return WorldModel.Restore(restored);
            }
            catch (Exception exception)
            {
                Debug.LogWarning("Evergrove save could not be read: " + exception.GetType().Name);
                return WorldModel.New();
            }
        }

        public static void Save(WorldModel world)
        {
            try
            {
                PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(world.Export()));
                PlayerPrefs.Save();
            }
            catch (Exception exception)
            {
                Debug.LogWarning("Evergrove could not save progress: " + exception.GetType().Name);
            }
        }

        public static void Clear()
        {
            PlayerPrefs.DeleteKey(SaveKey);
            PlayerPrefs.Save();
        }
    }
}
