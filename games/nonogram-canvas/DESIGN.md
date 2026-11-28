# Nonogram Canvas — visual and interaction system v0.2

## 1. Positioning and emotional promise

A quiet picture-cross for competent adults: you deduce a small picture from its row and column numbers and watch it appear. The promise is calm certainty. Every line you finish is confirmed in place, and nothing punishes a wrong mark, so exploring stays cheap. No health, memory or cognitive claims.

**Three adjectives:** patient · pictorial · warm.

**Signature interaction: line lock.** When a row or column first matches its clue, its own squares sweep once in order along that line (the glow runs from the first square to the last) and its clue numbers stay put. The lock is a fact about the clue, not decoration: it happens once per line, never again on untap, and it is always paired with the text count "n/10 clue lines match".

**Palette source:** the icon (coral tiles on a cream card over slate). The game uses the same family: cream paper, slate ink, a deeper coral for filled squares so white text clears 4.5:1.

## 2. Color tokens

| Token | Hex | Use |
| --- | --- | --- |
| `bg` | `#F7F0E1` | Frame background (cream paper) |
| `surface` | `#FFFBF2` | Unknown squares, raised panels |
| `surface_alt` | `#EAE1CB` | Clue cells, secondary buttons |
| `ink` | `#27343D` | Body text, clue numbers, empty-square marks |
| `ink_muted` | `#5B6670` | Eyebrow, captions, crossed-square mark |
| `accent` | `#C4452F` | Filled squares, primary button |
| `accent_hover` | `#B23E29` | Primary button hover |
| `accent_pressed` | `#A83A27` | Primary button pressed |
| `accent_ink` | `#FFFFFF` | Text and ■ glyph on accent |
| `success` | `#2F6B5A` | Completed-status text and border (with the words "Picture revealed!") |
| `error` | `#A32A2A` | Rejected-input text (with words); none fires in normal play |
| `focus` | `#2B5F8C` | 3 px focus ring on every control and square |
| `border` | `#6F7A83` | Square and button outlines |
| `glow` | `#F7C9A6` | Line-lock sweep tint (self_modulate only) |

**WCAG contrast (computed):**
- `ink` on `surface`: 12.36:1; on `surface_alt`: 9.81:1 (both pass).
- `accent_ink` on `accent`: 4.95:1; on `accent_hover`: 5.81:1; on `accent_pressed`: 6.36:1 (pass).
- `error` on `surface`: 6.96:1; `success` on `surface`: 6.03:1 (pass).
- `accent` on `surface`: 4.80:1 (pass, used for filled squares and text-less fills).
- `ink_muted` on `surface_alt`: 4.51:1 (pass, the lowest text pair).
- `focus` on `surface`: 6.52:1; `border` on `surface`: 4.25:1 (non-text boundary, above 3:1).

**State is never color-only.** Filled = accent fill + ■ glyph; crossed = × glyph on surface; unknown = · glyph; completed line = numbers plus words "n/10 clue lines match"; solved = "Picture revealed!" text in `success`. Focus is a ring, not a tint.

## 3. Typography scale (px, default fallback face, regular weight)

| Role | Size | Weight | Notes |
| --- | --- | --- | --- |
| Eyebrow | 14 | regular | `ink_muted`, uppercase |
| Title | 28 | regular | "Let the picture appear." |
| Heading (puzzle name) | 16 | regular | `PUZZLE n OF 6 • name` |
| Body / status | 15 | regular | instructions and status |
| Button label | 17 | regular | min height 54 |
| Caption | 14 | regular | tap rule, footer |
| Clue number | 15 | regular | fits three numbers in 53 px |
| Cell glyph | 22 | regular | ■ × · on 55 px squares |

Type is never below 14 px outside the clue numbers. Body text wraps; nothing is set in a fixed-height box.

## 4. Spacing and layout

- Base unit: 4 px. Screen margins 16 px sides, 20 px top, 24 px bottom.
- Column rhythm 12 px; control row gap 8 px; grid gap 4 px.
- Square 55×55 px; clue cell 53×53 px. Grid width ≈ 53 + 5×55 + 5×4 = 348 px, fits 414 px with margins.
- Min touch target 48 px (buttons 54 px, squares 55 px, the Reduce motion row 48 px).
- Content scrolls vertically on short screens; no horizontal scroll.

## 5. Components

- **Board square:** 55 px, radius 12, `surface` fill, 1 px `border`. Normal → hover (fill +6 % light) → pressed (fill −8 %) → focus (3 px `focus` ring, inset) → disabled (not used for play squares). Filled: `accent` fill, `accent_ink` ■. Crossed: `surface`, `ink_muted` ×.
- **Clue cell:** `surface_alt`, radius 10, no pointer response (disabled), `ink` 15 px numbers.
- **Primary button (Next, Copy challenge when primary):** `accent` fill, `accent_ink` label, radius 12, raised: 3 px shadow at y 2, `rgba(0,0,0,0.14)`. Hover `accent_hover`; pressed `accent_pressed` with shadow 0; focus 3 px `focus` ring; disabled `surface_alt` with `ink_muted` label.
- **Secondary button (Undo, Restart, Previous, Copy):** `surface_alt` fill, `ink` label, 1 px `border`, radius 12, shadow as above. Hover +6 % light; pressed −8 %; focus ring; disabled `bg` with `ink_muted` label and no shadow.
- **Reduce motion row:** `CheckButton`, 17 px `ink` label, 48 px min height, focus ring.
- **Status banner:** the status label under the progress line. Text is `ink`; when all ten lines match it becomes `success` and says "Picture revealed!". Text always carries the meaning.
- **Panels:** no card chrome in the game; the board sits directly on `bg`, raised only through its own 1 px border.

## 6. States

- **Idle:** unknown squares `surface`, clue numbers `ink`.
- **Selected (touch down):** the square dips (press motion) and shows pressed fill; no persistent selection exists.
- **Related / peer:** the ten clue lines the square belongs to are not highlighted; the count text is the peer signal.
- **Conflict:** none in normal play. The game accepts any mark and checks constraints only when counts change, so there is nothing to flag. A rejected input (out-of-range value) would shake the square and use `error` text.
- **Completed line:** line-lock sweep once; the line's clue numbers are unchanged; the count text increments.
- **Solved:** all ten clue lines match and no square is unknown. Status turns `success` with "Picture revealed!"; celebrate burst, then the sweep wave over all 25 squares in reading order.

## 7. Motion system

Shared timings from `scripts/motion.gd`; no game-specific durations.

| Name | Trigger | Duration ms | Easing | Property |
| --- | --- | --- | --- | --- |
| press | Square tapped (any outcome) | 90 | BACK / OUT | `offset_transform_scale` 0.92 → 1 |
| pop | Mark changed (filled, crossed, unknown) | 180 | BACK / OUT | `offset_transform_scale` 1.16×/0.90× → 1 |
| shake | Rejected input | 260 | SINE / IN_OUT | `offset_transform_position` x ±7 px, damped |
| sweep (line lock) | Row/column first matches its clue | 35 stagger, 260 total per square | SINE / OUT then BACK / OUT | `offset_transform_scale` 1.08, `self_modulate` to `glow` and back |
| intro | Level load, Restart | 16 stagger (cap 420), 220 | BACK / OUT scale; CUBIC / OUT position, alpha | scale 0.86 → 1, y +10 → 0, alpha 0 → 1 |
| slide | Next / Previous | 280 | CUBIC / OUT | board `offset_transform_position` ±48 px x, alpha 0 → 1 |
| celebrate | Solved | ≤ 1100, 22 pieces | CUBIC / OUT | piece position, rotation, alpha (freed after) |

**Signature moment (line lock):** a completed line sweeps only its own squares, from first to last, so the eye follows the clue it just satisfied. Solved adds the full-board reading-order wave and one soft burst; no flashing, no strobe.

Level change never waits on animation: the new puzzle is applied first, then `slide_swap` runs.

## 8. Reduced motion

A visible `CheckButton` "Reduce motion" in the control area persists to `user://settings.json` as `{"reduced_motion": bool}`. When on, every motion call applies its final state synchronously: no tweens, no slide, no stagger, no sweep glow, and no celebration burst. Completion is still shown by the words "Picture revealed!" and the count. Nothing flashes at any setting.

## 9. Haptics, accessibility, icon and screenshots

- **Haptics:** none shipped, none planned for v0.2. Any future haptic would fire only on solved and only with a settings toggle.
- **Sound:** none.
- **Accessibility checklist:** every square has an `accessibility_name` with row, column and glyph state; all controls have focus rings; touch targets ≥48 px; type ≥14 px outside clue numbers; text pairs ≥4.5:1; no color-only state; Reduce motion present. Still required on real devices: VoiceOver/TalkBack traversal, 200% text, and a 40+ tester session.
- **Icon:** keep the icon as-is (coral tiles, cream card, slate ground). The palette above is its in-game counterpart; do not change the icon files.
- **Screenshots (store, later):** initial board with clue headers; a legal intermediate state with crossed squares; a line-locked state showing the count; the solved state with "Picture revealed!".

## 10. QA checklist

1. Tap, pop and press behave on every square; a line lock fires once per line and never on untap.
2. Reduce motion on: taps, Next, Previous and Restart produce no tween (`Motion.running` is 0 right after a tap).
3. Reduce motion off: a tap starts motion and the square returns to rest within 0.6 s.
4. Solved state shows "Picture revealed!" in text with `success`, in both motion settings.
5. At 414×896, no clipped clue numbers or buttons; scroll reaches the footer.
6. Focus ring visible on every button and square when navigated by keyboard.
7. Every text pair in section 2 still meets its ratio after any color change (recompute).
8. Headless tests (`test_rules`, `test_progress`, `test_scene`, `test_motion`) exit 0 with no `SCRIPT ERROR`, `Parse Error` or `ERROR:` lines.
