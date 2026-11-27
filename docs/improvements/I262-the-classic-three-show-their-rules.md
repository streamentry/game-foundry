# I262 — The classic three show their rules

## Player
- The new kinds show their rule in pictures the first few times (I254). The
  water, Pairs and Mosaic puzzles under my own things just start, so the rule
  is different by kind.

## PM decision
- The same picture strip for the classic three, the first three times each:
  - Water: waves, arrow, three water squares, arrow, a sprout.
  - Pairs: a card back, arrow, two cards with the same picture, arrow, a check.
  - Mosaic: two tiles in the wrong order, arrow, the same two swapped, arrow,
    the painting whole.
- Every one of the eight kinds now introduces itself the same way.
- Also from b287, the I261 depth leaves were illegible. They now sit on their
  own paper pill under the thing, with empty leaves dimmed rather than nearly
  invisible.

## Dev
- `TreePlay.ClassicRule(card)` uses `BoardKit.FirstTimeRule`. The Pairs and
  Mosaic tables fit beneath it.
- `ShowTier` uses a pill and 18-point leaves.
- `DevQaTour` resets all eight rule counts at its start, so every build checks
  the strips.

## Result (b288)
- Tours complete in portrait, landscape and 150% text (79 shots each).
  - The family card is two columns sideways (`17p`, landscape).
  - `17h2`: a replay deals a fresh board.
  - The depth leaves are on their own pill.
  - The classic three show their picture rules (`17f`, `17i`, `17m`).
- Core tests: 343,997 assertions pass.
- Not yet tried by players aged 40+. How quickly favourites should deepen is
  unmeasured.
