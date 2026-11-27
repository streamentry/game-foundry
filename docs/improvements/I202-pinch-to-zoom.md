# I202 — Pinch to zoom the garden

## Player
- The garden is meant to feel like an open world, but it had one fixed zoom.
  I could not step back to see the whole stretch of river with the cottage and
  the path, or lean in to look at what I'd planted.

## PM decision
- Two fingers zoom, as in every photo app: from the whole width of the painting
  (1×) to close up (3×), around the point between the fingers.
- Home remembers your zoom while the app runs, upright and sideways separately.
- Lifting the fingers is never a tap: no accidental planting.

## Dev
- `GardenMapView.Pinch()` (LateUpdate): reads two touches from the legacy Input
  Manager (the project's active handler), or the Input System when that is the
  only one. While pinching, the scroll is off and the land is only scaled with
  `localScale`, keeping the land point under the fingers fixed; when the
  fingers lift, the width scale is set and the map is laid out once at the same
  spot (items are rebuilt once, not every frame). The newest thing does not
  replay its rise.
- `PinchBusy` swallows land and item taps during a pinch and for 0.35 s after.
- `EvergroveApp`: `hubZoom[2]` feeds the hub map's width scale and is updated
  through `view.Zoomed`.

## Result
- Hands-on on the simulator (two-finger paths): before / zoomed in / zoomed
  out, `I202-pinch.jpg` (local only). Zoom in and out both settle in place;
  nothing was planted or opened by the pinch.
- Portrait, landscape and 150% tours: all complete (build b239).
- Seen: zoomed all the way out, Orin's and Fen's name plates overlap.
- Not tested on a real device or with players 40+.
