# M3 run plan: first uncoached playtest with adults 45+

**Status: PLAN, 2026-11-24. No participant has been recruited and no session has run.** This plan turns the instruments in [m3-session-kit.md](m3-session-kit.md) and [m3-recruitment-kit.md](m3-recruitment-kit.md) into one ordered path. It contains no participant data. Every recommended default in section 3 is a proposal until the founder accepts it and it is logged in [decision-log.md](../../../../docs/research/decision-log.md).

## 1. Preflight

| Question | Answer |
| --- | --- |
| **Truth** | Observed: the build runs on Android emulators and the iOS Simulator; 39,931 rule assertions pass; the facilitator flow was dry-run on an emulator (section 2). Assumed: adults 45+ can read the first board, act without help and notice the garden change. No adult 45+ has played any build. |
| **Owner** | Gate M3 in [09](../specs/09-EXECUTION-CONTROL.md) and the stopping rules in [DESIGN §12](../../DESIGN.md). |
| **Delete** | Removed from M3: D1/D7 return, price questions, M4's Living World vs Puzzle Only comparison, localized copy. Each has its own later gate. |
| **User value** | Whether a first-time adult can play the first minute without help. If not, no other feature matters. |
| **Audience fit** | Unknown. That is what this study measures, one age band at a time (45–54, 55–64, 65+). |
| **Falsification** | 10 uncoached sessions on real compact phones. 3 or more of 10 missing the goal or the restoration is a redesign result. |
| **Stop** | See the decision rule in section 4. |
| **Evidence** | Filled [results-template.md](results-template.md) with raw counts by band, plus a signed decision-log entry. |

## 2. Facilitator dry run on the emulator (2026-11-24)

Build `nt-pillfix` (source `59920b3`; later commits on `main` up to `9df0bf8` change docs and tools only), Android emulator API 35, 1080×2400, English.

| Step | Result | Evidence |
| --- | --- | --- |
| Clean save: `pm clear` (on a phone: Settings → Apps → Mosselora → Storage → Clear storage) | **Pass.** Opens directly on Stream 1, "First Bend", with no intro screen. App launch is the first interactive screen. | [android-fresh-start.png](m3-dryrun-2026-11-24/android-fresh-start.png) |
| Start the local report ("Start an optional private playtest", below the board) | **Pass.** The button changes to copy and erase controls. | — |
| Copy the report after two tile taps | **Pass.** "Copied to clipboard." The text begins `EVERGROVE LOCAL PLAYTEST v1`. Nothing leaves the device. | [android-report-copied.png](m3-dryrun-2026-11-24/android-report-copied.png) |
| What a participant sees by scrolling down | **Blocker B1.** See below. | [android-dev-controls-visible.png](m3-dryrun-2026-11-24/android-dev-controls-visible.png) |

**Blocker B1: the playtest build revealed the hypothesis. Fixed the same day; see below.** In the development build, the area below the board showed "PLAYTEST MODE · Choose before your first move", "Living world / Puzzle only", "Start matched control (dev)", the 2D/2.5D visual switch, the report controls and "Start return diary". Any participant may scroll on their own. The session kit forbids revealing the hypothesis ([10](../specs/10-EXPERIMENT-PROTOCOLS.md)), and a participant who taps "Puzzle only" leaves the condition under test.

**Fix: participant view.** Tapping "Start an optional private playtest" now starts the report, hides every research and developer control, and jumps back to the top of the page. Player features stay: language, text size, board size, neighbors, the garden file, the chime and "Restart garden". **Holding the Mosselora logo for 3 seconds** brings the controls back, with the report still running. A tap or a scroll on the logo does nothing. This applies to development builds only; ordinary player builds never contain these controls. Code: `StartParticipantView` in [EvergroveApp.cs](../../Assets/Scripts/Unity/EvergroveApp.cs) and [FacilitatorHold.cs](../../Assets/Scripts/Unity/Dev/FacilitatorHold.cs).

| Check (source `107f05c` plus this change) | Android emulator, API 35 | iOS Simulator |
| --- | --- | --- |
| Starting the report jumps to the top | Pass | Pass |
| Below the board, only player features show | Pass: [android-participant-view-bottom.png](m3-dryrun-2026-11-24/android-participant-view-bottom.png) | Same code path; not captured separately |
| A short tap on the logo keeps the participant view | Pass | Not tried |
| A 3.5 s hold on the logo restores the controls; the report keeps running | Pass: [android-after-facilitator-hold.png](m3-dryrun-2026-11-24/android-after-facilitator-hold.png) | Pass: [ios-after-facilitator-hold-copied.png](m3-dryrun-2026-11-24/ios-after-facilitator-hold-copied.png) |
| The copied report counts the participant's taps | Copy shown; clipboard not readable through adb | Pass: `rotations=2`, `first_tap_seconds=5.7`; text in [ios-report-sample.txt](m3-dryrun-2026-11-24/ios-report-sample.txt) |

Checks run: C# tests (39,931 assertions), `verify_evergrove_project.py` (including the dev-control source guard), the Unity Editor smoke, the Android APK build and the iOS Simulator build. Physical phones: **not run**.

**Gap G1: the report clock starts at the facilitator's tap.** `first_tap_seconds` counts from the tap on "Start an optional private playtest". The view now jumps straight to the board, so the hand-over can follow within a second or two, but the stopwatch started at hand-over stays the primary measure of time to first input. Record the report value as secondary.

**Minor:** a small "Development Build" watermark shows in the bottom-right corner. It matches the "early prototype" framing, so no change is needed for M3.

**Not yet tried:** the same flow on a physical iPhone and a physical Android phone. That is part of step 2 below.

## 3. Founder decision sheet (one sitting)

Every `TO DECIDE` and `TO PREDECLARE` item from the kits, with a recommendation. Accept or change each one, then log the full set before the first invitation goes out.

| # | Decision | Recommended default | Why |
| --- | --- | --- | --- |
| 1 | Target N | **10 starters** | The only M3 rule with defined actions ([DESIGN §12](../../DESIGN.md)) is written for 10. [VALIDATION](../VALIDATION.md) already says to use it. |
| 2 | Threshold set | **DESIGN §12, with the middle band in section 4** | Settles contradiction 1 in [README](README.md). The 70%/60% and 80%/70–75% figures are not applied. |
| 3 | Age-band quotas | **45–54: 3 · 55–64: 4 · 65+: 3** | Covers every band. 55–64 gets the extra seat because the first market hypothesis is U.S. adults 50–75 ([AGENTS.md](../../../../AGENTS.md)). Per-band counts are descriptive only. |
| 4 | Prior puzzle-play mix | **In each band, at least 1 "never" and no more than 1 "weekly or more"** | Use the screener's existing categories; no new cut-offs are needed. |
| 5 | Add the phone-game screener question ([recruitment kit §3c](m3-recruitment-kit.md)) | **Yes**; at least 2 of 10 rarely or never play phone games | Stops the sample filling with practised players. |
| 6 | Professional game developers or UX researchers | **Exclude**, and record them as excluded starters | They are not the audience, and they narrate design intent. |
| 7 | Founder-network cap | **At most 2 of 10, acquaintances only** | Close friends and family are already excluded. |
| 8 | Phones | **Provided phones only**: one compact iPhone and one compact Android phone (section 5) | Keeps the device class fixed. PUZ-006 needs compact-phone evidence. No research build is installed on anyone's own phone. |
| 9 | Which phone each participant uses | **The platform they normally use** | Removes system-gesture confusion that is not the game's fault. Record it. |
| 10 | Hint policy | **The facilitator never points at "Guide me"** | The study is uncoached. The participant may find it alone; that is counted as a hint request. |
| 11 | Facilitator stop rule | **Stop only if the participant asks, or for a technical failure:** crash, a freeze over 10 s, save loss, or a device fault | Technical-failure rows stay in the table, marked invalid, and are not counted as comprehension failures. |
| 12 | Session format | **In person, 30-minute slots** | Provided phones and observing taps both need a room. |
| 13 | Language | **English only**, with country of residence recorded | English is the only certified locale. The U.S. market claim needs U.S. residents; record a mix honestly. |
| 14 | Return diary | **Off for M3** | Return is not an M3 measure, and the button is part of blocker B1. |
| 15 | Thank-you gift and expenses | **Founder sets the amount.** The same gift goes to everyone who attends, unconditionally. | Spending is a founder decision ([recruitment kit §1c](m3-recruitment-kit.md)). |
| 16 | Private storage | **One private folder outside the repository**, readable only by the founder and named facilitators | [Recruitment kit §5](m3-recruitment-kit.md). |
| 17 | Retention | **Contact details: delete 30 days after the last session. Recordings: none.** The founder sets the other periods. | Data minimization. Consent-record retention may have a legal answer; check locally. |
| 18 | Pilot session | **1 pilot with an adult 45+ who is otherwise excluded** (for example a close friend). It is logged and never counted. | Rehearses the script, the reset and the stopwatch without spending a real seat. |

## 4. Decision rule (proposed for N = 10)

Definitions, all observed by the facilitator:

- **Act within 30 s:** the first tile rotation within 30 s of the hand-over, measured by stopwatch, with no help.
- **Cannot infer the goal:** the answer to "What are you trying to do?" does not mention water, pipes, connecting or the garden.
- **Misses restoration:** after the first solve, the answer to "What just happened?" does not mention water arriving or the garden or stream changing.
- **Notices the consequence:** names a change to the creek, garden or village in their own words, either unprompted or in answer to one of the scripted questions. Record which.

| Result | Action |
| --- | --- |
| **Redesign:** 3+ of 10 cannot infer the goal, **or** 3+ of 10 miss restoration | Stop expanding visual scope. Fix legibility of the first board, then run 10 new starters. ([DESIGN §12](../../DESIGN.md)) |
| **Proceed:** 8+ act within 30 s **and** 7+ notice the consequence, and the redesign rule is not triggered | Go to M4 preparation (the matched control build). This is **not** a launch claim. |
| **Middle band:** neither rule fires | Fix the most repeated observed failure once, then run 10 new starters under the same rule ([10 Interpretation](../specs/10-EXPERIMENT-PROTOCOLS.md)). |
| **Under 10 valid starters** | No verdict. Report raw counts and recruit the rest. |

Report raw counts per band, not percentages. Record the median time to first input; it is not part of the rule.

## 5. Critical path

| Step | Who | Done when | Blocks |
| --- | --- | --- | --- |
| 1. ~~Fix B1 (participant view); rebuild; dry-run again on the emulator and Simulator~~ **Done 2026-11-24** (section 2) | Agent | New screenshots show no experiment or developer controls in participant view; report still copies | — |
| 2. Get two phones. Compact iPhone, iOS 15 or later, 375×667-point screen class (for example iPhone SE 2nd or 3rd generation). Compact Android, Android 8.0 (API 26) or later, about 360 dp wide. | Founder (buy or borrow) | Both phones are at the desk | Step 3 |
| 3. M2 device gate: one source SHA, install on both phones, run [12 §4](../specs/12-M1-M2-EXECUTION-CHECKLIST.md) and repeat the section 2 dry run on each | Agent, once the phones are connected (iPhone needs development signing) | SHA and artifact hashes recorded; zero open S0/S1 | Step 6 |
| 4. Accept or change section 3; log it | Founder (about 30 minutes) | Dated decision-log entry | Step 5 |
| 5. Recruit 15–20 people for 10 seats across 3 or more channel types ([recruitment kit §2](m3-recruitment-kit.md)) | Founder | 10 booked, with quotas met or the shortfall recorded | Step 6 |
| 6. Pilot session, then 10 sessions | Founder or a facilitator | One observation sheet per starter, same day | Step 7 |
| 7. Fill [results-template.md](results-template.md), apply section 4, sign the decision | Founder; the agent can tabulate anonymized sheets | Decision-log entry with counts by band | — |

Steps 1, 2 and 4 can run at the same time. Sessions can be spread over 2 or 3 days; five 30-minute sessions a day leaves time to reset and write notes.

## 6. Per-session run sheet

**Before the participant arrives**

1. Clear the app's storage (Android: Settings → Apps → Mosselora → Storage → Clear storage; iPhone: delete and reinstall the same build). Note the reset on the sheet.
2. Open the app and confirm Stream 1, "First Bend", is showing. Confirm the text size is at the default and note it.
3. Scroll down and tap "Start an optional private playtest". The app jumps to the board and hides the research controls. Scroll down once to check that only player settings show.
4. Fill in the sheet header: code, band, device, build SHA, time.

**During the session**

5. Read the consent script. Record notes consent and recording consent separately; recording defaults to no.
6. Hand over the phone, say the exact opening line, and **start the stopwatch at the hand-over**. Stay silent for one minute.
7. Use only the scripted questions at the scripted moments ([session kit §3](m3-session-kit.md)). Write the participant's words down exactly.

**After the session**

8. Hold the Mosselora logo for 3 seconds, scroll down, tap "Copy my local playtest notes" and paste the text into the private sheet, not into the repository.
9. Tap "Stop and erase local notes", then clear storage again.
10. Write up the observation sheet the same day. Update the tracking table.

## 7. What this plan does not cover

- M4 (Living World vs Puzzle Only). It still needs the matched control build ([m4-preregistration-draft.md](m4-preregistration-draft.md)).
- Return (D1/D7), price, store listing or any launch claim.
- Any claim about adults 45+ as a whole. Ten people show where the first minute breaks; they do not give a population rate.
