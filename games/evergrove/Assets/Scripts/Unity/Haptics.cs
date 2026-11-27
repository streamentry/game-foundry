using System;
using UnityEngine;
#if UNITY_IOS && !UNITY_EDITOR
using System.Runtime.InteropServices;
#endif

namespace Evergrove.UnityClient
{
    /// <summary>
    /// Haptics with meaning (checklist 29, I140 / EG-013). Exactly three cues:
    /// Placed (a move or a thing placed), Finished (a board or thing completed)
    /// and Arrived (a letter, parcel or gift arrived). Each one echoes a moment
    /// that motion and sound already show; none is the only signal. One switch,
    /// "evergrove:haptics" (default on), turns all three off. The OS haptics
    /// setting is honoured on both platforms.
    /// </summary>
    internal static class Haptics
    {
        public const string SwitchKey = "evergrove:haptics";

        public static bool Enabled
        {
            // Until the player sets this switch, it follows their earlier Gentle motion choice, which used
            // to gate haptics: someone who turned motion off doesn't suddenly feel the phone buzz.
            get { return PlayerPrefs.HasKey(SwitchKey) ? PlayerPrefs.GetInt(SwitchKey, 1) == 1
                                                       : PlayerPrefs.GetInt("evergrove:gentle-motion:v1", 1) == 1; }
            set
            {
                PlayerPrefs.SetInt(SwitchKey, value ? 1 : 0);
                PlayerPrefs.Save();
            }
        }

#if UNITY_IOS && !UNITY_EDITOR
        [DllImport("__Internal")] private static extern void _MosseloraHapticPlaced();
        [DllImport("__Internal")] private static extern void _MosseloraHapticFinished();
        [DllImport("__Internal")] private static extern void _MosseloraHapticArrived();
#endif

        // android.view.HapticFeedbackConstants
        private const int KeyboardTap = 3;   // soft, for arrivals
        private const int ClockTick = 4;     // light, for placements
        private const int VirtualKey = 1;
        private const int Confirm = 16;      // API 30+, for completions

        private static bool failed;

        // A move landed: a tile turned, a thing placed on the map or a plant put in.
        public static void Placed()
        {
            if (!Enabled || failed) return;
#if UNITY_IOS && !UNITY_EDITOR
            try { _MosseloraHapticPlaced(); } catch (Exception error) { Fail(error); }
#elif UNITY_ANDROID && !UNITY_EDITOR
            Perform(ClockTick);
#endif
        }

        // A board or a thing is complete.
        public static void Finished()
        {
            if (!Enabled || failed) return;
#if UNITY_IOS && !UNITY_EDITOR
            try { _MosseloraHapticFinished(); } catch (Exception error) { Fail(error); }
#elif UNITY_ANDROID && !UNITY_EDITOR
            Perform(AndroidApi() >= 30 ? Confirm : VirtualKey);
#endif
        }

        // Something came to the door: a letter, a parcel or a gift.
        public static void Arrived()
        {
            if (!Enabled || failed) return;
#if UNITY_IOS && !UNITY_EDITOR
            try { _MosseloraHapticArrived(); } catch (Exception error) { Fail(error); }
#elif UNITY_ANDROID && !UNITY_EDITOR
            Perform(KeyboardTap);
#endif
        }

#if UNITY_ANDROID && !UNITY_EDITOR
        private static int AndroidApi()
        {
            try
            {
                using (var version = new AndroidJavaClass("android.os.Build$VERSION"))
                    return version.GetStatic<int>("SDK_INT");
            }
            catch (Exception) { return 0; }
        }

        private static void Perform(int constant)
        {
            try
            {
                using (var player = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                {
                    AndroidJavaObject activity = player.GetStatic<AndroidJavaObject>("currentActivity");
                    if (activity == null) return;
                    activity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
                    {
                        try
                        {
                            using (AndroidJavaObject window = activity.Call<AndroidJavaObject>("getWindow"))
                            using (AndroidJavaObject view = window.Call<AndroidJavaObject>("getDecorView"))
                                view.Call<bool>("performHapticFeedback", constant);
                        }
                        catch (Exception error) { Fail(error); }
                    }));
                }
            }
            catch (Exception error) { Fail(error); }
        }
#endif

        // One warning, then silence: a missing haptic engine never breaks play.
        private static void Fail(Exception error)
        {
            failed = true;
            Debug.LogWarning("Mosselora haptics unavailable: " + error.Message);
        }
    }
}
