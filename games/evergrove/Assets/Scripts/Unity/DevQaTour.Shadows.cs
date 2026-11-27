#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System.Collections;
using UnityEngine;

namespace Evergrove.UnityClient
{
    /// <summary>
    /// Checklist 22, tour shots: the sun shadows at dawn (06:00), noon (12:00) and dusk (19:00), each
    /// on the hub's map. Part of the dev QA tour (DevQaTour.cs); the same guard applies.
    /// </summary>
    public sealed partial class DevQaTour
    {
        private IEnumerator ShadowShots()
        {
            app.QaSetClock(6);
            app.QaShowHub(true);
            for (int f = 0; f < 10; f++) yield return null;
            yield return new WaitForSecondsRealtime(0.5f);
            yield return new WaitForEndOfFrame();
            SavePng("22-shadow-dawn.png");
            app.QaShowHub(false);
            app.QaSetClock(12);
            app.QaShowHub(true);
            for (int f = 0; f < 10; f++) yield return null;
            yield return new WaitForSecondsRealtime(0.5f);
            yield return new WaitForEndOfFrame();
            SavePng("22-shadow-noon.png");
            app.QaShowHub(false);
            app.QaSetClock(19);
            app.QaShowHub(true);
            for (int f = 0; f < 10; f++) yield return null;
            yield return new WaitForSecondsRealtime(0.5f);
            yield return new WaitForEndOfFrame();
            SavePng("22-shadow-dusk.png");
            app.QaShowHub(false);
            app.QaSetClock(13);
        }
    }
}
#endif
