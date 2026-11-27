# I199 — The mist downstream starts softly

## Player
- On boards with only one stretch of river open, a pale band started in a hard
  straight line across the lower quarter of the screen, as if a sheet of paper
  had been laid over the garden. It looked like a layout bug, not mist.
- The same line flashed across the garden on the memory card while the mist
  lifted off a newly opened stretch (I195).

## PM decision
- Mist has no edges. The straw "still dry" wash over the next reach fades in
  from clear, exactly like the white mist on top of it.

## Dev
- `GardenMapView`: the teaser's straw wash and the lifting overlay's straw wash
  now use the fog form of `Wash` (a flipped `TopFade`: clear at the top, solid
  at the foot) instead of a flat rectangle.
- Checked by sampling the screenshot rows below the reach boundary: build b230
  jumps from (172,188,84) to (200,197,118) in one step; b232 shades gradually
  over about 200 px.

## Result
- Close-up before / after: `I199-soft-mist-edge.jpg`; full screens and the
  memory card in `I199-soft-mist.jpg` (local only).
- Portrait, landscape and 150% tours: all complete (build b232).
- Lesson: a downscaled contact sheet hid the difference; crops and pixel
  samples settled it.
- Not tested on a real device or with players 40+.
