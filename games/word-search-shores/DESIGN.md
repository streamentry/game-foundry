# Word Search Shores: design system v0.2

## 1. Positioning and emotional promise

A calm, legible offline word search for competent adults: find three words on a shore of letters, with nothing hurrying you. The promise is clarity, not cleverness. Every letter is large, every found word is unmistakable, and nothing claims to train memory or health.

**Three adjectives:** maritime · legible · unhurried.

**Signature interaction:** tap a word's first and last letter and a sand-gold capsule grows across the line from the first letter to the last (260 ms), locking the word in place. Found words also strike through in the word list with a check mark, so the state never depends on colour alone.

## 2. Colour tokens

Palette taken from the app icon: a sea-teal ground, a sand-gold highlight capsule, cream letters and an aqua wave line.

| Token | Hex | Use |
| --- | --- | --- |
| `bg` | `#17525F` | Screen ground (icon's sea teal) |
| `surface` | `#0F3F4C` | Board panel, deeper than the ground |
| `surface_alt` | `#226474` | Secondary buttons |
| `ink` | `#FBF6EA` | Letters, headings, body text (icon's cream) |
| `ink_muted` | `#B7DCE4` | Instructions and secondary copy |
| `accent` | `#F2C97D` | Found-word capsule, kicker, primary button (icon's sand gold) |
| `accent_ink` | `#0C3440` | Letters on the capsule, text on primary button |
| `success` | `#A7E8C4` | Completion accent (paired with the word "Well done" text) |
| `error` | `#FFB8A8` | Reserved for invalid-input text (paired with text, never colour alone) |
| `focus` | `#A6E3EC` | Selection ring and keyboard focus (icon's wave aqua) |

**WCAG 2.x contrast (computed):**

| Pair | Ratio | Requirement |
| --- | --- | --- |
| `ink` on `bg` | 8.08:1 | ≥ 4.5 |
| `ink` on `surface` | 10.61:1 | ≥ 4.5 |
| `ink` on `surface_alt` | 6.20:1 | ≥ 4.5 |
| `ink_muted` on `bg` | 5.97:1 | ≥ 4.5 |
| `accent_ink` on `accent` | 8.49:1 | ≥ 4.5 |
| `success` on `bg` | 6.22:1 | ≥ 4.5 |
| `error` on `bg` | 5.26:1 | ≥ 4.5 |
| `focus` on `bg` | 6.15:1 | ≥ 3:1 (non-text) |

**State is never colour-only:** found = sand capsule + strike-through + check mark + "part of a found word" in the accessible name; selected = aqua ring + tinted fill; invalid = text message explaining the rule; completed = text "All three original words discovered."

## 3. Typography scale

Default Godot face (no extra font files). Weights are the engine default.

| Role | Size px | Notes |
| --- | --- | --- |
| Title | 27 | "Three words, one little shore." |
| Heading (shore line) | 16 | Shore number and name |
| Body / instructions | 15 | Wraps at 386 px |
| Word chips | 18 | Strike-through for found words |
| Buttons | 17 | Minimum 54 px tall |
| Caption / kicker | 12–13 | Uppercase kicker and footer only |
| Cell letter | 28 | Large, high contrast, one glyph per cell |

## 4. Spacing and layout

- Base unit 4 px. Column gap 12 px, screen margins 14 px sides, 20 px top, 23 px bottom.
- Board: 6 × 6 cells of 58 × 58 px, 4 px gaps, a 368 × 368 px area inside a 9 px inset panel (386 px wide, which fills a 414 px phone).
- Word chips wrap in a flow row with 14 px horizontal and 6 px vertical gaps.
- Minimum touch target 48 px. Every cell is 58 px; every action button is 54 px; the Reduce-motion toggle is 48 px.
- Layout holds at 414 × 896; the column scrolls on shorter screens.

## 5. Components

- **Board cell (letter):** transparent fill so the capsule behind shows through. Normal: cream 28 px letter. Hover: 10% white fill. Pressed: 16% white fill. Focus: 3 px aqua ring. Selected: 16% white fill with 3 px aqua ring. Found (inside a capsule): `accent_ink` letter.
- **Found-word capsule:** drawn behind the letters from the two cell centres; radius 0.44 of a cell; grows over 260 ms (cubic ease-out) from the first letter to the last. Reduced motion draws it at once.
- **Word chip:** RichTextLabel. Open: `○ WORD` in `ink`. Found: `✓ ~~WORD~~` in `accent`, with a strike-through.
- **Primary button ("Next puzzle"):** `accent` fill, `accent_ink` text. Normal: flat with a 4 px shadow. Hover: 8% lighter. Pressed: 12% darker, no shadow. Focus: 3 px aqua outline. Disabled: 35% fill and 60% text.
- **Secondary buttons (Undo, Restart, Previous, Copy challenge):** `surface_alt` fill, `ink` text. Same state rules as primary.
- **Board panel:** `surface`, 18 px corner radius, 10 px soft shadow (30% black, 5 px down).
- **Reduce-motion toggle:** a CheckButton labelled "Reduce motion", 48 px tall, saved to `user://settings.json`.
- **Status banner:** the result line in `ink` 16 px, with text that states progress ("1/3 words found") and completion.

## 6. States

- **Idle:** all cells transparent, chips show "○".
- **Selected:** first tapped cell gets the aqua ring and tinted fill. The note line says to tap the end letter.
- **Related / peer highlight:** none. Only the selected cell is marked, to keep the board calm.
- **Conflict:** the endpoints do not identify an unfound word. The cell shakes and the note line explains why.
- **Completed group:** a found word becomes a sand capsule and its chip gets a strike-through and check mark. The group sweeps in accent colour.
- **Solved:** all three words are found. The result line reads "All three original words discovered. Well done!" and the board celebrates.

## 7. Motion system

Shared timings come from `scripts/motion.gd`, copied unchanged from the store template.

| Name | Trigger | Duration ms | Easing | Property |
| --- | --- | --- | --- | --- |
| press | tap any cell | 90 | back, ease-out | `offset_transform_scale` |
| pop | a word is found (both endpoints) | 180 | back, ease-out | `offset_transform_scale` |
| shake | invalid endpoints | 260 | sine, ease-in-out | `offset_transform_position` |
| sweep | a word's cells, and all cells on solve | 35 stagger / 260 | sine / back | scale and `self_modulate` |
| intro | level load or restart | 16 stagger / 220 | back / cubic | scale, position, `modulate:a` |
| slide | next or previous puzzle | 280 | cubic, ease-out | position and `modulate:a` on the board |
| celebrate | solved | ≤ 1100 | cubic, ease-out | pieces of `#F2C97D`, `#A7E8C4`, `#0F3F4C` (≤ 22 pieces) |
| capsule grow | a word is found | 260 | cubic, ease-out | capsule tip position |

**Signature moment:** the capsule grows from the first letter to the last, then the word's cells sweep in sand. Solve adds a short burst of tiles from the board centre.

## 8. Reduced motion

Every motion becomes instant: cells rest at once, the capsule appears complete, and no shake, slide, sweep or celebration runs. The celebration is omitted entirely. Nothing flashes. The setting is read on launch from `user://settings.json` and saved when toggled.

## 9. Haptics, accessibility and icon

- **Haptics:** none shipped. Taps and finds may gain haptics later, after a device check.
- **Sound:** none.
- **Accessibility checklist:** every cell has an accessible name with row, column, letter and found state; targets are at least 48 px; text is at least 12 px; contrast ratios are listed above; state uses text and shape as well as colour; reduce motion is a visible toggle; no timers.
- **Icon:** the existing icon already matches this system (teal ground, sand capsule, cream letters, aqua wave). The in-game palette was chosen to match it.
- **Screenshots:** 01 shows the initial board. 02 shows a selected cell. A found-word capture is still to be taken on a real phone.

## 10. QA checklist

1. Every grid cell is visible and on the board after the intro settles (`tests/test_motion.gd`).
2. Reduced motion leaves no tween running; normal motion returns every cell to rest within 0.9 s.
3. The capsule grows from first letter to last in 260 ms and stays drawn after a restart clears it.
4. Found words show a strike-through and a check mark in the word list.
5. Letters remain readable at 414 × 896 and the column scrolls without clipping.
6. Undo removes the last capsule and its word state; Restart clears every capsule.
7. Each rules, progress and scene test still passes headless with no `SCRIPT ERROR`, `Parse Error` or `ERROR:` lines.
8. On a real iPhone and Android device: confirm the capsule reads clearly outdoors and at 200% system text.
