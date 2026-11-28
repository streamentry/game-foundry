# Number Pyramid — visual, motion and QA system v0.2

## 1. Positioning and emotional promise

A quiet stone-carving puzzle for adults who like arithmetic they can reason through. You rebuild the four foundation stones from a handful of printed sums, and every stone above them settles into place as the pieces fit. The mood is patient and constructive, never urgent.

**Adjectives:** stonework · calm · constructive.

**Signature interaction:** entering a foundation digit updates the sum stones above it at once, and a solved pyramid lets a wave of light climb from the foundation to the apex, row by row, once the sums check out against every clue.

## 2. Color tokens

Palette sampled from the icon (burnt orange background `#CE7D37`, cream stones `#FCEDCC`, dark brown digits `#451901`). Accent fills are deepened from the icon orange so white text on them passes contrast.

| Token | Hex | Use |
| --- | --- | --- |
| `bg` | `#F7EDDA` | Page background |
| `surface` | `#FCEDCC` | Editable stones, raised panels |
| `surface_alt` | `#EBCFA6` | Printed clue stones, secondary buttons |
| `ink` | `#451901` | Headings, digits, body text |
| `ink_muted` | `#6B4A33` | Kicker, helper and preview text |
| `accent` | `#A65A2A` | Primary button fill, selection border, sweep and particle accents |
| `accent_ink` | `#FFFFFF` | Text on accent |
| `success` | `#2F6B3A` | Completion message (always with the words "All ten sums match") |
| `error` | `#9E3E40` | Reserved for conflict text; not used for text yet |
| `focus` | `#6E3210` | Keyboard focus ring (3 px) |
| `glow` | `#F6D9A8` | Sweep tint (multiplied into `self_modulate`, returns to white) |

**Contrast (WCAG 2.x, computed):**
- `ink` on `surface`: **13.02:1** (also 12.98 on `bg`, 10.06 on `surface_alt`)
- `accent_ink` on `accent`: **5.10:1**
- `error` on `surface`: **5.63:1**
- `success` on `surface`: **5.52:1** (used only on `surface`, not on `surface_alt`, where it drops to 4.26)
- `ink_muted` on `surface`: 6.83:1
- `accent` on `surface`: 4.40:1, so `accent` is used only for fills and borders, never for small text.

**Color is never the only signal:** the selected stone has a 3 px border and a text heading; completion is announced in words; a clue stone is distinguished by fill and by being a fixed number.

## 3. Typography scale

Godot fallback sans-serif, bold for headings.

| Role | Size px | Weight |
| --- | --- | --- |
| Title (headline) | 27 | bold |
| Heading (puzzle line) | 16 | bold |
| Body / instructions | 15 | bold/regular |
| Button label | 17 | bold |
| Caption / kicker | 12 | bold |
| Helper note | 13 | regular |
| Stone digits (pyramid) | 19 | bold |

## 4. Spacing and layout

- Base unit: 4 px. Page margins 16 px sides, 22 px top, 24 px bottom, inside a scroll container so small screens scroll instead of clipping.
- Column gap 12 px; keypad gaps 7 px; action row gap 8 px; stone grid gap 3-4 px.
- Minimum touch target 48 px. Keypad and action buttons 54 px; the Reduce motion toggle is 48 px tall.
- Reference viewport 414×896 (iPhone). Content above the keypad fits without scrolling on that size.

## 5. Components

- **Board stone (editable):** 69×65 px, `surface` fill, 1 px `#CDB18F` border, 12 px radius, 4 px soft shadow. Selected: 3 px `accent` border.
- **Clue stone (fixed):** `surface_alt` fill, disabled, same geometry, no hover.
- **Derived stone (upper, computed):** `surface` fill, disabled, shows `?` until its inputs are known.
- **Keypad digit buttons (primary):** `accent` fill, `accent_ink` text. States: normal = accent; hover = accent lightened 8%; pressed = accent darkened 7% with a 1 px shadow; focus = 3 px `focus` ring; disabled = accent fill at 60% opacity is not used; disabled buttons keep full contrast and are skipped by focus.
- **Secondary buttons (Undo, Erase, Restart, Previous, Copy challenge):** `surface_alt` fill, `ink` text, same state rules.
- **Panels:** `surface` with 12 px radius and a 4 px shadow at 16% opacity.
- **Status banner:** text line under the board. Neutral `ink`; when solved it switches to `success` and says "All ten sums match every printed clue!".
- **Reduce motion toggle:** CheckButton, 48 px tall, `ink` text, 16 px, labelled "Reduce motion".

## 6. States

| State | Visual |
| --- | --- |
| Idle | Stones at rest, no outline |
| Selected | 3 px `accent` border on the foundation stone |
| Related / peer | Derived stones above that depend on the selected stone are not highlighted; their values update instead |
| Conflict | The rejected stone shakes horizontally (the keypad currently cannot reach a rejected entry, so this is wired but rarely visible) |
| Completed group | Not a separate state; the foundation row is covered by the solved sweep |
| Solved | Sweep wave from foundation to apex, then confetti burst; status text becomes `success` |

## 7. Motion system

Shared timings from `templates/motion.gd`; no constants changed.

| Name | Trigger | Duration ms | Easing | Property |
| --- | --- | --- | --- | --- |
| Press | Tap or select a foundation stone | 90 | back-out | `offset_transform_scale` 0.92 → 1 |
| Pop | Digit placed, erased or changed | 180 | back-out | `offset_transform_scale` 1.16/0.90 → 1 |
| Shake | Entry rejected (conflict) | 260 | sine in-out | `offset_transform_position` x |
| Intro | Level load or restart | 16 ms stagger, 220 | back-out / cubic-out | scale, position, alpha |
| Slide | Next or Previous level | 280 | cubic-out | board `offset_transform_position` x ±48 and alpha |
| Sweep | Solved (foundation then apex) | 35 ms stagger, 260 | sine/back | scale 1.08, `self_modulate` to `glow` |
| Celebrate | Solved | ≤ 1100 total, 22 pieces | cubic-out | position, rotation, alpha |

**Signature moment:** the solve wave. Foundation stones light first, then the two middle stones, then the two next, then the apex, so the pyramid visibly builds upward.

## 8. Reduced motion

Setting: "Reduce motion" toggle, persisted in `user://settings.json` as `{"reduced_motion": bool}`.

When on, every motion call applies the final state synchronously: no tweens, no intro, no slide, no shake, no sweep, and no confetti. The celebration burst is omitted entirely. No flashing, and the solved state is still conveyed by the `success` text and the completed sums.

## 9. Haptics, accessibility, icon and screenshots

- **Haptics:** none shipped. Intent: none, to keep the game quiet.
- **Sound:** none shipped.
- **Accessibility checklist:** 48 px targets; text 12–27 px with 200% scaling still scrolls rather than clipping; contrast table above; focus ring on every control; accessibility names on stones (`Stone N, value`, clue flagged); reduce-motion toggle visible on the first screen; no timer; no color-only signals.
- **Icon:** keep the existing icon (orange field, cream stones, dark digits). Palette is drawn from it.
- **Screenshots:** initial board, mid-entry, solved state, and the reduced-motion state.

## 10. QA checklist

1. `test_rules`, `test_progress`, `test_scene`, `test_motion` exit 0 with no `SCRIPT ERROR`, `Parse Error` or `ERROR:` lines.
2. Reduced motion on: placing a digit leaves no running tween and the stone at rest.
3. Reduced motion off: a placed stone animates and returns to rest within 0.6 s.
4. Layout at 414×896 shows the board and keypad with no overlap and no clipped text.
5. Every text/background pair in section 2 meets 4.5:1 (checked by the numbers above).
6. Solve the first puzzle with keypad input only; the status changes to the success line and the wave plays.
7. Restart and Undo still update state immediately; animations never gate state.
8. Real phone check (iPhone compact and large, Android) is still required before any release.
