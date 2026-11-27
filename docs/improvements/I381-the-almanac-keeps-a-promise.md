# I381: The almanac keeps a promise

Run 3, batch 9.

## PM decision

Before the deeper three open, the almanac shows the four rules and one faint line: "Three more, once these four are found." It's a promise rather than a list, so there's something to look forward to without spoiling it. Once they open, their rows appear (waterside flower, seat and tree paintings) and work like the first four.

The keepers also hint at the deeper pairings in their own lines of work once those are open:

- Mira hints at the orchard.
- Orin hints at the shady seat.
- Fen hints at flowers by the water.

## Dev

- `PairingRows` skips deeper rows until `Open` includes them, and adds the teaser row before then.
- `NeighborSays` keeper hints include the deeper pairings, gated by `Open`.
- `PairingHint` gives a line in each keeper's voice for each deeper pairing.
