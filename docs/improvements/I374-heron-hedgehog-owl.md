# I374: A heron, a hedgehog and an owl

Run 3, batch 8.

## PM decision

Good neighbours (I368) should change the garden for good, not only pay once. In real gardens, habitat brings wildlife, so each pairing brings a visitor with a logical reason and its own hours:

| Pairing | Visitor | When |
| --- | --- | --- |
| Something built by the water | a heron, fishing | by day, not at dawn; not in winter (the river is frozen, I342) |
| A flower sheltered by a tree | a hedgehog, snuffling at the flower's foot | dusk and night; not in winter (it hibernates) |
| A grove of two trees | an owl, up in the tree | night only, any season |

The evening and night garden now has its own life, which rewards visiting at different hours. Visitors come only while the pairing still stands: move the trees apart and the owl isn't there.

## Dev

- `Visitor` gains `Heron`, `Hedgehog` and `Owl`.
  - `Visitors.Welcomes` uses `HasPairing`.
  - `Visitors.Out` gives each its hours.
  - `Visitors.Why` explains each.
- `Pairings.SpotFor(map, things, pairing)` gives where the visitor stands:
  - for the heron, the water square beside the built thing;
  - for the hedgehog, the sheltered flower;
  - for the owl, a grove tree;
  - -1 when the pairing isn't there.
- `GardenCreature` (Unity) draws each from soft shapes, like the cat (I335):
  - the heron stands still, then dips to fish;
  - the hedgehog shuffles;
  - the owl blinks.
  They hold still when motion is reduced.
- The almanac's visitors page lists all six.
- Tests cover each spot, each visitor's hours, and that a grove found welcomes the owl only.

## Result

Where you place things now shapes what lives in the garden. Not checked on device yet: whether the creatures read at a glance at their 40 pt size.
