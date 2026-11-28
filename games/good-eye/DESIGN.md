# Good Eye — DESIGN.md (upgrade pass)

## 1. Positioning and emotional promise
Good Eye asks one question: how close to half can you cut this shape by eye? The promise is a quiet, exact moment: you commit a line, the shape splits cleanly, and the numbers say how good your eye is. It is a judgment game for adults, not a brain-training claim and not a "senior" product.

**Adjectives:** warm, precise, quietly playful.
**Signature interaction:** the cut. The moment you press Cut, the two revealed parts slide apart from one seam to a 7 px gap over 260 ms. It is the only large visual change in the game.

## 2. Color tokens
Palette is drawn from the icon: cream field, cobalt-to-deep-teal family, one warm coral-ochre counterpoint. In-game teal (`accent`) sits in the same family as the icon's cobalt, and the ochre `shape` mirrors the icon's warm accent.

| Token | Hex | Use |
| --- | --- | --- |
| `bg` | `#F8F6EF` | Screen background (paper) |
| `surface` | `#EFECE4` | Board play surface |
| `surface_alt` | `#EAE3D3` | Secondary button fill |
| `surface_alt_hover` | `#E0D6C4` | Secondary button hover/pressed |
| `ink` | `#223D35` | Headings, cut guide, outlines |
| `ink_muted` | `#52675E` | Supporting and status text |
| `accent` | `#24786D` | Primary button, active mode, eyebrow |
| `accent_hover` | `#1B5E54` | Primary button hover/pressed |
| `accent_ink` | `#FFFFFF` | Text on `accent` |
| `success` | `#2E6B45` | Celebration pieces only |
| `error` | `#9E3E40` | Invalid-aim status text |
| `focus` | `#223D35` | 3 px focus ring on every control |
| `disabled` | `#DFE0D9` | Disabled button fill |
| `shape` / `part_a` / `part_b` | `#DFC68F` / `#6FA99C` / `#DCA36D` | Uncut shape and revealed parts |

**Contrast (WCAG 2.x, computed):**
- `ink` on `bg` 10.87:1; `ink` on `surface` 9.95:1; `ink` on `surface_alt` 9.19:1.
- `accent_ink` on `accent` 5.27:1; `accent_ink` on `accent_hover` 7.57:1.
- `error` on `surface` 5.53:1; `error` on `bg` 6.04:1.
- `ink_muted` on `bg` 5.61:1 (supporting text).
- `focus` (`ink`) on `bg` 10.87:1, above the 3:1 non-text minimum.
- `disabled` text (`ink_muted` on `disabled`) 4.57:1. Disabled controls are exempt from contrast rules; the state is also shown by the label and the greyed fill.

**State is never color-only:** correctness always appears as numbers (A %, B %, points from perfect, eye score). Invalid aim is a text sentence and a shake. The error colour only reinforces the sentence.

## 3. Typography scale
System font fallback, no custom font files. Sizes in px.

| Role | Size | Weight | Used for |
| --- | --- | --- | --- |
| Eyebrow | 12 | regular | "GOOD EYE / A SMALL TEST OF JUDGMENT" |
| Title | 30 | regular | "How close to half?" |
| Result / cell digits | 24 | regular | A %, B % result line |
| Body / buttons | 16 | regular | Instructions, button labels |
| Caption | 14 | regular | Round line, detail, footer hint |
| Status | 13 | regular | Live aim status |

Pairing: one family, four distinct sizes for hierarchy. No second typeface.

## 4. Spacing and layout
- Base unit 4 px; section gap 12 px; action gap 8 px.
- Side margin 18 px; bottom 26 px. Column scrolls on compact phones (414×896 checked).
- Minimum touch target 48 px; all buttons are 54 px high, primary actions 60 px.
- Board 320 px minimum height, full width.

## 5. Components
- **Board:** paper surface, outline 2.5 px `ink`, cut guide 3 px `ink` with end-caps 6 px radius.
- **Given / clue cell:** not applicable (no grid).
- **Controls row:** four equal 54 px buttons (Move −, Turn −, Turn +, Move +).
- **Primary button (Cut shape):** fill `accent`, label `accent_ink`, radius 14, shadow 3 px at 18% `ink`, offset y 2.
  - normal: `accent` · hover: `accent_hover` · pressed: `accent_hover`, no shadow · focus: 3 px `focus` ring · disabled: `disabled` fill with `ink_muted` label.
- **Secondary button:** fill `surface_alt`, label `ink`, same radius and shadow.
  - hover: `surface_alt_hover` · pressed: same, no shadow · focus: ring · disabled: `disabled`.
- **Mode toggle:** primary style for the active mode.
- **Reduce motion toggle:** CheckButton, 48 px high, 16 px label, `ink`.
- **Status banner:** 13 px `ink_muted` line under the board; `error` colour when the aim is invalid.

## 6. States
- **Idle:** shape uncut, guide line in `ink`.
- **Selected / aimed:** guide line moves; status line describes the imbalance ("A side is larger…").
- **Related / peer highlight:** none (single shape, no peers).
- **Conflict (invalid aim):** the line does not cross the shape; status line turns `error`, the text says what to do, the board shakes.
- **Completed group:** not applicable; there are no groups.
- **Solved (revealed):** parts shown in `part_a` and `part_b` with the split animation; near-perfect cuts (error ≤ 1 point) also celebrate.

## 7. Motion system
Shared timings from `scripts/motion.gd` (unchanged template).

| Name | Trigger | Duration ms | Easing | Property |
| --- | --- | --- | --- | --- |
| Press | Any button tap | 90 | back, ease-out | `offset_transform_scale` 0.92 → 1 |
| Pop | Move or Turn changes the line | 180 | back, ease-out | board `offset_transform_scale` (1.064, 0.96) → 1 (strength 0.4) |
| Shake | Invalid cut | 260 | sine, ease-in-out | board `offset_transform_position` x ±7 px, damped |
| Intro | Level load / restart / mode switch | 220, 16 ms stagger, capped 420 | back / cubic | control `offset_transform_scale`, `offset_transform_position`, `modulate:a` |
| Slide | Next shape | 280 | cubic, ease-out | board `offset_transform_position` ±48 px, `modulate:a` |
| Split (game-specific) | Cut revealed | 260 | cubic, ease-out | `reveal_gap` 0 → 7 px on the two parts |
| Celebrate | Cut error ≤ 1 point | ≤ 1100 | cubic, ease-out | up to 22 soft pieces, freed afterwards |

**Not used:** `sweep` (there are no completed rows, regions or words to sweep), a board-tap press (a drag would pulse constantly; the line moves at once instead), and camera shake or hit-stop (no impact event; the cut is a reveal).

**Signature moment:** the split. The seam opens from zero to 7 px while the numbers appear; no flash, no sound.

## 8. Reduced motion
A visible "Reduce motion" toggle sits in the controls area and persists to `user://settings.json` as `{"reduced_motion": bool}`. When on, every call applies its final state instantly: no press, pop, shake, intro, slide or split animation, and no celebration pieces. The round, cut, score and all controls work identically. No flashing at any setting; the celebration pieces fade rather than flash.

## 9. Haptics, accessibility, icon and screenshots
- **Haptics:** none shipped.
- **Accessibility checklist:**
  - every button has text and a screen-reader name;
  - the board has an accessible name and description;
  - keyboard: arrows plus Enter, the same actions as the buttons;
  - 48 px minimum touch targets; text at least 13 px, body 16 px;
  - reduced-motion toggle; no timers; no colour-only state.
  - Not yet verified on physical phones with VoiceOver or TalkBack.
- **Icon:** the existing icon (cream field, cobalt blob, coral slash) is kept. The in-game palette shares its cool-to-warm pairing: teal accent beside an ochre shape.
- **Screenshots:** `01` launch state (cut ready), `02` after taps (aim status). Captured at 414×896 @ 3×.

## 10. QA checklist
1. `test_geometry`, `test_state`, `test_scene` pass with 0 failures.
2. `test_motion` passes: reduced motion leaves no tween running and rest scale ONE; normal motion settles to rest within 0.6 s.
3. Headless import and test runs print no `SCRIPT ERROR`, `Parse Error` or `ERROR:` lines.
4. Screenshots at 414×896 show no clipped labels and no overlap; the toggle sits below the fold and scrolls.
5. Invalid cut keeps its text cue and shakes; the status text returns to `ink_muted` after the next valid aim.
6. Toggling "Reduce motion" writes `user://settings.json` and takes effect without restarting.
7. Keyboard arrows and Enter still drive the same actions, and the shake and pop fire on them.
8. Real-device check (not yet done): VoiceOver/TalkBack reading order, large system text, and the split on a phone at 60 fps.
