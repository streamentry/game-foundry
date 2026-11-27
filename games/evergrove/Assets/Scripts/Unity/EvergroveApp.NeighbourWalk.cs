using System.Collections.Generic;
using Evergrove.Core;
using UnityEngine;

namespace Evergrove.UnityClient
{
    public sealed partial class EvergroveApp
    {
        // Checklist 24: where each neighbour stood when the hub last placed them, in the hub's own
        // coordinates. Kept for one garden only; the walk starts from here.
        private readonly Dictionary<int, Vector2> neighbourLastAt = new Dictionary<int, Vector2>();
        private int neighbourWalkGarden = int.MinValue;

        // Checklist 24: the spot a neighbour walks from to reach `at` on the hub. False when they
        // stay put: not the hub, no earlier spot this garden, a short move, or reduced motion.
        // The hub's spot is always remembered (the walk's end), so a rebuild mid-walk does not walk again.
        // Where a placed neighbour will stand: a walker's destination, not where the walk starts, so the
        // next neighbour is never placed on top of someone still on their way there.
        private static Vector2 NeighbourSpot(RectTransform placed)
        {
            var walker = placed.GetComponent<NeighbourWalker>();
            return walker != null ? walker.To : placed.anchoredPosition;
        }

        private bool NeighbourStartFor(int villager, Vector2 at, bool fromTitle, out Vector2 from)
        {
            from = at;
            if (world.Garden != neighbourWalkGarden)
            {
                neighbourLastAt.Clear();
                neighbourWalkGarden = world.Garden;
            }
            if (!fromTitle) return false; // other maps use their own scale; only the hub's spots are compared
            bool known = neighbourLastAt.TryGetValue(villager, out Vector2 last);
            neighbourLastAt[villager] = at;
            if (!known || !gentleMotion || !NeighbourWalk.Moves(Vector2.Distance(last, at))) return false;
            from = last;
            return true;
        }
    }
}
