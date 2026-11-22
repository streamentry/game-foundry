# Vowel Garden — independent visual and interaction system v0.1

**Positioning:** a calm, original missing-vowel phrase restoration research puzzle for competent adults, without age stereotypes or health/cognitive treatment claims.

**Three adjectives:** welcoming · verbal · nostalgic.

| Token | Hex | Intended use |
| --- | --- | --- |
| Paper | `#FFF8EE` | Frame background |
| Ink | `#514331` | Rules, board symbols and text |
| Accent | `#9A6A45` | Active choices and primary action |
| Soft | `#F3E4C9` | Secondary cells and clues |
| White | `#FFFFFF` | Editable tiles and contrast text |

**Type:** Godot fallback face, 27–28 px title; 15–17 px instructions/status; 18–25 px puzzle symbols; 12–14 px secondary explanation. **Spacing:** 14–20 px margins, 8–13 px vertical rhythm, 3–7 px cell gutters. Controls have ≥48 logical px targets, except fixed visual annotations which are never required to tap; short-screen content scrolls.

**Signature interaction:** Tap a visible underscore, then the large A/E/I/O/U keypad. Undo, Erase and Restart recover choices. Incorrect inputs remain reversible; the game checks constraints rather than comparing visible colors to a secret answer, except explicitly authored phrase vocabulary. Avoid penalties, dark patterns and mandatory timers.

**Contrast and accessible input:** numbers/letters/symbol labels convey information without hue alone. Native button focus, explicit row/column labels where relevant, visible Undo/Restart and progress; screen-reader navigation and 200% text still require real iOS/Android audits. Do not claim accessibility certification from headless CI.

**Motion:** immediate reversible feedback, no flashing/confetti; **sound/haptics:** none by default.

**Puzzle feedback:** Reconstruct original short phrases by inserting A/E/I/O/U into a visible consonant skeleton using topic clues. Show completion only after actual rules pass, and explain at least one prominent violation or remaining action.

**Original app icon brief:** a warm cream word card with highlighted missing vowel slots; no reused third-party art, store screenshots or competitor logo.

**Screenshots to capture:** Vowel Garden — initial board, legal intermediate input, visible recovery/error feedback, verified completed state. No actual phone captures produced yet.

**Known business uncertainty:** Consonant-only prompts can admit multiple sensible phrases; a single canonical answer is potentially unfair without broader acceptance. Real demand, age fit, spontaneous replay, competitor advantage, acquisition and monetization are all unmeasured. No automatic App Store release.
