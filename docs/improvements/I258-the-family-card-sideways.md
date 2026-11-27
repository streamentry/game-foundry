# I258 — The family card, sideways

## Player
- Holding the phone sideways, the family card runs from the top edge to the
  bottom. Its tabs touch the screen edge and the send arrow is cut off. The
  fit-to-screen scaling stops at 75%, and the send view needs about 67%.

## PM decision
- Don't shrink the words below a comfortable size; use the width. Sideways and
  short (wider than 1.2 times its height, under 520 points tall), sending lays
  out in two columns:
  - name and gifts on the left;
  - the letter, taller, and the send arrow on the right.
- Upright is unchanged.

## Dev
- `FamilySheet.Build`: the `wide` flag, a card up to 760 points wide, `Column()`
  helpers and `rightColumn`. `BuildSend` moves to the right column before the
  letter.

## Result (b288)
- Tours complete in portrait, landscape and 150% text (79 shots each).
  - The family card is two columns sideways (`17p`, landscape).
  - `17h2`: a replay deals a fresh board.
  - The depth leaves are on their own pill.
  - The classic three show their picture rules (`17f`, `17i`, `17m`).
- Core tests: 343,997 assertions pass.
- Not yet tried by players aged 40+. How quickly favourites should deepen is
  unmeasured.
