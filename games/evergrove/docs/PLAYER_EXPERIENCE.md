# Evergrove — player experience, visual grammar, story and accessibility

**North-star sensation:** “I did one understandable thing, and the place remembers it.” Screens are means, not the product. **Current runtime UI is generated from code**; the following distinguishes implemented flow, design intent and unverified device behavior.

**A separate research-only browser slice** at [research/first-minute](../research/first-minute/README.md) now implements only the first Unity-authored puzzle and one illustrated environmental decision. It is a low-cost first-minute comprehension stimulus, not a Unity export or the 12-stream app. Historical licensed builds and simulator/emulator checks are recorded in [build QA](BUILD_AND_QA.md); they do not verify the current preview changes or physical-device behavior.

## 1. The intended first 90 seconds

| Moment | What the player should perceive | Implemented surface | Biggest uncertainty |
| --- | --- | --- | --- |
| Launch | A calm place, a clear way to continue, no login or advertisement | Painted hub with a play/continue action; the puzzle page puts the board before optional village art | Do players find the first action without coaching, including on compact phones? |
| First ~10 s | I can tap a pipe; the interaction does something | blue-wave/sprout endpoint icons, 3×3 board, one-tap rotation, changing channel graphics | Mobile hitbox/legibility and actual zero-coaching comprehension |
| First attempts | An additional connection visibly carries water; a lost connection visibly dries | Only spring-connected pipe geometry becomes blue; dry channels stay earthen; 2D creek advances with reach; connected count and hollow-circle open-join cues remain | Do people grasp real connectivity without coaching, including grayscale play? |
| First solution | The previously dry downstream creek fills completely | `VillageScenePlan.WaterRestored` switches only after complete leak-free connectivity; the pre-choice localized consequence now uses `solved` instead of the stale dry-creek `world_none` label | Is this more rewarding than incremental flow, without confetti? |
| First choice | I can compare what will change before deciding | Tap tree / crossing / flowers to preview the garden consequence and actual next protected tile; **Make this change** confirms | Do players distinguish an unsaved preview from a committed choice, and understand the effect? |
| Memory beat | A character and environment acknowledge my action | `LocalizedVillage.Memory` and `Consequence`; explicit choice correction | Does this deepen attachment or interrupt a short puzzle session? |
| Second chapter | My last choice matters mechanically | Rotated route, warm fixed landmark tile; short rule line | Does player connect this geometry to the previous decision without explanation? |

**Unverified duration:** 1–3 minutes per session is a design target, not a timed gameplay result. No claim of “instantly intuitive” until first-tap and completion observations are recorded.

## 2. Existing screen hierarchy and controls

| Surface | Current source behavior |
| --- | --- |
| Launch / return hub | Painted garden, chapter progress, a genuine saved memory when available, and play/continue. No elapsed-time penalty or claim of unseen growth |
| Unsolved puzzle | Stream heading, 3×3 or 4×4 board, water reach, Undo and progressive Hint. The village follows the board in portrait and occupies the side panel in landscape |
| Solved Living puzzle | Three equal choice cards; a card tap opens an uncommitted preview. Other cards remain available to compare |
| Choice preview | Visible garden consequence, actual next protected pipe and rule, **Make this change**, and Close. The last chapter has no next-stream section |
| Committed memory | Changed garden, authored memory/consequence, **Change this choice** and Continue |
| Ending | Twelve recorded choices, authored ending, journal and confirmed reset |
| Optional sheets | Menu/settings, garden-file copy/restore, text postcard and journal. Research controls and optional 2.5D experiments exist only in development builds |

This is an implementation map, not a guarantee that every surface fits without scrolling. The new preview requires real-device measurement at compact widths, 150% text, landscape and safe-area cutouts. Board taps update mounted tile widgets; selecting a preview rebuilds the choice area without changing the world.

### Interaction states

- **Leak visibility:** unmatched/out-of-bounds water ports also show a **hollow-circle open-join** marker, supplementing shape, color and global leak feedback. Phone grayscale/readability test is still open.
- **Rotatable tile:** tap turns it clockwise by its allowed orientation cycle.
- **Protected landscape tile:** after chapter one, one non-endpoint tile is fixed, amber/marked and non-interactable. The explanation references roots/foundation/sanctuary.
  - **Landmark marker (2026-11-23):** the protected tile gets a heavy bark-brown frame plus a badge in its top-left corner, drawn procedurally by `LandmarkGraphic` with no new art. The badge matches the choice: root tendrils (tree), a stone footing under a plank deck (bridge), or a five-petal flower (wildflower sanctuary). The badge sits outside every channel arm, so pipe geometry and endpoint markers stay unchanged. The marker is non-color: frame thickness and badge shape carry the meaning. The text line below the board still reads "…anchor this pipe. It cannot rotate." without naming the marker. **Unverified with players:** the QA screenshots (`docs/qa/mac-player-2026-11-23/tour/07–09`) show the shapes are distinct on a 540×960 frame, but no adult 40+ playtest has confirmed that the marker is noticed unaided.
- **Board size (EG-112, first pass 2026-11-23):** the settings row beside "Aa" offers "▦ Standard" or "▦ Large". The choice is stored in PlayerPrefs (`evergrove:board-size:v1`) and never touches the world save. Standard keeps the EG-007 sizing exactly. Large widens the board card, removes most of its side padding and lifts the tile cap (120 pt), so 3×3 and 4×4 tiles grow to the available width. Pipe channels and landmark frames already scale with their tile; the open-join circle, endpoint icon and hint "?" scale by the same factor, and each tile stays one full-size tap target. No zoom or pan. Dev builds accept `-mosseloraBoardSize large` (or `standard`) for one run without persisting it. **Unverified:** whether Undo, Hint and Restart stay reachable without scrolling at 402×874 pt on 4×4 (the board grows by roughly 75 pt, so scrolling is likely there), low-vision testers, and physical devices. The label words are English; no new localization key was added.
- **Text size (EG-007, implemented 2026-11-23):** one central scale in `TextScale` applies to every generated label, button, save banner and dev-only input. The choice is stored in PlayerPrefs (`evergrove:text-scale:v1`) and never touches the world save. Default 100%. Buttons and rows keep a scaled minimum height and grow with their text; board tiles, pipe geometry and the tile-embedded markers (open-join circle, hint "?") stay fixed. QA evidence at 150%, 375x667: `docs/qa/mac-player-2026-11-23/tour-text150-375/`. Dev builds accept `-mosseloraTextScale 150` for one run without persisting it. **Unverified:** physical devices, OS text-size interaction, and adult 40+ testing.
- **Solved board:** current water path is complete, no leaks; further rotate is blocked, undo may reopen a previous move.
- **Choice preview (2026-11-26):** tapping a tree, crossing or flower card opens an unsaved projection. Compare cards or Close without changing the live world. The visible consequence comes from a detached `VillageChoicePreview.WorldAfter`; the next protected pipe uses the real kind, orientation and initial water reachability. The final chapter shows only the garden outcome. **Make this change** commits and saves once; the preview is not a saved decision or a playable next board. Relaunch, Undo or world replacement must not apply a stale selection. Native behavior is not yet verified.
- **Memory state:** shows the result; **Change your choice** reopens the prior solved puzzle without replaying it.
- **Completion:** after 12 Living choices, show a conclusion. In PuzzleOnly, the campaign ends after advancing from the final solved board. Restart/erase is two-step.
- **Audio:** silent by default; optional original procedural short chime.
- **Motion:** the current gentle-motion setting controls pipe turns, transitions and decorative effects; it can be switched off. The separate development-only miniature starts still.
- **Playtest:** never records before deliberate opt-in; explicit clipboard export or in-memory erase.

## 3. Art design language

Implementation tokens: [DESIGN-SYSTEM.md](../DESIGN-SYSTEM.md). Brand direction: [design.md](../design.md). Working visual tokens:

| Name | HEX | Purpose |
| --- | --- | --- |
| Paper | `#F7F4E9` | Soft, non-glaring canvas |
| Evergreen Ink | `#23443C` | Primary content |
| Moss | `#52685F` | Quiet support text |
| Leaf | `#276D54` | Primary action |
| Pale | `#DFEBDD` | Neutral button |
| Sand | `#F2E7D4` | Story emphasis |
| River | `#DCEDEF` | Memory area |
| Water blue | `#6FB4BA` | Stream and hydrology cue |

**Default 2D:** `PaintedVillage` uses the committed illustrated assets, with `VillageIllustration` as a procedural fallback. The upstream inlet may be partly wet at launch; its water front grows or retracts with the *actual spring-connected graph*, while the downstream creek stays dry until solved. Trees, crossings, flowers, butterflies, visitors and landscape changes remain driven by saved choices. Earlier simulator/emulator art captures are historical evidence; this source increment has not been visually verified in Unity or on phones.

**Optional 2.5D:** `VillageDiorama` creates a small orthographic primitive village, with single-camera rendering to a 768×384 texture. Does not need licensed assets or network. Motion is off by default and not proof of game-quality animation. It must fall back to 2D on unsupported devices.

**Quality risk:** neither painted art nor the procedural fallback establishes player preference. A comparison should include a polished reference and test puzzle comprehension, not only visual liking.

## 4. Narrative contract

**Characters and verbs:** Mira cares for shelter, Orin for crossing and connection, Fen for meadows and wildlife. The game's claim “remembers” is **only** a description of deterministic reactions to `WorldModel.choices`. The game does not store, interpret or personalize open-ended player conversation.

- `ShadeTree + Bridge` → shaded crossing; Mira and Orin can refer to it.
- `ShadeTree + Flowers` → butterflies find shelter in a grove; Fen references it.
- `Bridge + Flowers` → people visit the meadow; Orin/Fen respond.
- All three → the place feels connected; later passage and ending acknowledge the combined history.
- Counts matter for some higher-level authored consequence branches; a single threshold is not a dynamic ecological carrying capacity.
- There are multiple hand-authored endings, differentiated by decision history.

**Localization nuance:** `VillageStory.cs` contains the **fuller English manuscript**, but runtime `LocalizedVillage.cs` intentionally uses a shorter canonical state-dependent script for **every** supported UI language, including English. The two may express different levels of emotional detail; translators should review the actual runtime strings, not merely the long manuscript.

**Story guardrails:** no trauma manipulation, no insinuation that game characters are sentient, no fabricated user memory, no shaming players for a dry riverbed, no punishing players for skipping dialogues.

## 5. Thirty research locales ≠ thirty verified releases

See [LOCALIZATION.md](../LOCALIZATION.md). Current implementation provides **English + 29 offline JSON catalog files**, **124 compact keys per catalog**, device-language detection and a manual picker. `LocaleRuntime` keeps the preference separately from `WorldSnapshot`.

- Catalog completeness is **tested**; native-speaker grammar, tone and market relevance are **not**.
- Font fallback uses available OS fonts; do **not** redistribute proprietary fonts or assume glyph availability.
- RTL shaping/bidi and a Bengali native-text prototype now have [historical simulator/emulator observations](qa/native-text-2026-11-24/README.md). They still need native-reader, physical-device and assistive-technology review. Catalog completeness does not establish correct script rendering.
- UI captions, dialogue cards and playtest consent must remain correct when text expands 30–100%; avoid hardcoded clipping assumptions.
- Non-English puzzle names may be withheld in UI; NPC names use the locale catalog. Exported researcher notes use machine-readable English keys intentionally.

**Launch strategy:** open only languages that pass native copy review, right-to-left/complex-shaping device verification, screenshot and accessibility QA. Others remain in code as drafts.

## 6. Accessibility requirements / unresolved items

| Requirement | Current implementation | Real validation before launch |
| --- | --- | --- |
| Large touch targets | Dynamic grid aims ~90px for 3×3 and ~67px for 4×4 at reference 414px; smaller phones clamp lower | Verify effective pixel hit areas and non-overlap at 320px/landscape/tablets |
| Distinct states | blue-wave/sprout icons, hollow-circle open-join marker and warm protected-tile badge, water color + text counters | Colorblind comprehension, high contrast, distinguishable rotation/lock without relying on amber |
| No pressure | No countdown/streak; Undo, Restart, optional Guide me | Observe no shame/forced help; examine difficulty spikes |
| Hearing independence | Sound optional and default off | Verify mute/ringer and audio interruption on iOS/Android |
| Motion sensitivity | Gentle motion can be switched off; separate development-only miniature motion starts off | Verify all nonessential motion stops while state and controls remain clear |
| Language | 30 catalogs and OS-font lookup | True RTL, Indic shaping, Thai breaking, native copy QA |
| Assistive technology | First-pass screen-reader hierarchy (EG-113) publishes puzzle tiles, status, controls and choices on iOS/Android while a screen reader runs; Unity smoke passes | **Not** verified on VoiceOver/TalkBack devices; focus order, activation and frame accuracy are unproven; keyboard/gamepad accessibility not covered |
| Save durability | Backup/restore guards and a pinned error banner exist in source | Kill/upgrade/disk-full/migration tests on devices; banner readability and recovery still unverified |

**Do not equate accessibility-oriented source code with measured accessible UX.** At least one person who uses assistive technology should be included before making public accessibility claims.

## 6a. Motion audit (EG-114)

The earlier still-only audit predates the current painted UI. Current source includes gentle pipe turns, water/transition effects, choice settling and decorative butterflies. `evergrove:gentle-motion:v1` controls this motion separately from the save; it currently defaults on. Disabling it must retain the same board state and use immediate transitions. The development-only 2.5D miniature has a separate preference and starts still.

The preview itself must not grow the live garden or imply that a choice was saved. Selecting, comparing and closing should remain understandable with motion disabled. Verify both settings, OS accessibility interaction and physical-device behavior; no sensitivity testing with the 40+ audience is recorded.

## 7. Content QA and design acceptance

A chapter is “done” only when the water rules are valid and unique, the difficulty is observed rather than guessed, the previous choice is clearly seen in the next puzzle, the world illustration and dialogue correspond to the same event history, all supported live UI languages fit, the save survives interruption and the player can stop without guilt.

**Most expensive design failure:** content feels like the same pipe puzzle rotated, with an amber non-rotatable cell. If players correctly recognize the transform as superficial, stop producing more rotations; instead test one true physical dependency with an observable opportunity cost. See [validation](VALIDATION.md).

## Mosselora visual-system implementation (2026-11-23)

**Player-facing:** UI/English and all 29 *draft* locale titles now say **Mosselora**, while legacy `Evergrove` source paths, save identifiers, namespaces, asset paths and release automation stay stable. The subtitle's English source is “A living garden that remembers.” Other locale subtitles retain existing drafts. This working name still needs trademark clearance before public launch.

**Unity source:** one `MosseloraTheme.TileFill` rule now colors the initial board **and** updates after tap/Undo/hint, protecting truthful wet/dry/anchored states; rounded pipe hearts soften the plumbing feel without changing ports or reach; a hollow circle replaces the warning-like red exclamation-mark mismatch cue. All three ecological choices now share **equal visual weight** (the former default first-choice green emphasis could bias research). Larger decision typography, a darker forest-ink palette, and warmer 2D/2.5D cottage/stream art are included. Memory, journal, neighbors, choices, end and settings all inherit the same shared UI colors and text styling.

**Browser research:** matching botanical monogram, palette, open-circle explanation, branded copy and compact-viewport checks. It still implements exactly one Unity-authored puzzle; no saved progression. **This dated design increment is not current-source verification.** Later licensed builds and simulator/emulator checks are linked in [build QA](BUILD_AND_QA.md); the new preview still needs Unity, physical-device, screen-reader and player checks. Trademark clearance and player preference remain open. The two implementations are not pixel-identical and must not be assumed to be.

**Counterargument:** a richer illustration and unfamiliar hollow-circle mark might lower comprehension, particularly for adults 40+, despite looking friendlier. Cheapest test: show a competing build/screenshot and watch 8–12 uncoached players identify loose connections, make a first move, notice the creek restoration, and choose without leading questions. If 3/10 starters misread the goal or miss restoration, simplify the UI before expanding art.
