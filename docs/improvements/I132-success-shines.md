# I132 — A finished stream is the brightest water on the board

Date: 2026-11-25. Branch: feat/mosselora-i127-plus.

## Player

I solved the first board by hand (b134). The instant the last elbow clicked into place, the bright teal water across the board **turned dark bottle-green** and the tiles went grey-green. The page then moved to "What will you help grow?". The finished stream looked flatter and duller than it had a second earlier, while it was half-done. My success had dimmed the water.

Under the choices, the "revisit the water" control was the old square parchment plaque, the last square tool left after I129's round tokens.

## PM decision

- **A finished stream is the brightest state, never a duller one.** Solved pipes become a clearer, lighter teal with a sunlit white stripe, and solved tiles a sunlit pale water colour (lighter than wet tiles). Solved stays distinct from in-progress through the choice screen, the full drop meter and the flowing creek behind the board, not through a darker pipe.
- The revisit control becomes a round parchment token (64pt), the same family as the board's Undo.

## Dev

- `PipeGraphic.OnPopulateMesh`, completed colours: outer `30,110,90` → `22,132,146`, collar `20,81,63` → `16,96,108`, stripe → `232,251,250`.
- `MosseloraTheme.SolvedTile` `#E0EDD9` → `#E4F5EF`.
- `EvergroveApp`: "Revisit water" uses `IconButton(..., 64, QuietAction, …, round: true)` and drops its fixed 84pt width.

## Result (b135, iPhone 17 simulator, scripted tour)

- `03-chapter1-solved-choice`, b134 compared with b135: the finished board's pipes are now a clear teal with a pale stripe on sunlit water tiles, lighter than the half-done board (`02`), not darker. The revisit control is a round parchment token beside the choices. Local sheet: `docs/improvements/I132-success-shines.jpg` (gitignored).
- `verify_evergrove_project.py` passes. The scripted tour captures all 20 screens.

**Not verified:** whether a colour-blind player can still tell solved from almost-solved without the choice screen. The stripe pattern, not colour, carries "wet versus dry", but solved versus wet now differs only in lightness.
