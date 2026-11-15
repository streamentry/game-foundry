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

## 2026-11-15 — Mobile export preflight

**Decision:** Keep Takuzu Lab SDK-free. Treat Godot 4.7.2 mobile distribution as unverified until a signed build runs on physical iOS and Android devices.

**Evidence:** Xcode 27.2 and iOS 27.2 SDKs are installed. The official Godot 4.7.2 binary was run from temporary storage; headless import and rules checks passed. Device discovery found 11 shutdown iOS simulators and no physical iPhone; `adb devices -l` found no Android device. The Android SDK link resolves to a directory with Platform Tools but no Platform or Build Tools.

**Static baseline:** The Godot 4.7.2 Android build config sets min SDK 24, compile/target SDK 36, Build Tools 36.1.0 and Java 17; the Android build detector pins NDK 29.0.14206865. API 36 meets Google Play's current new-app/update target requirement. See [mobile export and SDK assessment](../mobile-validation.md) for the exact template, plugin review, and commands.

**Opposing case:** Godot's matching export templates and Xcode are available for download/installation, so a small debug build may be straightforward once the missing environment and device access are restored. That does not prove signing, launch, touch handling, pause/resume, offline behavior or low-end performance.

**Unknown:** Successful export/install, the generated iOS deployment target, connected device identities, physical runtime behavior and any SDK-specific compatibility/privacy issues.

**Cheapest falsification:** Install the standard Godot 4.7.2 editor and matching templates, then export and launch the current sample on one physical iPhone and one Android device with local test signing.

**Next real action:** Restore access to both physical devices and the Android SDK build packages, then complete the device workflow in `docs/mobile-validation.md`. Do not add an SDK until a production game's data or monetization needs justify it.

**Local checks:** Candidate validation passed for 100 hypotheses and 15 public listing anchors, with 0 measured popularity rows. The 28 Python unit tests passed. Godot 4.7.2 headless import passed; all 8 Takuzu rules checks passed. The editor remains absent from `PATH`; the temporary official binary was used explicitly.

**Related P0 blockers:** Apple Ads token/account environment variables are absent here, so issue #2's authenticated collection was not run. `measurements.csv` has only its header; there are no measured terms to select for issue #3's organic-result review. No keyword or rank evidence was inferred.
