# Kuromasu Windows — independent game design system (v0.1)

**Positioning:** a calm, approachable and original Kuromasu / Kurodoko experiment; no medical/cognitive outcome claims, age stereotypes, timer, streak trap or purchase prompt.

**Three adjectives:** architectural · contemplative · monochrome.

| Role | Color | Purpose |
| --- | --- | --- |
| Paper | `#F8F4EC` | Main background |
| Ink | `#333D45` | Numbered clues, text, and labels |
| Accent | `#4C5565` | Focus, primary buttons and active marks |
| Pale | `#EAE4D8` | Alternative puzzle states and non-primary controls |
| White | `#FFFFFF` | Editable cells and high-contrast text |

**Type:** native/system fallback; 28px title, 15–17px rule/status, 14–21px cell captions, 13px secondary. **Spacing:** 16px horizontal gutters, ~12px rhythm, 4px between cells; interactive cells are 53px (5×5 fleet) or 76px (4×4 games), each action ≥54px. Scroll on short screens, not tiny controls.

**Puzzle feedback:** Tap a blank square black → white → unknown. Numbered clues remain fixed white; text reports sight-line and connectivity violations. The board uses text/letters/numbers as well as color to distinguish states; the visible rules engine checks exact constraints. Incorrect choices display a specific rule warning where possible, without manipulating player input. Undo and Restart always visible.

**Motion:** instantaneous reversible presses; no forced animation or time pressure. **Audio and haptics:** absent; later opt-in testing only.

**Icon brief:** a warm white window looking along a line of light squares past one dark blocking square. Original vector/iconography; do not reuse competitor logos or artwork.

**Screenshot plan:** fixed numbered windows, shaded cells, connected-white feedback, solved sight lines. No real phone screenshots have been captured.

**Accessibility:** minimum 53px touch targets, native focusable buttons, cell row/column accessibility labels and readable text. True nonvisual spatial puzzle equivalence, contrast at 200% text, safe-area and VoiceOver/TalkBack testing must be verified on actual devices before distribution.

**Original content:** six independently authored, exact-rule uniquely-solvable fixtures. This visual system alone is **not** a defensible differentiator. User preference, return behavior, search intent, distribution cost and monetization are all unmeasured.
