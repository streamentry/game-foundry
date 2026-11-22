# Decision: Evergrove as the one long-horizon game bet — build a falsifiable vertical slice

**Date:** 2026-11-22  
**Owner:** Game Foundry  
**Decision type:** Founder-selected *product direction + implementation of a research prototype*. **Not production greenlight, validated market demand, or a promise of a global hit.**

## Selection and overlap

**Evergrove (`PH-006`)**: a mobile-first, cozy living-world simulation entered through an accessible spatial puzzle. It integrates the persistent ownership idea of **Little Worlds (`GC-021`)** and mechanisms researched in **Neighbor Memory (`GX-002`)** and **Living Village (`LW-01`)**. Existing Pipe Garden (`GF-064`) provides a tested base puzzle for a fair world-on/off comparison. This is a new *experience* hypothesis, not a claim of an original pipe mechanic.

The founder explicitly chose Evergrove as the single most compelling multi-year creative bet over **Nexusforge** (higher platform ceiling but network-effect cold start), **Echoes of Aether** (AAA-grade world costs), and **Unfold** (clearer indie puzzle wedge but less ownership/retention scope).

**Caveat:** This is a founder conviction after reviewing idea families, **not** the result of a complete, quantitative cross-portfolio study. The [portfolio ranking audit](portfolio-ranking-audit-and-protocol-2026-11-22.md) remains valid. No one can responsibly predict a specific hit a decade out.

## Player promise and smallest loop

**Promise:** A world remembers what you do—not as endless AI text, but as consistent, comprehensible consequences.

1. Tap to rotate a 3×3 stream puzzle, matching every connection without leaks.
2. Once solved, choose **one** village change: plant shade, restore a crossing, or grow flowers.
3. The changed environment, persistent event history and three villagers' reactions reflect the choice.
4. Combining choices creates second-order consequences, such as shade by a restored crossing.
5. Resume offline and continue to the next stream; six fixtures provide the first short observation window.

**Control:** The *same* puzzle levels run in **Puzzle only** mode with the village layer absent. This isolates the incremental value of the persistent world more cleanly than comparing two different games, though outcomes may also differ due to extra time, interface complexity and self-selection.

## What is deliberately deleted

No LLM API, voice chat, infinite world generation, cloud, account, user profiling, UGC, multiplayer, marketplace, combat, economy, subscriptions, synthetic engagement metrics or tracking SDK. Start deterministic. Add runtime AI only if credible controlled comparisons show additional value greater than reliability, privacy, moderation and inference costs.

## Falsifiable assumptions

| Load-bearing hypothesis | Cheap disconfirmation |
| --- | --- |
| Puzzle-to-world transition creates *meaning*, not distraction | Players struggle to understand why they must solve a pipe puzzle to affect the village |
| Persistent choices cause voluntary return | Similar or better replay/return in the puzzle-only control |
| Three NPCs with bounded memory are sufficient to feel reactive | People cannot describe any coherent remembered consequence |
| Wordless spatial input opens a wide audience | Real target users cannot understand/operate the 3×3 grid without help |
| Founder-led distribution is plausible | No realistic way to bring participants/users beyond friends and personal network |

**Strongest opposing case:** A polished deterministic puzzle may outperform a partly simulated world by being clearer, cheaper, and faster. Greater engineering complexity can reduce, not improve, retention. Even a fun cozy game can fail if discoverability is weak.

## Seven-day player experiment (planned; no players tested yet)

- Recruit **20–30** target players, including age-band segmentation of adults 45+ when feasible; don't generalize if those participants are absent.
- Assign **Living world** versus **Puzzle only** conditions by a prewritten balanced schedule rather than letting preference self-select. Same boards, input and instructions; record device and prior puzzle familiarity.
- Observe first-minute comprehension, puzzle completion, whether players can explain the village consequence, and whether they voluntarily replay or return on D1 and D7. Keep raw counts/denominators and identify missing follow-up.
- Ask at the end: “What would make you open this tomorrow?” and “What felt unnecessary?” Do not treat polite praise as usage.
- **Continue to a three-NPC deeper simulation only if** users demonstrably understand the causal link, show credible voluntary return, and there is a plausible channel to recruit outside the founder's circle. A rough directional screen might seek more voluntary returns and preference for Living world than Puzzle only, but 20–30 participants cannot establish small effects reliably.
- **Stop or simplify** if puzzle-only is preferred, the story becomes an obligation, mobile readability fails or distribution has no realistic wedge. Costs must remain compatible with a small studio.

## Implementation footprint and correctness gates

**Deliverable:** `games/evergrove/` isolated Unity 6.3 C# project with six internally vendored Pipe Garden fixtures/rules, a visual rotation board, locally persisted world-choice history and deterministic character reactions, two comparison modes, Undo/Restart, pure C# tests for puzzle validity and state restoration, and a programmatically built Unity UI. No runtime web preview is claimed. An optional Cloudflare Workers/Durable Objects API is *separately* experimental and currently does not synchronize actual gameplay.

**Not yet established:** passing CI until workflow runs complete, verified iPhone/Android ergonomics, 45+ player feedback, retention, willingness to pay, trademark clearance, commercial viability, store approval or a publicly hosted game.

**Next observable action:** run web and phone playtests with real participants, collect the side-by-side replay/return evidence, and file one decision: *deepen / simplify / stop*. Do not expand the idea inventory merely because the project is now buildable.

## Engine change — 2026-11-22

At explicit user instruction **“hãy đổi qua C#”**, **Evergrove alone** changes from Godot/GDScript to **Unity/C#**. The other games retain their existing pinned Godot engine. The original unmerged Godot Evergrove implementation was replaced on its feature branch; the concept, state machine, six puzzle fixtures and head-to-head experiment remain. Backend stays Cloudflare Workers/TypeScript: C# is the client language, not a Cloudflare runtime. Cloud synchronization remains off by default and unproven. This exception to the studio-wide Godot convention should be reevaluated against Unity project upkeep and mobile export cost after the test.
