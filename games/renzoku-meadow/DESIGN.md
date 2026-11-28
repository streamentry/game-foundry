# Renzoku Meadow — visual and interaction design v0.2

## 1. Positioning and emotional promise

A calm, original consecutive Latin-square puzzle for competent adults. You are reading a small
meadow of number tiles and deciding, edge by edge, which neighbours are consecutive. The promise
is quiet focus: every tap answers at once, a conflict is explained rather than punished, and
nothing claims to train the mind. No age stereotypes, no health or cognitive claims.

**Three adjectives:** botanical · grounded · numeric.

**Signature interaction:** tap a square, then press a digit. Edge marks (`→•` / `↓•`) say
"these two differ by exactly one"; unmarked edges say "these do not". A rejected digit shakes its
tile and the status line names the contradiction.

## 2. Colour tokens

Derived from the icon: olive field, cream number tiles, gold bridges, dark charcoal digits.

| Token | Hex | Use |
| --- | --- | --- |
| `bg` | `#F6F1DE` | Frame background (cream paper) |
| `surface` | `#FBF6E7` | Editable tiles, raised cards |
| `surface_alt` | `#E6E9C9` | Given clues, secondary buttons |
| `ink` | `#262C24` | Digits, rules, body text, focus ring |
| `ink_muted` | `#4F5643` | Captions and secondary explanation |
| `accent` | `#5C6B25` | Selected tile edge, primary button fill, eyebrow |
| `accent_ink` | `#FFFFFF` | Text on `accent` |
| `success` | `#33601F` | Solved status text |
| `error` | `#9A3324` | Conflict status text |
| `focus` | `#262C24` | 3 px keyboard focus ring (plus 2 px `gold` halo) |
| `gold` | `#F2CC45` | Completion sweep tint and focus halo only; never text |

**WCAG contrast (computed):**

- `ink` on `surface` 13.24:1, on `bg` 12.64:1, on `surface_alt` 11.49:1 (all ≥ 4.5:1).
- `ink_muted` on `surface` 7.08:1, on `bg` 6.76:1, on `surface_alt` 6.14:1.
- `accent_ink` on `accent` 5.86:1.
- `accent` on `surface` 5.42:1 (eyebrow and preview text).
- `success` on `surface` 6.86:1; `error` on `surface` 6.77:1.
- `focus` (`ink`) on `accent` 2.44:1 only: the gold halo carries the non-text focus indicator on
  primary buttons. Re-measure on device before claiming 3:1.

**State is never colour-only:** selection uses a 3 px `accent` border plus the "Selected row …,
column …" sentence; conflict uses the word "Check …"; solved uses "Solved!"; given clues are
disabled and announced as such; an entered value is the digit itself, with no colour code.

## 3. Typography scale

Godot fallback face throughout (no bundled font, so the scale is in px at 414 × 896 logical).

| Role | Size | Weight | Notes |
| --- | --- | --- | --- |
| Eyebrow | 12 px | regular, `accent` | "RENZOKU / THE SPACES BETWEEN" |
| Title | 27 px | bold (default) | Puzzle headline |
| Heading | 16 px | regular | "PUZZLE n OF 6 • name" and preview |
| Body / instruction | 15 px | regular, `ink` | Rules paragraph, status line |
| Button label | 17 px | regular | Keypad, Undo, Erase, Restart, Next |
| Toggle label | 16 px | regular | "Reduce motion" |
| Cell digit | 21 px | regular | Board tiles (digit only) |
| Hint | 13 px | regular, `ink_muted` | Edge-bar legend |

## 4. Spacing and layout

- Base unit 4 px; column separation 12 px; keypad and action rows 7–8 px.
- Side margins 16 px; top 22 px; bottom 24 px. The whole column scrolls vertically; no
  horizontal scroll.
- Board: 4 × 4 grid, 3 px horizontal and 4 px vertical gutters, tiles 76 × 76 px.
- Minimum touch target **48 px**. Keypad, actions and the toggle are 54 px or 48 px high.

## 5. Components

- **Board cell:** `surface` fill, 1 px `EDGE` (`#BFC4A2`) border, 11 px corner radius, shadow
  `size 3, offset (0, 2)` in ink at 18 % alpha. Given clue: `surface_alt` fill, disabled.
  The tile shows only its digit; no glyphs.
- **Edge bars (overlay):** a 12 px gap separates tiles. Each dotted edge draws a rounded bar
  (7 px thick, 21 px long, 1 px darker outline) centred in its gap: a vertical bar between
  horizontal neighbours, a horizontal bar between vertical neighbours. Bar colour: `gold` while
  either tile is empty, `success` when both are filled and consecutive, `error` when both are
  filled and inconsistent. An undotted pair that is consecutive is drawn as an `error` bar.
- **Keypad (digits 1–4):** primary style (`accent` fill, `accent_ink` text), 54 px high.
- **Secondary buttons** (Undo, Erase, Restart, Previous, Copy challenge): `surface_alt` fill.
- **Primary Next:** `accent` fill.
- **Button states:**
  - normal: fill as above, 1 px edge, shadow;
  - hover: fill lerped 35 % toward white, 2 px `accent` edge;
  - pressed: fill darkened 8 %, 3 px `accent` edge, no shadow (reads as pressed-in);
  - focus: transparent fill, 3 px `ink` ring plus 2 px `gold` halo;
  - disabled: same fill, 1 px edge, no shadow, dimmed text colour.
- **Reduce motion toggle:** `CheckButton`, 48 px high, labelled "Reduce motion", placed above
  the action row.
- **Status line:** body text; `success` when solved, `error` when the last move is
  contradictory ("Check a row/column repeat …"), otherwise `ink`.

## 6. States

- **Idle:** board tiles `surface`; empty tiles show `·`.
- **Selected:** 3 px `accent` border; the preview sentence names row and column.
- **Related / peer highlight:** none. Edge marks already show the consecutive relation.
- **Conflict:** the tile shakes; the status line turns `error` and names the repeat or
  contradiction. The rejected digit is not written.
- **Completed group:** a row, column or the board sweeps with a gold tint (see motion table).
- **Solved:** the whole board sweeps once; status reads "Solved!" in `success`; Next unlocks.

## 7. Motion system

Timings come from the shared `scripts/motion.gd` module (copied unchanged).

| Name | Trigger | Duration (ms) | Easing | Property |
| --- | --- | --- | --- | --- |
| press | select a tile | 90 | back-out | `offset_transform_scale` 0.92 → 1 |
| pop | digit placed (value > 0) | 180 | back-out | `offset_transform_scale` (1.16, 0.90) → 1 |
| shake | rejected digit (conflict) | 260 | sine in-out | `offset_transform_position` x ±7 px, damped |
| sweep | row / column completes | 35 stagger, 260 | sine out / back-out | scale 1.08 → 1 and `self_modulate` gold → white |
| sweep (solved) | board completes | 35 stagger, 260 | as above | all 16 tiles, same tint |
| intro | first load and restart | 16 stagger (≤ 420), 220 | back-out | scale 0.86 → 1, y +10 → 0, alpha 0 → 1 |
| slide | next / previous level | 280 | cubic out | board x ±48 px and alpha |

**Signature moment:** the completed row or column lights up in gold, one tile at a time, as the
number is placed. A solved board is one gold wave across all 16 tiles and then the words
"Solved!" appear. There is no confetti and no particle burst: the root shared celebrate burst
is intentionally not used, because the puzzle's own spec forbids confetti.

## 8. Reduced motion

- The toggle writes `user://settings.json` (`{"reduced_motion": bool}`), read on the next start.
- With it on, every motion call applies its final state instantly. Shake, press, pop, intro,
  sweep and slide create no tweens. The gold tint is cleared at once.
- Nothing flashes at any setting; the largest tint is a single 260 ms pass.

## 9. Haptics and accessibility

- **Haptics:** none shipped, by design (no vibration).
- **Accessibility checklist:**
  - every tile has an accessibility name with row, column, value and consecutive neighbours;
  - keypad and actions are native `Button`s with focus;
  - the reduce-motion toggle is a native `CheckButton` with an accessible name;
  - status is text first, colour second.
- Not yet verified: screen-reader order on iOS/Android, 200 % text size, and gamepad reach on
  real devices. These are open items, not claims.

## 10. Icon, screenshots and QA checklist

- **Icon:** the existing `icon.png` (olive field, four cream tiles with charcoal digits, gold
  bridges). Its hues drive the palette above; keep it unchanged.
- **Screenshots (to capture):** initial board, a legal mid-solve state, a rejected entry
  (shake plus "Check" status), and the solved board.
- **QA checklist:**
  1. `test_rules`, `test_progress`, `test_scene` and `test_motion` exit 0 with no `SCRIPT ERROR`
     or `ERROR:` lines.
  2. With reduced motion on, a placed tile is at rest (scale 1) immediately.
  3. A rejected digit leaves the board unchanged and triggers exactly one shake.
  4. Completing a row sweeps only that row; solving sweeps all 16 tiles.
  5. Toggle off and on again persists across a relaunch.
  6. Focus ring visible on every button, including on `accent` fills.
  7. Layout holds at 320 px width and at 414 × 896 without horizontal scroll.
  8. Status colour changes never remove the word cue.

**Known business uncertainty:** the full negative-edge rule may surprise users familiar only
with positive-dot Kropki, which is another studio prototype. Real demand, age fit, replay,
competitor advantage, acquisition and monetisation are unmeasured. No automatic App Store
release.
