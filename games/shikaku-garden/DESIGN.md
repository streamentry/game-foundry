# Shikaku Garden — design system (v0.2, motion and accessibility pass)

## 1. Positioning and emotional promise

A quiet spatial puzzle where each number is given its own rectangular bed in a walled garden.
The promise: the rules are fair, the board is calm, and every accepted bed visibly settles into
place. No timer, no streaks, no health or cognitive-benefit claims, and no "senior" styling.

**Three adjectives:** botanical · methodical · comfortable.
**Signature interaction:** two taps on opposite corners lay a bed. When a bed is accepted, its
cells lift and glow in reading order, like light crossing freshly turned soil.

## 2. Color tokens

| Token | Hex | Use |
| --- | --- | --- |
| `bg` | `#F7F7EF` | Page (warm ivory), full-screen backdrop |
| `surface` | `#FFFFFF` | Unfilled board cells |
| `surface_alt` | `#E5EDDC` | Neutral buttons (Undo, Restart, Previous) at rest |
| `surface_alt_hover` | `#D5E1CE` | Neutral button hover |
| `surface_alt_pressed` | `#C4D6BA` | Neutral button pressed (darker than hover) |
| `ink` | `#293C36` | Headline, clue numbers, cell text, status text |
| `ink_muted` | `#54665E` | Instructions, captions, disabled text |
| `accent` | `#277A65` | Primary button, selected first corner, kicker text |
| `accent_ink` | `#FFFFFF` | Text on `accent` |
| `accent_hover` | `#185C4B` | Primary button hover and pressed |
| `success` | `#2D6A4F` | Completion confirmation text (always with words) |
| `error` | `#9E3E40` | Invalid-move status text (always with words) |
| `focus` | `#293C36` | 3 px focus ring on board cells; buttons use `accent` ring |
| `grid_stroke` | `#BDCFC1` | 1 px boundary of unfilled cells |
| `glow` | `#FFF1B8` | Sweep tint on an accepted bed (self-modulate, transient) |
| `bed_1..8` | `#D1E6CA`, `#F0D4AC`, `#D2E3ED`, `#ECD4D8`, `#DCCBEA`, `#F0E6B5`, `#C6E7DF`, `#E2DBCF` | Region fills, cycled by placement index |

**Contrast (WCAG 2.x, computed):**

| Pair | Ratio | Result |
| --- | --- | --- |
| ink on surface | 11.71:1 | pass |
| ink on bg | 10.87:1 | pass |
| ink_muted on bg | 5.67:1 | pass |
| ink_muted on surface_alt | 5.08:1 | pass |
| accent_ink on accent | 5.17:1 | pass |
| accent_ink on accent_hover | 7.86:1 | pass |
| error on surface | 6.53:1 | pass |
| success on surface | 6.39:1 | pass |
| ink on every bed fill | 7.68–9.30:1 | pass |

State is never color-only: a selected corner is white text on green with a 3 px border; an
accepted bed is announced in text; an invalid move is announced in text beside the board; a
completed garden is a sentence plus the progress line.

## 3. Typography scale (px, system face)

| Role | Size | Weight | Use |
| --- | --- | --- | --- |
| Kicker | 12 | regular, `accent` | "SHIKAKU GARDEN / A QUIET LOGIC PUZZLE" |
| Title | 27 | bold (system) | "Make room for every number." |
| Heading | 16 | regular | Garden name, size |
| Body | 15 | regular | Instructions, status message |
| Button | 16 | medium | All buttons, min height 54 |
| Caption | 13 | regular, `ink_muted` | Footer note |
| Cell digit | 21 (4×4, 5×5) / 19 (6×6) | bold | Clue numbers |

## 4. Spacing and layout

- Base unit 4 px. Horizontal margin 18 px, top/bottom 22 px. Vertical rhythm 12 px.
- Grid gap 4 px. Button gap 8 px. Controls stacked full-width below the board.
- Cell size: 70 px (4×4), 61 px (5×5), 52 px (6×6). All ≥ the 48 px minimum touch target.
- Layout target 414×896 (portrait phone). The board is centred; a vertical scroll absorbs
  compact heights. No horizontal scroll.
- Safe margins: content keeps 18 px from the screen edge.

## 5. Components

- **Board cell:** 7 px corner radius. Unfilled: `surface` fill, 1 px `grid_stroke`. Filled:
  bed colour, 1 px `grid_stroke`. Selected: `accent` fill, `accent_ink` text, 3 px `accent`
  border. Focus: 3 px `focus` ring, inset.
- **Clue cell:** same as board cell; the number is the only text, bold, ink.
- **Primary button (Next garden, Copy this challenge):** `accent` fill, 12 px radius, 2 px
  shadow in `shadow_color` black at 12%. Normal `accent`; hover and pressed `accent_hover`;
  focus 3 px `accent` ring with transparent fill; disabled `surface_alt` fill with `ink_muted`
  text, no shadow.
- **Secondary button (Undo, Restart, Previous):** `surface_alt` fill, 12 px radius, 2 px
  shadow. Normal `surface_alt`; hover `surface_alt_hover`; pressed `surface_alt_pressed`;
  focus 3 px `accent` ring; disabled `surface_alt` at 55% alpha with `ink_muted` text.
- **Reduce motion toggle:** a `CheckButton` labelled "Reduce motion", 48 px minimum height,
  `ink` text, placed under the controls.
- **Status banner:** `ink` text on `bg`, 15 px, one line of current feedback. Errors use the
  words "does not"/"overlaps" so the cue reads without colour.

## 6. States

| State | Visual | Text cue |
| --- | --- | --- |
| Idle | white cell, grid stroke | none |
| Selected (first corner) | `accent` fill, white digit, 3 px border | "First corner selected…" |
| Related / peer | no peer highlight in v0.2 (intentionally not added) | — |
| Conflict (invalid bed) | cell shakes; status text names the rule | "That area overlaps…", "Every rectangle needs exactly one number.", etc. |
| Covered bed | bed colour fill | progress line "N of M squares filled" |
| Completed bed | sweep glow across the bed's cells | "A good rectangle…" |
| Solved garden | sweep across all cells, celebration burst, status line | "Beautifully done. Every square is covered." |

## 7. Motion system

Shared timings come from `scripts/motion.gd`. Only visual offsets and modulate are animated; the
game state updates before any motion starts.

| Name | Trigger | Duration ms | Easing | Property |
| --- | --- | --- | --- | --- |
| press | select a corner | 90 | back-out | `offset_transform_scale` 0.92 → 1 |
| pop | undo removes a bed (its cells) | 180 | back-out | `offset_transform_scale` overshoot → 1 |
| shake | invalid rectangle | 260 | sine in-out | `offset_transform_position` x, damped |
| sweep | accepted bed (cells in reading order, 35 ms stagger) | 260 | sine/back-out | scale 1.08 → 1; `self_modulate` → `glow` → white |
| intro | level load and restart (16 ms stagger, capped at 420) | 220 | back/cubic out | scale, position, alpha |
| slide | next/previous garden | 280 | cubic out | board `offset_transform_position` x ±48, alpha |
| celebrate | garden solved | ≤ 1100 | cubic out | ≤ 22 soft tiles, freed after |

**Game-specific signature:** the sweep. The accepted bed's cells rise and glow in reading order,
so the player sees exactly which rectangle the rules took. The solved garden repeats it across the
full board, then the restrained celebration plays.

## 8. Reduced motion

When "Reduce motion" is on (persisted in `user://settings.json` as `{"reduced_motion": bool}`):
every call in `motion.gd` applies its final state synchronously; intro, slide and sweep are
instant; the celebration is omitted entirely; no flashing or tint pulses are used. The setting
is read on launch and applied the moment the toggle changes.

## 9. Haptics and accessibility

- **Haptics:** none shipped. Any future haptic must be tied to the same events and be optional.
- **Sound:** none shipped.
- **Accessibility checklist:**
  - touch targets ≥ 48 px; two-tap corners instead of drag precision;
  - every cell has an accessibility name: row, column, clue, covered/uncovered;
  - status and errors are text, not colour only;
  - reduced motion toggle visible and persistent;
  - text stays legible at 414×896 and with larger system fonts (buttons wrap rather than clip);
  - real VoiceOver and TalkBack testing and a target-age (40+) playtest remain **unverified**.

## 10. Icon and screenshots

- **Icon:** deep-green garden boundary with three pastel beds and one dark-green number in each.
  The in-game palette uses the same family: greens, warm yellow and cream on ivory. Keep the icon
  unchanged (it is not edited by this pass).
- **Screenshots (future, real device):** an untouched 4×4 garden; first correct bed; a 6×6
  intermediate board; a completed garden; the controls on a narrow phone.

## 11. QA checklist

1. Every shipped garden solves with the reference solution; `test_rules.gd` and `test_scene.gd` pass.
2. Reduced motion on: placing, undoing and changing garden create no tweens (`Motion.running == 0`).
3. Reduced motion off: a placed bed returns to rest within 0.6 s; no cell is left scaled or shifted.
4. Button focus ring is visible with a gamepad or keyboard; no hover-only state.
5. The 414×896 layout has no clipped text and no horizontal scroll; the 6×6 board fits.
6. Contrast of every text token pair stays ≥ 4.5:1 after any colour change.
7. Motion never exceeds three flashes per second and never animates layout-bound properties.
8. Each palette token above is the only colour source; no inline colours are added outside tokens.
