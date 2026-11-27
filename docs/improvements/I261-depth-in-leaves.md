# I261 — Depth in leaves

## Player
- If a board is deeper, I'd like to know before I start, without a number or a
  word like "hard".

## PM decision
- Three small leaves sit under the thing on the puzzle card. Bright leaves show
  how deep this board is: one, two or three.
- The screen reader says "Depth 2 of 3."

## Dev
- `TreePlay.ShowTier(tier)` is called from `EvergroveApp.OpenThingPuzzle`
  after the card opens. The card keeps its font (`uiFont`) for the spoken
  caption.

## Result (b288)
- Tours complete in portrait, landscape and 150% text (79 shots each).
  - The family card is two columns sideways (`17p`, landscape).
  - `17h2`: a replay deals a fresh board.
  - The depth leaves are on their own pill.
  - The classic three show their picture rules (`17f`, `17i`, `17m`).
- Core tests: 343,997 assertions pass.
- Not yet tried by players aged 40+. How quickly favourites should deepen is
  unmeasured.
