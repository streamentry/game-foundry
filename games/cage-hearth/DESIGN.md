# Cage Hearth — design system (v0.2, upgrade pass)

## 1. Positioning, promise, signature

- **Positioning:** A quiet arithmetic puzzle for adults who like a clean numbers-and-rules
  problem: fill 1–4 once per row and column so every colored cage meets its clue. Nothing is
  timed, nothing is scored against a reference answer, and no health or cognition claim is made.
- **Emotional promise:** "The numbers settle into place." Each correct cage should feel like a
  small warm seal on a hearth-side mosaic, earned by the player's own reasoning.
- **Three adjectives:** hearthside · mathematical · crafted.
- **Signature interaction: the cage seal.** When a cage's clue becomes fully satisfied, its
  cells receive a single warm ember sweep in reading order (about 35 ms apart). The sweep
  confirms the rule was met; it does not reward speed.

## 2. Color tokens

Icon family: ember background, cream tiles, deep maroon dashes. The palette keeps the cream
paper and moves the accent and ink toward those icon hues.

| Token | Hex | Use |
| --- | --- | --- |
| `bg` | `#FBF5EC` | Screen background (cream, echoes icon tiles) |
| `surface` | `#FFFFFF` | Editable empty cells, keypad surface |
| `surface_alt` | `#F3E4D6` | Secondary buttons, fixed clue cells, panels |
| `ink` | `#4A1E14` | Numbers, cage targets, body text (deep maroon, echoes icon dashes) |
| `ink_muted` | `#7A5246` | Captions and secondary help text |
| `accent` | `#A8401E` | Primary keypad and Next button, kicker text (ember rust, icon background family) |
| `accent_ink` | `#FFFFFF` | Text on `accent` |
| `success` | `#2E6B45` | Completed-cage and solved status text, with a sentence |
| `error` | `#A31F1F` | Conflict status text, with a sentence |
| `focus` | `#4A1E14` | 3 px focus ring on every control (high contrast on all surfaces) |
| `selected` | `#C4E3DD` | Selected empty cell background (also carries a 3 px ink outline) |
| cage tints | `#EEDCD3` `#F5E9D2` `#E0E8D7` `#E3E4EE` `#F1DFE7` `#E6E2D9` | Cage group backgrounds, cycled by cage serial |

**WCAG 2.2 contrast ratios (computed):**
- `ink` on `surface` 14.12:1; `ink` on `bg` 13.03:1; `ink` on `surface_alt` 11.35:1 (all ≥ 4.5:1)
- `accent_ink` on `accent` 6.15:1 (≥ 4.5:1)
- `error` on `surface` 7.57:1; `success` on `surface` 6.35:1; `ink_muted` on `surface` 6.72:1
- `ink` on every cage tint 10.9–11.8:1; `ink` on `selected` 10.34:1; `ink` on fixed clue 9.68:1
- `accent` on `bg` 5.67:1 (kicker text, ≥ 4.5:1)

**State is never color-only:** a selected cell has a 3 px ink outline plus a text caption; a
conflict shows its sentence in the status line; a completed cage shows the seal sweep and its
status line; fixed clues are disabled and drawn in a warm tint.

## 3. Typography scale (px, system font)

| Role | Size | Weight |
| --- | --- | --- |
| Kicker (`CAGE HEARTH / QUIET ARITHMETIC`) | 12 | bold, letter-spaced caps |
| Title (headline) | 27 | bold |
| Heading (puzzle name line) | 16 | semibold |
| Body and rule text | 15 | regular |
| Buttons (keypad, actions) | 17 (keypad digits 24) | semibold |
| Cage target label | 18 | semibold |
| Cell digit | 22 | bold |
| Caption / secondary help | 13 | regular, `ink_muted` |

Text scales with the system setting; do not shrink critical type to fit.

## 4. Spacing & layout

- **Base unit:** 4 px. Vertical rhythm 12 px between blocks.
- **Safe margins:** 17 px left and right, 21 px top, 22 px bottom; respect device safe areas.
- **Grid gaps:** 3 px between puzzle cells; 7 px between keypad keys; 8 px between action buttons.
- **Minimum touch target:** 48 px (all buttons 54 px tall; puzzle cells 75 px).
- **Scroll:** the whole column scrolls vertically; nothing is clipped at 414 × 896.

## 5. Components

- **Board cell:** 75 × 75 button. Empty: `surface`. Fixed clue: `surface_alt`, disabled.
  Cage tint behind the cell; cage target caption in the top-left cell of each cage.
- **Controls / keypad:** digits 1–4 in a 4-column grid, primary style; Undo, Erase, Restart
  in a row; Previous and Next puzzle in a row; "Copy this challenge"; a `CheckButton` labelled
  "Reduce motion" below them.
- **Primary button:** `accent` fill, `accent_ink` text, 12 px corners, 1 px `accent` border.
  Hover: `accent` lightened 6%. Pressed: `accent` darkened 8%, 0 px shadow. Focus: 3 px `focus`
  ring. Disabled: `surface_alt` fill, `ink_muted` text.
- **Secondary button:** `surface_alt` fill, `ink` text, 12 px corners, 1 px `#C9B3A4` border. Hover and
  pressed darken the fill 5% and 10%. Focus: 3 px `focus` ring. Disabled: `bg` fill, `ink_muted` text (5.4:1 on `surface_alt`).
- **Raised surfaces** (buttons, status panel): `shadow_size` 2, `shadow_color` `#4A1E14` at 14%.
- **Panel:** `surface` at 12 px corners with 1 px `surface_alt` border (status / help).
- **Status banner:** single line below the grid, always with a text sentence. Conflict uses
  `error`, complete uses `success`, neutral uses `ink`. No icon glyphs (the default font lacks them).

## 6. States

- **Idle:** empty cell `surface`; tinted cage background shows membership.
- **Selected:** `selected` fill, 3 px `ink` outline, keypad enabled.
- **Related / peer highlight:** the other cells of the selected cell's cage share a 1 px
  `ink_muted` inner outline (no fill change, so cage tint still reads).
- **Conflict:** the new entry that repeats a row or column value, or breaks a cage total, shows
  the `error` status line; the entered cell shakes once (only when the conflict is new).
- **Completed cage:** the cage seal sweep (see §7); the status line names it.
- **Solved:** the status line reads "A complete little mosaic!"; all cells sweep and a restrained
  ember tile burst plays from the board center.

## 7. Motion system

| Name | Trigger | Duration (ms) | Easing | Property |
| --- | --- | --- | --- | --- |
| press | select / tap a cell | 90 | BACK / OUT | `offset_transform_scale` 0.92 → 1 |
| pop | digit placed, erased or changed | 180 | BACK / OUT | `offset_transform_scale` overshoot → 1 |
| shake | new conflict on the entered cell | 260 | SINE / IN_OUT | `offset_transform_position` x, damped |
| sweep | cage seal (cells in reading order) | 35 stagger, 260 | SINE / OUT, then BACK / OUT | `offset_transform_scale`, `self_modulate` |
| intro | level load and Restart | 16 stagger, 220 | BACK / OUT, CUBIC / OUT | scale, position, `modulate:a` |
| slide | Next / Previous puzzle | 280 | CUBIC / OUT | board `offset_transform_position`, `modulate:a` |
| celebrate | solved | ≤ 1100 (22 pieces max) | CUBIC / OUT | piece position, rotation, alpha |

**Signature moment (cage seal):** the sweep glows each cell of the just-satisfied cage with a
warm `#FFEBB0` tint and a 1.08 lift, one cell after another. It runs only for newly satisfied
cages, so it fires once per cage, not on every tap.

## 8. Reduced motion

- Every motion above becomes instant: final state applied at once, no tweens created.
- The solved celebration (ember tile burst) is omitted entirely.
- No flashing anywhere: the seal glow is a single tint change that ends at rest, and the
  celebration pieces fade rather than blink.
- The toggle lives in the controls area, persists in `user://settings.json`
  (`{"reduced_motion": bool}`), and applies immediately.

## 9. Haptics, accessibility, icon & screenshots

- **Haptics:** none shipped. Intent: none unless a later playtest shows value.
- **Sound:** none by default.
- **Accessibility checklist:** 48 px targets; cell accessible names give row, column and digit
  or "empty"; cage accessible descriptions give target and operator; focus ring on every
  control; status changes are text, never color alone; Reduce motion toggle; no timers.
- **Icon:** keep the ember-square icon (cream 3×3 mosaic with dashed cages and a "7") as the
  family anchor. The in-game accent `#A8401E` and ink `#4A1E14` are sampled from its background
  and dashes.
- **Screenshots:** a partially filled board with one cage showing a conflict sentence, one board
  with a completed cage seal, and the solved mosaic. Captures must be made on real phones before
  any store listing; desktop captures are not evidence of phone usability.

## 10. QA checklist

1. Every control reachable by keyboard focus with a visible 3 px ring.
2. Every state (idle, selected, conflict, completed cage, solved) is readable with color removed.
3. At 414 × 896, nothing clips horizontally; the Reduce motion toggle is reachable by scrolling.
4. With Reduce motion on, the headless motion test finds zero running motion tweens.
5. With Reduce motion off, a placed value returns to rest within 0.6 s.
6. Cage seal fires once per newly satisfied cage, never on unchanged cages.
7. Level change slides the board and the new board's cells stagger; undo/erase do not slide.
8. Real-phone and 200% text checks are still pending and must be recorded before release.
