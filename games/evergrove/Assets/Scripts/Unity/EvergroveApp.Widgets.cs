using System;
using System.Collections;
using System.Globalization;
using Evergrove.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

namespace Evergrove.UnityClient
{
    // A plain tap target that is not a Button, so screen readers don't list it (I127).
    internal sealed class TapCatcher : UnityEngine.MonoBehaviour, UnityEngine.EventSystems.IPointerClickHandler
    {
        public System.Action Tapped;
        public void OnPointerClick(UnityEngine.EventSystems.PointerEventData eventData) => Tapped?.Invoke();
    }
}

namespace Evergrove.UnityClient
{
    // Turns a mark a quarter at a time, with a pause between: the board's own verb (I138).
    internal sealed class QuarterTurns : UnityEngine.MonoBehaviour
    {
        private float clock;
        private float from;

        private void Update()
        {
            clock += UnityEngine.Time.unscaledDeltaTime;
            const float pause = 1.1f, swing = 0.35f;
            if (clock < pause) return;
            float t = UnityEngine.Mathf.Clamp01((clock - pause) / swing);
            float angle = from - 90f * UnityEngine.Mathf.SmoothStep(0f, 1f, t);
            transform.localRotation = UnityEngine.Quaternion.Euler(0f, 0f, angle);
            if (t >= 1f) { clock = 0f; from = angle % 360f; }
        }
    }
}

namespace Evergrove.UnityClient
{
    // Presses a little (down and smaller) just before each quarter turn of the
    // arrow beside it, on the same 1.45s beat as QuarterTurns (I147).
    internal sealed class GhostPress : UnityEngine.MonoBehaviour
    {
        private float clock;
        private UnityEngine.Vector2 rest;
        private UnityEngine.RectTransform rect;

        private void Start()
        {
            rect = (UnityEngine.RectTransform)transform;
            rest = rect.anchoredPosition;
        }

        private void Update()
        {
            clock = (clock + UnityEngine.Time.unscaledDeltaTime) % 1.45f;
            // 0.85–1.25s: down, then up as the arrow swings.
            float press = clock < 0.85f || clock > 1.25f ? 0f
                : UnityEngine.Mathf.Sin((clock - 0.85f) / 0.4f * UnityEngine.Mathf.PI);
            rect.anchoredPosition = rest + new UnityEngine.Vector2(0f, -5f * press);
            transform.localScale = UnityEngine.Vector3.one * (1f - 0.12f * press);
        }
    }
}
