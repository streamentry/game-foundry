# Release checklist (per game)

## Gameplay
- [ ] Valid puzzles, exactly one solution where promised; deterministic test corpus.
- [ ] Difficulty verified by solver/human sessions rather than arbitrary grid size.
- [ ] Save/resume after app kill, upgrade and low storage.
- [ ] Undo, hints, errors, offline behavior and no unskippable onboarding.
- [ ] Tested on small iPhone, large iPhone, iPad, representative low-end Android.

## Platform & rights
- [ ] Stable engine/editor and export templates pinned.
- [ ] iOS macOS/Xcode signing, screenshots, TestFlight and actual device install verified.
- [ ] Android package signing, target SDK, internal track and real device verified.
- [ ] Original name, icon, UI, levels, sounds, illustrations; licensed dependency inventory.
- [ ] Apple 4.3 indistinguishable-app review; avoid variant spam.
- [ ] Accessibility, localization, privacy policy and age rating evaluated.

## Monetization & telemetry
- [ ] SDK versions audited for current OS, consent and privacy manifest requirements.
- [ ] Ads (if any) not deceptive, excessive or targeted inappropriately.
- [ ] Receipts, restoring purchases and offline fallbacks tested where applicable.
- [ ] Event schema and opt-out reviewed; no secrets in code or build logs.
- [ ] Crash monitoring and basic cohort dashboards instrumented.

## Launch decision
- [ ] Non-branded keyword data and top competitors documented with source/date.
- [ ] Different experience/promise is observable to a new player.
- [ ] Stop/continue threshold and review date recorded.
