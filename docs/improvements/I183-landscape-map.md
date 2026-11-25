# I183 — Sideways, the left side is your garden map

Date: 2026-11-26. Branch: feat/mosselora-i127-plus.

## Player (found in I182)

Sideways, the left half of every screen was still the old fixed painting (a memory vignette or the village picture). A player who rotates the phone, which the M3 kit's P12 asks about, couldn't see their garden or place anything.

## PM decision

Sideways, **the whole left pane is your garden map**: the same map, the same things in the same places, the neighbours standing in it, tap to place and to move, the dry valley greening, and the first-time planting tip. It melts into the paper at its right edge. The old painting stays only as a fallback if the map art is missing.

## Dev

`EvergroveApp.LandscapeGardenMap(pane)`: called from `LandscapeGarden` in living mode. It builds a full-pane `GardenMapView` (1.15× width), with the place tip and a right-edge `SideFade` melt.

## Result (b197, landscape tour of 23 screens; portrait and 150% also pass)

`I183-landscape-map.jpg` (gitignored):
- **Memory card:** the map with Mira and Orin near the birch by the cottage, Fen in the meadow, and the tip "Tap the grass to plant your silver birch there."
- **Stream 2 board:** the map beside the board.
- **Winter board:** the year's things on the map (a willow, a stage, flowers) with the neighbours beside their own.
- **First board:** Mira's coach bubble over the map.

Verify passes.

**Noted:** on the first board sideways, Mira appears twice (the coach portrait and Mira on the map). A small follow-up.
