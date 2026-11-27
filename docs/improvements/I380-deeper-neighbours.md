# I380: Deeper neighbours

Run 3, batch 9.

## PM decision

The goal asks for logical complexity that the more people play, the more they love. Once a garden has found the first four good neighbours (I368), three more open:

| Pairing | Rule | Reward |
| --- | --- | --- |
| Flowers by the water | a flower beside the river | 25 seeds |
| A shady seat | something built beside a tree | 25 seeds |
| An orchard | a tree touching two other trees | 25 seeds |

They stay closed until then, so a new player meets four simple rules, and a long-time player finds the garden still has more to show.

## Dev

- `Pairing` gains `Waterside`, `ShadySeat` and `Orchard`.
- `Pairings.FirstFour`, `Open(world)`, `Deeper(p)` and `RewardFor(p)`, with `DeeperReward = 25`.
- `Between` detects the three new pairings.
- `Found` is masked by `Open`.
- `DiscoverPairing` refuses a pairing that isn't open yet.
- Links skip the water pairings, which aren't pairs.
- I372's spot glows count closed pairings as already found, so a closed pairing never glows.
- Save: same `pairings` mask; validation allows all seven bits.
- Tests cover each rule, the gate, the reward and the rule text.
