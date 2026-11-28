# Word Ladder Lane — design system v0.2

## 1. Positioning & emotional promise

A warm, unhurried word puzzle for competent adults: each rung is a small, checkable change, and the ladder grows visibly as you climb. The promise is "one letter at a time, and you can always step back." Nothing here claims to train memory or health.

**Three adjectives:** warm · wooden · legible.

**Signature interaction:** tap a cream word card; the new rung lifts into the ladder with a settle, and the single letter that changed glows gold. Reaching the goal sweeps the whole ladder from the bottom rung to the top.

## 2. Colour tokens

Derived from `icon.png`: brown ground, wooden rails, cream letter tiles, gold for the changed letter.

| Token | Hex | Use |
| --- | --- | --- |
| `bg` | `#66401F` | Full-screen ground (warm brown) |
| `rail` | `#C68B4F` | Ladder rails and frame edge (decorative only) |
| `frame` | `#4E2E15` | Ladder panel fill behind the tiles |
| `surface` | `#FBF3E0` | Cream letter tiles, choice cards, body text on `bg` |
| `surface_alt` | `#F1E3C4` | Empty tiles, captions on `frame`, secondary explanation |
| `ink` | `#3B2212` | Text and letters on `surface` |
| `ink_muted` | `#6E4A2E` | Empty-slot outlines and blank letters on `surface_alt` |
| `accent` | `#EDB94F` | Changed-letter tile (gold), primary button |
| `accent_ink` | `#2E1A0B` | Text on `accent` |
| `success` | `#2F6B2A` | Solved celebration pieces only; the word "Solved!" stays `surface` |
| `error` | `#9E2A1B` | Reserved for invalid-input text; paired with text, never colour alone |
| `focus` | `#FFE08A` ring on `bg`; `ink` ring on cream tiles; `surface` ring on gold and secondary buttons | 3 px keyboard focus ring |
| `secondary` | `#84592F` | Undo, Restart, Previous, Copy challenge |

**Contrast (computed, WCAG 2.x):**
- `ink` on `surface` = 13.35:1
- `ink_muted` on `surface` = 7.08:1
- `accent_ink` on `accent` = 9.18:1 (hover `#F6C864` 10.53:1, pressed `#D9A23E` 7.24:1)
- `error` on `surface` = 6.78:1
- `success` on `surface` = 5.83:1
- `surface` on `bg` = 8.20:1
- `surface` on `secondary` = 5.51:1
- `surface` on `frame` = 11.01:1
- `focus` on `bg` = 7.02:1

State is never colour-only. The changed letter is gold **and** carries a dark 2 px outline plus the caption line "The gold letter is the one that changed." Solved is shown as the word "Solved!" in the status line. Empty slots are shown as `_` glyphs in outlined tiles.

## 3. Typography scale

Godot fallback face, embolden 0.8 (`FontVariation`) for headings and tiles.

| Role | Size px | Weight |
| --- | --- | --- |
| Eyebrow | 14 | bold |
| Title | 28 | bold |
| Heading (challenge line) | 16 | bold |
| Body / guidance | 15 | regular |
| Caption (`GOAL`, `START`, `STEP n`) | 13 | bold |
| Status line | 16 | bold |
| Secondary explanation | 14 | regular |
| Tile letter (ladder) | 34 | bold |
| Choice card word | 26 | bold |
| Button label | 19 | regular |
| Toggle label | 17 | regular |

## 4. Spacing & layout

- Base unit 8 px. Page margins 20 px sides, 24 px top and bottom; column rhythm 13 px.
- Ladder rows 10 px apart; tiles 6 px apart; ladder frame 14 px inner padding; 10 px wooden rails on left and right.
- Choice grid 10 px gaps, two columns at 414 px width.
- Minimum touch target 48 px. Choice cards 64 px tall, buttons 58 px, toggle 48 px.
- Tiles (64 × 68 px) are not tappable; they are visual readouts.
- Layout reference width 414 × 896 (portrait). The screen scrolls vertically; no horizontal scroll.

## 5. Components

- **Ladder tile:** cream `surface`, 12 px radius, 3 px shadow at 25 % black, bold letter 34 px in `ink`. Changed: `accent` fill, `accent_ink` 2 px outline and letter. Empty: `surface_alt`, 2 px `ink_muted` outline, `_` glyph.
- **Ladder row:** caption (56 px wide, 13 px bold) then four tiles. Rows: GOAL, START, STEP 1…n.
- **Ladder frame:** `frame` fill, 16 px radius, `rail` 10 px left and right edges, 6 px shadow.
- **Choice card (tile variant):** `surface` fill, 2 px `#C9B28A` edge, 64 px tall, 26 px bold word. States: normal `surface`, hover `#FBF3E0`→`surface_alt`, pressed `#E8D5AE`, disabled `#D8CBB0`, focus 3 px `ink` ring.
- **Primary button (Next):** `accent` fill, `accent_ink` label; hover `#F6C864`, pressed `#D9A23E`, disabled `#CDB98F`; focus 3 px `surface` ring.
- **Secondary buttons (Undo, Restart, Previous, Copy challenge):** `secondary` fill, `surface` label; hover `#9A6C3C`, pressed `#6E4A24`, disabled `#6F5A44`; focus 3 px `surface` ring. 11–14 px radius, 1 px shadow at rest.
- **Reduce-motion toggle:** `CheckButton` labelled "Reduce motion", 48 px min height, `surface` text.
- **Status banner:** plain text line under the choices (16 px bold). Invalid input shakes the ladder and keeps the existing status text.

## 6. States

- **Idle:** the next goal is shown as the GOAL row; START is cream; empty steps are outlined.
- **Selected / pressed:** the tapped card dips to 92 % scale (press tier) before the rung is placed.
- **Related / peer highlight:** the previous rung's letters are the reference for the gold change; no other highlight.
- **Conflict / invalid:** a tap that is not a legal next word shakes the ladder frame; state is unchanged.
- **Dead end:** "No legal continuation. Undo your last choice." in guidance text.
- **Completed group:** each accepted rung is a completed group. It pops into place on acceptance.
- **Solved:** every tile sweeps bottom to top in gold; the status line reads "Solved!"; celebration pieces burst once from the ladder centre.

## 7. Motion system

| Name | Trigger | Duration ms | Easing | Property |
| --- | --- | --- | --- | --- |
| press | tap a choice card | 90 | Back out | `offset_transform_scale` 0.92 → 1 |
| pop | rung accepted (each tile in the new row) | 180 | Back out | `offset_transform_scale` 1.16/0.90 → 1 (squash-stretch overshoot) |
| shake | invalid tap or failed update | 260 | Sine in-out | `offset_transform_position` x, ±7 px damped |
| intro (stagger) | level load, restart: GOAL, START, steps, cards in reading order | 220, 16 ms stagger | Back/cubic out | scale 0.86 → 1, y +10 → 0, alpha 0 → 1 |
| slide | next / previous level, after the state is applied | 280 | Cubic out | `offset_transform_position` x ±48 px, alpha |
| sweep (solved) | every ladder tile, bottom to top | 35 ms stagger, 260 | Sine out / Back out | scale 1.08 and gold `self_modulate`, then back to rest |
| celebrate (solved) | once, from the ladder centre | ≤ 1100 | Cubic out | 22 soft pieces in `accent`, `success`, `surface`; freed after |

**Signature moment:** the rung pop plus gold changed letter on each accepted word, and the bottom-to-top sweep on solve. The sweep reads as the ladder being climbed, so it uses the same tiers as the rest of the board.

Game state updates before any tween. Nothing flashes; no particle exceeds the celebrate budget.

## 8. Reduced motion

A visible "Reduce motion" toggle sits at the bottom of the screen and persists in `user://settings.json` as `{"reduced_motion": bool}`. When on, every motion call applies the final rest state synchronously: no tweens, no intro stagger, no slide, no shake, no sweep, no celebration pieces. The gold changed-letter colour and the "Solved!" text remain, because they are state, not motion. Nothing flashes at any setting.

## 9. Haptics, accessibility, icon and screenshots

- **Haptics:** none shipped; no sound by default.
- **Accessibility checklist:** tiles and buttons have accessible names (`accessibility_name`); rows read as "STEP n WORD"; focus rings on every control; 48 px minimum targets; no timers; changed letter identified by colour plus outline plus caption; text at 13 px or larger.
- **Icon:** unchanged (`icon.png`, `icon_bg.png`, `icon_fg.png` are not edited here). The in-game palette is taken from it: brown ground, rails, cream tiles, gold change.
- **Screenshots (to capture on real devices):** start board with GOAL row; a legal intermediate rung showing the gold letter; the dead-end message; the solved sweep; the reduce-motion toggle on.

## 10. QA checklist

1. Every tile letter is readable at 414 × 896 and at 150 % text scale without clipping.
2. Each accepted rung pops once; no rung pops on undo or restart.
3. The gold letter is exactly the position that differs from the previous rung.
4. A tap that is not a legal next word shakes the ladder and leaves state unchanged.
5. Solving sweeps all tiles bottom to top, then celebrates once; Next unlocks.
6. With "Reduce motion" on, no tween runs (`Motion.running == 0`) and nothing moves.
7. Focus ring is visible on every control when navigating by keyboard or gamepad.
8. Contrast figures in section 2 match the rendered colours.
