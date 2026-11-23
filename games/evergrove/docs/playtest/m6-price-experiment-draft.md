# M6 price experiment: draft design (Mosselora / Evergrove)

**Status: DRAFT. Not registered. Not recruiting. No price is configured by this document.**
Every numeric threshold below is `TO PREDECLARE` unless a source document declares it, and the source is quoted. Nothing here charges anyone. No billing SDK or IAP code is authorized by this draft.

Related: [09 — Execution control, M6](../specs/09-EXECUTION-CONTROL.md#milestone-gates-and-feature-freeze); [07 — Validation and GTM](../specs/07-VALIDATION-AND-GTM.md); [10 — Experiment protocols, commercial study](../specs/10-EXPERIMENT-PROTOCOLS.md); [verification-matrix.json](../specs/verification-matrix.json) (`STORE-004`, `VAL-005`); [PRODUCT](../PRODUCT.md) section 2; [VALIDATION](../VALIDATION.md); [vision §7](../../vision.md); [playtest README](README.md); [game-foundry AGENTS.md](../../../../AGENTS.md).

---

## 1. Purpose and gate

**Gate.** M6 requires a "real price experiment" before a commercial release slice ([09, M6](../specs/09-EXECUTION-CONTROL.md#milestone-gates-and-feature-freeze)). Two tests are attached:

| ID | Passing criteria (verbatim from matrix) | Evidence required (verbatim) | Status |
| --- | --- | --- | --- |
| `STORE-004` | "One-time unlock or premium price tested without ads/paid hints/streaks" | "Price experiment and entitlement QA" | `verified: false` |
| `VAL-005` | "10-30 targeted prospects offered real playtest or payment tradeoff" | "Recruitment and conversion/price decision log" | `verified: false` |

**Question.** Among adults aged 45+ who have actually played the game, is there real, unpressured willingness to pay a one-time price for the full game?

**Not the question.** The optimal price, the store conversion rate, or whether the game will retain players. Those need larger samples and are out of scope for 10-30 people.

**Ambiguity to resolve (founder).** `VAL-005` accepts a "real playtest or payment tradeoff", but `STORE-004` requires a price to be "tested". This draft treats `STORE-004` as requiring a price that is actually presented to participants. A non-monetary tradeoff alone should not mark `STORE-004` passed.

---

## 2. Current state: conflicts to resolve before any design runs

- **The user has already decided on free pricing.** The decision log records on 2026-11-23 that "all games in the current publishing batch, including Evergrove, are free" ([decision-log.md](../../../../docs/research/decision-log.md), entry "Free pricing confirmed for the publishing batch").
- **The Apple price schedule is saved at $0.00** for U.S. base pricing and all 175 listed territories. Apple applies it only when the app is Ready for Sale ([decision-log.md](../../../../docs/research/decision-log.md), "Evergrove Apple free-price schedule configured"). Google Play has no price set.
- **Any nonzero price test reverses or extends that decision.** It needs an explicit user instruction. Nothing in this draft changes the price schedule.
- **Price points are founder decisions.** No candidate price appears in this document. Any figure used in a session must be chosen by the founder and recorded before recruiting.

---

## 3. Prerequisites (all BLOCKING)

| # | Prerequisite | Source | Status today |
| --- | --- | --- | --- |
| P1 | **M4 continue decision.** A premium price presupposes the core experience is worth paying for. M4 must end in an explicit "continue" (not "simplify" or "stop"). | [09 M4](../specs/09-EXECUTION-CONTROL.md#milestone-gates-and-feature-freeze) | BLOCKING. The matched control build is not device-tested; the [M4 preregistration](m4-preregistration-draft.md) is unregistered. |
| P2 | **M5 certified launch-locale subset.** Price and store copy can only be tested in a locale that is certified. | [09 M5](../specs/09-EXECUTION-CONTROL.md#milestone-gates-and-feature-freeze); [VALIDATION](../VALIDATION.md) | BLOCKING. Zero per-locale certification records exist. |
| P3 | **Release candidate without research UI** (`BLD-007`: no experiment controls in consumer build). The current guard is source-level only. | [09](../specs/09-EXECUTION-CONTROL.md); [PRODUCT](../PRODUCT.md) | BLOCKING. Not verified on a shipping binary. |
| P4 | **Signed distribution route and name/brand clearance** (`STORE-001`). The decision log records "trademark clearance remains unverified." | [verification-matrix.json](../specs/verification-matrix.json); [decision-log.md](../../../../docs/research/decision-log.md) | BLOCKING. |
| P5 | **Privacy and SDK audit** (`STORE-002`). | [09 M6](../specs/09-EXECUTION-CONTROL.md) | BLOCKING. |
| P6 | **Zero open S0/S1** on the release candidate. | [09, defect triage](../specs/09-EXECUTION-CONTROL.md) | BLOCKING. |
| P7 | **Entitlement and restore flow designed and QA-tested.** "A store purchase needs a secure way to restore it." | [vision §7](../../vision.md); [07, ethical revenue architecture](../specs/07-VALIDATION-AND-GTM.md#ethical-revenue-architecture) | BLOCKING for any design that reaches a store. |
| P8 | **Price intent validated before billing code.** "Do not add billing architecture before validating price intent." | [07](../specs/07-VALIDATION-AND-GTM.md#ethical-revenue-architecture) | Satisfied by this draft's order: no IAP code until the experiment passes. |
| P9 | **Founder decision: test a nonzero price at all**, given the 2026-11-23 free decision. Record the decision and its date. | [decision-log.md](../../../../docs/research/decision-log.md) | BLOCKING. Not yet recorded. |
| P10 | **Legal review** of the purchase model and of any payment to a participant (design a-1 below). | [PRODUCT](../PRODUCT.md) (naming and rejecter roles); [AGENTS.md](../../../../AGENTS.md) (never automate spending) | BLOCKING for design a-1. Recommended for all designs. |

---

## 4. Hypotheses (predeclare before observing anyone)

Sources give **no numeric price threshold**. The model itself is quoted from [07](../specs/07-VALIDATION-AND-GTM.md#ethical-revenue-architecture): "Preferred hypothesis: paid game or free demo → one-time full unlock; substantial optional expansion packs only after content demand."

- **H1 (directional, primary).** Among adults 45+ who completed chapters 1–3 of a playable build, at least `TO PREDECLARE`% choose the paid one-time unlock over stopping at the free demo, at a price the founder chooses (`TO PREDECLARE`).
- **H0 (primary).** The rate in H1 is not distinguishable from `TO PREDECLARE`% (the founder sets the null level).
- **H2 (rejecter).** At least `TO PREDECLARE`% of eligible participants decline any paid option. Reasons are coded (see section 6).
- **H3 (store, post-launch only).** A free demo + unlock converts better than paid upfront in one storefront. Not testable with 10-30 people; see design b.

Note that 10-30 participants cannot resolve modest differences. Every result is directional ([07](../specs/07-VALIDATION-AND-GTM.md): "Small-sample caveat").

---

## 5. Candidate designs, ranked by honesty and cost

Ranking: honesty first, then cost. **Recommended first design: a-2.**

### (a) Post-session real tradeoff in a separate commercial study

**Two variants.**

- **a-1 (money collected or donated by the facilitator). NOT RECOMMENDED until legal review (P10).** After the session, participants choose between keeping a reward and paying a token amount that the facilitator actually collects or donates. Collecting money from research participants is a payment transaction. It raises tax, refund, card-data, charity-disclosure and coercion questions, especially for adults on fixed incomes. It also creates a second price that is not the product price. Do not run it without written legal and founder approval.
- **a-2 (no money moves). RECOMMENDED FIRST STEP.** After the session, the participant chooses between two real, binding options with no charge: (i) receive a reminder and early-access link to the paid build when it exists, or (ii) receive nothing further. The option price is written as the founder-chosen `TO PREDECLARE` figure, and the disclosure says no charge happens in the study. This is cheap and honest about the commitment, but it is a weak price signal. It cannot mark `STORE-004` passed.

**Sample.** 10-30 total (`VAL-005`). Quotas per age band are `TO PREDECLARE`. The sources give no band quotas. Report bands 45-54, 55-64 and 65+ separately. Do not generalize from one band to all adults 45+ ([AGENTS.md](../../../../AGENTS.md)). If a band is missing, its status is `age fit not validated`, not zero.

**Recruitment.** Explicit recruitment from the 45+ segment. Do not recruit only founder friends or staff ([VALIDATION](../VALIDATION.md), Day 3). The screener must not reveal the price hypothesis ([10](../specs/10-EXPERIMENT-PROTOCOLS.md)).

**Measures (denominator in brackets).**

- Paid-option choice rate: `n/N (x%)`, where N = all starters assigned, reported overall and by band.
- Decline rate: `n/N`, with reason codes for each decline.
- Choice rate among participants who state they decide their own purchases (see section 6): `n/N`.
- No-answer, skipped or abandoned: reported as missing, not as declines.
- Stated "would pay" answers: reported separately and labelled "stated, weak evidence" ([07 metric dictionary](../specs/07-VALIDATION-AND-GTM.md): "Compliments and hypothetical 'would buy' answers are weak evidence").

**Pass / stop.** `TO PREDECLARE` for every threshold. Pre-state the stop condition: if the choice rate among eligible payers is at or below the null level in H0, keep the game free and do not add IAP code.

### (b) Store test: free demo + unlock vs paid upfront, one storefront, after launch

**Honesty.** High. Real purchase decisions by real strangers.

**Cost.** Highest. Requires a live listing, Ready for Sale status, a certified locale (P2), entitlement and restore flow (P7), and enough traffic to say anything. Two different sale models may need two listings or product configurations. **Verify current Apple and Google capabilities before design.** Do not assume that either store offers a native price experiment. Store-native experiments are generally creative or listing tests.

**Limits.**

- Store analytics do not report age. Age-band results are `unknown` unless collected through a consented channel. Do not infer 45+ demand from keyword volume or category data ([AGENTS.md](../../../../AGENTS.md)).
- The minimum traffic for a credible test is `TO PREDECLARE`. Do not claim statistical significance without it ([07](../specs/07-VALIDATION-AND-GTM.md): "no fake statistical significance").
- Changing the live price requires an explicit user instruction (section 2).

**Pass / stop.** `TO PREDECLARE`.

### (c) Fake-door "unlock" screen in a research build, no charge

**Honesty.** Lowest. Participants see a price and an unlock button that does nothing. This deceives adults who may be making a real spending judgment.

**Risks to flag.**

- It is deception of research participants. It requires a debrief immediately after the choice and a written deception review (P10).
- A research build can leak or be screenshotted. Store policy and reviewer trust are at risk even though the build is not a release.
- A fake price creates a false impression of a real offer. It cannot be used as `STORE-004` evidence, because the price was not really offered.

**Recommendation.** Reject by default. Use only with founder and legal approval, a built debrief screen, and no data that identifies the participant.

---

## 6. Payer, player and rejecter in the analysis

From [PRODUCT](../PRODUCT.md) section 2, the three roles are "discovered, not assumed". Apply them as follows.

| Role | Who in this study | Analysis rule |
| --- | --- | --- |
| **Player** | Every participant who plays the matched build. | Denominator for play, comprehension and recall measures. A player is not automatically a payer. |
| **Payer** | The person who would press buy, record `decides own purchases: yes / no / shared`. | Price-choice rate is reported on the eligible-payer denominator as well as on all starters. Shared or family-controlled devices are reported separately. |
| **Rejecter** | Anyone who can block purchase or install (family or device owner, store reviewer, storage or connectivity limits). | Usually outside the sample. Record it as a limitation. A choice rate among players with an unknown rejecter is not a purchase rate. |

**Do not report** the paid-option choice rate as conversion. It is stated intent in a study, not a sale.

---

## 7. Ethics, consent and legal

- **Separate consent for the pricing question.** Consent to play, to notes and to recording is already separate in the [M3 kit](m3-session-kit.md). Add a fourth consent item for the price block, with its own record. Store consent records outside the public repo ([playtest README](README.md)).
- **Plain statement of no charge.** "No money is taken in this study. Nothing is bought. You can decline without any effect on the rest of the session."
- **No pressure.** No urgency, no scarcity wording, no discounts framed as limited, no pre-selected options, no repeated asking after a decline.
- **No dark patterns.** No hidden charges, no auto-renewal, no subscription, no streak or timer tied to paying. These are rejected in [07](../specs/07-VALIDATION-AND-GTM.md#ethical-revenue-architecture) and [vision §7](../../vision.md).
- **Adults 45+.** Use plain language and no age-stereotyped framing ([AGENTS.md](../../../../AGENTS.md)).
- **Platform rules.** Digital unlocks usually need the platform's own purchase system. Verify current Apple and Google rules before any design that reaches a store. Do not assume an external payment route is allowed.
- **No billing SDK or IAP code** until price intent is validated ([07](../specs/07-VALIDATION-AND-GTM.md#ethical-revenue-architecture)). This draft does not authorize any.
- **Price points are founder decisions.** Record each one with a date before recruiting.
- **Nothing charges anyone automatically.** No automatic charge, auto-renew, or pre-filled payment at any stage of any design here.

---

## 8. Required decision record

Each completed study needs one entry in [decision-log.md](../../../../docs/research/decision-log.md) containing:

- date, cohort, age-band quotas and actual counts;
- assignment and order;
- raw counts with denominators `n/N` for every percentage;
- missing, abandoned and declined participants, reported separately;
- the strongest alternative explanation;
- what result would change the decision;
- the next owner and action;
- a stop condition: "keep free; no IAP code" is always an allowed outcome.

---

## 9. First design

**Recommended first design: a-2**, run as a separate commercial study after the M4 decision (P1). It is cheap, requires no money handling and does not need a live listing. It also does not satisfy `STORE-004`, so M6 stays open until a price is really tested (design b, or a legally reviewed a-1).

**Open items for the founder:** (1) whether to test a nonzero price at all (P9); (2) the candidate price points; (3) whether a non-monetary tradeoff satisfies `STORE-004`.
