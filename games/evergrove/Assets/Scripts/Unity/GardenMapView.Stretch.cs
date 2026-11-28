using System;
using Evergrove.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Evergrove.UnityClient
{
    /// <summary>
    /// Checklist 42: each stretch of the river has a character. Its painting wears a gentle tint of its
    /// kind (StretchCharacter.Tint), and its name plank carries the puzzle kinds its neighbour keeps, drawn
    /// as the play marks draw them. The neighbour placement that uses the kind is in EvergroveApp.Neighbours.cs.
    /// </summary>
    internal sealed partial class GardenMapView
    {
        // The stretch the hub shows, set by Layout just before the neighbours are placed (static, as LanternsNext is).
        internal static int StretchShown;
        /// <summary>The map row at the middle of the view (checklist 42: where a stretch's neighbour stands).</summary>
        internal static int ViewRow = -1;

        // The stretch whose middle is nearest the middle of the window, from the scroll position in use.
        private int StretchInView(float reachHeight)
        {
            if (land == null || window == null || reachHeight <= 0f || reaches <= 1) return 0;
            float travel = Mathf.Max(0f, land.sizeDelta.y - window.rect.height);
            float down = keep.x >= 0f ? keep.y : scroll != null ? scroll.normalizedPosition.y : 1f;
            float centre = (1f - down) * travel + window.rect.height / 2f;
            ViewRow = map != null && map.ReachRows > 0 ? Mathf.Clamp((int)(centre / (reachHeight / map.ReachRows)), 0, map.Rows - 1) : -1;
            return Mathf.Clamp((int)(centre / reachHeight), 0, reaches - 1);
        }

        // A stretch's painting, washed with its character's tint: a soft overlay child, under the things and the season grade.
        private static void PaintStretch(RectTransform painting, int reach)
        {
            StretchTint tint = StretchCharacter.Tint(StretchCharacter.KindOf(reach));
            var node = new GameObject("Stretch tint", typeof(RectTransform));
            node.transform.SetParent(painting, false);
            var rect = (RectTransform)node.transform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            var image = node.AddComponent<Image>();
            image.color = new Color(tint.R, tint.G, tint.B, tint.A);
            image.raycastTarget = false;
        }

        // Beneath a stretch's name plank: the first two puzzle kinds its neighbour keeps, each on a small disc
        // like a play mark. Added to the map's nodes, so the next layout clears it with the rest.
        private void FlavourMarks(RectTransform sign, int reach, float signW)
        {
            PuzzleKind[] kinds = StretchCharacter.Flavour(StretchCharacter.KindOf(reach));
            int shown = Math.Min(2, kinds.Length);
            if (shown == 0 || sign.parent == null) return;
            float signH = signW * 0.28f;
            float box = signH * 0.8f, gap = box * 0.3f;
            float rowW = shown * box + (shown - 1) * gap;
            var row = new GameObject("Stretch flavour " + RiverNames.For(reach), typeof(RectTransform));
            row.transform.SetParent(sign.parent, false);
            var rowRect = (RectTransform)row.transform;
            rowRect.anchorMin = rowRect.anchorMax = new Vector2(0f, 1f);
            rowRect.pivot = new Vector2(0f, 1f);
            rowRect.sizeDelta = new Vector2(rowW, box);
            rowRect.anchoredPosition = new Vector2(sign.anchoredPosition.x - rowW / 2f, sign.anchoredPosition.y - signH - box * 0.25f);
            for (int i = 0; i < shown; i++)
            {
                var disc = new GameObject("Flavour " + kinds[i], typeof(RectTransform));
                disc.transform.SetParent(row.transform, false);
                var discRect = (RectTransform)disc.transform;
                discRect.anchorMin = discRect.anchorMax = new Vector2(0f, 1f);
                discRect.pivot = new Vector2(0.5f, 0.5f);
                discRect.sizeDelta = new Vector2(box, box);
                discRect.anchoredPosition = new Vector2(i * (box + gap) + box / 2f, -box / 2f);
                var image = disc.AddComponent<Image>();
                image.sprite = MosseloraArt.Rounded(16);
                image.color = MosseloraTheme.WaterAction;
                image.raycastTarget = false;
                KindBadge(disc.transform, kinds[i], box * 0.62f);
            }
            nodes.Add(rowRect);
        }
    }
}
