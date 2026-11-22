# 04 — Living world, consequence and truthful memory

**Design contract:** the world remembers **what the player actually did**, not a fabricated chat history or an infinite simulation. Every narrative, scene and puzzle consequence must be traceable to a deterministic saved choice or completed stream.

## Existing implementation, with limits

- The authoritative `WorldModel` holds an ordered array of choices: `ShadeTree`, `Bridge`, `Flowers`.
- `VillageStory`, `LocalizedVillage` and `VillageScenePlan` derive landscape phases, dialogue, journal and endings from that history.
- Mira, Orin and Fen have distinct authored state-dependent conversations. They **do not** track conversations or generate runtime text.
- The first solved stream turns the visible creek from dry to flowing; tree/bridge/meadow choices change scene details. The localized **pre-choice consequence** must immediately switch from `world_none` (dry) to the existing translated `solved` key (flowing), even before any ecological choice is committed. This prevents the real Unity view from contradicting its own flowing river.
- Existing combinations: **tree + bridge → shaded crossing; tree + flowers → butterflies; bridge + flowers → neighbors/visitors; all three → balanced shared-world outcome**.
- A prior choice rotates the next board and anchors a protected landmark. This is a real deterministic variant but a **shallow puzzle-mechanics change**.

Source: [WorldModel.cs](../../Assets/Scripts/Core/WorldModel.cs), [VillageStory.cs](../../Assets/Scripts/Core/VillageStory.cs), [LocalizedVillage.cs](../../Assets/Scripts/Core/LocalizedVillage.cs), [VillageScenePlan.cs](../../Assets/Scripts/Core/VillageScenePlan.cs), [ReactiveWatercourses.cs](../../Assets/Scripts/Core/ReactiveWatercourses.cs).

## Consequence registry proposal — one truth, four outputs

The existing `WorldModel` ordered choice history remains authoritative. `VillageScenePlan`, `VillageStory`, `LocalizedVillage` and `ReactiveWatercourses` are deterministic projections; **do not create a second mutable NPC truth store**.

Before implementing deep ecology rules (EG-102/105), define an inspectable, data-driven `ConsequenceRegistry` with a record per real choice:

```text
ChoiceId
RequiredState
ForbiddenState
ImmediateVisualEffectIds[]
NextPuzzleRuleId
NpcMemoryIds[]
JournalEntryId
DelayedWorldEffectIds[]
EndingContributionIds[]
LocalizationKeys[]
```

**Required invariants (`WLD-001..004`):** (1) choice → actual scene elements; (2) choice → NPC dialogue traceable to saved history; (3) choice → the **actual** next puzzle rule, never a proposed one; (4) choice → journal and ending contributions; (5) reconsider/undo/reload update all derived outputs. No NPC may remember an unmade action, no journal may describe a non-existent ecological rule, and no landmark may appear without its trigger.

**Current limitation:** the branch puzzle currently rotates an authored topology and fixes an already-correct tile. It does **not** deliver independently modeled root/culvert/meadow constraints. `EG-105` stays `Proposed` until mechanics genuinely alter the reasoning. Only pursue a deeper registry **after M3** shows players notice the current immediate reveal.

## One observable causal loop

`Solve stream → see water reach the village → choose tree/bridge/flowers → see physical before/after → hear one truthful reaction → see a consequence in the next puzzle → recall it later.`

**Reveal order:** (1) show changed physical object, (2) visually connect to the player's choice, (3) short NPC response, (4) optional journal detail. Avoid long mandatory cutscenes. Reduced-motion users receive the same state through static before/after frames and text.

### Proposed consequence matrix (EG-102)

| Choice/event | Immediate physical payoff | Later puzzle consequence (proposed, beyond current rotation) | Longer memory/ending |
| --- | --- | --- | --- |
| Tree | Sapling/shade appears where water reached | Roots protect an existing channel; route planning changes | Mira remembers habitat protected; shade changes crossing scene |
| Bridge | A crossing becomes usable | Water must pass beneath a fixed bridge/culvert | Orin recalls access restored; visitors can reach a meadow |
| Flowers | Meadow appears beside water | Protected flower patch constrains pipe path | Fen recalls a place for neighbors; butterflies appear with trees |
| Tree + bridge | Shaded crossing | Combine root and crossing constraints in later authored board | Shared place of shade and passage |
| Tree + flowers | Butterfly grove | Combine protected root/meadow cells | Wildlife visibly gathers |
| Bridge + flowers | Community walk | Path and fragile meadow constraints interact | Neighbors visit without trampling meadow |
| All three | Interlocking habitat/access/community | Late synthesis puzzle with clear visual rules | Balanced ending based on actual history |

**Source status:** first-column choices and existing visual combinations are implemented; the deeper future puzzle constraints and three-depth coverage are **design proposals**. Do not document proposed constraints as shipped.

## Memory model (EG-103) — bounded and inspectable

The existing event sequence remains the only source of truth. A proposed **derived memory token** is not a second independently mutable truth store:

```json
{
  "character": "Mira",
  "memoryId": "shade-first-stream",
  "trigger": {"choice": "ShadeTree", "chapterAtMost": 3},
  "sourceEventIndex": 0,
  "priority": 10,
  "localizedLineKey": "mira_shade_memory",
  "visibleConsequence": "shade-canopy"
}
```

This example is illustrative. At most **3–5 salient tokens per character** should be surfaced in a chapter, chosen deterministically from valid events. Never say an NPC remembers a conversation the game did not save; never fabricate elapsed-time events, relationships or emotional dependency. When a choice is reconsidered, all derived tokens and journal lines must update from the corrected event history.

### Character perspective, not morality meter (EG-104)

- **Mira — habitat:** notices roots, shade, wildlife and ecological continuity.
- **Orin — access:** notices footbridges, routes, repairs and practical passage.
- **Fen — community and beauty:** notices flowers, gatherings and shared spaces.

Each can value more than one thing; none labels the player "good" or "bad" for choosing another. Distinct voices are already partly authored, but **blind attribution and emotional effect remain untested**. A proposed exploratory bar is ≥70% correct attribution among engaged testers, with raw counts.

## Causal journal (EG-101)

Each saved chapter entry should answer:

1. **What I did:** "Planted a shade tree after stream 2."
2. **What changed:** show the exact scene element or short before/after image.
3. **Who noticed:** one truthful NPC line, not generated memory.
4. **What it changed next:** name the protected root/bridge/meadow rule in the next stream, if one actually exists.

The current journal now records real choices and immediate state-derived effects, with the **currently active next-puzzle rule** only when the saved variant proves it. Earlier entries (including migrated saves) do not invent historical mechanics. The full visual four-link causal journal is still proposed. A returning player should reconstruct a past decision without replaying the whole campaign (exploratory target ≥80%). The journal must not imply a puzzle effect that the current board logic does not implement.

## State machine and integrity

| State | Allowed action | Invariant |
| --- | --- | --- |
| New / unsolved board | Rotate valid non-anchored tile, hint, undo, restart | No world choice until water is fully connected |
| Solved board | Choose one ecology action; optionally undo solved board | No double reward or duplicate choice |
| Choice just made | Reconsider latest choice before proceeding | Reopen exact previous solved board and remove derived consequences |
| Next stream | Play branch conditioned on prior choice | Save/restore preserves board variant and protected landmark |
| Chapter 12 complete | Review journal, world and ending; optionally start another garden | Ending reflects ordered choices; no hidden random outcomes |

The current `WorldSnapshot` schema is **v4**, with `boardVariant`, `lastSolvedVariant`, `hintStep` and `hintTile`; v1/v2/v3 snapshots migrate with hint stage reset to idle. Any future token or undo-history change needs explicit migration tests. World state must not depend on selected locale, 2D/2.5D view, sound or network availability.

## Epilogue and postcard (EG-115/116)

- **Epilogue:** show the world the player made, three representative changes and why they occurred; avoid numeric morality score. Current authored endings exist; visual comparison is proposed.
- **Postcard:** optional locally generated still image and one truthful line. It must remain usable offline and must not auto-share or upload.
- **Replay:** only promise a meaningfully different garden after the authoring pipeline can deliver it. Different labels over the same puzzles are insufficient.

## 2D/2.5D representation

Both renderers must read the same `VillageScenePlan` and show the **same facts**. Fixed framing and before/after states matter more than polygon count. Keep 2D as baseline and fallback; 2.5D remains opt-in until measured. Any animation is decorative, never the only explanation.

## Acceptance tests

- Exhaustively test combinations and orderings of tree/bridge/flowers for consistent visual, dialogue, journal, puzzle and ending outcomes.
- Save/reload, language switch, renderer switch, undo and reconsideration produce no contradictory NPC statements.
- Every displayed memory has a traceable source event; every protected puzzle landmark has a matching world cause.
- Returning users can correctly answer "what changed and why?" without being prompted with the intended answer.
- Observe **preference for living-world versus matched puzzle-only play** before adding simulation depth. **Important confound:** current Puzzle Only uses original boards while Living World uses transformed/anchored boards after chapter one; match puzzle difficulty/topology or restrict the comparison to identical boards. Otherwise preference cannot cleanly be attributed to narrative consequences.

**Do not implement:** unlimited generative NPC chat, simulated attachment pressure, relationship meters, invented offline activity or cloud synchronization merely to make the world appear "alive."
