# I379: Bees between flower and tree

Run 3, batch 9.

## PM decision

Show the cause of I378. A bee at a flower that has a tree beside it flies over to the tree and back, lingering at each, so the player can see the pollination before being told about it.

## Dev

- `BeeBuzz.Toward`: the offset to a tree touching the bee's flower, or zero.
- The bee eases to and fro with `SmoothStep` over `PingPong` and keeps a smaller loop while travelling.
- Motion only, like all bees (I325). Reduced motion shows no bees.

## Result

Cause and effect are visible on the map. Not checked on device yet.
