# Mosselora — The Hundred-Year Garden

> **Status (2026-11-23):** founder-directed long-range vision. **This is an ambition, not a forecast, a roadmap commitment or a production approval.** Nothing here is implemented unless the [as-built handbook](docs/README.md) says so. The 100-million-player goal is a **north-star target chosen by the founder**. It is not derived from market data, and no evidence yet shows Mosselora can reach it.
>
> **Precedence:** code and passing tests describe what exists. [PRODUCT.md](docs/PRODUCT.md) and the [specs](docs/specs/README.md) describe what we are testing next. This file describes **where we want to end up**, and the evidence that has to accumulate before each step is allowed. If this file conflicts with an evidence gate, the gate wins.

---

## 1. The north star

**Mosselora becomes the garden a hundred million people tend, and that some of them hand to their grandchildren.**

Two goals, held together:

| Goal | What it means | What it does **not** mean |
| --- | --- | --- |
| **A century masterpiece** | A game that is still worth playing, still runs and still feels respectful in 2126. Its rules are learned in seconds and can be enjoyed for decades. | Prestige graphics, a famous IP, or a feature list that dates the game within five years. |
| **100 million players** | 100 million unique people who **chose to play** across Mosselora's lifetime, on every platform and in every language. Lifetime reach, not concurrent or monthly users. | Paid installs bought for a chart rank, inflated "downloads", or growth driven by compulsion loops. |

Mosselora only counts the second goal if it keeps the first. A hundred million players gained through streak anxiety, loot boxes or dark patterns would be a failure by this document's standard.

---

## 2. What makes a game last a century

The games still played after centuries are things like Go, chess, mahjong, solitaire and crosswords. They share some properties. Mosselora's design has to hold to all of them:

1. **The rule fits in one sentence.** *"Turn the tiles so the water reaches the garden."* Any child or great-grandparent can understand it without words.
2. **Depth comes from the system, not from content volume.** Water, roots, seasons and neighbors combine into situations no designer wrote by hand, yet every one can be checked and solved.
3. **No dependence on a fashion, a platform or a server.** The core game runs offline on a cheap phone. If the company disappears, saved gardens must still open.
4. **Respect is permanent; manipulation dates badly.** Timers, energy meters and guilt notifications age into embarrassment. Calm, fair play does not.
5. **It belongs to people, not to us.** Players own their garden, its history and anything they make. Rituals form around it: a morning puzzle, a shared seed with a friend, a garden passed down.
6. **Beauty that reads at every age.** Large, high-contrast and readable. We design for adults 45+ first (the studio's [primary audience](../../AGENTS.md)) because a game that is easy to read and touch for them works for nearly everyone. This is the "curb-cut" bet, and it still needs testing.

**Check for every future feature:** *Will this still make sense, still run and still feel kind in 50 years?* If not, it does not belong in the core game.

---

## 3. The end-state experience

This is the full Mosselora we are reaching for. Each layer grows out of the one shipped today. None of it is built.

### 3.1 The verb never changes: guide the water

The water puzzle that exists today (rotate tiles, connect the spring to the garden) stays the heart of the game forever. Every new system has to feed back into *where water can go next*. If a feature can't change a future puzzle, it's decoration.

### 3.2 A garden that actually lives

- **A deterministic ecology.** Water, soil, shade, pollinators and seasons interact through small, readable rules, so they can be tested, never as a black box. Trees you planted ten years ago shade today's channels. A flooded meadow becomes a marsh with new tiles.
- **Seasons and slow time.** The garden changes through spring floods, summer drought, autumn leaf-dams and frozen winter channels. It rewards coming back without punishing absence. A garden left alone for a year grows wild; it never dies.
- **Generations.** A garden can be finished, archived as a living storybook and **passed on**. A child or friend can start a new garden downstream of yours and inherit your old trees and bridges as features of their landscape.

### 3.3 Neighbors who remember

- Mira, Orin, Fen and future villagers keep **written, bounded memories** of what you did, shown honestly and never made up.
- **Companions you care for.** Over time, villagers ask for help, have their own small goals, grow older and leave things behind for you. This is where the original "AI companion" idea returns: **as authored characters who respond to real world state**.
- **Generative AI only if it earns its place.** Runtime language models may add variety to *phrasing* only after controlled tests show players prefer them to authored lines. They must never invent facts about the player's history, require a connection to play, or imitate a real relationship. ([Current rule](docs/specs/08-EVIDENCE-AND-DECISIONS.md): not authorized.)

### 3.4 The Commons: one river, many gardens

This is where the original "trade across a global player network" idea returns, redesigned to fit a century-long game:

- **Downstream gifts, not markets.** Players share seeds, cuttings and puzzle "springs" with friends and strangers. Water you release upstream shows up as a gift in someone else's garden.
- **No speculation and no real-money economy.** There are no auction houses, tradable currencies or scarcity sold for profit. Giving is the social act, as with seed libraries.
- **Async, small and safe.** No chat with strangers by default, no rankings that shame, no pressure to log in for others. Social play is opt-in and visible, and families and close friends come first.
- **Offline is still complete.** The Commons is something you can choose to take part in. A player who never connects still has the whole game.
- **Cloud sync is a convenience you choose.** Gardens can move between devices and survive a lost phone, using an **open, documented save format** players can export.

### 3.5 Players as gardeners of the game

- **A puzzle-making tool.** Players build watercourses; the solver checks that each one has exactly one solution and is fair before anyone can share it.
- **Curated, not flooded.** Shared gardens and puzzles come with moderation and attribution, and creators keep ownership.

### 3.6 Everywhere, for everyone

- Wordless core puzzles that work across cultures. Start from today's 30 language catalogs and grow toward every widely spoken language, with native-speaker review.
- Adjustable text size, contrast, motion, pace and hints. One-hand portrait play. Fully playable without sound or color.
- Mobile first. Tablet, desktop, TV and future devices only once the mobile game has proven itself.

---

## 4. How the 100-million goal is earned: the growth ladder

Every step needs **measured evidence** before the next one is funded. Each rung has a stop rule. These figures are **targets we set ourselves, not benchmarks or forecasts**. Retention thresholds have to be written down in [VALIDATION.md](docs/VALIDATION.md) **before** each launch, never fitted to the results afterwards.

| Stage | Lifetime players | What must be true before advancing | Unlocks | Stop or pivot if |
| --- | --- | --- | --- | --- |
| **Seed** *(now)* | Playtests | M1–M6 gates pass: Unity/device build, adults 45+ understand the water and notice the consequence without help ([execution control](docs/specs/09-EXECUTION-CONTROL.md)) | First store release | Players don't understand the rule or don't care about the consequence |
| **Sprout** | 1,000 organic | The studio's experiment goal of 1,000 non-branded organic installs in 60 days; Day-7 retention meets the threshold set before launch; reviews mention the garden, not just the puzzle | Seasons, more chapters | Retention below threshold after two honest iterations |
| **Sapling** | 100,000 | Players return without being pushed; a fair business model is proven with real pricing tests; crash-free sessions and accessibility hold at scale | Deterministic ecology v1, generations | Growth depends on paid ads, or the business model needs coercion |
| **Grove** | 1–10 million | Word of mouth is measurable (shares, gifts, invites); a second-language market works on its own merits | The Commons (gifts between friends), optional cloud sync | Social features reduce retention or bring safety incidents we can't moderate |
| **Forest** | 10–50 million | Long-term players (one year or more) form a measurable group; localized storefronts sustain organic discovery | Puzzle creation tools, wider platforms | Content can't keep up without lowering quality or fairness |
| **Hundred-Year Garden** | 100 million | Years of fair, profitable, respectful operation; gardens passed between generations | Open save format and permanent archive commitments | — |

**What is already true:** the Seed stage is unfinished. No player has completed a native build. Everything above the first row is aspiration.

---

## 5. Business model that a century allows

The business model is **undecided** ([PRODUCT.md §6](docs/PRODUCT.md)). Whatever we choose has to pass this test: *Would we be comfortable if a player's grandchild read how we made money?*

- **Candidates to test:** free first chapters with a one-time unlock; paid garden expansions and seasons; cosmetic seed and lantern collections; an optional supporter tier that never gates gameplay.
- **Never:** loot boxes, pay-to-skip energy, pay-to-win Commons trades, selling player data, ads interrupting a puzzle, or deliberately frustrating difficulty to sell hints.

---

## 6. Permanent anti-goals

These protect the century goal from the 100-million goal:

- No streaks, countdowns, energy meters, guilt notifications or fake urgency.
- No claims that Mosselora improves memory, cognition or health without proper evidence.
- No infantilizing "senior game" look. Accessible never means shallow.
- No copied art, levels, sounds or trade dress from other games.
- No required account, connection or server for the single-player game.
- No AI-generated memories presented as things the player did.
- No growth number accepted at the cost of player trust.

---

## 7. The strongest case against this vision

To be honest about it:

- **Base rate.** Very few mobile games ever reach 100 million lifetime players, and most that do rely on a large ad budget, an existing brand or competitive social play. Mosselora currently has none of these.
- **Niche risk.** Calm puzzles plus a light story may appeal to a loyal but small audience. That can still be a good business, just not a hundred-million one.
- **Simulation risk.** Players may like the art and the writing more than the systems. Then the ecology and Commons layers would add cost without value.
- **Focus risk.** A big vision can pull effort away from the next real test. That is why every layer above is locked behind evidence.

**What would change our mind:** if the Sprout and Sapling stages show strong retention but little sharing, Mosselora should aim to be an excellent premium puzzle game for a dedicated audience, and drop the Commons and the 100-million target without regret.

---

## 8. How to use this document

- **Use it to break ties.** When two options pass the evidence gates, choose the one that lasts longer, is kinder and works offline.
- **Don't use it as permission.** No layer in §3 may be built until its rung in §4 is reached and recorded in the [decision log](../../docs/research/decision-log.md).
- **Revisit it** after each stage gate. Update this file with what the evidence changed, including if the evidence says the vision itself is wrong.

*A small place changes because you were there. The vision is that this stays true for a hundred years and for a hundred million people.*
