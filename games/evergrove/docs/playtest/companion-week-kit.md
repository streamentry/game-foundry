# Companion week kit: do the neighbours feel like they notice?

**What this is:** the player check the [smart companion spec](../specs/proposals/smart-companion-2026-11-27.md) ends with (step 7). It asks one question: after a week, do adults 40+ remember what the neighbours said, find it true to their garden, and find it fresh rather than repetitive?

**Nothing here is evidence until sessions are run.** Thresholds marked `TO PREDECLARE` must be fixed in writing, in [decision-log.md](../../../../docs/research/decision-log.md), before the first participant is recruited. Privacy rules from the [packet README](README.md) apply: participant codes only, nothing identifying in the repository, and no recordings of children.

## The claim being tested

The spec's assumption, stated plainly so it can fail: *players feel "noticed and remembered" by true, varied, pre-written remarks, without free conversation.* The companion never claims to improve wellbeing, memory or connection, and neither may this study.

## Who

- 5 to 10 adults who already play casual games on their own phone, split across bands **40–49, 50–64 and 65+**. Report each band separately; never pool to "40+".
- Optional: 2 or 3 families with a child aged 7+ who plays alongside, under the [family co-play protocol](family-coplay-session.md) (consent, no child data). Children are co-players, never interviewed alone.

## Pre-checks before recruiting

Do these as a team, once, before the first participant. Record the outcome in the decision log with the other `TO PREDECLARE` items. They are not yet run for this kit.

1. **Read the sample transcript aloud.** [sample-transcript.md](../../../../docs/improvements/companion-plus/sample-transcript.md) (CP46) is one scripted garden over 30 days, 188 lines, in context. Read it aloud as a team and note every line that would feel like pressure or being watched, or that reads wrongly, with its day number. Fix wording through the [writing guide](../specs/companion-writing-guide.md) and the checker, not by editing the kit.
2. **Run the build's QA tour and read the why-log.** The dev tour prints `MOSSELORA_COMPANION_WHY` lines: for each neighbour and day, the top three candidate facts and why each was or wasn't chosen (kind, score, base, new, own, nag, cooldown, eligible or the reason). Use it to check that a waiting fact that has been nagging drops out of the top slot. The tour is a separate run (CP50); this kit does not count it as done.

## Build

- A development build from `feat/mosselora-companion-50` (CP01–CP48 and SC01–SC07 included), on the participant's own iPhone (TestFlight or a cable install). SC04 and earlier are already on `main`; the companion branch is not yet merged, so build from the branch or from `main` after the merge.
- **Build commit:** `3c6576fd` (the last code-bearing commit when this kit was updated; the CP49 commit is docs only). Re-check the hash at build time and record the exact one here: `_to record at build_`.
- English only. Other languages keep their story lines, unchanged, so they can't test the companion.
- The research diary on, as in the [measurement decision](../specs/proposals/measurement-decision-2026-11-24.md): local open dates only.
- How the companion chooses and words its remarks, with every constant: [companion architecture](../specs/companion-architecture.md).
- What a neighbour said is kept only on the phone (`evergrove:companion-heard:<garden>` in PlayerPrefs). A development-only control copies a readable summary to the clipboard (see Day 7, step 4a). Nothing else exports it.

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
4a. **Recall check, only with consent.** Ask: "Would you like to check what the phone recorded? You can say no, and we'll skip this." If yes, the participant (not the researcher) opens the development build's research controls, taps **Copy what the neighbours said**, and pastes the text into the session notes under the participant code. Nothing else leaves the phone: the control only fills the clipboard. Do not show the text to the participant before question 5. The control is hidden while the facilitator's participant view is on, so end that view first (holding the wordmark brings the facilitator controls back, and the participant view ends from there) before handing over the phone. Not yet tested on a device.
5. Show the old-versus-new card (below) and ask: "Which of each pair would you rather hear?"
6. "What would you change about the neighbours?"

**Using the check (step 4a).** The pasted text has one line per remark heard: date, neighbour and kind, under a header. Match each remark the participant recalled in Q1 and Q2 against it: the same neighbour, a date in the week, and a kind that is true of their garden. A match supports recall of the gist and speaker. It does not check wording, and it isn't proof of reading: the phone records each remark when it is shown, not whether it was read. Record matches and mismatches in the results table, not in the repository.

**By design, it shows kinds, not exact words.** A kind is the internal fact type (for example `RainComing` or `NewArrival`), and a neighbour's name is one of the three characters (Mira, Orin or Fen; "someone" if unknown). The player's own name is not in it, and neither are the lines. So the check can confirm gist and speaker, never the exact wording. Keep the pasted text with the session notes, not in git.

**Old-versus-new card.** Print five pairs, in mixed order. Each pair has one old fixed sentence (the pre-SC03 wording, from `CompanionSpeech.Legacy`) and one bank line for the same fact, for example:

- "You freed the water under one of your things. It is drinking deeply." against "I saw what you did with the water. Patient hands. My grandmother would have liked you."

Don't say which is which.

## Results, by band, with denominators

Use [results-template.md](results-template.md). Count a remark as **recalled** when the participant gives its gist and its speaker, or gist plus a detail that matches a fact kind true of their garden. Where step 4a was done, its match can support the count; it never replaces the participant's own recall.

| Question | Suggested threshold (argue before recruiting) | The spec's stop rule |
| --- | --- | --- |
| Recalls at least one remark (Q1) | `TO PREDECLARE` (suggest 7 of 10) | testers can't recall any remark |
| Recalled remarks judged true (Q2) | `TO PREDECLARE` (suggest at least 8 in 10 recalled remarks) | (a false remark is a bug: log it and fix the fact) |
| Calls the neighbours repetitive within the week (Q3) | `TO PREDECLARE` (suggest at most 2 of 10) | testers call them repetitive within the week |
| Prefers the new line in at least 3 of 5 pairs (Q5) | `TO PREDECLARE` | testers prefer the old lines |
| Any report of pressure or surveillance (Q4) | none | any report: remove or soften that kind (the hour habit first, as run 3's stop rule says) |

**Stop or pivot** if any of the spec's three stop rules hold for most of a band: no recall, repetitive within a week, or the old lines preferred. Record the result and what changed in [decision-log.md](../../../../docs/research/decision-log.md).

## Limits, said plainly

- **Recall is checked only where the participant agrees, and only by kinds.** The export (step 4a) shows date, neighbour and kind, not wording, and only for participants who consent. Without it, judge recall by the fact kinds true of that garden.
- **A waiting thing can repeat, but less often.** Since CP07, a waiting fact (a cutting in the basket, an unread letter) that a neighbour has mentioned on two of the last three days (yesterday and the two before) loses 40 points for that neighbour. Other facts take the slot, and it returns when the window clears. It is a penalty, not a ban, so a waiting thing can still be picked when nothing else scores higher. The constants are `NagDays` 3, `NagTimes` 2 and `NagDamp` 40 in `Core/CompanionRanker.cs`. They are hand-set tunables, not validated; if this study is to test them, predeclare them as `TO PREDECLARE` like the thresholds above. Q3 and Q4 still watch for nagging.
- One week can't show whether variety lasts. The bank has 20 wordings per kind and voice, and the 14-day rule means a remark heard daily won't repeat its wording within the study week.

**Status:** not run. `age fit not validated`.
