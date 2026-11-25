# I135 — The book numbers its pages with the stones you walked

Date: 2026-11-25. Branch: feat/mosselora-i127-plus.

## Player (Haiku walk-through of b136, as a 63-year-old)

- On the journal: "'STREAM 1 / 12' reads more like a settings or record screen than part of a game."
- Under "hard to read": "Screen 6: the 'STREAM 1 / 12' captions and the journal body text are small."

I agree. Every page in the book starts with a 13pt all-caps label and a 12pt coloured square. It's the one place in the book that looks like a database row.

## PM decision

- Each page starts with **the painted stepping stone of that stream** (the same stone the player walked on the path, in its season: summer moss, an autumn leaf, winter frost) and **the stream's number** in 17pt bold. No "STREAM" and no "/ 12".
- The full "Stream 3 of 12" is still spoken to screen readers.
- The remembered line keeps its 17pt book serif from I98.

## Dev

`EvergroveApp.MemoryJournal`: the 12pt rounded colour chip and the visible `stream` label are replaced by `PathStone(i, …)` (22pt high, aspect kept, falling back to the old chip if the art is missing), a bold 17pt numeral, and a transparent, layout-ignored `Journal stream name` label holding the localised "Stream n of 12".

## Result (b139, iPhone 17 simulator, scripted tour)

- `10-journal`, b138 compared with b139: each page now opens with a mossy stone and a bold "1", "2"… instead of "● STREAM 1 / 12". The ending journal (`11c`) shows the year passing in the stones themselves: moss on 1–6, amber leaves on 7–9 and frost on 10–12, matching the path the player walked.
- The scripted tour captures all 20 screens. Verify passes. Local sheet: `docs/improvements/I135-journal-stones.jpg` (gitignored).

**Not verified:** the numerals in non-Latin locales (English-only builds ship today), and VoiceOver reading the hidden "Stream n of 12".
