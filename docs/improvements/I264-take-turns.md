# I264 — Take turns

## Player
- With a child beside me, I want an easy way to share a puzzle: you tap, then I
  tap.

## PM decision
- A two-people token in every puzzle card's top row: close, thing, together,
  hint, seeds. Tap it to play together.
- While on, the token wears whose turn it is, sage or honey, and flips after
  every move. When the puzzle's solved, it blooms.
- It's cooperative only: no scores between players, no timer, nobody "wins".
- Colours, not names: nothing about who's playing is asked or stored.
- It lives in the top row rather than a row of its own, so no board gives up
  height for it.

## Dev
- `TreePlay`:
  - `TogetherRow`, `ToggleTogether`, `FlipTurn`, `PaintTurn`, `PlayedTogether`;
  - `onTurn` is wrapped so every kind's move flips the turn.
- `EvergroveApp`: the solve wrapper calls `world.MarkTogether(shown)` when the
  card was played together.
- Tour: `17i2-taking-turns`.

## Result (b289)
- Tours complete in portrait, landscape and 150% text (82 shots each).
  - `17i2`: the take-turns token in sage.
  - `17j2`: the forget-me-nots wear the two-colour mark.
  - `17j3`: Fen says "The two of you matched the flower beds together. My
    bees approve."
- Core tests: 344,155 assertions pass.
- Not yet tried with two real people on one device. That's the next family
  playtest.
