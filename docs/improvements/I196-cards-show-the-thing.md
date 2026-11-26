# I196 — Choice cards show the thing, not a sentence

## Player
- After a stream fills, three cards ask "What will you help grow?". Each read
  "Plant a silver birch", "Raise the weathervane", "Grow tulips": the verb is the
  same idea three times and pushes the art up.
- A small brown framed tile sat in each card's top-left corner. I could not tell
  what it was: it looked like a closed door or a box. (It was the I134 miniature of
  the landmark this choice leaves on the next board.) The next board shows that
  landmark full size anyway, so the miniature only added a question.
- The neighbour portrait in the corner is good: it says *someone* will remember it.

## PM decision
- Delete the miniature next-stream tile from the cards. Nothing lost: the next
  board still roots the landmark, and the memory card names the consequence.
- The card label becomes the thing's name ("Silver birch", "Weathervane",
  "Tulips"). The full sentence and its consequence are still spoken by screen
  readers through the hidden caption.
- Bigger art (84 → 104 pt tall in the three-across layout).
- Words never break in the middle. Found while checking: "Weathervane" wrapped as
  "Weathervan / e" in portrait and landscape.

## Dev
- `EvergroveApp.ChoiceCard`: removed `NextStreamTile` (method deleted), picture
  104 pt when not wide, visible label from new `ChoiceName` (English: the item's
  `Name` with a capital first letter; other languages keep their localized
  choice text).
- New `WholeWordFit` component: steps the font down (not below 12) until the
  longest word fits its box. Skipped for CJK/RTL locales, which have their own
  fitter.

## Result
- Before / after / 150% / landscape: `I196-choice.jpg` (local only).
- Portrait, landscape and 150% tours: 27 shots each, all complete (build b226).
- Not tested on a real device or with players 40+.
- Still open: the small orange smudge under the birch art.
