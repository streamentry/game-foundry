# Nonogram Canvas — independent visual and interaction system v0.1

**Positioning:** a calm, original picture cross / Nonogram research puzzle for competent adults, without age stereotypes or health/cognitive treatment claims.

**Three adjectives:** minimal · pictorial · leafy.

| Token | Hex | Intended use |
| --- | --- | --- |
| Paper | `#F5F6F3` | Frame background |
| Ink | `#30483F` | Rules, board symbols and text |
| Accent | `#557C69` | Active choices and primary action |
| Soft | `#E3EBE4` | Secondary cells and clues |
| White | `#FFFFFF` | Editable tiles and contrast text |

**Type:** Godot fallback face, 27–28 px title; 15–17 px instructions/status; 18–25 px puzzle symbols; 12–14 px secondary explanation. **Spacing:** 14–20 px margins, 8–13 px vertical rhythm, 3–7 px cell gutters. Controls have ≥48 logical px targets, except fixed visual annotations which are never required to tap; short-screen content scrolls.

**Signature interaction:** Tap a cell filled → crossed empty → unknown. Ten row/column clue lines stay visible. Incorrect inputs remain reversible; the game checks constraints rather than comparing visible colors to a secret answer, except explicitly authored phrase vocabulary. Avoid penalties, dark patterns and mandatory timers.

**Contrast and accessible input:** numbers/letters/symbol labels convey information without hue alone. Native button focus, explicit row/column labels where relevant, visible Undo/Restart and progress; screen-reader navigation and 200% text still require real iOS/Android audits. Do not claim accessibility certification from headless CI.

**Motion:** immediate reversible feedback, no flashing/confetti; **sound/haptics:** none by default.

**Puzzle feedback:** Fill a 5×5 image using exact row/column runs separated by at least one empty cell. Show completion only after actual rules pass, and explain at least one prominent violation or remaining action.

**Original app icon brief:** five-by-five tiny pixel-tree silhouetted in green blocks; no reused third-party art, store screenshots or competitor logo.

**Screenshots to capture:** Nonogram Canvas — initial board, legal intermediate input, visible recovery/error feedback, verified completed state. No actual phone captures produced yet.

**Known business uncertainty:** A 5×5 tutorial can demonstrate correctness but may provide little replay value, and clue headers may crowd narrow screens. Real demand, age fit, spontaneous replay, competitor advantage, acquisition and monetization are all unmeasured. No automatic App Store release.
