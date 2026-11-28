# Odd Bloom — visual and motion design (v0.2)

## 1. Positioning and emotional promise

A quiet garden on a late afternoon: one flower in the bed stands out, and noticing it feels
like a small, private pleasure. The game never rushes, never judges, and celebrates a found
flower with a brief petal flourish, then gets out of the way.

- **Three adjectives:** botanical · unhurried · warm.
- **Signature interaction:** tap a flower; the chosen bloom dips and settles, a wrong bloom
  gives a short head-shake while its text cue says "matches the others", and a found bloom
  lifts its tile and releases a few soft petal pieces (skipped under reduced motion).

The palette is taken from the app icon (plum field, cream tiles, gold petals, coral and brick
accents) so that the in-game world and the store icon read as one garden.

## 2. Color tokens

| Token | Hex | Use |
| --- | --- | --- |
| `bg` | `#FBF3E6` | Page background (cream, from icon tiles) |
| `surface` | `#FFFDF8` | Flower tiles and raised panels |
| `surface_alt` | `#F4E6D3` | Secondary buttons, disabled fill, progress band |
| `ink` | `#3A1B2B` | Headings, body text, bloom outlines (deep plum) |
| `ink_muted` | `#6A4D5C` | Explanations, captions, disabled text |
| `accent` | `#8A2F57` | Primary button, eyebrow label, focus ring (plum from icon field) |
| `accent_ink` | `#FFFFFF` | Text on accent |
| `success` | `#2E6B4E` | Found check mark and "found" message (always with text and check) |
| `error` | `#A3332B` | Wrong mark (cross) and error message (always with text and cross) |
| `focus` | `#8A2F57` | 3 px keyboard/gamepad focus ring |
| `petal` | `#EDC46E` | Bloom petals (from icon) — non-semantic drawing color |
| `petal_pale` | `#F7E3B3` | Alternate petal style, sweep glow |
| `center` | `#B4603B` | Bloom centre (brick, from icon) — non-semantic |
| `leaf` | `#5E7D4F` | Stem and leaves (olive) — non-semantic |

**Contrast (WCAG 2.x, computed):**

- `ink` on `surface`: **15.06:1**; `ink` on `bg`: 13.90:1
- `ink_muted` on `surface`: 7.30:1; on `surface_alt`: 6.05:1
- `accent_ink` on `accent`: **8.00:1** (hover `#6E2446` with white: 10.48:1)
- `error` on `surface`: **6.74:1**
- `success` on `surface`: 6.20:1
- `focus` on `bg`: 7.26:1 (non-text ≥ 3:1 satisfied)

State is never color-only: a found bloom shows a check mark and the words "You found it";
a wrong bloom shows a cross and the words "matches the others"; the odd bloom is defined by
structure (petal count, ring, leaves, style, centre mark), never by hue alone.

## 3. Typography scale

System font (no bundled fonts; two-family limit respected).

| Role | Size px | Weight | Use |
| --- | --- | --- | --- |
| Title | 29 | bold | "One flower is different." |
| Heading | 18 | semibold | Pattern name line |
| Body / action | 16 | regular / semibold on buttons | Instructions, status, buttons |
| Caption / progress | 15 | regular | Attempt count, progress line |
| Eyebrow | 12 | semibold, letter-spaced caps | "ODD BLOOM / A SIMPLE GAME OF NOTICING" |
| Cell digit | 14 | semibold | Index on each tile |

## 4. Spacing and layout

- Base unit 6 px; outer margin 18 px sides, 22 px top and bottom; vertical rhythm 12 px.
- Grid gutter 7 px. Tiles: 103 px (3 columns) or 76 px (4 columns) on a 414 pt wide screen.
- Corner radius: tiles 13 px, buttons 14 px.
- Minimum touch target **48 px**; buttons 54 px tall; tiles at least 76 px.
- The column scrolls vertically on compact screens; horizontal scroll is disabled.
- Safe reading width: full width minus margins, no text wider than the column.

## 5. Components

- **Flower tile (`BloomCard`):** white `surface` panel, 2 px `#DDC8C6` border, 13 px radius.
  Normal: as above. Hover: no change (touch-first). Pressed: uses the press motion. Focus:
  3 px `focus` ring, transparent fill. Disabled (tried or solved): fill stays `surface`, the
  tile is not tappable; a wrong tile keeps its cross.
- **Primary button ("Next blooms"):** `accent` fill, `accent_ink` text, 14 px radius, soft
  shadow (`shadow_size` 4, `shadow_color` 20 % plum). Normal `accent`, hover `#6E2446`,
  pressed `#6E2446` with no shadow, focus 3 px `focus` ring, disabled `surface_alt` fill with
  `ink_muted` text.
- **Secondary button:** `surface_alt` fill, `ink` text, same radii and states; hover
  `#EBD6BE`; disabled as primary disabled.
- **Reduce motion control:** a `CheckButton` labelled "Reduce motion", 54 px tall, `ink` text,
  placed with the other controls.
- **Panels:** the progress band and status line sit on `bg` with no border; the status line
  is `ink_muted` at 16 px, and result text is `success` or `error` plus a glyph.
- **Status banner:** single line under the progress count; text carries the meaning.

## 6. States

- **Idle:** tiles `surface`, border `#DDC8C6`, index digit `ink`.
- **Selected (pressed):** tile dips to 92 % scale and returns (press).
- **Related / peer:** not used (no peer highlighting; it would reveal the answer).
- **Conflict (wrong pick):** tile gets a cross in `error`, index digit turns `error`, the
  tile stays disabled, and the status says "That one matches the others. Look again."
- **Completed group:** not applicable (no row or region rule).
- **Solved:** the odd tile gets a `success` ring and check mark; the other tiles sweep once
  with `petal_pale`; status says "Found. Choose Next blooms, or replay this pattern."

## 7. Motion system

Shared timings from `scripts/motion.gd` (the template is not changed).

| Name | Trigger | Duration ms | Easing | Property |
| --- | --- | --- | --- | --- |
| press | tap any tile | 90 | back out | `offset_transform_scale` 0.92 → 1 |
| pop | correct tile placed | 180 | back out | `offset_transform_scale` overshoot → 1 |
| shake | wrong tile | 260 | sine in-out | `offset_transform_position` x, ±7 px damped |
| sweep | solved, all tiles | 35 stagger / 260 | sine out / back out | `offset_transform_scale` 1.08 → 1, `self_modulate` to `petal_pale` → white |
| intro | new pattern or replay | 16 stagger / 220 | back out, cubic | scale 0.86 → 1, y +10 → 0, alpha 0 → 1 |
| slide | next / previous pattern | 280 | cubic out | board `offset_transform_position` ±48 px, alpha 0 → 1 |
| celebrate | solved | ≤ 1100 | cubic out | up to 22 soft petal pieces from the grid centre, freed after |

**Signature moment:** the found flower lifts (pop) and the grid's petals drift out in a single
soft burst (`celebrate`, palette `[accent, success, surface]`), while the other tiles sweep
once in reading order. Total under 1.1 s; no flashing, no looping.

The motion layer is visual only: `Progress.choose()` updates state before any animation
starts, and input is never blocked by a tween.

## 8. Reduced motion

- A visible **"Reduce motion"** switch in the controls area, saved to `user://settings.json`
  as `{"reduced_motion": bool}` and applied on load.
- With it on, every call in `motion.gd` applies its final state at once: no tweens, no
  shake, no intro, no slide, and **no celebrate pieces** (the solved state still shows the
  check mark and text).
- No flashing at any setting; all motion stays below 3 Hz.

## 9. Haptics and accessibility

- **Haptics:** none shipped. Intent: none unless players ask for them.
- **Sound:** none shipped.
- **Accessibility checklist:**
  - every tile is a `Button` ≥ 76 px with a `accessibility_name` ("Flower card n of N");
  - focus ring visible on every control; keyboard and gamepad can reach all controls;
  - no timer, no fast gestures, no color-only signals;
  - text remains readable at 414 × 896 with the system text size; the column scrolls;
  - a visible reduce-motion switch (default off, respecting the player's choice).
- **Physical VoiceOver/TalkBack, real-device contrast and adult 40+ testing are still
  required and have not been performed.**

## 10. Icon and screenshot guidance

- **Icon:** keep the plum field, cream tiles, gold petals and one coral-free, structurally
  different bloom. Do not add text to the icon.
- **Screenshots:** (1) first six-tile screen with the title; (2) a marked wrong tile with the
  cross and text; (3) the found tile with the check and status text; (4) the 16-tile screen.
  Capture with `tools/store_catalog/capture.gd` at store sizes; motion is settled before the
  shot is taken.

## 11. QA checklist

1. Each round has exactly one structural outlier (`tests/test_patterns.gd`).
2. Progress saves, resumes and rejects tampered data (`tests/test_progress.gd`).
3. Tap a wrong tile: cross, text cue and shake; tap again is blocked.
4. Found tile: check, text, celebrate, and the sweep; Next unlocks only after finding.
5. Reduce motion on: no tweens or celebrate pieces; the state still updates (`tests/test_motion.gd`).
6. Reduce motion persists across launch via `user://settings.json`.
7. Focus ring visible when tabbing or using a gamepad; no control is smaller than 48 px.
8. Layout checked at 414 × 896 (portrait); the column scrolls and no text is clipped.
