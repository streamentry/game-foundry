# Mosselora contrast audit (computed)

**Date:** 2026-11-23  
**Status:** computed contrast only; not a substitute for device/assistive-technology testing with adults 45+.

Pre-launch accessibility gate M5. Palette tokens in `design.md` section 3 are marked "proposed, not contrast-certified". This audit certifies arithmetic only, for the colors that code and docs actually set.

## Scope and sources

- `games/evergrove/design.md` section 3 palette tokens (lines 46-63) and play-layer/pipe rules (lines 102, 117, 158)
- `games/evergrove/DESIGN-SYSTEM.md` palette table (lines 13-22) and tokens
- `games/evergrove/Assets/Scripts/Unity/MosseloraTheme.cs` (paper, ink, leaf, water, tile fills, `TileFill` rule)
- `games/evergrove/Assets/Scripts/Unity/PipeGraphic.cs` (channel outer/inner colors per state)
- `games/evergrove/Assets/Scripts/Unity/EvergroveApp.cs` (text colors, button states, leak/landmark/clue glyphs)
- `games/evergrove/Assets/Scripts/Unity/VillageDiorama.cs` (3D stream colors, line 183)
- `games/evergrove/research/first-minute/style.css` and `game.js` (browser slice tiles, text, focus)
- `games/evergrove/site/src/styles/global.css` (site, included because it was requested; it is not the play surface)

## Method

- Relative luminance per WCAG 2.x (sRGB linearization, threshold 0.03928); ratio = (L_lighter + 0.05) / (L_darker + 0.05). Same formula used by WCAG 2.2 SC 1.4.3 and 1.4.11.
- Thresholds: normal text 4.5:1 (SC 1.4.3); large text (24px regular or 18.66px bold) 3:1; non-text UI and graphical objects 3:1 (SC 1.4.11).
- AAA note (text only): 7:1 for normal text, 4.5:1 for large text. Flagged "consider", not a failure.
- Unity `Text` sizes are Unity points, treated as px for classification. This is an approximation.
- Backgrounds are flat colors set in code. Gradients, SVG art and photo-like scenery are not sampled.
- Mutual pairs (connected vs dry, flowing vs dry) are heuristics for SC 1.4.1 cue needs, not literal WCAG thresholds.
- Fix method: keep HLS hue and saturation, move lightness in 0.05% steps to the nearest value that passes. Where a value is shared, one replacement token was checked against every partner background.
- Scripts (not in the repo): `/private/tmp/claude-501/-Volumes-SSD-james-claude-spares-spare-JFXjA7/ef46a67d-15e8-44c7-b012-4dcbc994fab3/scratchpad/contrast_audit.py`, `/private/tmp/claude-501/-Volumes-SSD-james-claude-spares-spare-JFXjA7/ef46a67d-15e8-44c7-b012-4dcbc994fab3/scratchpad/build_doc.py`, `/private/tmp/claude-501/-Volumes-SSD-james-claude-spares-spare-JFXjA7/ef46a67d-15e8-44c7-b012-4dcbc994fab3/scratchpad/compose.py`.

## Summary

- **Pairs audited:** 97
- **Pass AA:** 76
- **Fail AA:** 21 (6 text, 15 non-text/state)
- **AAA "consider" flags (passing text):** 30
- **Unity text (MosseloraTheme + EvergroveApp):** all text pairs pass AA. Several sit between 4.5:1 and 7:1 (see AAA table).
- **Failing text is concentrated in the browser slice:** 9-11px research tag, footer, note, hint and choice sub-line.
- **Failing state graphics:** the dry-vs-connected distinction, the `earth.dry` token, the diorama streams, and tile and textarea boundaries.

## Top fixes (in order of impact)

1. **Unify the dry-channel color to `#7D684F`.** It passes every dry partner (4.31:1 on the dry tile, 3.19:1 on the pressed tile). Apply it to `PipeGraphic.cs` (dry outer `140,118,91` becomes `125,104,79`), to the browser `.tile--dry` icon (`#8c765b` becomes `#7d684f`), and to the design token `earth.dry` (`#9B8266` becomes `#7D684F`). Fixes P01, P09, P11, B27.
2. **Add a non-color cue for connected vs dry.** The code uses identical geometry for both states, so the flow state is color-only. Section 5 has the detail and a proposed cue. This is the most important finding because it fails SC 1.4.1 for the core mechanic.
3. **Darken the six browser-slice text tokens that fail AA, and raise sub-12px labels to 12px.** `#748179` becomes `#67726B` (research tag), `#477663` becomes `#457260` (eyebrow), `#856D4D` becomes `#846C4D` (hint), `#748076` becomes `#68726A` (note), `#8B9286` becomes `#6B7267` (footer), `#65786C` becomes `#607266` (choice sub-line). The 9px and 11px sizes are below a comfortable floor for 45+ readers regardless of contrast.
4. **Fix the boundary strokes to 3:1 against the panel.** `#D1CDBD` becomes `#9C9371` (dry tile border), `#9CCECB` becomes `#4FA09C` (wet tile border), `#9FB9A5` becomes `#789C80` (notes textarea).
5. **Reconcile the design tokens.** `DESIGN-SYSTEM.md` lists Evergreen ink `#23443C` and Moss `#52685F`, while `design.md` lists `#183E34` and `#49645A`. Both pairs pass AA on paper, but the audited values are not the shipped ones. Keep one set. Also, the "water blue" connection motif `#6FB4BA` fails 3:1 on paper (2.14:1); use the Unity `WaterInk` `#176C83` (5.43:1) for the connection accent.
6. **Diorama stream states.** `#6FB4BA` (flowing) and `#C4B69C` (dry) on the scene background `#E5ECDD` fail at 1.95:1 and 1.65:1. Use `#499198` (flowing) and `#99835B` (dry), or add an outline. Mutual state contrast (1.18:1) still needs a non-color cue.

## Color-only cue finding (connected vs dry pipes)

**Result: the connected/dry state is conveyed by color alone in both the Unity and browser builds.** This fails SC 1.4.1 for the core mechanic.

- **Unity, `PipeGraphic.cs`:** dry and connected channels use the same rectangle stroke, the same inner stripe position and the same junction disc. Only three values change: outer color (`#8C765B` dry vs `#176C83` connected), inner color (`#EFE0C2` vs `#D3EFE7`), and tile fill (`DryTile #EAE8DC` vs `WetTile #D9EEEB`). The code comment says channel geometry "remains readable without any color", which is true for channel direction but not for the state.
- **Design requirement not implemented:** `design.md` line 117 specifies a "distinct stroke/texture" for connected channels, and line 158 says "never encode state only by color". The code does not meet either.
- **Browser slice, `game.js`:** `tileIcon()` draws identical strokes for both states. State lives in the `tile--wet` / `tile--dry` class (background, border and `currentColor`). The `aria-label` announces "flowing" or "dry" for assistive technology only. Visible non-color marks exist (`≋` spring, `✿` garden, `○` leak, and `?` hint in Unity), but none of them shows flow.
- **Mutual contrast is too low for a color-only split:** connected vs dry channel 1.38:1 (P04), browser glyphs 1.05:1 (B32), tile backgrounds 1.02:1 (B33), diorama streams 1.18:1 (V03). With the connected channel at relative luminance 0.125 on light tiles, a dry channel would need luminance at least 0.476 (fails against a light tile) or at most 0.0085 (near black). A 3:1 split by lightness alone is therefore not achievable while both states sit on light tiles.
- **Proposed cue (not yet tested):** draw dry channels with a dashed or segmented earth stroke and no inner water stripe. Keep the solid inner water stripe for connected channels and add a small droplet or ripple mark at the junction disc. Confirm in grayscale and with the 45+ playtest before adopting it.

## AAA "consider" items for the Unity labels read most often

Unity body-size labels sit between 4.5:1 and 7:1. Darkening them to 7:1 is a design call; the candidates are shown here so the choice is informed.

| ID | Pair | Current | Ratio | Darkened to 7:1 | Ratio |
|---|---|---|---|---|---|
| U05 | SecondaryInk #49645A on Paper | #49645A | 5.86:1 | #40574F | 7.08:1 |
| U07 | Leaf #276D54 on Paper | #276D54 | 5.61:1 | #215D48 | 7.00:1 |
| U11 | White on Leaf #276D54 (darken button fill) | #276D54 | 6.17:1 | #24634D | 7.08:1 |

## Full pair table

| ID | Where used (size) | Foreground / background | Ratio | Need | AA | AAA note | Proposed fix | Fixed ratio |
|---|---|---|---|---|---|---|---|---|
| U01 | Body/titles on paper (EvergroveApp backdrop) (14-17 body; title 24 (L)) | Ink #183E34 / Paper #F7F4E9 | 10.73:1 | 4.5:1 | PASS | pass (>=7:1) | - | - |
| U02 | Story cards (17) | Ink #183E34 / Sand #F2E7D4 | 9.65:1 | 4.5:1 | PASS | pass (>=7:1) | - | - |
| U03 | Journal entries (14) | Ink #183E34 / RiverWash #DCEDEF | 9.79:1 | 4.5:1 | PASS | pass (>=7:1) | - | - |
| U04 | Puzzle card (White card) (14-15) | Ink #183E34 / White #FFFFFF | 11.81:1 | 4.5:1 | PASS | pass (>=7:1) | - | - |
| U05 | Secondary/Moss labels on paper (11-14 (11px hint fails AAA too)) | SecondaryInk #49645A / Paper #F7F4E9 | 5.86:1 | 4.5:1 | PASS | consider: 4.5 to 7:1 | - | - |
| U06 | Journal blank-page text (14) | SecondaryInk #49645A / RiverWash #DCEDEF | 5.35:1 | 4.5:1 | PASS | consider: 4.5 to 7:1 | - | - |
| U07 | Leaf labels (wordmark, Chapter, Neighbors, habitat) (12-15) | LeafAction #276D54 / Paper #F7F4E9 | 5.61:1 | 4.5:1 | PASS | consider: 4.5 to 7:1 | - | - |
| U08 | Memory moment title (15) | LeafAction #276D54 / Sand #F2E7D4 | 5.04:1 | 4.5:1 | PASS | consider: 4.5 to 7:1 | - | - |
| U09 | Journal title (12) | LeafAction #276D54 / RiverWash #DCEDEF | 5.12:1 | 4.5:1 | PASS | consider: 4.5 to 7:1 | - | - |
| U10 | Secondary button caption (13-16) | Ink #183E34 / LeafSoft #DFEBDD | 9.60:1 | 4.5:1 | PASS | pass (>=7:1) | - | - |
| U11 | Primary button caption (default) (13-16) | White #FFFFFF / LeafAction #276D54 | 6.17:1 | 4.5:1 | PASS | consider: 4.5 to 7:1 | - | - |
| U12 | Primary button caption (highlighted) (13-16) | White #FFFFFF / PrimaryHighlight #337C63 | 5.00:1 | 4.5:1 | PASS | consider: 4.5 to 7:1 | - | - |
| U13 | Primary button caption (pressed) (13-16) | White #FFFFFF / PrimaryPressed #1D4C3A | 9.78:1 | 4.5:1 | PASS | pass (>=7:1) | - | - |
| U14 | Secondary button caption (highlighted) (13-16) | Ink #183E34 / SecHighlight #C6DFC9 | 8.33:1 | 4.5:1 | PASS | pass (>=7:1) | - | - |
| U15 | Secondary button caption (pressed) (13-16) | Ink #183E34 / SecPressed #B2CCBB | 6.89:1 | 4.5:1 | PASS | consider: 4.5 to 7:1 | - | - |
| U16 | Save-warning caption (min 12) (12-14) | DangerUnity #983E37 / WarnBg #F9E5E0 | 5.62:1 | 4.5:1 | PASS | consider: 4.5 to 7:1 | - | - |
| U17 | Hint clue '?' on focus tile (17 regular) (17 regular) | Ink #183E34 / HintFocusTile #E8CB8C | 7.51:1 | 4.5:1 | PASS | pass (>=7:1) | - | - |
| U18 | Leak marker circle on dry tile (23 bold glyph; shape cue) | EarthInk #765F45 / DryTile #EAE8DC | 4.89:1 | 3:1 | PASS | n/a (non-text) | - | - |
| U19 | Leak marker on connected (wet) tile (23 bold glyph) | EarthInk #765F45 / WetTile #D9EEEB | 4.98:1 | 3:1 | PASS | n/a (non-text) | - | - |
| U20 | Leak marker on solved tile (23 bold glyph) | EarthInk #765F45 / SolvedTile #DDECDD | 4.90:1 | 3:1 | PASS | n/a (non-text) | - | - |
| U21 | Leak marker on anchored tile (23 bold glyph) | EarthInk #765F45 / AnchoredTile #F1DEB6 | 4.54:1 | 3:1 | PASS | n/a (non-text) | - | - |
| U22 | Leak marker on hint-row tile (23 bold glyph) | EarthInk #765F45 / HintRowTile #F7E8BB | 4.93:1 | 3:1 | PASS | n/a (non-text) | - | - |
| U23 | Leak marker on hint-focus tile (23 bold glyph) | EarthInk #765F45 / HintFocusTile #E8CB8C | 3.82:1 | 3:1 | PASS | n/a (non-text) | - | - |
| U24 | Protected landmark dot on anchored tile (14 glyph) | Ink #183E34 / AnchoredTile #F1DEB6 | 8.93:1 | 3:1 | PASS | n/a (non-text) | - | - |
| P01 | Dry channel vs dry tile (PipeGraphic outer) (channel stroke) | PipeDry #8C765B / DryTile #EAE8DC | 3.52:1 | 3:1 | PASS | n/a (non-text) | unify to #7D684F | - |
| P02 | Connected channel vs wet tile (outer) (channel stroke) | PipeWater #176C83 / WetTile #D9EEEB | 4.95:1 | 3:1 | PASS | n/a (non-text) | - | - |
| P03 | Completed channel vs solved tile (outer) (channel stroke) | PipeDone #276D54 / SolvedTile #DDECDD | 5.03:1 | 3:1 | PASS | n/a (non-text) | - | - |
| P04 | Dry vs connected channel (mutual, state distinction) (state pair) | PipeDry #8C765B / PipeWater #176C83 | 1.38:1 | 3:1 | **FAIL** | n/a (non-text) | no color-only fix (see section 5): dry L would need >=0.476 or <=0.0085; add non-color cue | - |
| P05 | Connected vs completed channel (mutual) (state pair (both blue-green)) | PipeWater #176C83 / PipeDone #276D54 | 1.03:1 | 3:1 | **FAIL** | n/a (non-text) | no color-only fix: both blue-green; completed state needs shape/pattern cue | - |
| P06 | Dry inner stripe vs dry outer (within channel) (inner highlight stripe) | PipeDryInner #EFE0C2 / PipeDry #8C765B | 3.32:1 | 3:1 | PASS | n/a (non-text) | unify outer to #7D684F | - |
| P07 | Connected inner stripe vs connected outer (inner highlight stripe) | PipeWetInner #D3EFE7 / PipeWater #176C83 | 4.92:1 | 3:1 | PASS | n/a (non-text) | - | - |
| P08 | Dry channel vs hover tile (BFE0DC) (hover) | PipeDry #8C765B / HighlightTile #BFE0DC | 3.07:1 | 3:1 | PASS | n/a (non-text) | unify to #7D684F | - |
| P09 | Dry channel vs pressed tile (A3D2CC) (pressed) | PipeDry #8C765B / PressedTile #A3D2CC | 2.60:1 | 3:1 | **FAIL** | n/a (non-text) | unify to #7D684F | 3.19:1 |
| P10 | Connected channel vs hover tile (BFE0DC) (hover) | PipeWater #176C83 / HighlightTile #BFE0DC | 4.25:1 | 3:1 | PASS | n/a (non-text) | - | - |
| P11 | Dry channel: design-doc token earth.dry vs dry tile (design.md token value) | EarthToken #9B8266 / DryTile #EAE8DC | 2.95:1 | 3:1 | **FAIL** | n/a (non-text) | replace design token with #7D684F | 4.31:1 |
| P12 | Water.deep focus/connection token vs paper (focus accent token) | WaterInk #176C83 / Paper #F7F4E9 | 5.43:1 | 3:1 | PASS | n/a (non-text) | - | - |
| P13 | Lilac accent token vs paper (optional, non-text) (optional accent; not in code) | Lilac #9A78AE / Paper #F7F4E9 | 3.36:1 | 3:1 | PASS | n/a (non-text) | - | - |
| P14 | Gold token vs paper (design: never essential text) (reserved emphasis; not used as text in code) | Gold #E8B958 / Paper #F7F4E9 | 1.66:1 | 3:1 | **FAIL** | n/a (non-text) | reserved token; if used as icon on paper, #B88419 (3.00:1); not for text | 3.00:1 |
| P15 | Danger token vs surface.light (design.md) (design.md token; no code use found) | DangerToken #87402F / SurfaceLight #FFFDF6 | 7.36:1 | 4.5:1 | PASS | pass (>=7:1) | - | - |
| B01 | Body ink on paper (15-16) | Ink #183E34 / Paper #F7F4E9 | 10.73:1 | 4.5:1 | PASS | pass (>=7:1) | - | - |
| B02 | Lede (moss) on paper (15) | Moss #49645A / Paper #F7F4E9 | 5.86:1 | 4.5:1 | PASS | consider: 4.5 to 7:1 | - | - |
| B03 | Brand wordmark (12 bold) | BrandInk #275442 / Paper #F7F4E9 | 7.84:1 | 4.5:1 | PASS | pass (>=7:1) | - | - |
| B04 | Research tag (9) | ResearchTag #748179 / Paper #F7F4E9 | 3.70:1 | 4.5:1 | **FAIL** | n/a (fails AA) | #67726B; also raise 9px to >=12px | 4.55:1 |
| B05 | Eyebrow on world caption (10) | Eyebrow #477663 / WorldCaption #E2ECE1 | 4.29:1 | 4.5:1 | **FAIL** | n/a (fails AA) | #457260 | 4.52:1 |
| B06 | World caption text (13) | WorldText #355D4C / WorldCaption #E2ECE1 | 6.14:1 | 4.5:1 | PASS | consider: 4.5 to 7:1 | - | - |
| B07 | H1 emphasis (italic leaf) (52 (large)) | H1Em #34775C / Paper #F7F4E9 | 4.84:1 | 3:1 | PASS | pass | - | - |
| B08 | Hint detail line (12) | HintDetail #856D4D / Paper #F7F4E9 | 4.44:1 | 4.5:1 | **FAIL** | n/a (fails AA) | #846C4D | 4.51:1 |
| B09 | Note under board (11) | Note #748076 / Paper #F7F4E9 | 3.75:1 | 4.5:1 | **FAIL** | n/a (fails AA) | #68726A; 11px is below readable size, raise to >=12px | 4.54:1 |
| B10 | Footer text (9) | Footer #8B9286 / Paper #F7F4E9 | 2.91:1 | 4.5:1 | **FAIL** | n/a (fails AA) | #6B7267; 9px footer, raise to >=12px | 4.51:1 |
| B11 | Puzzle guide (moss) on panel (12) | Moss #49645A / PanelLight #FFFDF6 | 6.34:1 | 4.5:1 | PASS | consider: 4.5 to 7:1 | - | - |
| B12 | Legend water symbol on panel (21 weight 900 (large bold)) | LegendBlue #2D8798 / PanelLight #FFFDF6 | 4.10:1 | 3:1 | PASS | consider | - | - |
| B13 | Legend flower symbol on panel (19 weight 900 (large bold)) | LegendFlower #437E58 / PanelLight #FFFDF6 | 4.73:1 | 3:1 | PASS | pass | - | - |
| B14 | Utility button label (13 / 12 mobile) | Ink #183E34 / UtilBg #F8F6ED | 10.91:1 | 4.5:1 | PASS | pass (>=7:1) | - | - |
| B15 | Hint utility button label (13 / 12 mobile) | HintBtnInk #245E43 / HintBtnBg #DFEFE3 | 6.38:1 | 4.5:1 | PASS | consider: 4.5 to 7:1 | - | - |
| B16 | Choice button title (15) | Ink #183E34 / ChoiceBg #EDF2E6 | 10.38:1 | 4.5:1 | PASS | pass (>=7:1) | - | - |
| B17 | Choice button sub-line (12) | ChoiceSmall #65786C / ChoiceBg #EDF2E6 | 4.14:1 | 4.5:1 | **FAIL** | n/a (fails AA) | #607266 | 4.50:1 |
| B18 | Choice icon glyph (30 (large)) | ChoiceSpan #47825B / ChoiceBg #EDF2E6 | 4.00:1 | 3:1 | PASS | consider | - | - |
| B19 | Memory panel heading (approx. 33 (large)) | MemTitle #2A6B4B / MemBg #E3EEE5 | 5.34:1 | 3:1 | PASS | pass | - | - |
| B20 | Memory panel body (moss) (14; footnote 12) | Moss #49645A / MemBg #E3EEE5 | 5.42:1 | 4.5:1 | PASS | consider: 4.5 to 7:1 | - | - |
| B21 | Water stat value (15 bold) | WaterStatInk #1E6152 / WaterStatBg #DCEFEB | 6.09:1 | 4.5:1 | PASS | consider: 4.5 to 7:1 | - | - |
| B22 | Text button (leaf underline) (12) | Leaf #276D54 / Paper #F7F4E9 | 5.61:1 | 4.5:1 | PASS | consider: 4.5 to 7:1 | - | - |
| B23 | Primary button caption (15 bold) | White #FFFFFF / Leaf #276D54 | 6.17:1 | 4.5:1 | PASS | consider: 4.5 to 7:1 | - | - |
| B24 | Brand sigil letter, light end of gradient (25 italic bold) | SigilText #FBE7AF / SigilLight #4F7F45 | 3.85:1 | 3:1 | PASS | consider | - | - |
| B25 | Brand sigil letter, dark end of gradient (25 italic bold) | SigilText #FBE7AF / SigilDark #234D36 | 7.84:1 | 3:1 | PASS | pass | - | - |
| B26 | Connected tile glyph (currentColor) on wet tile (pipe icon) | TileWetInk #2B8092 / WetTile #D9EEEB | 3.77:1 | 3:1 | PASS | n/a (non-text) | keep (passes) | - |
| B27 | Dry tile glyph (currentColor) on dry tile (pipe icon) | TileDryInk #8C765B / DryTile #EAE8DC | 3.52:1 | 3:1 | PASS | n/a (non-text) | unify to #7D684F | - |
| B28 | Spring mark (wave) on wet tile (19-25 bold) | TileMark #247C99 / WetTile #D9EEEB | 3.93:1 | 3:1 | PASS | consider | - | - |
| B29 | Garden sprout mark on dry tile (19-25 bold) | Sprout #328456 / DryTile #EAE8DC | 3.74:1 | 3:1 | PASS | consider | - | - |
| B30 | Leak marker on alert disc (23 bold) | TileAlert #765F45 / AlertBg #FBF7EA | 5.61:1 | 3:1 | PASS | pass | - | - |
| B31 | Solved tile glyph (pipe icon when complete) | SolvedInk #276D54 / SolvedTileBr #DEEEE3 | 5.13:1 | 3:1 | PASS | n/a (non-text) | - | - |
| B32 | Connected vs dry glyph (mutual) (state pair) | TileWetInk #2B8092 / TileDryInk #8C765B | 1.05:1 | 3:1 | **FAIL** | n/a (non-text) | no color-only fix (see section 5): add non-color cue | - |
| B33 | Wet tile bg vs dry tile bg (mutual state) (state pair (backgrounds)) | WetTile #D9EEEB / DryTile #EAE8DC | 1.02:1 | 3:1 | **FAIL** | n/a (non-text) | no color-only fix (see section 5): add non-color cue | - |
| B34 | Dry tile fill vs panel (cell boundary) (cell boundary) | DryTile #EAE8DC / PanelLight #FFFDF6 | 1.21:1 | 3:1 | **FAIL** | n/a (non-text) | dry cell fill is 1.2:1 vs panel; boundary carried by border B35 once fixed | - |
| B35 | Dry tile border vs panel (cell boundary) (cell boundary) | DryBorder #D1CDBD / PanelLight #FFFDF6 | 1.57:1 | 3:1 | **FAIL** | n/a (non-text) | #9C9371 | 3.02:1 |
| B36 | Wet tile border vs panel (state border) | WetBorder #9CCECB / PanelLight #FFFDF6 | 1.70:1 | 3:1 | **FAIL** | n/a (non-text) | #4FA09C | 3.02:1 |
| B37 | Focus ring (#176995) on paper (button:focus-visible) | Focus #176995 / Paper #F7F4E9 | 5.46:1 | 3:1 | PASS | n/a (non-text) | - | - |
| B38 | Tile focus ring (--water) on panel (tile:focus-visible) | Water #176C83 / PanelLight #FFFDF6 | 5.88:1 | 3:1 | PASS | n/a (non-text) | - | - |
| B39 | Notes textarea border on panel (form control boundary) | NotesBorder #9FB9A5 / PanelLight #FFFDF6 | 2.07:1 | 3:1 | **FAIL** | n/a (non-text) | #789C80 | 3.01:1 |
| S01 | Ink on site paper (16 body) | Ink #183D34 / SitePaper #FBFCF8 | 11.61:1 | 4.5:1 | PASS | pass (>=7:1) | - | - |
| S02 | Muted body on site paper (16) | Muted #4A645D / SitePaper #FBFCF8 | 6.23:1 | 4.5:1 | PASS | consider: 4.5 to 7:1 | - | - |
| S03 | Caption / release note on paper (0.82-0.9rem) | Caption #526A62 / SitePaper #FBFCF8 | 5.67:1 | 4.5:1 | PASS | consider: 4.5 to 7:1 | - | - |
| S04 | Caption on white (footer) (0.88rem) | Caption #526A62 / White #FFFFFF | 5.84:1 | 4.5:1 | PASS | consider: 4.5 to 7:1 | - | - |
| S05 | Muted on white (footer) (0.9-1rem) | Muted #4A645D / White #FFFFFF | 6.42:1 | 4.5:1 | PASS | consider: 4.5 to 7:1 | - | - |
| S06 | Leaf deep link on paper (inherits body) | LeafDeep #174B3A / SitePaper #FBFCF8 | 9.69:1 | 4.5:1 | PASS | pass (>=7:1) | - | - |
| S07 | Leaf accent on paper (eyebrow) (0.8rem) | Leaf #276D54 / SitePaper #FBFCF8 | 5.99:1 | 4.5:1 | PASS | consider: 4.5 to 7:1 | - | - |
| S08 | White on leaf (primary button) (16 bold) | White #FFFFFF / Leaf #276D54 | 6.17:1 | 4.5:1 | PASS | consider: 4.5 to 7:1 | - | - |
| S09 | White on leaf-deep (button hover) (16 bold) | White #FFFFFF / LeafDeep #174B3A | 9.98:1 | 4.5:1 | PASS | pass (>=7:1) | - | - |
| S10 | Ink on leaf-wash (garden section) (16) | Ink #183D34 / LeafWash #EDF4EE | 10.69:1 | 4.5:1 | PASS | pass (>=7:1) | - | - |
| S11 | Muted on leaf-wash (choice list) (16) | Muted #4A645D / LeafWash #EDF4EE | 5.74:1 | 4.5:1 | PASS | consider: 4.5 to 7:1 | - | - |
| S12 | White on ink (skip link) (16 bold) | White #FFFFFF / Ink #183D34 | 11.96:1 | 4.5:1 | PASS | pass (>=7:1) | - | - |
| S13 | Focus outline (leaf) on site paper (focus-visible 3px) | Leaf #276D54 / SitePaper #FBFCF8 | 5.99:1 | 3:1 | PASS | n/a (non-text) | - | - |
| D01 | DESIGN-SYSTEM.md 'Evergreen ink' vs Paper (text) (not in code; token drift) | EvergreenInk #23443C / Paper #F7F4E9 | 9.71:1 | 4.5:1 | PASS | pass (>=7:1) | replace with design.md Ink #183E34 (10.73:1) | - |
| D02 | DESIGN-SYSTEM.md 'Moss' vs Paper (text) (not in code; token drift) | MossDS #52685F / Paper #F7F4E9 | 5.44:1 | 4.5:1 | PASS | consider: 4.5 to 7:1 | replace with design.md Moss #49645A (5.86:1) | - |
| D03 | DESIGN-SYSTEM.md 'Water blue' connection motif vs Paper (connection motif; not in Unity theme) | WaterBlueDS #6FB4BA / Paper #F7F4E9 | 2.14:1 | 3:1 | **FAIL** | n/a (non-text) | not text; use #176C83 (5.43:1) for connection accent | - |
| V01 | Flowing stream #6FB4BA vs scene background (VillageDiorama.cs:183) | StreamFlow #6FB4BA / SceneBg #E5ECDD | 1.95:1 | 3:1 | **FAIL** | n/a (non-text) | #499198 | 3.00:1 |
| V02 | Dry stream #C4B69C vs scene background (VillageDiorama.cs:183) | StreamDry #C4B69C / SceneBg #E5ECDD | 1.65:1 | 3:1 | **FAIL** | n/a (non-text) | #99835B | 3.02:1 |
| V03 | Flowing vs dry stream (mutual state) (state pair; mutual 1.x:1 expected) | StreamFlow #6FB4BA / StreamDry #C4B69C | 1.18:1 | 3:1 | **FAIL** | n/a (non-text) | no color-only fix; add ripple/outline cue | - |

Notes on the table:
- "Proposed fix" lists the new foreground hex, except where noted; for shared tokens, the replacement is applied everywhere that token appears. "Fixed ratio" is against the same background.
- Rows with `-` in the fix column already pass AA.
- Mutual rows (P04, P05, B32, B33, V03) have no color-only fix. They need the non-color cue in section 5.
- B34 (dry cell fill vs panel, 1.21:1) is covered by the cell border B35 once that is fixed.

## Limits

- Computed contrast only. It does not model anti-aliasing, font rendering, screen brightness, OS high-contrast modes or colour-blindness simulation.
- Gradients, the browser body gradient, SVG world art and the Unity 3D scene were not sampled.
- Unity sizes are approximations (points treated as px).
- No change was made to any existing file. Proposed hexes are candidates, not applied.
- Real-device, screen-reader and playtest verification with adults 45+ is still required.

**Status: computed contrast only; not a substitute for device/assistive-technology testing with adults 45+.**
