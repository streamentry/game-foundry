using System;
using System.Collections.Generic;
using Evergrove.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Evergrove.UnityClient
{
    /// <summary>
    /// Checklist 22: every thing on the map casts one soft shadow whose angle and length follow the
    /// sun (<see cref="SunShadows"/>). The shadow is a sibling placed just before the thing, so it
    /// draws beneath it, starts at the thing's feet and lies on the ground: its direction is squashed
    /// vertically. It is registered with the view's node list, so each relayout destroys it with the
    /// thing it belongs to. Cosmetic only; it never takes input.
    /// </summary>
    internal static class SunShadow
    {
        /// <summary>Set by the app from the local clock before the map is laid out (EvergroveApp.Daylight.cs).</summary>
        public static DayPhase Phase = DayPhase.Day;

        private const float WidthShare = 0.9f;   // the shadow's base length, as a share of the thing's width
        private const float Thickness = 0.25f;   // its base depth, as a share of the thing's width
        private const float Flatten = 0.35f;     // a shadow on the ground reads shorter up the screen
        private static readonly Color Ink = new Color(0.07f, 0.06f, 0.10f, 1f);

        /// <summary>Adds the sun shadow for <paramref name="thing"/> just beneath it, and records it in <paramref name="nodes"/>.</summary>
        public static void Cast(RectTransform thing, List<RectTransform> nodes)
        {
            SunShadowLook look = SunShadows.For(Phase);
            if (!look.Shows || thing == null || thing.parent == null) return;
            float width = thing.sizeDelta.x;
            var shadow = new GameObject("Sun shadow", typeof(RectTransform));
            shadow.transform.SetParent(thing.parent, false);
            shadow.transform.SetSiblingIndex(thing.GetSiblingIndex());
            var rect = (RectTransform)shadow.transform;
            rect.anchorMin = thing.anchorMin;
            rect.anchorMax = thing.anchorMax;
            rect.pivot = new Vector2(0f, 0.5f); // the left end sits at the feet; the length runs away from them
            rect.sizeDelta = new Vector2(width * WidthShare, width * Thickness);
            rect.anchoredPosition = thing.anchoredPosition;
            rect.localScale = new Vector3(look.Length, 1f, 1f);
            double radians = look.AngleDegrees * Math.PI / 180.0;
            float dx = (float)Math.Cos(radians), dy = (float)Math.Sin(radians) * Flatten;
            rect.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(dy, dx) * Mathf.Rad2Deg);
            var image = shadow.AddComponent<Image>();
            image.sprite = MosseloraArt.SoftGlow();
            image.color = new Color(Ink.r, Ink.g, Ink.b, look.Alpha);
            image.raycastTarget = false;
            nodes.Add(rect);
        }
    }
}
