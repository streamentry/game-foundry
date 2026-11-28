# Hidato Walk — design system (v0.2, icon-harmonized)

## 1. Positioning and emotional promise

A quiet walk along a path of numbers. Every step you place has to touch the next one, so the
board reads as a trail you are laying down, not a grid you are grading. The promise is calm
agency: the rules are visible, a wrong step is named in words, nothing ticks, and finishing
the trail feels like arriving somewhere.

**Adjectives:** trail-lit · tactile · unhurried.

**Signature interaction:** on solve, the gold trail lights cell by cell in path order, 1 to 16,
so the player watches their own route get walked. Reduced motion shows the same solved board
without the sweep.

## 2. Colour tokens

Palette sampled from the icon: deep violet background `#3E026D`, gold trail `#FECB38`,
cream numerals `#FDF9D9`. The UI uses the same family, lightened for paper-like reading.

| Token | Hex | Use |
| --- | --- | --- |
| `bg` | `#FBF6E6` | Scene background (cream paper, from the numerals) |
| `surface` | `#FFFDF6` | Editable cells, status panel |
| `surface_alt` | `#E9DDF5` | Given/clue cells (lavender tint of the tile violet) |
| `ink` | `#2B0A52` | Numbers, headings, body text (deep icon violet) |
| `ink_muted` | `#5E4C7A` | Captions, disabled labels |
| `accent` | `#4B1487` | Primary buttons, eyebrow label, focus ring |
| `accent_ink` | `#FFF8E1` | Text on accent |
| `trail` | `#FECB38` | Selected cell fill and solve sweep (always with a 3 px ink border) |
| `trail_soft` | `#FFEFB0` | Editable cells once the path is solved |
| `success` | `#1E6B45` | "Complete!" status text |
| `error` | `#A3263A` | Conflict status text |
| `focus` | `#4B1487` | 3 px focus ring on every control |
| `disabled_bg` | `#E2DAEA` | Disabled buttons (Next when locked, Undo with no history) |
| `disabled_ink` | `#5E4C7A` | Disabled button text |
| `border` | `#B9A8D0` | 1 px control borders |

**Contrast (WCAG 2.x, computed):**

- `ink` on `bg` 15.3:1; `ink` on `surface` 16.3:1; `ink` on `surface_alt` 12.7:1
- `accent_ink` on `accent` 11.3:1
- `ink` on `trail` 9.3:1
- `error` on `bg` 6.7:1; `error` on `surface` 7.1:1
- `success` on `bg` 6.0:1
- `disabled_ink` on `disabled_bg` 5.5:1
- `ink_muted` on `bg` 7.0:1

**Colour is never the only cue.** Selected = gold fill + 3 px ink border. Given = lavender
fill + 2 px ink border + disabled (not tappable). Conflict = red text sentence naming the
numbers. Solved = "Complete!" text, not just the colour change.

## 3. Typography (px, system font)

| Role | Size | Weight | Notes |
| --- | --- | --- | --- |
| Eyebrow | 12 | regular | accent colour, uppercase label |
| Headline | 27 | regular | "Every step touches the next." |
| Heading (puzzle name) | 16 | regular | "PUZZLE n OF 6 • name" |
| Body / status | 15 | regular | instructions, progress, status |
| Button label | 17 | regular | keypad, actions |
| Cell digit | 23 | regular | number inside a 71 px square |
| Caption / help | 13 | regular | footer rule note |

No bold face is shipped; hierarchy comes from scale and colour. Text reflows (autowrap) and the
screen scrolls rather than shrinking type.

## 4. Spacing and layout

- Base unit 4 px. Screen margins 17 px left/right, 21 px top, 22 px bottom.
- Column separation 12 px; action row and keypad gaps 7–8 px; grid gutter 3 px.
- Grid cells 71 × 71 px (4 × 4 board). Keypad buttons and action buttons are at least
  54 px tall, and full width of their column. **Minimum touch target 48 px**, met everywhere.
- Layout tested at 414 × 896 (portrait). Vertical scroll is the overflow strategy; nothing
  overlaps or clips horizontally.

## 5. Components

- **Board cell (editable):** `surface` fill, 1 px `border`, radius 12. Hover lifts shadow
  (2 → 3 px) and lightens fill 5 %. Pressed darkens 8 % and drops shadow. Focus = 3 px
  `focus` ring, transparent fill. Empty shows a middle dot `·`.
- **Given / clue cell:** `surface_alt` fill, 2 px `ink` border, radius 12, always disabled
  (not tappable), same disabled look in all states.
- **Keypad (1–16) buttons:** primary style: `accent` fill, `accent_ink` text, radius 10.
- **Secondary buttons (Undo, Erase, Restart, Previous, Copy challenge):** `surface_alt` fill,
  1 px `border`, `ink` text, radius 10.
- **Primary navigation (Next puzzle):** primary style when available; `disabled_bg`/
  `disabled_ink` when locked.
- **Button states:** normal, hover (fill +5 %, shadow +1), pressed (fill −8 %, no shadow),
  focus (3 px `focus` ring), disabled (`disabled_bg`, `disabled_ink`, no shadow).
- **Reduce-motion control:** `CheckButton` labelled "Reduce motion", 16 px, 48 px tall, below
  the navigation row.
- **Panels / status banner:** the status line under the grid, 15 px. Text colour: `success`
  when solved, `error` on conflict, `ink` otherwise.

## 6. States

| State | Look | Non-colour cue |
| --- | --- | --- |
| Idle editable | `surface`, 1 px border | empty dot or number |
| Selected | `trail` fill, 3 px ink border | border thickness |
| Related / peer highlight | **not shipped**: the status sentence names the conflicting pair instead | text |
| Conflict | status text in `error`; the entered cell shakes once | sentence naming the numbers |
| Completed group | not used. Completion is judged on the whole path | — |
| Solved | non-given cells `trail_soft`, status "Complete!" in `success`; solve sweep plays once | text |
| Given | `surface_alt`, 2 px border, disabled | border + disabled |

## 7. Motion system

Shared module timings from `tools/store_catalog/templates/motion.gd`.

| Name | Trigger | Duration ms | Easing | Property |
| --- | --- | --- | --- | --- |
| press | select an editable cell | 90 | back, ease-out | `offset_transform_scale` 0.92 → 1 |
| pop | place / erase a number | 180 | back, ease-out | `offset_transform_scale` 1.16×0.90 → 1 |
| shake | conflict after placing | 260 | sine, ease-in-out | `offset_transform_position.x` damped ±7 px |
| intro | level load and Restart | 220 (16 ms stagger, max 420) | back / cubic | scale 0.86 → 1, y 10 → 0, alpha |
| slide | Next / Previous puzzle | 280 | cubic, ease-out | board x ±48 px, alpha |
| sweep | solved (path order 1 → 16) | 35 ms stagger over 260 | sine / back | scale 1.08 → 1, `self_modulate` to `trail` and back |
| celebrate | solved | ≤ 1100 | cubic, ease-out | up to 22 soft pieces (ink-violet, gold, success), freed after |

**Signature moment:** the sweep follows path order (the cell holding 1, then 2 … 16), not
reading order. This is the only place the path itself is animated.

## 8. Reduced motion

A visible "Reduce motion" toggle persists in `user://settings.json` as
`{"reduced_motion": bool}` and sets `motion.reduced_motion` on load and on change.
With it on, every motion call applies its final state synchronously: no press, pop, shake,
intro, slide, sweep, or celebration pieces. Solved state stays visible through `trail_soft`
and "Complete!" text. Nothing flashes at any setting.

## 9. Haptics, accessibility, icon and screenshots

- **Haptics:** none shipped, no intent to add in this version.
- **Accessibility:** every cell has an accessibility name ("Row r, column c, n or empty");
  every control is focusable; text is 13 px or larger; all touch targets ≥ 48 px; conflict and
  completion are announced as text. Real VoiceOver, Dynamic Type and 200 % scaling are **not
  verified** on device.
- **Icon:** keep the existing violet tile icon; this palette was matched to it, so no icon
  change is needed.
- **Store screenshots:** launch board (intro settled), a selected cell with the keypad, a
  conflict sentence, and the solved board with "Complete!".

## 10. QA checklist

1. Each colour pair in §2 stays at or above 4.5:1; re-run the contrast list if a hex changes.
2. Every state in §6 is recognisable with the colour removed (greyscale check).
3. Tap a cell, place a number, undo, restart: state updates before any animation ends.
4. Solve board 1 by taps: sweep plays in path order and ends at rest; celebration frees itself.
5. Reduce motion on: no tween starts (`Motion.running` == 0) and the solved board looks the same.
6. Next / Previous slide in and out; locked Next shows the disabled style.
7. Layout at 414 × 896 and a smaller phone width: no clipped number, no button under 48 px.
8. Real-device check on iOS and Android before any claim about comfort or readability.
