# Evergrove AGENTS.md — source-grounded execution contract

> **Mosselora is Evergrove.** Evergrove is the internal codename; Mosselora is the public name. Keep internal identifiers; use Mosselora for player-facing text. Logo and app icon masters: `brand/` (copies in `Assets/Branding/` and `site/public/brand/`).

Read [repository AGENTS.md](../../AGENTS.md) and the [Evergrove handbook](docs/README.md) **before** changing this Unity/C# project. It is the approved engine exception in Game Foundry's mostly Godot portfolio.

The separate [37-feature product specifications](docs/specs/README.md) are **proposals** (P0/P1/P2), not part of the current implemented contract. The [execution gates](docs/specs/09-EXECUTION-CONTROL.md) and [machine-readable verification registry](docs/specs/verification-matrix.json) control readiness: **M1 real Unity build → M2 devices/recovery/privacy → M3 first-session usability → M4 puzzle-identical narrative comparison → M5 certified locales → M6 commercial release**. New P1/P2 features are blocked by evidence, not a high abstract score. Check the as-built handbook and code before taking a proposed feature into a PR.


## Player invariant

**Short, satisfying spatial play must have a clear, truthful consequence in the village and the next puzzle.** No extra AI, servers, content volume or art polish unless it can improve a player behavior observable in testing.

## Source authority

- `Assets/Scripts/Core/PipeRules.cs` and `ReactiveWatercourses.cs`: pure water puzzle and branching mathematics.
- `Assets/Scripts/Core/WorldModel.cs`: authoritative progress and save schema; maintain v1/v2/v3 → v4 migration.
- `Assets/Scripts/Core/VillageScenePlan.cs`: derived read-only world illustration.
- `Assets/Scripts/Core/VillageStory.cs`: longer English manuscript; `LocalizedVillage.cs` and `GameLocalization.cs`: compact 30-language runtime copy.
- `Assets/Scripts/Unity/EvergroveApp.cs`: Unity UI, not another world authority.
- `Assets/Scripts/Core/SaveJournal.cs` + `Assets/Scripts/Unity/WorldStore.cs`: validated primary/backup/staged recovery, safe backup rotation, and explicit write block when all slots fail. The UI warns and requires a confirmed reset; **physical interruption and recovery remain unverified**.
- `Assets/Scripts/Unity/VillageIllustration.cs` / `VillageDiorama.cs`: 2D default and optional experimental 2.5D.
- `Assets/Scripts/Unity/WorldClient.cs` and `backend/`: detached prototype, not real cloud game sync.

## Definition of done

1. State one **observable player value**, strongest counterargument, and cheapest disproof.
2. Keep world events deterministic and the user-visible cause ↔ effect contract coherent.
3. Update pure C# tests for all 12 base puzzles, 33 subsequent variants, corrupted saves, migrations, localization and any new action. No “test passed” without a run.
4. Run `dotnet run --project games/evergrove/Tests/Evergrove.Core.Tests.csproj --configuration Release` and `python3 tools/verify_evergrove_project.py`.
5. Unity import/compile and physical mobile QA are **separate** gates. When not executed, explicitly say **not run** even if C# CI is green.
6. Update the relevant handbook chapter in `docs/` and the existing test/decision log in the same PR. Prefer precise diffs to duplicative megadocs.
7. Avoid external sends, tracking, monetization, backend deployment, new third-party assets and app-store publishing without explicit scope and evidence.

## Never claim

- That 90°/180°/270° variants are 33 independent authored ecosystem simulations.
- That NPCs have agent goals or learned conversational memory.
- That Living World vs PuzzleOnly isolates narrative after chapter 1; the puzzle differs. Only Living vs **PuzzleMatched** (same variant schedule) is puzzle-identical, and its Unity UI is not wired yet.
- That 30 locale catalogs mean 30 native-reviewed, RTL/Indic-safe releases.
- That .NET tests prove Unity Editor, iOS or Android works.
- That Evergrove as a public app name is cleared, or that global market demand is established.

## Highest-leverage unfinished action

With a licensed Unity 6.3 Editor on a maintainer Mac, run `bash tools/evergrove_unity_local_preflight.sh smoke` then `Android`/`iOS` to collect actual private build logs/hashes; every run creates an isolated `.evergrove-evidence/<SOURCE_SHA>/<MODE>/run-*/` directory, and its Editor log must name that same run's binary output path; the unlicensed source CI cannot satisfy M1. The first import may generate untracked Unity metadata: the preflight records SHA-256 and flags incomplete source provenance. Review/commit real generated `.meta`, lockfiles and settings, then rerun before certifying reproducibility. Preserve `.evergrove-evidence/` locally and keep it out of the public repo.

Produce an actual Unity build, verify input/save/text on one phone, watch an uncoached relevant adult navigate chapters 1 and 2, and record whether the **previous village choice** is understood without hints. The next branch should respond to that evidence, not to speculative scope.
