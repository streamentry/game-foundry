#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Evergrove.UnityClient
{
    /// <summary>
    /// Checklist 13, tour shots: a held thing shows Play and Move beside it on the hub. The tour logs the
    /// canvas-point distance from the held thing's centre to its Play token (MOSSELORA_THING play-distance),
    /// presses Move (the hint shows and the thing stays in hand), then puts the thing down where it stood.
    /// Part of the dev QA tour (DevQaTour.cs); the same guard applies.
    /// </summary>
    public sealed partial class DevQaTour
    {
        private IEnumerator ThingActionShots()
        {
            app.QaShowHub(true);
            for (int f = 0; f < 10; f++) yield return null;
            int thing = app.QaHoldThing();
            if (thing < 0)
            {
                Debug.Log("MOSSELORA_THING no thing to hold");
                app.QaShowHub(false);
                yield break;
            }
            for (int f = 0; f < 10; f++) yield return null;
            yield return new WaitForSecondsRealtime(0.4f);
            yield return new WaitForEndOfFrame();
            SavePng("30-thing-actions.png");
            Debug.Log("MOSSELORA_THING play-distance " + PlayDistanceFromThing().ToString("0.0"));

            Press("Move it");
            for (int f = 0; f < 6; f++) yield return null;
            yield return new WaitForEndOfFrame();
            SavePng("30-thing-move.png");

            app.QaPutDownThing();
            for (int f = 0; f < 6; f++) yield return null;
            app.QaShowHub(false);
        }

        // Distance in canvas points between the held thing's centre and the centre of its Play token.
        private static float PlayDistanceFromThing()
        {
            RectTransform ring = FindActiveRect("In your hand");
            RectTransform play = FindActiveRect("Play its puzzle");
            if (ring == null || play == null) return -1f;
            RectTransform thing = null;
            float nearest = float.MaxValue;
            foreach (RectTransform node in FindObjectsByType<RectTransform>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
            {
                if (node.name != "On the map") continue;
                float d = (node.position - ring.position).sqrMagnitude;
                if (d < nearest) { nearest = d; thing = node; }
            }
            if (thing == null) return -1f;
            Canvas canvas = play.GetComponentInParent<Canvas>();
            if (canvas == null) return -1f;
            Transform root = canvas.rootCanvas.transform;
            Vector2 a = root.InverseTransformPoint(thing.TransformPoint(thing.rect.center));
            Vector2 b = root.InverseTransformPoint(play.TransformPoint(play.rect.center));
            return Vector2.Distance(a, b);
        }

        private static RectTransform FindActiveRect(string name)
        {
            foreach (RectTransform node in FindObjectsByType<RectTransform>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
                if (node.name == name) return node;
            return null;
        }
    }
}
#endif
