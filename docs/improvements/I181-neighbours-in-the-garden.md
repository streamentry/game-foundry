# I181 — Mira, Orin and Fen live in the garden map

Date: 2026-11-26. Branch: feat/mosselora-i127-plus.

## Player

After I176–I180, the home screen showed **two villages**: the new garden map on top, then the old painted village strip below it, with Mira, Orin and Fen standing on the *old* picture. Under each board the neighbours were missing from the map entirely. In an open world, the people should live in it.

## PM decision (end-game plan E6, first step)

- **Mira, Orin and Fen stand in the garden map**: each beside the newest thing of their family (Mira by your latest tree, Orin by your latest build, Fen by your latest flowers), or at their own waiting spot in the valley before that. Tap one to hear what they remember, as before.
- **The old village strip is gone from the home screen**: the map takes its place and runs down to the wordmark. Under the boards, the neighbours stand on the map instead of on the old strip.
- On the memory card the map shows only your things, so the new item in your hand is the focus.

## Dev

- `GardenMapView.Build(…, decorate)`: a callback that gets the plot and a cell→position function and returns extra nodes laid out with the map.
- `EvergroveApp.NeighborsOnMap(land, point, map, fromTitle)`: a fixed 96×92pt (text-scaled) holder per neighbour with `PortraitButton`. It sits one cell beside their newest item (left or right, staying on the map), or at waiting cells 25, 36 and 67 (land in the mask, clear of the corner buttons).
- Hub: the strip is hidden when the map shows, the window anchors to 0.33 (was 0.41), and the melt band is shorter. **Bug found and fixed:** `FitHubStack` re-anchored the window to the old 0.41 a frame later, leaving a blank band; it now takes the base as a parameter.
- Page strip: `NeighborsOnPainting` is skipped when the map is shown.

## Result (b195; portrait, landscape and 150% tours, 23 each; hands-on home screen)

`I181-neighbours-in-garden.jpg`: Mira and Orin by the cottage and Fen in the meadow; the map reaches the wordmark; Play reads "→ First Bend". `I181-pages.jpg` (b192, both gitignored): neighbours standing on the board pages' maps. Verify and the .NET tests pass.

**Iterations along the way:** b191 had name tags off-centre (nested size fitters) and the three neighbours piled on top of each other; b192 still had the hub gap; b194 had Fen off the visible edge.
