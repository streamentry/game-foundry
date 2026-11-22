# Evergrove — a world that remembers

**Status: integrated Unity/C# Chapter One playtest, not a published game or independently verified Unity Editor build.**

Evergrove is Game Foundry's founder-selected long-horizon research bet, **PH-006**, combining accessible visual puzzles with a *small, genuinely consequential* virtual village. This is a testable product direction, not evidence of a 2036 hit.

## What the player does

1. Restore a winding stream by rotating tiles. No time limit, score penalty, account or advertisements.
2. Watch the spring's water reach more of the route. The display explains connected tiles and unmatched joins. An optional **Guide me** button reveals one connection without shame or a penalty.
3. After the water flows through every tile, choose one meaningful change: **plant shade trees**, **restore a footbridge**, or **grow a wildflower meadow**.
4. See an original illustrated world change, read the new memory (and revise a mis-tap before continuing), and return to discover combinations of previous choices. Mira, Orin and Fen respond to the same world history.
5. Reach a hand-authored conclusion after twelve streams. Review your complete history; optionally begin another playthrough with different choices.

**What makes consequences meaningful:** tree + bridge creates a shaded crossing; tree + flowers attracts butterflies; bridge + flowers draws neighbors to the meadow. A balance of all three generates a different ending from a world focused on only one. Art, dialogue, journal and ending are derived from one persisted event sequence, not invented by a chatbot.

### Content and accessibility

- **12 original independent puzzles:** six 3×3 fixtures from our own Pipe Garden research, six fresh 4×4 fixtures with deterministic uniqueness tests.
- **Causal memories:** three characters, explicit combination rules, four landscape phases, multiple epilogues and an ordered journal.
- **Visual design:** procedurally drawn original river, village, tree canopy, flowers, bridge, visitors and butterflies. No third-party visual assets.
- **Player respect:** large portrait-oriented touch targets, scrollable/safe-area-aware layout, undo, restart, optional hints, sound **off by default** with an opt-in gentle chime. No streak anxiety.
- **Offline-first:** a versioned JSON save in `Application.persistentDataPath`, backup recovery, migration from the original 6-chapter PlayerPrefs snapshot, and a two-step confirmation for destructive resets.

The experience supports **Living world** and **Puzzle only** (the same water puzzles without narrative) for controlled research. This is a *research comparison mode*, not analytics or proper randomized A/B assignment.

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

## Architecture and cloud

- Pure rules: `Assets/Scripts/Core/PipeRules.cs`
- World state, events, migration: `Assets/Scripts/Core/WorldModel.cs`
- Authored causal narrative: `Assets/Scripts/Core/VillageStory.cs`
- UI and interactions: `Assets/Scripts/Unity/EvergroveApp.cs`
- Procedural landscape: `Assets/Scripts/Unity/VillageIllustration.cs`
- Offline saved progress: `Assets/Scripts/Unity/WorldStore.cs`

An [optional Cloudflare Durable Objects preview](../../backend/evergrove/README.md) is **not deployed or attached**. Its demo `WATER_TREE` action does not synchronize game decisions, NPC memory or puzzle state. Do not add live AI or cloud infrastructure until independent players demonstrate that the core loop merits it.

## What is still unknown

Editor compilation, screen-reader behavior, device safe areas, iOS/Android signing, app-store title/trademark clearance, 45+ audience suitability, voluntary D1/D7 return, measurable acquisition and monetization remain **unverified**. No authored game can be called “insanely great” without observing whether real people voluntarily return.

**Next non-negotiable test:** recruit 20–30 adults in the target cohort, manually balance between Living world and Puzzle only, record first-minute comprehension, spontaneous return and head-to-head preference versus a polished existing title. The [experiment protocol](../../docs/research/evergrove-build-decision-2026-11-22.md) defines the stop rule.
