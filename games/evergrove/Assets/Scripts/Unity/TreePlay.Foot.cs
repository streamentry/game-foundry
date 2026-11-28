using System.Collections.Generic;
using UnityEngine;

namespace Evergrove.UnityClient
{
    /// <summary>Checklist 94 (founder, 2026-11-28): upright, the card's hint / take turns / send move to its foot.</summary>
    internal sealed partial class TreePlay
    {
        /// <summary>Height of the foot row, in points: room for a 52 pt token (a touch target) with its gap.</summary>
        private const float FootRowHeight = 60f;

        // Upright only: the top row keeps the thing and the seed pill (its picture and what it pays), and
        // every other token in it moves to a "Card foot" row at the card's end, beside undo, in the same order.
        // Sideways the top row already sits in the control panel, so nothing moves there.
        private void MoveControlsToFoot(RectTransform root)
        {
            if (Sideways(root) || cardRect == null) return;
            Transform top = cardRect.Find("Tree puzzle top");
            if (top == null) return;
            var moving = new List<Transform>();
            foreach (Transform part in top)
            {
                if (part.name == "The thing") continue;
                if (rewardPill != null && part.gameObject == rewardPill) continue;
                moving.Add(part);
            }
            // Row() appends, so the foot is the card's last part (after the board, trays and undo).
            Transform foot = Row(cardRect, "Card foot", FootRowHeight);
            if (moving.Count == 0) { Destroy(foot.gameObject); return; }
            foreach (Transform part in moving) part.SetParent(foot, false);
        }
    }
}
