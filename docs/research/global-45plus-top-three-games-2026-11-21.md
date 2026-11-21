# Global games for adults 45+: the first three playtests

**Decision date:** 2026-11-21  
**Status:** exploratory hypotheses; age fit, repeat use, acquisition and economics are unvalidated.  
**Recommended order:** **1. Good Eye → 2. Quiet Shot → 3. One Path.**  
**Current commitment:** document and compare these experiments; no production game or public release is approved by this ranking.

## Decision and scope

Investigate **short, untimed visual challenges that make adults feel observant and capable**. The useful Flappy Bird analogy is a legible action, immediate feedback and an understandable reason to retry. A rare breakout is not a forecast or a repeatable acquisition plan.

This is an order for buying information cheaply, not a ranking of proven market opportunities. Good Eye tests perceptual estimation; Quiet Shot tests trajectory prediction; One Path tests deliberate route planning. All three face existing competitors. Their controls and content must be independently designed, and a demonstrated reason to choose our product is still missing.

### Relationship to the existing Kakuro decision

| Track | Current priority | Question it answers | Evidence still required |
| --- | --- | --- | --- |
| **Search-led product research** | [Kakuro, with Shikaku as fallback](first-game-decision-2026-11-21.md) | Can an original product win an existing, measurable App Store search intent? | Country-specific demand, physical storefront results, a repeatable product gap and comparative player evidence. |
| **Global visual playtest exploration** | Good Eye, Quiet Shot, One Path | Can a small visual challenge earn voluntary replay, return and introductions to other players? | Target-player behavior, preference against existing games, actual referred players and sustainable economics. |

The new shortlist does not invalidate the earlier research or promote any title to production. The [research protocol](README.md), [architecture](../architecture.md), [design rules](../../DESIGN.md) and [release checklist](../release-checklist.md) remain applicable. The existing ASO production gate is not waived: referral evidence cannot be relabeled as search demand. A commercial plan that relies mainly on referral acquisition would need its own recorded distribution decision and release criteria before production.

The requirement in AGENTS.md to compare 30 queries before choosing a prototype also remains in force. Verify the existing screen and applicable prototype gates before beginning the proposed week-one implementation; this note does not waive them. Observing existing competitor games can inform that selection while gates remain open.

**If ASO-only remains a hard constraint, none of these three is a validated selection.** Their proposed discovery path includes an exact playable challenge opening in a browser. English query phrases below are research seeds, not measured demand or recommended store metadata.

## Audience and what the evidence supports

Keep adults **45+** as the studio audience. For this exploratory track, first recruit adults **45–64 who already enjoy casual puzzles or small personal challenges**, and examine participants aged **65+** separately. This is a recruitment hypothesis, not a finding that these ages are the best buyers. The existing Kakuro study retains its U.S. 50–75 segment.

AARP's October 2026 study reports that 75% of U.S. gamers aged 50+ play to relax, 79% usually play alone, and 65% typically play for 15–60 minutes. It identifies several motivational segments. These findings support testing a comfortable solo experience with optional sharing; they do not prove demand for these mechanics, global 45+ preferences, willingness to pay or health benefits. Short rounds should permit longer voluntary sessions. [S1]

Use large objects, adjustable input, visible recovery, readable feedback and difficulty that comes from the decision. Follow the repository's minimum 48 logical-pixel touch targets. Do not infer sensory or motor ability from age. Visual estimation and spatial puzzles will not suit every player; record actual usability barriers and preferences.

## Ranked shortlist

Names are working labels. Rank reflects immediate comprehensibility, distinct learning value, content cost and speed of falsification. No numerical opportunity score is assigned without measured inputs.

| Order / ID | Game and core action | What could make someone return? | Strongest reason against it | First test |
| --- | --- | --- | --- | --- |
| **1 / GV-01** | **Good Eye:** position one cut to divide a shape into equal areas. | A clear near miss and visible improvement. | It may be a disposable novelty; direct daily/social versions exist. | A small set of asymmetric shapes, adjusted-cut controls and comparison with an existing 50/50 game. |
| **2 / GV-02** | **Quiet Shot:** aim one ball to clear all targets on a small board. | Predicting a sequence, watching it unfold and correcting a miss. | Precise angle control may overwhelm the reasoning; an established competitor already serves this premise. | Eight original, verified boards with forgiving aim ranges. |
| **3 / GV-03** | **One Path:** traverse every visible edge exactly once. | Planning a complete route and resolving a tempting dead end. | A familiar mechanic may feel repetitive; rules and crossings can be misread. | Eight compact boards, comparing tracing with tap-to-extend input. |

Run experiments sequentially. Do not build three production games in parallel. Within a comparative session, rotate the order of experiences so that the research ranking does not bias the results.

## 1. Good Eye — visual estimation

**Player promise:** judge a shape, make a cut and immediately see how close you came to an even split.

### Smallest complete experience

1. Display one large, asymmetric convex silhouette.
2. Let the player position a stable cutting line with generous handles and confirm when ready.
3. Reveal the two areas as complementary percentages, such as 48.6% and 51.4%.
4. Offer another shape or a retry. Practice can be unlimited; a later daily comparison scores first attempts on identical shapes separately.

Use a small original shape set first. Compute true area on each side; do not score where the line crosses a bounding box. Keep percentages hidden while aiming. Show enough precision to communicate improvement without making tiny device/input differences the main game. Convex shapes keep each cut to two pieces; symmetric shapes are poor initial challenges. There can be many valid 50/50 cuts, so this is estimation skill, not a unique-solution logic puzzle.

**Why first:** it exposes the whole player proposition with little content or instruction. A first test can distinguish enjoyable judgment from control frustration without building a level economy or a large content generator.

**Direct competition:** Slicing Expert already advertises irregular convex shapes, 50/50 scoring and five-round solo/local play; its Google Play listing displays 5K+ cumulative downloads. Half: Cut in Two advertises calm presentation, daily sets and sharing. WAAI's 50/50 has a browser demonstration and daily attempts. Halfsies already offers a cut-for-a-friend interaction. These are observed listings, not evidence of current acquisition, profit or age fit. [S2–S5]

**Three load-bearing assumptions:** near-miss improvement stays enjoyable; the controls reward intended judgments fairly; some satisfied players return and introduce someone else.

**Cheapest falsification:** observe a few adults on their own phones with this interaction and an existing 50/50 game, using comparable shapes and alternating order. Distinguish failed control from poor estimation. If players can control it but decline another round, more generated shapes are not the next investment. Advance only to the cohort test below if the basic loop shows voluntary pull.

**Discovery seed:** an unsolved shape and a link to attempt that exact shape. Test “try my shape” before a chooser mode: when a cut is almost equal, selecting the larger piece may feel like guessing. English search seeds: `cut in half game`, `50 50 puzzle`; demand is unknown.

**Monetization hypothesis:** optional practice/archive or original challenge packs after repeat use appears. Willingness to pay is unknown. **Stop:** after one deliberate revision, comfortable play still produces weak optional replay or return, or players show no reason to prefer this over the existing alternatives.

## 2. Quiet Shot — a single planned shot

**Player promise:** choose a direction, launch once and watch a small board clear in a satisfying sequence.

### Smallest complete experience

- One fixed launch point, constant ball speed, stationary large targets and a few fixed reflecting surfaces.
- An adjustable aim direction and a large launch button. Show the initial direction, not an automatic solved trajectory.
- Targets clear on contact and the ball passes through them; only the marked reflecting surfaces change its direction.
- Every shot ends when all targets clear, the ball leaves the board, or it reaches a visible four-reflection limit. Display the limit before launch, keep retry available, and validate failed as well as successful routes so misses cannot loop indefinitely.
- Immediate retry and eight original boards, beginning with direct shots before introducing reflections.

No timed release or swipe-strength calibration is needed for this test. Difficulty should come from predicting consequences. Verify that introductory solutions occupy forgiving angle ranges; a mathematically possible shot requiring extreme precision is not an accessible introductory board. A later implementation follows the existing Godot/GDScript architecture with independently testable collision and scoring rules.

**Why second:** it tests a different pleasure from Good Eye: a planned sequence working as expected. It needs more simulation and board validation, but a few boards can expose whether the entire interaction is attractive.

**Direct competition:** Okay? already centers on clearing a board with one line. Its Google Play listing displays 10M+ cumulative downloads and describes optional hint ads. This is evidence of an established category and a strong comparator, not a novel premise, a current organic-install rate or a 45+ audience measurement. [S6–S7]

**Load-bearing assumptions:** players understand the reflection rules; they can execute their intended aim; correcting a miss feels informative rather than arbitrary.

**Cheapest falsification:** six to ten formative phone sessions with eight verified boards. Observe three introductory boards, then offer optional play. Ask what a missed shot would need to do differently. If reasoning is sound but execution repeatedly fails, revise input once. If players execute comfortably but decline another board, stop before expanding the generator or visual production.

**Discovery seed:** a short honest demonstration and a playable link to the same unsolved board. English search seeds: `one shot puzzle`, `ricochet puzzle`; demand is unknown. **Monetization hypothesis:** optional original board packs after repeat use is established. **Stop:** input remains too exacting, the failure feedback is unclear, or a current comparator is preferred with no reproducible unmet need.

## 3. One Path — discrete route planning

**Player promise:** find one continuous route that uses every connection once.

### Smallest complete experience

- Display a compact graph with large nodes and visible connecting edges.
- **Traverse every edge exactly once. Nodes may be revisited; an edge may not.** This is not a “visit each node once” puzzle.
- Support both tracing and tapping the next connected node to extend the same route, with undo and restart.
- A crossing is a junction only when a visible node marks it. Teach the rule on simple boards before testing a crossing.

Start with eight original boards: a short route, a node revisit, a tempting dead end and a clearly marked nonconnecting crossing. For the initial undirected graphs, validate connectivity and zero or two odd-degree nodes. With two odd-degree nodes, an Euler trail starts at one of them; with zero, it can start at any nonisolated node. Many routes may be correct. Check the player's actual path rather than require a single stored answer. These are proposed correctness requirements, not a claim that a generator already exists.

**Why third:** discrete choices help separate reasoning enjoyment from tracing accuracy. The tradeoff is a less immediate rule than “divide it evenly,” and a mature format whose novelty may wear off quickly.

**Direct competition:** 1LINE displays 10M+ cumulative Google Play downloads for its one-stroke format. One Line explicitly advertises covering every line once, untimed play and marked starts. Accessible controls and a daily board should be tested as improvements, not described as an uncovered market. [S8–S9]

**Load-bearing assumptions:** players distinguish nodes from edges; the presentation makes valid connections unambiguous; planning remains enjoyable after introductory boards.

**Cheapest falsification:** six to ten formative phone sessions. Ask players to demonstrate the goal, compare tracing with tap-to-extend in alternating order, and observe optional additional play. A short demonstration is allowed in the formative session; record it so it is not later counted as unaided cohort comprehension.

**Discovery seed:** an unsolved route that opens directly for play. English search seeds: `one line puzzle`, `one stroke puzzle`; demand is unknown. **Monetization hypothesis:** optional original puzzle collections after repeat use. **Stop:** users still misread the rule after one presentation revision, tap input does not resolve the observed barrier, or comfortable solvers consistently decline further boards.

## Acquisition, economics and the earlier localization hypothesis

The proposed referral funnel is **challenge sent → recipient opens → recipient completes an attempt → recipient returns**. Track onward introductions separately. A share-button click is not a sent message, an opened link is not a player, and an activated referral is not yet retained. The recipient should open the exact challenge without an install, account or code-copy step. Browser use must not be reported as an app install.

Begin recruitment in a market where relevant players can actually be reached. Record language, age band, prior puzzle habits and device. Before claiming global fit, repeat with an independent second-market cohort; do not pool away a poor result or infer worldwide appeal from a local convenience sample.

Country-specific paper puzzles remain a separate research direction. The conversation's Brazilian Portuguese cryptogram, French mots codés, Japanese kanji and readable-Sudoku examples are **leads**, not newly verified demand in this document. Native content can create value, but needs local editorial work and evidence that people seek it. Accessibility may improve conversion without creating a separate search channel. Prefer one localized product when only language changes; do not turn country variants into repetitive app submissions.

The three visual experiments have lower language-content requirements, but none has demonstrated a distribution advantage. A recurring audience and reliable access to new players would be valuable; generic puzzle code alone is not a defensible business.

**Illustrative economics only:** 10,000 new installs × 3% purchasing × $4.99 = **$1,497 gross sales**, before fees, taxes, refunds and costs. These are hypothetical inputs, not observed conversion or a price recommendation. Validate acquisition and willingness to pay separately. Preserve the existing launch target and reporting definitions; do not substitute browser visits or this illustration for the repository's non-branded organic-install goal.

## Seven-day test for the leading candidate

The first week tests Good Eye. Quiet Shot and One Path are next experiments if the evidence supports proceeding, not parallel production commitments. Adapt the per-round engagement measure before testing another mechanic; five short cuts and five longer logic boards are not equivalent effort.

| Window | Work | Observable output |
| --- | --- | --- |
| Days 1–2 | Inspect an existing comparator and run a few formative sessions; fix the clearest input problem. | Versioned small prototype, comparable task set and a record of assisted versus unaided play. |
| Days 2–3 | Recruit a fresh cohort of about 30 adults 45+ beyond developer friends; record segments and recruitment limits. | Eligible participant count, first-session dates and a predeclared measurement plan. |
| Days 3–6 | Leave the playable challenge available and observe optional play, return and referral activation. | Counts with denominators; no reminders counted as spontaneous return. |
| Day 7 | Compare outcomes against the declared gates and current competitors. | Continue, one revision, narrow the audience, or park; record what changed the decision. |

### Proposed spending gates for Good Eye

These are internal decision thresholds, not published benchmarks or proof of a market. Start the behavioral cohort after formative fixes; do not pool versions or replace dropouts to improve percentages. Cohort members receive the same opportunity to start, play more and share, without being asked to meet the targets.

| Measure | Predeclared first gate | Definition |
| --- | --- | --- |
| Unaided start | **24 of 30** | Complete the first cut without coaching; errors and requested help remain in the denominator. |
| Immediate pull | **18 of 30** | Voluntarily complete five cuts in the first session. |
| Next-day return | **9 of 30** | Open again and complete an attempt on the next local calendar day without a reminder. |
| Day-three return | **6 of 30** | Complete an attempt on local day 3 after first play, without a reminder. |
| Referral activation | **3 additional people** | Identifiable challenge referrals produce a completed attempt by someone outside the original cohort; self-opens, tests and duplicates are excluded. |

Count all 30 eligible starters for these gates, not only first-round completers. Record participant-local dates, exact build and challenge version. With this small convenience sample, report raw counts and uncertainty; do not interpret differences between tiny age or country subgroups as reliable population estimates.

Rotate comparator order, use tasks of comparable difficulty, and record the experience participants voluntarily choose when both are available. There should be an observable reason to prefer our version beyond saying that it looks pleasant. The numerical gates alone do not establish differentiation.

**Interpretation:**

- Good replay, return and actual referral activation: fund a larger independent cohort and second-market replication, then test economics.
- Good return but weak referrals: the organic-breakout thesis is unsupported; investigate a credible acquisition route before expanding.
- Strong first-session reaction but weak return: make one specific core-loop revision and retest with a fresh cohort.
- Persistent input frustration or boredom after that revision: park the mechanic and record the reason before trying the next one.

Day-seven retention occurs seven days after an individual's first session, so a day-2/3 cohort cannot yield that metric by project day 7. Record it on participant day 7. A later native release needs its own predeclared retention gate and the existing store, device and commercial checks.

### Minimal observation record

Record `experiment_id`, build version, challenge ID, pseudonymous participant ID, age band, market/language, device, assistance, first attempt, voluntary extra rounds, return day and completed referred attempts. Keep participation voluntary; do not collect contacts or message contents. Document retention and attribution definitions before adding any analytics SDK. Follow [the existing playtest protocol](../playtest.md) and repository privacy requirements.

## Current outcome and next action

**Actual player results:** none recorded for these three candidates. A conversational mechanic illustration is not a real-device playtest, a shipped build or an acquisition result. This document adds no keyword measurements, installs, retention results or new games to the repository.

**Confidence:** moderate in the learning value of this experiment order; low in any particular game's commercial opportunity; insufficient evidence to estimate breakout probability.

**Next observable action:** compare Good Eye's basic interaction with an existing 50/50 game on target players' own phones. Use the result to decide whether a fresh 30-person cohort is worth recruiting. Update [the decision log](decision-log.md) with actual behavior before expanding implementation.

## Primary sources and their limits

Sources were checked on **2026-11-21**. Store descriptions are developer claims; public pages do not constitute a hands-on audit or a physical-storefront search ranking. Download bands are cumulative Android totals, not monthly installs, organic attribution, iOS demand or age-specific use.

| ID | Source | What it supports |
| --- | --- | --- |
| S1 | [AARP, Play on Purpose, published 2026-10-22](https://www.aarp.org/pri/topics/technology/internet-media-devices/play-purpose-video-games-older-adults/) | U.S. 50+ motivations and habits. The study includes 1,014 gamers; it does not validate these games or global 45+ demand. |
| S2 | [Slicing Expert, Google Play](https://play.google.com/store/apps/details?hl=en-US&id=com.rocketpuncher.slicingexpert) | Direct 50/50, convex-shape, five-round competitor; displayed 5K+ cumulative downloads. |
| S3 | [Half: Cut in Two, Apple Albania storefront](https://apps.apple.com/al/app/half-cut-in-two/id6827065044) | Direct calm, endless/daily and shared-results positioning. |
| S4 | [WAAI Games 50/50, developer](https://www.waaigames.com/games/fifty-fifty) | Public browser demonstration and daily, area-scored product positioning; the page says iPhone release is forthcoming. |
| S5 | [Halfsies: I Cut, You Choose, Apple Singapore storefront](https://apps.apple.com/sg/app/halfsies-i-cut-you-choose/id6826800281) | Direct daily and friend-choice variant; no inference about traction. |
| S6 | [Okay?, Google Play](https://play.google.com/store/apps/details?hl=en_US&id=de.stollenmayer.philipp.Pop_1_1_Android) | Established spatial/physics comparator; displayed 10M+ cumulative downloads and optional hint-ad positioning. |
| S7 | [Okay?, original developer](https://www.kamibox.de/okay) | The developer's description of its board-clearing premise. |
| S8 | [1LINE, Google Play](https://play.google.com/store/apps/details?hl=en-US&id=com.gamestart.oneline) | Established one-stroke category; displayed 10M+ cumulative downloads. Its marketing wording is not our full rule specification. |
| S9 | [One Line, Google Play](https://play.google.com/store/apps/details?id=com.owen.oneline) | Direct every-edge-once, untimed graph-puzzle competition. |

Older marketplace assumptions should be rechecked before a spending or launch decision. None of these sources supplies an exact-query ASO opportunity, a representative competitor-quality comparison or a measured 45+ cohort for the proposed products.


## Implementation status — 2026-11-21

At user direction, **all three mechanic experiments now have source projects**: [Good Eye](../../games/good-eye/), [Quiet Shot](../../games/quiet-shot/), and [One Path](../../games/one-path/). The latter two have original eight-board test sets, deterministic rule engines, offline persistence, independent design notes, and separate verification/web-preview workflows.

**This is engineering implementation, not behavioral validation.** In particular, it does not imply that 45+ users will prefer these games to direct competitors, return, invite others, or pay; nor does it establish a search-acquisition opportunity. Run sequential real-player comparisons and retain the stop rules above.
