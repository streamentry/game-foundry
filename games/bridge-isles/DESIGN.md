# Bridge Isles — visual and motion system (v0.2, icon-harmonized)

## 1. Positioning and emotional promise

Bridge Isles is a short, untimed harbor puzzle for competent adults: join the islands so every
number is satisfied and the whole archipelago stays connected. The promise is calm certainty:
the board is always honest about what is right, what is not, and what is left to do. Nothing in
this game claims to treat, train or improve any cognitive or medical condition.

**Three adjectives:** sunlit · steady · legible.

**Signature interaction:** tap one island, then its neighbor. The two islands dip and rebound
and a bridge line is drawn between them. Tapping the same pair again cycles 0 → 1 → 2 bridges.
The pair of islands is the only "cell" the player ever needs to think about.

## 2. Color tokens

The palette is taken from the app icon: a deep teal ground, warm gold islands and dark teal
numerals. Teal carries structure and action; gold marks islands; ink carries all information.

| Token | Hex | Use |
| --- | --- | --- |
| `bg` | `#F3EFE2` | Screen background (warm paper, the icon's gold family, lightened) |
| `surface` | `#FFFCF3` | Board card, raised panels |
| `surface_alt` | `#DCEBE8` | Secondary button, selected helper chip |
| `water` | `#E6F0EC` | Board water inside the map |
| `ink` | `#173B42` | All text, island numerals, island outlines (icon numerals) |
| `ink_muted` | `#4D6A6E` | Helper and caption text |
| `accent` | `#1E6E76` | Primary button fill, bridges, kicker label (icon ground) |
| `accent_hover` | `#17606A` | Primary hover |
| `accent_pressed` | `#114B53` | Primary pressed |
| `accent_ink` | `#FFFFFF` | Text on `accent` |
| `pale_hover` | `#CFE4E0` | Secondary hover |
| `pale_pressed` | `#C5DFDD` | Secondary pressed |
| `gold` | `#F2C46D` | Island fill (decorative), completed-island glow |
| `success` | `#2F6B3E` | Solved banner text (always with the word "Solved") |
| `error` | `#A3352A` | Invalid-move text (always with a sentence explaining why) |
| `focus` | `#B5770D` | Keyboard / switch focus ring, selected island ring |
| `disabled_bg` | `#E4E8E3` | Disabled button fill |
| `disabled_ink` | `#56686B` | Disabled button text |

**Contrast (WCAG 2.2, computed):**
- `ink` on `surface` **11.76 : 1**; `ink` on `bg` **10.49 : 1**.
- `accent_ink` on `accent` **5.92 : 1** (and on `accent_hover` 7.20, on `accent_pressed` 9.74).
- `error` on `surface` **6.60 : 1**; `error` on `bg` 5.89 : 1.
- Also: `success` on `surface` 6.22; `ink_muted` on `surface` 5.68; `ink` on `pale_hover` 9.09.
- Non-text: `accent` on `water` 5.09; `ink` outline on `water` 10.36; `focus` on `bg` 3.25 and on `surface` 3.65 (meets the 3:1 focus-indicator minimum).
- Gold on water is only 1.40:1, so gold never carries meaning alone: the **ink outline** (10.36:1) defines the island shape and the numerals carry the number.

**State is never color-only:** selected = larger focus ring + "Island B selected" text;
connected = bridge line plus "N of M island totals matched"; error = text sentence starting
"Those islands cannot connect directly"; solved = "Solved" banner; disabled = greyed
fill plus the button's native disabled state.

## 3. Typography scale

System font (no bundled font in this release). All sizes in px at 414 pt width.

| Role | Size | Weight | Notes |
| --- | --- | --- | --- |
| Kicker (`BRIDGE ISLES / STILL WATERS`) | 12 | regular, caps | `accent` |
| Title (`Connect the islands.`) | 29 | bold | `ink` |
| Heading (`HARBOR n OF 6 • name`) | 16 | semibold | `ink` |
| Body / instructions / status | 15 | regular | `ink`, line wrap on |
| Button label | 16 | semibold | min 48 px tall |
| Island numeral (cell digit) | 23 | bold | `ink` on island fill |
| Island letter | 13 | regular | `ink`, above island |
| Helper text | 13 | regular | `ink_muted` |

## 4. Spacing and layout

- **Base unit:** 4 px. Side margin **17 px**, top 22, bottom 24.
- **Vertical rhythm:** 11 px between column items; 8 px gap between grid and row buttons.
- **Board:** minimum 340 px tall, expands horizontally, island ring at least 23 px radius.
- **Touch target:** minimum **48 px** (buttons 54 px, island buttons 54 px tall, 4 per row).
- **Safe margins:** the scroll container handles short screens; no essential control is shrunk below 48 px.
- Layout is the same in portrait at 414×896 and at smaller phone sizes; nothing sits under the bottom safe area.

## 5. Components

- **Board island (cell):** `water` map; ink 2 px outline circle (r 23) with `gold` fill (r 19) and
  an ink numeral (23 px). Selected: `focus` ring (r 25). Completed (total matched): the island's
  accessibility description reads "Required n, currently n bridges"; the visual change is the gold glow sweep.
- **Bridge:** guide line `#C9D6CF` 3 px under; one bridge = `accent` 7 px line; two bridges = two
  parallel `accent` 5 px lines offset 5 px.
- **Island letter buttons (nonprecision alternative):** 54 px tall, `surface_alt` fill, ink text.
- **Primary button (Next harbor):** normal `accent`; hover `accent_hover`; pressed `accent_pressed`;
  focus 3 px `focus` border; disabled `disabled_bg`/`disabled_ink`; radius 12; subtle shadow
  (2 px, `#000000` at 15 %, offset y 2).
- **Secondary button (Undo, Restart, Previous, Copy challenge):** normal `surface_alt`;
  hover `pale_hover`; pressed `pale_pressed`; focus 3 px `focus` border; disabled as above.
- **Reduce motion toggle:** `CheckButton` labelled "Reduce motion", 48 px tall, `ink` text,
  placed under the navigation row.
- **Status banner (feedback label):** 15 px `ink` text with a leading word that states the state
  ("Solved", "Cannot connect", "Island B selected"). Never a color-only indicator.

## 6. States

| State | Visual | Text / non-color cue |
| --- | --- | --- |
| Idle | island gold, ink outline, no ring | counts line |
| Selected | `focus` ring r 25 | "Island B selected. Now choose its neighbor." |
| Related / peer | none shipped; the board does not highlight neighbors | the helper line names the rule |
| Conflict | the two islands shake 7 px (medium tier) | "Those islands cannot connect directly; choose a visible neighbor." |
| Completed island | gold glow sweep on that island (medium tier) | "N of M island totals matched" |
| Solved | every island sweeps, then 22-piece burst (large tier) | "Solved" banner from `Rules.status` |

## 7. Motion system

All motion is visual only. Game state updates first; motion follows. Timings use the shared module.

| Name | Trigger | Duration (ms) | Easing | Property |
| --- | --- | --- | --- | --- |
| press | first island tapped | 90 | back / out | `offset_transform_scale` 0.92 → 1 |
| pop | bridge placed (both endpoints) | 180 | back / out | `offset_transform_scale` (strength 0.6: 1.10×0.94 → 1) |
| shake | invalid pair (both islands) | 260 | sine in-out | `offset_transform_position` x, 7 px damped |
| sweep | newly matched island / solved board | 35 ms stagger, 260 | sine / back | `offset_transform_scale` 1.08, `self_modulate` → gold glow → white |
| intro | level load or restart | 16 ms stagger (max 420), 220 | back / out, cubic | scale 0.86 → 1, y +10 → 0, alpha 0 → 1 |
| slide | next / previous harbor | 280 | cubic / out | `offset_transform_position` x ±48 px, alpha 0 → 1 |
| celebrate | solved | ≤ 1100 | cubic / out | 22 pieces, `position` + alpha fade |

**Signature moment — the tide rolls in.** Each connection is two islands rebounding toward each
other; a completed island glows gold; on solve the whole archipelago sweeps left to right in
island order, then a restrained burst of 22 teal, gold and paper tiles rises from the board
center and fades. No flashing, no looping, nothing above the medium tier except the solve.

## 8. Reduced motion

A visible **Reduce motion** toggle lives in the controls area. Its value is stored in
`user://settings.json` as `{"reduced_motion": bool}` and loaded on start.

When on: every motion call is instant (final state applied synchronously, no tweens created);
the shake, sweep glow and intro are skipped; the solve **celebration is omitted entirely**; the
slide between harbors is an instant cut. No effect flashes at any setting; the glow is a tint
held for the sweep duration only.

## 9. Haptics, accessibility, icon and screenshots

- **Haptics:** none shipped. No vibration API is called; no haptic intent is planned without opt-in player testing.
- **Sound:** none shipped.
- **Accessibility checklist:** all text ≥ 13 px and ≥ 4.5:1 except disabled controls (exempt by WCAG); buttons ≥ 48 px; accessibility names on board, islands, buttons and toggle; focus rings ≥ 3:1; no timer; Reduce motion toggle; no information by color alone.
- **Icon:** the existing `icon.png` is a deep teal ground with four gold numbered islands in a square (3 and 3 on top, 2 and 2 below) joined by gold bridges, the top pair doubled. The in-game palette uses the same teal ground and gold island language. Do not change the icon files in this upgrade.
- **Screenshots:** (1) first harbor with island letters and bridges empty; (2) two bridges placed with a pop; (3) a completed harbor showing "Solved". Capture only real in-engine scenes; native-device captures are still pending.

## 10. QA checklist

1. Every text pair in §2 is at or above its listed ratio (recompute if a hex changes).
2. Every island, letter button and control is ≥ 48 px tall and reachable at 414×896 without horizontal scroll.
3. Reduce motion on: placing a bridge, a conflict, a level change and a solve produce no tween and no celebration pieces.
4. Reduce motion off: each motion in §7 ends with scale 1, position 0, alpha 1 and no running tween.
5. Selecting, placing, undoing, restarting and navigating harbors never leaves an island misaligned or half-faded.
6. The solved banner reads "Solved" and no conflict text remains visible.
7. Headless tests (`test_rules`, `test_progress`, `test_scene`, `test_motion`) exit 0 with no `SCRIPT ERROR`, `Parse Error` or `ERROR:` lines.
8. Visual check against `icon.png` in an in-engine capture: the palette reads as one family with the icon.
