# Companion week kit: do the neighbours feel like they notice?

**What this is:** the player check the [smart companion spec](../specs/proposals/smart-companion-2026-11-27.md) ends with (step 7). It asks one question: after a week, do adults 40+ remember what the neighbours said, find it true to their garden, and find it fresh rather than repetitive?

**Nothing here is evidence until sessions are run.** Thresholds marked `TO PREDECLARE` must be fixed in writing, in [decision-log.md](../../../../docs/research/decision-log.md), before the first participant is recruited. Privacy rules from the [packet README](README.md) apply: participant codes only, nothing identifying in the repository, and no recordings of children.

## The claim being tested

The spec's assumption, stated plainly so it can fail: *players feel "noticed and remembered" by true, varied, pre-written remarks, without free conversation.* The companion never claims to improve wellbeing, memory or connection, and neither may this study.

## Who

- 5 to 10 adults who already play casual games on their own phone, split across bands **40–49, 50–64 and 65+**. Report each band separately; never pool to "40+".
- Optional: 2 or 3 families with a child aged 7+ who plays alongside, under the [family co-play protocol](family-coplay-session.md) (consent, no child data). Children are co-players, never interviewed alone.

## Build

- A development build from `main` with SC01–SC06 merged, on the participant's own iPhone (TestFlight or a cable install). Record the commit hash.
- English only. Other languages keep their story lines, unchanged, so they can't test the companion.
- The research diary on, as in the [measurement decision](../specs/proposals/measurement-decision-2026-11-24.md): local open dates only.
- How the companion chooses and words its remarks, with every constant: [companion architecture](../specs/companion-architecture.md).
- What a neighbour said is kept only on the phone (`evergrove:companion-heard:<garden>` in PlayerPrefs). No build exports it yet (see Limits).

## Day 1: a 20-minute session, observed, uncoached

Say only: "This is a garden game; play as you would at home." Record each item as yes, no or not reached, with time where marked.

| # | Observe | What it tests |
| --- | --- | --- |
| C1-1 | Taps a neighbour without being told to | that the neighbours invite a tap |
| C1-2 | Reads the line aloud or to themselves; asked "Is that about your garden?", says yes and why | truth (SC01) |
| C1-3 | Asked "Who said that?", names the neighbour, or says what kind of person they seem | voice (SC03) |
| C1-4 | Opens the cottage; asked "What's waiting for you?", answers from the card | cottage order (SC05) |
| C1-5 | Notices the basket's ribbon when a cutting waits; asked "What does that mean?" | the soft mark (SC05) |
| C1-6 | Any moment of confusion, delight or unease about a line (verbatim, timed) | all |

## Days 2 to 7: at home, unobserved

- No reminders from the team. The phone's notes are whatever the player chose.
- Nothing is asked during the week, so recall isn't primed.

## Day 7: a 15-minute call

Ask in this order and write the answers down verbatim. Don't show the game until question 5.

1. "What did the neighbours say this week? Tell me any you remember, in your own words or theirs."
2. For each remark recalled: "Who said it?" and "Was it true of your garden?"
3. "Did any of them repeat themselves? Which?"
4. "Did anything they said feel like pressure, or like being watched?" (for example the hour you usually come, or a letter waiting)
5. Show the old-versus-new card (below) and ask: "Which of each pair would you rather hear?"
6. "What would you change about the neighbours?"

**Old-versus-new card.** Print five pairs, in mixed order. Each pair has one old fixed sentence (the pre-SC03 wording, from `CompanionSpeech.Legacy`) and one bank line for the same fact, for example:

- "You freed the water under one of your things. It is drinking deeply." against "I saw what you did with the water. Patient hands. My grandmother would have liked you."

Don't say which is which.

## Results, by band, with denominators

Use [results-template.md](results-template.md). Count a remark as **recalled** when the participant gives its gist and its speaker, or gist plus a detail that matches a fact kind true of their garden.

| Question | Suggested threshold (argue before recruiting) | The spec's stop rule |
| --- | --- | --- |
| Recalls at least one remark (Q1) | `TO PREDECLARE` (suggest 7 of 10) | testers can't recall any remark |
| Recalled remarks judged true (Q2) | `TO PREDECLARE` (suggest at least 8 in 10 recalled remarks) | (a false remark is a bug: log it and fix the fact) |
| Calls the neighbours repetitive within the week (Q3) | `TO PREDECLARE` (suggest at most 2 of 10) | testers call them repetitive within the week |
| Prefers the new line in at least 3 of 5 pairs (Q5) | `TO PREDECLARE` | testers prefer the old lines |
| Any report of pressure or surveillance (Q4) | none | any report: remove or soften that kind (the hour habit first, as run 3's stop rule says) |

**Stop or pivot** if any of the spec's three stop rules hold for most of a band: no recall, repetitive within a week, or the old lines preferred. Record the result and what changed in [decision-log.md](../../../../docs/research/decision-log.md).

## Limits, said plainly

- **Recall can't yet be checked against what was actually said.** The heard-list lives only in PlayerPrefs on the phone. A dev-only "what the neighbours said this week" export, with the participant's consent and nothing leaving the phone without it, is a follow-up. Until then, judge recall by the fact kinds true of that garden.
- **A waiting thing takes that neighbour's remark on every visit until it's acted on** (a cutting in the basket, an unread letter). The spec intends this, but it may read as nagging. Q3 and Q4 are where it would show.
- One week can't show whether variety lasts. The bank has 20 wordings per kind and voice, and the 14-day rule means a remark heard daily won't repeat its wording within the study week.
- `RainComing` (v1, checklist 48) speaks one fixed sentence until its drafted lines are reviewed.

**Status:** not run. `age fit not validated`.
