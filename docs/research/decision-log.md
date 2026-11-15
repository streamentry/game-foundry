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
