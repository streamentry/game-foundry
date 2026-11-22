# Pipe Garden — independent visual and interaction system v0.1

**Positioning:** a calm, original pipe rotation research puzzle for competent adults, without age stereotypes or health/cognitive treatment claims.

**Three adjectives:** botanical · engineered · tranquil.

| Token | Hex | Intended use |
| --- | --- | --- |
| Paper | `#F4F8F6` | Frame background |
| Ink | `#27433F` | Rules, board symbols and text |
| Accent | `#387A75` | Active choices and primary action |
| Soft | `#DDEEE7` | Secondary cells and clues |
| White | `#FFFFFF` | Editable tiles and contrast text |

**Type:** Godot fallback face, 27–28 px title; 15–17 px instructions/status; 18–25 px puzzle symbols; 12–14 px secondary explanation. **Spacing:** 14–20 px margins, 8–13 px vertical rhythm, 3–7 px cell gutters. Controls have ≥48 logical px targets, except fixed visual annotations which are never required to tap; short-screen content scrolls.

**Signature interaction:** Tap a tile to rotate; curved and end pieces have four orientations, straights have two. Incorrect inputs remain reversible; the game checks constraints rather than comparing visible colors to a secret answer, except explicitly authored phrase vocabulary. Avoid penalties, dark patterns and mandatory timers.

**Contrast and accessible input:** numbers/letters/symbol labels convey information without hue alone. Native button focus, explicit row/column labels where relevant, visible Undo/Restart and progress; screen-reader navigation and 200% text still require real iOS/Android audits. Do not claim accessibility certification from headless CI.

**Motion:** immediate reversible feedback, no flashing/confetti; **sound/haptics:** none by default.

**Puzzle feedback:** Rotate nine pipes on a 3×3 board into a single continuous, leak-free route from one endpoint to the other. Show completion only after actual rules pass, and explain at least one prominent violation or remaining action.

**Original app icon brief:** two jade pipe elbows connecting a single source and destination; no reused third-party art, store screenshots or competitor logo.

**Screenshots to capture:** Pipe Garden — initial board, legal intermediate input, visible recovery/error feedback, verified completed state. No actual phone captures produced yet.

**Known business uncertainty:** Rotating small pipes can become mechanical trial-and-error rather than strategic play, and 3×3 grids are beginner-only. Real demand, age fit, spontaneous replay, competitor advantage, acquisition and monetization are all unmeasured. No automatic App Store release.
