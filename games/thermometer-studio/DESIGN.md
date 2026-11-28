# Thermometer Studio — design system (v0.2, icon-matched)

## 1. Positioning and emotional promise

Thermometer Studio is a quiet lab bench for balancing liquid. Each glass tube fills from its round bulb toward its tip, and the row and column totals tell you how much liquid the whole rack holds. The promise: calm, exact reasoning with a warm, tactile result. No timer, no streak, no health or brain-training claim.

**Three adjectives:** laboratory · warm · exact.

**Signature interaction:** the mercury rises inside the glass. A tap starts a 160 ms column of pink liquid from the bulb, so the player sees the fill grow along a continuous tube instead of reading a label.

## 2. Color tokens

The palette is taken from the icon: deep red field, cream glass, rose-pink liquid.

| Token | Hex | Use |
| --- | --- | --- |
| `bg` | `#6B1D2B` | Screen background (deep red, the icon's field family) |
| `surface` | `#FBF4E4` | Glass tube body, status card, secondary buttons (cream, icon thermometers) |
| `surface_alt` | `#F2E3C8` | Empty tube interior, secondary-button pressed state |
| `ink` | `#3A0E18` | Text on cream and on pink; tube outline |
| `ink_muted` | `#6B3F4A` | Secondary help text on cream only |
| `accent` | `#F08CA0` | Mercury (liquid) fill, primary button (rose pink from the icon) |
| `accent_ink` | `#3A0E18` | Text on primary button |
| `success` | `#2E6B4F` | Solved heading on the cream status card |
| `error` | `#9B1C2C` | Conflict message on the cream status card |
| `focus` | `#FFD27A` | Keyboard focus ring (gold, reads on both red and cream) |
| `text_on_bg` | `#FBF4E4` | Headings and counters on the red field |

**Contrast (WCAG 2.x, computed):**

- `ink` on `surface`: **15.26:1** (pass)
- `ink_muted` on `surface`: **7.87:1** (pass)
- `accent_ink` on `accent`: **7.14:1** (pass)
- `error` on `surface`: **7.38:1** (pass)
- `success` on `surface`: **5.75:1** (pass)
- `text_on_bg` on `bg`: **10.44:1** (pass)
- `focus` on `bg`: **8.04:1** (non-text, passes 3:1)

Mercury `accent` on cream `surface` is only 2.14:1, so the glass outline (`ink`, 2 px) carries the boundary and the fill height is the shape cue. Every state is also stated in text: "filled" or "empty" in the cell's accessibility label, "3/3 OK" on matched counters, and the status sentence. Color is never the only signal.

## 3. Typography scale

System font, two weights only (regular and bold where the platform offers them).

| Role | Size (px) | Weight | Notes |
| --- | --- | --- | --- |
| Kicker (eyebrow) | 12 | bold | Uppercase, `accent` on `bg` is not used; cream on red |
| Title | 29 | bold | "Fill from the bulb." |
| Heading (experiment line) | 16 | bold | "EXPERIMENT 1 OF 6 · Copper Glass" |
| Body / rule text | 15 | regular | Intro sentence, status |
| Button label | 16 | bold | Minimum 54 px tall |
| Counter (row/col totals) | 14 | bold | Fits 65 × 65 cell, "3/3 OK" |
| Caption / help | 13–14 | regular | Control hints, muted on cream |

No text inside the tube shapes. The thermometer is drawn, not spelled.

## 4. Spacing and layout

- **Base unit:** 4 px.
- **Screen margins:** 17 px horizontal, 22 px top and bottom; the column scrolls vertically rather than shrinking type.
- **Vertical rhythm:** 11 px between blocks.
- **Board:** 5 × 5 grid of 65 px cells (4 × 4 puzzle plus clue row and column), 4 px gutters. Board width 341 px, centred; fits 414 px width with margins.
- **Touch targets:** cells 65 × 65 px; buttons 54 px tall; all ≥ 48 px.
- **Safe margins:** content respects device insets on the 414 × 896 reference; no critical control sits in a corner.

## 5. Components

**Board cell (hit area).** Transparent `Button`, 65 × 65 px. Normal, hover and pressed styles are all transparent so the drawn tube shows through. Focus: 3 px rounded `focus` ring, radius 10. Accessibility name: "Row, col, tube, bulb or tip, filled or empty".

**Thermometer tube (drawn).** One `Control` per tube spanning its two cells. Drawn as a continuous shape: round bulb (radius 26 px) at the first cell's centre, glass body (half-width 14 px) running to the tip cell's far edge, `ink` outline 2 px, `surface` body in both empty and filled states. Filled liquid is a pink capsule from the bulb to the fill height, with a small highlight dot on the bulb, matching the icon.

**Count label.** Cream text on `bg`, 65 × 65 px, centred. Matched totals read "3/3 OK" in `focus` gold; unmatched read "2/3".

**Primary button ("Next studio").** Fill `accent`, text `accent_ink`, radius 12.
- normal `#F08CA0` · hover `#F7A6B6` · pressed `#E07A90` · focus gold 3 px ring · disabled fill `accent` at 40 % over `bg`, text `ink` at 60 %.

**Secondary button (Undo, Restart, Previous, Copy).** Fill `surface`, text `ink`, radius 12.
- normal `#FBF4E4` · hover `#FFF9EE` · pressed `#F2E3C8` · focus gold 3 px ring · disabled `surface` at 45 %.

**Panels.** One cream status card holds the feedback sentence and progress line (radius 14, shadow size 4 px, `ink` text). The rest of the screen is flat `bg`.

**Status banner.** Cream card, 15 px text. Conflict messages begin "Too much liquid in…" in `error`; solved begins "All thermometers…" in `success`.

## 6. States

| State | Visual | Text equivalent |
| --- | --- | --- |
| Idle | Glass empty (cream body, no liquid) | Cell label "empty" |
| Selected / focused | Gold 3 px ring on the cell | Focus order |
| Filled | Pink liquid rises to segment | Cell label "filled" |
| Related / peer highlight | Not shipped | — |
| Conflict | Tapped tube shakes; status shows "Too much liquid in row N" | Status card |
| Completed group | Matching counter becomes "3/3 OK" and glows once | Counter text |
| Solved | Whole rack sweeps once, a short burst of pieces, status "All thermometers…" | Status card, heading |

## 7. Motion system

Shared module timings (`scripts/motion.gd`, unchanged): press 90 ms, pop 180 ms, shake 260 ms, sweep 35 ms stagger over 260 ms, intro 16 ms stagger over 220 ms, slide 280 ms, celebrate ≤ 1100 ms.

| Name | Trigger | Duration (ms) | Easing | Property |
| --- | --- | --- | --- | --- |
| Press | Tap a cell (on its tube) | 90 | back, ease-out | `offset_transform_scale` 0.92 → 1 |
| Pop | Value placed or removed | 180 | back, ease-out | `offset_transform_scale` squash/stretch → 1 |
| **Mercury rise (signature)** | Fill level changes | 160 | cubic, ease-out | liquid top, tween of fill amount in the tube's own draw |
| Shake | Tap produces "Too much liquid" | 260 | sine, ease-in-out | `offset_transform_position` x, damped |
| Stagger in | Level load or restart | 16 per tube, 220 each | back / cubic | scale, position, alpha |
| Sweep | A row or column newly matches, or solved | 35 stagger, 260 | sine / back | scale and `self_modulate` on the group's counter |
| Slide | Next / previous level | 280 | cubic, ease-out | board position and alpha |
| Celebrate | Solved | ≤ 1100 | cubic, ease-out | 22 small soft pieces, freed afterwards |

Game state always updates immediately; motion only follows. Input is never locked by an animation.

## 8. Reduced motion

Reduced motion is a visible toggle, "Reduce motion", in the controls area, saved to `user://settings.json`.
- Every motion call becomes instant: the final state is applied at once.
- Mercury snaps to its new level with no rise.
- Celebration pieces are not created; the solved state is conveyed by text and the status card.
- No flashing at any setting. Nothing blinks or strobes; the photosensitivity guidance (WCAG 2.3.1) is met by design.

## 9. Haptics, accessibility, icon and screenshots

**Haptics:** none shipped and none planned for v0.2. Any future haptic must be optional and short.

**Accessibility checklist**
- [ ] Every cell has a label with row, column, tube, bulb or tip, and filled or empty.
- [ ] No color-only state: each state has text or shape (fill height, "OK", status sentence).
- [ ] Contrast ratios above are verified for every text pair in the table.
- [ ] Targets ≥ 48 px; the board is reachable by keyboard and gamepad focus.
- [ ] Reduce motion toggle is visible and persists.
- [ ] Layout survives 200 % text scale by scrolling, not clipping (verify on device).

**Icon guidance (already shipped):** three cream glass thermometers, rose-pink bulbs and liquid, deep red gradient field. Keep the same proportions for any store art. Use no copied competitor art, no text on the icon.

**Screenshot guidance:** the 4 × 4 rack with partial fill, a matched counter ("3/3 OK"), a conflict message with the shaking tube, and the solved rack. Capture at 1242 × 2688 and 1290 × 2796.

## 10. QA checklist

1. Board, labels and status fit 414 × 896 with no clipped text.
2. Each tap fills or empties the intended segment; undo and restart restore state.
3. Mercury rises in about 160 ms and ends exactly at its fill level.
4. Reduce motion on: no tween runs, final state is correct immediately (test `tests/test_motion.gd`).
5. Keyboard focus ring is visible on every cell and button in turn.
6. Contrast of status text and counters checked on a real phone in daylight.
7. Solving completes the puzzle state before any celebration starts.
