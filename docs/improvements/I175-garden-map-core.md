# I175 — The open garden map: grid, land and where things stand

Date: 2026-11-25. Branch: feat/mosselora-i127-plus.

## Player (founder, 2026-11-25)

"The map/garden should be dynamic / open world, something like Minecraft or Roblox, the simpler 40+ version. The fixed picture makes it hard to put trees and flowers."

He's right. The village strip is one fixed painting with nine pre-drawn landmark slots. With 200 possible items and a garden that grows for a year, there's nowhere for things to go, and the player decides nothing about where they live.

## PM decision (end-game plan E3–E5)

- The garden becomes **a top-down painted map you shape**: an open meadow valley with the creek winding through, the cottage and a footpath, and all the rest free land.
- It's divided into a soft grid (10 × 15 cells) that the player never sees as lines. Each cell is land, water or built.
- Every item you've grown or built **stands on a cell**. Until E4 (tap to place) lands, each goes to a sensible default for its family: trees toward the edges, a little way from the water; flowers in the open meadow near it; things to build on the creek bank. Each settles in the band of the valley that matches the time of year, so the garden fills from top to bottom as the year goes on.
- Simple for adults over 40: no inventory, no crafting, no resources. Pan, look, and (from E4) tap to place.

This iteration is the **core and the art**. The map view is the next iteration.

## Art (Codex image generation)

`art/prompts/garden-map.txt` → `art/generated/garden-map.png` (1024×1536), with the village painting as the palette reference: a straight top-down meadow, a blue creek in two S-curves with mossy stones, a thatched cottage top-right, a footpath down the right side, and everything else open land (`I175-garden-map-art.jpg`, gitignored). Reviewed: matches the brief; no trees or buildings to fight the player's own.

## Dev

New `Core/GardenMap.cs`: a mask of free, water and built cells (`.` `~` `#`), `CanPlace(placements, cell, moving)`, `DistanceToWater`, `DefaultCell(placements, family, chapter)` (family scoring plus a seasonal band, ties to the lowest index) and `Layout(choices, placed)` (keeps valid player placements, defaults the rest, and gives −1 when the map is full). `GardenMapTests`: mask parsing and sizes, land, water and built cells, distance, no stacking, determinism, builds nearer the water than trees, a kept placement, water and taken-cell fallbacks, moving onto your own cell, and a full map.

## Result

.NET tests pass (52,709 assertions). Verify passes. **Not in the game yet:** the map view and the mask derived from the painting are next.
