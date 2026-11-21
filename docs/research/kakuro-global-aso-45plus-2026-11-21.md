# Kakuro: ASO-only global test for adults 45+ (2026-11-21)

**Status: conditional first experiment, not an approved production launch or a proven ASO gap.** Start with a U.S. English proof of play for adults 50-75, U.K. as English follow-up, Japan as first potential localization. If the measured search + user-experience gates fail, park Kakuro and test Shikaku. See [first-game decision](first-game-decision-2026-11-21.md).

**Objective:** sustained *non-branded organic App Store installs per founder-hour*, not revenue, total category installs, ad-driven rankings, or generic interest in games. No paid user acquisition, influencers, web SEO, or publisher cross-promotion in the acquisition experiment.

## What is measured and what is not

| Country | Measured Google web-search phrase | Historical GKP monthly band | Apple/Play organic demand and ranks |
| --- | --- | --- | --- |
| U.S. | `kakuro puzzle` | 1K-10K | **Unknown** |
| U.K. | `kakuro` | 1K-10K | **Unknown** |
| Japan | `カックロ` | 1K-10K | **Unknown** |
| Germany, France, South Korea | None reliably measured here | **Unknown** | **Unknown** |

These estimates come from the existing [first-game comparison](first-game-decision-2026-11-21.md): Google Keyword Planner, November 2025-October 2026, with close variants and Google Search network. **They are not App Store search volumes, organic app downloads, or age-45+ demand.** Ratings are not downloads; public catalog match order is not live unsponsored storefront ranking. Do not translate Google's advertiser-competition field into app competition.

The audience evidence in [the 45+ screen](45plus-market-screen-2026-11-21.md) concerns U.S. adults **50+** as a broad group. Neither GKP nor public store listings establish Kakuro's popularity among 45-54, 55-64, or 65+ specifically. Recruit actual players rather than inferring age from reviews.

## Competitor supply recheck: the initially proposed wedge is already claimed

Public listings inspected November 21, 2026 (listing descriptions are publisher claims, not verified hands-on capabilities or keyword positions).

| Market | Direct observable listing | Consequence for the hypothesis |
| --- | --- | --- |
| U.S. | [Conceptis Kakuro: Number Crossword](https://apps.apple.com/us/app/kakuro-number-crossword/id1197071375): about 1.9K public ratings; zoom, pencil marks, combinations, and an *Assistant* offering next-step hints since its March 2025 update. | Do **not** describe hints, zoom, or no ads as novel. Strong professional incumbent. |
| Japan | [Conceptis 加算パズル](https://apps.apple.com/jp/app/id1197071375): about 962 public ratings. The Japanese title uses **加算パズル**, while its description recognizes `カックロ`, `クロスサム` and `足し算クロス`. | Native naming matters. Testing only `カックロ` can miss a separate intent; title choice does **not** prove search volume for `加算パズル`. |
| Japan | [Kakuro Haven](https://apps.apple.com/jp/app/id6800144243) advertises larger digits and zoom. | Larger clues alone are not a unique position. |
| France | [Kakuro++](https://apps.apple.com/fr/app/id1445001139): around 90 ratings, thousands of grids, beginner tutorial and intelligent hints. | The education/basic-hint proposition already exists. |
| France | [Kakuro Grid: Sommes croisées](https://apps.apple.com/fr/app/id6817269735) claims a proof of unique solutions, hints that explain the next logical deduction, and paper-like notes. | **Directly overlaps our proposed explainable-solver wedge.** Audit actual behavior before investing in a solver. |
| Germany | [Kakuro++ Kreuzsummen Rätsel](https://apps.apple.com/de/app/id1445001139): about 110 public ratings; localized Kakuro positioning. | An existing localized incumbent, not an empty market. |
| Germany | [Zenkuro: Kakuro Kreuzsummen](https://apps.apple.com/de/app/id6787019507) advertises calm presentation and freshly generated unique boards. [Kreuzsummen: Kakuro, Calcudoku](https://apps.apple.com/de/app/id6819104770) advertises offline/no-account/no-ad play, but says its menus are English. | Calm, offline, original-grid and ad-free claims are also in the market. English-only menus are a **possible** localization lead, not proven player dissatisfaction. |

**Strongest counterargument:** A small Kakuro search market may already be well served. A better interface can improve store conversion or retention but cannot manufacture search impressions. Recent low-rating indie apps are ambiguous evidence: they may indicate light competition **or negligible demand**.

## The cross-country insight: familiar *local query* first; accessibility as conversion advantage

Kakuro is one reusable number-puzzle mechanic with different linguistic discovery surfaces. For each local query, measure it separately; don't pool synonyms or assume literal translations are commonly searched.

| Storefront | Candidate exact terms to audit (not validated volumes) | Critical caveat |
| --- | --- | --- |
| U.S. / U.K. | `kakuro`, `kakuro puzzle`, `cross sums`, `number crossword` | The last phrase may attract unrelated products; check intent. |
| Japan | `カックロ`, `加算パズル`, `クロスサム`, `足し算クロス` | Conceptis uses the second phrase in its app title; do not treat `ナンプレ` as Kakuro intent. |
| Germany | `Kakuro`, `Kreuzsummen`, `Kreuzsummenrätsel` | A visible new entrant already uses `Kreuzsummen`; measure intent and rank. |
| France | `Kakuro`, `Sommes croisées`, `Nombres fléchés` | Similar number-word descriptions may overlap other puzzle categories. |
| South Korea | `카쿠로`, `크로스섬` | Local terminology and query popularity need native verification. |

Copy this into a measurable work queue: [Kakuro exact-query worksheet](kakuro-aso-keyword-queue-2026-11-21.csv). Blank Apple popularity/rank columns deliberately mean **not measured**, not zero.

**Do not lead with “for seniors” by default.** Age 45+ is a product-research cohort, not necessarily a user-selected search term. Searchers likely recognize the puzzle's name; test accessibility claims as listing conversion and playability differentiators. “Large digits” or “no ads” must be measured as standalone query demand before treating them as acquisition keywords.

## Product concept worth attempting to falsify

**One differentiating hypothesis:** small-screen Kakuro can keep **both intersecting sum clues and the currently relevant candidate logic readable without repeated zoom/pan**, so newer or visually strained players can solve the *same intellectual challenge* with less interface friction.

Proposed test treatment (not yet a proven gap):
- A large, persistent **row/column clue inspector** linked to the selected cell; both clues stay visible without losing board context.
- Tap a clue to see only its still-valid combinations and a **why-this-candidate-is-impossible** explanation, without auto-solving.
- Beginner boards deliberately sized for a phone; reliable original grids with unique, programmatically verified solutions and difficulty grading.
- Adjustable clue size, contrast, touch targets and error feedback; normal adult tone and optional assistance, no age label or therapeutic claim.
- Offline-first, clear progression and no forced timers. These are quality expectations, **not independent differentiators**.

**Do not implement the entire feature list before comparisons.** Produce one short interactive mock and one original board. In side-by-side play, test whether this is materially easier than Conceptis and a newer explainable-hint competitor. Distinctness must be observed, not asserted in store metadata.

## ASO-only research protocol and decision gates

**Days 1-7 — buy information first.**
1. Use authorized Apple Ads Insights to query specific local keywords by country/month; preserve source, API response, query exactness and 1-100 popularity. Absence in a top-K response is **unknown**, not zero. See [research method](README.md).
2. On a real device set to each target storefront, capture ten **unsponsored** rankings for the promising non-branded terms. Also check Google Play individually if Android is in scope. Compare top publisher share, title/subtitle match, recent entrants and current review sample; do not infer rankings from Apple's public catalog API.
3. Sample about 20 **recent, mixed-sentiment reviews** across the top direct competitors. Require repeatable, recent failures in clue legibility or reasoning assistance rather than anecdotal complaints.
4. Observe at least **five** U.S. adults aged 50-75 playing a comparable beginner board in an incumbent and in the small proposed treatment, counterbalanced by order. Count first correct input, clue-reading errors, help requests, first-board completion and explicit preference.

**Prototype gate (pilot heuristic, not statistical proof):** continue only if at least **3/5** participants reproduce a *specific* incumbent problem **and 4/5** complete and understand the treatment's first board, with an observable advantage over the strongest competitor. Confirm distinct non-branded search demand and plausible entry into the real top ten separately. If incumbents already solve the need, **park** and test Shikaku.

**Days 8-30 — only if the gate passes.** Build a one-game, original-grids proof of play; validate uniqueness/logic hints with deterministic tests; perform native Japanese phrase review only after English signal. Run an initial TestFlight/internal Play trial with participants from the intended age bands. Predeclare Day-7 retention and minimum meaningful-play thresholds from a pilot baseline; do not invent benchmarks.

**Days 31-90 — only after an approved store release.** One original product, not localized near-clones. Localize within the same app when behavior is materially the same. Track search-source App Store impressions, page conversion, first-session completion and organic installs by storefront and query where reporting permits. Use **1,000 non-branded organic installs by day 60 after launch** as a *stretch experimental checkpoint*, not an industry expectation; interpret against actual search impressions and the low-volume niche. No paid acquisition in this experiment. Review Day-7 retention by observed cohort, not assumed age.

**Stop condition:** no credible non-branded App Store keyword demand, dominant top-10 positions with no feasible displacement, no repeated unsolved competitor problem, the prototype loses head-to-head, or organic traffic remains too small to plausibly meet the portfolio's installs-per-founder-hour objective. Do not use low review counts or web-search ranges to override those failures.

## Strategic conclusion

Kakuro may be **easier to build than to distribute**. Its small scope is useful for a fast engine-and-ASO experiment, but it is not currently a defensible, ASO-only global gap. **The transferable research method is cross-market keyword arbitrage in culturally familiar paper-puzzle formats**: identify *local terms* with demonstrated store demand, verify whether recent independent apps appear in unsponsored results, and only then test the player experience. [Global puzzle shortlist](global-puzzle-market-priorities-2026-11-21.md) and [research protocol](README.md) remain the broader selection framework.
