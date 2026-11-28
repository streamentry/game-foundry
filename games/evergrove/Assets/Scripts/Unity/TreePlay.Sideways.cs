using System.Collections.Generic;
using Evergrove.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Evergrove.UnityClient
{
    /// <summary>
    /// Checklist 91 (founder, 2026-11-28): held sideways, the puzzle card no longer shrinks to fit.
    /// It splits in two: the board on the left, and on the right a full-height panel holding the top row
    /// (the thing, hint, take turns, seeds), the trays, undo and the leaves chooser, all at full size.
    /// The board is built for the room it really has (the top row's height is freed for it).
    /// </summary>
    internal sealed partial class TreePlay
    {
        /// <summary>The control panel's width, in points: room for the top row's tokens and a tray.</summary>
        internal const float SidePanelWidth = 260f;
        /// <summary>Height the top row and its gap take in the upright card, freed for the board sideways.</summary>
        internal const float TopRowSpace = 96f;
        private const float SideGap = 16f, CardSides = 36f;

        private bool splitSideways;

        /// <summary>Held sideways: the screen clearly wider than tall.</summary>
        internal static bool Sideways(RectTransform root)
        {
            return root != null && root.rect.width > root.rect.height * 1.15f;
        }

        /// <summary>The board's width sideways: square to the room below the card's padding, never past 440.</summary>
        internal static float SidewaysBoardWidth(RectTransform root)
        {
            float height = root != null && root.rect.height > 1f ? root.rect.height : 390f;
            return Mathf.Min(height - 48f, 440f);
        }

        /// <summary>The whole card's width sideways: board, gap, panel and the card's own padding.</summary>
        internal static float SidewaysCardWidth(float boardWidth)
        {
            return boardWidth + SideGap + SidePanelWidth + CardSides;
        }

        /// <summary>Where a control added after the split belongs: the panel when there is one, else the card.</summary>
        internal static Transform ControlsOf(Transform card)
        {
            if (card == null) return null;
            Transform panel = card.Find("Card controls");
            return panel != null ? panel : card;
        }

        // Names of the card's parts that are controls, not the board.
        private static bool IsControl(string name)
        {
            string lower = name.ToLowerInvariant();
            return name == "Tree puzzle top" || lower.Contains("tray") || name == "Turn and mirror" ||
                   name == "Undo and redo" || name == "Leaves choice" || name == "Chain cause";
        }

        // Runs once, after the leaves chooser has attached (KeepCardOnScreen waits two frames for it).
        private void SplitSideways(RectTransform root)
        {
            if (splitSideways || cardRect == null || !Sideways(root)) return;
            splitSideways = true;
            var column = cardRect.GetComponent<VerticalLayoutGroup>();
            if (column != null) DestroyImmediate(column);
            var parts = new List<Transform>();
            foreach (Transform child in cardRect) parts.Add(child);
            var across = cardRect.gameObject.AddComponent<HorizontalLayoutGroup>();
            across.padding = new RectOffset(18, 18, 16, 18);
            across.spacing = SideGap;
            across.childAlignment = TextAnchor.MiddleCenter;
            across.childControlWidth = true;
            across.childControlHeight = true;
            across.childForceExpandWidth = false;
            across.childForceExpandHeight = false;
            float boardWidth = SidewaysBoardWidth(root);
            Transform boardSide = Side("Board side", boardWidth, TextAnchor.MiddleCenter, 0);
            Transform panel = Side("Card controls", SidePanelWidth, TextAnchor.UpperCenter, (int)BackPlacement.Reserve - 16);
            foreach (Transform part in parts)
            {
                var layout = part.GetComponent<LayoutElement>();
                if (layout != null && layout.ignoreLayout) continue; // the close token and spoken captions stay put
                part.SetParent(IsControl(part.name) ? panel : boardSide, false);
            }
            Balance((RectTransform)boardSide, (RectTransform)panel, root.rect.height - 16f - 34f); // 34: the card's padding
        }

        // When the panel is taller than the screen allows and the board side has room, its lowest
        // controls (undo, then trays) move under the board, so the card fits without shrinking.
        private static void Balance(RectTransform boardSide, RectTransform panel, float room)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(boardSide);
            LayoutRebuilder.ForceRebuildLayoutImmediate(panel);
            float board = LayoutUtility.GetPreferredHeight(boardSide), controls = LayoutUtility.GetPreferredHeight(panel);
            for (int i = panel.childCount - 1; i >= 0 && controls > room; i--)
            {
                var part = panel.GetChild(i) as RectTransform;
                if (part == null || part.name == "Tree puzzle top") continue;
                float tall = LayoutUtility.GetPreferredHeight(part) + 12f;
                if (board + tall > room) continue;
                part.SetParent(boardSide, false);
                part.SetAsLastSibling();
                board += tall;
                controls -= tall;
            }
        }

        private Transform Side(string name, float width, TextAnchor alignment, int top)
        {
            var side = new GameObject(name, typeof(RectTransform));
            side.transform.SetParent(cardRect, false);
            var layout = side.AddComponent<LayoutElement>();
            layout.preferredWidth = layout.minWidth = width;
            var stack = side.AddComponent<VerticalLayoutGroup>();
            stack.padding = new RectOffset(0, 0, top, 0);
            stack.spacing = 12;
            stack.childAlignment = alignment;
            stack.childControlWidth = true;
            stack.childControlHeight = true;
            stack.childForceExpandWidth = true;
            stack.childForceExpandHeight = false;
            return side.transform;
        }
    }
}
