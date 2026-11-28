# Kakuro Compass — design specification (v0.2, upgrade pass)

## 1. Positioning, promise, signature

Kakuro Compass is a quiet crossing-sums puzzle: the two clues on each white square are always
readable, so the player reasons with the numbers rather than against the screen. The promise is
an honest puzzle, with no hidden answer filled in and no timer, and it respects an adult's
competence. No health, memory or cognitive-benefit claim is made.

- **Three adjectives:** navigable · scholarly · reassuring.
- **Signature interaction:** tap a white cell and its two crossing sums light up as a "compass"
  on the same card, with the digits that can still fit each run. The selected run's peers are
  tinted, so the eye follows the sum, not the grid.
- **Icon link:** the icon is a navy field, cream cells and one gold compass star, with a navy
  clue cell holding cream numerals. The in-game palette reuses that exact family: navy clue
  tiles with cream numerals, cream open cells, and gold as the single warm accent.

## 2. Color tokens

| Token | Hex | Use |
| --- | --- | --- |
| `bg` | `#F2E9D2` | Page background (cream, close to the icon's cells) |
| `surface` | `#FBF6E8` | Open (white) board cells, inspector card |
| `surface_alt` | `#E6D9B8` | Secondary buttons, pale controls |
| `ink` | `#0B1A3B` | Primary text, entered digits, navy focus ring (icon navy) |
| `ink_muted` | `#4B4F63` | Helper text, captions |
| `accent` | `#C9A24B` | Primary button fill, progress glow (icon gold) |
| `accent_ink` | `#0B1A3B` | Text on the accent fill |
| `clue` | `#0E2550` | Non-editable clue tiles (icon navy) |
| `clue_ink` | `#F6EDD2` | Clue sums on the navy tile |
| `selected` | `#F4DE95` | Active cell fill (gold tint), with a 3 px navy border |
| `peer` | `#EDE3C8` | Cells in the selected cell's across or down run |
| `success` | `#2F6B4A` | Solved status text, always with a check mark |
| `error` | `#9C2F2A` | Conflict status text, always with a "!" prefix |
| `focus` | `#0B1A3B` | 3 px focus ring on every control |

**Contrast (WCAG 2.x, computed):**

- `ink` on `bg` 14.17:1; `ink` on `surface` 15.88:1
- `ink_muted` on `surface` 7.49:1
- `accent_ink` on `accent` 7.14:1
- `clue_ink` on `clue` 12.84:1
- `error` on `surface` 6.82:1; `success` on `surface` 5.85:1
- `focus` on `surface` 15.88:1 (non-text 3:1 met)

The gold `accent` is never used as a border or icon on cream (2.22:1, below 3:1). State is
never color-only: a selected cell has a thicker border, conflicts say "!" and solved says
"All crossing sums…", and completed runs are announced in text.

## 3. Typography

| Role | Face | Size px | Weight |
| --- | --- | --- | --- |
| Kicker | system sans | 12 | semibold, tracking via caps |
| Title | serif (Georgia, Times New Roman fallback) | 29 | bold |
| Heading / inspector title | serif | 17 | bold |
| Body / helper | system sans | 15 | regular |
| Caption / legal line | system sans | 13 | regular |
| Button label | system sans | 17 | semibold |
| Cell digit (entry) | serif | 24 | bold |
| Clue sum in tile | serif | 15 | semibold, two lines |

The serif is a deliberate choice: its numerals echo the icon's "7" and "4" and read as
printed-puzzle, not as default UI.

## 4. Spacing and layout

- Base unit 4 px; section rhythm 10 px; page gutters 16 px horizontal, 17 px top, 25 px bottom.
- Board gap 3 px; cell 69 × 69 logical px on a 414 × 896 base (scroll on short screens).
- Keypad gap 7 px; action row gap 8 px.
- Minimum touch target 48 px; all buttons are 52 px tall.
- Corner radius: 12 px for buttons and panels, 7 px for board cells (cells stay flatter than
  controls so the grid reads as paper).
- Shadows: a 4 px soft shadow under the inspector card only; no shadows on the grid.

## 5. Components

- **Board cell (open):** `surface` fill, 1 px `#A7B3BD`-family hairline border; entered digit in
  `ink` serif. Normal, hover (`#F6F0DE` fill), pressed (`selected`), focus (3 px navy ring),
  disabled not used (open cells are always selectable).
- **Selected cell:** `selected` fill, 3 px navy border.
- **Peer cell:** `peer` fill; applies to the selected cell's across and down runs.
- **Clue tile (non-editable):** `clue` fill, `clue_ink` text, no border; the sums stacked as
  "↓ n" over "→ n".
- **Keypad digit:** primary style: `accent` fill, `accent_ink` text. Hover `#D9B25E`, pressed
  `#B8903A`, focus 3 px navy ring, disabled `surface_alt` with `ink_muted` text.
- **Secondary button** (Undo, Erase, Restart, Previous, Copy): `surface_alt` fill, `ink` text.
  Hover `#DCCBA6`, pressed `#CDB98E`, focus 3 px navy ring, disabled 45 % opacity.
- **Reduce-motion toggle:** a `CheckButton` labelled "Reduce motion", in the action area, with
  `ink` text and the same focus ring.
- **Inspector card:** `surface`, 12 px radius, 4 px shadow; holds the across, down and both-sums
  lines.
- **Status banner:** the feedback label. Neutral text in `ink_muted`; conflict in `error` with a
  "!" prefix; solved in `success` with a check mark.

## 6. States

- **Idle:** first open cell selected, all others `surface`.
- **Selected:** gold tint, thick navy border; inspector shows its runs.
- **Related / peer:** `peer` fill on the cells of the selected across and down runs.
- **Conflict:** the cell's run text turns `error` with "!"; the selected cell shakes once.
- **Completed group:** a run whose cells are all filled and sum correctly gets a gold sweep
  across its cells and the status line says "Across N complete" or "Down N complete".
- **Solved:** all runs complete, banner "All crossing sums agree. You solved this puzzle!",
  board sweep, and one celebration burst.

## 7. Motion system

Shared timings from `scripts/motion.gd` (unchanged template).

| Name | Trigger | Duration ms | Easing | Property |
| --- | --- | --- | --- | --- |
| press | select a white cell | 90 | back-out | offset scale 0.92 → 1 |
| pop | digit placed, changed or erased | 180 | back-out | offset scale 1.16 × 0.90 → 1 (squash) |
| shake | placement conflicts with a run | 260 | sine in-out | offset position x, damped |
| intro | level load / restart | 16 stagger, 220 | back / cubic out | offset scale 0.86 → 1, y +10 → 0, alpha |
| sweep | a run completes; solved sweeps all | 35 stagger, 260 | sine / back out | offset scale 1.08 → 1, self modulate glow |
| slide | next / previous board | 280 | cubic out | offset x ±48 → 0, alpha |
| celebrate | solved | ≤ 1100 | cubic out | up to 22 soft tiles, then freed |

**Signature moment:** the completed run's gold sweep travels along its cells in reading order,
as if the sum "lights up" on the compass. Solved adds the restrained burst from the centre of
the board; nothing flashes.

## 8. Reduced motion

- Every call becomes instant: `motion.reduced_motion = true` applies the final state
  synchronously, creates no tweens.
- The celebration burst is omitted entirely; solved shows the banner only.
- Conflicts still show the "!" text and the `error` color; no shake.
- The setting persists in `user://settings.json` as `{"reduced_motion": bool}` and applies on
  launch.
- No flashing anywhere (WCAG 2.3.1 is respected; no full-screen or strobing effect exists).

## 9. Haptics, accessibility, icon and screenshots

- **Haptics:** none shipped. Intent: an optional light tick on conflict only, if a player asks.
- **Sound:** none shipped.
- **Accessibility checklist:** each cell has a text label with row, column and digit; each clue
  has direction and target; focus rings on every control; keyboard digits and Backspace; no
  timers; contrast figures in section 2; reduce-motion toggle; text sized to 15 px minimum for
  body.
- **Icon:** keep the existing icon; it is the palette reference (navy, cream, gold).
- **Screenshots:** untouched board showing the selected cell and both run inspectors; a
  conflict shot showing "!" text; a completed run with its gold sweep; and the solved board.
  Captured with `tools/store_catalog/capture.gd`.

## 10. QA checklist

1. Each cell is tappable at 48 px or more on a 414 × 896 base, and nothing clips at 320 px width.
2. Focus ring is visible on every control, and the keypad reaches every digit with the keyboard.
3. Every state is readable without color: selected (border), conflict ("!"), solved (text).
4. Reduce motion on: placing a digit shows the final value with no tween; `Motion.running()` is 0.
5. Reduce motion off: a placement returns to rest within about 0.6 s; no offset drift.
6. Undo, erase, restart and board navigation still update state immediately during animation.
7. Solved board: the celebration frees all its pieces and the status line is correct.
8. Existing rule, progress and scene tests stay green.
