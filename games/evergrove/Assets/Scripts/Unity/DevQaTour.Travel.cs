#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System.Collections;
using UnityEngine;

namespace Evergrove.UnityClient
{
    /// <summary>
    /// Checklists 43 and 45, tour shots: the boat travel sheet and arrival, then the river map
    /// opened from its own menu entry. Part of the dev QA tour (DevQaTour.cs); the same guard applies.
    /// </summary>
    public sealed partial class DevQaTour
    {
        private IEnumerator BoatAndMapShots()
        {
            // Checklist 43: travel by boat, from the hub, to the furthest open stretch.
            for (int f = 0; f < 8; f++) yield return null;
            app.QaOpenTravel();
            yield return new WaitForSecondsRealtime(0.5f);
            yield return new WaitForEndOfFrame();
            SavePng("17zc-boat-travel.png");
            app.QaTravel(app.QaOpenReaches() - 1);
            yield return new WaitForSecondsRealtime(1.5f);
            yield return new WaitForEndOfFrame();
            SavePng("17zd-boat-arrived.png");
            // Checklist 45: the river map over the hub (the menu entry's own action), then closed by its button.
            app.QaOpenRiverMap();
            for (int f = 0; f < 8; f++) yield return null;
            yield return new WaitForSecondsRealtime(0.5f);
            yield return new WaitForEndOfFrame();
            SavePng("17ze-river-map.png");
            Press("Close river map");
        }
    }
}
#endif
