# Ripple Rooms — design system (v0.2, feel pass)

## 1. Positioning and emotional promise

A calm pond of rooms: each irregular room fills with its numbers, and each finished room ripples outward. No timer, no streaks, no health or cognition claims, no age stereotypes. Picking a square and a number should feel quiet and deliberate, and a completed room should feel like a small, earned ripple.

**Three adjectives:** modular · tidal · patient.

**Signature interaction:** when a room is completed, its squares sweep a warm glow in reading order, 35 ms apart, like a ripple crossing the room. A solved board repeats the sweep across all cells and adds a short burst of soft tiles.

## 2. Color tokens

Derived from the icon: navy-to-blue gradient, cream tiles and light-blue rings.

| Token | Hex | Use |
| --- | --- | --- |
| `bg` | `#F3F7FB` | Screen background (pale water) |
| `surface` | `#FBF4E3` | Cream tile; room tint 2; celebration piece |
| `surface_alt` | `#D3E8F7` | Secondary buttons; room tint 1 |
| `ink` | `#15355A` | Headings, numerals, body text, cell captions |
| `ink_muted` | `#3E5E7A` | Reserved for secondary captions (not yet applied; helper copy uses `ink`) |
| `accent` | `#2F6DA3` | Primary keypad and Next buttons; selected-cell border; kicker |
| `accent_ink` | `#FFFFFF` | Text on `accent` |
| `success` | `#2E6B4F` | Status line when solved (text says "Complete!") |
| `error` | `#A4352F` | Status line on a conflict (text says "Duplicate ... too close") |
| `focus` | `#8A5A00` | 3 px focus ring on every focusable control |
| `rule` | `#6F8CA6` | 1 px cell and button borders (non-text contrast) |
| `glow` | `#FFF3C4` | Warm tint used by the completion sweep (state tint only) |

Room tints (cell backgrounds, cycled by room index): `#D3E8F7`, `#FBF4E3`, `#BFDDF2`, `#F4E4C1`, `#E0ECF6`.

**WCAG contrast (computed):**
- ink on surface 11.34:1; ink on bg 11.55:1; ink on surface_alt 9.86:1; ink on the room tints 8.79 to 10.36:1
- accent_ink on accent 5.47:1
- error on surface 6.12:1; error on bg 6.24:1
- success on bg 5.85:1
- accent on surface 4.99:1 (kicker text)
- focus on bg 5.51:1; rule on surface 3.20:1 (non-text border, meets 3:1)

**State is never color-only:** a given is a 2 px ink border plus a locked control; the selected cell gets a 3 px accent border; conflict and solved are carried by status text ("Duplicate room digit...", "Complete!"), not only by the status color.

## 3. Typography scale

The default theme font is used with no custom font files (regular weight).

| Role | Size px |
| --- | --- |
| Kicker (`RIPPLE ROOMS / DISTANCE`) | 12 |
| Title ("Every number leaves space.") | 28 |
| Heading (`PUZZLE n OF 6 • name`) | 16 |
| Body and rule copy | 15 |
| Info line (selected room) | 14 |
| Helper and footnote | 13 to 14 |
| Button label | 16 |
| Cell caption (value, then room letter and size) | 17 |

## 4. Spacing and layout

- Base unit 4 px. Screen gutters 16 px left and right; top 20 px; bottom 24 px.
- Column separation 12 px; keypad gap 7 px; action row gap 8 px; cell gap 4 px.
- Cells 76 × 76 px (4 × 4 board). Buttons at least 54 px high (above the 48 px minimum).
- Layout is a vertical scroll on short screens; nothing is shrunk to fit.
- Verified at 414 × 896 logical px (portrait, with 1242 × 2688 store-scale capture).

## 5. Components

- **Board cell:** room-tint background, 1 px `rule` border, 12 px corner. Given: 2 px `ink` border, disabled, keeps tint. Editable: 1 px border, hover lightens 6 %, pressed darkens 8 %. Selected: 3 px `accent` border. Focus: 3 px `focus` ring.
- **Keypad digits 1 to 4 (primary):** `accent` fill, `accent_ink` label, 12 px corners, raised 4 px shadow (ink at 22 %). Hover +6 % lightness; pressed −8 % lightness with the shadow removed; focus ring; disabled blends 45 % toward `bg`.
- **Secondary buttons (Undo, Erase, Restart, Previous, Copy challenge):** `surface_alt` fill, `ink` label, 1 px `rule` border, no shadow. Same hover, pressed, focus and disabled rules.
- **Next puzzle (primary):** as keypad primary; disabled until the current board is solved.
- **Panels:** no card chrome; sections are separated by 12 px rhythm on the `bg`.
- **Status banner:** 15 px line under the progress count. `ink` by default, `error` on a conflict, `success` when solved. Always carries text.
- **Reduce motion toggle:** a `CheckButton` labelled "Reduce motion", stored in `user://settings.json` as `{"reduced_motion": bool}`.

## 6. States

| State | Shown by |
| --- | --- |
| Idle | Room tint, rule border |
| Selected | 3 px accent border, info line names room and size |
| Related / peer | Same room letter in the caption; no extra highlight (kept minimal) |
| Conflict | `error` status text; the entered square shakes once (not persistent) |
| Completed room | Sweep of that room's squares with `glow` |
| Solved | Status "Complete!" in `success`, sweep of all squares, celebration burst, Next unlocked |

## 7. Motion system

Shared timings from `scripts/motion.gd`:

| Name | Trigger | Duration ms | Easing | Property |
| --- | --- | --- | --- | --- |
| press | Tap a square (selection) | 90 | Back, ease-out | `offset_transform_scale` 0.92 → 1 |
| pop | Value placed or erased | 180 | Back, ease-out | `offset_transform_scale` 1.16 × 0.90 → 1 (squash and stretch) |
| shake | Entry creates a conflict | 260 | Sine, ease-in-out | `offset_transform_position` x, damped 7 px |
| sweep | Room completed (cells staggered 35 ms) | 260 per cell | Sine out, Back out | scale 1.08 and `self_modulate` to `glow`, then back |
| intro | Board load or Restart (16 ms stagger, capped at 420 ms) | 220 | Back and Cubic, ease-out | scale 0.86 → 1, y 10 → 0, alpha 0 → 1 |
| slide | Next or Previous puzzle | 280 | Cubic, ease-out | x ±48 px to 0, alpha 0 → 1 |
| celebrate | Solved | ≤ 1100 | Cubic, ease-out | 22 soft tiles, fade in the last half, freed after |

Game state updates immediately; every animation only follows it, and every tween ends at its exact rest value.

**Game-specific signature:** the completed-room sweep (ripple across the room in reading order) and the solved burst from the board centre.

## 8. Reduced motion

When "Reduce motion" is on, every motion call becomes instant: cells rest at scale 1, the intro and slide are not played, the sweep applies no tint, and the celebration is omitted entirely. Nothing flashes at any setting; the only glow is a single soft tint per completed room. The setting persists across launches.

## 9. Haptics, accessibility and icon guidance

- **Haptics:** none shipped.
- **Accessibility checklist:** touch targets at least 54 px; each cell has a row, column, fixed-or-editable and value accessibility name; focus rings on all controls; status conveyed by text as well as color; motion toggle present; no timer; no color-only state. Still to verify on device: VoiceOver and TalkBack reading order, 200% text scale, safe areas.
- **Icon:** keep the navy-to-blue field, cream tiles and light-blue rings from `icon.png`. Any screenshot should use the same palette.
- **Screenshots:** 01 (puzzle start with room letters and sizes), 02 (after taps, a room selected), then a completed room and the solved screen once a board is solved on a real device.

## 10. QA checklist

1. All four test files exit 0 with zero failures, no `SCRIPT ERROR`, `Parse Error` or `ERROR:` lines.
2. Every room tint and text pair meets the contrast ratios in section 2.
3. Tap a square: it dips (press) and is selected; placing a value pops it.
4. An entry that breaks a rule shakes once and the status line turns `error` with text.
5. Completing a room sweeps only its squares; solving sweeps the whole board and shows "Complete!" in `success`.
6. "Reduce motion" on: no tween runs on placement, restart or level change; relaunch keeps the setting.
7. At 414 × 896 nothing overlaps or clips; the lower buttons are reachable by scrolling.
8. Real-device check (VoiceOver, TalkBack, 200% text) recorded before any store submission.
