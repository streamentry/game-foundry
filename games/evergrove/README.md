# Evergrove — a living world in Unity/C#

**Status:** C# implementation of a local-first, experimental game. **Not a shipped app or verified Unity build.** It is the founder-selected long-horizon bet `PH-006`, with influences from `GC-021 Little Worlds`. See [experiment decision](../../docs/research/evergrove-build-decision-2026-11-22.md).

**Technology decision:** [Unity + C# ADR](../../docs/decisions/2026-11-22-evergrove-engine-unity-csharp.md). This is a provisional Evergrove-specific choice; re-evaluate on actual mobile devices and after the first playtest, rather than treating a ten-year engine choice as permanent.

## Gameplay

Tap to rotate nine stream tiles to create one continuous water route. Once the water flows, decide what the village gains:

- **Plant a shade tree** — Mira remembers its shade.
- **Repair the footbridge** — Orin remembers the crossing.
- **Grow wildflowers** — Fen remembers the meadow.

The next chapter sees the persistent consequences, including combinations such as a bridge shaded by a tree. Six deterministic, independently checked fixtures were ported from the repo's own Pipe Garden experiment; no proprietary artwork or levels are used.

**Comparative research:** Select **Living world** or **Puzzle only** before your first turn. The latter plays the exact same six water puzzles without choices, NPCs or village panel. The built-in mode switch is for manual usability checks, **not** randomized A/B assignment or analytics.

## Open in Unity

1. Install Unity Hub and **Unity 6.3 LTS** (the project requests `6000.3.0f1`; if your editor uses a later 6.3 patch, Unity Hub may prompt for a migration).
2. In Hub choose **Add project from disk** and select `games/evergrove/`. The package manifest includes uGUI (`com.unity.ugui`); opening the project resolves packages and imports scripts.
3. Open the **Evergrove → Create Playtest Scene** editor menu, which creates `Assets/Scenes/Evergrove.unity` and marks it as the build scene. Press **Play**.
4. The scene contains no hand-authored game objects. `EvergroveApp` generates a mobile-readable uGUI canvas, controls and pipe artwork at runtime. The C# puzzle state and world model use no Unity dependency.

No backend, account, telemetry, payment or AI models are needed to play. Progress is saved locally via Unity `PlayerPrefs`. This is sufficient for an experiment, **not** production-grade atomic persistence, secure secret storage or cloud reconciliation.

## Run the deterministic C# tests

The pure-C# core is compiled separately from Unity:

```bash
dotnet run --project games/evergrove/Tests/Evergrove.Core.Tests.csproj
```

This verifies all six puzzle solutions, uniqueness constraints, rotation and undo, memory/choice effects, complete progression and rejection of corrupted local snapshots. **Unity Editor/device gameplay still needs independent human testing.** The repository CI runs this .NET command automatically.

## Optional Cloudflare Workers sync experiment

`Assets/Scripts/Unity/WorldClient.cs` is a **separate, disabled-by-default** C# HTTP client for [the optional Durable Objects backend](../../backend/evergrove/README.md). The Worker stays TypeScript because Cloudflare Workers do not run Unity C# directly.

The demo API only implements `WATER_TREE` with a single capped `treeStage` and a per-world revision. **It does not synchronize Evergrove's actual world history or puzzle/choice progression.** Do not attach this client to production play or present it as working full-world sync.

## Deliberate exclusions

No LLM-driven NPCs, UGC, multiplayer, cloud requirement, live economy, engagement-streak manipulation, subscriptions, monetization SDKs or AI behavioral tracking. The working title `Evergrove` is not cleared for trademark or store use.

## Next falsifiable step

With 20–30 relevant participants, balance players manually between Living world and Puzzle only. Measure **comprehension, voluntary replay, unprompted D1/D7 return**, preference versus a polished existing game, and whether players can explain the cause of village changes. Record raw denominators and selection bias. **Stop or simplify the simulation if the fixed-rule puzzle is preferred.** Do not add AI, online services or content scale until the living layer clearly earns its cost.

## Technical scope

- **Client:** Unity C# (uGUI + procedural drawing).
- **Pure domain logic:** `Assets/Scripts/Core/PipeRules.cs`, `WorldModel.cs`; six ported fixtures.
- **Local state:** `Assets/Scripts/Unity/WorldStore.cs`.
- **Editor scene creation:** `Assets/Editor/EvergroveSceneSetup.cs`.
- **Server experiment:** TypeScript Worker and SQLite-backed Durable Object, separate.
- **Status:** Editor/IL2CPP, touch latency, dynamic text, accessibility semantics and real phone layouts **not verified**. No signed builds or store releases.
