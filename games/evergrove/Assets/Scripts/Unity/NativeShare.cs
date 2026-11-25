using System;
using UnityEngine;
#if UNITY_IOS && !UNITY_EDITOR
using System.Runtime.InteropServices;
#endif

namespace Evergrove.UnityClient
{
    /// <summary>
    /// Opens the platform share sheet with plain text (I149). Returns false when
    /// no sheet could be shown (editor, desktop, failure), so the caller can fall
    /// back to the clipboard. Nothing leaves the device unless the player sends it.
    /// </summary>
    internal static class NativeShare
    {
#if UNITY_IOS && !UNITY_EDITOR
        [DllImport("__Internal")] private static extern bool _MosseloraShareText(string text);
#endif

        public static bool Text(string text, string title)
        {
            if (string.IsNullOrEmpty(text)) return false;
#if UNITY_IOS && !UNITY_EDITOR
            try { return _MosseloraShareText(text); }
            catch (Exception error) { Debug.LogWarning("Mosselora share unavailable: " + error.Message); return false; }
#elif UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                using (var player = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                using (var intents = new AndroidJavaClass("android.content.Intent"))
                using (var intent = new AndroidJavaObject("android.content.Intent"))
                {
                    AndroidJavaObject activity = player.GetStatic<AndroidJavaObject>("currentActivity");
                    if (activity == null) return false;
                    intent.Call<AndroidJavaObject>("setAction", intents.GetStatic<string>("ACTION_SEND"));
                    intent.Call<AndroidJavaObject>("setType", "text/plain");
                    intent.Call<AndroidJavaObject>("putExtra", intents.GetStatic<string>("EXTRA_TEXT"), text);
                    using (AndroidJavaObject chooser = intents.CallStatic<AndroidJavaObject>("createChooser", intent, title))
                        activity.Call("startActivity", chooser);
                    return true;
                }
            }
            catch (Exception error) { Debug.LogWarning("Mosselora share unavailable: " + error.Message); return false; }
#else
            return false;
#endif
        }
    }
}
