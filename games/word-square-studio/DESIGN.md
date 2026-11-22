# Word Square Studio — independent visual and interaction system v0.1

**Positioning:** a calm, original symmetric word squares research puzzle for competent adults, without age stereotypes or health/cognitive treatment claims.

**Three adjectives:** editorial · playful · precise.

| Token | Hex | Intended use |
| --- | --- | --- |
| Paper | `#F9F3EA` | Frame background |
| Ink | `#4D3C41` | Rules, board symbols and text |
| Accent | `#A06B6A` | Active choices and primary action |
| Soft | `#F1DFE2` | Secondary cells and clues |
| White | `#FFFFFF` | Editable tiles and contrast text |

**Type:** Godot fallback face, 27–28 px title; 15–17 px instructions/status; 18–25 px puzzle symbols; 12–14 px secondary explanation. **Spacing:** 14–20 px margins, 8–13 px vertical rhythm, 3–7 px cell gutters. Controls have ≥48 logical px targets, except fixed visual annotations which are never required to tap; short-screen content scrolls.

**Signature interaction:** Tap the unplaced word cards in top-to-bottom order; the board shows row positions immediately. Incorrect inputs remain reversible; the game checks constraints rather than comparing visible colors to a secret answer, except explicitly authored phrase vocabulary. Avoid penalties, dark patterns and mandatory timers.

**Contrast and accessible input:** numbers/letters/symbol labels convey information without hue alone. Native button focus, explicit row/column labels where relevant, visible Undo/Restart and progress; screen-reader navigation and 200% text still require real iOS/Android audits. Do not claim accessibility certification from headless CI.

**Motion:** immediate reversible feedback, no flashing/confetti; **sound/haptics:** none by default.

**Puzzle feedback:** Arrange three 3-letter English words so each vertical column matches the corresponding horizontal row. Show completion only after actual rules pass, and explain at least one prominent violation or remaining action.

**Original app icon brief:** three interlocking rose-pink word strips reading identically across and down; no reused third-party art, store screenshots or competitor logo.

**Screenshots to capture:** Word Square Studio — initial board, legal intermediate input, visible recovery/error feedback, verified completed state. No actual phone captures produced yet.

**Known business uncertainty:** Three-word puzzles may be too trivial for practiced word-game players without adding larger verified word squares. Real demand, age fit, spontaneous replay, competitor advantage, acquisition and monetization are all unmeasured. No automatic App Store release.
