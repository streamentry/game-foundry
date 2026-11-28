using System;
using System.Collections;
using Evergrove.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Evergrove.UnityClient
{
    /// <summary>
    /// Checklist 94: a downward swipe that starts on a sheet or card closes it, as iOS sheets do. Only
    /// drags that no child handles reach this component: a ScrollRect inside keeps its own drags, so
    /// scrolling is never hijacked. While the finger is down the panel may follow it a little (its
    /// localPosition only, clamped); a release short of the swipe, or a cancel, eases it back.
    /// Layout is never touched. The close button stays; this is the one-thumb route to the same action.
    /// </summary>
    internal sealed class SwipeToClose : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        /// <summary>The same action the panel's close button runs.</summary>
        public System.Action Close;

        /// <summary>False under reduced motion: the swipe still closes, but nothing follows the finger.</summary>
        public bool Follow;

        private const float EaseSeconds = 0.18f;

        private RectTransform rect;
        private Vector3 restPosition;
        private Coroutine easing;

        /// <summary>
        /// Attaches the swipe to a sheet or card panel, or updates it when already attached (sheets are
        /// rebuilt with their page). An empty panel gets a clear graphic so a drag on it still reaches here.
        /// </summary>
        internal static SwipeToClose Attach(RectTransform panel, Action close, bool follow)
        {
            if (panel.GetComponent<Graphic>() == null)
            {
                var clear = panel.gameObject.AddComponent<Image>();
                clear.color = new Color(0f, 0f, 0f, 0f);
                clear.raycastTarget = true;
            }
            SwipeToClose swipe = panel.GetComponent<SwipeToClose>();
            if (swipe == null) swipe = panel.gameObject.AddComponent<SwipeToClose>();
            swipe.Close = close;
            swipe.Follow = follow;
            return swipe;
        }

        /// <summary>True when the control sits inside a sheet or card that has an enabled SwipeToClose.</summary>
        internal static bool Closable(Transform control)
        {
            for (Transform t = control; t != null; t = t.parent)
            {
                var swipe = t.GetComponent<SwipeToClose>();
                if (swipe != null && swipe.isActiveAndEnabled) return true;
            }
            return false;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            rect = transform as RectTransform;
            if (rect == null) return;
            if (easing != null)
            {
                StopCoroutine(easing);
                easing = null;
                rect.localPosition = restPosition;
            }
            else
            {
                restPosition = rect.localPosition;
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!Follow || rect == null) return;
            float scale = CanvasScale();
            float dy = (eventData.pressPosition.y - eventData.position.y) / scale; // positive = downward
            rect.localPosition = restPosition + new Vector3(0f, -SwipeClose.FollowOffset(dy), 0f);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (rect == null) return;
            float scale = CanvasScale();
            float dx = (eventData.position.x - eventData.pressPosition.x) / scale;
            float dy = (eventData.pressPosition.y - eventData.position.y) / scale; // positive = downward
            if (SwipeClose.Closes(dx, dy))
            {
                rect.localPosition = restPosition;
                Action close = Close;
                close?.Invoke();
                return;
            }
            if (Follow && rect.localPosition != restPosition && gameObject.activeInHierarchy)
                easing = StartCoroutine(EaseBack(rect.localPosition));
        }

        private void OnDisable()
        {
            easing = null;
            if (rect != null) rect.localPosition = restPosition;
        }

        private IEnumerator EaseBack(Vector3 from)
        {
            float t = 0f;
            while (t < EaseSeconds)
            {
                t += Time.unscaledDeltaTime;
                float k = 1f - Mathf.Pow(1f - Mathf.Clamp01(t / EaseSeconds), 3f);
                rect.localPosition = Vector3.LerpUnclamped(from, restPosition, k);
                yield return null;
            }
            rect.localPosition = restPosition;
            easing = null;
        }

        // Screen pixels to canvas points: the root canvas's scale factor (1 when unknown).
        private float CanvasScale()
        {
            var canvas = GetComponentInParent<Canvas>();
            if (canvas == null || canvas.rootCanvas == null || canvas.rootCanvas.scaleFactor <= 0f) return 1f;
            return canvas.rootCanvas.scaleFactor;
        }
    }
}
