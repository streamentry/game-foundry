# I178 — Swipe the garden map and put things where you want

Date: 2026-11-26. Branch: feat/mosselora-i127-plus.

## Player (founder, with a screenshot of the memory card's village strip)

"We have to change this into vertical scroll and swipable like in home screen, where user can plant and put many things."

Under every board and above every memory card sat the old fixed village strip: one painting with nine pre-drawn spots. It couldn't hold 200 different things, and the player couldn't put anything anywhere.

## PM decision (E4: place what you earn)

- **The strip becomes the garden map**, the same one as the home screen: a 260pt window (230pt at larger text) you can **swipe in every direction**. When the map reaches its top or bottom edge, a vertical swipe scrolls the page instead, so the map and the page never fight.
- **Right after a choice, the new thing is in your hand**: it stands on its default spot with a soft breathing ring and a gentle bob, and the map opens on it. **Tap any free land to put it there.** Water, the cottage, the path and taken cells don't accept it; it simply stays in hand.
- **Tap anything already standing to pick it up**, then tap land to move it. Tap it again to put nothing in hand.
- Every placement saves at once, in the save and the garden file (I177), with a wooden tock (if sound is on) and a light haptic tick (with gentle motion).
- The same taps work on the home screen's map.
- The garden number now comes from the save, so a restored garden file keeps its own items. A new garden gets the next number.

## Dev

- `GardenMapView`: taps on land (`MapTap`, ignores drags) report the cell, and taps on items report the choice index. The item in hand gets a `Breathe` ring and a `Bob`; the newest gets a `GrowUp` rise after a choice. New `NestedScrollRect` passes vertical drags to the page at the map's edges, or when the map isn't taller than its window.
- `EvergroveApp`: `PageGardenMap(window, memoryScreen)` replaces `PaintedVillage` in the page strip when the map art is present (old painting as fallback). `MapItems` lays items out with `world.Placements`. `ItemInHand`, `OnMapItem`, `OnMapLand` (validates with `GardenMap.CanPlace`, then `world.PlaceAt` and `SaveWorld`) and `RefreshMaps` (rebuilds the hub or the page). `ConfirmedReset` starts `WorldModel.New(Living, nextGarden)`. `ItemAt` reads `world.Garden`.

## Result (b188, iPhone 17 simulator; portrait, landscape and 150% tours, 23 screens each)

`I178-map-strip.jpg` (gitignored): the memory card shows the map strip with the new silver birch in its ring at the top-left; stream 2's board shows the map below the tools with Mira, Orin and Fen standing on it; 150% keeps the same layout, and the pinned continue arrow stays clear. Verify passes; .NET tests pass (52,728).

**Not verified yet:** tapping to place and drag-scrolling by hand (the tour doesn't tap the map; the next step is a hands-on simulator check). Items look small at this zoom (one cell is about 50pt). Bigger items or a closer default zoom is the next tuning.
