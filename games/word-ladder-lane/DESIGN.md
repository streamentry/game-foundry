# Word Ladder Lane — independent visual and interaction system v0.1

**Positioning:** a calm, original four-letter word transformations research puzzle for competent adults, without age stereotypes or health/cognitive treatment claims.

**Three adjectives:** exploratory · verdant · typographic.

| Token | Hex | Intended use |
| --- | --- | --- |
| Paper | `#F6F5ED` | Frame background |
| Ink | `#374539` | Rules, board symbols and text |
| Accent | `#477C69` | Active choices and primary action |
| Soft | `#E4EAE0` | Secondary cells and clues |
| White | `#FFFFFF` | Editable tiles and contrast text |

**Type:** Godot fallback face, 27–28 px title; 15–17 px instructions/status; 18–25 px puzzle symbols; 12–14 px secondary explanation. **Spacing:** 14–20 px margins, 8–13 px vertical rhythm, 3–7 px cell gutters. Controls have ≥48 logical px targets, except fixed visual annotations which are never required to tap; short-screen content scrolls.

**Signature interaction:** Tap an eligible next-word card; each move shows the current ladder, goal and step count. Incorrect inputs remain reversible; the game checks constraints rather than comparing visible colors to a secret answer, except explicitly authored phrase vocabulary. Avoid penalties, dark patterns and mandatory timers.

**Contrast and accessible input:** numbers/letters/symbol labels convey information without hue alone. Native button focus, explicit row/column labels where relevant, visible Undo/Restart and progress; screen-reader navigation and 200% text still require real iOS/Android audits. Do not claim accessibility certification from headless CI.

**Motion:** immediate reversible feedback, no flashing/confetti; **sound/haptics:** none by default.

**Puzzle feedback:** Change exactly one letter at a time, using only available word cards, to reach a given goal in an exact shortest move budget. Show completion only after actual rules pass, and explain at least one prominent violation or remaining action.

**Original app icon brief:** two four-letter words joined by stepping-stone letters; no reused third-party art, store screenshots or competitor logo.

**Screenshots to capture:** Word Ladder Lane — initial board, legal intermediate input, visible recovery/error feedback, verified completed state. No actual phone captures produced yet.

**Known business uncertainty:** Limited card dictionaries lower vocabulary burden but may feel too guided; free-entry word ladders have different appeal. Real demand, age fit, spontaneous replay, competitor advantage, acquisition and monetization are all unmeasured. No automatic App Store release.
