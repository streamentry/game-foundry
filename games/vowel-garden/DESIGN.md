# Vowel Garden — design system v1.0 (Godot 4.7.2)

## 1. Positioning and emotional promise

A quiet garden of word tiles. Consonant tiles are planted in place; the player brings the vowels up
from the soil and watches each one bloom into gold. The promise: a calm, verbal puzzle that
respects competent adults, with no timer, no penalty and no health or cognition claim.

**Three adjectives:** verdant · tactile · warm.

**Signature interaction:** a tapped vowel slot lifts, the A/E/I/O/U keypad takes the letter, and the
tile blooms gold with a pop. A completed word sweeps its tiles left to right.

## 2. Color tokens

Derived from the icon: forest green ground, cream letter tiles, warm gold vowel tiles.

| Token | Hex | Use |
| --- | --- | --- |
| `bg` | `#2A6446` | Page ground (matches the icon's mid green) |
| `surface` | `#FBF3DF` | Cream consonant tiles, status banner, body text on `bg` |
| `surface_alt` | `#1E4D35` | Empty vowel slots, secondary buttons |
| `ink` | `#1A4329` | Letters on cream and gold tiles, banner text |
| `ink_muted` | `#CBE2CC` | Eyebrow and secondary captions on `bg` |
| `accent` | `#E9B449` | Gold vowel tiles, primary button, selected ring |
| `accent_ink` | `#1A4329` | Letters on gold |
| `success` | `#2F7A4E` | Completed banner text and rule |
| `error` | `#9B2F22` | Error text on the cream banner (never on green) |
| `focus` | `#FFE08A` | Keyboard focus ring on `bg` |

**Contrast (WCAG 2.x):** `ink` on `surface` **10.1:1** · `accent_ink` on `accent` **5.9:1** ·
`error` on `surface` **6.7:1** · `success` on `surface` **4.7:1** · `surface` on `bg` **6.3:1** ·
`surface` on `surface_alt` **8.8:1** · `ink_muted` on `bg` **5.1:1** · `focus` on `bg` **5.4:1**.

**State is never color-only.** A filled vowel changes letter and tile color together; an empty slot
shows "_"; a selected slot has a 3 px gold ring plus a larger tile; a completed word is a banner
line of text, not only a glow.

## 3. Typography scale (Godot fallback face)

| Role | Size px | Weight | Note |
| --- | --- | --- | --- |
| Eyebrow | 12 | regular, caps | `ink_muted` |
| Title | 27 | bold | `surface` |
| Heading (puzzle name) | 16 | bold | `surface` |
| Body / instructions | 15 | regular | `surface` |
| Status and preview | 16 | regular | banner text is `ink` |
| Caption (disclaimers) | 13 | regular | `ink_muted` |
| **Tile letter (vowel and consonant)** | **30** | bold | min tile 50×60 |
| Keypad letter | 24 | bold | min height 56 |
| Button label | 18 | bold | |

## 4. Spacing and layout

- Base unit 4 px. Side margin 16 px; section gap 12 px; tile gutter 3 px horizontal, 4 px vertical.
- Vowel grid: 7 columns, tiles 50×60 px (7 × 50 + 6 × 3 = 368 px, fits 414 px minus margins).
- Keypad: 5 columns, 7 px gaps. Buttons min 56 px high. Min touch target **48 px**.
- Content scrolls on short screens; no horizontal scroll at 320–430 px width.

## 5. Components

- **Consonant tile (given):** cream `surface`, `ink` 30 px letter, 11 px corner radius, 2 px shadow
  (`shadow_size` 3, `shadow_color` black at 25 %). Disabled keeps the cream colour; never focusable.
- **Vowel slot, empty:** cream `surface` tile with a 3 px solid gold `accent` outline and a small
  `ink` "_" (24 px). Tappable. Hover lightens 6 %; pressed darkens 8 %.
- **Vowel slot, filled:** solid `accent` gold tile, `ink` 30 px letter, 11 px radius, shadow.
- **Selected slot:** a 4 px `ink` ring on whichever tile it is.
- **Word layout:** each word is its own row (never split); words flow in an `HFlowContainer` with a
  16 px word gap and 10 px row gap; spaces are gaps, not tiles. Tile size is the largest that lets
  the longest word fit the 382 px column, clamped to 40–50 px wide (height 1.2 × width).
- **Space:** no tile.
- **Keypad (A E I O U):** primary buttons, gold `accent` fill, `accent_ink` 24 px letters, 12 px
  radius. Hover/pressed/focus/disabled as above.
- **Primary button (Next):** gold fill. **Secondary buttons (Undo, Erase, Restart, Previous, Copy
  challenge):** `surface_alt` fill with `surface` text, 12 px radius, 1 px `#4F8A66` border.
- **Reduce motion toggle:** `CheckButton`, `surface` text, 48 px high row, in the controls area.
- **Status banner:** `surface` panel, 12 px radius, `ink` text 16 px; a completed phrase switches the
  rule to `success`.

## 6. States

- **Idle:** all empty slots at `surface_alt`.
- **Selected:** 3 px `focus` ring, tile lifts with a 90 ms press dip.
- **Related / peer:** not used (one phrase is on screen at a time).
- **Conflict:** a rule-violating entry does not change state; the selected tile shakes 260 ms and the
  banner keeps its text cue.
- **Word complete:** every tile of that word sweeps left to right, gold glow.
- **Solved:** all tiles sweep, a short celebration of soft tiles plays, and the banner reads
  "The authored phrase is complete."

## 7. Motion system

| Name | Trigger | Duration ms | Easing | Property |
| --- | --- | --- | --- | --- |
| Press | tap a vowel slot | 90 | back, ease-out | `offset_transform_scale` 0.92 → 1 |
| Pop / bloom | vowel placed (bloom: pop 1.3 strength) | 180 | back, ease-out | `offset_transform_scale` |
| Bloom glow | vowel placed | 260 | sine | `self_modulate` gold-bright → white |
| Shake | rule rejects the entry | 260 | sine, ease-in-out | `offset_transform_position` x |
| Intro | level load, restart | 16 ms stagger (cap 420), 220 | back / cubic | scale, position, alpha |
| Sweep | word completes; all tiles when solved | 35 ms stagger, 260 | sine / back | scale 1.08, `self_modulate` |
| Slide | next / previous level | 280 | cubic, ease-out | position x ±48, alpha |
| Celebrate | solved | ≤ 1100 (22 pieces) | cubic, ease-out | position, rotation, alpha |

**Signature moment:** the bloom. The placed vowel pops to 1.3 strength and its gold tile briefly
brightens, so a single letter reads as a sprout.

Game state updates immediately; every animation only follows it.

## 8. Reduced motion

A "Reduce motion" toggle is saved in `user://settings.json` as `{"reduced_motion": bool}`. When on,
every `motion.*` call applies its final state instantly, the bloom glow is skipped, the word sweep and
celebration are omitted, and no effect flashes. The sweep is replaced by the static filled tile plus
the banner text.

## 9. Haptics and accessibility

- **Haptics:** none shipped. No sound by default.
- **Accessibility checklist:** tiles ≥ 50×60 px; letters 30 px; keypad 56 px; focus ring visible on
  every focusable control; labels on every button (`accessibility_name`); no color-only state; no
  timer; Reduce motion toggle visible; text wraps at 200 % text size (needs device audit).
- **Icon:** keep the forest green ground with cream and gold tiles. **Screenshots:** initial board,
  a legal mid-phrase entry, the rejected-entry shake, and the solved banner.

## 10. QA checklist

1. Tiles and keypad are readable at 414×896 and at 320 px width without clipping.
2. Every placed vowel blooms; a rejected entry shakes and leaves the state unchanged.
3. A completed word sweeps its tiles left to right, in reading order.
4. Solved state sweeps all tiles and shows the completion banner.
5. Reduce motion on: no tweens run after a tap or placement (`Motion.running` = 0).
6. Undo, Erase and Restart still update state instantly.
7. Contrast spot-checked against the table in section 2.
