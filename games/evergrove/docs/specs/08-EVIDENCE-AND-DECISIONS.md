# 08 — Evidence, source traceability and decision discipline

**Date:** 2026-11-22. This directory was created from the supplied *Evergrove: evidence-backed product and feature strategy for a best-in-class indie cozy puzzle game* report, then cross-checked against the actual Game Foundry repository. **No tests, Unity builds, player interviews, store experiments or legal clearance were performed by this documentation change.**

## Evidence labels used in the backlog

| Key | Source and what it supports | Evidence strength / boundary |
| --- | --- | --- |
| **R1** | [Evergrove README](../../README.md), [design](../../DESIGN.md), [core puzzle rules](../../Assets/Scripts/Core/PipeRules.cs), [reactive boards](../../Assets/Scripts/Core/ReactiveWatercourses.cs), [world state](../../Assets/Scripts/Core/WorldModel.cs), [domain tests](../../Tests/Program.cs) | **Repository-verified implementation intent/code**; does not establish Unity/device success or user preference |
| **R2** | [Build/QA](../../BUILD_QA.md), [WorldStore](../../Assets/Scripts/Unity/WorldStore.cs), [local opt-in study](../../Assets/Scripts/Core/LocalPlaytest.cs) | **Repository-verified** scaffolding and limitations; actual crash/privacy/device outcomes unknown |
| **R3** | [Localization inventory](../../LOCALIZATION.md), [LocaleRuntime](../../Assets/Scripts/Unity/LocaleRuntime.cs), [LocalizedVillage](../../Assets/Scripts/Core/LocalizedVillage.cs) | **Repository-verified** 30 draft catalogs and known rendering blockers; no native certification |
| **R4** | [Visual comparison protocol](../../../../docs/research/evergrove-visual-comparison-protocol-2026-11-22.md), [2.5D ADR](../../../../docs/decisions/2026-11-22-evergrove-visual-direction-2p5d.md) | **Existing hypothesis/experiment design**; not an observed preference |
| **P1** | [Apple Human Interface Guidelines: Accessibility](https://developer.apple.com/design/human-interface-guidelines/accessibility), [Android accessibility](https://developer.android.com/guide/topics/ui/accessibility), [Material accessibility](https://m3.material.io/foundations/accessible-design/accessibility-basics) | **Official platform design guidance**; individual app/device compliance must be tested |
| **P2** | [Apple App Store product page](https://developer.apple.com/app-store/product-page/), [Apple App Store search](https://developer.apple.com/app-store/search/), [Apple App Privacy](https://developer.apple.com/app-store/app-privacy-details/), [Google Play Data Safety](https://support.google.com/googleplay/android-developer/answer/10787469) | **Official policy/marketing guidance**; recheck current field limits and disclosure rules at submission |
| **U1** | [Unity Advanced Text Generator](https://docs.unity.com/en-us/engine/6000.0/manual/uitoolkits/uielements/uie-work-with-text/uie-advanced-text-generator), [Unity language direction](https://docs.unity.com/en-us/engine/6000.3/manual/uitoolkits/uielements/uie-work-with-text/language-direction) | **Official engine capability docs**; integration, licensed fonts and native-device results still required |
| **Q1** | [JMIR Serious Games 2021, mobile game design for adults 50–85](https://games.jmir.org/2021/2/e24449), [systematic review of age-friendly mobile apps](https://link.springer.com/article/10.1007/s40520-025-03157-7) | **Empirical qualitative and review evidence** for larger targets, simpler interaction, clarity; not an Evergrove efficacy trial |
| **Q2** | Supplied Deep Research synthesis of player feedback for **Monument Valley 2** and puzzle-difficulty literature | **Anecdotal qualitative competitor signal**: beauty/shortness/low difficulty/mechanics not recombined; verify review sample before quantifying |
| **Q3** | Supplied Deep Research synthesis of **Cozy Grove** player feedback | **Anecdotal qualitative competitor signal**: repetitive tasks and real-time gating can frustrate; not a controlled causal study |
| **H1** | Product/engineering judgment and proposed Evergrove experiments in these specs | **Hypothesis**; thresholds, feature impact and scoring are not measured |

### Additional independently located market sources

- [Newzoo — Global games market key numbers, updated November 2026](https://new.newzoo.com/key-numbers): **2026 forecast** of **$213.9B** total games revenue, with **mobile $121.1B / 57%**. This supports platform scale, **not Evergrove demand**.
- [Sensor Tower — Q2 2026 Digital Market Index](https://sensortower.com/blog/q2-2026-digital-market-index-report): reports mobile **puzzle IAP above $4B, +17% year over year**, and **+1% year-over-year puzzle downloads** in that quarter. This is a genre snapshot dominated by other business models, **not premium indie revenue**.
- [JMIR Serious Games original study](https://pmc.ncbi.nlm.nih.gov/articles/PMC8176340/): workshop of **40 adults aged 50–85**; discusses difficulty with small objects, time limits, complex settings/rules and fast pacing. It supports usability testing, **not stereotyping all older players**.

**Important distinctions:** forecasts are not realized revenue; category growth is not title-level product-market fit; IAP genre revenue is not a price estimate for an ad-free premium game; selected user reviews are not representative statistics. The research report's platform and competitor summaries should not be treated as an audit of every review.

## Competitor lessons and the counterfactual

| Reference | What the supplied research found | What we can and cannot conclude |
| --- | --- | --- |
| **Monument Valley 2** | Players value calm visual beauty and emotional storytelling; some reviews call puzzles easy/short and mechanics underdeveloped | **Design warning:** deepen puzzle recombination; **not** proof Evergrove needs a specific number of levels |
| **Cozy Grove** | Some players value memory-rich persistent characters; others dislike repetitive chores and real-time gating | **Design warning:** no forced daily waits or chores; **not** proof persistent worlds always improve retention |
| **Dorfromantik** | Relaxing systemic play can coexist with meaningful challenge | **Design inspiration**, not a forecast or directly comparable mobile ASO channel |
| Generic offline pipe/puzzle games | Some reviews object to intrusive interstitial ads; some competitors market no ads/timers/accounts | **Positioning hypothesis:** player-respect messaging may matter; willingness to pay remains unproven |

## Strongest opposing case — preserve this in every review

Evergrove could be **two weak experiences joined together**: familiar pipe rotation that puzzle fans exhaust, and thin village dialogue that casual players skip. Current ecology branches rotate topologies and protect one tile; players may not perceive them as meaningfully different. Twelve boards may be too short, while adding complex systems may destroy the accessible core. An excellent **pure puzzle game** could be clearer, cheaper and easier to distribute.

**Disconfirmation:** if a fair matched-board test shows no improvement in consequence recall, preference or voluntary return, remove/simplify the village layer. Do not justify it by sunk cost, attractive screenshots or founder conviction.

## Load-bearing assumptions and status

| Assumption | Present evidence | What changes the decision |
| --- | --- | --- |
| "Puzzle changes place" is immediately comprehensible | Deterministic code and dry→flowing visuals; **no player data** | ≥80% uncoached goal comprehension, ≥70–75% specific consequence recall (provisional) |
| Persistent village improves return | Story, journal and choices implemented; **no matched preference/D1/D7 data** | Controlled matched-board preference and manual voluntary follow-up |
| Older/casual players can operate 4×4 | Accessible intent and platform guidance; **no real-device cohort** | Target-device mis-taps, hints, solve times, qualitative comfort |
| 2.5D earns cost | Opt-in prototype; **no cost/benefit study** | Counterbalanced 2D/2.5D comparison plus device metrics |
| 30 languages increase accessible reach | 30 draft catalogs; **shaping/native QA blockers** | Certified smaller locale subset and conversion evidence |
| Premium economics and ASO work | Category revenue forecasts only; **no pricing or channel proof** | Real price intent and affordable qualified acquisition |

## Decision log template — required before committing major scope

```markdown
### YYYY-MM-DD — [decision]
- Status: proposed / tested / accepted / rejected / reversed
- Problem and user outcome:
- Evidence: source, commit/build, participants, numerator/denominator
- Strongest counterargument:
- Reversible? Cost and opportunity cost:
- Load-bearing assumptions:
- Cheapest falsification:
- Continue threshold / stop threshold:
- Owner and next observable action:
- Revisit date or trigger:
```

## Current decision register (documentation only)

| Decision | State | Next evidence |
| --- | --- | --- |
| Unity 6.3/C# for Evergrove only | Provisional engine decision | Licensed Editor compile + actual iOS/Android build |
| 2D default, 2.5D opt-in | Existing experiment; no commercial art commitment | Same-state, counterbalanced player/device comparison |
| Offline/no account/no ads | Product principle; code intent | Production binary/dependency/network audit |
| 30 draft language choices | Catalog infrastructure exists | Advanced shaping + native QA; certify only passing locales |
| 12 authored boards and deterministic ecology | Existing prototype | Human difficulty and matched-world preference study |
| Runtime LLM, backend, multiplayer, daily lockout | **Not authorized** | Separate proof of incremental player value and cost |

**North star:** make the connection between a solved stream and a remembered place unmistakably good. Do not confuse more features, more code or larger market forecasts with better player outcomes.
