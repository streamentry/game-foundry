# I179 — Things on the map are big, in view, and invite a tap

Date: 2026-11-26. Branch: feat/mosselora-i127-plus.

## Player (hands-on, b188, iPhone 17 simulator, fresh install)

I solved the first board, chose "Plant a silver birch", and looked at the new map strip on the memory card:

1. **The new birch was tiny and cut off**, in the map's very top-left corner. The default tree spot was the corner cell, so the map couldn't centre on it and the in-hand ring was half off the view.
2. **Items too small for this audience.** The birch read as a 25pt speck at arm's length.
3. **Nothing invited a tap.** I knew to tap only because I built it.

What worked:
- **Tap-to-place works.** I tapped open land beside the cottage, the birch moved there, saved, and the map re-centred on it.
- **Swiping works.** A diagonal swipe panned the map smoothly.

## PM decision

- **Bigger things:** trees 2.3 cells tall (was 1.7), buildings 1.85 (was 1.35), flowers 1.45 (was 1.05).
- **Never in the corner:** trees prefer one step in from the edge.
- **Show the verb:** the item in hand gets a small pressing finger beside it, the same `ui-tap` glyph and press rhythm as the first board's demo (I147), so "tap the land" is shown, not told.

## Dev

- `GardenMap.Score` for trees: `−|edge − 1| × 6` (was `−edge × 6`). Tests unchanged and passing.
- `GardenMapView`: new heights per family, and a `Tap land to place` hint (`IconTap`, `GhostPress`) beside the held item.

## Result (b189, iPhone 17 simulator, fresh install, same play by hand)

`I179-before.png` (b188: a tiny birch cut off in the corner) compared with `I179-after.png` (b189), both gitignored. The silver birch now stands large, one step in from the edge beside the cottage, inside its breathing ring, with the pressing finger beside it, and the map opens centred on it. Portrait, landscape and 150% tours pass (23 each). Verify and the .NET tests (52,728) pass.

**Seen but not changed:** the finger can overlap a neighbouring thing (here the cottage roof). The choice tap during the solve's glide sometimes misses: the card moves under the finger. That's for a later iteration.
