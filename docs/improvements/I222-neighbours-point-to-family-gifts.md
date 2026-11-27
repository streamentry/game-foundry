# I222 — Neighbours point to gifts from family

## Player
- A cutting from Nan (I215) waits in the basket behind a small ribbon. If I
  close the parcel card without reading closely, nothing reminds me it's there.

## PM decision
- The neighbour of the gift's kind mentions it, first, until it's planted:
  Fen for flowers ("A clover cutting came for you! It's waiting in your
  basket."), Mira for trees, Orin for buildings. The companion and the family
  layer meet: the village notices what your family sends.
- Truthfulness fix found by the new test: a planted gift becomes a nursery
  plant in the save, so the I210 line "A poplar from the nursery" could be
  false. Arrival lines now say "new in the garden", true either way.

## Dev
- Test-first (`NoticeTests`: red, Fen's old line came back), then
  `NeighbourNotice.Line` checks `world.Gifts` first; `Waiting` lines; neutral
  `Arrival` wording. Tour: Fen asked after Nan's parcel.

## Result
- Core tests: 71,302 assertions pass. Tours: 45 shots each, complete (b267); `I222-gift-notice.jpg` (local only) shows Fen pointing to the clover cutting and Mira's corrected "new in the garden" line.
