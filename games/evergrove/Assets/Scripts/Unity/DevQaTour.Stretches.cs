#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System.Collections;
using Evergrove.Core;
using UnityEngine;

namespace Evergrove.UnityClient
{
    /// <summary>
    /// Checklist 42, tour shots: three stretches open at once, the whole-river lookout, one shot where three
    /// stretches are visible (32-three-stretches.png), then each of the first three stretches with its neighbour
    /// standing in it (32-stretch-1.png and 32-stretch-2.png). Logs MOSSELORA_STRETCH <reach> <kind> neighbour <name>.
    /// The river length, the lookout and the clock are put back at the end. Part of the dev QA tour
    /// (DevQaTour.cs); the same guard applies.
    /// </summary>
    public sealed partial class DevQaTour
    {
        private const int StretchTourRiver = 6, StretchesShown = 3;

        private IEnumerator StretchShots()
        {
            app.QaSetClock(13);
            bool longRiver = app.QaOpenReaches() < StretchesShown;
            if (longRiver) app.QaLongRiver(StretchTourRiver);
            app.QaShowHub(true);
            for (int f = 0; f < 10; f++) yield return null;
            app.QaQuiet(true, true);
            for (int f = 0; f < 6; f++) yield return null;
            yield return new WaitForEndOfFrame();
            SavePng("32-three-stretches.png");
            app.QaQuiet(false);
            for (int f = 0; f < 4; f++) yield return null;
            app.QaShowHub(false);
            for (int reach = 0; reach < StretchesShown; reach++)
            {
                // The hub first, then the boat: travel moves the hub's own view, so it needs the hub built.
                app.QaShowHub(true);
                for (int f = 0; f < 10; f++) yield return null;
                app.QaTravel(reach);
                yield return new WaitForSecondsRealtime(1.5f);
                yield return new WaitForEndOfFrame();
                if (reach >= 1) SavePng("32-stretch-" + reach + ".png");
                StretchStand(reach);
                app.QaShowHub(false);
            }
            if (longRiver) app.QaLongRiver(0);
            app.QaShowHub(false);
            app.QaSetClock(13);
        }

        private static void StretchStand(int reach)
        {
            StretchKind kind = StretchCharacter.KindOf(reach);
            Debug.Log("MOSSELORA_STRETCH " + reach + " " + kind + " neighbour " + LocalizedVillage.Name(StretchCharacter.Neighbour(kind)));
        }
    }
}
#endif
