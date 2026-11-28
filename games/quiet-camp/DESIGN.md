# Quiet Camp — design system (v0.2 playtest, motion pass)

## 1. Positioning & emotional promise

A quiet Tents & Trees clearing where every pine has its own tent and nothing rushes you. The
promise is calm, checkable order: you can see why each tent belongs, and every change answers
at once. No health, memory-improvement or age-stereotype claims.

**Three adjectives:** woodland · clear · deliberate.

**Signature interaction:** a tap cycles a square tent → grass → blank, and the tent *settles*
with a small squash. When a row or column reaches its target count, that line gets a soft
light sweep, so the player sees progress without a number changing alone.

## 2. Color tokens

| Token | Hex | Use |
| --- | --- | --- |
| `bg` | `#F4F8EF` | Page paper behind everything |
| `surface` | `#FDFBF3` | Blank playable grass cells, celebration tiles |
| `surface_alt` | `#E1ECDE` | Secondary buttons (light moss) |
| `ink` | `#273F37` | Headings, body text, rule notes, cell outline on focus-free states |
| `ink_muted` | `#52665D` | Supporting text, disabled labels |
| `accent` | `#2E6955` | Eyebrow, matched counts, primary button, focus ring |
| `accent_ink` | `#FFFFFF` | Text on primary (accent) buttons |
| `success` | `#2E6955` | Completed-status line (same hue as accent, paired with the word "complete") |
| `error` | `#9A5244` | Violation status copy (paired with a text message) |
| `focus` | `#2E6955` | 3 px ring drawn outside the control |
| `tent` | `#F4DCCB` (tile), `#CB7957` (canvas) | Occupied cell and drawn tent |
| `tree` | `#CCE0D1` (tile), `#2E6955` / `#78A489` (canopy) | Fixed trees |

Hover/pressed tints are derived, not separate brand colors: primary hover `#245341`, primary
pressed `#1C4236`; secondary hover `#CFDDCA`, pressed `#BFD2BC`; disabled fill `#DCE4D9`; cell
hover `#F7F4E6`, pressed `#EEEAD8`; tent hover `#EFCDB8`, pressed `#E9BFA5`.

**Contrast (WCAG 2.x, computed):**

| Pair | Ratio | Pass |
| --- | --- | --- |
| ink on surface `#273F37` / `#FDFBF3` | 10.94 : 1 | AA |
| accent_ink on accent `#FFFFFF` / `#2E6955` | 6.43 : 1 | AA |
| error on surface `#9A5244` / `#FDFBF3` | 5.53 : 1 | AA |
| ink_muted on bg `#52665D` / `#F4F8EF` | 5.71 : 1 | AA |
| disabled text `#52665D` on `#DCE4D9` | 4.72 : 1 | AA |
| focus ring accent on bg | 5.97 : 1 | AA (non-text 3:1) |

State is never color-only: a tent is a drawn triangle, grass is a three-tuft mark, a fixed tree
is a drawn pine with a trunk, and every line count is printed. Matched counts also change text
color, but the status line says "Camp complete" or names the conflict in words.

## 3. Typography scale

System face (no bundled fonts).

| Role | Size | Weight | Use |
| --- | --- | --- | --- |
| Eyebrow | 12 px | regular, uppercase | "QUIET CAMP / TENTS AND TREES" |
| Title | 27 px | regular | "Find a place for every tent." |
| Heading | 15 px | regular | Clearing name line |
| Body | 15 px | regular | Instruction paragraph, status line |
| Feedback | 16 px | regular | Progress totals line, button labels |
| Caption / rule note | 13–14 px | regular | Footnotes |
| Cell digit (counts) | 19 px | regular | Row and column counts |

## 4. Spacing & layout

- Base unit 4 px; vertical rhythm 11 px between blocks.
- Outer side gutter 16 px; top 20 px; bottom 26 px. No horizontal scroll at 320 or 414 px.
- Board cell gutter 3 px. 5 × 5 cells are 56 px; 6 × 6 cells are 49 px.
- Minimum touch target 48 px; buttons, toggle and navigation are 54 px tall.
- Button row gap 8 px. Board is centered in its own container.

## 5. Components

- **Board cell** (`CampCell`, a `Button`): 10 px rounded rect, 1 px `#A7C0AB` border. Blank
  grass uses `surface`; tent uses `tent` tile; fixed tree uses the tree tile and is disabled.
- **Count labels**: 19 px digits in a fixed square; accent when satisfied, ink otherwise.
- **Primary button** ("Next clearing"): accent fill, white text, 12 px radius, 4 px soft shadow
  that drops to 0 when pressed. Hover `#245341`, pressed `#1C4236`, disabled uses the flat
  disabled fill.
- **Secondary button** (Undo, Restart, Previous, Copy): `surface_alt` fill, ink text, same
  radius and shadow. Hover `#CFDDCA`, pressed `#BFD2BC`.
- **Reduce motion toggle** (`CheckButton`): 54 px tall, in the controls area under Undo and
  Restart, ink text.
- **Focus** (all controls): 3 px accent ring drawn 3 px outside the control, no fill.
- **Status banner**: one line under the totals; muted while open, accent with "Camp complete!"
  when solved.

## 6. States

- **Idle**: flat paper cells, no outline.
- **Hover** (desktop/pointer): one step darker tint; no movement.
- **Pressed / selected**: a 90 ms dip to 0.92 scale on the tapped cell.
- **Related / peer highlight**: none in v0.2 (the line sweep is the only group feedback).
- **Conflict**: the tapped tent cell shakes once, and the existing text cue ("Two tents touch",
  "Too many tents in row n") stays on screen.
- **Completed group**: a row or column whose count has just reached its target gets a 35 ms
  stagger light sweep across its cells.
- **Solved**: every cell gets the sweep, then a soft burst of 22 small tiles in accent, forest
  pale and paper colors. The burst is skipped when reduced motion is on.

## 7. Motion system

Shared timings from `scripts/motion.gd` (copied from the store-catalog template). All motion is
visual only and goes through `offset_transform_*` or `modulate`; game state changes first.

| Name | Trigger | Duration | Easing | Property |
| --- | --- | --- | --- | --- |
| press | tap a cell | 90 ms | back / out | `offset_transform_scale` 0.92 → 1 |
| pop | tent, grass or blank placed | 180 ms | back / out | scale overshoot 1.16 × 0.90 → 1 |
| shake | placed tent causes a conflict | 260 ms | sine / in-out | `offset_transform_position` x, damped ±7 px |
| sweep | row/column satisfied (and solve) | 35 ms stagger, 260 ms | sine + back / out | scale 1.08 then 1, `self_modulate` lift |
| intro | load level or restart | 16 ms stagger (cap 420 ms), 220 ms | back / out | scale 0.86 → 1, y +10 → 0, alpha 0 → 1 |
| slide | next or previous clearing | 280 ms | cubic / out | board x ±48 → 0, alpha 0 → 1 |
| celebrate | solved | ≤ 1100 ms | cubic / out | 22 tiles, position and alpha, freed after |

**Signature moment:** the tent pop on placement plus the line sweep when a row or column count
is met. Solving adds the celebrate burst; it is restrained, has no flashing, and is removed when
reduced motion is on.

## 8. Reduced motion

A visible "Reduce motion" toggle, saved in `user://settings.json` as `{"reduced_motion": bool}`
and applied on load and on every change. When on: every motion call applies its final state
instantly with no tweens, the solve burst is not created, and nothing flashes. Stored boards and
progress are not affected by this setting. Default is off.

## 9. Haptics intent & accessibility & icon/screenshot

- **Haptics:** none shipped. Sound: none in v0.2.
- **Accessibility checklist:** native `Button` for every cell with a row/column accessible name;
  fixed trees disabled and named; 48 px minimum targets; focus ring on every control; counts
  printed as numbers; motion can be turned off; no timer.
- **Known gap:** grid positions are not yet a fully accessible play route. VoiceOver, TalkBack
  and large text need direct user testing before any release.
- **Icon:** existing `icon.png` (terracotta tent beside a tall pine on sage paper). The palette
  above uses the same greens and terracotta, so the game matches the icon.
- **Screenshots:** first 5 × 5 clearing with counts; a marked grass cell; a text-explained
  diagonal conflict; a 6 × 6 solved clearing. These are future device captures, not existing
  assets.

## 10. QA checklist

1. Every tap changes state immediately; motion never delays a count, status or save.
2. A conflict shakes the tapped tent and the text cue matches the rule (touch, too many, pairing).
3. A row or column count reached sweeps only its own cells; undo does not replay a celebration.
4. Solving plays the sweep and burst once; undoing and redoing does not replay the burst.
5. Reduced motion on: no tween is left running and every cell rests at scale 1, offset 0.
6. Reduced motion survives a restart of the app (settings file).
7. Layout holds at 320, 375, 414 and 768 px wide, in portrait and landscape, with 150% text.
8. Focus ring visible on every control with a keyboard or controller.

## Evidence boundary

Uniqueness and constraint tests are not commercial or age-specific validation. Motion and
reduced-motion settings are not usability evidence for adults 40+. This is a GF-006
**hypothesis**. See [experiment brief](../../docs/research/next-game-experiments-2026-11-22.md).
