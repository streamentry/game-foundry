# Renzoku Meadow — independent visual and interaction system v0.1

**Positioning:** a calm, original consecutive Latin squares research puzzle for competent adults, without age stereotypes or health/cognitive treatment claims.

**Three adjectives:** botanical · grounded · numeric.

| Token | Hex | Intended use |
| --- | --- | --- |
| Paper | `#F7F7ED` | Frame background |
| Ink | `#33443B` | Rules, board symbols and text |
| Accent | `#637E57` | Active choices and primary action |
| Soft | `#E5EDD9` | Secondary cells and clues |
| White | `#FFFFFF` | Editable tiles and contrast text |

**Type:** Godot fallback face, 27–28 px title; 15–17 px instructions/status; 18–25 px puzzle symbols; 12–14 px secondary explanation. **Spacing:** 14–20 px margins, 8–13 px vertical rhythm, 3–7 px cell gutters. Controls have ≥48 logical px targets, except fixed visual annotations which are never required to tap; short-screen content scrolls.

**Signature interaction:** Select a square, enter a digit 1–4; in-cell arrows identify marked right/below edges. Incorrect inputs remain reversible; the game checks constraints rather than comparing visible colors to a secret answer, except explicitly authored phrase vocabulary. Avoid penalties, dark patterns and mandatory timers.

**Contrast and accessible input:** numbers/letters/symbol labels convey information without hue alone. Native button focus, explicit row/column labels where relevant, visible Undo/Restart and progress; screen-reader navigation and 200% text still require real iOS/Android audits. Do not claim accessibility certification from headless CI.

**Motion:** immediate reversible feedback, no flashing/confetti; **sound/haptics:** none by default.

**Puzzle feedback:** 4×4 full Renzoku: fill 1–4 once per row/column, with marked consecutive edges and unmarked nonconsecutive edges. Show completion only after actual rules pass, and explain at least one prominent violation or remaining action.

**Original app icon brief:** four moss-toned number tiles with a delicate single-dot bridge; no reused third-party art, store screenshots or competitor logo.

**Screenshots to capture:** Renzoku Meadow — initial board, legal intermediate input, visible recovery/error feedback, verified completed state. No actual phone captures produced yet.

**Known business uncertainty:** Full negative-edge rule may surprise users familiar only with positive-dot Kropki, which is already another studio prototype. Real demand, age fit, spontaneous replay, competitor advantage, acquisition and monetization are all unmeasured. No automatic App Store release.
