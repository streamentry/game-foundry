# Battleship Harbor — independent game design system (v0.1)

**Positioning:** a calm, approachable and original Battleships logic experiment; no medical/cognitive outcome claims, age stereotypes, timer, streak trap or purchase prompt.

**Three adjectives:** nautical · tactical · quiet.

| Role | Color | Purpose |
| --- | --- | --- |
| Paper | `#F2F8FB` | Main background |
| Ink | `#21435B` | Numbered clues, text, and labels |
| Accent | `#267B9D` | Focus, primary buttons and active marks |
| Pale | `#E0EEF3` | Alternative puzzle states and non-primary controls |
| White | `#FFFFFF` | Editable cells and high-contrast text |

**Type:** native/system fallback; 28px title, 15–17px rule/status, 14–21px cell captions, 13px secondary. **Spacing:** 16px horizontal gutters, ~12px rhythm, 4px between cells; interactive cells are 53px (5×5 fleet) or 76px (4×4 games), each action ≥54px. Scroll on short screens, not tiny controls.

**Puzzle feedback:** Tap a square ship → optional water note → blank; current/target row and column totals remain visible; only six ship cells need placing. The board uses text/letters/numbers as well as color to distinguish states; the visible rules engine checks exact constraints. Incorrect choices display a specific rule warning where possible, without manipulating player input. Undo and Restart always visible.

**Motion:** instantaneous reversible presses; no forced animation or time pressure. **Audio and haptics:** absent; later opt-in testing only.

**Icon brief:** three short ivory ships above a deep teal chart with horizontal coordinate ticks. Original vector/iconography; do not reuse competitor logos or artwork.

**Screenshot plan:** empty chart with total counters, ship/water notes, detected invalid diagonal contact, completed fleet. No real phone screenshots have been captured.

**Accessibility:** minimum 53px touch targets, native focusable buttons, cell row/column accessibility labels and readable text. True nonvisual spatial puzzle equivalence, contrast at 200% text, safe-area and VoiceOver/TalkBack testing must be verified on actual devices before distribution.

**Original content:** six independently authored, exact-rule uniquely-solvable fixtures. This visual system alone is **not** a defensible differentiator. User preference, return behavior, search intent, distribution cost and monetization are all unmeasured.
