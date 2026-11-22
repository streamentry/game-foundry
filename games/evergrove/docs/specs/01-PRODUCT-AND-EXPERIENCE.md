# 01 — Product strategy and player experience

**Product thesis (hypothesis, not measured):** Evergrove is an offline, short-session water-pipe puzzle where **solving restores water, choices physically change a small village, people remember those choices, and later puzzles inherit their consequences**. The game wins through clarity, care and consequence, not feature count or simulated infinity.

## Audience and job to be done

- **Primary validation cohort:** adults 45+ who enjoy calm, readable mobile puzzles, alongside casual puzzle players of other ages. Age is a recruitment hypothesis, **not** a restriction or proof of demand.
- **Player job:** enjoy a complete, satisfying 2–5-minute puzzle without pressure; return to a small place that visibly reflects past decisions.
- **Emotional job:** "I solved this. I chose this. The place changed. They remember."
- **Commercial job:** a one-sentence, screenshot-demonstrable reason to choose Evergrove over a generic pipe game, with an affordable founder-led distribution channel.
- **Respect contract:** offline core play; no account, forced ads, timers, energy, lives, streak loss, notifications or mandatory daily waiting. Hints never cost currency or punish progress.

### What would make this genuinely better?

1. **Legible causality** — a player sees the changed water/trees/bridge/flowers **before** reading an explanation.
2. **Excellent puzzles** — fair, teachable, progressively recombined, human-calibrated, and accessible at small phone sizes.
3. **Reliable attachment** — bounded authored NPC memory and a journal that accurately explains choices, without fake intelligence or contradictory events.
4. **Frictionless trust** — robust offline saves, honest localization, readable touch targets, no unwanted data transfer.
5. **Enough meaningful content** — replay or expansions only when the core is proven, never chores to inflate retention.

## The current baseline

See [source map](08-EVIDENCE-AND-DECISIONS.md) and [project README](../../README.md). The repository implements 12 authored 3×3/4×4 boards, 33 deterministic choice-conditioned variants, a fixed ecological landmark, authored reactions from Mira/Orin/Fen, journal/endings, 2D and opt-in 2.5D, offline save/backup/migration and 30 draft locale choices. Current consequences are **limited**: rotating existing topology and anchoring a tile is not a new ecological rule. The 2.5D renderer is an experiment, not a production art commitment. Unity and real-device gates are open.

## The first 60 seconds — proposed storyboard

| Moment | Player sees/does | Design and test intent |
| --- | --- | --- |
| 0–5s | An already-visible **dry stream**, one obvious pipe tile, small unobtrusive settings access. Device language is detected. | Do not require a menu, mode choice, lore screen or account before play. |
| 5–15s | First tap rotates a pipe. Visible port alignment, water reach and optional gentle tactile feedback respond instantly. | Median first meaningful input **<15s** in observed target sessions. |
| 15–45s | Two or three approachable rotations reconnect the spring; visual cues explain flow without paragraphs. | At least **80%** of first-time target testers can explain the goal without researcher instruction. |
| 45–60s (design target) | Water reaches the village; a dry detail visibly revives; one brief line acknowledges it. | Player experiences the complete solve→change promise before being asked to invest in the story. |

**Caveat:** the 60-second completion is a test target, not a universal UX fact; allow slower players to proceed without anxiety or countdown. Preserve manual language/accessibility settings for those who need them.

## First 10 minutes — three complete loops

1. **Stream 1:** solve → water flows → choose shade tree/bridge/meadow → show the visible change first.
2. **Stream 2:** one obvious consequence of the prior choice changes the next board; explain the protected landmark with a visual label and optional help.
3. **Stream 3:** one villager refers to an actual past event; journal can show **Choice → Physical effect → Remembered by → Puzzle effect**.

**Comprehension test:** after ten minutes, ask an open question without supplying the game's words. A target of **≥75% correctly recalling a specific consequence** is a provisional go/no-go signal, not a statistically established benchmark. Watch for players who enjoy pipes but treat village text as an interruption.

**Consumer presentation:** do not put the internal `Puzzle Only` experiment mode, visual renderer switch, language picker, research report controls and journal all ahead of the first meaningful interaction. Keep experiment controls behind a facilitator/debug entry for production testing.

## First seven days — return without compulsion

- Campaign can be finished in one sitting or spread across a week; **never** gate content by wall-clock time.
- On return, show a one-sentence **state-derived recap**, not a fabricated event that occurred while absent.
- An optional short return stream may exist **after** proof of demand; missed days cannot erase rewards, break streaks or expire story.
- Keep a manual/consented researcher follow-up for D1/D7. Existing in-memory playtest counters **cannot** measure return automatically.
- Separate voluntary return from politeness, reminder response and completion of a paid task.

## Long-term design

- Deepen **puzzle ecology**: tree roots, bridge foundations and protected meadow spaces should change reasoning, not just orientation. Later puzzles recombine taught rules.
- Use a solver-backed **authoring assistant** to suggest and verify boards, then require human editorial approval and actual solve-time calibration.
- Offer a new-garden replay with materially different consequences only if the current 12-chapter ending leaves players asking for more.
- Consider one-time substantial chapter expansions after premium viability; never inflate hours with repetitive chores.

## Positioning and commercial hypothesis

**Three truthful store-message candidates:**
1. "Every water puzzle changes your village."
2. "Relaxing offline water puzzles. No timers or ads."
3. "Restore the stream. Grow a world that remembers."

**Suggested first three screenshots:** (1) large readable pipe board with dry→flowing water; (2) tree/bridge/meadow decision; (3) the resulting visible village plus one NPC memory. Each image should communicate value without relying on tiny text.

**Monetization to test, not preapprove:** premium or free demo → one-time full-game unlock; optional substantial chapter packs. No subscription, currency, hint sales, ads or retention pressure. Test willingness to pay with real players and comparable premium titles; the market size of mobile games does not establish Evergrove's demand.

**Brand risk:** the research report identified an unrelated public indie project also named "Evergrove." Perform store and trademark clearance before committing to branding; this observation is not a legal determination.

## Visual decision: 2D versus 2.5D

Keep **2D as safe/default**. Test the existing opt-in fixed-camera 2.5D miniature with identical world histories, text, device class and sound; counterbalance presentation order. Measure unprompted consequence identification, attachment, frame time, memory, battery/heat and art authoring hours. **Do not** attribute higher preference to 3D if the 3D version has more polish or animation. Prefer 2D if clarity is similar or device/art cost is meaningfully higher. See [existing visual protocol](../../../../docs/research/evergrove-visual-comparison-protocol-2026-11-22.md).

## Strongest argument against the concept

**A familiar pipe puzzle plus thin village prose may be worse than a polished puzzle alone.** Twelve chapters could feel short; shallow world branches could feel cosmetic; more characters and systems could make the game harder to understand without improving attachment. The comparison with `Puzzle Only` is therefore a **kill test**, not a promotional demo.

Other failure modes: accessibility fails on compact devices; translations render incorrectly; 2.5D drains performance; players enjoy the game but will not pay; ASO is too competitive; another product already uses the title. No single new feature fixes these.

## Top ten implementation priorities

1. Verified Android/iPhone builds and real first-session observations.
2. Dry stream → immediate touch → flowing water → village reaction in the first minute.
3. Visually explicit consequences **before** explanatory dialogue.
4. Solver metadata and human-calibrated difficulty for all 12 authored boards.
5. Progressive, non-punitive hints.
6. Correct Unicode/RTL/Indic text rendering; certify a smaller launch locale set.
7. Large targets, non-color cues, scaling and accessibility device QA.
8. Ecological choices that genuinely alter future puzzle reasoning.
9. Small, distinct, truthful memories for Mira, Orin and Fen.
10. Subtle sound/haptics and a satisfying before/after world payoff **after** blockers.

Eight of ten improve clarity, feeling or reliability; only the ecology/memory work substantially increases scope. See the [full backlog](02-FEATURE-BACKLOG.md) and [decision thresholds](07-VALIDATION-AND-GTM.md).
