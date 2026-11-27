# Mosselora — a living garden that remembers

**Internal project codename:** Evergrove. The source path, Unity namespaces, save keys, bundle/package identifiers and playtest build labels retain this internal name.

**Working player-facing brand: Mosselora** (previously Evergrove). Trademark clearance is not complete. **Status (2026-11-26): Unity/C# playtest with historical licensed builds and simulator/emulator evidence. This choice-preview increment has passed pure-C# checks but has not been compiled or exercised in Unity or on physical devices.** See [build QA](docs/BUILD_AND_QA.md).

**Design implementation (2026-11-23):** [Mosselora living-garden system](design.md) is now applied to the Unity first puzzle, later boards, choice/memory/journal surfaces, 2D illustration, optional 2.5D color direction, and the [offline browser first-minute slice](research/first-minute/README.md). The former `evergrove` folder, Unity class/namespace names, save keys and package IDs remain intentionally unchanged to protect saved worlds. Brand masters are committed under `brand/`; their presence is not a rights review or proof of shipping approval. C# and browser CI do not replace licensed Unity/device QA.

**Long-range vision:** [The Hundred-Year Garden](vision.md) sets the founder's north star: a lasting game with a 100-million lifetime-player goal and gentle family co-play, tested stage by stage through evidence gates. **It is an ambition, not a forecast or a build approval.**

**Evidence-led product and feature specifications:** [docs/specs/](docs/specs/README.md) contains the complete 37-feature P0/P1/P2 backlog, player experience, puzzle/ecology/narrative contracts, accessibility/localization and device release gates, market evidence, experiments, six-week plan and GTM/stop rules. **These are proposals, not completed features or production approval.** See the [M0–M6 execution and evidence gates](docs/specs/09-EXECUTION-CONTROL.md).

Evergrove is Game Foundry's founder-selected long-horizon research bet, **PH-006**, combining accessible visual puzzles with a *small, genuinely consequential* virtual village. This is a testable product direction, not evidence of a 2036 hit.

**Technology decision:** [Unity + C# ADR](../../docs/decisions/2026-11-22-evergrove-engine-unity-csharp.md). Unity is a provisional choice for Evergrove, subject to verified device and player tests; all other studio games retain Godot.

**Visual direction:** [2.5D miniature ADR](../../docs/decisions/2026-11-22-evergrove-visual-direction-2p5d.md). The **development-only procedural 2.5D miniature** exists beside the painted 2D garden and its procedural fallback. Its player value and physical-device cost remain unvalidated. See [handbook](docs/README.md).

## Canonical documentation

**Read the [full Evergrove product and engineering handbook](docs/README.md)**: [product](docs/PRODUCT.md), [game rules](docs/GAMEPLAY_AND_WORLD_RULES.md), [architecture](docs/ARCHITECTURE.md), [save v4/migrations](docs/SAVE_AND_MIGRATION.md), [UX and localization](docs/PLAYER_EXPERIENCE.md), [validation](docs/VALIDATION.md), [build QA](docs/BUILD_AND_QA.md), [risk review](docs/DECISIONS_AND_RISKS.md) and [tóm tắt tiếng Việt](docs/TOM_TAT.vi.md).

These documents clearly label implemented code, independent C# checks, unverified Unity/device behavior and commercial hypotheses.

## What the player does

1. Restore a winding stream by rotating tiles. No time limit, score penalty, account or advertisements.
2. Watch the spring's water genuinely reach more of the route: connected channels fill blue, dry pipes remain earth-colored, and the 2D upstream water advances or retracts as you rotate tiles. The display explains connected tiles and unmatched joins. Optional **Guide me** reveals one connection without shame or a penalty.
3. After the water flows through every tile, tap **plant shade trees**, **restore a footbridge**, or **grow a wildflower meadow** to inspect the result. Compare the choices, read what changes in the garden and see the actual next protected pipe. **Make this change** confirms; closing the preview leaves the garden untouched. The last chapter has no next-puzzle preview.
4. See an original illustrated world change, read the new memory (and revise a mis-tap before continuing), and return to discover combinations of previous choices. Mira, Orin and Fen respond to the same world history.
5. Reach a hand-authored conclusion after twelve streams. Review your complete history; optionally begin another playthrough with different choices.

**Choices change the next water puzzle:** After chapter one, the last decision physically changes the next stream's layout and preserves a specific non-rotatable ecological landmark: tree roots, a bridge foundation, or a wildflower sanctuary. Different actions select a board rotation and a protected tile; rotations preserve the connection graph. PuzzleOnly keeps the original board, while development-only PuzzleMatched can reproduce the same variant schedule without the story. The target route remains mathematically unique, and restarting, hints and save/resume retain the actual transformed puzzle. This is a small deterministic cause-and-effect system, **not** yet a simulated food chain or an infinite AI world.

**What makes consequences meaningful:** tree + bridge creates a shaded crossing; tree + flowers attracts butterflies; bridge + flowers draws neighbors to the meadow. A balance of all three generates a different ending from a world focused on only one. Art, dialogue, journal and ending are derived from one persisted event sequence, not invented by a chatbot.

### Content and accessibility

- **12 authored puzzle chapters and 33 branch-sensitive follow-up variants:** six 3×3 fixtures from our own Pipe Garden research and six 4×4 fixtures. From chapters 2–12, the prior ecological choice selects one of three rotated layouts with a visibly protected connection. All variants are deterministic and have unique-solution tests; rotation adds no new connection graph. The fixed tile changes the available actions, and perceived difficulty still needs player testing.
- **Causal memories:** three characters, explicit combination rules, four landscape phases, multiple epilogues and an ordered journal.
- **Visual design:** a painted garden, landmarks and neighbors, with a procedural 2D fallback. Both derive the garden state from committed choices.
- **Player respect:** large portrait-oriented touch targets, scrollable/safe-area-aware layout, undo, restart, three voluntary progressive hints (row → tile → one full alignment), non-color leakage indicators and a visible save-failure warning. Sound **off by default** with an opt-in gentle chime. No streak anxiety.
- **Offline-first:** a versioned JSON save (v4 preserves original v1/v2/v3 puzzles in progress and supports staged hint progress) in `Application.persistentDataPath`, backup recovery, migration from the original 6-chapter PlayerPrefs snapshot, and a two-step confirmation for destructive resets.

The experience supports **Living World** and **Puzzle Only** for exploratory comparison. They share chapter 1; after that Living uses ecology-shaped boards while Puzzle Only uses originals. This is **not a clean narrative-only A/B** after chapter 1. The **PuzzleMatched** mode and its facilitator UI exist in development builds for a puzzle-identical comparison; no human result is recorded. See [experimental plan](docs/VALIDATION.md).

## Play the first minute in a browser (research only)

Before setting up Unity, you can open the **[offline first-minute research slice](research/first-minute/index.html)** in any modern browser. It uses the **identical chapter-one pipe fixture and reachability rules**, with original responsive spring/creek art and three immediate visual choices. No account, tracking, external assets, server or build step; no complete 12-stream progression either.

It is a fast, testable **concept/interaction stimulus** to find out whether people *understand the water* and *notice the consequence*. It is **not** a Unity export or an acceptable substitute for M1/M2, native accessibility, retention or randomized narrative-value evidence. Read its [research README](research/first-minute/README.md) before interviewing players.

## Run in Unity

1. Open `games/evergrove/` in **Unity Hub**, using **Unity 6.3 LTS**. It currently requests `6000.3.0f1`.
2. Let Unity import uGUI and compile C# scripts. Choose **Evergrove → Create or Open Playtest Scene** to generate/open the empty game scene. You can also run **Evergrove → Verify C# Game and Scene** for a local Editor check.
3. Press **Play**. C# builds the interface and composes the committed art from the current world state.
4. For physical iOS/Android builds, use the appropriate installed Unity platform modules and configure signing, bundle identifiers, orientation, input handling and device accessibility. **These have not been independently verified.**

For a licensed Unity Editor installation, a separate *Unity-engine* smoke check can be executed headlessly (its script import is the most relevant compilation gate for the uGUI classes):

```bash
Unity -batchmode -nographics -projectPath games/evergrove \
  -executeMethod Evergrove.UnityClient.Editor.EvergroveSceneSetup.VerifyProject \
  -logFile - -quit
```

This step **has not been run for the 2026-11-26 changes**. Historical licensed smoke/build evidence is linked in [build QA](docs/BUILD_AND_QA.md); no Unity license is bundled in CI. Re-run against the current source. Device touch and platform behavior require separate checks.

The Game Foundry's other Godot games remain unchanged.

### Reproducible pure-C# checks

```bash
dotnet run --project games/evergrove/Tests/Evergrove.Core.Tests.csproj --configuration Release
```

This validates all 12 board shapes and unique solutions, source connectivity, hint/undo, consequence interactions, preview/commit equivalence without source mutation, the complete campaign, legacy migration, corrupted-save rejection and exact-state import of copied garden files with standard JSON whitespace. See [CI workflow](../../.github/workflows/ci.yml). These tests **do not compile the UnityEngine-specific UI or verify native builds**; Godot's tests do not test Evergrove.

## Language support: English release candidate, 30 research locales

Source includes **English plus 29 additional offline translation catalogs**. Development builds expose the research languages through device-language selection and a picker; release candidates stage uncertified catalogs out and currently offer English only. You can change languages without losing your garden or restarting progression. The chosen language is saved separately from the world; **Automatic** follows the device language on future launches. Unrecognized locales fall back to English. For example, `zh-TW` → Traditional Chinese, `zh-CN` → Simplified Chinese, `pt-PT` → Brazilian Portuguese (the only supported Portuguese catalog).

The current 127-key catalog covers the primary interface, all water-puzzle interaction copy, choices, compact localized NPC responses, world consequences, memories, journal and endings. The original, longer English narrative manuscript is preserved in source but not yet professionally translated: localization uses the same shorter authored game script for all 30 languages to keep gameplay meaning consistent. **The 29 non-English catalogs remain drafts pending native-speaker review; English copy also needs player comprehension checks.** See [localization coverage, language list and QA gates](LOCALIZATION.md).

**Important rendering limitation:** Unity's current uGUI `Text` controls do not automatically provide the full advanced shaping required for Arabic, Persian, Hebrew, Urdu and Indic scripts. The source now includes shaping/bidi helpers and a Bengali native-text prototype with historical simulator/emulator observations. **Native-reader and physical-device font QA remain release blockers** for those languages. Having a translation file is not the same as production-ready language support.

## Real playtest build and first-water moment

Evergrove restores the **actual creek**, not just a counter. The garden begins with a **partly wet spring inlet and dry downstream riverbed**; the 2D upstream water advances and retracts with real puzzle connectivity, while the village creek becomes fully restored **only** after a leak-free solution. Completing the first puzzle brings water to both the default 2D storybook and optional 2.5D diorama; the later world choices still shape trees, crossings, flowers, visitors and the characters' responses. Scene rendering reads the same deterministic state and remains offline-first.

A [manual Unity 6.3 Android/WebGL/iOS-Xcode build workflow](../../.github/workflows/evergrove-unity-playtest.yml) and [step-by-step build/QA checklist](BUILD_QA.md) are provided. Historical licensed compilation, exports and simulator/emulator checks are recorded in [build QA](docs/BUILD_AND_QA.md). The [2026-11-25 store handoff](docs/qa/mosselora-store-owner-handoff-2026-11-25.md) records an attached, unsubmitted Apple build 2 and no Google Play bundle; neither verifies current source. This increment still needs licensed compilation and physical-device testing. The Unity client uses the modern Unity Input System for touch compatibility (with a guarded legacy fallback if a project explicitly uses the old input manager).

**Optional, informed local study (development builds only):** research controls allow testers to begin collecting *anonymous session-level counters* (time to first turn/solve, hints, undo, choices, conversations and rendering-mode changes). Nothing is collected before consent; no identifiers, IP, accounts, SDK, network calls or passive background observation are added. The tester can **explicitly copy** the report to their clipboard or stop and discard it. The record exists in memory only and does **not** measure D1/D7 return. See `Assets/Scripts/Core/LocalPlaytest.cs` and [the experiment plan](../../docs/research/evergrove-build-decision-2026-11-22.md).

## Two original visual renderers — an explicit experiment

Mosselora uses the **painted 2D garden**, with an original procedural fallback. Development builds also expose the **2.5D miniature**, an opt-in orthographic diorama. Both draw **the same locally persisted world decisions**. The 2.5D scene is constructed in Unity from original procedural primitives: an island, winding creek, little cottage, trees, repaired crossing, flowers, butterflies and neighbors who move into different positions as the village evolves. It uses one orthographic camera rendered into a 768×384 texture; no prefabs, purchased art, model API or mandatory online assets.

The 2.5D preview is **not the chosen commercial art style yet**. A fair side-by-side test must show it improves readability or affection enough to justify its GPU/memory and authoring cost. See [the visual-comparison protocol](../../docs/research/evergrove-visual-comparison-protocol-2026-11-22.md) and the [2.5D art ADR](../../docs/decisions/2026-11-22-evergrove-visual-direction-2p5d.md).

The preview defaults to **no animation**. Players can enable a restrained 12fps butterfly/water/villager loop. No pressure mechanics or autoplay sound have been added.

**New, optional NPC conversation:** tap **Mira**, **Orin** or **Fen** after reaching the gameplay controls. Their authored words depend on the trees, crossings and meadows you actually chose, and remain identical after reloading the game. They do not track or pretend to remember conversations.

## Architecture and cloud

- Pure rules: `Assets/Scripts/Core/PipeRules.cs`
- Ecology-shaped watercourses: `Assets/Scripts/Core/ReactiveWatercourses.cs`
- World state, events, migration: `Assets/Scripts/Core/WorldModel.cs`
- Detached choice inspection: `Assets/Scripts/Core/VillageChoicePreview.cs`
- Authored causal narrative and NPC conversations: `Assets/Scripts/Core/VillageStory.cs`
- Pure scene specification: `Assets/Scripts/Core/VillageScenePlan.cs`
- Optional original 3D miniature renderer: `Assets/Scripts/Unity/VillageDiorama.cs`
- UI and interactions: `Assets/Scripts/Unity/EvergroveApp.cs`
- Procedural landscape: `Assets/Scripts/Unity/VillageIllustration.cs`
- Offline saved progress: `Assets/Scripts/Unity/WorldStore.cs`

The [Mosselora Cloudflare Worker backend](backend/README.md) is a separate prototype. It does not synchronize game decisions, NPC memory or puzzle state, and is not attached to the default game. The game remains playable offline; do not describe the API as cloud save or connect it to player data until security, quota, privacy and player-value gates pass.

## What is still unknown

Current-source Unity compilation, physical-device behavior, complete screen-reader review, app-store title/trademark clearance, 40+ suitability, family co-play, voluntary D1/D7 return, acquisition and monetization remain **unverified**. Earlier builds and screenshots establish only their recorded historical behavior. No authored game can be called “insanely great” without observing whether real people voluntarily return.

**Next real test:** compile this source in licensed Unity, run the updated development tour, then use physical phones for uncoached adult sessions and the existing five-family co-play protocol. Isolate the story, branch mechanics and 2D/2.5D visual effects in different tests. See [validation](docs/VALIDATION.md) and [QA](docs/BUILD_AND_QA.md).

### November 22 implementation increment (source-level, not a verified Unity build)

The [37-feature execution registry](docs/specs/verification-matrix.json) records EG-002/004/005/008/011/101 as **partially implemented and unverified**. This prototype now includes [45 deterministic puzzle profiles](docs/specs/puzzle-catalog.generated.json), a v4 recoverable-save format with persistent progressive hint stage, a concise cause-grounded journal, non-color (!) leak markers and a visible save-error notice. Domain tests check the solver and hint schema, **not** actual mobile text shaping or puzzle readability. Do not call the game release-ready before Unity, device and independent player gates pass.
