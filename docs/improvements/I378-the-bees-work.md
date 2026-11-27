# I378: The bees' work

Run 3, batch 9 ("the bees' work, and deeper neighbours").

## PM decision

Make the economy a chain of real causes the player can reason about:

> flowers bring bees (I325) → bees pollinate a tree beside a flower → that tree gives **one more fruit** when tended → fruit becomes jam, gifts and parcels for family.

Conditions: the tree has a flower beside it (touching, diagonals included), the bees have come (three flowers growing), and it isn't winter (no bees then). Each tending gives at most one extra fruit, and the fruit cap still applies, so the economy stays bounded.

## Dev

- `Pairings.Pollinated(map, things, index)` is pure: the thing is a tree and a flower touches it.
- `WorldModel.PickPollinatedFruit(flowerBeside, season)` checks Living mode, bees welcomed, not winter and the cap, then adds one fruit.
- `EvergroveApp.NotePollinated(thingIndex)` runs after every tree puzzle solve, for story trees and nursery trees alike. On return home a toast says: "The bees carried pollen from the flower beside it: one more fruit."
- Tests:
  - pollinated vs. not (the flower itself, a far flower);
  - no extra fruit without bees;
  - one extra fruit with bees;
  - none in winter or without a flower beside.

## Result

A gardener's real logic now drives the fruit economy, so placement pays off every day, not only once.
