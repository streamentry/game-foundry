# I209 — Buildings hold a Mosaic

## Player
- Trees hold water boards and flowers hold Pairs; buildings still held a water
  board. A building is something mended and kept, so its puzzle should be
  mending too.

## PM decision
- Buildings hold a **Mosaic**: one of the village's memory paintings (the
  bridge, the tree, the flowers, the village, the festival), cut into square
  tiles and scattered. Tap a tile, tap another, they swap; a tile in its place
  stays put and warms to straw. No timer, no move limit.
- Sizes by tier: 3×3, 4×4, 5×5. Seams are tight so the picture reads as one
  painting as it comes together.

## Dev
- Test-first: `Tests/MosaicTests.cs` (red: `MosaicGame` missing), then
  `Core/MosaicGame.cs`: seeded xorshift shuffle with at most a third of tiles
  starting home, `Select` → Picked / PutDown / Swapped / Ignored, `Moves`,
  `Solved`. Tests: every piece once, the same deal per building, varied deals,
  pick/put down/swap, home tiles locked, a greedy mend within Count−1 swaps.
- `TreePlay.OpenMosaic/BuildMosaic/TapPiece/PaintMosaic`: the painting's
  central square, sliced with `RawImage.uvRect`; the picked tile lifts in water
  blue. `OpenThingPuzzle` routes the Bridge family (buildings) to Mosaic.
- Tour: a building is picked up, its Mosaic opened, mended.

## Result
- `I209-mosaic.jpg` (local only): scattered 5×5 cottage mosaic, mended (the
  village's gathering painting), 150%, sideways.
- Portrait, landscape and 150% tours: 37 shots each, complete (b248). Core
  tests: 70,289 assertions pass.
- Not played by hand yet; not tested with players 40+.
