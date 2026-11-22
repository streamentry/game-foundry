# Evergrove — a world that remembers

**Status: integrated Unity/C# Chapter One playtest, not a published game or independently verified Unity Editor build.**

**Evidence-led product and feature specifications:** [docs/specs/](docs/specs/README.md) contains the complete 37-feature P0/P1/P2 backlog, player experience, puzzle/ecology/narrative contracts, accessibility/localization and device release gates, market evidence, experiments, six-week plan and GTM/stop rules. **These are proposals, not completed features or production approval.**

Evergrove is Game Foundry's founder-selected long-horizon research bet, **PH-006**, combining accessible visual puzzles with a *small, genuinely consequential* virtual village. This is a testable product direction, not evidence of a 2036 hit.

**Technology decision:** [Unity + C# ADR](../../docs/decisions/2026-11-22-evergrove-engine-unity-csharp.md). Unity is a provisional choice for Evergrove, subject to verified device and player tests; all other studio games retain Godot.

**Visual direction:** [2.5D miniature ADR](../../docs/decisions/2026-11-22-evergrove-visual-direction-2p5d.md). The **opt-in procedural 2.5D miniature prototype exists** beside the default 2D illustration; it has **not** passed Unity Editor/device QA or production art approval. See [handbook](docs/README.md).

## Canonical documentation (2026-11-22)

**Read the [full Evergrove product and engineering handbook](docs/README.md)**: [product](docs/PRODUCT.md), [game rules](docs/GAMEPLAY_AND_WORLD_RULES.md), [architecture](docs/ARCHITECTURE.md), [save v3/migrations](docs/SAVE_AND_MIGRATION.md), [UX and localization](docs/PLAYER_EXPERIENCE.md), [validation](docs/VALIDATION.md), [build QA](docs/BUILD_AND_QA.md), [risk review](docs/DECISIONS_AND_RISKS.md) and [tóm tắt tiếng Việt](docs/TOM_TAT.vi.md).

These documents clearly label implemented code, independent C# checks, unverified Unity/device behavior and commercial hypotheses.

## What the player does

1. Restore a winding stream by rotating tiles. No time limit, score penalty, account or advertisements.
2. Watch the spring's water reach more of the route. The display explains connected tiles and unmatched joins. An optional **Guide me** button reveals one connection without shame or a penalty.
3. After the water flows through every tile, choose one meaningful change: **plant shade trees**, **restore a footbridge**, or **grow a wildflower meadow**.
4. See an original illustrated world change, read the new memory (and revise a mis-tap before continuing), and return to discover combinations of previous choices. Mira, Orin and Fen respond to the same world history.
5. Reach a hand-authored conclusion after twelve streams. Review your complete history; optionally begin another playthrough with different choices.

**NEW — choices now change the NEXT water puzzle:** After chapter one, the last decision physically changes the next stream's layout and preserves a specific non-rotatable ecological landmark: tree roots, a bridge foundation, or a wildflower sanctuary. Different actions lead to different spring/outlet positions and pipe geometry; control mode keeps the original authored board unchanged. The target route remains mathematically unique, and restarting, hints and save/resume retain the actual transformed puzzle. This is a small deterministic cause-and-effect system, **not** yet a simulated food chain or an infinite AI world.

**What makes consequences meaningful:** tree + bridge creates a shaded crossing; tree + flowers attracts butterflies; bridge + flowers draws neighbors to the meadow. A balance of all three generates a different ending from a world focused on only one. Art, dialogue, journal and ending are derived from one persisted event sequence, not invented by a chatbot.

### Content and accessibility

- **12 authored puzzle chapters and 33 branch-sensitive follow-up variants:** six 3×3 fixtures from our own Pipe Garden research and six 4×4 fixtures. From chapters 2–12, the prior ecological choice selects one of three rotated topologies with a visibly protected connection. All variants are deterministic and have unique-solution tests; symmetry preserves difficulty broadly, not perfectly.
- **Causal memories:** three characters, explicit combination rules, four landscape phases, multiple epilogues and an ordered journal.
- **Visual design:** procedurally drawn original river, village, tree canopy, flowers, bridge, visitors and butterflies. No third-party visual assets.
- **Player respect:** large portrait-oriented touch targets, scrollable/safe-area-aware layout, undo, restart, optional hints, sound **off by default** with an opt-in gentle chime. No streak anxiety.
- **Offline-first:** a versioned JSON save (v3 retains original v1/v2 puzzles in progress) in `Application.persistentDataPath`, backup recovery, migration from the original 6-chapter PlayerPrefs snapshot, and a two-step confirmation for destructive resets.

The experience supports **Living World** and **Puzzle Only** for exploratory comparison. They share chapter 1; after that Living uses ecology-shaped boards while Puzzle Only uses originals. This is **not a clean narrative-only A/B** after chapter 1. See [experimental plan](docs/VALIDATION.md).

## Run in Unity

1. Open `games/evergrove/` in **Unity Hub**, using **Unity 6.3 LTS**. It currently requests `6000.3.0f1`.
2. Let Unity import uGUI and compile C# scripts. Choose **Evergrove → Create or Open Playtest Scene** to generate/open the empty game scene. You can also run **Evergrove → Verify C# Game and Scene** for a local Editor check.
3. Press **Play**. The UI and original art are generated from C#.
4. For physical iOS/Android builds, use the appropriate installed Unity platform modules and configure signing, bundle identifiers, orientation, input handling and device accessibility. **These have not been independently verified.**

For a licensed Unity Editor installation, a separate *Unity-engine* smoke check can be executed headlessly (its script import is the most relevant compilation gate for the uGUI classes):

```bash
Unity -batchmode -nographics -projectPath games/evergrove \
  -executeMethod Evergrove.UnityClient.Editor.EvergroveSceneSetup.VerifyProject \
  -logFile - -quit
```

This step **has not been run by this assistant**; no Unity license is bundled in CI. It creates the local editor scene and checks all puzzles. Device touch/IL2CPP still require their own checks.

The Game Foundry's other Godot games remain unchanged.

### Reproducible pure-C# checks

```bash
dotnet run --project games/evergrove/Tests/Evergrove.Core.Tests.csproj --configuration Release
```

This validates all 12 board shapes and unique solutions, source connectivity, hint/undo, consequence interactions, complete campaign, legacy saved-game migration and corrupted-save rejection. See [CI workflow](../../.github/workflows/ci.yml). These tests **do not compile the UnityEngine-specific UI or verify native builds**; Godot's tests do not test Evergrove.

## Language support: 30 locale choices

Evergrove now includes **English plus 29 additional offline translation catalogs**, with **automatic device-language selection** and an explicit **Language** button near the top of the game. You can change languages without losing your garden or restarting progression. The chosen language is saved separately from the world; **Automatic** follows the device language on future launches. Unrecognized locales fall back to English. For example, `zh-TW` → Traditional Chinese, `zh-CN` → Simplified Chinese, `pt-PT` → Brazilian Portuguese (the only supported Portuguese catalog).

The complete 76-key catalog covers the primary interface, all water-puzzle interaction copy, choices, compact localized NPC responses, world consequences, memories, journal and endings. The original, longer English narrative manuscript is preserved in source but not yet professionally translated: localization uses the same shorter authored game script for all 30 languages to keep gameplay meaning consistent. **Every catalog is draft quality until native-speaker review.** See [localization coverage, language list and QA gates](LOCALIZATION.md).

**Important rendering limitation:** Unity's current uGUI `Text` controls do not automatically provide the full advanced shaping required for Arabic, Persian, Hebrew, Urdu and Indic scripts. The implementation aligns RTL labels and selects available system fonts where possible, but **a proper advanced-text renderer and real-device font QA are release blockers** for those languages. Having a translation file is not the same as production-ready language support.

## Real playtest build and first-water moment

Evergrove now restores the **actual creek**, not just a progress counter. The new garden begins with a **dry riverbed**. Completing the first water puzzle brings flowing water to both the default 2D storybook and the optional 2.5D diorama; the later world choices still shape trees, crossings, flowers, visitors and the characters' responses. Scene rendering reads the same deterministic state and remains offline-first.

A [manual Unity 6.3 Android/WebGL/iOS-Xcode build workflow](../../.github/workflows/evergrove-unity-playtest.yml) and [step-by-step build/QA checklist](BUILD_QA.md) are provided. This is **not a published or signed app** and needs Unity licensing supplied privately through GitHub Actions secrets. It has not yet passed an actual Unity engine compilation or physical-device test. The Unity client uses the modern Unity Input System for touch compatibility (with a guarded legacy fallback if a project explicitly uses the old input manager).

**Optional, informed local study:** a button near the bottom of the game allows testers to begin collecting *anonymous session-level counters* (time to first turn/solve, hints, undo, choices, conversations and rendering-mode changes). Nothing is collected before consent; no identifiers, IP, accounts, SDK, network calls or passive background observation are added. The tester can **explicitly copy** the report to their clipboard or stop and discard it. The record exists in memory only and does **not** measure D1/D7 return. See `Assets/Scripts/Core/LocalPlaytest.cs` and [the experiment plan](../../docs/research/evergrove-build-decision-2026-11-22.md).

## Two original visual renderers — an explicit experiment

Evergrove now offers **2D storybook** (the original default baseline) and **2.5D miniature** (a switchable, opt-in orthographic diorama). Both draw **the same locally persisted world decisions**. The 2.5D scene is constructed in Unity from original procedural primitives: an island, winding creek, little cottage, trees, repaired crossing, flowers, butterflies and neighbors who move into different positions as the village evolves. It uses one orthographic camera rendered into a 768×384 texture; no prefabs, purchased art, model API or mandatory online assets.

The 2.5D preview is **not the chosen commercial art style yet**. A fair side-by-side test must show it improves readability or affection enough to justify its GPU/memory and authoring cost. See [the visual-comparison protocol](../../docs/research/evergrove-visual-comparison-protocol-2026-11-22.md) and the [2.5D art ADR](../../docs/decisions/2026-11-22-evergrove-visual-direction-2p5d.md).

The preview defaults to **no animation**. Players can enable a restrained 12fps butterfly/water/villager loop. No pressure mechanics or autoplay sound have been added.

**New, optional NPC conversation:** tap **Mira**, **Orin** or **Fen** after reaching the gameplay controls. Their authored words depend on the trees, crossings and meadows you actually chose, and remain identical after reloading the game. They do not track or pretend to remember conversations.

## Architecture and cloud

- Pure rules: `Assets/Scripts/Core/PipeRules.cs`
- Ecology-shaped watercourses: `Assets/Scripts/Core/ReactiveWatercourses.cs`
- World state, events, migration: `Assets/Scripts/Core/WorldModel.cs`
- Authored causal narrative and NPC conversations: `Assets/Scripts/Core/VillageStory.cs`
- Pure scene specification: `Assets/Scripts/Core/VillageScenePlan.cs`
- Optional original 3D miniature renderer: `Assets/Scripts/Unity/VillageDiorama.cs`
- UI and interactions: `Assets/Scripts/Unity/EvergroveApp.cs`
- Procedural landscape: `Assets/Scripts/Unity/VillageIllustration.cs`
- Offline saved progress: `Assets/Scripts/Unity/WorldStore.cs`

An [optional Cloudflare Durable Objects preview](../../backend/evergrove/README.md) is **not deployed or attached**. Its demo `WATER_TREE` action does not synchronize game decisions, NPC memory or puzzle state. Do not add live AI or cloud infrastructure until independent players demonstrate that the core loop merits it.

## What is still unknown

Editor compilation, screen-reader behavior, device safe areas, iOS/Android signing, app-store title/trademark clearance, 45+ audience suitability, voluntary D1/D7 return, measurable acquisition and monetization remain **unverified**. No authored game can be called “insanely great” without observing whether real people voluntarily return.

**Next non-negotiable test:** Build with actual Unity on a real phone; recruit 20–30 relevant adults for unprompted observation. Isolate the story, branch mechanics and 2D/2.5D visual effects in different tests. See [validation](docs/VALIDATION.md) and [QA](docs/BUILD_AND_QA.md).
