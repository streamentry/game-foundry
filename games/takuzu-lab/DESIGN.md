# Takuzu Lab — game design spec

Scope: in-game design for the 4×4 / 6×6 binary logic board. Rules, puzzle generation and save format are unchanged.

## 1. Positioning & emotional promise

A quiet, tactile logic board for a player who wants to think without a clock. Every tap is a deliberate, reversible choice, and every settled token confirms it. Takuzu Lab is a reference build; it is not a validated product.

- Three adjectives: **calm, tactile, exact**.
- Signature interaction: a tap cycles a cell blank → dark → coral and the new token *settles* with a small squash-and-stretch, so the placement is felt as well as seen.

## 2. Color tokens

Derived from `icon.png` (slate field, cream tiles, coral and dark-slate tokens).

| Token | Hex | Use |
|---|---|---|
| `bg` | #2F4858 | Full-screen slate field (sampled from icon mid-tone) |
| `surface` | #EFEADF | Board panel, editable cell tile (icon cream) |
| `surface_alt` | #E2DACA | Fixed-clue tile (slightly darker cream) |
| `ink` | #1C2D3A | Text on cream; dark-slate token value 0 fill is `bg` |
| `ink_muted` | #4A5B66 | Captions and helper text on cream |
| `ink_on_bg` | #EFEADF | Title and helper text on slate |
| `accent` | #EA6854 | Token value 1 (coral); primary button fill |
| `accent_ink` | #14212B | Digit and label on `accent` |
| `success` | #2E6B57 | Solved status text (paired with the word "Complete") |
| `error` | #A33A2C | Contradiction status text (paired with the word "Contradiction") |
| `focus` | #EFEADF | 3 px focus ring on slate; 3 px `ink` ring on cream |

Contrast (WCAG 2.x, computed):

- `ink` on `surface`: **11.78 : 1**
- `ink_muted` on `surface`: **5.88 : 1**
- `ink` on `surface_alt`: **10.17 : 1**
- `accent_ink` on `accent`: **5.15 : 1**
- `error` on `surface`: **5.47 : 1**
- `success` on `surface`: **5.21 : 1**
- `ink_on_bg` on `bg`: **8.00 : 1**
- `ink_muted` (light, #B7C3CB) on `bg`: **5.34 : 1**

State is never color-only: a value is a filled token with a printed digit (0 or 1); a fixed clue has a cream inner ring plus a darker tile; a contradiction has text and a shake; a complete line has a glow plus the status word.

Token colors: value 0 is `bg` (#2F4858) with a cream digit (8.00 : 1); value 1 is `accent` with an `accent_ink` digit (5.15 : 1).

## 3. Typography scale

Default Godot UI font (no bundled font yet), regular weight.

| Role | Size (px) | Weight | Notes |
|---|---|---|---|
| Eyebrow (`TAKUZU LAB`) | 13 | regular | `ink_on_bg`, uppercase |
| Title | 25 | regular | `ink_on_bg` |
| Heading / puzzle line | 15 | regular | `ink` on `surface` |
| Body (rules) | 14 | regular | `ink` on `surface` |
| Caption (footer, helper) | 13 | regular | `ink_muted` |
| Button label | 14 | regular | `ink` on `surface_alt` / `accent_ink` on `accent` |
| Cell digit, 4×4 | 15 | regular | inside a 74 px cell's token |
| Cell digit, 6×6 | 12 | regular | inside a 49 px cell's token |

## 4. Spacing & layout

- Base unit: **8 px**. Used grid gaps: 6 px (board), 7 px (action rows), 17 px (column rhythm).
- Screen margin: 18 px sides, 28 px top and bottom. Content column min width 344 px fits 414 px portrait with margins; the 6×6 board (324 px) fits the panel's 10 px padding.
- Minimum touch target: **48 px**. 4×4 cells are 74 px; 6×6 cells are 49 px (≥ 48 px).
- Board sits in a cream `surface` panel with 16 px padding and 16 px corner radius.
- Layout is portrait 414×896. Landscape is not optimised for this build.

## 5. Components

- **Board panel:** cream `surface`, 16 px radius, 10 px padding, 1 px `#D5CBB8` border, shadow 6 px.
- **Board cell (editable):** cream `surface` tile, 9 px radius. Empty shows no token. A placed value shows a circular token (diameter 70% of the cell, centred).
  - Value 1: coral token, `accent_ink` digit.
  - Value 0: dark-slate token (`bg`), cream digit.
  - normal: as above. hover: tile tint `#F7F3EA`. pressed: token scale 0.92 (motion `press`). focus: 3 px `ink` ring. disabled: n/a for editable cells.
- **Clue cell (given):** tile `surface_alt`, same token colours, plus a 3 px cream inner ring on the token. Always `disabled` (not focusable for input, still read by screen readers as "Fixed clue").
- **Controls / action row:** `Undo`, `Redo`, `Hint`, `Restart` as secondary buttons (fill `surface_alt`, `ink` label, 12 px radius, 48 px tall).
  - normal: `surface_alt`. hover: `#F3EEE3`. pressed: `#D5CBB8`. focus: 3 px `focus` ring. disabled: 40% opacity `ink`.
- **Reduce-motion toggle:** `CheckButton` labelled "Reduce motion", `ink_on_bg` text, in the actions area.
- **Panels:** cream `surface` with 16 px radius and a 1 px `#D5CBB8` border plus `shadow_size` 6 px, `shadow_color` `#0F1A22` at 35%.
- **Status banner:** text line under the board. Neutral: `ink_muted`. Solved: `success` with "Complete." Contradiction: `error` with "Contradiction."

## 6. States

- **Idle:** all editable tiles `surface`, no token.
- **Selected (focused cell):** 3 px `ink` ring, tile unchanged.
- **Related / peer highlight:** not used in this build (no peer highlight is shipped).
- **Conflict:** the placed cell shakes; status text switches to `error` "Contradiction", naming the cause in words.
- **Completed group (row or column):** when the placed value fills its row or column, that line sweeps with a cream glow.
- **Solved:** all tokens sweep, then a coral and cream burst from the board centre; status "Complete."

## 7. Motion system

Shared module timings from `scripts/motion.gd` (copied, not rewritten).

| Name | Trigger | Duration (ms) | Easing | Property |
|---|---|---:|---|---|
| press | Tap a cell | 90 | back-out | `offset_transform_scale` 0.92 → 1 |
| pop | Value placed or changed | 180 | back-out | `offset_transform_scale` 1.16/0.90 → 1 (squash-stretch) |
| shake | Placement creates a contradiction | 260 | sine in-out | `offset_transform_position` x ±7 px, damped |
| sweep | Row or column becomes complete (glow `#F6B8A6`); solved (default cream glow) | 35 step, 260 each | sine out, back out | `offset_transform_scale` 1.08 → 1 and `self_modulate` glow → white |
| stagger_in | Board built, restart | 16 step (cap 420), 220 | back-out / cubic-out | scale 0.86 → 1, position y 10 → 0, alpha 0 → 1 |
| slide_swap | New puzzle opened (daily or new size) | 280 | cubic out | board x ±48 px → 0, alpha 0 → 1 |
| celebrate | Solved | ≤ 1100 (22 pieces) | cubic out | piece position, rotation, alpha; freed after |

**Signature moment:** a placed token settles with a pop (squash-stretch). Solving sweeps the board in reading order, then a restrained burst of 22 coral and cream tiles rises from the board centre and fades. No flashing.

## 8. Reduced motion

- `reduced_motion` on: every `motion` call applies the final state synchronously. No tweens are created, and no shake, sweep, stagger, slide or celebrate runs.
- The celebration burst is omitted. The status text still says "Complete."
- No flashing at any setting.
- Setting persists in `user://settings.json` as `{"reduced_motion": bool}` and loads on start.

## 9. Haptics & accessibility

- **Haptics:** none shipped. Intent: none for this build; if added later, light impact only on a contradiction, with a setting.
- **Accessibility checklist:**
  - Every cell has a screen-reader name ("Row r, column c: value") and description ("Fixed clue" or "Editable cell…").
  - Touch targets ≥ 48 px.
  - Contrast ≥ 4.5 : 1 for all text pairs above.
  - No timers.
  - Reduce-motion toggle visible in the controls.
  - Keyboard and focus ring on every control.
- **Icon & screenshot guidance:** the icon (slate field, cream tiles, coral and dark-slate tokens) is the in-game palette source. Screenshots should show a half-filled 6×6 board with a visible contradiction and the reduce-motion toggle.

## 10. QA checklist

1. Icon and in-game palette match: slate field, cream tiles, coral (1) and dark slate (0) tokens.
2. Every token shows its digit at both 4×4 and 6×6, legible at 414×896.
3. Fixed clues are distinguishable without color (inner ring and tile shade).
4. Reduce-motion on: a select + place completes at rest with no tweens running.
5. Reduce-motion off: a placed token returns to scale 1 within about 0.3 s.
6. A contradiction shakes the cell and shows the "Contradiction" text.
7. Solving sweeps the board and celebrates, then leaves no stray pieces.
8. No overflow or clipped text at 414×896 and 320 px-wide content.
