# Evergrove — a tiny world that remembers

**Status:** a **playable Godot 4.7.2 research vertical slice**, not a published or validated commercial game. Selected for a *small experimental build* on 2026-11-22, following the founder's single long-term game bet. Original concept `PH-006` (Evergrove), with `GC-021` (Little Worlds) as adjacent inspiration. See [decision record](../../docs/research/evergrove-build-decision-2026-11-22.md).

## The experience

Solve a short stream-connection puzzle, then choose one meaningful environmental consequence:

- **Plant a shade tree:** Mira remembers the shade you chose to plant.
- **Restore a footbridge:** Orin remembers the repaired crossing.
- **Grow wildflowers:** Fen remembers the meadow you protected.

The world records every choice locally. Combinations of choices change what happens to the village—e.g., planting a tree and repairing the crossing creates shade at the bridge. Six original, rule-verified 3×3 pipe fixtures are **deliberately reused from our own [Pipe Garden](../pipe-garden/README.md) prototype**, so the first experiment isolates the *world/persistence layer*, not mechanic novelty. The rules and fixture scripts are vendored within this standalone game, without fragile cross-project resource paths.

Select either **Living world** or **Puzzle only** before the first turn. The puzzle-only control uses the same boards and interactions without the village, characters or choices. This does **not** randomize A/B groups or collect analytics: recruit and assign players manually, recording informed consent and observed return.

## Run and test

Open `games/evergrove/project.godot` in the **Godot 4.7.2 stable** editor and press **F5**. The project runs offline, including on supported mobile builds, without a backend, LLM requests, accounts, tracking SDK, advertisements, purchases, or social services.

From the repository root, the automated checks are:

```bash
godot --headless --path games/evergrove --editor --import --quit
godot --headless --path games/evergrove --script res://tests/test_rules.gd
godot --headless --path games/evergrove --script res://tests/test_world.gd
godot --headless --path games/evergrove --script res://tests/test_scene.gd
```

The [Evergrove Web Playtest workflow](../../.github/workflows/evergrove-preview.yml) builds a **downloadable browser artifact** when manually dispatched or when Evergrove-related PR files change. It is not a public live URL. Extract the ZIP, run `python3 serve.py`, and open `http://127.0.0.1:8000/`. The browser must support WebGL2. Browser local saves may be removed by storage clearing or eviction.

## Why this particular vertical slice

The untested hypothesis is that **a small world with consistent consequences creates greater voluntary replay and return** than the exact same well-designed puzzle without the world. The studio's existing focus on adults 45+ remains a hypothesis, not a proven Evergrove audience.

Use the same six boards in both conditions. Compare player comprehension, reported reason to return, voluntary replay, and actual D1/D7 return with the same recruitment channel. Observe direct preference; do not claim scientific causal inference from a tiny sample.

## Boundaries

- Only six pipe puzzles and three deterministic NPC reaction tracks; **not** an infinite world, generative NPCs or an LLM experience.
- NPC memory is a local action history. Characters do not infer personality, monitor daily life or process conversations.
- No cloud sync, UGC, friends, in-game market, production-grade analytics, monetization, platform signing, or server economics.
- Original visual styling is an early systems-oriented interface, **not yet device-verified art direction**. Phone accessibility, consent, older-adult fit and retention remain untested.
- Working title **Evergrove** has not been trademark or store-availability screened.
- Do not multiply new puzzle levels or hire a live-ops team before comparable players demonstrate that consequences add value.

**Stop rule:** If players prefer the puzzle-only mode, do not justify world-building by citing future AI capabilities. Reduce or delete the simulation. If the living layer helps, test a three-NPC village with observable causal events before considering AI dialogue.
