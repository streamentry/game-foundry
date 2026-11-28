# Kropki Pairs — design spec (v0.2, upgrade)

## 1. Positioning and emotional promise

Kropki Pairs is a quiet logic board for a player who likes exact rules. Each dot is a small
sentence about two neighbouring squares, and a solved row feels like a sentence that finally
reads right. It makes no health, memory or cognitive claims, has no timer and no streak.

**Three adjectives:** exact · calm · legible.

**Signature interaction:** a digit placed in a square gives a short squash-and-settle, and a row
or column that becomes a valid full Latin line is "sealed" by a gold light that travels across
its four squares in reading order. The seal is the only celebratory cue until the solve.

## 2. Color tokens

Derived from the icon: navy background `#242949`, slate tile `#4D547F`, gold dot `#EABB3A`.
The gold is too light for text on paper, so the focus ring uses a deepened gold.

| Token | Hex | Use |
| --- | --- | --- |
| `bg` | `#F3F4F8` | Page background (cool paper) |
| `surface` | `#FFFFFF` | Editable cells, keypad keys on white |
| `surface_alt` | `#E3E7F2` | Given (fixed) cells, secondary buttons |
| `ink` | `#242949` | Clues, body text, labels (icon navy) |
| `ink_muted` | `#4A5170` | Captions and helper text |
| `accent` | `#4D547F` | Primary buttons, selected-cell border, placed-digit marks |
| `accent_ink` | `#FFFFFF` | Text on accent |
| `success` | `#1E6B55` | "Sealed" and "Solved" status text (always with words) |
| `error` | `#A3322E` | Rule-conflict status text (always with words and shake) |
| `focus` | `#9A6A00` | 3 px focus ring (deepened gold; the icon's `#EABB3A` fails 3:1 on paper) |

**Contrast (WCAG 2.2, computed):**

- ink / surface **14.11:1**; ink / surface_alt 11.41:1 (both ≥ 4.5:1)
- accent_ink / accent **7.27:1** (≥ 4.5:1)
- error / surface **6.88:1** (≥ 4.5:1)
- success / surface 6.38:1; ink_muted / surface_alt 6.28:1
- focus / bg **4.31:1**, focus / surface 4.73:1 (non-text, ≥ 3:1)

**State is never color-only.** Conflict = red text "Repeated number..." + shake + outline.
Sealed = gold glow + the word "sealed" in the status line. Given = `surface_alt` fill,
the digit is never editable (disabled) and the caption says "fixed".

## 3. Typography scale (system font, px)

| Role | Size | Weight | Notes |
| --- | --- | --- | --- |
| Title | 28 | regular (system) | Screen title "Numbers in conversation." |
| Heading | 16 | regular | Puzzle line, buttons |
| Body | 15 | regular | Rules and status |
| Caption | 12–14 | regular | Eyebrow label 12, helper text 13–14 |
| Cell digit | 17 | regular | Editable and given digits; caption line 14 |

No custom font is shipped, so weight is carried by size and colour, not by bold faces.

## 4. Spacing and layout

- Base unit 4 px. Screen gutters 16 px. Section rhythm 12 px.
- Board cells 76 px square, grid gap 4 px. Keypad gap 7 px.
- Safe margins: 20 px top, 24 px bottom inside a scroll container; layout must fit 414×896.
- Minimum touch target 48 px; every button is 54 px tall.

## 5. Components

- **Board cell (editable):** `surface` fill, 1 px `#B5C2BE` border, radius 10. Selected: 3 px
  `accent` border and a 4 px soft shadow.
- **Given cell:** `surface_alt` fill, disabled but with full-contrast ink text.
- **Keypad (1–4):** primary style, 54 px. Erase and Restart use secondary style.
- **Primary button:** `accent` fill, `accent_ink` text, radius 12, shadow 4 px at 25 % `ink`.
  - normal: `accent`; hover: `accent` darkened 6 %; pressed: darkened 12 %, shadow 0;
  - focus: 3 px `focus` ring; disabled: `accent` at 40 % alpha (text stays readable).
- **Secondary button:** `surface_alt` fill, `ink` text, radius 12. Same state pattern with
  lightened/darkened fills.
- **Reduce-motion toggle:** `CheckButton` labelled "Reduce motion", 48 px tall, in the
  controls area under the action row.
- **Panels:** `surface` cards with radius 14 are not used; the layout stays on `bg`.
- **Status banner:** single 15 px line under the board; text carries the meaning.

## 6. States

- **Idle:** no selection; first editable cell is selected at load.
- **Selected:** accent border 3 px + shadow; inspector names the dots touching the square.
- **Related / peer:** dot-neighbour squares get the inspector text only (no colour change).
- **Conflict:** the placed cell shakes; status line "Repeated number or broken W/B dot rule."
- **Completed group:** a full valid row or column is "sealed" (gold wave, status says "sealed").
- **Solved:** all groups sealed, grid celebration, status "Solved!".

## 7. Motion system

| Name | Trigger | Duration | Easing | Property |
| --- | --- | --- | --- | --- |
| press | tap an editable square | 90 ms | TRANS_BACK / EASE_OUT | offset_transform_scale 0.92 → 1 |
| pop | digit placed, erased | 180 ms | TRANS_BACK / EASE_OUT | offset_transform_scale 1.16×0.90 → 1 |
| shake | rule conflict after placing | 260 ms | SINE / EASE_IN_OUT | offset_transform_position x |
| sweep | row/column becomes a valid full line | 35 ms stagger, 260 ms | SINE / BACK | offset_transform_scale 1.08, self_modulate gold |
| intro | level load, restart | 16 ms stagger, 220 ms | BACK / CUBIC | scale 0.86 → 1, y +10 → 0, alpha |
| slide | next / previous puzzle | 280 ms | CUBIC / EASE_OUT | offset_transform_position x ±48, alpha |
| celebrate | solved | ≤ 1100 ms, 22 pieces | CUBIC / EASE_OUT | position, rotation, alpha |

**Signature moment ("seal"):** when a row or column first becomes a valid full line, its four
squares receive the sweep in reading order, lit gold, then settle. Solved adds the celebration
burst from the board centre and a sweep over all cells.

## 8. Reduced motion

Every call becomes instant: the final resting values are applied synchronously and no tween
is created. The celebration is omitted completely. Nothing flashes at any setting. The choice
is saved to `user://settings.json` as `{"reduced_motion": bool}` and applied on load and on toggle.

## 9. Haptics and accessibility

- **Haptics:** none shipped. Intent: a single light tap on conflict only, opt-in, if a real
  device test shows it helps; never on every placement.
- **Accessibility checklist:** 48 px+ targets; every cell has a row/column accessibility label
  with its fixed/editable state and caption; status changes are text; reduce-motion toggle is
  reachable and labelled; text is legible at 414×896.
- **Icon and screenshots:** keep the navy/slate/gold family. Screenshots: first board with the
  W/B captions visible, a sealed row, and the solved grid. Capture is done by
  `tools/store_catalog/capture.gd`.

## 10. QA checklist

1. Rules tests (`test_rules.gd`) and progress tests still pass; game logic is unchanged.
2. Scene test passes with motion enabled and with reduced motion on.
3. `test_motion.gd`: reduced motion leaves no running tweens and rest scale; motion on returns to rest within 0.6 s.
4. Every cell and button is ≥ 48 px and has a visible focus ring.
5. Conflict is shown by text as well as colour, and the shake ends at rest.
6. Sealed rows and columns are announced in the status text, not only by the gold wave.
7. Screenshots at 414×896 show no clipped text and no overlap at the scroll top.
8. Reduce-motion setting survives an app restart.
