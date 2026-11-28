# Skyline Steps — design system (v0.2, upgrade pass)

## 1. Positioning and emotional promise

A quiet street-level puzzle: the player reads a city from every side and places each height
once per row and column. The promise is calm agency. Every move answers immediately, errors
are explained in words, and nothing hurries the player. No timer, streak, ad, login or health
claim. Accessible to adults 40+ by clear contrast, large targets and adjustable motion, not by
simplified or "senior" styling.

**Three adjectives:** architectural · spacious · legible.

**Signature interaction:** placing a height gives a small squash-and-settle pop on that one
square. Completing a row or column makes its squares glow in a single wave from left to right,
so the player sees which sight line is now settled.

## 2. Colour tokens

Palette taken from the app icon (night navy, amber and orange towers, cream sign): the
family and mood carry across icon, board and buttons.

| Token | Hex | Use |
| --- | --- | --- |
| `bg` | `#FAF4E8` | Screen paper (cream, from the icon sign) |
| `surface` | `#FFFDF7` | Editable squares, secondary buttons |
| `surface_alt` | `#EFE5CF` | Given (fixed) squares |
| `peer` | `#F5EDDB` | Row and column of the selected square |
| `selected` | `#FBD6A0` | Selected square fill (plus 3 px ink border) |
| `ink` | `#1E2847` | Numbers, clues, headings (icon night navy) |
| `ink_muted` | `#555C73` | Eyebrow, help text, control outlines |
| `accent` | `#EE8E45` | Primary buttons (icon tower orange) |
| `accent_ink` | `#1E2847` | Text on accent |
| `success` | `#2E6F4E` | Solved message (always with text) |
| `error` | `#A8322A` | Conflict message (always with text) |
| `focus` | `#1E2847` | 3 px focus ring |

Contrast (WCAG 2.2, computed):

- `ink` on `bg` 13.22:1; `ink` on `surface` 14.24:1; `ink` on `surface_alt` 11.57:1;
  `ink` on `peer` 12.43:1; `ink` on `selected` 10.50:1
- `accent_ink` on `accent` 5.93:1
- `error` on `surface` 6.54:1; `success` on `surface` 5.90:1
- `ink_muted` on `bg` 6.06:1
- Non-text: `accent` alone on `bg` is 2.23:1, so every button also has a 1 px `ink_muted`
  outline (6.06:1 on `bg`), and the focus ring is `ink` (13.22:1).

**State is never colour-only.** Selected = fill + thick border + accessibility label
"selected"; given = fill + "given" label + disabled; conflict = red text that begins with the
rule sentence; solved = green text with the word "Complete!".

## 3. Typography (px, system font)

| Role | Size | Weight | Notes |
| --- | --- | --- | --- |
| Eyebrow | 12 | regular | caps, `ink_muted` |
| Title | 27 | regular | "See the city from every side." |
| Heading | 16 | regular | puzzle name line |
| Body / status | 15 | regular | rules text, feedback, progress |
| Button | 17 | regular | digits and actions |
| Clue | 18 | regular | edge counts |
| Cell digit | 23 | regular | entries; "·" for empty |
| Help | 13 | regular | footnote, `ink_muted` |
| Reduce-motion toggle | 16 | regular | |

Text grows with the layout (autowrap); the screen scrolls vertically rather than shrinking type.

## 4. Spacing and layout

- Base unit 4 px. Screen margins 17 px left/right, 21 px top, 22 px bottom.
- Column rhythm 12 px. Board gutters 3 px. Digit-pad gutters 7 px. Action row gutters 8 px.
- Minimum touch target 48 px. Clue cells and grid cells are 50 × 50; buttons are 54 px tall;
  the reduce-motion toggle is 48 px tall.
- Layout tested at 414 × 896 (portrait, the project's base size).

## 5. Components

- **Board cell (editable):** `surface` fill, 1 px `ink_muted` border, no shadow, 0-radius
  12 px radius, no shadow, so the board reads as one flat sheet. States below.
- **Given cell:** `surface_alt`, disabled (cannot be tapped), ink digit.
- **Clue cell:** no fill, `ink` 18 px number or dash.
- **Digit pad / primary button:** `accent` fill, `accent_ink` text, 12 px radius, 3 px shadow
  at 18% ink. Hover +5% lightness; pressed −8% lightness, no shadow, 2 px border; focus 3 px
  `ink` ring; disabled = flat fill, no shadow.
- **Secondary button (Undo, Erase, Restart, Previous, Copy):** `surface` fill, `ink` text,
  same radii and shadow rules as primary.
- **Panel:** the screen itself is the only panel; no nested cards.
- **Status banner:** the feedback line under the board, 15 px; colour changes with the message
  (`ink` idle, `error` for a conflict, `success` for solved), text always carries the meaning.

## 6. States

| State | Visual | Text / label |
| --- | --- | --- |
| Idle | `surface` | "·" |
| Selected | `selected` fill, 3 px `ink` border | label ends ", selected" |
| Peer | `peer` fill (same row or column as selected) | none |
| Given | `surface_alt`, disabled | label ends ", given" |
| Conflict | error text; selected square shakes once when a conflict first appears | rule sentence |
| Completed line | transient glow wave on that line only (no persistent mark) | none |
| Solved | green text, celebration burst (≤ 22 pieces, ≤ 1.1 s) | "Complete!" |

## 7. Motion system

Shared module timings from `scripts/motion.gd` (unchanged template):

| Name | Trigger | Duration | Easing | Property |
| --- | --- | --- | --- | --- |
| press | tap a square | 90 ms | BACK out | `offset_transform_scale` 0.92 → 1 |
| pop | value placed or erased (no line completed) | 180 ms | BACK out | `offset_transform_scale` 1.16/0.90 → 1 |
| shake | conflict first appears, on selected square | 260 ms | SINE in-out | `offset_transform_position` x, damped |
| sweep | row or column newly full and matching its clues; 35 ms stagger per square | 260 ms | SINE out, BACK out | `offset_transform_scale` 1.08 → 1 plus `self_modulate` glow → white |
| stagger_in | level load, restart; 16 ms stagger (max 420 ms) | 220 ms | BACK / CUBIC out | scale 0.86 → 1, y +10 → 0, alpha |
| slide_swap | next / previous puzzle | 280 ms | CUBIC out | x ±48 → 0, alpha |
| celebrate | solved; 22 pieces | ≤ 1100 ms | CUBIC out | position, rotation, alpha; pieces freed |

**Signature moment:** the completed-line wave. Solving the whole board adds the celebration
burst on the board's centre and sweeps all squares in reading order. The burst uses only
`ACCENT`, `SUCCESS` and `SURFACE` on pieces that never flash.

Game state never waits on motion: the entry is applied before any tween starts.

## 8. Reduced motion

- A visible "Reduce motion" switch sits under "Copy this challenge". Its value is stored in
  `user://settings.json` as `{"reduced_motion": bool}` and applied on load and on every toggle.
- With it on, every motion call applies the rest state synchronously. Stagger, slide, pop,
  shake and sweep create no tweens. The solved celebration is omitted entirely.
- Nothing flashes. Glow is a tint that returns to white. No strobing at any setting.
- Limitation: a tween already running when the switch flips finishes its ≤ 1.1 s course.

## 9. Haptics, accessibility, icon and screenshots

- **Haptics:** none shipped. Intent is a single light tap on a conflict if a later build adds
  it; off by default and never the only cue.
- **Accessibility checklist:** every square has a label with row, column, value and
  given/selected state; focus ring on every control; non-colour explanations for every status;
  tap targets ≥ 48 px; reduce-motion switch; text wraps rather than clipping.
- **Icon:** keep the existing icon (navy night, four amber and orange towers, cream sign with
  the number 4). Do not change it; the palette above is drawn from it.
- **Screenshot brief:** top/side clues with three squares entered; a conflict explained in
  red text; a completed row glowing; the solved state with "Complete!". Capture at 414 × 896.

## 10. QA checklist

1. Each square at 414 × 896 is ≥ 48 px and no label clips on the digit pad or action rows.
2. Every control shows the focus ring when reached by keyboard.
3. Conflict and solved messages can be read with colour removed (text is present).
4. Reduce motion on: placing a height, undo, restart, next puzzle and solving all finish
   instantly with no tween left running.
5. Reduce motion off: each motion ends at exactly rest (scale 1, position 0, alpha 1).
6. Undo and Restart restore entries exactly; progress unlocks as before.
7. Nothing animates more than once per action; no flashing.
8. Real iOS/Android phone check of contrast, touch and text size is still pending and
   unverified for the 40+ audience.
