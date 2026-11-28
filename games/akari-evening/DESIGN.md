# Akari Evening — visual, motion and accessibility system v0.2

## 1. Positioning and emotional promise

Akari Evening is an original Light Up / Akari puzzle for competent adults: place lamps so every open square is lit, no two lamps see each other, and each numbered wall gets exactly its count. The promise is a quiet evening at a desk with one lamp on, where each placed light visibly spreads across the board. It makes no memory, cognition or health claims.

**Three adjectives:** candlelit · meditative · luminous.

**Signature interaction:** placing a lamp pops the bulb and sends a short gold glow across the squares that lamp newly lights. Blocked placements shake the tapped square and keep the existing text explanation. Solving the board sends one restrained wave across the lit squares and a small burst of amber tiles.

## 2. Color tokens

Palette is drawn from the app icon (night navy `#15034C`, amber `#AD7B3C`, lamp gold `#FCD454`) so the frame reads as the same evening as the icon, while staying light for legibility.

| Token | Hex | Use |
| --- | --- | --- |
| `bg` | `#F6F1E3` | Frame background (paper) |
| `surface` | `#FFFFFF` | Open white squares, secondary panels |
| `surface_alt` | `#F1DFA6` | Squares lit by a lamp (pale candle gold); secondary buttons |
| `ink` | `#15034C` | Rules, numbers, wall squares, text |
| `ink_muted` | `#4A4670` | Explanations, captions, progress hints |
| `accent` | `#AD7B3C` | Primary button fill, active border, focus-adjacent emphasis |
| `accent_ink` | `#15034C` | Text on accent |
| `lamp` | `#FCD454` | Filled lamp square |
| `success` | `#1F6B45` | Solved status text (always with a check mark ✓) |
| `error` | `#A3262E` | Violation status text (always with a warning sign ⚠ and text) |
| `focus` | `#15034C` | 3 px keyboard focus ring |
| `disabled_bg` | `#E4DCC7` | Disabled buttons |

**Contrast (WCAG 2.x, computed):**
- `ink` on `surface`: **18.27:1**; `ink` on `bg`: 16.19:1; `ink` on `surface_alt`: 13.79:1
- `accent_ink` on `accent`: **4.94:1**
- `error` on `surface`: **7.31:1** (`error` on `bg`: 6.48:1)
- `success` on `surface`: 6.47:1; `ink_muted` on `bg`: 7.75:1
- `focus` on `bg`: 16.19:1; `accent` on `bg`: 3.28:1 (non-text, used only as a border)

**State is never color-only.** Lamp = ☀ glyph plus fill; cross note = × glyph; wall = fixed number on ink; lit = pale fill plus a Lit label in the progress text; solved = ✓ text; violation = ⚠ text; focus = thick ring.

## 3. Typography scale

| Role | Size | Weight | Notes |
| --- | --- | --- | --- |
| Kicker (eyebrow) | 12 px | Bold | `accent` on `bg` is 3.28:1, so kicker uses `ink_muted` |
| Title | 28 px | Bold | "Light every open square." |
| Heading (puzzle name) | 16 px | Bold | `PUZZLE n OF 6` line |
| Body / instructions | 15 px | Regular | line wrap on |
| Button label | 17 px | Bold | min 54 px tall |
| Caption / hint | 13–14 px | Regular | `ink_muted` |
| Cell digit (wall) | 19 px | Bold | fixed numbers |
| Cell glyph (lamp / note) | 19 px | Regular | ☀ and × |

Fallback Godot font; no custom font shipped.

## 4. Spacing and layout

- **Base unit:** 4 px. Vertical rhythm 8–12 px between blocks.
- **Safe margins:** 16 px sides, 20 px top, 24 px bottom; the column scrolls on short screens.
- **Grid:** 4×4 board, 4 px gaps, cell 76 px (96 px on a 3-wide board, unused here). Board is centred.
- **Min touch target:** 48 px (controls at 54 px; the reduce-motion check row at 48 px).
- **Target width:** 414×896 logical portrait; no horizontal scroll.

## 5. Components

- **Board cell (open):** `surface` fill, 11 px corner, 1 px `#C0CBC4`-equivalent border (`ink` at 25 % alpha). Normal → hover: fill lightens 8 % toward white; pressed: fill darkens 8 % toward `ink` (no raised shadow) plus the press dip; focus: 3 px `focus` ring; disabled: the cell keeps its own fill (walls never change colour).
- **Lit cell (no lamp):** `surface_alt` fill, same border; the state is also reported in the progress text.
- **Lamp cell:** `lamp` fill, ☀ glyph in `ink`, 2 px `ink` border.
- **Cross note cell:** `surface` fill, × glyph in `ink_muted`.
- **Wall cell (fixed):** `ink` fill, white number, disabled (not tappable), no focus ring.
- **Primary button (Next):** `accent` fill, `accent_ink` label; normal, hover +6 % lighter, pressed −8 % darker with 1 px lower offset, focus 3 px `focus` ring, disabled `disabled_bg` with `ink_muted` label. Raised: `shadow_size` 3, `shadow_color` `ink` at 18 %.
- **Secondary button (Undo, Restart, Previous, Copy):** `surface_alt` fill, `ink` label; same state rules as primary.
- **Reduce-motion check row:** `CheckButton` labelled "Reduce motion", 48 px tall, `ink` label, placed above the action row.
- **Panels:** the status block is a `surface` panel with 12 px corners and `shadow_size` 2.
- **Status banner:** a text line in the panel; `success` with ✓ when solved, `error` with ⚠ when a rule is violated, `ink` otherwise.

## 6. States

- **Idle:** open squares white; lit squares pale gold.
- **Selected (pressed/tapped):** the square dips to 92 % scale for 90 ms.
- **Related / peer highlight:** every lit square (the lamp's reach) is pale gold, the same signal lamps use.
- **Conflict:** the tapped square shakes; the status line names the conflict in text ("Two lamps see each other…"); no colour-only change.
- **Completed group:** not used; Akari has no row or column completion rule.
- **Solved:** all white squares lit and no lamps see each other; the status reads "All white squares lit…" with ✓; one wave and a celebration burst.

## 7. Motion system

Timings come from the shared `motion.gd` module (unchanged).

| Name | Trigger | Duration (ms) | Easing | Property |
| --- | --- | ---: | --- | --- |
| press | tap any open cell | 90 | BACK / OUT | `offset_transform_scale` 0.92 → 1 |
| pop | lamp, note or blank placed | 180 | BACK / OUT | `offset_transform_scale` overshoot → 1 |
| shake | placement rejected (conflict) | 260 | SINE / IN_OUT | `offset_transform_position` x |
| lamp glow sweep | lamp placed; newly lit squares | 260 per square, 35 ms stagger | SINE / OUT then BACK / OUT | scale 1.08 + `self_modulate` gold → white |
| intro | level load or restart | 220, 16 ms stagger (cap 420 ms) | BACK / OUT and CUBIC / OUT | scale 0.86 → 1, offset y 10 → 0, alpha |
| slide | Next / Previous | 280 | CUBIC / OUT | board `offset_transform_position` ±48 px, alpha |
| celebrate | solved | ≤ 1100, 22 pieces | CUBIC / OUT | piece position, rotation, alpha |

**Signature moment:** the lamp glow. Newly illuminated squares are found with the existing `Rules.illuminated` before and after each placement (cheap, 16 cells), and only those squares receive the sweep, so the light visibly travels outward from the lamp.

## 8. Reduced motion

- A visible "Reduce motion" `CheckButton` sits in the controls area; its value is saved to `user://settings.json` as `{"reduced_motion": bool}` and applied on load and when toggled.
- With it on, every `Motion` call applies the final state synchronously: no tweens, no offsets, no alpha ramps, no sweep, no slide.
- The celebration burst is omitted entirely (the module already skips it).
- No flashing anywhere. Gold glows are single transitions, never repeated.

## 9. Haptics, accessibility and icon

- **Haptics:** none shipped; no vibration intent. **Sound:** none.
- **Accessibility checklist:** each cell has an `accessibility_name` ("Row r, column c, <state>"); all controls ≥ 48 px; no timers; Undo and Restart always visible; no colour-only state; status lines carry text; keyboard focus ring on every enabled control; 200 % text still requires a real phone audit.
- **Icon and screenshots:** the existing icon (navy night, amber tiles, glowing lamp) is kept untouched. Screenshot target: initial board, a legal intermediate placement with its glow, a conflict with its text cue, and a solved board. Captures are reviewed by eye; no store copy is claimed here.

## 10. QA checklist

1. Palette hexes in `scripts/main.gd` match section 2.
2. Every required contrast pair stays at or above 4.5:1 and the status text still matches its colour and glyph.
3. Tap a blank square: press then pop; a rejected lamp shakes and the text cue stays.
4. Restart and level changes run the intro; Next/Previous slide.
5. Reduce motion on: placements, shakes, slides and solved burst all instant; state matches reduced-off.
6. Reduce motion persists across relaunch via `user://settings.json`.
7. Layout fits 414×896 with no clipped text; the board stays centred.
8. All three test files and `test_motion.gd` exit 0 with no `SCRIPT ERROR`, `Parse Error` or `ERROR:` output.
