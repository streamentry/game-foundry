# Quiet Shot — design system (v0.2, motion and theme pass)

## 1. Positioning and emotional promise

Quiet Shot is a calm ricochet puzzle. You study one board, commit to a single shot, and watch
it resolve. A miss shows exactly where the ball went, so the next try is a considered
adjustment, not a guess. The promise is steady, earned satisfaction when the last target
clears. It makes no claim about memory, cognition or health.

**Three adjectives:** calm · exact · warm.

**Signature interaction:** one decisive "settle" per shot. The board takes a single soft pop
the moment the ball's path is revealed, and a win releases a small burst of amber and coral
tiles from the board. Nothing else flashes, bounces or repeats.

## 2. Color tokens

The palette is drawn from the app icon: deep navy playfield, cream target and mirror, coral
ball, amber trail. The surrounding UI is warm cream paper so the playfield reads as the
single dark object on the screen.

| Token | Hex | Use |
| --- | --- | --- |
| `bg` | `#F7F2E8` | Page background (cream paper) |
| `surface` | `#FFFDF8` | Raised panels, status surface |
| `surface_alt` | `#E9DFC8` | Secondary button fill |
| `ink` | `#1C2A47` | Headings, body text, focus ring (navy) |
| `ink_muted` | `#4F5878` | Supporting copy, captions |
| `accent` | `#F2B45B` | Primary button fill, celebration tile, trail |
| `accent_ink` | `#14213D` | Text on accent and on pressed accent |
| `accent_pressed` | `#D99A3F` | Primary button pressed and hover fill |
| `success` | `#2E7A5A` | Cleared-count text, check mark on mint |
| `error` | `#A8382B` | Error text (reserved; no current error state) |
| `focus` | `#1C2A47` | 3 px focus ring on every control |
| `playfield` | `#2A3B63` | Board background (icon navy) |
| `playfield_grid` | `#3A4D7A` | Grid lines on the playfield |
| `cream` | `#F6EEDC` | Mirror bars, unhit target rings, launch dot highlight |
| `coral` | `#E8674F` | Launch ball |
| `mint` | `#8FD9B6` | Cleared-target fill |

**Contrast (WCAG 2.x, computed):**

- `ink` on `surface`: **14.04:1**; `ink` on `bg`: 12.79:1
- `accent_ink` on `accent`: **8.71:1**; `accent_ink` on `accent_pressed`: 6.58:1
- `error` on `surface`: **6.33:1**; `error` on `bg`: 5.76:1
- `success` on `bg`: 4.65:1; `ink_muted` on `bg`: 6.27:1
- `cream` on `playfield`: 9.55:1; `trail` on `playfield`: 6.02:1
- `coral` on `playfield`: 3.41:1 (graphic only, above the 3:1 non-text minimum)

**Color is never the only signal.** A cleared target carries a check mark and the text
"X of Y targets cleared"; an unhit target keeps its cream ring and centre mark. The trail is
drawn as a line, not only as a colour change. Buttons carry text labels, and the primary
action is also the larger, filled control.

## 3. Typography scale

System typeface (no bundled font). Sizes in logical px.

| Role | Size | Weight | Example |
| --- | --- | --- | --- |
| Eyebrow | 12 | bold, caps | "QUIET SHOT / MAKE ONE GOOD DECISION" |
| Title | 29 | bold | "One shot. Clear them all." |
| Outcome / heading | 18 | bold | "3 of 4 targets cleared" |
| Board label | 16 | semibold | "BOARD 2 OF 8 • Clear sky" |
| Body and buttons | 16 | regular / semibold | "Fire once", "Turn −5°" |
| Lede and guidance | 15 | regular | Instruction paragraph |
| Caption | 13 | regular | Keyboard hint |
| Target numeral (none shipped) | 16 | bold | Reserved; targets are drawn, not labelled |

## 4. Spacing and layout

- **Base unit:** 6 px. Standard gaps are 12 px (column) and 9 px (button rows).
- **Safe margins:** 18 px left and right, 22 px top, 24 px bottom.
- **Min touch target:** 48 px; in practice buttons are 54 px tall and "Fire once" is 60 px.
- **Board:** at least 352 px tall, full content width, square playfield inside with 12 px inset.
- **Layout check:** 414 × 896 (portrait). Column scrolls vertically; no horizontal scroll.

## 5. Components

- **Board (playfield):** square drawn field with no rounded corners (matches the icon's
  framed look); 2 px `playfield_grid` border. Mirror bars 9 px solid `cream`. Launch point is
  a `coral` ball with an `ink` outline and a small cream highlight.
- **Targets:** `ink` ring 3 px around a `cream` disc (unhit, with an `ink` centre arc) or a
  `mint` disc (cleared, with an `ink` check mark).
- **Aim buttons (4):** `surface_alt` fill, 12 px corner radius, `ink` text, 54 px tall.
  - normal `surface_alt` · hover and pressed `#D8CBAE` · focus 3 px `focus` outline · disabled
    text `ink_muted`.
- **Primary button ("Fire once", "Next board" when enabled):** `accent` fill, 12 px corner,
  `accent_ink` text.
  - normal `accent` · hover and pressed `accent_pressed` · focus 3 px `focus` outline ·
    disabled: `surface_alt` fill with `ink_muted` text.
- **Secondary buttons (Adjust & retry, Previous, Copy challenge):** same as the aim buttons.
- **Preference row (Reduce motion):** a `CheckButton` labelled "Reduce motion", 54 px tall,
  `ink` text, below "Copy challenge".
- **Panels:** `surface` fill, 14 px corners, shadow `ink` at 10% alpha, `shadow_size` 6 px,
  offset y 2 px.
- **Status banner:** `surface` panel above the buttons; text "All N targets cleared!" in
  `success` when won, `ink` otherwise.

## 6. States

- **Idle:** the guide line from the launch point in `accent`, with a dot at its tip.
- **Selected direction:** the guide rotates with the chosen angle; the readout "Direction: N°"
  updates immediately.
- **Related or peer highlight:** not used (one board, no peers).
- **Conflict:** not used. The rules define no invalid move; a miss is a result, not an error.
- **Completed target:** `mint` fill with a navy check mark and the count text updated.
- **Solved:** every target cleared, status "All N targets cleared!", celebration burst.
- **Unhit after a shot:** `cream` disc with the `ink` centre arc; the count text reflects it.

## 7. Motion system

Shared module timings (`scripts/motion.gd`, unchanged from the template).

| Name | Trigger | Duration ms | Easing | Property |
| --- | --- | --- | --- | --- |
| Press | any button pressed | 90 | Back, ease-out | `offset_transform_scale` 0.92 → 1 |
| Settle (pop) | Fire resolves the shot | 180 | Back, ease-out | `offset_transform_scale` board 1.056 × 0.965 → 1 |
| Intro | level loads; board and aim buttons stagger | 220 each, 16 ms step, cap 420 | Back / cubic, ease-out | scale 0.86 → 1, y +10 → 0, alpha 0 → 1 |
| Slide | Next or Previous board | 280 | Cubic, ease-out | `offset_transform_position` x ±48 → 0, alpha |
| Celebrate | all targets cleared | ≤ 1100 (22 tiles) | Cubic, ease-out | tile position, rotation, alpha |

**Not used:** sweep (no per-target controls to animate; targets are drawn by one control,
and restructuring to create them was out of scope) and shake (no invalid action exists).

**Signature moment:** the board's settle pop on Fire. It is a single, restrained squash
that comes to rest, reading as "the ball landed". On a win, the celebration burst is the only
multi-element motion and is drawn in `accent`, `success` and `coral`, with no light flashing.

## 8. Reduced motion

- Setting: **Reduce motion** toggle, saved to `user://settings.json` as
  `{"reduced_motion": true|false}` and applied on launch and on every change.
- With it on, every motion call applies its final state synchronously; no tweens are created.
- The celebration burst is omitted entirely (no pieces are added to the scene).
- Nothing flashes at any setting. Large fills change only at rest.

## 9. Haptics, accessibility, icon and screenshots

- **Haptics:** none shipped. No vibration is requested by design.
- **Accessibility checklist:**
  - every action is a labelled button with a 48 px minimum touch target;
  - aim has four large buttons, so precision does not depend on a drag;
  - keyboard: left and right arrows turn, Enter fires, and every button has a focus ring;
  - no timers, no time pressure, no colour-only states;
  - reduced motion toggle visible on screen;
  - the board is a custom drawn visual; a non-visual play route is still incomplete and is
    flagged for user testing.
- **Icon:** the existing icon (navy field, cream target and mirror, coral ball, amber trail) is
  the reference for every colour here. Keep it unchanged.
- **Screenshots:** capture launch, after taps, and the help state with `capture.gd`.
  Screenshots must show the resting state (intro motion settles in 1.4 s).

## 10. QA checklist

1. Each text-on-background pair listed in section 2 is at least 4.5:1 (re-run the computation
   after any colour change).
2. At 414 × 896 no control or label is clipped, and nothing scrolls horizontally.
3. Tapping any button scales it and returns to rest within 0.1 s.
4. Fire pops the board once and leaves scale exactly 1.0 afterwards.
5. Next and Previous slide the board in from the correct side; game state never waits for it.
6. A win bursts at most 22 tiles and then removes them; no tiles remain after 1.2 s.
7. Reduce motion on: no tweens, no burst, board state is identical to motion on.
8. The reduce-motion choice survives an app restart.
