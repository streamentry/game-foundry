# ADR: 2.5D miniature living world as Evergrove's target visual direction

> **2026-11-22 implementation update:** Original future-tense statements below are historical. An opt-in procedural 2.5D renderer now exists at [VillageDiorama.cs](../../games/evergrove/Assets/Scripts/Unity/VillageDiorama.cs), but this is **not a verified Unity/device build or a production art approval**. See [current handbook](../../games/evergrove/docs/README.md).

- **Date:** 2026-11-22
- **Decision owner:** Founder / Game Foundry
- **Status:** **Accepted as a directional art hypothesis; not a production art-pipeline commitment**
- **Scope:** Evergrove (`PH-006`) only
- **Related:** [Mosselora design proposal](../../games/evergrove/design.md) · [prior implementation design](../../games/evergrove/DESIGN-SYSTEM.md) · [Unity/C# engine ADR](2026-11-22-evergrove-engine-unity-csharp.md) · [Evergrove game](../../games/evergrove/README.md) · [world-on/off experiment](../research/evergrove-build-decision-2026-11-22.md)

## Decision

**Aim for a stylized 2.5D miniature living village**: small, carefully authored 3D environments and characters presented through a **mostly fixed, elevated orthographic camera**, with **clear 2D puzzle and accessibility UI** overlaid on the world.

This is the **long-term visual direction to evaluate**, not authorization to rebuild the existing 2D prototype or commit to a full 3D pipeline before user evidence. The current Unity/C# Chapter One art uses original procedural **2D illustration**; keep it as the inexpensive baseline and playable research artifact.

The distinctive promise is **a little world that changes because the player was here**. Render fidelity must serve legible, persistent, cause-and-effect changes—not substitute for them.

## Why 2.5D fits the proposed game

1. **Emotional ownership:** A visible garden, stream, footbridge, paths and recognizable villagers can make choices feel tangible over weeks or years.
2. **Causal readability:** A stable camera makes *before/after* changes easy to notice: a sapling becomes shade, an opened crossing changes where villagers walk, flowers attract visitors. Avoid rearranging the camera when asking players to see consequences.
3. **Mobile friendliness:** A compact diorama with controlled framing can use small touch targets for the world only when appropriately designed; the puzzle itself remains a readable 2D interaction layer. Performance must be measured on real low/mid-tier devices.
4. **Sustainable authorship:** Reuse a small set of distinctive meshes, materials, rigs and hand-authored reactions rather than constructing a massive open world. Depth and animation are selective, not indiscriminate.
5. **Simulation independence:** NPC memory, resource/relationship rules and deterministic consequences remain in pure C# domain logic; visual presentation does not define what the world remembers.

These are **design hypotheses**, not demonstrated improvements to retention, monetization or discoverability.

## Definition of 2.5D for Evergrove

- **Environment:** lightweight 3D geometry for trees, buildings, bridges, riverbanks, flowers, paths and environmental props; distinctive stylized forms, restrained lighting and material complexity.
- **Camera:** orthographic, elevated and mostly fixed. Allow only measured, intentional movement if it improves legibility without disorienting the player. No mandatory free-roaming camera.
- **Characters:** three visually recognizable villagers initially; short readable walk/idle/interaction animations. Their positions and actions must correspond to the persisted world state.
- **Puzzle:** original high-contrast **2D** board and controls in UI space, consistent with the current playable stream-rotation mechanics. Do not make a crisp puzzle unnecessarily 3D.
- **World changes:** deterministic, inspectable triggers choose visible state variants and small staged transitions. Show *why* a village changed in both visuals and accessible text; never use color-only cues.
- **Performance:** target affordable mobile devices, support reduced-motion preferences, avoid expensive dynamic lighting or dense draw calls by default, and test before setting numeric FPS/memory budgets.
- **Visual personality:** tender, grounded, alive. Warm, handmade, calming diorama rather than photorealistic, maximalist or generic AI-generated art.

### What this is *not*

Not an unrestricted 3D open world, AAA asset production, procedural infinity, a promise of next-gen AI NPCs, a demand to replace the current 2D game immediately, or a new studio-wide engine decision. Unity/C# remains conditional under its separate ADR.

## Alternatives and strongest counterargument

| Direction | Strength | Cost / risk | Decision |
| --- | --- | --- | --- |
| **Excellent 2D illustration** | Most controllable art pipeline, fast iteration, often small build and GPU footprint, strong legibility | May make spatial movement and environmental transformations less expressive without more authored animation | **Keep as baseline and viable final option** |
| **2.5D fixed-camera diorama** | Legible, tactile depth with bounded camera and animation scope | 3D modeling, rigging, lighting, device performance, occlusion and authoring overhead | **Preferred long-term direction to test** |
| **Full free-camera 3D** | Potentially deep exploration and cinematic interactions | Stronger expectation of free navigation, environments, animation, camera controls and content volume | **Reject for initial scope** |

**Steel-man for 2D:** A beautiful hand-authored 2D world could generate equal or greater attachment for much less engineering and art cost. A diorama may be visually novel without increasing comprehension, agency or voluntary return. The simulation is not inherently more sophisticated because it is rendered in 3D.

## Two distinct experiments — do not mix their conclusions

### Product value (already planned)

**Living world versus Puzzle only**, same mechanics and puzzles. Test whether persistent environmental/NPC consequences add meaningful play and voluntary return. See [Evergrove build decision](../research/evergrove-build-decision-2026-11-22.md). This does **not** establish that 2.5D art is better.

### Art direction (new, separate and conditional)

Build one identical tiny **garden scene** in both styles **only when the playable core and recruitment are ready**:

- **A — 2D:** existing illustrated garden showing the stream, three villagers and three clear consequence changes.
- **B — 2.5D:** low-poly/stylized miniature of the *same* garden, same three villagers, same camera framing where possible, identical state transitions and text/controls.
- **Control:** same gameplay input, sound (ideally off), puzzle, story, dialogue, number and timing of changes, and device class; counterbalance viewing order.
- **Participants:** roughly **10–15 representative players** for a cheap qualitative preference/readability screen, not statistical proof of retention.
- **Observe:** can players spot and explain consequences unaided; ease of reading the board and NPCs; visual attachment; desire to return; comparison with an appealing polished genre reference; motion sensitivity and accessibility.
- **Engineering evidence:** measure real authoring hours per comparable asset/change, import/build friction, on-device cold start, runtime stability, memory/battery/GPU, loading and download size. Set quantitative budgets from target devices rather than inventing benchmarks.
- **Decision record:** keep raw notes, participant counts, failures and alternative explanations. Separate aesthetic preference from actual voluntary repeat play.

A comparison that gives B better art polish, more animation or more content is **not** isolating dimensionality. If art cannot be matched fairly, record that limitation instead of claiming a clean A/B.

## Seven-day next actions (pending editor/device access)

1. **Verify the current 2D Unity prototype on iOS/Android** first; document gameplay correctness and mobile UX without waiting for 3D artwork.
2. If current core is usable, assemble a **single garden-frame 2.5D mock/vertical slice** with one tree, one bridge, one flower meadow, and three NPC visual stand-ins; reuse pure C# state and 2D puzzle UI.
3. Give both styles to 10–15 suitable players, compare comprehension, attachment and time/cost on actual devices, and publish the result in the decision log.

**Continue with 2.5D** only when it demonstrably improves consequence readability and/or player attachment enough to justify its measured content and performance costs; **choose 2D** if outcomes are comparable or 2.5D slows iteration, makes text/interaction less readable, or performs poorly. **Do not expand to full 3D** without a separate compelling player need and architecture decision.

## Review trigger and scope control

Review after the first real-device art comparison and again after the core living-world-versus-puzzle-only test. Preserve code and production optionality. No new production approval, funding commitment, third-party art purchases, AI system or distribution claim is granted by this ADR.

> **North star:** A small beloved place, made meaningful by remembered choices—not a giant digital universe.
