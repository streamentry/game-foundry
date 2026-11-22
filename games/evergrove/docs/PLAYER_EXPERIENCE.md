# Evergrove — player experience, visual grammar, story and accessibility

**North-star sensation:** “I did one understandable thing, and the place remembers it.” Screens are means, not the product. **Current runtime UI is generated from code**; the following distinguishes implemented flow, design intent and unverified device behavior.

## 1. The intended first 90 seconds

| Moment | What the player should perceive | Implemented surface | Biggest uncertainty |
| --- | --- | --- | --- |
| Launch | A calm place, a dried creek, no login or advertisement | Warm paper UI; 2D original village art by default; local save | Does header/language selector/art push the first tile below fold? |
| First ~10 s | I can tap a pipe; the interaction does something | blue-wave/sprout endpoint icons, 3×3 board, one-tap rotation, changing channel graphics | Mobile hitbox/legibility and actual zero-coaching comprehension |
| First attempts | Water reaches more cells; a mismatch is understandable | Connected-tile count, leak feedback and highlighted reachable channels | Do players understand leaks, or guess until success? |
| First solution | The creek is flowing where it was dry | `VillageScenePlan.WaterRestored` and art state change | Is the restoration visibly rewarding without confetti? |
| First choice | I am choosing what this place will need next | Tree / crossing / flowers, promised outcome on each button | Do consequences feel meaningful or like three similar menu labels? |
| Memory beat | A character and environment acknowledge my action | `LocalizedVillage.MemoryEvent`; explicit choice correction | Does this deepen attachment or interrupt a short puzzle session? |
| Second chapter | My last choice matters mechanically | Rotated route, warm fixed landmark tile; short rule line | Does player connect this geometry to the previous decision without explanation? |

**Unverified duration:** 1–3 minutes per session is a design target, not a timed gameplay result. No claim of “instantly intuitive” until first-tap and completion observations are recorded.

## 2. Existing screen hierarchy and controls

```text
Portrait scrolling UI
  Brand + product headline
  Language selector (Automatic / 30-language picker)
  [Living mode] 2D illustration OR opt-in 2.5D miniature
  World consequence and one remembered action
  [If a choice was committed] memory interlude + Continue + Change choice
  [If campaign completed] authored epilogue + confirm-before-reset
  [Else]
    Stream title / environmental landmark notice
    One short prompt
    3x3 or 4x4 tap-to-rotate water board
       Blue waves = spring, green sprout = garden outlet
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

This is an **implementation map**, not a guarantee everything is visible without scrolling. Pressing a choice rebuilds the UI; board interactions update existing tile widgets. The 2D/2.5D renderer switch is intentionally below the core game, not a mandatory choice.

### Interaction states

- **Leak visibility:** unmatched/out-of-bounds water ports also show a per-tile **!** marker, supplementing shape, color and global leak feedback. Phone grayscale/readability test is still open.
- **Rotatable tile:** tap turns it clockwise by its allowed orientation cycle.
- **Protected landscape tile:** after chapter one, one non-endpoint tile is fixed, amber/marked and non-interactable. The explanation references roots/foundation/sanctuary.
- **Solved board:** current water path is complete, no leaks; further rotate is blocked, undo may reopen a previous move.
- **Choice state:** one tree, crossing, or flower decision and a short promise.
- **Memory state:** shows the result; **Change your choice** reopens the prior solved puzzle without replaying it.
- **Completion:** after 12 Living choices, show a conclusion. In PuzzleOnly, the campaign ends after advancing from the final solved board. Restart/erase is two-step.
- **Audio:** silent by default; optional original procedural short chime.
- **Motion:** miniature still by default; optional subtle low-frequency water/villager/butterfly motion.
- **Playtest:** never records before deliberate opt-in; explicit clipboard export or in-memory erase.

## 3. Art design language

Canon: [DESIGN.md](../DESIGN.md). Working visual tokens:

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

**Default 2D:** original Unity-generated `VillageIllustration`. Shows a creek changing from dry to flowing; cumulative trees, crossing, flowers, butterflies, visitors and later landscape hues.

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

## 5. 30 selectable languages ≠ 30 verified releases

See [LOCALIZATION.md](../LOCALIZATION.md). Current implementation provides **English + 29 offline JSON catalog files**, **76 compact translated keys per catalog**, device-language detection and a manual picker. `LocaleRuntime` keeps the preference separately from `WorldSnapshot`.

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
| Distinct states | Blue-wave/sprout endpoint icons, visible ! leak cues and warm protected-tile badge, water color + text counters | Colorblind comprehension, high contrast, distinguishable rotation/lock without relying on amber |
| No pressure | No countdown/streak; Undo, Restart, optional Guide me | Observe no shame/forced help; examine difficulty spikes |
| Hearing independence | Sound optional and default off | Verify mute/ringer and audio interruption on iOS/Android |
| Motion sensitivity | 2.5D motion optional/off initially | Test OS reduced-motion preference; check flashing and 12fps animation artifacts |
| Language | 30 catalogs and OS-font lookup | True RTL, Indic shaping, Thai breaking, native copy QA |
| Assistive technology | UI object names and labels exist in code | **Not** proven VoiceOver/TalkBack semantics, focus ordering or keyboard/gamepad accessibility |
| Save durability | Backup and restore attempts | Kill/upgrade/disk-full/migration tests on devices; player-visible save error missing |

**Do not equate accessibility-oriented source code with measured accessible UX.** At least one person who uses assistive technology should be included before making public accessibility claims.

## 7. Content QA and design acceptance

A chapter is “done” only when the water rules are valid and unique, the difficulty is observed rather than guessed, the previous choice is clearly seen in the next puzzle, the world illustration and dialogue correspond to the same event history, all supported live UI languages fit, the save survives interruption and the player can stop without guilt.

**Most expensive design failure:** content feels like the same pipe puzzle rotated, with an amber non-rotatable cell. If players correctly recognize the transform as superficial, stop producing more rotations; instead test one true physical dependency with an observable opportunity cost. See [validation](VALIDATION.md).
