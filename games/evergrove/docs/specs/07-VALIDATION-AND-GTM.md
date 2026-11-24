# 07 — Validation, six-week delivery, ASO and commercial gates

**Decision question:** does a readable short water puzzle **plus persistent, visible, truthful village consequences** produce enough incremental player preference, return desire and willingness to pay to justify its cost and distribution?

This is an **experiment plan**, not a report of completed player studies, revenue or retention.

## Research operations contract

Use [10 — Preregistered experiments](10-EXPERIMENT-PROTOCOLS.md) for assignment, fixture matching, timing, prompts, failure inclusion and data handling. Source-implemented v4 hints or altered board difficulties may **confound** comparisons if study arms differ. Do not call current Living vs PuzzleOnly a narrative-only randomized A/B after chapter 1. Record an actual study report and build SHA with [11 — Evidence packets](11-EVIDENCE-AND-RELEASE-PACKETS.md) before concluding any lift.

## Three different experiments — do not combine their conclusions

| Experiment | Question | Design | Suggested sample / limitation |
| --- | --- | --- | --- |
| A — First-session usability | Can target players play and understand the causal loop? | Observe uncoached phone sessions, time to first input/solve, recall, mis-taps and hints | **8–12** formative testers; finds gross defects, not population rates |
| B — Living world versus puzzle-only | Does the village actually add value? | Preassign balanced conditions, matched boards/input/difficulty, same instructions, blinded/unprompted recall, manual D1/D7 follow-up | **20–30** representative players for a directional screen; small differences cannot be resolved |
| C — 2D versus 2.5D | Does the miniature improve causal comprehension/attachment enough to cover its cost? | Same saved world histories, text, sound and device; alternate viewing order; measure GPU/memory/authoring time | **10–15** for qualitative comparison; not proof of retention |

**Critical confound in the existing implementation:** `Puzzle Only` uses original boards, while `Living World` uses transformed and anchored variants from chapter 2. Comparing their return rates as if only narrative differs would be misleading. For experiment B, **hold puzzle topology and difficulty constant** (e.g. matched fixtures/variants or first-chapter-only causal test), or explicitly report the confound. The in-game mode toggle is research scaffolding, **not randomized assignment**.

## Hard preregistration rule — VAL-002

**Current A/B code is NOT causally valid after Chapter One.** Living World chooses branch-rotated and anchored variants; Puzzle Only uses canonical boards. A preference or retention difference could be a puzzle-difficulty difference rather than narrative value.

Do not report a living-world lift unless both conditions use **identical topology hash, initial rotations, anchored cells, hint behavior, difficulty, session length, actual text legibility, device class and exposure timing**. Only the **visible consequences and authored memory layer** may vary. Record exact source build, board fixture checksums, order assignment, participant inclusion/exclusion and question script **before collecting preference responses**.

If matching cannot be implemented yet, limit the first study to the **identical first board** or labeled non-causal qualitative impressions. No amount of retrospective statistical adjustment turns an unmatched small experiment into a valid product decision. The current in-game comparison toggle is a facilitator tool, not participant randomization.

### Formative versus directional product decisions

- **8–12 uncoached sessions:** diagnose repeated failure modes and observe actual behavior (goal, first tap, mis-taps, consequence notice, hint frustration, interruptions), report raw counts and recordings with consent. Percent targets are provisional triage, **not** population estimates.
- **20–30 matched-condition sessions:** preregister comparison and report raw preference/recall counts, follow-up losses, uncertainty and alternative explanations. It may support a directional decision, **not** a precise D7 uplift claim.
- **After two consequence-presentation iterations:** if players still prefer equally good pure puzzles or do not recognize cause→effect, **simplify the world layer**; if core puzzle engagement also fails, stop or pivot rather than add daily content.
- **No P2 expansion** before M4, and no ASO/pricing rollout before an actually usable binary and truthful listing.

## Metric dictionary — define before recruiting

| Metric | Operational definition | Collection and caveat |
| --- | --- | --- |
| Time to first meaningful input | Seconds from first interactive screen to first valid tile rotation | Existing opt-in local report can capture first move; researcher observes launch/menu time |
| Unaided goal comprehension | Player states how to restore water without researcher supplying the answer | Ask neutral "What are you trying to do?" before teaching |
| First solve / solve time | Seconds from interactive board to successful water route | Log board ID, mode, device, prior puzzle familiarity and whether hints used |
| Causal recall | After 10 minutes, names an actual choice **and** a specific physical or puzzle effect | Score against saved event history; do not count vague "it got nicer" |
| Mis-tap rate | Invalid or unintended touch actions per 10 minutes | Observe and annotate; in-game rotation count alone cannot distinguish errors |
| Hint recovery | Stuck tester resumes purposeful play after progressive hint | Record stage used and whether they felt spoiled |
| NPC memory recognition | Player identifies a truthful remembered event without leading prompt | Separate from simply liking dialogue |
| Living-world preference | Forced or ranked preference after **matched** experiences, with open reason | Report raw counts, order and confidence; no causal certainty from small samples |
| Voluntary D1/D7 return | Player **chooses** to reopen on day 1/day 7, without streak, incentive or mandatory task | **Manual consented follow-up**; existing local counters do not measure this |
| Price intent | Player chooses a real price/tradeoff or commits to a paid pilot/demo unlock | Compliments and hypothetical "would buy" answers are weak evidence |
| Store creative conversion | Listing views → qualified installs for truthful creative | Use store experiment tools when enough traffic; no fake statistical significance |

**Report:** numerator/denominator, participant characteristics, recruitment channel, assigned condition, missing follow-up, actual device/build and negative quotes. Never convert small-sample percentages into precise forecasts.

## Newly implemented prototypes awaiting independent validation

- The three hint stages are **explicit choices**; Stage 1/2 are clue-only, Stage 3 aligns one incorrect tile. Count `LocalPlaytest.hints` as **all hint requests**, not the number of mutated tiles. `WorldModel.HintsUsed` increments only when a tile is aligned. Report both definitions honestly.
- A non-color `!` now marks leaking tiles, but which joint is actually open may remain ambiguous: evaluate interpretation with grayscale displays.
- The source-grounded journal includes only the **latest** proven board rule and avoids fictional past effects. Test unprompted recall after a genuine choice.
- The 45 deterministic diagnostic board profiles expose **minimum clockwise taps**, not human challenge; keep observations null until a real user dataset exists.
- The first board has been moved earlier in the Unity-generated scroll hierarchy, but actual first-tap time remains unknown.

**Validation remains blocked until M1/M2**: no inference about retention, adoption or comfort should be drawn from these source changes.

## Seven-day first reality check

| Day | Work | Deliverable / exit gate |
| --- | --- | --- |
| 1 | Get Android APK and iPhone build path; fix launch/touch crashes only | Build logs and actual phone screenshots |
| 2 | Prepare neutral interview script, local consent, metric sheet and current-vs-consequence-first comparison | Prewritten protocol, no hidden analytics |
| 3 | Recruit representative 40+ (bands 40–49, 50–64, 65+) and casual puzzle testers, including users who enlarge text | Target 8–12 formative sessions; document convenience bias |
| 4 | Observe uncoached first minute, first 3 puzzles, choices, hints, language and save/reopen | Raw times, mis-taps, quotes, critical bugs |
| 5 | Create two presentation variants: current versus visible consequence-first, keeping puzzle content identical | Controlled stimuli and assignment order |
| 6 | Counterbalance and test both; ask unprompted "what changed, why, would you return?" | Preference/recall table and failures |
| 7 | Decide **continue / simplify / stop**; select the one highest-value next fix | Written decision with thresholds, counterarguments and owner |

If Unity cannot produce a usable phone build by day 1, the priority becomes fixing the build. Do **not** substitute an internal simulator video for real touch evidence.

## Provisional thresholds and stop rules

These are **decision heuristics from the supplied research**, not empirically validated benchmarks:

- **Continue testing the integrated concept** if ≥80% of target first-time testers explain the puzzle goal unaided, ≥70–75% recall a specific real village consequence after 10 minutes, and a clear majority prefers the living-world condition **when board difficulty is matched**. Show raw counts; do not overclaim.
- **Simplify the village** if puzzle enjoyment/comprehension is strong but causal recall, attachment or matched preference is weak after two presentation improvements.
- **Stop or pivot to a pure premium puzzle** if the living-world layer adds no observable preference or return desire after a fair test, if readability remains poor, or if no low-cost route exists to reach buyers.
- **Do not greenlight 2.5D** unless it improves unprompted consequence recognition/attachment **and** fits measured device/art cost; keep 2D when outcomes are similar.
- **Do not scale localization** beyond certified markets or content beyond 12 chapters before solving core quality and distribution.

**Small-sample caveat:** 8/10 is not a population estimate; a 20–30-person comparison cannot detect modest retention lifts. Repeat with larger, less founder-biased cohorts before investing heavily.

## Six-week dependency-aware plan

| Week | Theme | Must produce | Stop/scope gate |
| --- | --- | --- | --- |
| 1 | Reality and first minute | Verified phone builds, safe areas, save crash tests, uncoached first 60s | If no reliable build, defer all art/new features |
| 2 | Puzzle quality | Solver metadata for 12+33, hint ladder, difficulty review, content-tool prototype | If players cannot understand puzzles, do not add ecology rules |
| 3 | Emotional causality | Consequence-first reveal, causal journal, bounded truthful NPC memory, ending clarity | If village preference absent, simplify rather than expand |
| 4 | Accessibility/localization | Correct shaping path, touch/contrast/large text/screen-reader audit, **certified** locale subset | Disable unverified locales; do not advertise unsupported accessibility |
| 5 | Feel and art decision | Subtle audio/haptics if cheap, 2D/2.5D matched comparison, performance records | 2.5D must earn its cost |
| 6 | Commercial proof | Polished slice, three store screenshots, 30-second video, small-market metadata, pricing test, decision log | No launch or expansion approval without demand/distribution evidence |

**This is an indicative sequence, not a six-week promise.** Complexity of Unicode UI migration, native reviewers and device build issues can exceed the schedule. Protect the reality gate and shrink scope instead of pretending the plan is on time.

## Founder-led distribution: cheapest meaningful test

1. Define two or three real search intents: **water/pipe puzzle**, **offline relaxing puzzle**, **cozy game with lasting choices**. Verify query volume/competition with actual store tools rather than inventing numbers.
2. Recruit **10–30 highly relevant** puzzle/cozy players or community contacts with personalized, consent-respecting outreach; prioritize actual phone sessions and comparisons, not broad surveys.
3. Show a **30-second gameplay clip** with the three-scene promise: dry stream → rotation → changed village. Ask for a real test commitment, not praise.
4. Test a truthful landing/store creative pair. Measure qualified installs/requests and the proportion who actually play, with source and denominator.
5. Only after an audience responds, test premium versus free-demo/one-time-unlock price points and a small certified locale set. Do not infer willingness to pay from mobile industry revenue.

### Store listing constraints to recheck at submission

The supplied research reports Apple app name/subtitle **30 characters each**, keyword field **100 characters**, and Google Play app name **30** / short description **80**. Platform limits and policy can change; recheck official docs before submission. Apple product-page optimization/custom pages and Google Play store-listing experiments are appropriate **only with sufficient traffic**. Avoid competitor-name stuffing, unsupported "AI world", "infinite", "brain training" or medical claims.

**First three screenshots:** (1) restore the water, (2) choose what grows, (3) your village remembers. Localize screenshots **only** for shipping-certified languages.

**Brand clearance:** the research report flagged another indie game named "Evergrove" on itch.io. Check stores, trademark databases and counsel as needed before publishing; no legal conclusion is made here.

## Ethical revenue architecture

**Preferred hypothesis:** paid game or free demo → one-time full unlock; substantial optional expansion packs only after content demand. **Reject:** forced ads, paid hints, lives, subscriptions, streak penalties and paid narrative outcomes. Keep all core puzzles, journal and save behavior offline. A store entitlement may need its own secure restore flow; do not add billing architecture before validating price intent.

## Risks, cheapest tests and actions

| Load-bearing assumption | Strongest contrary explanation | Cheapest disconfirmation | Next real action |
| --- | --- | --- | --- |
| Consequences create attachment | Players just want good pipe puzzles | Matched living/puzzle-only comparison with unprompted recall | Show one before/after reveal to 8–12 users |
| Target users can operate 3×3/4×4 | UI is too small/complex even without a timer | Uncoached compact-phone session with enlarged text users | Observe first three boards, record mis-taps |
| Twelve boards support a premium promise | Too short, easy or repetitive | Compare completion and price intent against polished reference | Time all 12, ask actual purchase tradeoff |
| Founder can distribute economically | Store search competition overwhelms discovery | 10–30 targeted invites and a small truthful creative test | Recruit first 10 external testers |
| 2.5D is worth extra cost | 2D communicates the same change more clearly | Same-state counterbalanced visual comparison + device measurements | Show identical garden in both modes |
| Thirty locales improve reach | Broken shaping/translation destroys trust | Native reviewer and compact-device screenshot matrix | Certify one market at a time |

**Decision artifact required:** date, cohort, assignment, raw counts, alternative explanation, what would change the decision, next owner/action, and a **stop condition**. No "the game is best possible" claim until independent players demonstrate the experience.
