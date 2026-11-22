# Number Pyramid — independent visual and interaction system v0.1

**Positioning:** a calm, original number sum pyramid research puzzle for competent adults, without age stereotypes or health/cognitive treatment claims.

**Three adjectives:** stonework · calm · constructive.

| Token | Hex | Intended use |
| --- | --- | --- |
| Paper | `#F8F4E9` | Frame background |
| Ink | `#4C3C35` | Rules, board symbols and text |
| Accent | `#A56743` | Active choices and primary action |
| Soft | `#F0DFCC` | Secondary cells and clues |
| White | `#FFFFFF` | Editable tiles and contrast text |

**Type:** Godot fallback face, 27–28 px title; 15–17 px instructions/status; 18–25 px puzzle symbols; 12–14 px secondary explanation. **Spacing:** 14–20 px margins, 8–13 px vertical rhythm, 3–7 px cell gutters. Controls have ≥48 logical px targets, except fixed visual annotations which are never required to tap; short-screen content scrolls.

**Signature interaction:** Select a bottom stone and enter a digit 1–9; upper stones show live derived sums and fixed clue labels. Incorrect inputs remain reversible; the game checks constraints rather than comparing visible colors to a secret answer, except explicitly authored phrase vocabulary. Avoid penalties, dark patterns and mandatory timers.

**Contrast and accessible input:** numbers/letters/symbol labels convey information without hue alone. Native button focus, explicit row/column labels where relevant, visible Undo/Restart and progress; screen-reader navigation and 200% text still require real iOS/Android audits. Do not claim accessibility certification from headless CI.

**Motion:** immediate reversible feedback, no flashing/confetti; **sound/haptics:** none by default.

**Puzzle feedback:** Reconstruct four bottom digits 1–9 from scattered clues; each of the six upper numbers equals the sum of the two directly below. Show completion only after actual rules pass, and explain at least one prominent violation or remaining action.

**Original app icon brief:** ten stacked limestone blocks, with the top etched with a sum sign; no reused third-party art, store screenshots or competitor logo.

**Screenshots to capture:** Number Pyramid — initial board, legal intermediate input, visible recovery/error feedback, verified completed state. No actual phone captures produced yet.

**Known business uncertainty:** Without scaffolded deduction explanations, this may feel like arithmetic guesswork rather than an enjoyable logic game. Real demand, age fit, spontaneous replay, competitor advantage, acquisition and monetization are all unmeasured. No automatic App Store release.
