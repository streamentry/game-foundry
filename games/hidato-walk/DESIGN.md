# Hidato Walk — independent design system (v0.1)

**Positioning:** Place numbers 1–16 exactly once so every consecutive pair touches horizontally, vertically or diagonally, while preserving given numbers. Calm puzzle agency, clear mechanical feedback, and no age stereotype, health claim, timer or dark pattern.

**Three adjectives:** trail-like · tactile · curious.

| Semantic token | Hex | Purpose |
| --- | --- | --- |
| Base | `#F7F4EC` | Warm independent scene background |
| Ink | `#253E3D` | Readable numbers and directions |
| Accent | `#2E7071` | Primary controls and focus |
| Soft region | `#E1EDE6` | Secondary controls and puzzle group |
| White | `#FFFFFF` | Editable cells or contrast text |

**Typography:** system font, 27–29px headline, 15–17px rule explanations and status, 14–27px puzzle labels, 13px secondary help. **Spacing:** 17–18px horizontal margins, ~12px vertical rhythm, ≥48px grid and action touch targets, ~4px grid gutters. Scroll vertically instead of shrinking critical type.

**Visual focus:** give player a usable first move immediately; non-color text distinguishes errors, clues and progress. Tap an editable cell, choose a number on the 4×4 keypad, or use Undo, Erase and Restart. **Motion:** immediate reversible taps, no countdown, flashing or confetti. **Sound/haptics:** none by default.

**Puzzle feedback:** actual constraint engine, not reference-answer comparison, determines completion. Undo and Restart are visible, preserved entries locally, optional next puzzle after rules verification, and readable messages for conflicts.

**Icon brief:** four pebble numbers forming a gentle diagonal footprint trail. Independent vector illustration, not copied competitor imagery.

**Screenshot brief:** initial partially numbered trail, keypad, non-adjacent warning, finished 1-to-16 walk. Screenshots are planned; no native-phone captures have been completed.

**Accessibility:** large tap targets, focusable controls, relevant cell accessibility labels and non-color explanation. Full nonvisual equivalence, screen-reader navigation, device-safe-area and 200% text scaling **require physical phone testing**. Do not infer age-45+ usability from a desktop Godot scene.

**Originality and validation:** independently authored six boards and controls; GF-013 is an unvalidated idea from the research queue. No claims of proven niche popularity, commercial differentiation, or cognitive treatment.
