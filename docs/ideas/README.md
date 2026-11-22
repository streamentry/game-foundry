# 100 niche-game research hypotheses

The [100-row candidate backlog](candidates.csv) is a **discovery queue**, not a list of games to clone or ship. It was seeded on **2026-11-15**. All 100 are **unvalidated** for App Store search demand; 15 have a **real US App Store listing anchor**, which proves only the genre exists.

The separate [global 45+ playtest shortlist](../research/global-45plus-top-three-games-2026-11-21.md) proposes **Good Eye, Quiet Shot and One Path** as three distinct visual experiments. Their `GV-01`–`GV-03` identifiers belong to that note, not the `GF-` keyword catalog. Their ranking does not supply measured demand or change the 100-row research dataset.

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
