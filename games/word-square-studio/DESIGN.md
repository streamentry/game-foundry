# Word Square Studio — visual and interaction system v0.2

## 1. Positioning and emotional promise

A calm, confident word puzzle for competent adults: three three-letter words stacked into a square
that reads the same across and down. The promise is a clean "it fits" moment, earned by the
player's own reasoning, never by a timer or a penalty. The look is the app icon brought in-game:
a royal purple field, cream letter tiles, and one gold diagonal that marks a finished square.

- **Three adjectives:** clear · regal · unhurried
- **Signature interaction:** the 3×3 letter board fills row by row as each word is chosen, and
  the solved square lights its diagonal in gold, the same diagonal as the icon.

No health, memory or cognition claims. No age-coded styling: the palette is chosen for the icon,
not for a "senior" look.

## 2. Color tokens

| Token | Hex | Use |
| --- | --- | --- |
| `bg` | `#3B1D6B` | Screen field (royal purple, icon midtone) |
| `surface` | `#2A1252` | Disabled button fill, deepest panel |
| `surface_alt` | `#4E2A86` | Secondary buttons, empty board cells |
| `ink` | `#FBF5E6` | Primary text on purple (cream) |
| `ink_muted` | `#DCCFF0` | Secondary text, empty cell outline |
| `tile` | `#FBF5E6` | Filled letter tile (icon cream) |
| `tile_edge` | `#D9CFB8` | Filled tile outline |
| `tile_ink` | `#2E1656` | Letters on cream tiles and gold tiles |
| `accent` | `#ECB84C` | Gold: primary buttons, solved diagonal, kicker |
| `accent_ink` | `#2E1656` | Text on gold |
| `success` | `#A8E6B8` | Completion particles (paired with the words "Square complete!") |
| `error` | `#FFB0A8` | Reserved for the conflict text cue (paired with words) |
| `focus` | `#FFE08A` | 3 px focus ring |

**WCAG contrast ratios (computed):**

| Pair | Ratio | Required |
| --- | --- | --- |
| `ink` on `surface` | 14.77 : 1 | 4.5 |
| `ink` on `bg` | 12.27 : 1 | 4.5 |
| `ink` on `surface_alt` | 9.66 : 1 | 4.5 |
| `ink_muted` on `bg` | 9.04 : 1 | 4.5 |
| `tile_ink` on `tile` | 14.14 : 1 | 4.5 |
| `accent_ink` on `accent` | 8.44 : 1 | 4.5 |
| `error` on `surface` | 9.20 : 1 | 4.5 |
| `success` on `surface` | 11.23 : 1 | 4.5 |
| `focus` on `bg` | 10.34 : 1 | 3 (non-text) |

State is never color-only: a filled tile carries its letter, the solved diagonal is paired with
the status sentence "Square complete!", placed rows are listed as text, and the focus ring is a
border shape.

## 3. Typography

| Role | Size | Weight | Notes |
| --- | --- | --- | --- |
| Kicker | 12 px | regular | Gold, uppercase |
| Title | 28 px | regular | Screen headline |
| Body / instructions | 15 px | regular | Line height from theme |
| Heading (challenge) | 16 px | regular | |
| Row labels | 23 px | regular | Text list for screen readers and recovery |
| Button label | 19 px | regular | Buttons are 58 px tall |
| Toggle label | 17 px | regular | |
| Tile letter | 60 px | regular (default face) | Tiles are 104 × 104 px; letters are the largest type on screen |

Godot's default face is used for all text. No second family is needed for this prototype.

## 4. Spacing and layout

- **Base unit:** 8 px. Column rhythm 13 px; row gaps 8 px.
- **Safe margins:** 20 px left and right, 24 px top and bottom, inside a scroll container so the
  full stack is reachable on 320 px-tall content.
- **Board:** 3 × 3 grid, 10 px gaps, tiles 104 px. Board width 332 px fits the 374 px content
  width at 414 px.
- **Touch targets:** every button is 58 px tall (minimum 48 px). The motion toggle row is a full
  `CheckButton` row.

## 5. Components

- **Letter tile (board cell):** 104 × 104 px `Panel`, 14 px corner radius, 2 px border.
  - Empty: `surface_alt` fill, `ink_muted` outline (the cell is always visible).
  - Filled: `tile` fill, `tile_edge` outline, 6 px soft shadow, `tile_ink` letter at 60 px.
  - Solved diagonal: `accent` fill, `accent_pressed` outline.
- **Word cards (choice buttons):** primary style, gold, `accent_ink` text. One per word, shown
  only while that word is still legal. Tap to place it in the next empty row.
- **Primary button (Next):** gold. Normal / hover `#F4C863` / pressed `#D39B33` / disabled
  `surface` with `ink_muted` text. Focus: 3 px `focus` ring.
- **Secondary buttons (Undo, Restart, Previous, Copy challenge):** `surface_alt` fill with `ink`
  text. Hover `#5E36A0`, pressed `surface`, disabled `surface` with `ink_muted` text.
- **Panels:** the screen field is a single `bg` rect; no nested cards.
- **Status banner:** plain text under the word cards. It always states the next action or the
  completion, for example "Arrange the three words: every column must match its row."
- **Motion toggle:** `CheckButton` labelled "Reduce motion" in the controls area, saved to
  `user://settings.json`.

## 6. States

- **Idle:** all tiles cream or empty; word cards gold; no diagonal.
- **Selected (word card tapped):** press dip (90 ms); the next empty row fills and its tiles pop.
- **Related / peer highlight:** none; a word already used is hidden, not dimmed, so no row
  duplicates appear.
- **Conflict / invalid:** the board shakes once and the status text stays, so the cue is motion
  plus words.
- **Completed group:** a placed row pops; the whole board sweeps in reading order on solve.
- **Solved:** the diagonal (cells 1, 5, 9) turns gold, the status says "Square complete!",
  and a restrained burst of ≤22 pieces plays once.
- **Disabled:** Next is disabled until the level is solved; Previous on level 1; Undo when no
  history.

## 7. Motion system

Timings come from the shared `motion.gd` module and are not changed.

| Name | Trigger | Duration | Easing | Property |
| --- | --- | --- | --- | --- |
| Press | Word card tapped | 90 ms | back, ease-out | `offset_transform_scale` 0.92 → 1 |
| Pop | Row placed (each of its 3 tiles) | 180 ms | back, ease-out | `offset_transform_scale` 1.16/0.90 → 1 |
| Shake | Rejected input | 260 ms | sine, ease-in-out | `offset_transform_position` x, damped |
| Stagger in | Level load and Restart | 16 ms stagger, 220 ms each | back / cubic | scale, position, alpha |
| Sweep | Solved (all 9 cells, reading order) | 35 ms stagger, 260 ms | sine / back | scale 1.08 → 1 and `self_modulate` glow |
| Slide | Next / Previous | 280 ms | cubic, ease-out | board `offset_transform_position` x ±48, alpha |
| Celebrate | Solved | ≤1100 ms, ≤22 pieces | cubic, ease-out | position, rotation, alpha; freed afterwards |

**Signature moment:** the solved square. The board sweeps as one wave, the diagonal turns gold,
and a short burst of cream and gold pieces leaves the centre of the board. It ends on the rest
state. The burst never repeats and never flashes.

## 8. Reduced motion

When "Reduce motion" is on, every call in `motion.gd` applies the final state at once:

- placement, shake, stagger, slide and sweep are instant;
- the celebration burst is omitted;
- the gold diagonal and "Square complete!" text still appear, because they are state, not motion;
- nothing flashes at any setting.

The setting is read on launch and written when toggled.

## 9. Haptics and accessibility

- **Haptics:** none shipped. Intent: at most a light tap on solve, and only if the platform
  offers it, with the feature off under reduced motion.
- **Sound:** none.
- **Accessibility checklist:**
  - every tile letter is 60 px and high-contrast;
  - word cards and buttons are 58 px tall and reachable by keyboard focus (3 px ring);
  - row labels repeat the board as text for screen readers;
  - no colour-only cue; the diagonal is paired with text;
  - Reduce motion is visible in the controls, not buried;
  - no timers, no penalties.
  Real device audits with adults 40+ and VoiceOver/TalkBack are still required; headless CI
  does not validate taste or legibility.

## 10. Icon and screenshots

- **Icon:** royal purple field, three cream tiles with dark purple letters, the gold diagonal
  (A, U, N) as the only warm accent. Keep the icon and in-game palette in the same family.
- **Screenshots:** the initial board (empty cells visible), a board with one row placed, the
  solved square with the gold diagonal, and the Reduce motion toggle. Capture on a real phone
  before any store submission.

## QA checklist

1. All 9 grid cells are visible and on screen after the intro (asserted in `tests/test_motion.gd`).
2. Reduce motion on: placement leaves every cell at rest scale with no running tweens.
3. Reduce motion off: after 0.6 s every cell returns to rest.
4. Solve: diagonal cells 1, 5, 9 are gold; off-diagonal stay cream; celebration pieces are freed.
5. Each tile letter reads at 60 px on a 414 × 896 screen; no clipped letters.
6. Every control keeps a 58 px target and a visible focus ring with keyboard navigation.
7. The palette matches the icon in the store listing and on the launcher.
8. Rules, levels, solutions and save semantics are unchanged (test_rules, test_progress pass).
