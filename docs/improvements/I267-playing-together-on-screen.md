# I267 — Playing together, on screen

## Player
- Does the whole chain hold on a real screen: take turns, solve, see the mark,
  hear the word?

## PM decision
- The tour plays the Pairs table taking turns and shoots each step: the token
  in sage, the solved table, the two-colour mark on the flowers, and Fen's line.
- Not covered by a simulator: two real people sharing one phone. That is the
  first family playtest to run.

## Dev
- `DevQaTour`:
  - `QaTogetherTreePlay`;
  - shots `17i2`, `17j2`, `17j3`.

## Result (b289)
- Tours complete in portrait, landscape and 150% text (82 shots each).
  - `17i2`: the take-turns token in sage.
  - `17j2`: the forget-me-nots wear the two-colour mark.
  - `17j3`: Fen says "The two of you matched the flower beds together. My
    bees approve."
- Core tests: 344,155 assertions pass.
- Not yet tried with two real people on one device. That's the next family
  playtest.
