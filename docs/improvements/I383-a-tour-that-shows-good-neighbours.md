# I383: A tour that shows good neighbours

Run 3, batch 10.

## Why

Batches 7–9 added pairings, spot glows, sprigs and four visitors, but the tour garden never held the arrangements that show them. A feature that has never been seen on a device isn't verified. This step makes the simulator show them.

## Dev

- `EvergroveApp.QaArrangeGoodNeighbours()` (dev builds only) finds a bank on the real map where six of a finished garden's things can stand so that all seven pairings hold. The six are two flowers, three trees and something built. The search tries each land cell against a few cluster shapes and checks the result with `Pairings.Between`. It then places them with `PlaceAt`.
- `DevQaTour` builds a finished summer garden, arranges it, and opens home twice: the first four pairings are found, then the deeper three open and are found. It then shoots:
  - `21-good-neighbours-day.png` at 11:00: sprigs, the heron, the kingfisher, bees between flower and tree;
  - `21-good-neighbours-night.png` at 23:00: the owl, the hedgehog.
- The tour fails loudly if no bank fits, rather than skipping.
