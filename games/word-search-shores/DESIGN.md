# Word Search Shores — independent visual and interaction system v0.1

**Positioning:** a calm, original offline word search research puzzle for competent adults, without age stereotypes or health/cognitive treatment claims.

**Three adjectives:** maritime · airy · legible.

| Token | Hex | Intended use |
| --- | --- | --- |
| Paper | `#F4F8FA` | Frame background |
| Ink | `#243F50` | Rules, board symbols and text |
| Accent | `#296884` | Active choices and primary action |
| Soft | `#DDE9F1` | Secondary cells and clues |
| White | `#FFFFFF` | Editable tiles and contrast text |

**Type:** Godot fallback face, 27–28 px title; 15–17 px instructions/status; 18–25 px puzzle symbols; 12–14 px secondary explanation. **Spacing:** 14–20 px margins, 8–13 px vertical rhythm, 3–7 px cell gutters. Controls have ≥48 logical px targets, except fixed visual annotations which are never required to tap; short-screen content scrolls.

**Signature interaction:** Tap the first and last letters of a target word, in either direction; found words become visibly highlighted. Incorrect inputs remain reversible; the game checks constraints rather than comparing visible colors to a secret answer, except explicitly authored phrase vocabulary. Avoid penalties, dark patterns and mandatory timers.

**Contrast and accessible input:** numbers/letters/symbol labels convey information without hue alone. Native button focus, explicit row/column labels where relevant, visible Undo/Restart and progress; screen-reader navigation and 200% text still require real iOS/Android audits. Do not claim accessibility certification from headless CI.

**Motion:** immediate reversible feedback, no flashing/confetti; **sound/haptics:** none by default.

**Puzzle feedback:** Find three target words in a 6×6 original letter grid by tapping their start and end along a straight horizontal, vertical or diagonal line. Show completion only after actual rules pass, and explain at least one prominent violation or remaining action.

**Original app icon brief:** an original wave-shaped border around a word-search grid; no reused third-party art, store screenshots or competitor logo.

**Screenshots to capture:** Word Search Shores — initial board, legal intermediate input, visible recovery/error feedback, verified completed state. No actual phone captures produced yet.

**Known business uncertainty:** Direct competitors have much more content; requiring two endpoint taps may be less natural than a continuous swipe for some players. Real demand, age fit, spontaneous replay, competitor advantage, acquisition and monetization are all unmeasured. No automatic App Store release.
