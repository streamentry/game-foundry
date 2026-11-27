# v1 trust batch: items 88 (ticked) and 96 (test only)

Date: 2026-11-27. Branch `feat/mosselora-v1-trust-88-96`. Two Haiku agents wrote the tests, and the coordinator reviewed them.

## 88 Honest companions (ticked)

`Tests/HonestCompanionTests.cs` audits every English player-facing line against vision §1a ("What we must not claim") and AGENTS.md. It uses three rule groups, kept as data with self-tests:

- **R1:** health, cognition or memory-benefit claims.
- **R2:** loneliness or dependence pressure.
- **R3:** implying a neighbour is a real person, or professing personal affection.

**Coverage.** Every string literal in `Assets/Scripts/Core` and `Assets/Scripts/Unity` is scanned, comments excluded, and dev and QA files skipped. On top of that, a reflection sweep reads Core's static text, and the generators (VillageStory, NeighbourNotice, SeasonTales, Visitors, NeighbourWish, NeighbourGifts, MemoryShelf, GameLocalization) are enumerated over 39 garden states. In total 2,644 distinct lines are audited, and canaries prove each path reaches real text.

**Result:** no violations. `KnownViolations` is empty, so any new offending line fails the build.

**Decisions recorded as allowed examples:**
- Fictional family backstory in SeasonTales ("When I was small, my grandmother…"): characters with histories are not claims to be real people.
- "Good health" as a family blessing: a wish, not a claim.

**Not covered:** the 29 translations. They need native review under item 86.

## 96 Save is never lost (not ticked)

`Tests/SaveFuzzTests.cs` reproduces the on-disk state after every step of `SaveJournal.Commit`, including torn writes (empty, one character short, random cut), over 150 seeded rounds. It also fuzzes each slot with garbage.

**Invariant:** after any crash, the game loads the complete old save or the complete new save, never a corrupt one or nothing, and a later commit recovers. **No data-loss bug found**, and `SaveJournal.cs` is unchanged.

Still missing before 96 can be ticked:
- **A visible "saved" mark.** Today only a failure banner exists (`EvergroveApp.cs` ~949–971).
- **Export to Files.** Today the garden file can only be copied to the clipboard (`EvergroveApp.cs` ~8180–8214).

Design findings for a decision:
- **A torn very first save blocks saving.** If it leaves only a corrupt `.tmp`, the game sets `RecoveryBlocked` and refuses every later save until a reset. Nothing valuable is lost, but the player can't save. This is deliberate fail-closed behaviour today (`SaveJournalTests`).
- **A complete newer save can be skipped.** `TryLoad` prefers `.bak` over a complete `.tmp`, so a crash during publication rolls back one save. A valid `.tmp` only exists mid-commit, so it is always the newest.

## Verification

- `.NET` suite: 1,212,565 assertions pass on this branch (off main `cb1e0fa`).
- `tools/verify_evergrove_project.py`: OK.
