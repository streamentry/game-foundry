#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Evergrove.UnityClient
{
    /// <summary>
    /// Development-build-only facilitator gesture for the M3 participant view:
    /// pressing and holding the target for <see cref="HoldSeconds"/> fires
    /// <see cref="Held"/> once. Releasing, leaving the target or scrolling away
    /// cancels the hold. A tap or an ordinary scroll never triggers it.
    /// </summary>
    internal sealed class FacilitatorHold : MonoBehaviour,
        IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        public const float HoldSeconds = 3f;

        public Action Held;
        private float pressedAt = -1f;

        public void OnPointerDown(PointerEventData eventData) => pressedAt = Time.unscaledTime;
        public void OnPointerUp(PointerEventData eventData) => pressedAt = -1f;
        public void OnPointerExit(PointerEventData eventData) => pressedAt = -1f;

        private void OnDisable() => pressedAt = -1f;

        private void Update()
        {
            if (pressedAt < 0f || Time.unscaledTime - pressedAt < HoldSeconds) return;
            pressedAt = -1f;
            Held?.Invoke();
        }
    }
}
#endif
