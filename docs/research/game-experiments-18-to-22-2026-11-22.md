# Game Foundry prototypes 18–22: five distinct deduction tests

**Decision date:** 2026-11-22. **Status:** completed engineering prototypes, not a market-selection result. User requested five additional games; all 100 backlog ideas are still hypotheses. Source-of-truth strategy: search-led Kakuro first only **after** existing App Store and comparative player gates.

| Game | Existing query hypothesis | Researchable value | Strongest countercase |
| --- | --- | --- | --- |
| [Numberlink Paths](../../games/numberlink-paths/README.md) | GF-014 — Numberlink | Can letter-first, large-tap path assignment beat precise drag gestures for short play? | Full-board coverage is a variant of Numberlink; tap-to-fill squares may be slower and less tactile than drawing lines. |
| [Kropki Pairs](../../games/kropki-pairs/README.md) | GF-020 — Kropki | Do explicit directional dot labels make ratio/consecutive reasoning more legible than tiny colored dots? | This beginner positive-dot-only variant simplifies classic full Kropki, and dense clue labels can crowd a small phone. |
| [Ripple Rooms](../../games/ripple-rooms/README.md) | GF-022 — Ripple Effect | Can room ID and size labels make spatial number-spacing deductions clear without zoom? | An additional room label on every tile creates visual noise and may not be needed after the first few puzzles. |
| [Battleship Harbor](../../games/battleship-harbor/README.md) | GF-026 — Battleships logic | Can exact fleet rules and optional water notes make a no-guess logic game satisfying in brief sessions? | There are mature fleet-logic products; row/col counters alone establish no competitive preference. |
| [Kuromasu Windows](../../games/kuromasu-windows/README.md) | GF-028 — Kuromasu / Kurodoko | Do visible white/black labels clarify sight-line deduction more than abstract shading? | A full white/black/unknown mark per square adds tapping and could be tiring; readability must be tested on phones. |

## Truth of the mechanics

- **Numberlink** uses **two number pairs and mandatory full-cell coverage**, which is an explicitly announced variant of classic Numberlink. It validates each path as one connected, nonbranching orthogonal line, endpoint degrees one, intermediate degrees two and no unoccupied cells.
- **Kropki** uses only positive W (= consecutive) and B (= double) clues. **No dot does not imply a prohibition**; this differs from some full-dot conventions. An explicit, readable arrow and letter identify dot types.
- **Ripple Effect** follows [Nikoli's rules](https://www.nikoli.co.jp/en/puzzles/ripple_effect/): each room contains 1 through its size, and two identical k values in a row or column must have **at least k intervening squares**.
- **Battleships** is a fully specified 5×5 beginner fleet of lengths **3, 2, 1**. Distinct ships cannot touch even diagonally, must be straight, and match row/column totals. Marking empty water is optional; no unmeasured battleship fleet conventions are assumed.
- **Kuromasu** follows the [Kurodoko rules](https://www.nikoli.co.jp/en/puzzles/kurodoko/): numbered whites see a total of white squares in their row and column including themselves; black squares do not share an edge; white cells must remain connected.

Six original puzzle fixtures per project are validated by a pure rules solver for **one distinct legal arrangement**, not compared to a hidden reference at runtime. Three automated suites per game cover rules, local save/corruption and a real scene/controller playthrough. Each project has a separate Godot 4.7.2 identity, local-only saved state, Undo/Restart, and a downloadable WebGL2 artifact verified for HTML, JavaScript, WASM and PCK loading over localhost. A local artifact is **not** a publicly accessible game website.

## Distribution, experiment, and stop gates

**Primary unknown:** every title's actual player preference versus established incumbents, age-45+ fit, organic non-branded storefront search intent, retention, accessibility on real phones, willingness to pay, and sustainable differentiation. Store listing anchors in the research catalog are not cohort, revenue, search-volume or user-intent data.

**Cheapest experiment:** recruit 6–10 relevant puzzle players per shortlisted mechanic across 45–54, 55–64 and 65+ when possible; counterbalance incumbent/game order; watch an unaided first move, record error types, ask for no applause or social-proof feedback, note **voluntary** extra puzzles and unsponsored next-day return. Record numerator/denominator, version, device and prior genre familiarity. Compare actual choice and behavior, not the fact that test scripts pass.

**Proposed decision rule (not a benchmark):** after one UX correction, only develop a game beyond its small fixture library if at least three independent users voluntarily play additional boards and at least one concrete repeatable advantage versus an incumbent emerges. Otherwise park and document what falsified it. Do not flood app stores with look-alike packages. The 22-game count is inventory, not market traction.

**One highest-value next action:** put a privacy-reviewed and user-approved small browser playtest at stable HTTPS addresses for target phones, then actually observe players; until then the public discoverability and retention hypothesis is unsupported. No hosting, accounts, ads, payment collection, analytics SDKs, iOS signing or store releases are included in this PR.
