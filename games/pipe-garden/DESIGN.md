# Pipe Garden — visual and motion system v0.2

**Positioning and emotional promise.** A quiet greenhouse workbench where a single water route is
found by turning copper pipes one tap at a time. The player should feel patient and capable, never
rushed or talked down to. No health, memory or cognitive claims.

**Three adjectives:** botanical · engineered · tranquil.

**Signature interaction:** every tap rotates one pipe in place, with a soft copper squash-and-settle
on that tile. Completing the single route releases a short, restrained burst of leaf and copper tiles
across the board, then stops.

## 1. Color tokens

Palette is taken from the app icon: forest green field, cream tiles, copper pipe, leaf-green sprigs.

| Token | Hex | Use |
|-------|-----|-----|
| `bg` | `#F3EEDD` | Screen background (cream, the icon's tile family) |
| `surface` | `#FBF8EE` | Raised cell and panel fill |
| `surface_alt` | `#E6DCC2` | Endpoint (END) cells, secondary buttons |
| `ink` | `#1E3A2B` | Headings, body text, pipe marks, borders on focus |
| `ink_muted` | `#4F5E4C` | Captions and explanatory text |
| `accent` | `#A4572A` | Copper: primary button, active tile border, celebration |
| `accent_ink` | `#FFFFFF` | Text on accent |
| `success` | `#2F6B3A` | Completed-route status text (always with words) |
| `error` | `#9E3E40` | Reserved for rule-violation text (none shipped in pipe mode) |
| `focus` | `#1E3A2B` | 3 px focus ring around every control |

Contrast (WCAG 2.x, computed):

- `ink` on `surface` 11.66:1; `ink` on `bg` 10.67:1; `ink` on `surface_alt` 9.07:1
- `accent_ink` on `accent` 5.28:1
- `ink_muted` on `bg` 5.95:1; on `surface_alt` 5.06:1
- `success` on `surface` 6.02:1; `error` on `surface` 6.14:1
- `focus` on `bg` 10.67:1

State is never color-only. Route completion is stated in words ("All nine pipes form one
connected route. Flow complete!"), each tile shows its shape name (END, LINE, BEND) and its
orientation glyph, and the active tile has a 3 px border as well as a tint.

## 2. Typography scale (Godot default face, px)

| Role | Size | Weight |
|------|-----:|--------|
| Kicker (small caps label) | 12 | regular, `accent` |
| Title | 28 | bold-looking by size |
| Body / instructions | 15 | regular |
| Heading (puzzle name) | 16 | regular |
| Button label | 17 | regular |
| Tile glyph | 25 | regular (19 for non-pipe modes) |
| Tile caption (END / LINE / BEND) | inside tile, 13 | regular |
| Footnote | 13–14 | regular, `ink_muted` |

No bundled font: keeps the export small and the text scales with the system UI. Deliberate choice
for this research build, not a brand face.

## 3. Spacing and layout

- Base unit 4 px. Screen gutter 16 px left and right, 20 px top, 24 px bottom.
- Column separation 12 px; button row separation 8 px; grid gap 4 px.
- Board tile 96 px on the 3×3 grid (min touch target 48 px, tiles far exceed it).
- Every control has a minimum height of 48 px (buttons 54 px). The whole column scrolls at short
  heights, so nothing is cut off at 414×896.

## 4. Components

- **Board cell (pipe tile):** `surface` fill, 1 px `#C9BE9F` border, 12 px corner radius, soft
  2–3 px shadow. END cells use `surface_alt`. Hover lightens the fill by 4 %; pressed darkens it
  by 6 % and draws the 3 px `accent` border; focus draws the 3 px `focus` ring.
- **Primary button (Next):** `accent` fill, `accent_ink` text. Normal, hover (+4 % light), pressed
  (−6 % dark), focus (3 px ring), disabled (text `ink_muted`, fill `surface_alt`, no shadow).
- **Secondary button (Undo, Restart, Previous, Copy challenge):** `surface_alt` fill, `ink` text,
  same state set.
- **Reduce-motion control:** a `CheckButton` labelled "Reduce motion", 48 px tall, in the controls
  area below the Undo/Restart row. Its state is saved in `user://settings.json`.
- **Panel / status banner:** status text is plain body text in `ink`; success state adds the
  word "complete" and uses `success`.

## 5. States

- **Idle:** surface fill, neutral border.
- **Selected (last tapped):** 3 px `accent` border; the same tile carries the pop motion.
- **Related / peer:** none in pipe mode (no peer highlighting is shipped).
- **Conflict:** none in pipe mode. Rotation always succeeds, so there is no rejected input to shake.
- **Completed group:** the route is one group; it sweeps as one wave on completion.
- **Solved:** status line states completion; the board sweeps and a restrained celebration plays
  once, then the board rests.

## 6. Motion system

Shared timings from `scripts/motion.gd` (copied unchanged from the shared template).

| Name | Trigger | Duration (ms) | Easing | Property |
|------|---------|--------------:|--------|----------|
| press | tap a tile (before pop) | 90 | back out | `offset_transform_scale` |
| pop | rotate a tile | 180 | back out | `offset_transform_scale` 1.16 / 0.90 → 1 |
| intro | load, restart: board cells in reading order | 220, stagger 16 | back / cubic out | scale, position, `modulate:a` |
| slide | next / previous level | 280 | cubic out | `offset_transform_position`, `modulate:a` |
| sweep | route complete | 260 per tile, stagger 35 | sine / back out | scale 1.08, `self_modulate` glow |
| celebrate | route complete | ≤ 1100 (22 pieces) | cubic out | position, rotation, alpha |

Signature moment: the tile squash on each tap (copper, `pop`), and on completion one wave across
the board followed by a short burst of leaf and copper pieces (`celebrate`, palette
`accent`, `success`, `surface`). Pieces never flash and clear themselves within 1.1 s.

Game state updates immediately on every tap; motion only follows.

## 7. Reduced motion

- Every call becomes instant: the final rest state is applied synchronously and no tween is created.
- Celebration pieces are not created at all.
- Nothing flashes at any setting; the sweep glow is a tint change only, and it is removed in
  reduced mode.
- The toggle is visible in the controls area and applies immediately.

## 8. Haptics and sound

No haptics and no sound are shipped. Any future haptic must be a single short tap on completion
only, and must respect the same reduced-motion setting where the OS exposes one.

## 9. Accessibility checklist, icon and screenshots

- Accessibility: every tile has an accessible name with row, column and shape; every control is
  focusable with a 3 px ring; text is 12 px minimum; no timers; no color-only cues.
- Icon: deep forest green field, cream tiles, copper pipe, leaf sprigs. The in-game palette uses
  the same greens, cream and copper so the app and icon read as one family.
- Screenshots to capture on real devices: initial board, mid-route with a rotation, the completed
  route with its status line, and the reduce-motion toggle on.

## 10. QA checklist

1. Palette tokens match the table above; every text/background pair is at least 4.5:1.
2. A tap rotates one tile and the pop never leaves a tile off its rest scale.
3. Undo and Restart re-run the intro without blocking input.
4. Next and Previous slide the board and the new level is already applied.
5. Completion shows the status line and the one-time celebration, then the board rests.
6. Reduce motion on: no tweens run after a tap or a level load; the state is saved and restored.
7. At 414×896 and at the 200 % text setting, no control is clipped and the column scrolls.
8. Headless tests pass: rules, progress, scene and motion.
