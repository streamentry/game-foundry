# Portfolio-ranking correction and decision protocol (2026-11-22)

> **Status: methodological correction; comparison NOT yet executed.** This note records the limitation in a previously proposed “top five” global game ranking and defines how Game Foundry should evaluate the complete idea set. No new game is approved for production, and no fabricated cross-portfolio scores are supplied.

## Decision record: earlier ranking was narrower than claimed

A previous subjective “top five” named:

1. **Nexusforge** — AI creation / social UGC platform (`PH-002`)
2. **Evergrove** — cozy living-world mobile simulation (`PH-006`)
3. **Echoes of Aether** — reactive open-world RPG (`PH-001`)
4. **Solstice Collective** — persistent cooperative survival (`PH-003`)
5. **Rune Chain** — evolving-card strategy (`PH-034`)

**Correction:** these names were selected mainly from the **most recently discussed 50 projected hits** (`PH-*`), informed by general themes from earlier messages. **They were not selected after deduplicating and systematically scoring the entire Game Foundry corpus.** Consequently, that list should be labeled *illustrative PH-only shortlist*, **not “the top five from everything we discussed.”** Its order and confidence are not supported by a cross-portfolio comparison.

The core analytical error was **recency/selection bias**: exciting living-world proposals were overrepresented while simpler, lower-cost and more readily discoverable game concepts were underweighted. This also mixed **largest imaginable global outcome** with **best attainable small-studio investment**.

## Corpus that an actual comparison must cover

| Portfolio / source | What it represents | Example candidates | How to use it |
| --- | --- | --- | --- |
| [`GF-001`–`GF-100` keyword hypotheses](../ideas/candidates.csv) | Search intents and differentiations, not necessarily 100 distinct products | Kakuro, specialized logic, adult-friendly games | Group equivalent keywords into *one product candidate*; do not count alternates twice |
| [`GC-001`–`GC-050` original concepts](../ideas/global-concepts-2027-2036.md) | Speculative original mechanics | Unfold, Still, One Move, Hidden Rule, Together, Little Worlds | Compare mechanic and audience value rather than imagined fame |
| [`GX-001`–`GX-012` genre experiments](../ideas/four-genre-outlook-2026-2036.md) and [`GX-013`–`GX-014`](../ideas/ugc-ai-companion-2036-thesis.md) | Twelve genre hybrids plus creator-world and ambient-companion hypotheses | Three Cards, Neighbor Memory, Infinite Worlds, Ambient AI Companion | Merge overlaps with GC/PH rather than making redundant apps |
| [`PH-001`–`PH-050` projected hits](../ideas/projected-global-hits-2026-2036.md) | Founder-ranked original-IP *forecast*, not verified games | Nexusforge, Evergrove, Echoes of Aether, Rune Chain | Keep forecast rank distinct from evidence-based priority |
| [`BR-001`–`BR-050` franchise references](../ideas/global-franchise-benchmarks-2026-2036.md) | Competitive benchmarks and speculative sequels | Roblox, GTA, Balatro | **Not independent build candidates**; identify incumbents and transferable mechanisms |
| [`FP-001`–`FP-050` 2034 forecast references](../ideas/2034-top-50-speculative-forecast.md) | Future franchise variants plus speculative mechanics | AI companion, spatial chess, creator worlds | **Not 50 additional unique products**; map to candidates/benchmarks |
| [`LW-01`–`LW-04` experimental probes](../ideas/living-worlds-social-systems-2026-2036.md) | Testable mechanisms, not new games | NPC memory village, hybrid micro-loop | Treat as validation methods attached to candidate ideas |
| [Existing prototypes and current gates](../roadmap.md) | Actual code/assets and near-term studio priorities | Kakuro Compass, Good Eye, Shikaku Garden | Record technical maturity separately from product-market evidence |

**Do not add the rows across these sources and report the sum as unique opportunities.** There is substantial synonymy, shared mechanics and overlap. Maintain stable original IDs; make a deduplicated **canonical concept** map with aliases and sources.

### Examples of underweighted alternatives

- **Wordless mass-market puzzles:** Unfold, Still, One Move, Hidden Rule.
- **Cozy persistent progression:** Little Worlds, Evergrove and small living-world prototypes.
- **Social, asynchronous experiences:** Together, Secret Signal, Solstice Collective.
- **Compact systematic strategy:** Three Cards, Rune Chain, Chronicle Engine.
- **Search-led first products:** Kakuro Zen / existing Kakuro Compass, Mahjong and specialized logic variants.

These are *coverage checks*, not a replacement ranking. The studio currently defaults to adults 45+; claims about other age groups must be separately validated.

## Two independent ranking questions — never blend their answers

### A. What could become one of the largest global games or gaming platforms by 2036?

Assess **global reach, cultural influence and commercial scale**, but separate:
- **Conditional upside if successful** (market/platform ceiling), from
- **Plausibility of reaching that outcome** (incumbency, network effects, capital, execution, distribution).

An enormous winner-take-most platform can rank high on *ceiling* and low on *realistic likelihood*. State which one is being ranked.

### B. Which five concepts should this indie studio actually pursue?

Assess **achievable differentiated value and reliable learning per dollar**, including founder-led distribution, time to prove/kill, modest live-ops requirements, and preservation of optionality. The optimal studio candidate need not be a plausible global top-five title.

**Explicitly include “build none yet / validate current prototype” as a candidate action.** Do not turn backlog size into a production mandate.

## Six dimensions for both comparisons

| Dimension | Key question | Relevant direct evidence | Failure mode to challenge |
| --- | --- | --- | --- |
| 1. Global audience & market | Who already wants this experience, how often and in which regions? | Market/store data with dates, genre or query comparisons, identified cohorts | Confusing huge category with accessible market share |
| 2. Core mechanic strength & originality | Is it learnable, satisfying, sufficiently distinct and repeatable? | Live first-minute comprehension; playtest against polished alternatives | Novelty without lasting play; cloned mechanics |
| 3. Retention & social/viral potential | Do users return voluntarily, share or invite others? | D1/D7 with denominators, repeated voluntary play, actual invitations used | Projected retention from “emotional attachment” stories |
| 4. Distribution advantage | How will this game be discovered *without* incumbent budgets? | Keyword demand and SERP, referrer/cohort data, communities, click-through | Platform cold start; content generation without distribution |
| 5. Engineering + operating economics | What does a useful version cost to build, host, moderate and maintain? | 7-day vertical slice, priced infra, QA burden, team fit, LLM per-session cost | Underestimating backend, UGC moderation, art/content and live-ops |
| 6. Defensibility | What compounds beyond a polished launch? | Brand/identity, owned discovery, creator graph, habitual use, content QA, switching cost | “AI-powered” mistaken for an enduring moat |

**Evidence taxonomy:** Observed fact / sourced external estimate / measured internal test / plausible inference / unverified hypothesis / unknown. Missing evidence is **unknown**, never a zero, an invented score or assumed green light. Explicitly timestamp external research.

## Reproducible evaluation procedure

1. **Inventory & normalize:** Include every corpus above, unify mechanic synonyms, join duplicates to a canonical concept ID, preserve provenance and raw names. Distinguish benchmark/IP owner names from potentially ownable concepts.
2. **Screen eligibility:** Exclude unauthorized third-party IP clones, speculative sequels presented as actual products, and concepts whose build relies on unavailable rights or implausible capital. Such entries may remain as competitive references.
3. **Evidence matrix:** For each canonical candidate, record target player, why they would switch, comparable incumbent, distribution plan, first prototype cost, operating cost, repeat-use hypothesis, and what is still unknown.
4. **Create *two separate* shortlists:** Global upside/probability and indie achievable opportunity. Do not apply a single universal weight vector. Make the strongest counterargument to each finalist.
5. **Rank with explicit assumptions, not false precision:** Use qualitative ratings first (high/medium/low/unknown), evidence confidence, a sensitivity check for capital/distribution, and only optionally quantified weights **after** enough candidate-specific evidence exists.
6. **Run inexpensive head-to-head tests:** At minimum compare a tactile puzzle (e.g., Unfold/Still), an accessible strategic loop (e.g., Three Cards/Rune Chain), and one tiny persistent simulation, against relevant commercial alternatives. Reuse [existing prototypes](../roadmap.md) where possible.
7. **Publish an evidence-backed decision note:** Top five for each objective, inclusion/exclusion logic, strongest counterexample, key missing data, a cheapest falsification experiment, 7-day action, and explicit advance/stop criteria. A recommendation is *conditional* until it has observed player and acquisition evidence.

### Minimum quality bar before publishing any “top five from all ideas”

- All the above source families were inspected and normalized; no recency-only shortlist.
- Overlapping concepts are linked to one canonical mechanic candidate.
- Two rankings are independent, with clear objective functions and plausible alternatives.
- Missing data, real direct player evidence and assumed outcomes are visibly differentiated.
- At least one non-AI evergreen puzzle, one indie strategy, and one AI/social world receive serious counterfactual evaluation.
- The decision note documents assumptions that could reverse the outcome.
- Nothing is labeled `validated` or `approved for production` without passing the repo's separate demand, comparative preference and commercial gates.

## What we know today / what we do not

**Known:** the repository contains multiple kinds of candidate and benchmark lists; it has a 45+ / ASO-led research direction and many prototype experiments. The earlier five-name list came from a narrower subset, not a complete cross-backlog assessment.

**Unknown:** category-specific accessible demand for the newly imagined concepts; actual acquisition performance; whether personalization, emergent AI and deep social systems improve long-term voluntary retention enough to justify their cost; final deduplicated count; credible likelihood of any fictional title becoming a top global game.

**Current action:** preserve this audit as a warning against confidently extrapolating from the latest idea batch. **No new five-winner verdict is issued in this document.**

## Decision principle

> Theoretical platform scale is not the same as a good indie investment. Compare all concepts, separate upside from likelihood, and let observed player behavior and viable distribution—not the elegance of a speculative story—decide what to build.
