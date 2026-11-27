# v1 batch 25: item 24 (ticked, verified with a half-speed tour clip)

Date: 2026-11-28. Branch `feat/mosselora-v1-batch-25`, stacked on #156. A Haiku agent built the walk. The coordinator found why it never showed in the first tour, fixed it, and toured three more times.

## 24 Footsteps for neighbours

When Mira, Orin or Fen stand somewhere new on the hub, they walk there instead of appearing:
- **Where they move:** from a morning spot to the shady seat in the golden hour, or to the cottage door at dusk.
- **The walk:** a gentle ease in and out, a small bob (up to 5 pt) for each step and an alternating ±3° tilt. It takes 0.6–1.6 s at about 140 pt/s, and moves under 6 pt don't walk.
- **Pure rules:** `Core/NeighbourWalk.cs`.
- **The walker:** `NeighbourWalker` moves only position and rotation, on unscaled time, and ends exactly on the spot.
- **When there is no walk:** with reduced motion they appear as before; the first view after launch just places them; a walk replaces that view's hello hop.

**Fixed in review:**
1. **The walk never showed.** The map runs neighbour placement on every layout, and one rebuild lays out several times. The first placement started the walk; the next, a frame later, found nothing had moved and replaced the walker with a still holder. The app now keeps each walk under way (start, end, start time), and a re-placement mid-walk carries on from where the walk has reached (`NeighbourWalker.PoseAt`).
2. **Clash checks read where walks start.** Placement keeps neighbours apart by comparing them with those already placed, but it read a walker's starting point, so a neighbour could be set down where another was about to arrive. Clash checks now use each walker's destination (`NeighbourSpot`).

**The test, the half-speed tour clip:**
- The tour shows the hub at 09:00, then sets 19:00 and rebuilds the hub.
- It saves 16 frames (`23-walk-00` to `23-walk-15.png`) and logs every neighbour holder's position each frame (`MOSSELORA_WALK`).
- PNG frames take a few tenths of a second to save, so the tour slows walks to a quarter pace while it captures, then restores full pace. `NeighbourWalker.Pace` is 1 in play, with a test.
- The log shows Orin going from (337, −227) to (456, −283) over 14 frames, easing in and out, then standing still. Mira and Fen walk the same way.
- The frames were assembled into a GIF at 120 ms a frame, about half the walk's real speed: `/Volumes/SSD/mosselora-builds/v1/walk-half-speed.gif`. Each neighbour steps continuously to the dusk spot and none jumps. They pass each other briefly mid-walk and end apart.
- In the first frames the map itself settles its scroll after the rebuild. That is existing hub behaviour, unrelated to the walk.

**Tests:** `NeighbourWalkTests` checks:
- duration clamping;
- at least two steps;
- progress eased from 0 to 1;
- bob and tilt resting at both ends;
- no walk below 6 pt;
- the walker moves only position and rotation on unscaled time;
- walks only with gentle motion, forgotten with the garden;
- a walk survives relayout;
- clash checks use destinations;
- full pace in play;
- the tour frames and log.

## Checks

- Core tests: 5,213,014 assertions pass.
- Unity compile check is clean with and without `DEVELOPMENT_BUILD`; project check passes.
- Tours: portrait and landscape tours (166 shots each) with the walk fix, then the portrait clip tour.

## Open

- Rotating the phone or changing the text size moves every spot, so neighbours walk to the new layout too. That is harmless, but players haven't been asked.
- No footprint marks are left on the ground; the walk is the step animation only.
