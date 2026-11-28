# Futoshiki Terrace — visual and motion system (v0.2)

## 1. Positioning and emotional promise

A quiet, untimed inequality puzzle for adults who enjoy deduction. Each number has one place, and the
board tells you plainly when a rule is broken. Solving a terrace should feel like a neat row closing,
not like a reward machine. This game makes no claim to train or improve memory, cognition or health.

**Adjectives:** orderly · botanical · warm.
**Signature interaction:** a completed row or column is brushed gold from its first cell to its last
(the "terrace sweep"). It is the only bright moment during play; the solved board then settles into
the palette with a short burst of soft tiles.

## 2. Color tokens

The palette is drawn from the icon: its deep olive ground (`#202F03`), gold trim (`#DAB047`) and
ivory squares (`#F9EFCF`). Paper is the ivory family; accent is the icon's moss, pushed lighter so the
text on it reads well.

| Token | Hex | Use |
| --- | --- | --- |
| `bg` | `#F8F2DC` | Screen background (ivory) |
| `surface` | `#FFFBEA` | Editable cells, panels |
| `surface_alt` | `#EADFB8` | Fixed clue cells (givens) |
| `ink` | `#262E10` | Numbers, headings, body text |
| `ink_muted` | `#4F5536` | Captions, helper text, cell inequality marks |
| `accent` | `#3F5A17` | Primary buttons, selected-cell border, solved tiles |
| `accent_ink` | `#FFFFFF` | Text on `accent` |
| `pale` | `#E3E9C8` | Secondary buttons, selected cell fill |
| `gold` | `#DAB047` | Terrace sweep glow, solved burst |
| `success` | `#2F6018` | Solved status text (always with the words "solved" or a check) |
| `error` | `#9E3E40` | Conflict status text and conflict icon |
| `focus` | `#3F5A17` | 3 px focus ring on every control |

**Contrast (WCAG 2.x, computed):** `ink`/`surface` 13.67:1 · `ink`/`bg` 12.65:1 · `ink`/`surface_alt`
10.63:1 · `accent_ink`/`accent` 7.82:1 · `error`/`surface` 6.29:1 · `success`/`bg` 6.67:1 ·
`ink_muted`/`bg` 6.97:1 · `focus`/`bg` 6.97:1. All text pairs are at least 4.5:1.

**Color is never the only signal.** Selection has a 3 px border plus a fill change. A conflict shows the
word "contradicts" in the status line and shakes the cell. A solved state shows "Every inequality and
row/column is satisfied." Fixed clues are identified by their accessibility description and the
disabled state, not only by tint.

## 3. Typography scale

| Role | Size | Weight | Notes |
| --- | --- | --- | --- |
| Kicker | 12 px | regular | Uppercase, `accent` |
| Title | 27 px | bold (system) | "Every number has its place." |
| Heading (terrace name) | 16 px | bold | Board title |
| Body / instructions | 15 px | regular | Line height default |
| Button label | 17 px | semi-bold | Min height 54 px |
| Cell digit | 19 px | bold | Editable and fixed |
| Cell inequality marks | 11 px | regular | Under digit, `ink_muted` |
| Status / feedback | 14 px | regular | Minimum readable size on 414 px width |
| Helper | 13 px | regular | `ink_muted` |

Font: system default. Text scales with the system setting; no fixed-height text containers.

## 4. Spacing and layout

- Base unit 4 px. Screen margins 16 px left and right; 22 px top, 24 px bottom.
- Vertical rhythm 12 px between blocks, 8 px between buttons in a row.
- Board: 4 × 4 cells at 70 × 70 px minimum with 5 px gaps. Board fits 414 px width with margins.
- Minimum touch target 48 px (controls are 54 px high, cells 70 px).
- The whole column scrolls on short screens; no essential control is shrunk below 48 px.

## 5. Components

- **Board cell (editable):** `surface` fill, 1 px `#B5C3AC` border, radius 12 px. Digit in `ink`; a
  small `ink_muted` line below for inequality marks like `→2`.
  - normal: as above · hover: unchanged fill with border `accent` · pressed: fill `pale` · focus: 3 px
    `focus` ring · disabled (fixed): `surface_alt` fill, no border change, not tappable.
- **Board cell (selected):** `pale` fill, 3 px `accent` border.
- **Digit keypad (1 2 3 4):** primary buttons, `accent` fill, `accent_ink` text, radius 12 px,
  `shadow_size` 4 px, `shadow_color` black 12%. Hover `#2E4410`; pressed `#243610`; focus ring 3 px
  `focus`; disabled `pale` fill with `ink_muted` text.
- **Secondary buttons (Undo, Erase, Restart, Previous, Copy):** `pale` fill, `ink` text, radius 12 px,
  same shadow. Hover `#D0DEB0`; pressed `#C4D49C`; disabled `surface_alt` with `ink_muted` text.
- **Reduce motion switch:** a `CheckButton` labelled "Reduce motion", 48 px high, in the controls area.
- **Status banner (feedback line):** 14 px text, no fill. Prefix text states the result: "contradicts"
  (error colour) or "satisfied" (success colour). Never colour alone.
- **Panels:** the board sits directly on `bg` with no card; cells carry the surface treatment.
  No panel shadow is shipped.

## 6. States

- **Idle:** all editable cells `surface`; no selection highlight until a cell is tapped.
- **Selected:** `pale` fill, 3 px `accent` border; inspector lists possible digits.
- **Related / peer:** the inspector lists each inequality touching the selected cell, in text.
- **Conflict:** the entered cell shakes; status says "A repeated number or inequality contradicts the rules."
- **Completed group (row/column):** a gold sweep runs across that group; it stays in its normal fill.
- **Solved:** the sweep runs across all 16 cells, then a burst of 22 soft tiles in `accent`, `success`
  and `surface`; status says "Every inequality and row/column is satisfied."

## 7. Motion system

Timings are the shared module values in `scripts/motion.gd` (copied unchanged from the template).

| Name | Trigger | Duration (ms) | Easing | Property |
| --- | --- | --- | --- | --- |
| press | Tap a cell | 90 | BACK out | `offset_transform_scale` 0.92 → 1 |
| pop | Value placed, erased or changed | 180 | BACK out | `offset_transform_scale` 1.16/0.90 → 1 |
| shake | Entry creates a conflict | 260 | SINE in-out | `offset_transform_position` x ±7 px, damped |
| sweep | Row, column or solved board | 35 stagger / 260 | SINE out, BACK out | `offset_transform_scale` 1.08 → 1, `self_modulate` to gold → white |
| intro | Level load or restart | 16 stagger / 220 | BACK out, CUBIC out | scale 0.86 → 1, y +10 → 0, alpha 0 → 1 |
| slide | Next / Previous level | 280 | CUBIC out | board x ±48 px → 0, alpha |
| celebrate | Solved | ≤ 1100 | CUBIC out | 22 pieces, position and alpha; freed afterward |

**Signature moment:** the terrace sweep, a gold brushstroke that runs along the completed line.

## 8. Reduced motion

- Every motion becomes instant: the final state is applied at once, no tween is created.
- The celebration (burst of tiles) is omitted entirely; the sweep and shake are not played.
- Nothing flashes at any setting. Gold is a tint on the cell, shown and removed without blinking.
- The setting is saved in `user://settings.json` as `{"reduced_motion": true|false}` and applied on launch.
- The game state, status text and keyboard focus behave identically with motion on or off.

## 9. Haptics, accessibility, icon and screenshots

- **Haptics:** none shipped. No vibration on any device.
- **Sound:** none shipped.
- **Accessibility checklist:** every control has a text label and an accessibility name; cells report
  "Fixed clue" or "Editable" with their value; touch targets at least 48 px; contrast pairs listed in §2;
  reduce-motion switch visible on the main screen; no timers; no colour-only cues.
- **Icon:** existing icon (olive ground, gold trim, ivory squares with a moss less-than mark) is kept;
  the in-game palette above is drawn from it. No changes to the icon files.
- **Screenshots (in-engine only):** first board with givens and a selected cell; a completed row
  mid-sweep; the solved terrace.

## 10. QA checklist

1. Every text pair in §2 is at least 4.5:1 (recomputed when a token changes).
2. Reduce motion on: pressing, placing, conflicts, completing a line and solving all finish with the
   final state applied and no running tweens.
3. Reduce motion off: each cell returns to scale 1 and offset 0 within 0.6 s after any action.
4. Undo, Erase and Restart still work during any animation (state is never delayed).
5. Layout at 414 × 896 with no clipped text; the column scrolls rather than squeezes.
6. Focus ring visible on every button and on the reduce-motion switch when reached by keyboard or gamepad.
7. Conflict and solved states identifiable from text alone (turn colour off mentally).
8. Solving a board still unlocks the next level, and progress still saves.
