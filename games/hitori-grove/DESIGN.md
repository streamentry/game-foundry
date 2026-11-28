# Hitori Grove — visual system v0.2 (icon-matched, dark forest)

## 1. Positioning and emotional promise

A quiet grove of numbers. The player shades repeats, keeps a single white path open, and
the board settles into calm order. Never implies cognitive or health benefit.

**Three adjectives:** hushed · botanical · exact.

**Signature interaction:** a tap cycles a square Shade → Keep note → Plain. Shading dims the
square to near-black, like the icon's leaf tiles, and adds a visible "×", so the state reads
without colour.

## 2. Colour tokens

Derived from the icon: deep forest ground, pale mint digits, lime leaves.

| Token | Hex | Use |
| --- | --- | --- |
| `bg` | `#0F2A1F` | Screen background |
| `surface` | `#173A2B` | Unshaded (white) square, panels |
| `surface_alt` | `#22503C` | Keep-note square, secondary button |
| `shade` | `#061510` | Shaded square fill (icon's dark tiles) |
| `ink` | `#E6F2DF` | Digits, headings, body text |
| `ink_muted` | `#A8C6B0` | Captions, helper text |
| `accent` | `#A6E05A` | Primary button fill, selection glow, solved sweep |
| `accent_ink` | `#0B1F16` | Text on accent |
| `success` | `#7BD9A0` | Solved status text and celebration tile |
| `error` | `#FF9D8A` | Conflict status text and conflict outline |
| `focus` | `#F5E68A` | Keyboard/gamepad focus ring (3 px) |

**Contrast (WCAG 2.x, computed):**
- `ink` on `surface` 10.81:1; `ink` on `bg` 13.22:1; `ink` on `surface_alt` 7.95:1; `ink` on `shade` 16.15:1
- `accent_ink` on `accent` 10.99:1
- `error` on `surface` 6.23:1; `error` on `bg` 7.61:1
- Also: `ink_muted` on `bg` 8.29:1; `success` on `surface` 7.32:1; `focus` on `bg` 12.08:1

Every state is never colour-only: shaded = fill + "×" glyph; keep = fill + "KEEP" text;
conflict = `error` text plus the status sentence; solved = status sentence plus celebration.

## 3. Typography scale (px, system sans)

| Role | Size | Weight |
| --- | --- | --- |
| Kicker (`HITORI GROVE / QUIET INK`) | 12 | bold |
| Title (`Leave only one of each.`) | 27 | bold |
| Heading (round line) | 16 | semibold |
| Body / status | 15–16 | regular |
| Button label | 16 | semibold |
| Cell digit | 24 | bold |
| Caption / footnote | 13 | regular |

## 4. Spacing and layout

- Base unit 4 px. Side margin 18 px, top/bottom 24 px. Column rhythm 12 px.
- Cell grid: 4 columns, 77 px cells, 4 px gap (320 px wide, fits 414 px with margin).
- Minimum touch target 48 px (cells 77 px, buttons 54 px, check toggle 48 px high).
- Layout scrolls vertically on short screens; nothing shrinks below 48 px.

## 5. Components

- **Board cell:** 8 px corner radius, 2 px `surface_alt` border, states below.
  Focus: 3 px `focus` ring, transparent fill.
  - Plain: `surface` fill, `ink` digit.
  - Shaded: `shade` fill, `ink` digit plus "×" line, 2 px `accent` border.
  - Keep: `surface_alt` fill, `ink` digit plus "KEEP".
- **Given/clue cells:** no separate given state; every digit is a clue and is always tappable.
- **Primary button** (Next puzzle): `accent` fill, `accent_ink` text, radius 12.
  - normal `accent` · hover `#B8E86E` · pressed `#8FC94A` · focus 3 px `focus` ring · disabled: `surface_alt` fill, `ink_muted` text.
- **Secondary button** (Undo, Restart, Copy challenge, Previous): `surface_alt` fill, `ink`
  text, radius 12. hover `#2C6149` · pressed `#1A4232` · focus ring · disabled `surface` fill with `ink_muted` text.
- **Reduce-motion toggle:** `CheckButton` labelled "Reduce motion", `ink` text, 48 px high.
- **Panels:** no raised cards on this screen; the board sits directly on `bg`, with a
  `surface` backing around the grid (radius 16, 1-step shadow `shade` at 40 %).
- **Status banner:** the status label, 15 px `ink`; conflict sentences switch to `error`,
  solved to `success`.

## 6. States

- **Idle:** plain squares, no highlight.
- **Selected (tap in progress):** the square dips to 92 % (press), then holds its new state.
- **Related / peer highlight:** not shipped (no peer highlight in this rule set).
- **Conflict:** the tapped square shakes once; status turns `error` and names the rule
  ("Two shaded squares share a side", "Shade duplicate numbers ...").
- **Completed group:** not shipped per row; rows are not a rule event in Hitori.
- **Solved:** every square sweeps in a lime wave, then a small celebration of tiles drifts
  from the board; status turns `success` and the "Next puzzle" button enables.

## 7. Motion system

| Name | Trigger | Duration ms | Easing | Property |
| --- | --- | --- | --- | --- |
| press | tap a square | 90 | BACK out | `offset_transform_scale` 0.92 → 1 |
| pop | square value changes | 180 | BACK out | `offset_transform_scale` 1.16/0.90 → 1 |
| shake | tap creates a conflict | 260 | SINE in-out | `offset_transform_position` x |
| sweep | puzzle solved | 35 stagger / 260 | SINE out / BACK out | scale 1.08 → 1, `self_modulate` glow |
| intro | level load / restart | 16 stagger / 220 | BACK out / CUBIC out | scale, position, alpha |
| slide | next / previous level | 280 | CUBIC out | board `offset_transform_position` x ±48, alpha |
| celebrate | puzzle solved | ≤ 1100 (22 tiles) | CUBIC out | position, rotation, alpha |

**Signature moment:** a solved grove. The board sweeps a lime wave through the squares,
then a few small leaf-like tiles drift away from the grid over about a second.

## 8. Reduced motion

Every motion becomes instant: final state set synchronously, no tweens, no sweep, no
celebration, no shake, no slide. Nothing flashes. The setting persists in
`user://settings.json` as `{"reduced_motion": bool}` and applies on load and on toggle.

## 9. Haptics, accessibility, icon and screenshots

- **Haptics:** none shipped; no vibration intent.
- **Accessibility checklist:** 48 px targets; `accessibility_name` on every cell and button;
  focus ring on every control; text cues for every state; reduce-motion toggle; no timer.
  Pending: VoiceOver/TalkBack, 200 % text and real-device contrast checks.
- **Icon:** existing icon kept (dark forest, mint digits, lime leaves). The palette here is
  taken from it, so the in-game look is the same family.
- **Screenshots:** a fresh board (plain squares), a board with shaded squares and a conflict
  status, and a solved board with the "Next puzzle" enabled.

## 10. QA checklist

1. Board renders at 414×896 with no clipped digits, buttons or status text.
2. Every state (plain, shaded, keep, conflict, solved) is distinguishable without colour.
3. Focus ring is visible on each button and cell when navigating with the keyboard.
4. Tapping a cell changes state immediately; animation never delays the state.
5. Reduce motion on: no tweens run after a tap, a level change or a solve.
6. Undo and Restart restore state and stay reachable while animations run.
7. Solved board shows the success status and unlocks "Next puzzle" without a flash.
8. Tests: rules, progress, scene and motion all exit 0 with no SCRIPT ERROR lines.
