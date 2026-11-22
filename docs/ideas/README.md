# 100 niche-game research hypotheses

The [100-row candidate backlog](candidates.csv) is a **discovery queue**, not a list of games to clone or ship. It was seeded on **2026-11-15**. All 100 are **unvalidated** for App Store search demand; 15 have a **real US App Store listing anchor**, which proves only the genre exists.

The separate [global 45+ playtest shortlist](../research/global-45plus-top-three-games-2026-11-21.md) proposes **Good Eye, Quiet Shot and One Path** as three distinct visual experiments. Their `GV-01`–`GV-03` identifiers belong to that note, not the `GF-` keyword catalog. Their ranking does not supply measured demand or change the 100-row research dataset.

The [ten-year global gaming thesis and seven detailed game briefs](ten-year-global-gaming-thesis-2026-11-22.md) records the founder's 2027–2036 strategic hypotheses, including personalization, distribution tradeoffs, proposed experiments, and stop gates. The seven named titles map to existing `GC-*` entries in the [50-concept speculative backlog](global-concepts-2027-2036.md), **not additional products or verified opportunities**.

The [four-genre 2026–2036 outlook](four-genre-outlook-2026-2036.md) adds **Simulation, Role-Playing, Strategy, and Puzzle** as speculative research territories, with twelve cross-genre `GX-001`–`GX-012` experiments. These do not duplicate or promote the existing `GC-*` and `GF-*` backlogs to production.

The [50 global franchise and genre benchmarks](global-franchise-benchmarks-2026-2036.md) are competitive references with transferable mechanics and explicit speculation cautions. Their `BR-*` IDs are **not original build ideas**, and hypothetical sequels are not confirmed releases.

The [AI-created social worlds and ambient companion thesis](ugc-ai-companion-2036-thesis.md) adds two **speculative** experiments, `GX-013` and `GX-014`, concerning constrained creator-built worlds and a privacy-aware virtual companion. Neither is a production commitment; this note distinguishes genre selection strategies and avoids conflating the companion with `GC-043 Echo` (the existing temporal puzzle).

## Data dictionary

| Field | Meaning |
| --- | --- |
| `id` | Stable candidate identifier. Preserve it even when priorities change. |
| `family` | `logic`, `word`, `spatial`, `education`, or `audience`. |
| `primary_search_keyword` | Literal research query hypothesis. Not confirmed App Store demand. |
| `mechanic` | Type of gameplay to prototype, not proprietary content. |
| `differentiation_hypothesis` | A concrete feature worth testing, not an established product gap. |
| `engineering_effort_estimate` | Qualitative rough cut, not a schedule or cost. |
| `research_queue` | `screen_first` or `explore`: ordering for research only. |
| `listing_evidence_level` | Whether an example competitor listing was observed publicly. |
| `example_competitor`/`url` | One example, **never** evidence that top-10 competition is weak. |
| `observed_on` | Date the public listing was reviewed. |
| `apple_popularity_1to100` | **Blank** until measured from the official Ads API. |
| `popularity_country`/`month`/`source_url` | Required provenance when popularity is filled in. |
| `research_status` | `unvalidated`, `measured`, `interviewing`, `prototype`, `reject`, `park`, `ship`. |

**Do not fill empty fields with 0**. A blank means not researched. Apple's 1–100 is an index, not monthly searches.

Run `python3 tools/validate_ideas.py docs/ideas/candidates.csv` after editing.

## Ten additional independent experimental implementations (2026-11-22)

[Renzoku Meadow](../../games/renzoku-meadow/) (GF-025) · [Word Ladder Lane](../../games/word-ladder-lane/) (GF-031) · [Word Square Studio](../../games/word-square-studio/) (GF-038) · [Vowel Garden](../../games/vowel-garden/) (GF-043) · [Word Search Shores](../../games/word-search-shores/) (GF-048) · [Nonogram Canvas](../../games/nonogram-canvas/) (GF-056) · [Akari Evening](../../games/akari-evening/) (GF-062) · [Pipe Garden](../../games/pipe-garden/) (GF-064) · [Sliding Tiles](../../games/sliding-tiles/) (GF-068) · [Number Pyramid](../../games/number-pyramid/) (GF-079). The word-ladder, missing-vowels and sliding games do not pretend their answers are uniquely determined over all language or movement possibilities. All 100 keyword rows remain **hypotheses**: actual App Store popularity, adults-45+ relative preference, retention, willingness to pay and acquisition remain unknown. [Read the decision and stop rules](../research/game-experiments-23-to-32-2026-11-22.md).

## Independent game implementations, 18–22 (2026-11-22)

[Numberlink Paths](../../games/numberlink-paths/) (GF-014) · [Kropki Pairs](../../games/kropki-pairs/) (GF-020) · [Ripple Rooms](../../games/ripple-rooms/) (GF-022) · [Battleship Harbor](../../games/battleship-harbor/) (GF-026) · [Kuromasu Windows](../../games/kuromasu-windows/) (GF-028) are playable engineering experiments from already listed hypotheses. All five remain **unvalidated** for age 45+, official Apple keyword popularity, competitor preference, voluntary return or paying demand. The Numberlink and Kropki projects explicitly implement **variants**, not the full range of classical rules. [Research brief and stop rule](../research/game-experiments-18-to-22-2026-11-22.md).

## Five further implemented prototypes, without demand validation (2026-11-22)

[Nurikabe Bay](../../games/nurikabe-bay/) (GF-010) · [Hidato Walk](../../games/hidato-walk/) (GF-013) · [Skyline Steps](../../games/skyline-steps/) (GF-019) · [Thermometer Studio](../../games/thermometer-studio/) (GF-021) · [Cage Hearth](../../games/cage-hearth/) (GF-023) implement five previously listed **unvalidated** search-intent hypotheses. They do not add candidates, measured popularity, paying users or permission for multiple near-identical App Store submissions. [Read their comparative research plan](../research/game-experiments-13-to-17-2026-11-22.md).

## Five additional game implementations (2026-11-22)

[Futoshiki Terrace](../../games/futoshiki-terrace/) (GF-004), [Star Atlas](../../games/star-atlas/) (GF-005), [Bridge Isles](../../games/bridge-isles/) (GF-007), [Loop Lantern](../../games/loop-lantern/) (GF-008), and [Hitori Grove](../../games/hitori-grove/) (GF-009) are standalone **engineering prototypes only**. Their [research decision brief](../research/next-five-game-experiments-2026-11-22.md) sets comparison and stopping criteria. This does not alter the 100 hypotheses or imply keyword popularity, player demand, or Apple App Store production approval.

## Next two standalone logic prototypes (2026-11-22)

The user-directed [Kakuro Compass](../../games/kakuro-compass/) (GF-001) and [Quiet Camp](../../games/quiet-camp/) (GF-006) implement existing **unvalidated** search-led hypotheses as playable, independently verified games. Read the [decision brief](../research/next-game-experiments-2026-11-22.md); the 100-row research dataset remains unchanged. Building games does not create primary search-popularity evidence, differential preference or approval for store release.

## Additional visual research

[Odd Bloom](../research/next-two-game-experiments-2026-11-22.md) is an **unmeasured game-loop experiment** added at user request. It does not correspond to a validated Apple keyword or change the 100-row research dataset. [Shikaku Garden](../../games/shikaku-garden/) is an implementation of the already-listed **GF-018** hypothesis, whose acquisition demand remains unverified.

## Initial high-information research subjects

These are cheap to prototype **if demand is confirmed**, but face nontrivial incumbents:

- **Kakuro:** a numerical logic game with practical reusable constraints; known apps offer thousands of free levels.
- **Tents and Trees:** tractable validation and pleasant theme; established apps already ship daily levels and offline play.
- **Hashi:** distinctive touch mechanics; professional no-ads competitors exist, so quality alone isn't sufficient.
- **Logic Grid Mysteries:** more differentiation through original content but higher writing and QA costs.
- **Masyu / Shikaku:** elegant mechanics and small new listings, but demand remains unknown.
- **Large-print/adaptive variants:** may address accessibility needs, but do **not** rely on unsubstantiated cognition or dementia-treatment claims.

See [the latest catalog scan](../research/catalog-snapshot-2026-11-15.md), [the initial market scan](../research/market-scan-2026-11-15.md), [research method](../research/README.md), and [decision log](../research/decision-log.md).

## Why 100 candidates, not 100 apps?

Keyword overlap is deliberate research design: `takuzu` vs `binary puzzle`, `word ladder` vs `daily word ladder`, or `jigsaw offline` vs `large piece jigsaw` can be **one product addressing multiple search intents**. Do not turn alternate keywords or audience segments into near-identical App Store submissions.
