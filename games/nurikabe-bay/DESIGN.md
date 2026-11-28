# Nurikabe Bay — game design spec (upgrade v0.2)

## 1. Positioning and emotional promise

A quiet sea-and-island deduction puzzle for adults who like reasoning on a small board. Every square is sea or island; islands match their numbers, sea stays connected, no 2×2 all-sea block. The promise: calm, legible progress with no timer, no penalty and no health claim.

**Adjectives:** tidal · quiet · precise.

**Signature interaction:** an island "reaches its number" and its squares lift in a single sweep of light, so the player sees the rule satisfied at the moment it happens.

## 2. Color tokens

| Token | Hex | Use |
|---|---|---|
| bg | `#F7F6EE` | Page background (warm paper) |
| surface | `#FFFFFF` | Unknown (blank) squares |
| surface_alt | `#F1DFB9` | Fixed clue squares, island squares, secondary buttons (sand, from the icon) |
| ink | `#304543` | Body text, numbers, island glyph |
| ink_muted | `#4D5F5D` | Eyebrow, captions, help text, disabled button text |
| accent | `#28617A` | Primary button, focus ring, eyebrow emphasis |
| accent_ink | `#FFFFFF` | Text on accent buttons and sea squares |
| sea | `#315D79` | Sea squares (deep blue, matches the icon field) |
| success | `#2E6B4E` | Solved message |
| error | `#9B2D2D` | Rule-conflict message |
| focus | `#28617A` on paper/sand/white; `#FFFFFF` on sea | 3 px focus ring, always the contrast partner of its tile |
| border | `#BBC8BB` | Hairline outline on squares |
| glow | rgb(1.15, 1.20, 1.30) | Brief lift on a completed island (multiplier, not a fill) |

**Contrast (WCAG 2.x, computed):**
- ink on surface 10.20:1; ink on bg 9.41:1; ink on surface_alt 7.77:1
- accent_ink on accent 6.82:1; accent_ink on sea 7.06:1
- error on surface 7.48:1; error on bg 6.90:1
- success on surface 6.31:1; success on bg 5.82:1
- ink_muted on bg 6.23:1; ink_muted on surface_alt 5.14:1
- focus on surface 6.82:1; focus on bg 6.29:1

Every ratio is at least 4.5:1. State is never color-only: sea is "≈", island is "◆", fixed clue is its number, empty is "·", and every state has a text or accessibility label.

## 3. Typography scale

System font, all weights regular (no bundled font file). Sizes in px:

| Role | Size | Weight |
|---|---|---|
| Eyebrow | 12 | regular, uppercase, ink_muted |
| Title | 28 | regular |
| Body / heading / status | 16 | regular |
| Caption / help | 13–14 | regular, ink_muted |
| Button label | 16 | regular |
| Cell digit / glyph | 27 | regular |

Text reflows; the screen scrolls vertically rather than shrinking type.

## 4. Spacing and layout

- Base unit 4 px. Side margins 18 px; top 24 px; bottom 22 px.
- Column rhythm 13 px. Grid gap 4 px. Button gap 8 px.
- Minimum touch target 48 px. Cells 76 × 76 px; buttons 54 px tall; the reduce-motion toggle 48 px tall.
- Designed for 414 × 896 portrait; the board stays centred.

## 5. Components

- **Board cell (unknown):** surface fill, 2 px border, 12 px radius, 2 px shadow, "·" glyph. Hover: same. Pressed: motion dip. Focus: 3 px focus ring.
- **Clue cell (fixed):** surface_alt fill, number in ink, disabled (not tappable).
- **Sea cell:** sea fill, white "≈".
- **Island cell:** surface_alt fill, ink "◆".
- **Primary button (Next bay, Copy challenge):** accent fill, white label, 12 px radius, 3 px shadow. Normal / hover (+8% lightness) / pressed (−12%, 1 px shadow) / focus (3 px accent ring) / disabled (faded sand, no shadow).
- **Secondary button (Undo, Restart, Previous):** surface_alt fill, ink label. Same state set as primary.
- **Reduce-motion toggle:** CheckButton, "Reduce motion", 48 px tall, in the controls area.
- **Status:** heading line, progress line, feedback line. Feedback is ink normally, error when a rule conflicts, success when solved. Conflicts also keep their text, so color is never the only cue.

## 6. States

- **Idle:** unknown cells are white; clues are sand.
- **Selected (just tapped):** press dip, then the new value pops in.
- **Related/peer highlight:** none shipped; no highlighting of unrelated cells.
- **Conflict:** tapped cell shakes; feedback line turns error red and names the rule ("A 2×2 sea square is not allowed.", "Two numbered islands are touching.", "An island has more cells than its number.").
- **Completed island:** the island's squares sweep with the glow when the island first reaches its number.
- **Solved:** the whole board sweeps and a restrained burst of 22 soft tiles falls from the board (accent, success, white). Feedback line reads "Solved!" in success green.

## 7. Motion system

Timings use the shared `motion.gd` module without change.

| Name | Trigger | Duration (ms) | Easing | Property |
|---|---|---|---|---|
| press | tap on an editable square | 90 | back-out | offset scale 0.92 → 1 |
| pop | value changes (sea / island / empty) | 180 | back-out | offset scale (1.16, 0.90) → 1 |
| shake | rule conflict on the tapped square | 260 | sine in-out | offset x ±7 px, damped |
| sweep | an island first reaches its number; or solved (whole board) | 35 stagger, 260 | sine / back-out | offset scale 1.08 → 1, self_modulate → glow → white |
| intro | level load and restart | 16 stagger (max 420), 220 | back-out / cubic | offset scale 0.86 → 1, offset y 10 → 0, alpha 0 → 1 |
| slide | next / previous bay | 280 | cubic-out | offset x ±48 px, alpha 0 → 1 |
| celebrate | solved | ≤ 1100 | cubic-out | 22 soft tiles move and fade, freed after |

**Game state never waits for motion.** Every tap updates the board, the rule text and the saved progress immediately; motion only follows.

**Signature moment:** the sweep on an island that reaches its number. It is the only motion tied to a rule, so it teaches the player what counts as correct.

## 8. Reduced motion

- A visible **Reduce motion** toggle is persisted in `user://settings.json` as `{"reduced_motion": bool}`. It is read on launch and applied on change.
- When on, every motion call completes instantly: squares settle at rest, no shake, no sweep, no slide, no intro stagger.
- The solved celebration (falling tiles) is omitted entirely.
- No flashing: the glow is a single brief multiplier, never repeated or strobing.

## 9. Haptics, accessibility, icon and screenshots

- **Haptics:** none shipped; no vibration is planned for the first build.
- **Accessibility checklist:** squares carry row/column/state accessibility names; buttons are 48 px or more; focus ring on every control with contrast partner; reduce-motion toggle; no timer; state never color-only; text reflows.
- **Icon:** deep blue sea field with warm sand islands and a single large numeral, matching the palette above. Do not add copied iconography.
- **Screenshots (planned):** untouched numbered shore; an island reaching its number mid-sweep; a 2×2 conflict message; the solved bay.

## 10. QA checklist

1. Each motion ends exactly at rest (scale ONE, offset zero, alpha 1); `test_motion.gd` checks this for the tapped square.
2. With reduced motion on, no tweens run after a tap and the celebration does not appear.
3. Game state (entries, saved progress, rule text) is correct immediately after each tap, before any motion finishes.
4. Every tile remains readable at 414 × 896 with no clipped text, in both states of the reduce-motion toggle.
5. Focus ring is visible on sea squares (white) and on sand or white squares (accent) when navigating with a keyboard.
6. Physical-phone checks (VoiceOver, device safe areas, 200% text) are **not yet done**; they require real hardware.
