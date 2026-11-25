# I129 — Undo and Hint become round tokens flanking the water

Date: 2026-11-25. Branch: feat/mosselora-i127-plus.

## Player

Every board ends with two wide green and teal slabs, each half the screen wide, under a separate row of water drops. On a phone they read as web form buttons ("Back" / "Help") rather than tools in my hand, and the drops and the slabs take two lines of height the board could use.

## PM decision

- **Do:** turn Undo and Hint into round painted tokens (68pt, larger than the 44pt minimum touch target) that flank the water drops on one line: Undo | drops | Hint. That is one line instead of two, and the drops read as the score between the two hands.
- **Keep:** the same icons, colours, press-and-hold names, spoken labels and hint level dots (moved inside the token, under the glyph).
- **Fallback:** when a big board's drops are too wide to fit between the tokens, the drops keep their own line and the tokens stand at either end. Landscape keeps its vertical rail.
- **Art:** new Codex art. Three round enamel medallions in carved wood rings (forest, teal, parchment), generated from the existing plaque art as the style reference so they match.

## Dev

- `art/prompts/medallions.txt` and `art/generated/medallions.png` (Codex image generation). The `tools/evergrove_art_pipeline.py` medallions step cuts `medallion-forest|teal|parchment` (192px).
- `MosseloraArt.Medallion(image, tone)`: the painted round token behind a button, like `Plaque`.
- `IconButton(..., round)`: a round body as wide as it is tall, the medallion art, a 0.44 glyph, and level dots overlaid inside the disc.
- `FlankDrops(tools, height)`: moves the water-meter row between the two tokens when its preferred width fits (page width − 2 tokens − spacing), otherwise inserts a flexible gap.
- Removed four unused `ui-choose-*` candidate icons that the pipeline re-emitted.
- After the cold read (below): tokens are 76pt upright and 72pt in the landscape rail, which now uses tokens too, so both orientations match. A 4×4 board's 16 drops shrink (to at most 70%) to fit between the tokens instead of dropping to their own line.

## Cold read (Haiku sub-agent, blind P = slabs b129 vs Q = tokens b131, as a 58-year-old evening puzzle player)

- Preferred **Q (tokens)**: "feels more like a game… the round buttons sit in the same row as the water drops, like a game HUD", compared with P's "separate block of web-style buttons".
- Risk: Q's buttons looked "much smaller and could be missed", so I enlarged them from 68 to 76pt. Undo and hint were recognised in both versions.
- **New finding:** in both versions, "nothing says what [the drops] count: moves left, water for the garden, or lives". That is the next iteration's problem.

## Result (b132, iPhone 17 simulator, portrait and landscape tours)

- On 3×3 boards, Undo | 9 drops | Hint sit on one line under the board, and the board and village moved up by about one row of height.
- On 4×4 boards (`16-winter-village`), the 16 drops fit between the tokens. They are smaller, which needs watching for readability.
- Landscape: the rail shows two stacked tokens beside the board, the same as upright.
- The scripted tours capture all 20 portrait and 5 landscape screens. Local sheet: `docs/improvements/I129-round-tools.jpg` (gitignored).

**Not verified:** touch accuracy on a real phone, and whether the dimmed (disabled) undo token reads as "not yet" to players.
