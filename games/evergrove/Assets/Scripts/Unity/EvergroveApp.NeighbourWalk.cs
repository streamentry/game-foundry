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

        // Where a placed neighbour will stand: a walker's destination, not where the walk starts, so the
        // next neighbour is never placed on top of someone still on their way there.
        private static Vector2 NeighbourSpot(RectTransform placed)
        {
            var walker = placed.GetComponent<NeighbourWalker>();
            return walker != null ? walker.To : placed.anchoredPosition;
        }

        // A walk under way, per neighbour: the map lays its land out several times after a build (each
        // layout places the neighbours again), so a walk must outlive the holder it started on.
        private readonly Dictionary<int, NeighbourWalkUnderWay> neighbourWalks = new Dictionary<int, NeighbourWalkUnderWay>();

        private struct NeighbourWalkUnderWay
        {
            public Vector2 From, To;
            public float StartedAt;
        }

        // Checklist 24: whether a neighbour placed at `at` on the hub is walking there, from where and
        // since when. False when they stay put: not the hub, no earlier spot this garden, a short move,
        // or reduced motion. A walk already under way to the same spot carries on (same start and time).
        private bool NeighbourStartFor(int villager, Vector2 at, bool fromTitle, out Vector2 from, out float startedAt)
        {
            from = at;
            startedAt = 0f;
            if (world.Garden != neighbourWalkGarden)
            {
                neighbourLastAt.Clear();
                neighbourWalks.Clear();
                neighbourWalkGarden = world.Garden;
            }
            if (!fromTitle) return false; // other maps use their own scale; only the hub's spots are compared
            float now = Time.unscaledTime;
            if (gentleMotion && neighbourWalks.TryGetValue(villager, out NeighbourWalkUnderWay under) &&
                (under.To - at).sqrMagnitude < 1f &&
                (now - under.StartedAt) * NeighbourWalker.Pace < NeighbourWalk.Duration(Vector2.Distance(under.From, under.To)))
            {
                neighbourLastAt[villager] = at;
                from = under.From;
                startedAt = under.StartedAt;
                return true;
            }
            bool known = neighbourLastAt.TryGetValue(villager, out Vector2 last);
            neighbourLastAt[villager] = at;
            if (!known || !gentleMotion || !NeighbourWalk.Moves(Vector2.Distance(last, at))) return false;
            from = last;
            startedAt = now;
            neighbourWalks[villager] = new NeighbourWalkUnderWay { From = last, To = at, StartedAt = now };
            return true;
        }
    }
}
