# I221 — Things show they hold a puzzle

## Player
- Nothing on home said that a tree, a flower bed or a building could be played.
  I only found out by tapping one by chance. For a player who doesn't
  experiment, the whole depth loop (I203–I209) is invisible.

## PM decision
- Every thing on home whose puzzle you haven't solved wears a small round play
  mark at its shoulder: the same blue disc and arrow as the play button, so it
  reads "play this" without words. Solved things lose the mark and keep their
  golden ring (I204). Basket copies (plantings) hold no puzzle and get no mark.
- Only home shows the marks, because only home plays them; the board backdrop
  and the memory card's map, where a tap places things, stay clean.

## Dev
- `GardenMapView.Placed.HoldsPuzzle`; the mark is drawn beside the item (not as
  its child, so it never squashes with the item's grow animation), only when the
  map takes taps.
- `EvergroveApp`: `markPuzzles` is set only while building home's items.

## Result
- Tours: 44 shots each, complete (b266). `I221-puzzle-marks.jpg` (local only): home with marks, after a solve (ring,
  no mark), after a nursery purchase, sideways (b266).
- Not tested with players: whether the mark is noticed and understood without a
  word is the question to ask first.
