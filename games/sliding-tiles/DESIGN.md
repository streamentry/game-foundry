# Sliding Tiles: design system v0.2 (warm wood)

## 1. Positioning and emotional promise

A calm wooden sliding-tile puzzle for adults who want a classic board they can read at a glance. Each tile is a physical block that slides into the open gap, and the board answers every move at once without ever hurrying the player. Nothing here claims to train memory or health; the promise is a tidy, honest puzzle.

**Three adjectives:** tactile · warm · unhurried.

**Signature interaction:** the moved tile physically slides into the empty slot (about 140 ms, cubic ease-out). The game state changes first; the slide only follows it. Completed rows light up one at a time, and the solved board gets one restrained burst.

## 2. Colour tokens

Sampled from `icon.png`: walnut background `#3C2418` to `#784E36`, tray `#54361E`, recess `#482A18`, tan tile `#EACCA2`, numerals `#784E2A`.

| Token | Hex | Use |
| --- | --- | --- |
| `bg` | `#3C2418` | Page background and the empty slot (a hole, darker than the tray) |
| `surface` | `#54361E` | Board tray, status surfaces |
| `surface_alt` | `#5C3A22` | Secondary buttons (Undo, Restart, Previous, Reduce motion row) |
| `ink` | `#F6E7C8` | Headings, body text, labels on dark surfaces |
| `ink_muted` | `#D9C3A0` | Captions and secondary explanation |
| `accent` | `#E0A458` | Primary button (Next), movable-tile outline, row sweep glow |
| `accent_ink` | `#3C2418` | Text on `accent` |
| `tile` | `#EACCA2` | Every numbered tile face |
| `tile_ink` | `#784E2A` | Numerals on tiles |
| `edge` | `#7A5238` | Hairline wood edge on every tile and button |
| `focus` | `#FFD27A` | Keyboard focus ring; completed-row glow |
| `success` | `#B5D08A` | Solved state accent (always paired with the words "Solved!") |
| `alert` | `#F2977F` | Violation or conflict accent (always paired with text) |

**WCAG contrast (computed):**

| Pair | Ratio | Requirement |
| --- | --- | --- |
| `ink` on `bg` | 11.79 : 1 | ≥ 4.5 pass |
| `ink` on `surface` | 8.93 : 1 | ≥ 4.5 pass |
| `ink_muted` on `surface` | 6.38 : 1 | ≥ 4.5 pass |
| `accent_ink` on `accent` | 6.60 : 1 | ≥ 4.5 pass |
| `tile_ink` on `tile` | 4.67 : 1 | ≥ 4.5 pass |
| `accent` on `surface` | 5.00 : 1 | ≥ 4.5 pass |
| `alert` on `surface` | 4.94 : 1 | ≥ 4.5 pass |
| `success` on `surface` | 6.42 : 1 | ≥ 4.5 pass |
| `focus` on `surface` | 7.67 : 1 | ≥ 3 : 1 non-text pass |

**State is never colour-only.** A movable tile is marked by a 3 px `accent` outline and a lift shadow, and it is the only tile that responds to a tap. Solved shows the words "All eight tiles are back in order. Solved!". Conflicts show a shake and the existing status text.

## 3. Typography scale

Godot's default face, with the weights the engine gives it.

| Role | Size | Weight | Notes |
| --- | --- | --- | --- |
| Title ("Bring the numbers home.") | 28 px | Bold | Two lines allowed |
| Heading (puzzle name line) | 16 px | Regular | Caps only for the kicker line (12 px) |
| Body | 15 px | Regular | Instructions, status |
| Caption | 13–14 px | Regular | Footnotes, the privacy line |
| Button label | 17 px | Bold-ish (default) | Min 54 px tall |
| Tile numeral | 25 px | Bold | Tile 96 px square |

## 4. Spacing and layout

- **Base unit:** 4 px. Column rhythm 12 px; horizontal page gutter 16 px; top 20 px, bottom 24 px.
- **Tray:** 12 px inner padding, 16 px corner radius, 8 px shadow lifting it off the page.
- **Grid gaps:** 4 px between tiles (pitch 100 px for the 96 px tile).
- **Touch targets:** all buttons and the Reduce motion switch are at least 48 px tall; tiles are 96 px.
- **Safe margins:** 414 × 896 layout, the content scrolls on short screens, no horizontal scroll.

## 5. Components

- **Tile (movable):** `tile` face, 3 px `accent` outline, 12 px radius, 4 px shadow at offset (0, 2). Hover lightens by 6 %, pressed darkens by 8 % and drops the shadow to 1 px.
- **Tile (not movable):** `tile` face, 1 px `edge` outline, same shadow. Disabled, so it cannot be tapped.
- **Empty slot:** `bg` fill, no shadow, no outline, disabled.
- **Primary button (Next):** `accent` fill, `accent_ink` text. Hover +6 %, pressed −8 % with 1 px shadow, focus 3 px `focus` ring, disabled falls back to the same fill with the disabled text colour.
- **Secondary buttons (Undo, Restart, Previous, Copy challenge):** `surface_alt` fill, `ink` text, 1 px `edge` outline, same hover and pressed rules.
- **Reduce motion switch:** a `CheckButton` in a 48 px row, `ink` text, saved to `user://settings.json`.
- **Panels:** the tray (`surface`) and the status text block with no border.
- **Status banner:** the status line under the board. It holds text only; success and alert colours are reserved for the solved and conflict copy.

## 6. States

- **Idle:** the board at rest, every tile at scale 1.
- **Selected (tap):** a 92 % press dip on the tapped tile.
- **Related / peer:** the tiles beside the empty slot are movable and show the accent outline. Nothing else is highlighted.
- **Conflict:** a tap that is not a legal slide shakes that tile sideways and leaves the state unchanged. A text cue remains on the status line.
- **Completed group:** a row that matches its goal order lights in `focus` for one wave, once, when it becomes correct. Rows that break again do not replay.
- **Solved:** the status line says the puzzle is solved and Next becomes available. All nine cells get the completion wave, and a single small celebration bursts from the tray.

## 7. Motion system

| Name | Trigger | Duration (ms) | Easing | Property |
| --- | --- | --- | --- | --- |
| press | tap a tile | 90 | BACK out | `offset_transform_scale` 0.92 → 1 |
| pop | tile placed in the gap | 180 | BACK out | `offset_transform_scale` overshoot → 1 |
| slide (game-specific) | tile moves into the gap | 140 | CUBIC out | `offset_transform_position` old slot → 0 |
| shake | illegal tap | 260 | SINE in-out | `offset_transform_position` x, damped |
| sweep | a row becomes correct | 35 ms stagger, 260 total | SINE out / BACK out | `offset_transform_scale` 1.08 → 1 and `self_modulate` to `focus` and back |
| intro | level load or restart | 16 ms stagger, 220 total | BACK out | scale 0.86 → 1, fade in, rise 10 px |
| level swap | Next or Previous | 280 | CUBIC out | board slides 48 px and fades in |
| celebrate | solved | ≤ 1100, 22 pieces | CUBIC out | soft `tile`, `accent`, `success` pieces that fade out |

**Signature moment:** the tile slide. Its duration is intentionally shorter than the shared 280 ms slide, so repeated play keeps up. Level swaps keep the shared 280 ms.

## 8. Reduced motion

Turning **Reduce motion** on makes every call instant: the tile lands on its slot at once, the sweep and intro are skipped, and the level swap is an instant change. The celebration is omitted entirely, and no effect flashes. The setting is saved in `user://settings.json` as `{"reduced_motion": bool}` and is read on launch.

## 9. Haptics, accessibility, icon and screenshots

- **Haptics:** none shipped. Any future haptic must be a single short tap on a completed row and must respect the system setting.
- **Accessibility checklist:** every tile has an accessibility name with its row, column and value; the empty slot is named "empty"; Undo and Restart remain visible; focus ring on every control; tapping only works on movable tiles; no timer.
- **Icon and screenshots:** keep the walnut background and the tan numbered tiles. Do not add new trade dress. Screenshots show the initial board, a legal intermediate move, the shake on an illegal tap, and the solved state.

## 10. QA checklist

1. Each tap moves exactly one tile into the gap, and the slide starts from the old slot and lands on the new one within 140 ms.
2. Reduce motion on: no tween runs on the moved tile after a tap, and the setting survives a relaunch.
3. The completed-row wave plays once per row, and the solved celebration runs once.
4. Every tile numeral is `tile_ink` on `tile` and passes 4.5:1; every label passes 4.5:1 on its surface.
5. Layout holds at 414 × 896 with no clipped labels, and the tray fits without horizontal scroll.
6. Undo and Restart reverse the slide visually and in state.
7. Headless tests pass: `test_rules`, `test_progress`, `test_scene`, `test_motion`.
