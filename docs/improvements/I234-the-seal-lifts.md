# I234 — The seal lifts

## Player
- Opening a letter showed small grey "From Nan" over a plain card, like a dialog
  box. A letter from my mother should feel like one.

## PM decision
- A new letter arrives sealed: the envelope settles on the table with a little
  wobble and the chime, then the paper rises.
  - With reduced motion, the paper simply shows.
  - Older letters open straight to the paper.
- The page:
  - the opened envelope at the top;
  - the words at 21 points;
  - signed at the foot, right-aligned in leaf green: "— Nan".

## Dev
- `ShowLetter` checks unread before marking read. `OpenEnvelope` animates
  transform only. `LetterPaper` builds the page and uses `RiseIn`.

## Result
- b276 tours complete in portrait, landscape and 150% text (57 shots each).
  - `17t`: the envelope flies into the letterbox, which stands under the purse;
    neighbours keep clear of it.
  - `17t2`: Orin's line.
  - `17u0`: the sealed envelope.
  - `17u`: the letter. "Love, Nan" isn't signed twice.
  - `17u2`: "Dear Nan," begun.
- Found in b275 and fixed in b276:
  - the letterbox covered Mira's portrait (the home's top clear band is now 220
    points when there are letters);
  - the signature repeated a sign-off;
  - the tour showed home instead of Orin's card.
- Not yet tried by players aged 40+, or on a real device.
