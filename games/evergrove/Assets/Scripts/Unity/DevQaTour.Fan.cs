#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System.Collections;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

namespace Evergrove.UnityClient
{
    /// <summary>
    /// Checklist 15, the logged session: the fan opens, "Look" (low in the column) is used once, and the
    /// fan opens again. Each time the distance from the corner button to every token is logged
    /// (MOSSELORA_FAN, canvas points), so the second tap's distance before and after is on record.
    /// Part of the dev QA tour (DevQaTour.cs); the same guard applies.
    /// </summary>
    public sealed partial class DevQaTour
    {
        private const string FanTried = "Just look at the garden";

        private IEnumerator FanShots()
        {
            app.QaSetClock(13);
            app.QaShowHub(true);
            for (int f = 0; f < 8; f++) yield return null;
            Press("Open menu");
            for (int f = 0; f < 12; f++) yield return null;
            yield return new WaitForSecondsRealtime(0.4f);
            yield return new WaitForEndOfFrame();
            LogFan("before");
            SavePng("24-fan-before.png");
            Press(FanTried); // the token used: quiet garden
            for (int f = 0; f < 6; f++) yield return null;
            app.QaQuiet(false);
            for (int f = 0; f < 6; f++) yield return null;
            Press("Open menu");
            for (int f = 0; f < 12; f++) yield return null;
            yield return new WaitForSecondsRealtime(0.4f);
            yield return new WaitForEndOfFrame();
            LogFan("after");
            SavePng("24-fan-remembers.png");
            Press("Open menu"); // folds the fan away
            for (int f = 0; f < 4; f++) yield return null;
            app.QaShowHub(false);
        }

        // Distance from the corner button's centre to each fan token's centre, in canvas points.
        private static void LogFan(string when)
        {
            RectTransform corner = null;
            Transform column = null;
            foreach (Button button in FindObjectsByType<Button>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
                if (button.name == "Open menu") corner = (RectTransform)button.transform;
            foreach (RectTransform rect in FindObjectsByType<RectTransform>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
                if (rect.name == "Fan column") column = rect;
            if (corner == null || column == null) { Debug.Log("MOSSELORA_FAN " + when + " no fan"); return; }
            float scale = Mathf.Max(0.0001f, corner.lossyScale.x);
            Vector3 from = Centre(corner);
            for (int i = 0; i < column.childCount; i++)
            {
                var token = column.GetChild(i) as RectTransform;
                if (token == null || !token.gameObject.activeInHierarchy) continue;
                float points = Vector3.Distance(from, Centre(token)) / scale;
                Debug.Log("MOSSELORA_FAN " + when + " " + i + " " + token.name + " " +
                    points.ToString("0", CultureInfo.InvariantCulture));
            }
        }

        private static Vector3 Centre(RectTransform rect)
        {
            var corners = new Vector3[4];
            rect.GetWorldCorners(corners);
            return (corners[0] + corners[2]) * 0.5f;
        }
    }
}
#endif
