# Roadmap — evidence before inventory

## Current research priorities

- **Search-led:** [Kakuro with Shikaku as fallback](research/first-game-decision-2026-11-21.md), conditional on the existing demand, competitor and player gates.
- **Global visual exploration:** [Good Eye → Quiet Shot → One Path](research/global-45plus-top-three-games-2026-11-21.md). Start with the cheapest Good Eye comparison; test subsequent mechanics sequentially. These are proposed playtest hypotheses, not three production projects. Record real return and referral behavior before expanding.

## Phase 0: research and verification (7 days, time-boxed)
- Collect official Apple Ads popularity for target non-branded terms (US, GB, AU, DE, JP), when credentials are available.
- Audit the top 10 organic App Store results per keyword and recurring negative review themes.
- Mark low-volume or branded-only terms as `reject` or `park`, not as opportunities.
- Pick at most 3 technical/market candidates. Define clear user differentiation and organic distribution channel.

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
