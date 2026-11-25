# I169 — The choice screen offers only the choice

Date: 2026-11-25. Branch: feat/mosselora-i127-plus.

## Player (Haiku critique of b179, as a 69-year-old retired nurse; her "most confusing moment")

"Screen 2, 'What will you help grow?'… **The bottom-left button looks like undo, not close, so I was unsure whether tapping it would cancel the choice.**"

Under the three choice cards sat a round undo token, "revisit the water" (I79, made round in I132), which un-did the last pipe turn after the stream was already finished. At the game's most important decision it was the only other control, and it read as "cancel".

## PM decision

**Delete it.** Once the stream runs, the screen asks one thing: what will you help grow? The brief says not to show every button every time. Nothing is lost:
- Second thoughts about the *choice* already live on the memory card ("change my choice", I133).
- Undoing a *finished* stream had no purpose a player asked for. The board's own Undo still works on every move up to the solve.

The rest of the lead designer's list is noted, not done: ghost-preview a choice on the board (large; candidate), hide the stone path except at chapter change (conflicts with the P10 bet in the playtest kit), and remove the neighbour strip (it *is* the living world, so rejected).

## Dev

`EvergroveApp` choice screen: removed the "Revisit row" and its `Revisit water` button (no other references in code, tests or tools).

## Result (b181, iPhone 17 simulator, tours)

`I169-choices-only.jpg` (gitignored): `03` before (b180, with the undo token) and after (b181: only the three cards, with the village strip and Mira, Orin and Fen now visible on the same screen). Landscape shows the cards alone. Portrait, landscape and 150% tours pass (21/6/21). Verify passes.
