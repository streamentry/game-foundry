# One Path — design spec (v0.2, upgrade pass)

## 1. Positioning and emotional promise

One Path is a quiet, untimed trail puzzle: trace every line of a dot graph exactly once. The promise is calm certainty. A correct move settles into place, a dead end asks you to step back, and nothing hurries you. No brain-health, memory, or "senior" claims.

Adjectives: **tactile · unhurried · precise**.

Signature interaction: the **dot pop** — a tapped dot springs and settles, and the newly used line thickens into the trail, so every confirmed step is visible in two ways at once.

## 2. Color tokens

Palette drawn from the icon: deep teal ground, warm gold lines, cream dots. Paper and terracotta keep the board warm and readable.

| Token | Hex | Use |
| --- | --- | --- |
| `bg` | `#FBF4EB` | Page backdrop (paper) |
| `surface` | `#F1E9DE` | Play board panel |
| `surface_alt` | `#F0DFD2` | Secondary buttons, disabled fill |
| `ink` | `#22393E` | Titles, dot labels, outlines (deep teal-slate, echoes icon ground) |
| `ink_muted` | `#5A5552` | Instructions, captions, disabled text |
| `accent` | `#A25138` | Primary button (Next puzzle) |
| `accent_ink` | `#FFFFFF` | Text on accent |
| `accent_hover` | `#8A4330` | Primary hover / pressed |
| `success` | `#2A6F68` | Open-trail start rings, completion text (icon teal) |
| `error` | `#9B2C2C` | Dead-end / invalid message text |
| `focus` | `#2A6F68` | 3 px keyboard focus outline |
| `trail` | `#A8711C` | Used line (icon gold, darkened for graphics) |
| `dot_fill` | `#E7B488` | Unvisited dot fill |
| `current` | `#A25138` | Current dot fill |

WCAG 2.x contrast ratios (computed):

- `ink` on `surface`: **10.13:1** (on `bg` 11.17:1) — passes 4.5:1
- `accent_ink` on `accent`: **5.57:1** — passes; on `accent_hover` 7.18:1
- `error` on `surface`: **6.25:1** — passes
- `success` on `surface`: **4.89:1** — passes
- `ink_muted` on `surface`: **6.11:1**; on `surface_alt` 5.67:1 — passes
- `focus` on `bg`: 5.39:1 (non-text 3:1 met)
- `trail` on `surface`: 3.46:1 (non-text 3:1 met; the used line is also thicker, 7 px vs 4 px)

State is never color-only: used lines are thicker and counted in text ("X of Y lines"); the current dot is larger and white-labelled; open-trail starts carry a ring plus instruction text; dead ends and errors are sentences; completion is a sentence.

## 3. Typography scale

System sans fallback, no custom font files (keeps the APK small).

| Role | Size px | Weight | Use |
| --- | --- | --- | --- |
| Kicker | 12 | Bold | "ONE PATH / A QUIET LOGIC CHALLENGE" |
| Title | 29 | Bold | "Connect every line, once." |
| Heading (puzzle name) | 16 | Semibold | "PUZZLE n OF 8 • name" |
| Body | 15–16 | Regular | status, progress, instructions |
| Button | 16 | Semibold | all buttons (min height 54 px) |
| Caption | 13 | Regular | helper notes |
| Cell digit (dot letter) | 19 | Bold | A–H inside dots |

## 4. Spacing and layout

- Base unit 8 px; vertical rhythm 11 px; grid gaps 8 px.
- Outer safe margins 18 px left/right, 20 px top, 26 px bottom.
- Board minimum 338 px tall, full width; dots 40 px visual, 52 px+ hit area.
- Minimum touch target **48 px** (buttons 54 px).
- Layout must hold at 414×896 without horizontal scroll; the column scrolls vertically on small heights.

## 5. Components

- **Board dot (graph node):** 20 px radius outline `ink`, fill `dot_fill`; current dot fill `current`, radius 22 px, white letter. Open-trail start: 24 px ring in `success`.
- **Given/clue cell:** not applicable — One Path has no givens; every dot is equivalent.
- **Dot letter buttons (A–H):** 54 px, 4 per row, `surface_alt` fill, 12 px radius, `ink` label. Hover `#E1CEBF`; pressed same as hover with 1 px sink shadow; focus 3 px `focus` outline; disabled fill `surface_alt` with `ink_muted` text, no shadow.
- **Primary button (Next puzzle):** fill `accent`, label `accent_ink`, shadow 4 px / 18 % black, offset y 2. Hover/pressed `accent_hover`, shadow 1 px. Focus 3 px outline `focus`. Disabled: `surface_alt` fill, `ink_muted` text, no shadow.
- **Secondary button (Undo, Restart, Previous, Copy challenge):** fill `surface_alt`, label `ink`, same shadow scheme as above with a lighter 3 px shadow.
- **Reduce motion toggle:** `CheckButton` labelled "Reduce motion", 16 px `ink`, in the control stack beneath Next/Previous.
- **Panel:** `surface` board with 2 px `trail`-free `ink` outline at 30 % alpha, 12 px corners.
- **Status banner:** `ink_muted` line below the progress count; errors (dead end) switch to `error` text with the same sentence, so meaning is in the words.

## 6. States

- **Idle:** dots `dot_fill`, no lines used.
- **Selected (current dot):** fill `current`, radius 22, white label.
- **Related / peer highlight:** the open-trail start dots carry the `success` ring; the endpoints of a used line are darkened to `ink`.
- **Conflict:** tapping a dot that cannot be reached or reused: the board shakes once; status says why in `error` text.
- **Completed group:** not applicable (no rows or regions); completion is handled by "Solved".
- **Solved:** every line is `trail`, the status reads "Complete! Every line used exactly once.", the Next button becomes active, and the celebration plays once.

## 7. Motion system

Shared module timings from `scripts/motion.gd` (unchanged).

| Name | Trigger | Duration ms | Easing | Property |
| --- | --- | --- | --- | --- |
| press | tap any dot letter or board dot | 90 | Back / Out | `offset_transform_scale` 0.92 → 1 |
| pop | a dot is added to the trail (valid) | 180 | Back / Out | `offset_transform_scale` 1.16/0.90 → 1 |
| shake | invalid move on the board | 260 | Sine / InOut | `offset_transform_position` x, damped |
| stagger_in | level load or restart: dot letter buttons in reading order A–H | 16 ms stagger, 220 | Back / Out + Cubic | scale 0.86 → 1, y 10 → 0, alpha |
| slide_swap | Next / Previous puzzle | 280 | Cubic / Out | board x ±48 → 0, alpha |
| sweep | solved: all visible dot buttons | 35 ms stagger, 260 | Sine / Back | scale 1.08 → 1, self_modulate glow |
| celebrate | solved: soft burst from board centre | ≤ 1100 (22 pieces) | Cubic / Out | position, rotation, alpha |

**Signature moment — "the line settles":** when the final line is placed, the trail's lines snap from thin to thick and the last dot pops, then the whole dot set sweeps in reading order while a few warm pieces drift from the board's centre. It is a single pulse, not a loop.

## 8. Reduced motion

- Every motion call becomes instant: final state applied synchronously, no tweens.
- The celebration (burst pieces) is omitted; sweep only resets glow.
- The toggle "Reduce motion" is always visible; default off.
- No flashing anywhere; the only colour change is the static trail and focus outline.
- Setting is saved to `user://settings.json` as `{"reduced_motion": bool}` (the progress file has no settings field, so the progress save format is unchanged).

## 9. Haptics, accessibility, icon and screenshots

- **Haptics:** none shipped. Intent: none until playtests ask for it; never on every tap.
- **Accessibility checklist:** dots reachable by 54 px letter buttons; screen-reader names on every control; reduce-motion toggle; text sizes ≥ 12 px; contrast ≥ 4.5:1 for text; no timer; Undo and Restart work without precision.
- **Icon:** the existing teal/gold icon is kept. Palette keeps teal and warm gold family in the game.
- **Screenshots:** first trail board, a branch with the current dot visible, the solved state, and the letter-button grid (future device captures).

## 10. QA checklist

1. Motion off: every select, place, restart and level change is instant; `Motion.running()` is 0 after a placement.
2. Motion on: press/pop/shake/stagger/slide play once and end at rest scale 1.0 and position 0.
3. Reduce-motion toggle persists across restarts (`user://settings.json`).
4. Text at 414×896 and at 320 px width: no clipping, no horizontal scroll.
5. Every colour-coded state has a text or shape cue (see §6).
6. Focus outline visible on every button; keyboard A–H works.
7. Solved state shows the completion sentence and unlocks Next exactly as before (rules unchanged).
