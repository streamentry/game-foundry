# Cage Hearth — independent design system (v0.1)

**Positioning:** Fill 1–4 once per row and column while solving connected cages labeled with sums, products, differences, exact quotients or single values. Calm puzzle agency, clear mechanical feedback, and no age stereotype, health claim, timer or dark pattern.

**Three adjectives:** hearthside · mathematical · crafted.

| Semantic token | Hex | Purpose |
| --- | --- | --- |
| Base | `#FBF5EC` | Warm independent scene background |
| Ink | `#4B3733` | Readable numbers and directions |
| Accent | `#A15E47` | Primary controls and focus |
| Soft region | `#F3E4D6` | Secondary controls and puzzle group |
| White | `#FFFFFF` | Editable cells or contrast text |

**Typography:** system font, 27–29px headline, 15–17px rule explanations and status, 14–27px puzzle labels, 13px secondary help. **Spacing:** 17–18px horizontal margins, ~12px vertical rhythm, ≥48px grid and action touch targets, ~4px grid gutters. Scroll vertically instead of shrinking critical type.

**Visual focus:** give player a usable first move immediately; non-color text distinguishes errors, clues and progress. Tap a colored cage cell, enter 1–4 on the large keypad, or use Undo, Erase and Restart. Each cage includes a visible arithmetic target. **Motion:** immediate reversible taps, no countdown, flashing or confetti. **Sound/haptics:** none by default.

**Puzzle feedback:** actual constraint engine, not reference-answer comparison, determines completion. Undo and Restart are visible, preserved entries locally, optional next puzzle after rules verification, and readable messages for conflicts.

**Icon brief:** four warm mosaic tiles with a single bold plus and multiplication motif. Independent vector illustration, not copied competitor imagery.

**Screenshot brief:** colored initial arithmetic cages, entered numbers, partial invalid arithmetic, completed mosaic. Screenshots are planned; no native-phone captures have been completed.

**Accessibility:** large tap targets, focusable controls, relevant cell accessibility labels and non-color explanation. Full nonvisual equivalence, screen-reader navigation, device-safe-area and 200% text scaling **require physical phone testing**. Do not infer age-45+ usability from a desktop Godot scene.

**Originality and validation:** independently authored six boards and controls; GF-023 is an unvalidated idea from the research queue. No claims of proven niche popularity, commercial differentiation, or cognitive treatment.
