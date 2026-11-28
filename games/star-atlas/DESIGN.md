# Star Atlas — visual and motion system (v0.2, upgrade)

## 1. Positioning and emotional promise

Star Atlas is a quiet night-sky chart. Each round is a small constellation to map: five stars, one per row, column and lettered region, none touching. The promise is calm, deliberate satisfaction. The sky is dark and steady; the gold stars are the only things that glow. This is a logic playtest for competent adults, not a treatment or training claim.

**Three adjectives:** nocturnal · steady · luminous.

**Signature interaction:** a tap places a star that settles with a small overshoot and then keeps a slow, gentle twinkle, so the player can see at a glance which stars are placed. Region boundaries are drawn as thick gold lines, so the lettered regions read as map borders rather than as tiny letters alone.

## 2. Color tokens

| Token | Hex | Use |
| --- | --- | --- |
| `bg` | `#0E1638` | Sky base; background gradient runs `#1E2A5E` (top) to `#0A0F2C` (bottom) |
| `surface` | `#1C2A5C` | Board cells (region tints vary slightly around this value) |
| `surface_alt` | `#26366B` | Secondary buttons, cell hover |
| `ink` | `#F4F1E6` | Headline, body, status, labels (warm white, not pure white) |
| `ink_muted` | `#B9C2E0` | Helper text, region letters, dots |
| `accent` | `#F5D27A` | Pale-gold stars, region outlines, primary button fill, kicker |
| `accent_ink` | `#0A0F2C` | Text on gold buttons |
| `success` | `#9FE3B5` | Solved status text (always with the word "Five stars") |
| `error` | `#FF9B8A` | Conflict status text (always with the explanation) |
| `focus` | `#9CC3FF` | Keyboard focus ring, 3 px |

Region tints for cells (subtle, all close to `surface`): `#1C2A5C`, `#213166`, `#18244F`, `#26366B`, `#1F2D5E`.

**Contrast (WCAG 2.x, computed):**
- `ink` on `surface`: **12.1:1** (`ink` on `surface_alt` 10.2:1, on `bg` 15.6:1)
- `accent_ink` on `accent`: **12.9:1**
- `error` on `surface`: **6.7:1** (on `bg` 8.7:1)
- `success` on `surface`: **9.2:1**
- `ink_muted` on `surface`: **7.7:1**
- `focus` on `surface`: **7.6:1**

**State is never color-only.** A star is a drawn five-point shape; a dot is a drawn circle; a region is identified by its letter and by the thick outline; conflict and solved are stated in text; the focus ring is a border, not only a tint.

## 3. Typography scale

| Role | Face | Size (px) | Weight |
| --- | --- | --- | --- |
| Kicker (`STAR ATLAS / ...`) | system sans | 12 | bold, letter-spaced by caps |
| Title (`Five stars. A little order.`) | serif (Georgia, fallback system serif) | 27 | bold |
| Body / instructions | system sans | 15 | regular |
| Heading (`ROUND n OF 6`) | system sans | 16 | semibold |
| Status / progress | system sans | 15 | semibold |
| Buttons | system sans | 16 | semibold |
| Cell region letter | system sans | 13 | bold |
| Helper / footer | system sans | 13 | regular |

Scale is set with theme overrides on the existing nodes. Text reflows; it never clips at 414 px width.

## 4. Spacing and layout

- Base unit **6 px**; side margin **18 px**; vertical rhythm **12 px** between blocks.
- Board: 5 cells of **64 px** with **6 px** gaps = 344 px square, centred. Cell touch target 64 px (min 48).
- Controls: 54 px tall buttons, 8 px gap in a row.
- Reference size 414 x 896. The screen scrolls on short heights; nothing essential is shrunk.

## 5. Components

- **Board cell (Button):** fill = region tint, 1 px `#2D3D7A` border, 10 px corner radius. Normal / hover (`surface_alt`) / pressed (`#16214A`) / focus (3 px `focus` border) / disabled-not-used. Children: a star or dot drawn by `StarMark` (mouse ignored), and the region letter in the bottom-right corner.
- **Star mark:** five-point polygon, outer radius 0.32 of the cell, inner radius 0.42 of outer, fill `accent`. Dot: circle radius 6 px in `ink_muted`.
- **Region outlines:** one overlay drawn above the cells. Thick 4 px `accent` lines on every boundary between cells of different regions, and on the outer board edge.
- **Primary button** ("Next puzzle"): fill `accent`, text `accent_ink`; hover lightens 6 %; pressed darkens 8 %; focus ring `focus`; disabled = 40 % opacity.
- **Secondary button** (Undo, Restart, Previous, Copy challenge): fill `surface_alt`, text `ink`; hover `#30437F`; pressed `#16214A`; focus ring; disabled = 40 % opacity.
- **Toggle:** "Reduce motion" `CheckButton` in the controls area, 48 px tall, text `ink`.
- **Panel / status banner:** status text line under the board; no filled box, so it does not compete with the sky.

## 6. States

- **Idle:** navy cell, no mark.
- **Selected / just tapped:** press dip (see motion), focus ring if keyboard.
- **Star placed:** gold five-point star, gentle twinkle (off with reduce motion).
- **Dot (marked no-star):** small `ink_muted` dot.
- **Related / completed group:** when a region receives its single star, its cells glow briefly (sweep).
- **Conflict:** placed star violates the rules: the cell shakes and the status text names the clash ("Check: no touching stars, or two in one row, column or region.").
- **Solved:** status reads "Five stars, one per row, column and region. Beautiful." in `success`; all cells sweep once; a short burst of soft tiles.

## 7. Motion system

| Name | Trigger | Duration | Easing | Property |
| --- | --- | --- | --- | --- |
| press | tap on any cell | 90 ms | back-out | cell `offset_transform_scale` 0.92 → 1 |
| pop | a value changes (star, dot, clear) | 180 ms | back-out | cell scale 1.16 x / 0.90 y → 1 |
| shake | placed star conflicts | 260 ms | sine in-out | cell `offset_transform_position` x, damped |
| stagger_in | level load / restart | 16 ms step (cap 420 ms), 220 ms each | cubic / back-out | scale 0.86 → 1, y +10 → 0, alpha |
| sweep | a region becomes complete, and on solve | 35 ms step, 260 ms each | sine out | scale 1.08 → 1, `self_modulate` to gold-white and back |
| slide_swap | next / previous level, after state applied | 280 ms | cubic out | board x ±48 → 0, alpha |
| celebrate | solved | ≤ 1100 ms, 22 pieces | cubic out | position, rotation, alpha, then freed |
| twinkle (signature) | a star is placed | 1400 ms half-cycle, looping | sine in-out | star mark `scale` 1 → 1.07 → 1 |

Game state updates first; every animation only follows. Twinkle is the one loop, kept small (7 %) and slow.

## 8. Reduced motion

A visible "Reduce motion" toggle in the controls area; persisted to `user://settings.json` as `{"reduced_motion": bool}`.

- Every motion call becomes instant: the final rest value is applied synchronously.
- Twinkle is not started; star marks stay still.
- Celebration is omitted (no pieces); the solved status text still appears.
- No flashing anywhere. The sweep glow is a single tint change, not a blink, and it is replaced by an instant tint under reduce motion.

## 9. Haptics, accessibility, icon and screenshots

- **Haptics:** none shipped. **Sound:** none.
- **Accessibility checklist:** cell accessibility names state row, column, region and star/dot/empty; 64 px cells; focus ring on all controls; status always in words; contrast ratios above; reduce-motion toggle; no timers.
- **Icon:** deep navy night sky gradient, pale-gold stars, thick gold region outlines around cell groups. The in-game palette uses the same navy and gold family; the in-game outlines are thicker than the icon's to read at phone size.
- **Screenshots:** capture a 5x5 board mid-play with two stars placed and one region swept, and a solved board. Real device capture still pending.

## 10. QA checklist

1. Stars draw as five-point shapes; no `★` glyph remains in any cell.
2. Region outlines appear on every boundary between different regions, and on the board edge.
3. Every motion tween ends at its rest value (scale 1, position 0, alpha 1) within its duration.
4. With reduce motion on: no tweens are running on cells; no twinkle loop exists.
5. Tap during an animation still changes state immediately.
6. Conflict shake fires only when a placed star breaks the rules, and the status text names it.
7. Layout fits 414 x 896 and scrolls on short heights; no clipped labels at larger text.
8. Keyboard focus ring visible on every control.
