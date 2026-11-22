# Nurikabe Bay — independent design system (v0.1)

**Positioning:** Classify every square as sea or island. Numbered islands have exact sizes; each island contains one clue, the sea is connected, and there can be no 2×2 all-sea block. Calm puzzle agency, clear mechanical feedback, and no age stereotype, health claim, timer or dark pattern.

**Three adjectives:** tidal · quiet · precise.

| Semantic token | Hex | Purpose |
| --- | --- | --- |
| Base | `#F7F6EE` | Warm independent scene background |
| Ink | `#304543` | Readable numbers and directions |
| Accent | `#28617A` | Primary controls and focus |
| Soft region | `#F1DFB9` | Secondary controls and puzzle group |
| White | `#FFFFFF` | Editable cells or contrast text |

**Typography:** system font, 27–29px headline, 15–17px rule explanations and status, 14–27px puzzle labels, 13px secondary help. **Spacing:** 17–18px horizontal margins, ~12px vertical rhythm, ≥48px grid and action touch targets, ~4px grid gutters. Scroll vertically instead of shrinking critical type.

**Visual focus:** give player a usable first move immediately; non-color text distinguishes errors, clues and progress. Tap a blank cell to cycle SEA → LAND → EMPTY. Fixed clue cells are uneditable. **Motion:** immediate reversible taps, no countdown, flashing or confetti. **Sound/haptics:** none by default.

**Puzzle feedback:** actual constraint engine, not reference-answer comparison, determines completion. Undo and Restart are visible, preserved entries locally, optional next puzzle after rules verification, and readable messages for conflicts.

**Icon brief:** warm sand islands split by a continuous, deep blue sea with one large legible number. Independent vector illustration, not copied competitor imagery.

**Screenshot brief:** untouched numbered shore, selected sea path, 2×2 sea warning, solved bay. Screenshots are planned; no native-phone captures have been completed.

**Accessibility:** large tap targets, focusable controls, relevant cell accessibility labels and non-color explanation. Full nonvisual equivalence, screen-reader navigation, device-safe-area and 200% text scaling **require physical phone testing**. Do not infer age-45+ usability from a desktop Godot scene.

**Originality and validation:** independently authored six boards and controls; GF-010 is an unvalidated idea from the research queue. No claims of proven niche popularity, commercial differentiation, or cognitive treatment.
