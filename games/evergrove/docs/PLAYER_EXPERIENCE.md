# Evergrove — player experience, visual grammar, story and accessibility

**North-star sensation:** “I did one understandable thing, and the place remembers it.” Screens are means, not the product. **Current runtime UI is generated from code**; the following distinguishes implemented flow, design intent and unverified device behavior.

**A separate research-only browser slice** at [research/first-minute](../research/first-minute/README.md) now implements only the first Unity-authored puzzle and one illustrated environmental decision. It is for low-cost first-minute comprehension experiments while M1 Unity compilation remains blocked—not a playable Unity release or the 12-stream app. Unity/mobile rendering remains unverified.

## 1. The intended first 90 seconds

| Moment | What the player should perceive | Implemented surface | Biggest uncertainty |
| --- | --- | --- | --- |
| Launch | A calm place and an immediately tappable puzzle, no login or advertisement | The Unity first-chapter source now builds the puzzle **before** the optional village illustration; the water/creek are still inspectable below | Do the first tile and its tap feedback actually fit and remain discoverable at 320px and larger phones? |
| First ~10 s | I can tap a pipe; the interaction does something | blue-wave/sprout endpoint icons, 3×3 board, one-tap rotation, changing channel graphics | Mobile hitbox/legibility and actual zero-coaching comprehension |
| First attempts | An additional connection visibly carries water; a lost connection visibly dries | Only spring-connected pipe geometry becomes blue; dry channels stay earthen; 2D creek advances with reach; connected count and hollow-circle open-join cues remain | Do people grasp real connectivity without coaching, including grayscale play? |
| First solution | The previously dry downstream creek fills completely | `VillageScenePlan.WaterRestored` switches only after complete leak-free connectivity; the pre-choice localized consequence now uses `solved` instead of the stale dry-creek `world_none` label | Is this more rewarding than incremental flow, without confetti? |
| First choice | I am choosing what this place will need next | Tree / crossing / flowers, promised outcome on each button | Do consequences feel meaningful or like three similar menu labels? |
| Memory beat | A character and environment acknowledge my action | `LocalizedVillage.MemoryEvent`; explicit choice correction | Does this deepen attachment or interrupt a short puzzle session? |
| Second chapter | My last choice matters mechanically | Rotated route, warm fixed landmark tile; short rule line | Does player connect this geometry to the previous decision without explanation? |

**Unverified duration:** 1–3 minutes per session is a design target, not a timed gameplay result. No claim of “instantly intuitive” until first-tap and completion observations are recorded.

## 2. Existing screen hierarchy and controls

```text
Portrait scrolling UI
  Brand + product headline
  Language selector (Automatic / 30-language picker) and text size "Aa 100% / 125% / 150%" (language-neutral; cycles on tap), above the board except on the post-choice screen
  [Living mode] 2D illustration OR opt-in 2.5D miniature
  World consequence and one remembered action
  [If a choice was committed] world consequence, memory card, Continue + Change choice, then the language/text-size row (unverified with players)
  [If campaign completed] authored epilogue + confirm-before-reset
  [Else]
    Stream title / environmental landmark notice
    One short prompt
    3x3 or 4x4 tap-to-rotate water board
       Blue wave icon = spring, green sprout = garden outlet
       visibly marked non-rotatable environmental tile on branched boards
    Connected water / leak feedback
    Undo, Guide me, Restart (or choices after solved)
  Neighbor conversations (Living mode)
  Journal (Living mode)
  Research-only mode selector (before any committed progress)
  Art toggle and optional motion; optional gentle sound
  Optional local-playtest recorder + explicit copy/discard
  Offline, no-pressure footer
```

This is an **implementation map**, not a guarantee everything is visible without scrolling. The unsolved first board is now ordered before the illustrated village in Unity source; on solve, an anchored scroll goes to the actual choice heading with preceding context instead of blindly to the footer. These positions require **real-device measurement** at 320px/large text/safe-area sizes. Pressing a choice rebuilds the UI; board interactions update existing tile widgets. The 2D/2.5D renderer switch is intentionally below the core game, not a mandatory choice.

### Interaction states

- **Leak visibility:** unmatched/out-of-bounds water ports also show a per-tile **!** marker, supplementing shape, color and global leak feedback. Phone grayscale/readability test is still open.
- **Rotatable tile:** tap turns it clockwise by its allowed orientation cycle.
- **Protected landscape tile:** after chapter one, one non-endpoint tile is fixed, amber/marked and non-interactable. The explanation references roots/foundation/sanctuary.
  - **Landmark marker (2026-11-23):** the protected tile gets a heavy bark-brown frame plus a badge in its top-left corner, drawn procedurally by `LandmarkGraphic` with no new art. The badge matches the choice: root tendrils (tree), a stone footing under a plank deck (bridge), or a five-petal flower (wildflower sanctuary). The badge sits outside every channel arm, so pipe geometry and endpoint markers stay unchanged. The marker is non-color: frame thickness and badge shape carry the meaning. The text line below the board still reads "…anchor this pipe. It cannot rotate." without naming the marker. **Unverified with players:** the QA screenshots (`docs/qa/mac-player-2026-11-23/tour/07–09`) show the shapes are distinct on a 540×960 frame, but no adult 45+ playtest has confirmed that the marker is noticed unaided.
- **Text size (EG-007, implemented 2026-11-23):** one central scale in `TextScale` applies to every generated label, button, save banner and dev-only input. The choice is stored in PlayerPrefs (`evergrove:text-scale:v1`) and never touches the world save. Default 100%. Buttons and rows keep a scaled minimum height and grow with their text; board tiles, pipe geometry and the tile-embedded markers (open-join circle, hint "?") stay fixed. QA evidence at 150%, 375x667: `docs/qa/mac-player-2026-11-23/tour-text150-375/`. Dev builds accept `-mosseloraTextScale 150` for one run without persisting it. **Unverified:** physical devices, OS text-size interaction, and adult 45+ testing.
- **Solved board:** current water path is complete, no leaks; further rotate is blocked, undo may reopen a previous move.
- **Choice state:** one tree, crossing, or flower decision and a short promise.
- **Memory state:** shows the result; **Change your choice** reopens the prior solved puzzle without replaying it.
- **Completion:** after 12 Living choices, show a conclusion. In PuzzleOnly, the campaign ends after advancing from the final solved board. Restart/erase is two-step.
- **Audio:** silent by default; optional original procedural short chime.
- **Motion:** miniature still by default; optional subtle low-frequency water/villager/butterfly motion.
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

**Default 2D:** original Unity-generated `VillageIllustration`. The upstream inlet may be partly wet at launch; its water front grows or retracts with the *actual spring-connected graph*, while the downstream creek stays dry until solved. Trees, crossings, flowers, butterflies, visitors and landscape changes remain driven by saved choices. This is source-level behavior, not device-tested art direction.

**Optional 2.5D:** `VillageDiorama` creates a small orthographic primitive village, with single-camera rendering to a 768×384 texture. Does not need licensed assets or network. Motion is off by default and not proof of game-quality animation. It must fall back to 2D on unsupported devices.

**Quality risk:** the procedural art is a compelling prototype **at best**; generic low-poly geometry and Unity default fonts are not yet comparable to premium hand-authored art. A real-world head-to-head comparison must include a polished reference, not only the two in-house options.

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

See [LOCALIZATION.md](../LOCALIZATION.md). Current implementation provides **English + 29 offline JSON catalog files**, **81 compact translated keys per catalog**, device-language detection and a manual picker. `LocaleRuntime` keeps the preference separately from `WorldSnapshot`.

- Catalog completeness is **tested**; native-speaker grammar, tone and market relevance are **not**.
- Font fallback uses available OS fonts; do **not** redistribute proprietary fonts or assume glyph availability.
- For Arabic, Persian, Urdu and Hebrew, text alignment is **not** sufficient bidi shaping. Indic conjuncts and Thai word-breaking need real device testing. Modern advanced text shaping/rendering may require a UI stack change, not more translation strings.
- UI captions, dialogue cards and playtest consent must remain correct when text expands 30–100%; avoid hardcoded clipping assumptions.
- Non-English puzzle names may be withheld in UI; character proper names remain stable. Exported researcher notes use machine-readable English keys intentionally.

**Launch strategy:** open only languages that pass native copy review, right-to-left/complex-shaping device verification, screenshot and accessibility QA. Others remain in code as drafts.

## 6. Accessibility requirements / unresolved items

| Requirement | Current implementation | Real validation before launch |
| --- | --- | --- |
| Large touch targets | Dynamic grid aims ~90px for 3×3 and ~67px for 4×4 at reference 414px; smaller phones clamp lower | Verify effective pixel hit areas and non-overlap at 320px/landscape/tablets |
| Distinct states | blue-wave/sprout icons, hollow-circle open-join marker and warm protected-tile badge, water color + text counters | Colorblind comprehension, high contrast, distinguishable rotation/lock without relying on amber |
| No pressure | No countdown/streak; Undo, Restart, optional Guide me | Observe no shame/forced help; examine difficulty spikes |
| Hearing independence | Sound optional and default off | Verify mute/ringer and audio interruption on iOS/Android |
| Motion sensitivity | 2.5D motion optional/off initially | Test OS reduced-motion preference; check flashing and 12fps animation artifacts |
| Language | 30 catalogs and OS-font lookup | True RTL, Indic shaping, Thai breaking, native copy QA |
| Assistive technology | UI object names and labels exist in code | **Not** proven VoiceOver/TalkBack semantics, focus ordering or keyboard/gamepad accessibility |
| Save durability | Backup/restore guards and a pinned error banner exist in source | Kill/upgrade/disk-full/migration tests on devices; banner readability and recovery still unverified |

**Do not equate accessibility-oriented source code with measured accessible UX.** At least one person who uses assistive technology should be included before making public accessibility claims.

## 7. Content QA and design acceptance

A chapter is “done” only when the water rules are valid and unique, the difficulty is observed rather than guessed, the previous choice is clearly seen in the next puzzle, the world illustration and dialogue correspond to the same event history, all supported live UI languages fit, the save survives interruption and the player can stop without guilt.

**Most expensive design failure:** content feels like the same pipe puzzle rotated, with an amber non-rotatable cell. If players correctly recognize the transform as superficial, stop producing more rotations; instead test one true physical dependency with an observable opportunity cost. See [validation](VALIDATION.md).

## Mosselora visual-system implementation (2026-11-23)

**Player-facing:** UI/English and all 29 *draft* locale titles now say **Mosselora**, while legacy `Evergrove` source paths, save identifiers, namespaces, asset paths and release automation stay stable. The subtitle's English source is “A living garden that remembers.” Other locale subtitles retain existing drafts. This working name still needs trademark clearance before public launch.

**Unity source:** one `MosseloraTheme.TileFill` rule now colors the initial board **and** updates after tap/Undo/hint, protecting truthful wet/dry/anchored states; rounded pipe hearts soften the plumbing feel without changing ports or reach; a hollow circle replaces the warning-like red exclamation-mark mismatch cue. All three ecological choices now share **equal visual weight** (the former default first-choice green emphasis could bias research). Larger decision typography, a darker forest-ink palette, and warmer 2D/2.5D cottage/stream art are included. Memory, journal, neighbors, choices, end and settings all inherit the same shared UI colors and text styling.

**Browser research:** matching botanical monogram, palette, open-circle explanation, branded copy and compact-viewport checks. It still implements exactly one Unity-authored puzzle; no saved progression. **M1 Unity import/compile, iOS/Android, screen-reader checks, trademark review, contrast audit and actual player liking remain UNVERIFIED.** The two implementations are not pixel-identical and must not be assumed to be.

**Counterargument:** a richer illustration and unfamiliar hollow-circle mark might lower comprehension, particularly for adults 45+, despite looking friendlier. Cheapest test: show a competing build/screenshot and watch 8–12 uncoached players identify loose connections, make a first move, notice the creek restoration, and choose without leading questions. If 3/10 starters misread the goal or miss restoration, simplify the UI before expanding art.
