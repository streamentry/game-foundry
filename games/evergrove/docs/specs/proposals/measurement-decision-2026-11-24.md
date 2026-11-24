# Measurement decision proposal: how D1/D7 return and install sources get measured

**Status: proposal. Requires founder approval before any store build relies on it.** No store build, store listing, App Privacy answer or Data Safety answer changes until that approval and a privacy review are recorded. The research-only on-device return diary described in §5 is implemented behind research guards in this change; it does not add any network, analytics or store-facing data flow.

Sources read: [vision.md](../../../vision.md) §5 "Measurement constraint", [02-FEATURE-BACKLOG](../02-FEATURE-BACKLOG.md) (reject list, EG-103 consent-first notes), [06-ENGINEERING-AND-RELEASE](../06-ENGINEERING-AND-RELEASE.md) (privacy and release sections), [10-EXPERIMENT-PROTOCOLS](../10-EXPERIMENT-PROTOCOLS.md) (D1/D7 definition, `LocalPlaytest` limits), [VALIDATION](../../VALIDATION.md) metric dictionary, [LocalPlaytest.cs](../../../Assets/Scripts/Core/LocalPlaytest.cs), [m3-session-kit](../../playtest/m3-session-kit.md), [m4-preregistration-draft](../../playtest/m4-preregistration-draft.md), and the repository [AGENTS.md](../../../../../AGENTS.md) (privacy, consent, no tracking without approval, install attribution and retention instrumentation for a shipped game).

---

## 1. The problem

Every growth rung above Seed in [vision.md](../../../vision.md) needs a number that does not exist yet:

- **D1 / D7 voluntary return:** the Sprout gate reads "voluntary D7 meets a threshold predeclared before launch". Today a researcher can only ask consented participants afterwards, which is self-selected and easy to bias.
- **Search-source installs:** the "1,000 non-branded organic installs by day 60" checkpoint must be read against search impressions and non-branded share. Nothing in the app can see where an install came from.
- **Referral funnel:** sent → opened → completed → returned for the share card (EG-203) cannot be measured without some linkage.

The specs forbid the obvious instruments. The reject list in [02-FEATURE-BACKLOG](../02-FEATURE-BACKLOG.md) bans "always-on analytics" and [06-ENGINEERING-AND-RELEASE](../06-ENGINEERING-AND-RELEASE.md) requires "no ad SDK, forced login, server-time dependence". So the decision is not "add analytics" but "which privacy-preserving instrument answers which question, with what store disclosure".

## 2. Options compared

| # | Option | What it measures | Privacy impact | Consent flow | Store label implication | Verdict |
|---|---|---|---|---|---|---|
| A | **Manual follow-up** (researcher contacts consented participants, asks whether they opened the app on day 1 / day 7) | D1/D7 for a recruited cohort, self-reported | Researcher holds names/contact details off-device; minimal app impact | Written consent at recruitment; separate permission for follow-up | None (nothing collected by the app) | **Keep.** Current method. Biased and slow, but honest. |
| B | **On-device consented return log, exported by the participant** (`ReturnDiary`, §5) | Exact local calendar dates on which the app was opened; D1/D7 computed by the participant's copy and the facilitator | Dates only, stored in local preferences, never transmitted by the game; participant chooses to copy and send it | Two-step explicit consent in research builds; one-action delete | Data never leaves the device → "Data not collected" is expected, to be confirmed in review | **Adopt for research builds** (M3/M4 participants). |
| C | **App Store Connect / Play Console built-in aggregate metrics** (installs, sessions, retention cohorts by source where the platform provides them) | Store-side installs, impressions, conversion and platform retention, aggregated by the platform | No SDK. Apple and Google aggregate on their side; the developer receives aggregates. Attribution depends on platform rules | Platform-managed; the game adds no prompt | Depends on the platform's own disclosures; the developer's label is not changed by reading Apple's or Google's reports | **Adopt for store builds** as the only measurement source. Check the platform's current retention and source fields before relying on them. |
| D | **First-party server endpoint** (the game posts events or counts to Mosselora-owned servers) | Anything the server receives, including exact retention | Highest: device-linked or pseudonymous identifiers, network traffic, retention of logs, breach surface; breaks airplane-mode promise | Full consent model, data-retention policy, account deletion, security review | "Data collected" for analytics and possibly identifiers; must be declared in App Privacy and Data Safety | **Reject for now.** Violates the offline promise and needs a privacy review that does not exist. The `WorldClient.cs` / `backend/` prototype is detached and not part of this option. |
| E | **Third-party analytics or attribution SDK** | Everything the vendor collects | High and opaque; SDK-driven traffic is unaudited | Often bundled consent prompts that are hard to keep honest | Broad collected-data declarations; may conflict with "no tracking" positioning | **Rejected** per [02-FEATURE-BACKLOG](../02-FEATURE-BACKLOG.md) and [06-ENGINEERING-AND-RELEASE](../06-ENGINEERING-AND-RELEASE.md). |

## 3. What each option can and cannot measure

| Question | A manual | B diary | C store aggregates | D server | E SDK |
|---|---|---|---|---|---|
| D1 / D7 return, consented cohort | Yes, self-reported, biased toward those who answer | Yes, exact dates, only if participant copies the log | Not per cohort; platform retention is aggregate | Yes | Yes |
| D1 / D7 return, all installs | No | No | Partial: platform cohort retention if published | Yes, but with traffic and identity cost | Yes |
| Install source (search vs referral vs other) | No | No | Partial: store search impressions and install source where the platform reports it | Only with attribution linkage | Yes, with tracking identifiers |
| Referral funnel (share → open → complete → return) | Manual only | No (the diary cannot see other devices) | Not separable | Yes | Yes |
| Individual satisfaction | Interview | Not measured | Not measured (reviews are not evidence of demand) | Not measured | Not measured |
| Causality | No, observational | No | No | No | No |

**Limits to state in every report:** the diary measures *return* only for people who opt in and copy their log; it is not a random sample, and opt-in can select engaged players. Store aggregates describe installs and platform retention, not the 45+ audience; age bands are `unknown` unless a source states them ([AGENTS.md](../../../../../AGENTS.md)).

## 4. Store privacy-label implications

- **Option B (diary) in research builds only.** Research builds are not store submissions. If a research build is ever uploaded to a store, the diary is compiled out by the release-candidate guard in [verify_evergrove_project.py](../../../../../tools/verify_evergrove_project.py), so the store binary contains no diary code. Labels for store builds are unchanged.
- **Option C (platform aggregates).** Reading Apple App Analytics or Google Play statistics does not add a data collection by the app. Confirm each platform's current wording before publishing labels.
- **Option D or E** would require declaring collected identifiers, usage data and possibly tracking. These are not approved.
- **Store build answer, provisional:** "data not collected" only if the release binary has no network stack for measurement, no identifiers and no SDK. This must be verified against the built binary (the repository already requires "inspect build dependencies and actual traffic" before disclosure). Legal review of the actual wording is outside this proposal.

## 5. Recommendation

1. **Research builds (M3/M4 participants):** use the on-device consented return log (option B). It is implemented in this change as `ReturnDiary` (Core) and wired only into the development/research playtest controls. Its consent is two-step; it records local calendar dates only; it has no network, device identifiers or clocks beyond the local date; it can be deleted in one action; and it is exported only by a participant's explicit copy action. The facilitator computes day 1 / day 7 from the copied text.
2. **Store builds:** rely only on platform-provided aggregate analytics (option C). Nothing else is added to the store binary.
3. **Keep option A** (manual, consented follow-up) for any participant who does not opt into the diary, and for facilitator reminders that are not rewards.
4. **Do not adopt D or E** until a separate privacy review, data-retention policy, account-deletion design and store-label review exist. Nothing else until that review.

**Counterargument and falsifier:** a diary records only people who choose it, so D1/D7 from it may overstate retention. The cheapest disproof is to compare diary-based D1 for consented participants against manual follow-up for the same cohort. If they diverge by more than the predeclared margin in [10-EXPERIMENT-PROTOCOLS](../10-EXPERIMENT-PROTOCOLS.md), the diary is not a valid retention measure and the Sprout threshold must rest on manual follow-up alone.

## 6. Approval requested

- [ ] Founder approves option B for research builds only, with the consent and export behavior described above.
- [ ] Founder approves option C as the only store-build measurement, after checking the platforms' current fields.
- [ ] Privacy review of the built binary (network traffic, identifiers, label wording) is scheduled before any store submission.
- [ ] Options D and E remain rejected unless a new proposal and review are approved.

Until these boxes are ticked, this document is a proposal, and no release may claim measured D1/D7, search-source installs or referral conversion.
