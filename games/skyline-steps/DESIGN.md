# Skyline Steps — independent design system (v0.1)

**Positioning:** Fill tower heights 1–4 without repeating any height in a row or column. Perimeter numbers count how many buildings are visible from that direction. Calm puzzle agency, clear mechanical feedback, and no age stereotype, health claim, timer or dark pattern.

**Three adjectives:** architectural · spacious · legible.

| Semantic token | Hex | Purpose |
| --- | --- | --- |
| Base | `#F6F7FB` | Warm independent scene background |
| Ink | `#293649` | Readable numbers and directions |
| Accent | `#586F97` | Primary controls and focus |
| Soft region | `#E1E9F7` | Secondary controls and puzzle group |
| White | `#FFFFFF` | Editable cells or contrast text |

**Typography:** system font, 27–29px headline, 15–17px rule explanations and status, 14–27px puzzle labels, 13px secondary help. **Spacing:** 17–18px horizontal margins, ~12px vertical rhythm, ≥48px grid and action touch targets, ~4px grid gutters. Scroll vertically instead of shrinking critical type.

**Visual focus:** give player a usable first move immediately; non-color text distinguishes errors, clues and progress. Select a square and enter 1–4; outside edge clues are displayed around the grid. Undo, Erase and Restart are always available. **Motion:** immediate reversible taps, no countdown, flashing or confetti. **Sound/haptics:** none by default.

**Puzzle feedback:** actual constraint engine, not reference-answer comparison, determines completion. Undo and Restart are visible, preserved entries locally, optional next puzzle after rules verification, and readable messages for conflicts.

**Icon brief:** four soft-blue towers of different heights as seen from street level. Independent vector illustration, not copied competitor imagery.

**Screenshot brief:** top/side skyline clues, first three buildings entered, explained line conflict, finished city. Screenshots are planned; no native-phone captures have been completed.

**Accessibility:** large tap targets, focusable controls, relevant cell accessibility labels and non-color explanation. Full nonvisual equivalence, screen-reader navigation, device-safe-area and 200% text scaling **require physical phone testing**. Do not infer age-45+ usability from a desktop Godot scene.

**Originality and validation:** independently authored six boards and controls; GF-019 is an unvalidated idea from the research queue. No claims of proven niche popularity, commercial differentiation, or cognitive treatment.
