# Sliding Tiles — independent visual and interaction system v0.1

**Positioning:** a calm, original 8-puzzle sliding blocks research puzzle for competent adults, without age stereotypes or health/cognitive treatment claims.

**Three adjectives:** tactile · classic · reassuring.

| Token | Hex | Intended use |
| --- | --- | --- |
| Paper | `#F4F1EC` | Frame background |
| Ink | `#3F3B35` | Rules, board symbols and text |
| Accent | `#6F6B58` | Active choices and primary action |
| Soft | `#E3DFD2` | Secondary cells and clues |
| White | `#FFFFFF` | Editable tiles and contrast text |

**Type:** Godot fallback face, 27–28 px title; 15–17 px instructions/status; 18–25 px puzzle symbols; 12–14 px secondary explanation. **Spacing:** 14–20 px margins, 8–13 px vertical rhythm, 3–7 px cell gutters. Controls have ≥48 logical px targets, except fixed visual annotations which are never required to tap; short-screen content scrolls.

**Signature interaction:** Tap only a tile adjacent to the blank; Undo and Restart are always available. Incorrect inputs remain reversible; the game checks constraints rather than comparing visible colors to a secret answer, except explicitly authored phrase vocabulary. Avoid penalties, dark patterns and mandatory timers.

**Contrast and accessible input:** numbers/letters/symbol labels convey information without hue alone. Native button focus, explicit row/column labels where relevant, visible Undo/Restart and progress; screen-reader navigation and 200% text still require real iOS/Android audits. Do not claim accessibility certification from headless CI.

**Motion:** immediate reversible feedback, no flashing/confetti; **sound/haptics:** none by default.

**Puzzle feedback:** Slide numbered tiles orthogonally into the empty square to reconstruct the ordered 1–8 board. Show completion only after actual rules pass, and explain at least one prominent violation or remaining action.

**Original app icon brief:** an original tile arrangement with one open ivory square and eight slate-numbered stones; no reused third-party art, store screenshots or competitor logo.

**Screenshots to capture:** Sliding Tiles — initial board, legal intermediate input, visible recovery/error feedback, verified completed state. No actual phone captures produced yet.

**Known business uncertainty:** The 8-puzzle is extremely commoditized; deterministic reachability is engineering hygiene, not a commercial differentiator. Real demand, age fit, spontaneous replay, competitor advantage, acquisition and monetization are all unmeasured. No automatic App Store release.
