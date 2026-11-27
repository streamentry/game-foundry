#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System.Collections;
using UnityEngine;

namespace Evergrove.UnityClient
{
    /// <summary>
    /// Checklists 49 and 54, tour shots: a night creature on every open stretch at 22:00 (one shot
    /// per stretch), the morning's dew board at 07:00 and the evening's lantern letter at 19:00.
    /// Part of the dev QA tour (DevQaTour.cs); the same guard applies.
    /// </summary>
    public sealed partial class DevQaTour
    {
        private IEnumerator DayRhythmShots()
        {
            app.QaSetClock(22);
            int openStretches = app.QaOpenReaches();
            for (int reach = 0; reach < openStretches; reach++)
            {
                app.QaTravel(reach);
                app.QaShowHub(true);
                for (int f = 0; f < 10; f++) yield return null;
                yield return new WaitForSecondsRealtime(0.5f);
                yield return new WaitForEndOfFrame();
                SavePng("20-night-reach-" + reach + ".png");
                app.QaShowHub(false);
            }
            app.QaSetClock(7);
            app.QaShowHub(true);
            for (int f = 0; f < 10; f++) yield return null;
            yield return new WaitForSecondsRealtime(0.5f);
            yield return new WaitForEndOfFrame();
            SavePng("20-morning-dew.png");
            app.QaShowHub(false);
            app.QaSetClock(19);
            app.QaShowHub(true);
            for (int f = 0; f < 10; f++) yield return null;
            yield return new WaitForSecondsRealtime(0.5f);
            yield return new WaitForEndOfFrame();
            SavePng("20-evening-lantern.png");
            app.QaShowHub(false);
            app.QaSetClock(13);
        }
    }
}
#endif
