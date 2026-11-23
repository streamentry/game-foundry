# Evergrove — evidence plan, falsification and founder-led distribution

**Status:** *proposed tests; no human outcomes are recorded here*. Earlier [founder research](../../../docs/research/evergrove-build-decision-2026-11-22.md) planned a 20–30-person Living World vs PuzzleOnly comparison before gameplay mechanics diverged. **V3 branches now confound that comparison beyond chapter one**. This document explicitly repairs the experimental interpretation.

## 1. Decision hierarchy

1. **Can it run on a phone?** Unity Editor compiles; touch, text, save, battery and locale behavior work.
2. **Can an uncoached player solve and understand it?** First action, rule comprehension, hint use, clear cause of restoration.
3. **Do consequences have independent value?** Someone voluntarily chooses another chapter *for the village*, not merely because asked.
4. **Does mechanics + world beat a polished alternative?** Comprehension, delight, voluntarily returning players and quality compared to a real peer.
5. **Can the game find and keep users economically?** Verified search intent, channel experiments, ethical willingness to pay, retention.
6. Only then consider expensive world simulation, LLMs, creator tools, social systems or scalable content.

Do **not** jump from code volume / localization breadth / passing unit tests to steps 4–6.

## 2. Four distinguishable experiments

| Test | Control | Treatment | Question isolated | Measurement |
| --- | --- | --- | --- | --- |
| **A — first-chapter meaning** | PuzzleOnly, chapter 1, same authored 3×3 | Living, same board + visible creek, initial choice and story | Does the world layer make first play clearer or more compelling? | First-use comprehension, first solve, observed smile/delight, immediate desire for another chapter |
| **B — ecological branch mechanics** | **Must build a matched Living-world variant with original chapter-2 board and same art/story** | Living with v3 rotated, protected landmark | Do altered mechanics themselves improve or harm experience? | Per-board solving time, accuracy in explaining prior-choice→landmark, frustration, next-chapter intent |
| **C — visual treatment** | 2D garden, fixed world history | 2.5D miniature, **same** history and UI | Does 2.5D help causal comprehension enough to justify cost? | Unaided explanation, forced preference with counterbalanced order, device GPU/heat |
| **D — voluntary return and distribution** | Best polished reference / relevant acquisition channel cohort | Evergreen candidate experience with equal onboarding commitment | Is there an actual reason to return and a viable discovery channel? | D1/D7 actual return with denominators, source/search intent, willingness to recommend/pay |

> **Letter mapping (labels only, not renumbered):** VALIDATION A ≈ spec 07 B / spec 10 B restricted to chapter 1; VALIDATION B has no spec equivalent (matched branch-mechanics control); VALIDATION C = spec 07 C = spec 10 C; VALIDATION D has no spec equivalent. Spec 07/10 "A" (uncoached usability, VAL-001) corresponds to §3 Day 4 and the §4 metrics here.

**Existing in-app PuzzleOnly switch only isolates part of A.** Beyond chapter 1 its board rules diverge from Living, so **do not report a “narrative retention lift” from current two-mode data**.

**B control implemented in core (`PlayMode.PuzzleMatched`, M4 gate prerequisite).** The matched condition shows no village choices or story, but its boards for chapters 2–12 come from a predeclared 11-entry variant schedule. Deriving that schedule from a Living run (`WorldModel.ScheduleFromChoices`) gives byte-identical boards chapter by chapter; the .NET suite checks all 33 chapter×variant pairs, all 3^7 choice prefixes, and three full Living paths. This proves **board equivalence in code only**. It does not yet supply participant evidence, The Unity UI now selects it **only in Editor and Development builds** (`UNITY_EDITOR || DEVELOPMENT_BUILD`): a facilitator enters the 11-digit schedule, confirms a two-step replacement of any saved garden, or copies a Living run's schedule. The matched control has **not been tested on a device**, and M4 remains blocked until a predeclared build hash and a real session protocol exist. Puzzle-only still uses unchanged boards and must not be reported as a B control.

For 2D/2.5D, show the same actions and art information in both, with no extra animation/sound given only to treatment.

## 3. Seven-day focused execution (real-world feedback, not another design cycle)

| Day | Observable action / artifact | Hard stop before proceeding |
| --- | --- | --- |
| 1 | Run manual Unity Editor build/import; correct any compiler errors; capture launch and first chapter on a real target phone | No player study while app can't be controlled |
| 2 | QA 3×3 and 4×4 taps, blue-wave/sprout endpoint icons and ! leak markers, 2D/2.5D fallback, force-kill saves, locale switch and reset | Fix data loss, broken input or unreadable text |
| 3 | Recruit 20–30 relevant adults via personal outreach and narrow communities; include the proposed 45+ segment explicitly | Do not substitute only founder friends / staff |
| 4 | Observe 8–10 first plays in a pre-balanced A schedule; record verbatim “what changed and why?” | If most cannot solve or explain the creek, simplify onboarding before scale |
| 5 | Observe 8–10 more; run a separate small visual C comparison (10–15 people, order counterbalanced) | Do not interpret art preference as retention |
| 6 | Collect remaining participants and one voluntary next-day follow-up where feasible; run narrow discovery/outreach messaging tests | Do not confuse prompt-induced return with organic return |
| 7 | Aggregate raw counts/denominators, unprompted quotes, failures and device logs; document one `continue / simplify / stop` decision | No automatic production greenlight because a deadline passed |

**Human-study safeguards:** ask permission to record notes, avoid unnecessary personal identifiers, use explicit informed consent for any screen recordings, and do not copy private content into a public GitHub repo. The in-app `LocalPlaytest` report is **voluntary, memory-only and clipboard-exported**; it does **not** measure identities, cohorts or D1/D7. Any follow-up list must be separately permissioned and kept off the public repo.

## 4. Minimal event & metric dictionary

| Question | Recommended observable metric | Definition / denominator | Known caveat |
| --- | --- | --- | --- |
| Is onboarding understandable? | Uncoached first action | Participants who rotate a tile correctly with no prompt / all starters | A forced tutorial can inflate apparent comprehension |
| Is spatial rule learned? | Unassisted first solve | First board solved without Guide me or facilitator help / all starters | Record abandonment and elapsed time separately |
| Is world consequence legible? | Causal attribution | People who say **their specific choice** changed creek/board/NPC without being told / those who saw the change | Beware researcher hints and order effects |
| Do mechanics feel good? | Time to solve & help usage | Median, quartiles, hints and restarts per chapter/variant among everyone exposed | Distinct board variants can differ in effective difficulty |
| Is it worth more time? | Voluntary second chapter | Users selecting another chapter after being told they may stop / all first-chapter completers | Do not count “continue” pressed under facilitator request |
| Do people return? | D1/D7 voluntary return | Distinct consented starters opening without prompting next day / within seven days | Current local playtest notes **cannot** measure this automatically |
| Does visual style justify itself? | Unprompted visual causality comprehension | Correct environment changes named under comparable 2D vs 2.5D conditions | Small paired comparison is directionally informative only |
| Is there a channel? | Qualified acquisition signal | Target people who reach a playable session from a *verified* query/channel / reached audience | No verified ASO ranking, search volume or paid conversion currently |

**No fabricated baseline:** the above metrics have no observed values yet. Set budgets and targets prospectively. Do not calculate confidence intervals from made-up counts.

### Provisional decision thresholds (proposals, not claims)

For an inexpensive exploratory screen, record these **before** observing players:
- At least roughly **70% of relevant participants** can take the first correct action within 30 seconds without coaching; below this, prioritize onboarding.
- At least **60%** can correctly describe an environmental consequence and its cause *unaided*; below this, the differentiator is not being perceived.
- A meaningful share **voluntarily** starts a second puzzle; establish the exact target and sample-specific denominator before any test, rather than back-fitting.
- No unexplained save loss, blocked touches, severe clipping, or critical text-shaping failures in the intended launch language/device.
- **Continuing product investment requires** plausible distribution and positive observed desire to return, not merely a majority saying the art is beautiful.

These are **heuristic go/no-go triggers**, not powered statistical effect thresholds. With 20–30 people, a small numeric difference between conditions is generally inconclusive. Record failed follow-ups and attrition explicitly.

## 4A. Canonical M3/M4 threshold table

**Precedence.** Per the [handbook README](README.md), specs are proposals and handbook chapters describe the as-built state. [09 — Execution control](specs/09-EXECUTION-CONTROL.md) defines M3 and M4 as evidence gates. [10 — Experiment protocols](specs/10-EXPERIMENT-PROTOCOLS.md) requires prospective thresholds recorded before observation. No threshold below was chosen in this document.

| Measure | Threshold stated | Source | Gate |
| --- | --- | --- | --- |
| First correct action / valid tap | 8 of 10 starters (80%) act within 30 s | [design.md §12](../design.md) | M3 |
| First correct action | ~70% within 30 s (provisional) | This file, "Provisional decision thresholds" | M3 |
| Goal explained unaided | 3+ of 10 cannot infer goal → redesign (i.e. ≥70% needed) | design.md §12 | M3 |
| Goal explained unaided | ≥80% | [spec 07](specs/07-VALIDATION-AND-GTM.md) | M3 |
| Restoration recognized | 3+ of 10 miss → redesign (i.e. ≥70% needed) | design.md §12 | M3 |
| Consequence noticed | 7+ of 10 notice (70%) | design.md §12 | M3 |
| Consequence recalled after 10 min | ≥70–75% | spec 07 | M3 |
| Consequence described unaided | ~60% (provisional) | This file, "Provisional decision thresholds" | M3 |
| Median first meaningful input | <15 s | [spec 01](specs/01-PRODUCT-AND-EXPERIENCE.md) | M3 (observed) |
| Living-world preference | "clear majority prefers" when board difficulty is matched; no number | spec 07 | M4 |
| Voluntary second chapter | No number; set before testing | This file, "Provisional decision thresholds" | M4 |

**Which rule the facilitator applies**

- **M3:** Gate 09 requires 8–12 uncoached participants, all starters counted, and failures fixed or documented. The only numeric rule with a defined action is the 10-starter rule in design.md §12. Apply that rule and record raw counts. Do not apply the 70%/60% or 80%/70–75% figures. **FOUNDER DECISION NEEDED:** confirm design.md §12 as the M3 stop/proceed rule.
- **M4:** Gate 09 requires a preregistered, puzzle-identical study (VAL-002). It cannot be run on the current mode toggle, so no M4 number applies yet. **FOUNDER DECISION NEEDED:** choose M4 thresholds before the matched build is tested.

**Remaining disagreements (FOUNDER DECISION NEEDED)**

1. First action: 80% (design.md §12) vs ~70% (this file).
2. Goal comprehension: ≥70% implied by design.md §12 vs ≥80% in spec 07.
3. Consequence: notice (70%, design.md), 10-minute recall (70–75%, spec 07) and unaided description (60%, this file) are three different measures. Choose which one gates M3.
4. Preference: spec 07's "clear majority prefers the living world" cannot be observed in a between-subject design, because each participant experiences only one condition and so never states a preference between them. It needs within-subject exposure to both conditions, which conflicts with the matched-exposure controls in §2. Choose a within-subject design or drop the metric.
5. Speed: median first input <15 s (spec 01) and first correct action within 30 s measure different statistics. Record both; neither replaces the other.

## 5. Qualitative interview script (no leading)

Start with: “Play in whatever way feels natural; you may stop whenever you like.” Remain silent during the first minute.

After solving: “What just happened?” “What do you think caused the change?” After a choice: “What do you expect will happen next?” After chapter two: “Did anything from your previous choice matter here? What makes you say that?”

Exit questions: “What was confusing?” “What felt unnecessary?” “If you opened this again, what would you want to do?” “Which game would you choose to play now, this one or [polished reference]?” Avoid asking “Don't you love how the world remembers?”

Track **verbatim evidence**, not retrospective researcher impressions. Separate evidence of comfort from actual behavior.

## 6. Founder-led channel work

**No asserted ASO gap.** Evaluate 10–30 concrete highly relevant audiences/keywords in **one language and one storefront at a time**. Confirm actual contemporary search behavior and competitors rather than assuming age-band demand from language population or broad app-category numbers.

Cheapest distribution sequence:
1. Build a playable, shareable Android/WebGL test package; use human-targeted outreach, not artificial reviews.
2. Recruit creators/community moderators who already enjoy quiet logic/spatial games; offer a private prototype and request feedback, not endorsements.
3. Test two honest promises: **“Relaxing water puzzles”** versus **“A garden shaped by your choices.”** Measure real play starts from each.
4. If suitable, ask first 10–30 target users about paid willingness; **no** ad network/purchase SDK until pricing and ethics are evaluated.
5. Do not spend meaningful paid user-acquisition budget unless first-use quality and return intent already justify it.

External competitors: Gardenscapes/Lily's Garden/Merge Mansion, Cozy Grove/Tsuki's Odyssey/Animal Crossing, Dorfromantik/Terra Nil, Reigns/Wildermyth. Treat these as **benchmark hypotheses**, not verified 2026 feature audits. Verify each live version and store positioning before citing them in a pitch.

## 7. Ethics, privacy and guardrails

No hidden profiling, manipulative streaks, pressure timers or fabricated NPC intelligence. If measurement is introduced beyond the in-memory `LocalPlaytest` summary, it requires a separately reviewed consent model, data minimization, retention policy, account deletion/security and user transparency. Family and rest time are product constraints, not monetization leaks to exploit.

**Next observed action:** one licensed Unity build, one real phone, one unaided playthrough — then decide whether to deepen the world, simplify into a superior pure puzzle, or stop.
