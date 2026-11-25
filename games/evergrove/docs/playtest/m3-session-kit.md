# M3 session kit: uncoached first-minute comprehension

**Gate:** M3 (core comprehension), tests `VAL-001` and `PUZ-006`. Source: [09](../specs/09-EXECUTION-CONTROL.md), [07](../specs/07-VALIDATION-AND-GTM.md), [10](../specs/10-EXPERIMENT-PROTOCOLS.md), [DESIGN §12](../../DESIGN.md).

**Status:** blank instruments. No session has been run. The `TO PREDECLARE` items must be fixed before recruiting.

---

## 1. Recruitment screener

Recruit adults aged 40 and over (40+ until 2026-11-24). Do not recruit only founder friends, staff or the studio's own circle ([VALIDATION Day 3](../VALIDATION.md)).

| Item | Question or rule | Notes |
| --- | --- | --- |
| Age band | 40–49 · 50–64 · 65+ | Record the band only, never the birth year. Report each band separately; do not generalize from one band ([AGENTS.md](../../../../AGENTS.md)). |
| Prior puzzle experience | How often do you play puzzle games on a phone: never / occasionally / weekly or more? | Bands and cut-offs are `TO PREDECLARE`. Record the answer verbatim. |
| Device | Phone model, OS version, screen size (inches) | Record compact (for example 320×568 to 375×667 logical points) separately from larger screens. Compact devices are the hard case ([12 §4](../specs/12-M1-M2-EXECUTION-CHECKLIST.md)). |
| Accessibility need | Do you usually enlarge text or use a screen magnifier or reader? | Include people who enlarge text ([07 Day 3](../specs/07-VALIDATION-AND-GTM.md)). Do not record medical details. |
| Language | Which language do you read most comfortably in the app? | Only certified locales may be tested as shipping copy. Draft locales are research-only ([09](../specs/09-EXECUTION-CONTROL.md)). |
| Recruitment channel | Where did this invitation reach you? | Required for denominators in the results template. |

**Exclusions (recorded as excluded starters, not dropped):**

- Studio staff, founders, and their immediate family or close friends.
- Anyone who has previously played any Mosselora / Evergrove build or seen its prototype.
- Anyone who has previously been a tester in this study.
- Professional game developers or UX researchers: `TO PREDECLARE` whether this is an exclusion or a recorded covariate.
- Any other exclusion: `TO PREDECLARE` before recruiting.

Recruiting from the 40+ segment must be explicit ([VALIDATION Day 3](../VALIDATION.md)). The screener must not reveal the hypothesis about the village ([10](../specs/10-EXPERIMENT-PROTOCOLS.md): never describe the hypothesis before baseline observation).

---

## 2. Consent script (plain language, read aloud)

Read this before the session. Record the participant's answer on a separate consent record stored outside the public repo.

> Thank you for coming. This is a test of a puzzle game, not of you. We are checking whether the game is clear, not whether you are good at it. You can stop at any time, skip anything, and leave without giving a reason.
>
> With your permission, I will take written notes. I will not write your name in the notes; you will have a code, like P-01. I will not share anything that could identify you.
>
> Separately, I would like to ask whether we may record the screen or your voice. Recording is optional. You can say no and still take part. If you say yes, you can change your mind and ask us to delete the recording, and we will.
>
> Do you agree to take part? Do you agree to notes? Do you agree to recording? [Record each answer separately.]

**Consent rules:**

- Notes and recordings have **separate** consent. Recording must never be implied by consenting to notes.
- Consent is recorded per participant, with the time and the code.
- Withdrawal deletes the private record and any linked notes, by policy ([10 Contamination, safety and missing data](../specs/10-EXPERIMENT-PROTOCOLS.md)). Retention schedule: `TO PREDECLARE`.
- Do not store voice, faces, IP addresses, location, or medical or accessibility details in the public repo ([10](../specs/10-EXPERIMENT-PROTOCOLS.md)).

---

## 3. Facilitator script

Do not coach. The facilitator may only use the lines below. Any other words must be recorded as an intervention on the observation sheet.

**Opening (exact):**

> Please try the game however you like. You may stop at any time.

**Then remain silent for the first minute.** Do not explain controls, goals, the hint button or the village. Do not react to mistakes with words. Note the time.

**After the first solve (or after the first minute if unsolved), exact questions:**

1. "What just happened?"
2. "What do you think caused that change?"

**Before the next chapter (exact):**

> What do you expect might change?

**After the next chapter begins (exact):**

> Did anything from the earlier choice matter? What makes you say that?

**Exit (exact):**

1. "What was difficult or unnecessary?"
2. "Would you open this when nobody asked you?"
3. "Which game would you choose right now, and why?"

**Rules:**

- Record the participant's words exactly, not your interpretation. Do not use "Do you love the magical world?" or similar wording ([10 Observational prompts](../specs/10-EXPERIMENT-PROTOCOLS.md)).
- If the participant asks a rule or goal question, answer only with the neutral line: "Try it however makes sense to you." Record the question and the time.
- Hint policy: whether the facilitator may point at the in-game Hint control, and after how many seconds, is `TO PREDECLARE`. Until then, do not point at anything.
- Stopping: the facilitator may end a session only at the participant's request, or for a pre-specified technical failure (`TO PREDECLARE`). Record the stop reason.
- Record the build, the chapter, and the device on the observation sheet before the session starts.

---

## 4. Observation sheet (one row per starter)

Copy this table per session. Fill in only what was observed. Leave blanks blank; do not infer.

| Field | Definition (source) | Value |
| --- | --- | --- |
| Participant code | Anonymous, for example `P-01` | |
| Age band | 40–49 / 50–64 / 65+ | |
| Start time (UTC) and device | Model, OS, screen class | |
| Build | Git SHA and build hash of the candidate build ([10 Preregistration](../specs/10-EXPERIMENT-PROTOCOLS.md)) | |
| Time to first meaningful input (s) | Seconds from first interactive screen to first valid tile rotation ([07 metric dictionary](../specs/07-VALIDATION-AND-GTM.md)). Leave `not_observed` if none. | |
| First correct action within 30 s, unaided | Yes / No, with the action. Denominator is all starters ([10](../specs/10-EXPERIMENT-PROTOCOLS.md)). | |
| Unaided goal explanation | Asked only "What are you trying to do?" before any teaching. Record the words. Yes / No / partial. | |
| Restoration noticed | Participant says the water was restored or that a route was completed, unprompted. | |
| Consequence noticed | Participant names the change to the creek, garden or village after a choice. Record the words and whether it was prompted. | |
| Mis-taps | Count of invalid or unintended touch actions, with a short description. Count in minutes, not only in total ([07](../specs/07-VALIDATION-AND-GTM.md)). | |
| Hint requests | Count of all hint requests. Note whether a tile was aligned. `LocalPlaytest.hints` counts requests; `WorldModel.HintsUsed` counts aligned tiles ([07](../specs/07-VALIDATION-AND-GTM.md)). Record both if available. | |
| Restarts and undos | Count of restarts and undos, per chapter | |
| Abandoned or stopped | Yes / No, the reason given, and the time | |
| Return diary (opt-in, research builds only) | Only if the participant agrees after the consent step, the in-app return diary records local dates only. Ask them to copy the text at the end, or to delete it. Record `day1_return`, `day7_return` and `returned_within_7_days` from the copied text. Do not infer returns from anything else. Non-consenting participants stay on manual follow-up. | |
| Interruptions | Facilitator interventions, if any (should be zero), with the reason | |
| Verbatim quotes | Exact words, by prompt number from section 3 | |
| Failure modes observed | Couldn't see tile / wrong target / didn't understand rotate / no response / misread water flow / other | |
| Session end | Time, and whether the participant chose to continue | |

**Do not** convert quotes into emotion labels. Do not record "seemed happy". Record the words.

**Denominator rule:** every starter gets a row, including abandoned, excluded or incomplete sessions. A session is not dropped because it was incomplete unless a pre-specified technical failure invalidates it ([10](../specs/10-EXPERIMENT-PROTOCOLS.md)).

---

## 5. Decision rule for M3

Use the rule that was predeclared. The sources disagree. Choose one set below before recruiting, and record the choice in the decision log.

| Source | Rule as written | Status |
| --- | --- | --- |
| [DESIGN §12](../../DESIGN.md) | With 10 starters: 3+ cannot infer the goal, or 3+ miss the restoration, means stop expanding visual scope and redesign. 8+ act within 30 s and 7+ notice the consequence means proceed to native M1/M2 QA, not to a launch claim. | Quoted; the rule is for 10 starters. Other N: `TO PREDECLARE`. |
| [07 — Provisional thresholds](../specs/07-VALIDATION-AND-GTM.md) | ≥80% of target first-time testers explain the goal unaided; ≥70–75% recall a specific village consequence after 10 minutes. Provisional, "not empirically validated". | Quoted. |
| [VALIDATION — Provisional decision thresholds](../VALIDATION.md) | ~70% of relevant participants take the first correct action within 30 s; ≥60% describe a consequence and its cause unaided. "Heuristic go/no-go triggers", not statistical thresholds. | Quoted. |
| [vision §5 — Seed M1–M3](../../vision.md) | Median first input under 15 s; ≥80% explain the goal; redesign if 3+ of 10 miss the goal or restoration. | Quoted. The 15 s median does not appear in 07, 09 or 10. |

**Failure routing (from the sources):**

- Players cannot tap or solve: simplify controls and the first board. No content expansion ([10 Interpretation](../specs/10-EXPERIMENT-PROTOCOLS.md)).
- Puzzle understood, consequences not: improve visible cause and effect once, then rerun the matched small study ([10](../specs/10-EXPERIMENT-PROTOCOLS.md)).
- Save corruption or a mobile build failure: stop the study and fix reliability. Discard affected sessions ([10](../specs/10-EXPERIMENT-PROTOCOLS.md)).

**Reporting rule:** report raw counts and repeated failure patterns. Do not describe 8/10 as a population percentage ([09](../specs/09-EXECUTION-CONTROL.md)).

**Gate:** M3 passes only when the predeclared rule is met, failures are fixed or documented, and `PUZ-006` (compact-phone first-action accuracy) has device evidence. Passing M3 does not authorize replay, daily streams, social features or paid expansions ([09](../specs/09-EXECUTION-CONTROL.md)).

---

## 6. Items to predeclare before the first session

- Exact N and the recruitment cut-offs for prior puzzle experience.
- Professional-developer exclusion: yes or no.
- Hint policy and the stop rule for facilitators.
- Which M3 threshold set (section 5) governs.
- Consent record retention schedule and deletion method.
- Storage location of private records, outside the public repo.

## 7. Painted-build addendum (design iterations I01–I152, updated 2026-11-25)

The painted UI made several bets that only real players can settle. Observe these **without prompting** and record them on the sheet, separately for each age band (40–49, 50–64, 65+). Each links to its iteration log in `docs/improvements/`.

| # | Bet | Watch for | Fails if |
|---|---|---|---|
| P1 | The hub's icon-only arrow is obviously "play" (I67) | Time from launch to first tap on the arrow | Median > 10 s, or 2+ of 10 tap something else first |
| P2 | Mira's **picture-only** bubble (I151: sentence now spoken only), the glowing spring/garden on every board (I148), the turning-arrow disc (I138) and the pressing finger (I147) teach the board | First tap on a tile; whether they say a tap *turns* a pipe | Median first tile tap > 15 s, or 3+ of 10 can't explain the goal (spec rule) |
| P3 | Icon-only Undo / Guide me / Restart are found when needed (I53) | Uses Guide me when stuck? Presses and holds to learn a name? | 3+ of 10 stuck > 60 s without finding Guide me |
| P4 | Droplets and cream "mouths" show progress and open ends (I27, open mouths); empty drops are hollow (I146) and water flies tile → drop (I130) | Can they say what the drops mean after stream 1? Do they call them "lives" or "moves left"? | 3+ of 10 can't, unprompted, or 2+ read them as lives or moves |
| P5 | Picture-first choice cards are understood (I66), including the mini framed tile it leaves on the next board (I134) | Before choosing, ask: "What do you expect to change on the next board?" | 3+ of 10 say the choice "doesn't matter", or none connects the mini tile with the framed tile on the next board |
| P6 | The memory vignette and landmark grow-in are noticed (I24, I50) | Recall of who remembers what after 10 min (spec ≥70%) | Below the spec threshold |
| P7 | The menu and the book are found when needed (I03) | Can they change text size when asked, unaided? | 3+ of 10 can't find it in 60 s |
| P8 | Seasonal stones and the hub's latest memory invite a return (I63, I69) | Day-2 voluntary return (consented follow-up only) | Below the predeclared D1 threshold |
| P9 | Motion is pleasant, not distracting (I19, I24, I33, I39) | Comments, or turning Gentle motion off | 3+ of 10 turn it off or call it distracting |
| P10 | The painted stone path shows progress (I120, I123, I124): water under the walked stones, a ring and sprout on yours | After stream 2, ask "where are you in the season, and how far is left?" | 3+ of 10 can't say which stones are done, or which one is theirs |
| P11 | The story reads comfortably in the book serif (I117, I125); labels stay in the rounded sans | Squinting, leaning in, or comments while reading the memory card and journal; whether they raise text size | 3+ of 10 struggle to read story lines that they read easily as labels |
| P12 | Sideways play is wanted and works (I114–I116, I122) | Do they rotate the phone unprompted? If they do, can they find the menu and book on the painting, and Undo/Hint beside the board? | Of those who rotate, 2+ get lost; record the rotation rate either way (it decides whether landscape earns its upkeep) |
| P13 | Touch feedback (I140) helps and doesn't tire: a light tick per turn and a pulse on a finish, on real phones only | Comments, turning Gentle motion off, or the OS haptics switch | 2+ of 10 call it annoying, or none notices it |
| P14 | The running water (I144) and the brighter finished stream (I132) feel like a payoff | Reaction at the first solve (smile, comment); do they watch the water run? | 3+ of 10 miss that the board is finished before the choices appear |
| P15 | "Send a garden postcard" opens the phone's share sheet (I149) and is understood | Ask at the end: "Would you show this to someone? How?" Watch them use the postcard | 3+ of 10 can't tell what the postcard does, or expect a photo rather than text |

**Reversal rule.** Any P-item that fails reverts to the more explicit option already recorded in its iteration log, for example labelled tools (pre-I53) or a coach sentence (I64 kept it). Do not redesign from intuition between sessions. Fallbacks for the newer bets: **P10** shows the spoken position ("Stream 3 of 12") as visible text under the path, or reverts to the pre-I120 dots; **P11** sets story lines back in the rounded sans (pre-I117); **P12** locks the app to portrait (pre-I114) if rotation is rare and confusing, or keeps it if rare but harmless; **P2** restores the coach sentence (pre-I151) first; **P4** adds a visible "4 of 9 watered" line under the drops; **P13** turns haptics off by default; **P15** falls back to copy-to-clipboard wording (pre-I149).

