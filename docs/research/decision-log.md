# Research and shipping decisions

## 2026-11-15 — Engine and approach

**Decision:** Godot 4.7.2 + GDScript, separate exportable game projects. First feature is a 4×4 Takuzu **engineering** sample, not a market launch.

**What we know:** Godot 4.7.2 is listed as stable in the official release archive; public App Store listings demonstrate existence of multiple niche puzzle categories; Apple Ads documents a relative search-term popularity API.

**Unknown:** actual search popularity for candidate keywords, top-10 organic competition, review gaps and commercial viability.

**Why:** open-source independence, portable reproducible logic tests, 2D suitability. Deployment/privacy SDK maturity on native iOS/Android still needs verification.

**Opposing case:** Flutter + Flame can be easier for word/UI games, while commercial engines may have more ready-made mobile monetization. We will not add multiple engines without a tested economic benefit.

**Stop condition:** if native export, accessibility or sustainable distribution proves impractical, reconsider engine/segment before scaling.

**Next observable action:** authenticate Apple Ads insights (if accessible), collect country-specific monthly data for `screen_first` queries and audit top-10 storefront results. Do not build a game backlog without measurement.

## Template

```text
Date / candidate:
Decision: research | prototype | ship | park | reject
Player & search intent:
Evidence: primary links, country, date, confidence
Strongest alternative explanation:
Assumptions:
Cheapest falsification:
Action within seven days:
Continue / stop thresholds:
Actual outcome & model update:
```

## 2026-11-15 — Make search discovery source-first

**Decision:** Add genre-first, pattern-first and exact-term research modes to Apple's authenticated insights collector. Preserve per-country provenance, full pagination vs truncated top-K coverage, and treat missing niche keywords as **unknown**, not unpopular.

**Why:** The prior 100-row catalog was selected by mechanic preference. Apple data should determine which search intents are real. Catalog API result order is not an organic ranking and rating counts are not demand estimates.

**Strongest counterargument:** Apple's search term popularity covers only popular terms, so its results can omit profitable small niches and branded terms may dominate. This is a *candidate discovery source*, not a definitive market sizing instrument.

**Stop condition:** If the Apple Ads source cannot produce enough specialist keywords, sample public storefront search suggestions/ASO sources and test organic acquisition experimentally; do not infer absent demand as zero.

**Next real action:** Obtain a valid Apple Ads API token via an authorized account, run a read-only monthly genre discovery, then audit the top observed non-branded queries.

## 2026-11-15 — Storefront audit evidence pack

**Decision:** Add a structured audit template and a fail-closed validator for issue #3. Do not add result or review observations until they are captured from the target storefront on a physical device.

**What changed:** The validator requires ten distinct unsponsored result positions, the physical device/storefront and capture timestamp, listing metadata and local screenshots, a mixed-sentiment review sample or a stated sampling limit, and a wedge/no-wedge finding with a falsifier.

**Evidence status:** The measurement worksheet remains header-only with 0 measured popularity rows. No organic ranks, review themes, or wedge claims were created. The tool checks structure and file paths; a human still needs to compare entries with source screenshots and App Store pages.

**Opposing case:** A detailed pack adds capture effort and cannot prevent a person from entering false ranks. Its value is the audit trail and missing-field gate, not automated proof.

**Cheapest falsification:** Complete one audit pack from a real device for a measured non-branded query; check whether the template captures the evidence needed to decide continue or no-wedge without rework.

**Next real action:** First obtain a measured keyword from the authorized Apple Ads collection in issue #2, then capture its ten organic results and reviews on a controlled physical device. Until then, keep issue #3's opportunity score unranked.

## 2026-11-17 — Read-only demand scan; no game selected

**Decision:** Keep the first release unselected. Apple Ads now supplies measured search signals, but the organic competition and unmet need gates are still open.

**Evidence:** A read-only API user returned 2,500 genre observations for October 2026 across US, GB, AU, DE and JP; all five country queries were marked truncated to top-K. Exact searches for the 100 brainstormed candidates returned no rows, which remains **unknown**, not zero. Contains searches returned 113 observations for 40 queries; six localized Japanese patterns returned 12 more. The ten measured query/storefront pairs to audit are recorded in [the capture queue](storefront-capture-queue-2026-11-17.csv), with source snapshot hashes and no scores.

**Competition signal:** A separate public catalog scan found large, recently updated listings in Sudoku, Mahjong, color-by-number, word search, block puzzle, jigsaw and nonogram. The catalog API is not official organic rank, and ratings are not downloads or revenue. The findings justify a competition check, not a conclusion that a category is open or closed.

**Why no game is chosen:** No physical-device organic result set or balanced recent-review sample has been captured. Without those, we cannot support a product wedge, acquisition path or Apple 4.3 distinctness decision. `Takuzu Lab` remains an engineering reference.

**Engineering correction:** The unscored opportunity exporter now keeps distinct Unicode query terms such as `fussball` and `fußball` separate. The full research suite passes 42 tests.

**Next real action:** Capture the ten queued keyword/storefront pairs on controlled physical devices, record ten unsponsored results and a mixed review sample for each, then decide `wedge` or `no wedge`. Do not start a game-specific production project before that review.
