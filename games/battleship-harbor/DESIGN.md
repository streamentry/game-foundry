# Battleship Harbor — design system (v0.2, store-catalog upgrade)

## 1. Positioning and emotional promise

A quiet fleet-logic chart for a player who likes exact rules and a calm pace. The emotional promise is "the harbor is making sense": each ship you place is a clear, reversible decision, and a finished row or column reads as a small, satisfying settle, not a reward for speed. No timer, no streak, no health or cognition claims, and no age stereotypes.

**Three adjectives:** nautical · exact · unhurried.

**Signature interaction:** the chart. Ship marks sit on a navy grid that mirrors the app icon's chart. Placing a ship pops its coral square; completing a row or column sweeps a soft mint wave across that line and its counter; solving the chart sends a short, restrained burst of harbor-coloured tiles from the board's centre.

## 2. Color tokens

| Token | Hex | Use |
| --- | --- | --- |
| `bg` | `#EEF4F6` | Page background (pale sea mist) |
| `surface` | `#FFFFFF` | Empty editable square, cards |
| `surface_alt` | `#DCE8ED` | Water note square, disabled water given |
| `chart` | `#0B2545` | Chart frame and grid gaps (same navy as icon background) |
| `ink` | `#0B2545` | Body text, headings, numbered clues on `bg` |
| `ink_muted` | `#3D5A73` | Captions, helper copy |
| `clue` | `#EEF4F6` | Row and column totals drawn on `chart` |
| `accent` | `#1F5F8B` | Primary buttons (white label), progress eyebrow |
| `accent_ink` | `#FFFFFF` | Text on `accent` |
| `ship` | `#F0786F` | Ship squares (coral from the icon); label is `ink` |
| `success` | `#2E6F5E` | Solved banner text |
| `error` | `#B3372F` | Rule-violation text |
| `focus` | `#0B2545` | 3 px focus ring on controls over `bg` |
| `focus_chart` | `#F2C879` | 3 px focus ring on squares over `chart` (warm amber) |
| `mint` | `#6BBFBB` | Completion sweep glow, decorative rule |

**WCAG 2.x contrast (computed):**

| Pair | Ratio | Result |
| --- | --- | --- |
| `ink` on `surface` | 15.39:1 | AAA |
| `ink` on `bg` | 13.85:1 | AAA |
| `ink` on `surface_alt` | 12.32:1 | AAA |
| `accent_ink` on `accent` | 6.84:1 | AA |
| `ink` on `ship` | 5.58:1 | AA |
| `error` on `surface` | 5.99:1 | AA |
| `error` on `bg` | 5.40:1 | AA |
| `success` on `surface` | 5.91:1 | AA |
| `clue` on `chart` | 13.85:1 | AAA |
| `focus` on `bg` (non-text) | 13.85:1 | exceeds 3:1 |
| `focus_chart` on `chart` (non-text) | 9.75:1 | exceeds 3:1 |

**State is never colour-only.** Ship squares carry the word "SHIP"; water notes carry "WATER"; a completed line gets a text counter that matches its target; errors are written in words ("Row 2 has too many ship cells"); a solved board says "Fleet found". The mint glow is an extra cue, never the only one.

## 3. Typography scale

System font (no bundled font, so the layout stays legible at 414 × 896 and under Dynamic Type-equivalent sizing).

| Role | Size | Weight | Color |
| --- | --- | --- | --- |
| Eyebrow (`BATTLESHIP HARBOR / FLEET LOGIC`) | 12 px | bold | `accent` |
| Title (`Find the hidden fleet.`) | 28 px | bold | `ink` |
| Heading (puzzle name line) | 16 px | semibold | `ink` |
| Body / rule text | 15 px | regular | `ink` |
| Caption / helper | 13–14 px | regular | `ink_muted` |
| Button label | 16 px | semibold | per button |
| Row/column clue | 14 px | bold | `clue` |
| Cell caption (`SHIP` / `WATER`) | 17 px | bold | `ink` |

## 4. Spacing and layout

- Base unit **4 px**; rhythm between blocks **12 px**; page gutters **16 px** left and right, 20 px top, 24 px bottom.
- Cell grid gap **4 px** on the navy chart frame; chart frame padding **10 px**, corner radius **16 px**.
- Minimum touch target **48 px**; cells are **53 × 53 px**; buttons **54 px** tall.
- The page scrolls vertically on short screens; no control shrinks below its minimum.
- Safe area: content stays inside the scroll container's margins on notched phones.

## 5. Components

- **Board cell (ship / water / empty):** rounded 10 px. Empty = `surface`, water = `surface_alt` with `WATER`, ship = `ship` with `SHIP`. Clue cells draw the total on the navy chart, not in a cell.
- **Given cell:** disabled, same fill as its state, labelled, never editable. Stays fully legible (no opacity fade).
- **Row/column clue:** `clue` text on `chart`; shows `current/target`. Completed lines keep the text and gain a `✓`-free sweep only (no checkmark glyph added, so colour and text carry the state).
- **Primary button** (Next puzzle, keypad): `accent` fill, `accent_ink` label, radius 12, raised shadow.
  - normal: fill `accent`, shadow 4 px navy at 22%
  - hover: fill lightened 8 %, shadow 6 px
  - pressed: fill darkened 12 %, shadow removed, 2 px down
  - focus: 3 px `focus` ring outside the fill
  - disabled: fill unchanged, label `ink_muted`-equivalent, no shadow
- **Secondary button** (Undo, Restart, Previous, Copy): `surface_alt` fill, `ink` label, raised shadow, same five states.
- **Reduce-motion control:** a `CheckButton` labelled "Reduce motion", 48 px tall, `ink` label, placed under the action row.
- **Panels:** the chart is the only raised surface, with a navy frame and no extra card behind it.
- **Status banner:** plain text under the chart, 15 px. Error text uses `error`; solved text uses `success`; neutral text uses `ink`.

## 6. States

| State | Visual | Text |
| --- | --- | --- |
| Idle empty | white square | none |
| Selected / tapped | 53 px square, press dip (see motion) | none |
| Related (row/column of a ship) | none (kept deliberately quiet) | counter shows current total |
| Conflict | shake on the tapped square | status: "Row N has too many ship cells." |
| Completed row / column | sweep across that line | counter text reads `target/target` |
| Solved | sweep on all squares, celebration burst | "Fleet found: one ship of 3, one of 2, one of 1." |

## 7. Motion system

Shared timings from `scripts/motion.gd` (unchanged template).

| Name | Trigger | Duration (ms) | Easing | Property |
| --- | --- | --- | --- | --- |
| press | tap on a cell | 90 | back / out | `offset_transform_scale` 0.92 → 1 |
| pop | cell value changed | 180 | back / out | `offset_transform_scale` 1.16×0.90 → 1 |
| shake | placing a ship that exceeds a row/column total | 260 | sine / in-out | `offset_transform_position` x |
| sweep | row, column or full solve, cells staggered | 35 ms stagger, 260 | sine / out, back / out | scale 1.08 → 1, `self_modulate` to mint and back |
| intro | level load or restart, cells in reading order | 16 ms stagger, 220 | back / out, cubic / out | scale 0.86 → 1, y +10 → 0, alpha |
| slide | next or previous puzzle | 280 | cubic / out | board x ±48 → 0, alpha |
| celebrate | solve, once | ≤ 1100 | cubic / out | 22 pieces, position, alpha out |

**Game-specific signature moment:** the sweep across a completed line. It is the one moment that needs no rule explanation: a line lights up and its counter reads `target/target`.

## 8. Reduced motion

- A visible "Reduce motion" switch saves to `user://settings.json` as `{"reduced_motion": bool}`, and applies on load and on toggle.
- With it on, every motion call applies its final state instantly: no press dip, no pop, no shake, no intro stagger, no sweep, no slide. The celebration is omitted entirely.
- No flashing at any setting. The mint glow is a single tint, not a blink. The celebration never repeats.

## 9. Haptics, accessibility and icon

- **Haptics:** none shipped. Any future haptic must be opt-in and paired with the same text cue.
- **Accessibility checklist:**
  - every square has an accessibility name with row, column, state (`fixed` or `editable`) and text state;
  - the reduce-motion switch is reachable by keyboard and gamepad (native `CheckButton`);
  - all text is at least 13 px and contrast-checked above;
  - no timer; no information lives only in colour;
  - real-device VoiceOver and TalkBack checks remain **not run**.
- **Icon and screenshot:** the icon is the source for the palette (navy chart, coral ships, mint accent). Screenshots should show an empty chart with totals, a completed row sweep, and a solved board. No real-phone screenshots have been taken for this upgrade.

## 10. QA checklist

1. Tapping a square changes state through ship → water → empty, and the text label matches.
2. Placing a ship that exceeds a row or column total shakes the square and shows the existing "too many" text.
3. A completed row or column sweeps once and its counter reads `target/target`; it does not sweep again on every later tap.
4. Restart and level change play the intro or slide, and the board ends at rest with alpha 1 and scale 1.
5. With reduced motion on, every effect is instant and no celebration plays.
6. The reduce-motion choice survives a relaunch.
7. Text and focus contrast match the ratios above; focus is visible on the chart and on controls.
8. Layout holds at 414 × 896 and at 320 px width with no horizontal scroll.
