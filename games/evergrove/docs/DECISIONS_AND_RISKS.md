# Evergrove — decision record, strongest counterarguments and risk register

**Purpose:** preserve reasoning, not just conclusions. All dates below refer to decisions or code observed as of **2026-11-22**, not to later user validation. “Implemented” means code exists; “verified” identifies actual tests; “desired” is not scheduled work.

**Cross-reference:** [Future feature backlog (37 candidate features)](specs/02-FEATURE-BACKLOG.md) includes illustrative impact scores and potential milestones. These **do not** supersede the validation gates or turn features into existing behavior. Any future feature needs a player-value test before implementation.


## 1. Decisions already made (reversible unless stated)

| Decision | Reason | Strongest challenge / reversal trigger | Record |
| --- | --- | --- | --- |
| Founder picks Evergrove as a long-horizon creative bet (`PH-006`) | Potential overlap of short spatial play and persistent attachment | A simpler puzzle app may have better retention/distribution; selection was not quantified portfolio analysis | [Founder research](../../../docs/research/evergrove-build-decision-2026-11-22.md) |
| Unity 6.3/C# for Evergrove only | User-selected C# / Unity vertical slice; pure logic remains engine-free | Engine upkeep/exports may cost more than Godot; revisit with actual device and iteration evidence | [Unity ADR](../../../docs/decisions/2026-11-22-evergrove-engine-unity-csharp.md) |
| Small authored, offline-first single-player village | More testable, explainable and private than universal live AI | Bounded experience may lack sufficient repeat value | [Architecture](ARCHITECTURE.md) |
| 12 original authored stages, 3×3→4×4 | More content and gradual challenge than original six 3×3 fixtures | Still limited content; difficulty is not yet user-tested | [Gameplay](GAMEPLAY_AND_WORLD_RULES.md) |
| Trees/crossings/meadows branch the next water route | Makes choices affect mechanics rather than only prose/decor | 90° symmetry and one fixed tile may be perceived as superficial | [PR #42](https://github.com/streamentry/game-foundry/pull/42) |
| 2D art baseline, optional 2.5D miniature | Show small persistent world from different vantage points | 2.5D may add GPU and art costs without attachment gain | [Visual ADR](../../../docs/decisions/2026-11-22-evergrove-visual-direction-2p5d.md), [PR #38](https://github.com/streamentry/game-foundry/pull/38) |
| Bounded NPC memory with authored dialogue | Narrative is consistent, reproducible and privacy-protecting | Some people may want richer spontaneous agency; first test whether three characters suffice | [Player experience](PLAYER_EXPERIENCE.md) |
| Save schema v4 and v1/v2/v3 migration | No player should lose a garden when routing evolves | Rollback compatibility and disk errors need real-platform QA | [Save spec](SAVE_AND_MIGRATION.md) |
| 30 selectable draft languages | Broadens global usability hypothesis without network translation | Unreviewed copy and RTL/Indic shaping make broad release unsafe | [Localization](../LOCALIZATION.md) |
| Consent-first local playtest notes only | Learn with minimal personal data | Can't measure D1/D7 cohorts without separate consent/process | [Validation](VALIDATION.md) |
| Cloudflare prototype not attached to gameplay | Avoids identity, security, latency and moderation before proof of need | Cloud sync may help portability; require separate ADR | [Mosselora Worker backend](../backend/README.md) |

## 2. Strongest opposing case

**Evergrove may be overbuilt around a weak primary puzzle.** Players could interpret v3 variants as the same 12 boards rotated, the 2.5D miniature as basic primitive art, and NPC memory as canned text. These additions raise development cost without adding meaningful strategic decisions, a durable emotional bond or scalable content. Mobile users might prefer a perfectly executed familiar puzzle with immaculate graphics, or a true cozy simulator with deeper interactions. Even if current players enjoy it, discoverability and user acquisition may fail.

**What would overturn this objection?** An independent cohort identifies the earlier choice's effect without prompting, willingly starts another chapter to care for the village, returns after the session without chasing a reward, and expresses a clear preference over a well-made alternative. Real retention and install intent would outweigh speculative claims about “AI living worlds.”

## 3. Risk register (priority, signal and mitigation)

| Severity | Risk | Early observable signal | Cheapest mitigation / hard stop |
| --- | --- | --- | --- |
| **Critical** | Unity player cannot be built or input fails | Editor import errors, tapping has no effect | Run licensed Unity smoke first; do not add content until resolved |
| **Critical** | Save corruption or silent progress loss | Force-kill/disk-full/restart loses a choice | Repro on phone, fix player-visible saving errors; test backup/rollback |
| **Critical** | Branching not meaningful | Users cannot explain why the next board changed; regard it as a rotated duplicate | Test a genuine cost/resource dependency or revert to simpler puzzle |
| **High** | Confounded experiment | “Living is better” inferred from different board difficulty | Add matched Living control and split narrative vs mechanics vs art tests |
| **High** | Name collision and rights | Existing similarly named live app; uncertain app-store/trademark rights | Keep working name internal, conduct formal clearance and rename before launch |
| **High** | Poor usability for 45+ | Tiny/hidden first tile, confusion about fixed tile, scrolling burden | Uncoached 45+ device tests, larger targets and explicit labels |
| **High** | Localization looks broken | RTL/Indic reordered glyphs, clipped buttons | Advanced shaping and native device QA; disable affected storefront languages |
| **High** | No credible distribution | Only acquaintances try game; no search/channel intent | 10–30 focused outreach and one-market keyword/channel proof |
| **High** | Costly 2.5D with no benefits | Lower FPS/more heat, players prefer 2D | Keep 2D baseline; remove 2.5D if no evidence of higher value |
| **Medium** | Bounded content causes churn | Players finish 12 chapters and do not return | Test preference before building content factory; avoid faux “infinite AI” |
| **Medium** | NPC memories don't feel personal | Reactions seem generic despite choices | Improve authored specificity after observing actual confusion |
| **Medium** | Optional cloud harms trust | Network failures, secret in PlayerPrefs, sync conflicts | Keep off until explicit identity/security/product ADR |
| **Medium** | Overcounting locale readiness | 29 catalogs pass static tests, no reviews | Require native sign-off per launch language |
| **Medium** | Vanity analytics | Reported “engagement” is facilitator-driven | Use voluntary unprompted return; show denominator and refusals |

Risk severity is a **decision heuristic**, not a measured incident rate.

## 4. Three load-bearing assumptions

1. **Core is intrinsically satisfying** in 1–3 minutes without external rewards. Test: first-play comprehension, unaided solve, choose-to-continue; stop if controls are confusing.
2. **Persistent causal link is perceptible and valued.** Test: altered next board with matched alternative and unaided explanation. If people only notice wallpaper, simplify or redesign consequences.
3. **At least one viable distribution path exists.** Test: targeted outreach/keyword-to-play conversion in a single storefront/language; stop speculative production if no credible acquisition wedge emerges.

## 5. What not to build now

- Autonomous LLM NPCs, open-ended chats, background online economies or agent-based civilizational simulation.
- Full multiplayer/UGC/creator marketplace, behavioral targeting, battle pass, manipulative streaks.
- Thirty-language marketing launch, store submissions, paid UA campaigns without proven phone UX.
- Cloud sync merely because a reference Worker exists.
- Another large deck of concept lists, architecture rewrites or speculative features instead of a real player session.

These are **prioritization constraints**, not claims such capabilities are impossible forever.

## 6. One inexpensive falsification sequence

1. Produce a real Unity build (device first).
2. Observe first 10 uncoached players; log every UI misunderstanding.
3. Let them encounter variant branches; ask them to explain changes without hints.
4. Have an actually polished rival title available for a direct preference check.
5. Record next-day voluntary return for consented participants.
6. Publish a short decision with **observed numerator/denominator**, alternative explanations, cost, and the smallest next irreversible commitment.

**Continue:** meaningful, reproducible curiosity about the village and a plausible distribution wedge. **Simplify:** puzzle works but world adds friction or complexity. **Stop:** no compelling player value or no viable acquisition path after a fair cheap test.

## 7. Change-management handoff

When someone changes gameplay, request both a code PR and a doc update:
- New rule → [GAMEPLAY_AND_WORLD_RULES](GAMEPLAY_AND_WORLD_RULES.md), test for uniqueness, variant restoration.
- New save field → [SAVE_AND_MIGRATION](SAVE_AND_MIGRATION.md), migration matrix and corrupted snapshot tests.
- New NPC event → [PLAYER_EXPERIENCE](PLAYER_EXPERIENCE.md), narrative consistency, translation keys, no fabricated memory.
- New platform/SDK → [ARCHITECTURE](ARCHITECTURE.md), privacy, licensing, CI build and store disclosures.
- New research claim → [VALIDATION](VALIDATION.md), direct source or raw observation, threshold, uncertainty and decision log.
- Ship candidate → [BUILD_AND_QA](BUILD_AND_QA.md), real devices, screenshots, full sign-offs.

The north star does **not** change: *a small place that remembers honest choices*. But the implementation, engine, visual style, name and business model must remain falsifiable and reversible.

## 2026-11-23 — Mosselora brand and living-garden visual-system implementation

**Decision:** Adopt **Mosselora** as a *working, player-visible name* and apply the [design system](../design.md) to Unity first/later puzzle boards, choice, memory, storybook/diorama color direction, and the offline first-minute web research slice. Preserve existing save keys, namespace, package IDs, resource paths and `games/evergrove` folder. Do not announce trademark clearance.

**User value hypothesis:** one cohesive, readable living garden communicates water and consequence more quickly, while equal-weight ecological choices reduce a presentation-induced bias. The art should feel warm without competing with puzzle legibility.

**Strongest opposing case:** more illustration and unfamiliar leak marks could reduce recognition; a polished UI cannot redeem a shallow 12-board loop, unverified Unity import or absent distribution. A broad rebrand also risks trademark conflict and localization work.

**Evidence:** browser DOM/unit tests, real headless Chromium viewports and pure C# project assertions can check functional/source contracts after CI. They cannot establish native rendering, genuine delight, player understanding, retention or legal clearance. Brand art from conversation is *not* yet a shipped licensed icon asset.

**Next gate / reversal:** licensed Unity preflight and physical device checks, then 8–12 uncoached players (oversampling 45+). Observe first action, open-join comprehension, restoration, choice consequence and voluntary next chapter. If 3/10 starters misunderstand the goal or miss restoration, prioritize undoing decorative changes. If the mark is risky, revert public branding without touching legacy save identity.
