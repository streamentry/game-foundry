# I217 — Letters stay in the garden

## Player
- After opening Nan's parcel (I215) her words were shown once, then gone.
  A letter from a grandchild or a sister is the thing you'd want to read again.

## PM decision
- Letters live in the garden: a small round letterbox at home, under the seed
  purse, there once any letter has arrived. (First tried on the map by the
  cottage: home frames your newest things downstream, so for anyone past the
  first stretch of river the letterbox was off-screen. Found by logging the
  tour: the box existed, at the top of the river, out of view.) A red dot (and a gentle pulse,
  with gentle motion on) while one is unread.
- Tap it: the newest letter on cream paper, "From Nan", her words in the book
  font; round tokens for an older or newer letter, and a check to close. A tap
  outside closes it too; a tap on the paper doesn't.
- The 20 newest letters are kept (I214's rule).

## Dev
- `EvergroveApp.LetterBox` (a round token under the purse), `ShowLetter/CloseLetter`; read state is a count in PlayerPrefs.
  The paper absorbs taps with an inert Button so the backdrop's TapCatcher
  only fires outside it.
- Tour: after the parcel from Nan, the letterbox on home, then her letter.

## Result
- `I217-letters.jpg` (local only): the letterbox with its unread dot, the
  letter, 150%, sideways; plus the visit sign at 150% (I216 fix).
- Also fixed: the visit sign at 150% overlapped the home token; it now starts
  right of it and grows with text size.
- Portrait, landscape and 150% tours: 44 shots each, complete (b260).
- Seen, not fixed: on this save the round play token covers Fen at the bottom of home.
- Not tested with families; English only.
