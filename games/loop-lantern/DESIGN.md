# Loop Lantern — visual and motion system (v0.2)

## 1. Positioning, promise, signature

**Positioning:** One line. One complete loop. A short, untimed Slitherlink-style deduction for competent adult players, lit like a lantern in a quiet plum night. It makes no claim to train, treat or improve memory or any cognitive or medical condition.

**Emotional promise:** calm focus, then a single quiet "closed" moment when the loop is complete.

**Three adjectives:** lantern-lit · geometric · gentle.

**Signature interaction:** tap a square, and the square lights up gold and gives a small press; tap a side and the edge draws in gold with a squash-and-settle, like a wick catching. When the loop closes, gold, mint and cream sparks rise once from the board centre and fade.

## 2. Color tokens

Icon-harmonised: the icon is a gold line with lavender dots on a deep plum ground, and the in-game palette uses the same hues and mood.

| Token | Hex | Use |
| --- | --- | --- |
| `bg` | `#2A0039` | Screen ground (matches icon background) |
| `surface` | `#3B1450` | Board fill, toggle row, pressed secondary |
| `surface_alt` | `#4D2366` | Secondary buttons (Undo, Restart, Previous, Copy) |
| `ink` | `#F7EFDC` | Headline, body, clue digits, labels |
| `ink_muted` | `#C9B3DE` | Helper text, disabled labels, board dots |
| `accent` | `#FDD241` | Primary buttons (Top/Right/Bottom/Left, Next), drawn loop, selection edge, focus ring |
| `accent_hover` | `#FFE68A` | Primary hover |
| `accent_pressed` | `#E0B530` | Primary pressed |
| `accent_ink` | `#2A0039` | Text on gold |
| `success` | `#9BE7B5` | Solved celebration accent (never the only cue; the status text says "One closed loop") |
| `error` | `#FF9A8A` | Rule-conflict cue colour (always with the text message) |
| `focus` | `#FDD241` | 3 px focus ring on every control |
| `guide` | `#8A6FA3` | Unused edges on the board (non-text, shown with thin weight) |

**WCAG contrast (computed):**

| Pair | Ratio | Requirement |
| --- | --- | --- |
| ink on bg | 15.84 : 1 | ≥ 4.5 pass |
| ink on surface | 13.08 : 1 | ≥ 4.5 pass |
| ink on surface_alt | 10.49 : 1 | ≥ 4.5 pass |
| ink_muted on surface | 7.84 : 1 | ≥ 4.5 pass |
| accent_ink on accent | 12.50 : 1 | ≥ 4.5 pass |
| error on surface | 7.31 : 1 | ≥ 4.5 pass |
| success on surface | 10.34 : 1 | ≥ 4.5 pass |
| focus (accent) on bg | 12.50 : 1 | ≥ 3 : 1 non-text pass |
| guide on surface | 3.48 : 1 | ≥ 3 : 1 non-text pass (decorative; the drawn loop is the state) |

**State is never colour-only.** A drawn edge is a thick gold line *and* counted in the status line; a conflict is a red-tinted message *and* a shake; a solve is a line of text *and* the sparks.

## 3. Typography scale

System font (no bundled font). Sizes in px at the 414 × 896 base.

| Role | Size | Weight | Use |
| --- | --- | --- | --- |
| Kicker | 12 | regular, gold | "LOOP LANTERN / FIND THE WAY AROUND" |
| Title | 27 | regular | "One line. One complete loop." |
| Body | 15 | regular | Instructions, status |
| Heading | 16 | regular | "LOOP n OF 6 • name", progress count |
| Button | 16 | regular | All buttons (min 54 px tall) |
| Caption | 13 | regular, ink_muted | Helper notes |
| Cell digit | 31 | regular | Clue numbers on the board |

## 4. Spacing and layout

- Base unit 4 px; column rhythm 12 px; row gaps 8 px.
- Side margin 17 px; top 23 px; bottom 24 px. Screen scrolls on short phones; nothing is squeezed below its minimum.
- Board: minimum 345 px tall, square, centred, 11 px inset.
- Minimum touch target 48 px (side and action buttons 54 px; the reduce-motion toggle 48 px).
- Layout is never driven by motion; motion uses `offset_transform_*` only.

## 5. Components

- **Board cell (3 × 3):** no border on empty cells; the selected cell gets a 22 % gold fill and a 3 px gold edge (cursor child). Clue cells show cream digits; blank cells show nothing.
- **Drawn edge:** unused = 2 px `guide`; used = 7 px `accent` with round caps. Vertices are 3.5 px `dots` (`#C9B3DE`).
- **Primary buttons (Top, Right, Bottom, Left, Next loop):** normal = gold with plum text; hover = `#FFE68A`; pressed = `#E0B530`; focus = 3 px gold ring with 14 px corners; disabled = surface with muted text.
- **Secondary buttons (Undo, Restart, Previous, Copy):** normal = `surface_alt`; hover = `#5C3078`; pressed = `surface`; focus and disabled as primary.
- **Reduce-motion toggle:** `CheckButton` labelled "Reduce motion", 48 px tall, cream text.
- **Status banner:** the status line under the board. Neutral text by default; conflict text is `error` plus the sentence; solved text is the sentence "One closed loop, every visible clue satisfied."

## 6. States

| State | Board | Text cue |
| --- | --- | --- |
| Idle | Gold cursor on the last selected square | "Select a square, then tap a side…" |
| Selected | Cursor on cell; press dip on tap | Heading shows `selected square r·c` |
| Related / peer | None (no peer highlighting in this game) | — |
| Conflict | Board shakes once; edge stays as drawn | "A square has more sides than its clue allows." / "The line branches…" |
| Completed group | Not used (no per-clue sweep) | — |
| Solved | Sparks from centre, loop stays gold | "One closed loop, every visible clue satisfied." |

## 7. Motion system

Shared timings from `scripts/motion.gd` (unchanged). Motion targets the cursor (a child Control that sits on the selected square) and the board; game state updates before any tween starts.

| Name | Trigger | Duration (ms) | Easing | Property |
| --- | --- | --- | --- | --- |
| press | Tap a square (select) | 90 | Back, out | cursor `offset_transform_scale` 0.92 → 1 |
| pop | Draw or erase an edge (no conflict) | 180 | Back, out | cursor `offset_transform_scale` squash 1.16 × 0.90 → 1 |
| shake | Rule conflict after a toggle | 260 | Sine, in-out | board `offset_transform_position` x, damped |
| stagger_in | Launch: board then Top/Right/Bottom/Left | 16 ms step, 220 | Back/Cubic, out | scale 0.86 → 1, position y 10 → 0, alpha |
| stagger_in | Restart | 220 | Back, out | board scale and alpha |
| slide_swap | Next (+1) or Previous (−1) | 280 | Cubic, out | board x ±48 → 0, alpha |
| celebrate | Solved (once per solve) | ≤ 1100 | Cubic, out | 22 soft pieces from the board centre, alpha fade at mid-life, freed after |

Signature moment: the solve sparks, which are the only large motion in the game.

## 8. Reduced motion

- A visible "Reduce motion" toggle sits in the controls area; the choice persists in `user://settings.json` as `{"reduced_motion": bool}`. It is read on launch and applied immediately when toggled.
- With reduced motion on, every call applies the final state synchronously: no tweens, no sparks, no shake, no slide, no stagger.
- Nothing in the game flashes. The sparks are not a flash and are omitted entirely in reduced motion.

## 9. Haptics, accessibility, icon and screenshots

- **Haptics:** none shipped; no vibration.
- **Sound:** none shipped.
- **Accessibility checklist:** every button has an `accessibility_name`; the board has one; state is text plus colour; 48 px minimum targets; reduce-motion toggle; scroll on short screens. Not yet verified: VoiceOver/TalkBack, 200 % system text, device contrast under real displays.
- **Icon:** the existing `icon.png` (gold line on plum with lavender dots) is the reference; do not reintroduce the terracotta palette from v0.1.
- **Screenshots:** untouched clue board (launch), one edge drawn (after taps), the status message after a conflict, and the solved state with sparks settling. Capture only in-engine scenes.

## 10. QA checklist

1. Launch: board and side buttons rise in order; nothing overlaps at 414 × 896.
2. Tap a square: cursor dips and returns; state updates before the animation.
3. Draw a conflicting edge: board shakes once, the red conflict sentence appears, and the edge is still undoable.
4. Solve board 1: sparks rise once and fade; the status line reads the solved sentence.
5. Next and Previous slide the board in the correct direction and stop at rest.
6. Toggle reduce motion on: repeat 2–5 and confirm no motion; relaunch and confirm the toggle is still on.
7. Keyboard/gamepad focus ring is visible on every control; no control is unreachable.
8. Run `tests/test_motion.gd` alongside the rules, progress and scene tests; all exit 0 with no `SCRIPT ERROR`.
