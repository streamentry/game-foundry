# Numberlink Paths — independent game design system (v0.2)

## 1. Positioning & emotional promise

A calm, exact Numberlink where every route has a clear reason to exist. The promise: you see the
two paths settle into place, and the board tells you plainly when something branches. No health,
memory or cognition claims, no timer, no streak, no purchase prompt.

**Three adjectives:** woven · directional · grounded.
**Signature interaction:** a route that closes seals itself with a light wave that runs along the
line from its first endpoint to its second, in path order, in that route's colour.

## 2. Colour tokens

Palette taken from the icon: deep teal ground and an orange route, on paper.

| Token | Hex | Use |
| --- | --- | --- |
| `bg` | `#FAF8F0` | Screen background (paper) |
| `surface` | `#FFFFFF` | Empty cells, high-contrast text on accent |
| `surface_alt` | `#DFEDEA` | Secondary buttons, Undo/Restart/Previous |
| `ink` | `#1D3F46` | Headings, body text, clue captions |
| `ink_muted` | `#4D6670` | Secondary captions (AA on bg and surface_alt) |
| `accent` | `#1E5F68` | Route A, primary button, eyebrow label |
| `accent_ink` | `#FFFFFF` | Text on accent and on route B |
| `success` | `#2D6A4F` | Solved celebration sparks (text reads "Solved!") |
| `error` | `#A23B2C` | Conflict outline and conflict warning text |
| `focus` | `#A6520E` | Keyboard focus ring (3 px, route B tone) |
| `route_b` | `#A6520E` | Route B fill (same as focus) |
| `border` | `#7A9493` | Idle cell and control outline (3.2:1 on white) |

Contrast (WCAG 2.x, computed):
- ink on surface **11.34:1**; ink on bg 10.67:1
- accent_ink on accent **7.26:1**
- accent_ink on route_b **5.48:1**
- error on surface **6.57:1**
- focus on bg **5.15:1**

State is never colour-only: route A and B carry letters A and B, endpoints carry "● END",
conflicts carry a warning sentence, and the solved state carries "Solved!".

## 3. Typography scale (px, system font, regular weight)

| Role | Size | Notes |
| --- | --- | --- |
| Eyebrow | 12 | Uppercase, accent |
| Title | 28 | Two-line max |
| Heading (puzzle name) | 16 | |
| Body / status | 15 | Line height left at engine default |
| Info / rules note | 14 | |
| Button label | 16 | |
| Cell digit / letter | 21 | Caption `● END` at same size |
| Fine print | 13 | |

## 4. Spacing & layout

- Base unit 4 px. Screen gutter 16 px left and right; column rhythm 12 px.
- Grid gap 4 px between cells. Cell side 76 px (4×4 board).
- Buttons 54 px tall minimum (above the 48 px target floor). Two-column action rows, 8 px gap.
- Scrolls vertically on short screens; no horizontal scroll. Checked at 414×896.

## 5. Components

- **Board cell (empty):** white surface, 1 px `border`, radius 12, shadow 4 px at 16 % ink,
  label `·`. Hover darkens fill 5 %; pressed darkens 12 % with no shadow.
- **Board cell (route A / B):** `accent` or `route_b` fill, white letter; endpoints add "● END".
- **Conflict cell:** the same fill, 3 px `error` outline.
- **Focus:** 3 px `focus` outline on any control, radius 12.
- **Primary button (Next puzzle):** `accent` fill, white label. States: normal, hover (5 % darker),
  pressed (12 % darker, flush), focus (3 px `focus` ring), disabled (surface_alt, muted label).
- **Secondary button (Undo, Restart, Previous, Copy):** `surface_alt` fill, `ink` label, same states.
- **Reduce-motion switch:** a `CheckButton` labelled "Reduce motion", 54 px tall, in the controls.
- **Panels:** no card panels; text sits on paper. **Status banner:** the status line, 15 px.
  Neutral `ink` normally; `error` while a route branches.

## 6. States

- **Idle:** empty cell, `border` outline.
- **Selected:** the tapped cell presses in (press dip) before its mark changes.
- **Related / peer highlight:** none; the two routes are distinguished by letters and colour.
- **Conflict:** the cell whose degree exceeds its allowance gets the `error` outline and the status
  line turns `error`. Pure rule check (`Rules.branches_at`), no input is blocked.
- **Completed group (a route closed):** the route's cells sweep in path order.
- **Solved:** every cell sweeps, then a celebration of 22 soft pieces from the board centre.

## 7. Motion system

Shared timings from the motion module. The signature moment is the route seal.

| Name | Trigger | Duration (ms) | Easing | Property |
| --- | --- | --- | --- | --- |
| press | tap a blank cell | 90 | back, ease-out | scale 0.92 → 1 |
| pop | a mark is placed | 180 | back, ease-out | scale overshoot to 1.16 × 0.90, settles at 1 |
| shake | placed cell branches | 260 | sine, ease-in-out | position x, ±7 px damped |
| stagger_in | level load, restart | 16 ms per cell (max 420) + 220 | back / cubic | scale 0.86 → 1, y +10 → 0, alpha 0 → 1 |
| sweep (route seal) | route closes | 35 ms per cell + 260 | sine / back | scale 1.08 → 1, self-modulate glow → white, cells in path order |
| sweep (solved) | board solved | 35 ms per cell + 260 | sine / back | same, all cells in reading order |
| slide_swap | next / previous | 280 | cubic, ease-out | board x ±48 px, alpha 0 → 1 |
| celebrate | board solved | ≤ 1100 | cubic, ease-out | 22 pieces, position, rotation, alpha |

Game state updates first; every animation only follows it.

## 8. Reduced motion

With "Reduce motion" on, every call above applies its final state at once: no tweens, no shake, no
sweep glow, and no celebration pieces. Nothing flashes at any setting. The setting is saved to
`user://settings.json` as `{"reduced_motion": bool}`, separate from progress.

## 9. Haptics, accessibility, icon & screenshots

- **Haptics:** none shipped; no intent to add without opt-in testing.
- **Accessibility checklist:** 54 px targets; each cell has a row/column accessibility label with
  its fixed/editable state; colour is always paired with a letter, outline or sentence; focus ring
  on every control; reduce-motion switch; no timer. Still unverified: VoiceOver/TalkBack and 200 %
  text on real devices.
- **Icon:** teal ground, orange route, cream route (the existing icon, unchanged). Palette chosen to
  match its family of hues.
- **Screenshots:** the board at rest with paper background; a midway board with routes A and B; a
  branch conflict showing the error outline and sentence; and a completed partition.

## 10. QA checklist

1. `test_rules`, `test_progress`, `test_scene`, `test_motion` all exit 0 with no `ERROR:` output.
2. Reduce motion on: placing a mark leaves no tween on the cell.
3. Reduce motion off: a mark overshoots and settles at scale 1 within 0.6 s.
4. A branch shows the error outline and the warning sentence together.
5. Closing a route sweeps its cells in path order, not reading order.
6. Next / previous slide the board; the level state is correct before the slide ends.
7. Screenshots at 414×896 show no clipped text and no overlap; the reduce-motion switch is reachable
   by scrolling.
