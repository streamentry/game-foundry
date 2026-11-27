#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System.Collections;
using System.Globalization;
using UnityEngine;

namespace Evergrove.UnityClient
{
    /// <summary>
    /// Checklist 24, tour shots: the hub at nine in the morning, then the clock moves to dusk and the
    /// hub is redrawn without closing it, so the neighbours walk to the cottage door. Sixteen frames a
    /// tenth of a second apart; each frame logs every neighbour's map position (MOSSELORA_WALK).
    /// Part of the dev QA tour (DevQaTour.cs); the same guard applies.
    /// </summary>
    public sealed partial class DevQaTour
    {
        private IEnumerator WalkShots()
        {
            app.QaSetClock(9);
            app.QaShowHub(true);
            for (int f = 0; f < 10; f++) yield return null;
            yield return new WaitForSecondsRealtime(0.5f); // the morning spots are recorded
            app.QaSetClock(19);
            NeighbourWalker.Pace = 0.25f; // a quarter pace, so the frames catch every step
            app.QaRebuildHub();
            for (int i = 0; i < 16; i++)
            {
                yield return new WaitForSecondsRealtime(0.1f);
                yield return new WaitForEndOfFrame();
                SavePng("23-walk-" + i.ToString("00") + ".png");
                LogNeighbourSpots(i);
            }
            NeighbourWalker.Pace = 1f;
            app.QaShowHub(false);
            app.QaSetClock(13);
        }

        private void LogNeighbourSpots(int frame)
        {
            int n = 0;
            foreach (RectTransform rect in UnityEngine.Object.FindObjectsByType<RectTransform>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
            {
                if (rect.name != "Neighbour on the map") continue;
                var walker = rect.GetComponent<NeighbourWalker>();
                bool walking = walker != null && walker.enabled;
                Vector2 at = rect.anchoredPosition;
                Debug.Log("MOSSELORA_WALK " + frame + " " + n + " " +
                    at.x.ToString("0.0", CultureInfo.InvariantCulture) + "," + at.y.ToString("0.0", CultureInfo.InvariantCulture) +
                    (walking ? " walking" : " still"));
                n++;
            }
        }
    }
}
#endif
