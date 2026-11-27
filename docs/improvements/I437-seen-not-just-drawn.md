# I437: Seen, not just drawn, and the run in review

Run 3, batch 20. The hundredth iteration of this run (I338–I437).

## What the counts found (I433, I436)

The tour now logs each effect's count, screen position, inherited alpha and parent at every season, light and moment shot. On b327 and b328:

| Effect | Count | Where | Why it wasn't seen |
| --- | --- | --- | --- |
| Morning mist (I409) | 16 | on the water, in the map | real but too faint: about 0.3 alpha on pale water |
| Dew (I366) | 24 | in the map | many off-screen; small by design |
| Footprints in the snow (I412) | 138 | first print at y = 9,321 on a 2,622-high screen | the trail began at a door point far up a tall map, and 24 prints spread over thousands of points left almost none on screen |
| News toast (I387/I388) | 1 | on screen at (603, 2046), alpha 1 | child 3 of 5 on the canvas: two later siblings drew over it |
| Golden glitter, moonlight, autumn leaves | 14, 9, 10 | in the map | fine |

## Fixes

- **Toasts:** a toast keeps itself the last sibling on its canvas every frame, so nothing built after it can cover it.
- **Mist:** larger (3.2 × 1.3 cells) and stronger (0.62 alpha), cool white. It's now spread along the whole river: on b329 all 16 banks had sat on the first water cells at the far top of the map.
- **Footprints:** each neighbour gets ten prints at a steady 16-point stride, ending at their feet and leading in from below on the cottage's side, fading with distance. On b329 the trail ran upward from their feet, straight under their own portraits.

## Verified on b329

Portrait tours b329 and b330 on the iOS simulator:

- **News toasts:** on screen at every moment shot, as the last sibling of the canvas. 21a reads "Good neighbours: two trees side by side: a grove. +15 seeds." with its two paintings; 21c reads "Summer: the grove let fall its seeds. +20 seeds." with its three.
- **Footprints:** 60 in winter. A visible trail leads from below up to Fen's feet (`19-winter-home.png`).
- **Mist:** soft banks lie along the river in the visible stretch at dawn (`20-light-06.png`).

Not re-checked in landscape or at 150% text after these last fixes. The earlier landscape and 150% tours ran before them.

## The run in review (I338–I437)

**What changed for the player.** The garden went from a pretty map of choices to a place with logic:

- seven good neighbours with written rules (I368, I380);
- seven visitors who come for reasons and keep the weather (I374, I382, I405);
- a garden's year that gives something each season (I388);
- bees that turn flowers into fruit (I378);
- a river that rains, glitters, ripples and carries leaves (I413–I417, I425);
- days with weather (I353) and seasons that look like seasons (I338);
- a companion that notices gently from device-only facts (I419–I422);
- family visits that suggest a fitting gift (I398, I408, I418).

Buttons became pictures, help is said once (I393), and the garden's rules have their own tab (I403).

**How it was checked.**

- Pure Core rules with deterministic tests (from about 1.19M to about 1.28M assertions, including the economy session's).
- Release-define and development compiles against the Unity 6.3 player assemblies.
- iOS simulator tours in portrait, landscape and 150% text for each batch, from b311 to b329.
- From I433, logged object counts for anything too faint to trust by eye.

**What was not checked.**

- No physical device.
- **No adult 40+ has played any of this** (`age fit not validated`).
- No family pair.
- No translation of the new English lines.
- No measure of retention or wellbeing. None is claimed.

**What would change course.** See [garden-logic-2026-11-27.md § 8](../../games/evergrove/docs/specs/proposals/garden-logic-2026-11-27.md): if players don't find any pairing within a week, the glow comes unasked; if the ways page reads as homework, cut it to the at-a-glance row; if the hour notice feels like being watched, remove it.

**Next real action.** Run the existing session kits with adults 40+ in three bands (40–49, 50–64, 65+), solo and with a family member, on a physical phone. Watch the first week for: a pairing found unaided, a visitor noticed, a season's gift understood, and any note that feels like pressure.
