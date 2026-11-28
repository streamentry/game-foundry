#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System.Collections;
using UnityEngine;
using UnityEngine.Profiling;

namespace Evergrove.UnityClient
{
    /// <summary>
    /// Checklist 50, the memory test: a river of 34 stretches is scrolled from the top to the bottom, one
    /// stretch at a time, first with the stretch window off and then on. Each step logs the managed heap,
    /// Unity's allocated memory and how many things on the land are live (MOSSELORA_WINDOW). Part of the
    /// dev QA tour (DevQaTour.cs); the same guard applies.
    /// </summary>
    public sealed partial class DevQaTour
    {
        private const int LongRiver = 34, StretchesScrolled = 30;

        private IEnumerator WindowShots()
        {
            app.QaSetClock(13);
            app.QaLongRiver(LongRiver);
            foreach (bool on in new[] { false, true })
            {
                StretchWindow.Enabled = on;
                app.QaShowHub(false);
                app.QaShowHub(true);
                for (int f = 0; f < 10; f++) yield return null;
                System.GC.Collect();
                yield return null;
                string pass = on ? "on" : "off";
                Debug.Log("MOSSELORA_WINDOW " + pass + " stretches " + app.QaHubStretches());
                for (int step = 0; step <= StretchesScrolled; step++)
                {
                    app.QaScrollHub(1f - step / (float)LongRiver);
                    for (int f = 0; f < 3; f++) yield return null;
                    Debug.Log("MOSSELORA_WINDOW " + pass + " step " + step +
                        " heapKB " + (System.GC.GetTotalMemory(false) / 1024) +
                        " unityKB " + (Profiler.GetTotalAllocatedMemoryLong() / 1024) +
                        " live " + app.QaHubLive());
                    if (on && (step == 0 || step == 15 || step == StretchesScrolled))
                    {
                        yield return new WaitForEndOfFrame();
                        SavePng("25-window-stretch-" + step.ToString("00") + ".png");
                    }
                }
            }
            StretchWindow.Enabled = true;
            app.QaLongRiver(0);
            app.QaShowHub(false);
        }
    }
}
#endif
