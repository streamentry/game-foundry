# I368: Good neighbours

Run 3, batch 7 ("good neighbours").

## Player note (P13)

Once a thing is planted, where it stands doesn't matter. The garden is a picture you arrange, but arranging it never pays off. The goal asked for "very logical complexity behind simple game plays". Placement is where that should live.

## PM decision

Use companion planting, a real gardener's idea. Four pairings, each a rule you can say out loud:

| Pairing | Rule |
| --- | --- |
| Sheltered | a flower beside a tree |
| By the water | something built beside the river |
| A flower bed | two *different* flowers side by side |
| A grove | two trees side by side |

"Beside" means touching, diagonals included. The first time a garden makes each pairing, it pays 15 seeds once, with a word. Because each pays only once, the economy stays bounded and no one can farm it by moving things back and forth.

## Dev

- `Core/Pairings.cs`:
  - `Pairing`, `PairingMask` and `PlacedThing`;
  - `Pairings.Between(map, things)`, the pure rule;
  - `Things(world, map)`, which collects choices, plantings and bought plants with their cells;
  - `Found` and `Reward`.
- `WorldModel` (partial):
  - `PairingsFound`, `HasPairing` and `DiscoverPairing`;
  - a new save field `pairings`, written only when non-zero and validated on read;
  - `GardenTransfer` carries it.
- `EvergroveApp.NotePairings(map)` runs when home's map loads. It pays for new pairings and shows a toast such as "Good neighbours: a flower beside a tree: sheltered".
- Tests: `PairingTests` cover each rule, diagonals, same-art flowers (not a bed), far apart (nothing), paying once, save round-trip and transfer.

## Result

Arranging the garden now has a reason, and the reasons are logical, not arbitrary. Unknown: whether players find the pairings without the hints (I370, I372).
