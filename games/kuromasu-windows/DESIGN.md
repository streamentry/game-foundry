# Kuromasu Windows — design system (v0.2, motion and palette upgrade)

## 1. Positioning and emotional promise

A quiet night-window puzzle: each numbered window counts what it can see, and the board
glows only where you have worked it out. No medical or cognitive outcome claims, no age
stereotypes, no timer, no streaks, no purchase prompt. The app stays offline with no account.

**Three adjectives:** nocturnal · exact · unhurried.

**Signature interaction:** a tapped cell flicks like a lamp: it overshoots, then settles.
Black and white read as shade and light, so a solved board looks like a lit window grid.

## 2. Color tokens

Palette taken from `icon.png` (deep plum ground, rose lit cells, dark shaded cells).

| Token | Hex | Use |
| --- | --- | --- |
| `bg` | `#2A0A31` | Screen background (deep plum) |
| `surface` | `#3B1442` | Unknown cells, panels, secondary controls |
| `surface_alt` | `#4F1E57` | Hover and pressed fills for unknown cells and secondary buttons |
| `shade` | `#12030F` | Black (shaded) cell fill |
| `ink` | `#FFF4F8` | Body text, clue captions, black-cell label |
| `ink_muted` | `#E3B9D2` | Eyebrow and secondary captions |
| `accent` | `#F58DB9` | White (lit) cell fill, primary button fill |
| `accent_ink` | `#2A0A31` | Text on `accent` and on clue cells |
| `clue` | `#FFD3E6` | Fixed numbered clue fill (lighter than `accent`) |
| `success` | `#9BE7C4` | Solved status and completed-board accent |
| `error` | `#FF9E8C` | Rule-violation text (always paired with a sentence) |
| `focus` | `#FFE3A3` | Keyboard and gamepad focus ring (warm gold, not used for state) |

Contrast ratios (WCAG 2.x, computed):

- `ink` on `surface`: **14.3 : 1** (passes 4.5)
- `accent_ink` on `accent`: **7.9 : 1** (passes 4.5)
- `error` on `surface`: **7.7 : 1** (passes 4.5)
- Supporting: `ink` on `bg` 16.5 : 1; `ink` on `shade` 18.7 : 1; `ink_muted` on `bg` 10.2 : 1;
  `focus` on `bg` 14.1 : 1; `accent` on `surface` 6.9 : 1 (non-text, passes 3 : 1).

State is never color-only: black shows the word **BLACK**, white shows **WHITE**, unknown shows
a dot, clues show their number, and violations show a sentence in the status line.

## 3. Typography scale (px, system sans, weight 600 for numerals and buttons)

| Role | Size | Weight |
| --- | --- | --- |
| Title (puzzle line "See around the corners.") | 28 | 700 |
| Heading (PUZZLE n OF 6 • name) | 16 | 600 |
| Body and rules | 15 | 400 |
| Status line | 15 | 600 |
| Button label | 16 | 600 |
| Cell caption (BLACK / WHITE / clue) | 21 | 600 |
| Eyebrow and footnote | 12–14 | 500 |

## 4. Spacing and layout

- Base unit 4 px. Screen gutters 16 px (left, right), 20 px top, 24 px bottom.
- Section rhythm 12 px between column items; 8 px between buttons in a row.
- Board cells 76 × 76 px with a 4 px grid gap (the 4 × 4 board is 316 px square, fits 382 px
  content width at 414 px).
- Minimum touch target 48 px; all buttons are 54 px tall; the reduce-motion toggle is 48 px.
- Scroll vertically on short screens. Never shrink targets to fit.

## 5. Components

- **Board cell (editable):** `surface` fill, 12 px radius, 1 px `ink_muted` at 30 % border.
  Normal: as listed. Hover: `surface_alt`. Pressed: `surface_alt` with 1 px darker border.
  Focus: 3 px `focus` ring, no fill change. Disabled (clue): see 6.
- **Clue cell (fixed):** `clue` fill, `accent_ink` text, 12 px radius, no pointer response.
- **Black cell:** `shade` fill, `ink` text "BLACK", 12 px radius, 1 px `accent` border.
- **White cell:** `accent` fill, `accent_ink` text "WHITE", 12 px radius.
- **Primary button (Next puzzle, Copy):** `accent` fill with `accent_ink` label; hover lifts
  the fill 6 % lighter; pressed darkens 8 %; focus 3 px `focus` ring; disabled uses `surface`
  fill with `ink_muted` label at 60 % opacity. Shadow: `shadow_size` 4, `shadow_color` black 35 %.
- **Secondary button (Undo, Restart, Previous):** `surface` fill, `ink` label, 1 px `ink_muted`
  border; hover `surface_alt`; pressed `surface_alt` with `shade` border; focus gold ring;
  disabled 50 % opacity. Radius 12 px.
- **Panels / status banner:** `surface` fill, 14 px radius, shadow 6 px at 30 %. The status
  line is inside a panel; success text uses `success`, violation text uses `error`.
- **Reduce-motion toggle:** `CheckButton` with `ink` text, 48 px tall, in the controls area.

## 6. States

| State | Visual |
| --- | --- |
| Idle unknown | `surface`, dot "·" |
| Selected / focused | gold `focus` ring 3 px |
| Related / peer highlight | not used (no peer highlight in this puzzle) |
| Black | `shade`, "BLACK" |
| White | `accent`, "WHITE" |
| Conflict | `status` text in `error` with the rule sentence; the offending cell shakes once |
| Clue (fixed) | `clue`, number |
| Completed group | not used; a completed sight-line is not a clean event in this rule set |
| Solved | status in `success`; board sweep and celebration (section 7) |

## 7. Motion system

Shared module: `scripts/motion.gd` (copied from the store-catalog template; constants unchanged).

| Name | Trigger | Duration ms | Easing | Property |
| --- | --- | --- | --- | --- |
| Lamp flick | Tap that changes a cell (unknown → black → white) | 180 | TRANS_BACK / EASE_OUT | `offset_transform_scale` 1.16 × 0.90 → 1 |
| Conflict shake | Tap that makes two blacks touch | 260 | TRANS_SINE / EASE_IN_OUT | `offset_transform_position` x, damped |
| Board intro | Launch and Restart | 220 each, 16 ms stagger, capped at 420 ms | TRANS_BACK / TRANS_CUBIC | scale 0.86 → 1, y +10 → 0, alpha 0 → 1 |
| Window slide | Next or Previous puzzle | 280 | TRANS_CUBIC / EASE_OUT | `offset_transform_position` ±48 px x, alpha |
| Solved sweep | Board solved | 35 ms stagger, 260 | TRANS_SINE / BACK | scale 1.08 then 1, self_modulate glow |
| Solved celebration | Board solved | ≤ 1100 | TRANS_CUBIC / EASE_OUT | up to 22 soft tiles from board centre, freed after |

Signature moment: the lamp flick on every tap, and on a solve the board lights up cell by cell
(sweep) with a few rose and mint tiles drifting out of the grid.

`press` is not used: a tap is a toggle, so the flick (`pop`) carries the feedback.

Game state updates before any motion starts; animations only follow.

## 8. Reduced motion

- Setting: "Reduce motion" toggle, persisted to `user://settings.json` as
  `{"reduced_motion": bool}` (the progress file validates strictly, so it is not reused).
- When on, every `motion` call applies the final state instantly: no tweens, no intro, no slide.
- Solved celebration is omitted entirely (the module returns early).
- No flashing anywhere. The sweep uses a tint change only, and the tint is also
  shown by the status text.

## 9. Haptics, accessibility, icon and screenshots

- **Haptics:** none shipped. Possible future opt-in tick on a tap; not tested.
- **Accessibility checklist:** 48 px minimum targets (54 px buttons, 76 px cells); contrast
  table above; no timers; color never the only cue; accessibility names on every cell
  ("Row r, column c, fixed/editable, state"); keyboard and gamepad focus ring on every button;
  reduce-motion toggle visible; text scales to 200 % without clipping on a 414 px screen
  (verify on device).
- **Icon:** keep the existing `icon.png`: deep plum ground, a 3 × 3 window grid with dark
  shaded corners and rose lit cells, the centre cell showing a "5". Do not recolour the icon
  to match the game; the in-game palette follows it.
- **Screenshots (store):** 1 launch board, 2 after taps showing black and white cells with a
  status message, 3 solved board with the sweep glow. Use the capture script at
  `tools/store_catalog/capture.gd`.

## 10. QA checklist

1. Each tap changes one cell and the status line matches the rules on that tap.
2. Two blacks touching produce the shake and the status sentence, and the state still updates.
3. Restart and launch run the board intro; with reduce motion on, nothing animates.
4. Next and Previous slide the board in the right direction; the state is correct immediately.
5. Reduce-motion setting survives a relaunch (`user://settings.json`).
6. Every button and cell can be reached with keyboard or gamepad focus; focus ring visible.
7. No overflow or clipped text at 320, 375, 414 px widths; no horizontal scroll.
8. Headless tests pass: rules, progress, scene and motion.
