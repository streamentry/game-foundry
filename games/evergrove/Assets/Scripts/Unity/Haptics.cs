using System;
using UnityEngine;
#if UNITY_IOS && !UNITY_EDITOR
using System.Runtime.InteropServices;
#endif

namespace Evergrove.UnityClient
{
    /// <summary>
    /// Light touch feedback (I140, EG-013): a tick per tile turn and a success
    /// pulse on a finished stream. Never the only signal: every haptic moment
    /// also has motion and (optional) sound. The OS haptics setting is honoured
    /// on both platforms; the game calls this only while gentle motion is on.
    /// </summary>
    internal static class Haptics
    {
#if UNITY_IOS && !UNITY_EDITOR
        [DllImport("__Internal")] private static extern void _MosseloraHapticTick();
        [DllImport("__Internal")] private static extern void _MosseloraHapticSuccess();
#endif

        // android.view.HapticFeedbackConstants
        private const int ClockTick = 4;
        private const int Confirm = 16;      // API 30+
        private const int VirtualKey = 1;

        private static bool failed;

        public static void Tick()
        {
            if (failed) return;
#if UNITY_IOS && !UNITY_EDITOR
            try { _MosseloraHapticTick(); } catch (Exception error) { Fail(error); }
#elif UNITY_ANDROID && !UNITY_EDITOR
            Perform(ClockTick);
#endif
        }

        public static void Success()
        {
            if (failed) return;
#if UNITY_IOS && !UNITY_EDITOR
            try { _MosseloraHapticSuccess(); } catch (Exception error) { Fail(error); }
#elif UNITY_ANDROID && !UNITY_EDITOR
            Perform(AndroidApi() >= 30 ? Confirm : VirtualKey);
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
