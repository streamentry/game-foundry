# I296 — Lamplight at the cottage

Idea 28 of the [fifty](../../games/evergrove/docs/specs/proposals/world-games-50-ideas-2026-11-27.md), after hygge, the Danish warmth of a lit window on a dark evening.

## Player

"At night the garden goes dark, and so does the house."

## PM decision

From dusk to dawn, warm lamplight spills from each cottage door: two soft amber glows, drawn over the evening shade so they stay bright. The cottage is painted from above (a thatched roof), so the glow sits at the door, not in windows. Every reach's cottage is lit, so the neighbours' houses down the river glow too. It's cosmetic and follows the light switch.

## Dev

- `DayLight.LampLit(DayPhase)` is true at dusk, night and dawn. The test was run red first: it failed to compile without the method.
- `MosseloraArt.SoftGlow()` is a new procedural radial sprite. `GardenMapView` places the glow at the painted door (85.8% across, 14.8% down) in the first reach.
- The tour's light shots (20-light-20, 20-light-23) show it.

## Result

Evenings have a warm centre. The door position is measured from the painting, not from a mask cell; if the art changes, the glow must move with it.
