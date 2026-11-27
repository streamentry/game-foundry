# I233 — A real letterbox

## Player
- My letters live behind a chat bubble with a red dot. That's my phone's
  notification badge, not a garden. Nothing in it says "a letter came".

## PM decision
- A painted wooden letterbox on its post, with a red flag and ivy, stands under
  the purse once you've had any letter.
- An unread letter is a sealed envelope (red wax seal, leaf emblem) resting on
  top of the box, breathing gently. No red dot, no count.
- New art: `icons-letters.png`, generated with the Codex image tool in the
  food sheet's style. Prompt in `art/prompts/letters.txt`; provenance in
  `art/README.md`; sliced by the pipeline (`LETTER_ICONS`).

## Dev
- `EvergroveApp.LetterBox` uses the `letterbox` sprite and `letter-sealed` for
  unread. The red dot and the chat glyph are gone.

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
