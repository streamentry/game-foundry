# Mosselora (Evergrove) playtest packet

**What this is:** blank, ready-to-use instruments for two human evidence gates:

- **M3 — core comprehension:** 8–12 uncoached adults, first minute and first chapters. See [m3-session-kit.md](m3-session-kit.md). Gate definition: [09 — Execution control](../specs/09-EXECUTION-CONTROL.md#milestone-gates-and-feature-freeze); test `VAL-001` and `PUZ-006` in [verification-matrix.json](../specs/verification-matrix.json).
- **M4 — matched value comparison:** 20–30 participants, Living World vs Puzzle Only on identical boards. See [m4-preregistration-draft.md](m4-preregistration-draft.md). Protocol: [10 — Experiment protocols](../specs/10-EXPERIMENT-PROTOCOLS.md); test `VAL-002`.

Results are captured in [results-template.md](results-template.md).

**Start here for M3:** [m3-run-plan.md](m3-run-plan.md) orders the steps, gives a recommended default for every open decision, and records the facilitator dry run of 2026-11-24, including blocker B1 (the development build showed the experiment controls to participants), now fixed with a participant view.

**Nothing in this folder counts as evidence until real sessions are run.** No participants, sessions, outcomes, dates or decisions are recorded here. Every value the source docs do not declare is marked `TO PREDECLARE`, and must be fixed in writing before the first participant is recruited.

## Order of use

1. **Resolve the `TO PREDECLARE` items** and the source contradictions below. Record the decisions with a dated entry in [decision-log.md](../../../../docs/research/decision-log.md) before recruiting.
2. **M3 only:** recruit using the screener and run sessions with the consent script and facilitator script in [m3-session-kit.md](m3-session-kit.md). Record each starter on the observation sheet, including abandoned and excluded attempts.
3. **M4:** do not recruit until the matched control build exists and its fixture hashes are recorded. The in-app Puzzle Only mode is **not** a valid control after chapter 1. See the blocking prerequisite in [m4-preregistration-draft.md](m4-preregistration-draft.md).
4. Put raw counts into [results-template.md](results-template.md), with denominators.
5. Record the signed continue / simplify / stop decision in the same template and link it to the decision log.

## Privacy

- This folder lives in a public repository. Keep names, contact details, recordings, consent forms, raw notes that identify anyone, and any follow-up list **out of the repo**. Store them in a permissioned location and reference them only by an anonymous participant code (for example `P-01`), as [11 — Evidence packets](../specs/11-EVIDENCE-AND-RELEASE-PACKETS.md) requires.
- Publish only anonymized counts, or a redacted summary.
- The in-app `LocalPlaytest` report stores no identity and cannot measure D1/D7 return ([VALIDATION](../VALIDATION.md)). Any return follow-up needs a separate consent and process.

## Source contradictions found while drafting

These are not resolved here. Each needs a decision before the affected gate runs.

1. **M3 thresholds differ across sources.**
   - [DESIGN §12](../../DESIGN.md): with 10 starters, 8+ act within 30 s, 7+ notice the consequence; 3+ missing the goal or the restoration means redesign.
   - [07](../specs/07-VALIDATION-AND-GTM.md): ≥80% explain the goal unaided; ≥70–75% recall a consequence after 10 minutes.
   - [VALIDATION](../VALIDATION.md): ~70% first correct action within 30 s; ≥60% unaided consequence explanation.
   - [vision §5](../../vision.md): median first input under 15 s, and ≥80% explain the goal.
   The packet quotes all four. Pick one set for M3 before recruiting.
2. **D1 ≥30% and D7 ≥15%** are attributed by [vision §5](../../vision.md) to VALIDATION.md. That file gives no numeric D1 or D7 threshold. The numbers appear to have no source in the repo, so they are not used here.
3. **Experiment letters clash.** VALIDATION's "Test B" is ecological branch mechanics; [07](../specs/07-VALIDATION-AND-GTM.md) and [10](../specs/10-EXPERIMENT-PROTOCOLS.md) use "B" for Living World vs Puzzle Only. VALIDATION's "Test A" is first-chapter meaning, while 10's "A" is uncoached usability. This packet uses the names from 10.
4. **The preference rule needs within-person exposure.** 07's continue rule requires "a clear majority prefers the living-world condition". 10 uses a between-subject assignment, where each person sees one condition. A preference can only be compared if participants play both. This is unresolved; see the M4 draft.
5. **Preregistration location.** 10 says a redacted public plan goes in `docs/research/`. This packet puts the draft in `docs/playtest/` as requested. Move or link it when it is finalized.
6. **The sample range for M3 differs.** 07 and 09 say 8–12. DESIGN's decision rule uses exactly 10 starters, and it does not transfer directly to 8 or 12. Predeclare the rule for the actual N.
