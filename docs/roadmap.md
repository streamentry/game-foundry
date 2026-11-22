# Roadmap — evidence before inventory

## Current research priorities

- **Search-led:** [Kakuro with Shikaku as fallback](research/first-game-decision-2026-11-21.md), conditional on the existing demand, competitor and player gates.
- **Global visual exploration:** [Good Eye → Quiet Shot → One Path](research/global-45plus-top-three-games-2026-11-21.md). Start with the cheapest Good Eye comparison; test subsequent mechanics sequentially. These are proposed playtest hypotheses, not three production projects. Record real return and referral behavior before expanding.

### Proposed three-track portfolio follow-up (2026-11-22; **not** a production override)

The [staged next-three validation memo](research/next-three-games-validation-decision-2026-11-22.md) proposes: **(1)** test the already built Kakuro Compass against Conceptis and exact App Store query demand; **(2)** only after recording that evidence, prototype a single Rune Chain / Three Cards micro-roguelike; **(3)** conditionally A/B a five-puzzle Little Worlds world-persistence layer against puzzle-only play. These are **sequential learning experiments**, not a new three-game build authorization. Maintain the existing Good Eye → Quiet Shot → One Path global visual research queue and the Kakuro/Shikaku ASO gate; no title is promoted to `ship` by this memo.

## Selected long-horizon game direction: Evergrove (research build, not launch approval)

At the founder's explicit direction on **2026-11-22**, **Evergrove (`PH-006`)** is the single chosen decade-long **creative bet**. This is a direction for learning, **not evidence that it will become a global hit or that the prior Kakuro and Good Eye gates have passed**.

[Evergrove](../games/evergrove/README.md) is now an isolated Unity/C# offline-first **living-world vertical slice**. It reuses our own six Pipe Garden boards to compare the identical puzzle **with vs without** a persistent village layer. After connecting a stream, players choose a tree, crossing or wildflowers; three characters remember each choice and multiple choices lead to combined consequences. A versioned, validated local C# snapshot preserves the sequence via Unity PlayerPrefs; future production storage needs stronger durability. No generative AI, account, online world or tracking SDK. An **optional** standalone TypeScript Cloudflare Durable Objects reference server is present but not connected to the core game or deployed.

- **Next:** verify C# domain CI and test in the Unity Editor, real-device readability and an actual 20–30 person comparative observation with manually balanced conditions; log D1/D7 return and what players can explain about causal changes.
- **Continue:** players favor the living layer on observable return or preference, understand effects and a viable discovery path emerges.
- **Stop/simplify:** puzzle-only wins, players don't understand consequences, or maintaining a living world makes economics or trust worse.
- **Source of decision:** [research decision record](research/evergrove-build-decision-2026-11-22.md). Any store launch still needs a separate evidence-backed review.

## Evergrove Chapter One — quality milestone (not a store launch)

**Engine ADR:** [Evergrove uses Unity/C# provisionally](decisions/2026-11-22-evergrove-engine-unity-csharp.md). Real Unity Editor, Android/iOS and player validation must confirm the decision; the remaining Godot portfolio is unchanged.

[Evergrove](../games/evergrove/README.md) now has a **12-chapter** Unity/C# offline-first gameplay slice, progressing from 3×3 to 4×4 water-connection puzzles, one-tile optional hints, an original illustrated village that changes with player decisions, authored deterministic memory combinations, journal, branching endings and backup-recoverable local saves. This is a founder-selected long-horizon direction (`PH-006`) with no validated retention or distribution. The optional Cloudflare reference service is not attached.

**Acceptance gates:** .NET deterministic tests and review; Unity Editor compile and scene smoke; iOS/Android real-device readability; direct-player comparative preference and voluntary D1/D7 return; then a separate greenlight for any online functionality. A beautiful codebase is not a substitute for real play.

## Phase 0: research and verification (7 days, time-boxed)
- Collect official Apple Ads popularity for target non-branded terms (US, GB, AU, DE, JP), when credentials are available.
- Audit the top 10 organic App Store results per keyword and recurring negative review themes.
- Mark low-volume or branded-only terms as `reject` or `park`, not as opportunities.
- Pick at most 3 technical/market candidates. Define clear user differentiation and organic distribution channel.

## Ten original game prototypes 23–32 — still experimental

At the user's request, implement [Renzoku Meadow](../games/renzoku-meadow/), [Word Ladder Lane](../games/word-ladder-lane/), [Word Square Studio](../games/word-square-studio/), [Vowel Garden](../games/vowel-garden/), [Word Search Shores](../games/word-search-shores/), [Nonogram Canvas](../games/nonogram-canvas/), [Akari Evening](../games/akari-evening/), [Pipe Garden](../games/pipe-garden/), [Sliding Tiles](../games/sliding-tiles/), and [Number Pyramid](../games/number-pyramid/) as small distinct standalone experiments. Unique-solution claims apply **only** where mathematically meaningful; curated word answers and reachable sliding boards are qualified. They are not business opportunities or production approvals. Follow the [experiment decision and stop tests](research/game-experiments-23-to-32-2026-11-22.md) before larger levels or publishing.

## Five further independently playable puzzle experiments — games 18–22

[Numberlink Paths](../games/numberlink-paths/), [Kropki Pairs](../games/kropki-pairs/), [Ripple Rooms](../games/ripple-rooms/), [Battleship Harbor](../games/battleship-harbor/), and [Kuromasu Windows](../games/kuromasu-windows/) implement the existing GF-014, GF-020, GF-022, GF-026 and GF-028 hypotheses as user-requested, isolated Godot research prototypes, **not new store-launch approvals**. They test five distinct deduction mechanisms and six original uniquely-solvable fixtures per game. Age-specific fit, differentiating value, official search popularity and real retention remain unknown. See the [latest comparison and stop gates](research/game-experiments-18-to-22-2026-11-22.md).

## Five additional distinct game experiments — prototypes 13–17

The user requested five more independent playable games. [Nurikabe Bay](../games/nurikabe-bay/), [Hidato Walk](../games/hidato-walk/), [Skyline Steps](../games/skyline-steps/), [Thermometer Studio](../games/thermometer-studio/), and [Cage Hearth](../games/cage-hearth/) are implementation-only tests of GF-010, GF-013, GF-019, GF-021 and GF-023 respectively, with no production approval. Their exact-fit 4×4 solver tests prove mathematical correctness, not user appeal. The already recognized lack of distribution and 45+ user evidence is now a larger risk than inability to implement more puzzles. [Read the decision and stopping condition](research/game-experiments-13-to-17-2026-11-22.md).

## Five additional standalone puzzle experiments — prototypes 8–12

At user direction, build [Futoshiki Terrace](../games/futoshiki-terrace/), [Star Atlas](../games/star-atlas/), [Bridge Isles](../games/bridge-isles/), [Loop Lantern](../games/loop-lantern/), and [Hitori Grove](../games/hitori-grove/) as five independent Godot proofs of play. These implement existing GF-004, GF-005, GF-007, GF-008 and GF-009 hypotheses **without promoting them to production**. Each project has small original, solver-verified boards, its own design, local save/recovery and downloadable browser artifact. No age-specific adoption or ASO search volume is measured. See the [experiment decision](research/next-five-game-experiments-2026-11-22.md); recruit target players and compare to incumbents before adding more titles or content.

## Seventh game: Quiet Camp (distinct visual deduction, still gated)

[Quiet Camp](../games/quiet-camp/README.md) implements the GF-006 Tents & Trees hypothesis as an original **proof of gameplay**, not an App Store product selection. Eight uniquely-solvable boards test whether adults 45+ enjoy no-timer deduction by row/column tents counts, orthogonal tree pairing and non-touching placements. Unlike Kakuro Compass, it tests spatial constraints without numerical entry or clue calculations. Real-device readability, competitor preference, retention, search demand and economics remain unmeasured. Test before adding content or publishing.

## Sixth game: Kakuro Compass (primary search-led proof, still gated)

[Kakuro Compass](../games/kakuro-compass/README.md) is an original **proof-of-play** implementation of the already documented conditional [GF-001 Kakuro hypothesis](research/first-game-decision-2026-11-21.md). Its key experiment is whether a *persistent across/down candidate inspector* improves first-board comprehension and completion compared with mature apps already claiming hints and beginner tutorials. The eight boards are verified for unique solutions. Device usability, search availability, head-to-head preference, return and commercial viability remain unknown. Avoid content-factory automation until real feedback is recorded.

## Fifth visual research prototype (not a distribution winner)

[Odd Bloom](../games/odd-bloom/README.md) tests immediate visual discrimination and one-tap repeat play in twelve no-timer rounds. Unlike rule-heavy Shikaku Garden, it has almost no instruction overhead, but the direct competitor category is crowded and novelty can fade quickly. The user's directive to build more games changes the *implementation queue*, **not** the evidence needed to enter production. Do not scale a content factory before testing voluntary replay and return.

## Additional playtest prototypes (not production approvals)

At user request, build [Shikaku Garden](../games/shikaku-garden/README.md) as a small, **gated** logic prototype. GF-018 search demand and comparative user preference remain unknown. Record real-world outcomes, especially whether a 45+ player chooses this game over a mature Shikaku alternative. Do not conflate the ability to ship a solver with organic acquisition.

## Three separate global visual prototypes (not three production approvals)

[Good Eye](../games/good-eye/README.md), [Quiet Shot](../games/quiet-shot/README.md), and [One Path](../games/one-path/README.md) are independently runnable Godot projects with original small test sets. They test visual estimation, predicted reflections, and discrete Euler-trail planning respectively. They were built at the user's direction; no market, retention or competitor-preference gate has been passed. Do not build further catalog games merely because the engineering scaffolds work. Recruit target adults, compare direct alternatives and log actual behavior before choosing whether any deserves production.

## First playable global visual experiment

[Good Eye](../games/good-eye/README.md) is the first original, stand-alone game **prototype**, implementing the [ranked global playtest brief](research/global-45plus-top-three-games-2026-11-21.md). It tests geometry-based judgment with local practice and daily play. Building the prototype does not approve production or replace the conditional Kakuro search-led track. Run the comparative five-player screen, then the independently recruited 30-person observation only if warranted; log actual results before expanding features.

## Phase 1: vertical-slice proof
- Reference Takuzu Lab demonstrates Godot scenes, pure rules and CI. **Not a market selection.**
- Ship one original prototype on real iOS and Android test devices.
- Validate level uniqueness and accessibility; instrument meaningful voluntary return.

## Phase 2: commercial test
- Release one differentiated game, ideally with a tested organic search position.
- Run a launch review after 2 and 4 weeks. Distinguish indexation from discovery, installs from retention, and reviews from downloads.
- Stop or pivot if acquisition is absent after listing-quality tests; don't build 20 siblings as a workaround.

## Phase 3: conditional scale
- Extract shared puzzle constraints/generators and release tooling only after repeated use.
- Distinct game design systems, metadata, content and audience need to survive Apple 4.3 review.
- Only scale topics that demonstrate product and distribution signals; pause poorly performing projects.

## Never promised by this scaffold
- iOS/Android signed builds.
- Working AdMob, purchases or Firebase SDKs.
- Verified search volume, organic rankings, download forecasts or profitable niches.
- Automatically generated unique puzzles for production.
