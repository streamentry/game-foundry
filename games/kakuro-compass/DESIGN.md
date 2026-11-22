# Kakuro Compass — independent design system (v0.1)

**Positioning:** calm cross-sum deduction with both crossing clues readable at once. This is an honest puzzle, not a calculator that secretly fills the answer. No medical or age-related cognitive-benefit claims.

**Three adjectives:** navigable · scholarly · reassuring.

## Semantic tokens

| Role | Hex | Use |
| --- | --- | --- |
| Soft paper | `#F8F8F1` | Background |
| Ink | `#263B40` | Numbers and headings |
| Navy slate | `#274D5B` | Compass text |
| Deep teal | `#167F7A` | Primary action, active focus |
| Dark clue | `#2D5360` | Noneditable arrow-sum tiles |
| Selected | `#D0EEE8` | Active number cell |
| Pale action | `#E5F0E9` | Secondary buttons |
| White | `#FFFFFF` | Editable numbers |
| Warning | `#9D4A38` | Contradiction text, if shown |

**Type scale:** system fallback; 29px title, 22px entries, 15px clue sums and explanations, 17px keypad, 13px helper. Emphasis comes from numeric information and explicit axis labels, never just color.

**Spacing and targets:** 16px horizontal padding, 10px rhythm, 69×69 logical px board cells, ≥52px keypad/actions, 3px board gutters. Vertical scroll on small phones. Screen reader cell descriptions state row/column/digit; clue tile labels identify the direction and target.

**Puzzle feedback:** for the active white cell show **Across target → locally feasible digits**, **Down target → locally feasible digits**, **intersection**. Feasibility is exact under all existing digits in that run: no repeats and remaining digits must be able to reach the sum. Local feasible values are **not global proof** or the unique final answer. Invalid entry shows text rather than being silently rejected.

**Motion:** no forced timer, streak display, confetti, auto-typing or background animation. **Sound/haptics:** omitted for this pilot; opt-in if players request it.

**Icon brief:** two thin sum arrows meeting in a bright teal compass-like cross on slate, with one highlighted digit. Must be original and not visually imitate existing Kakuro titles.

**Screenshot brief:** untouched board, highlighted crossing clue inspector, an explained contradiction, and a completed board on a compact phone. No physical screenshots have been recorded yet.

**Accessibility boundaries:** large keypad and board, focus and numeric labels, no required drag. Test VoiceOver/TalkBack, 200% text scaling, scroll-safe layout and adequate contrast on actual phones before distribution. The fixed 5×5 board and current labels do not establish nonvisual puzzle equivalence.

**Originality and evidence:** original eight clue datasets. This is [GF-001](../../docs/research/first-game-decision-2026-11-21.md), but App Store search demand, competitor advantage and age fit remain **unverified**.
