# I90 — A landmark chosen again is remembered anew

Date: 2026-11-24. Branch: feat/mosselora-painted-ui (PR #76).

## Problem

Choosing a landmark again repeated its memory word for word. The journal could show "Orin remembers the bridge you restored." twice, and so could the memory screen and the hub's keeper line. I88 labelled the journal pages by stream, but the sentences were still identical. For a game whose promise is "a living garden that remembers", repeated memories read as a machine, not a neighbor.

## Change (b67)

- **Core:** new `LocalizedVillage.MemoryAt(int[] choices, int index)`. It counts how many times the same landmark was chosen earlier and returns the first, second or third line for that landmark, then cycles. `Memory(world)`, used by the memory screen and the hub keeper line, now uses it. So does the journal UI. The memory screen and journal always agree.
- **Six new lines**, in English source plus all 29 catalogs (121 keys, up from 115):

  | Landmark | 1st (existing) | 2nd | 3rd |
  |---|---|---|---|
  | Tree | Mira remembers the tree you planted. | Mira rests in the shade of your trees. | Mira counts your trees along the bank. |
  | Bridge | Orin remembers the bridge you restored. | Orin crosses your bridges every morning. | Orin keeps your crossings in good repair. |
  | Flowers | Fen remembers the flowers you grew. | Fen gathers seeds from your wildflowers. | Fen follows the bees along your wildflowers. |

- **Translations:** a Haiku sub-agent drafted all 29. I corrected them against each catalog's existing lines:
  - **Register:** Russian, Ukrainian and French were changed to the formal "you" (вы/ваші/vous) and Malay to "anda", matching their catalogs.
  - **Names:** Thai Orin became "โอริน" and Urdu Orin "اورِن", matching the catalogs' own `name_*` entries.
  - **Punctuation:** the Thai full stops were removed, and French uses typographic apostrophes.
  - **Check:** a script confirmed that every new line contains its catalog's own villager name.

## Tests (written first, failed, then passed)

`RepeatedMemoryTests` checks:

- The first bridge keeps the original line, and the 2nd and 3rd each have their own.
- All three are distinct, and the 4th cycles back to the first.
- The memory screen agrees with the journal.
- A repeat is counted across other landmarks (tree, flowers, tree → the second tree line).
- An out-of-range index returns the empty memory.

The C# tests pass with 40,813 assertions, and the verifier reports 29 catalogs × 121 keys.

## Evidence

- `I90-varied-memories.png` shows the journal from the QA tour, b65 then b67. Stream 4's bridge now reads "Orin crosses your bridges every morning."
- The b67 QA tour captured all 19 screens.

## Still open

- The translations are AI drafts and need native review, like the rest of the catalogs.
- The Codex change-choice glyph was retried and is still blocked by credits.
- No players aged 40+ have tried this (m3-session-kit P1–P9).
