# I229 — A pear tossed into the purse

## Player
- I solved my pear tree's puzzle and got seeds, but nothing told me the tree
  also gave fruit.

## PM decision
- After a solve that picks fruit, back home a painted pear is tossed from the
  middle of the screen in a small arc, with a spin, into the purse at the top
  right, which swells once. With reduced motion, the purse simply swells.
- Once you've picked any fruit, the purse shows the pear and its count beside
  the seeds. It stays a single line at any text size (I227).

## Dev
- `EvergroveApp`: the solve wrapper sets `fruitPicked`; `SeedPurse` adds the
  pear and count (`purseFruit`); `FlyFruit` animates `transform` only.

## Result
- b272: the purse reads "140 seeds, 2 pears" after two tree solves (log
  `MOSSELORA_QA_FRUIT 2`). One line, also at 150% text.
