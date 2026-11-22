# Evergrove: getting a real Unity build (honest release gate)

**Expanded test/runbook:** [Build and QA handbook](docs/BUILD_AND_QA.md).

As of 2026-11-22, pure C# domain tests run in GitHub Actions. **No licensed Unity Editor build or real-device runtime evidence has been provided in the repository.** Do not equate a clean `.NET` test run with a mobile-ready Unity client.

## One-click preview builds

A manual [Build Evergrove Unity Playtest](../../.github/workflows/evergrove-unity-playtest.yml) workflow can produce an **unsigned Android test APK**, **unhosted WebGL ZIP artifact**, or **unsigned iOS Xcode project**. **These are Development Builds for a research cohort**, with Puzzle Only and renderer comparison/report controls deliberately enabled. **Never ship this artifact as a production app.** A future store candidate requires a separate non-development player build and verification that research UI is absent (`BLD-007`). It uses `game-ci/unity-builder@v4`, a custom Editor build method `EvergroveBuild.Build` and the requested Unity 6.3 version from `ProjectSettings/ProjectVersion.txt`.

**Local licensed preflight:** on a Mac with Unity Hub and platform modules installed, from the repository root run `bash tools/evergrove_unity_local_preflight.sh smoke`, then `bash tools/evergrove_unity_local_preflight.sh Android` and/or `bash tools/evergrove_unity_local_preflight.sh iOS`. Set `UNITY_BIN` if the pinned Unity Editor is installed elsewhere. The script checks the exact source SHA, demands real log success markers/output files, records SHA-256 into private ignored `.evergrove-evidence/<SOURCE_SHA>/<MODE>/run-*/`, and **never signs, uploads, releases or claims phone verification**. Unity's first import may generate untracked `.meta`, `packages-lock.json`, and `ProjectSettings/*.asset`. The preflight hashes these explicitly, labels evidence `PROVISIONAL_GENERATED_INPUTS_NOT_IN_GIT`, and allows the next local probe. Before treating the build as reproducible M1 evidence, review and commit the legitimate generated metadata, then rerun cleanly. Unknown untracked source or Unity modification of already tracked inputs fails closed. Review its logs before promoting `BLD-001..003`; see [M1/M2 checklist](docs/specs/12-M1-M2-EXECUTION-CHECKLIST.md).

**Stale-artifact guard:** each local smoke/export invocation now writes to a **fresh** ignored `run-*/` directory. The Editor build method honors an absolute `EVERGROVE_OUTPUT_ROOT` only for local probes; GameCI still exports to `build/<platform>/`. The checker requires an actual Unity success log line identifying that run's exact output path and a nonempty output marker, then hashes every produced file. A leftover APK, Xcode project or old log cannot satisfy a later attempt. Pure Python tests use a fake Editor and do not prove real Unity compilation.

**Maintainer setup (one time):** Open repository **Settings → Secrets and variables → Actions** and configure a supported Unity license. GameCI Personal activation requires `UNITY_LICENSE`; Pro activation uses `UNITY_SERIAL`, `UNITY_EMAIL`, `UNITY_PASSWORD`. Do not paste credentials into chat, commit them or log them. See [GameCI activation instructions](https://game.ci/docs/github/activation/). This workflow refuses to run without license information, and it never publishes an app or deploys the optional Cloudflare backend.

Once credentials are configured, visit **Actions → Build Evergrove Unity Playtest → Run workflow** and choose **Android**, **WebGL** or **iOS**. After success, download the artifact from the workflow run. Android produces `Evergrove-Playtest.apk` (not a signed Play Store release). iOS produces an unsigned **Xcode project**: open and sign it on your own Mac with Xcode; it is **not an IPA**. WebGL produces a folder for self-hosting; serve over localhost or HTTPS (do not double-click `index.html` from a `file://` URL).

The automated Unity build method creates the otherwise empty bootstrap scene. **No manually authored scene asset is needed in the repository.** Runtime UI is constructed in code. This is a conscious temporary workflow; final studio-quality work should be imported into proper scenes, prefabs and assets after player validation.

## Traceable output (after a licensed build)

When GameCI **actually succeeds**, the workflow runs `tools/evergrove_build_provenance.py` and uploads `evergrove-build-provenance.json` alongside the APK/WebGL/Xcode output. The manifest lists source commit, run URL, **requested** editor version, relative paths and SHA256 of every exported file plus a stable content-tree hash. Verify actual editor version in GameCI logs and actual device behavior separately; the provenance file does **not** certify `BLD-004/005/006` or `M2`.

The Unity 6.3 uGUI core package pin was corrected from `2.6.0` (Unity 6.6 core line) to `2.0.0`, with a static CI gate. **The shared Unity Editor/build smoke now also requires the exact editor version and checks actual Unity JsonUtility v4 round trips (including branch + hint), all 12 puzzles, and import/registration of 29 locale TextAssets.** This strengthens a *future* real Unity run; no licensed Editor import has yet been executed here.

## Manual first-play acceptance checklist

- [ ] Opens on a compact Android phone and at least one iPhone in the Unity Editor / local Xcode workflow.
- [ ] Touching the spring's tiles actually rotates them. There are **no input-system exceptions**.
- [ ] On cold launch at 320×568, 375×667 and 430×932 logical viewports, confirm at least one functional water tile is **on screen before any scroll**. The initial Unity first board is intentionally placed ahead of the world art. Confirm focus/tap order matches visual order, with safe-area/large-font enabled. These are target acceptance tests, not measured outcomes.
- [ ] After a real solve, verify automatic navigation reveals the **choice heading and first choice** (not page footer, sound setting or research panel). Rotate again by Undo/re-solve and check no jarring position jumps. Test shortened safe areas and accessibility text expansion.
- [ ] First board: upstream partial trickle but dry downstream village; rotate first-chapter tile 7 (0-index 6) once and observe increased spring-connected water, blue channels and 2D creek reach. Undo retracts. Solve stream 1: full restoration in 2D and 2.5D. Device visual correctness remains unverified.
- [ ] Can solve all 12 boards. Real players should be able to infer *why* water flows; use hints without feeling penalized.
- [ ] Choose a tree, a crossing or flowers. On return, show the same changes and the correct NPC memory.
- [ ] The 2D default works even if the miniature renderer/RenderTexture fails. Animation and sound are off by default.
- [ ] Force close after a move and after a choice; reopen without losing progress or mixing worlds.
- [ ] Undo a solved board and reconsider one choice. Avoid irreversible accidental erasure.
- [ ] No missing font, input package, UI sprite, scene import, shader errors or clipped controls at 320–430px logical widths.
- [ ] Test Android back behavior, iOS safe areas, OS text scaling, keyboard focus, VoiceOver/TalkBack and older device frame time.
- [ ] Run a privacy review: all data local, no analytics or network requests without informed opt-in.
- [ ] Save actual screenshots, Unity build logs and device findings as evidence. Mark a test complete only after someone has performed it.

## Evidence and safety checklist from Deep Research audit

- [ ] **M1:** Licensed Unity 6.3 Editor import/compile + actual unsigned APK/iOS Xcode export; capture log, commit SHA, workflow URL and artifact checksum. The manual workflow has **not** been demonstrated to pass.
- [ ] **M2:** One physical compact Android and one physical iPhone complete 12 streams; save/restore, interruption and corrupt-primary backup tests; sticky save-failure warning visibility at any scroll position and its disappearance after a successful write; physical tap/safe-area audit; controlled network capture. Source-level C# tests alone do not satisfy M2.
- [ ] **Research isolation:** `#if UNITY_EDITOR || DEVELOPMENT_BUILD` encloses comparison/2.5D/report UI; a separate non-development build must prove these are absent. Old Puzzle Only saves need an explicit safe upgrade decision.
- [ ] **Endpoint icons:** Blue waves = spring, green sprout = garden. No language-dependent S/G labels; review contrast and meaning on real devices, including screen-reader semantics.
- [ ] **M3/M4:** Uncoached first-minute study followed by a preregistered **puzzle-identical** living-world/control comparison. After Chapter One the current branch boards differ and a naïve A/B test is confounded.
- [ ] **M5:** Certified locale subset only; 30 draft catalogs are not a 30-language release guarantee. `pt-PT` uses Brazilian Portuguese fallback unless separately approved/translated.
- [ ] **M6:** Legal/store name, privacy/SDK audit, store screenshots, price experiment and S0/S1 closure before shipping.

[Full execution gates, evidence artifacts and test IDs](docs/specs/09-EXECUTION-CONTROL.md) · [Machine-readable feature ledger](docs/specs/verification-matrix.json).

## Product stop/continue rule

This build exists to help people actually experience the game. Compare `Living World` with `Puzzle Only` for **first-chapter impressions only**. Later board rules differ. See [validation](docs/VALIDATION.md) for matched controls. Continue development only if the living-world layer measurably improves comprehension and voluntary replay, and the distribution channel is viable. A successful Unity build proves **buildability**, not product-market fit.

References: [GameCI Unity Builder](https://game.ci/docs/github/builder/), [Unity 6.3 uGUI input setup](https://docs.unity.com/en-us/engine/6000.3/manual/uitoolkits/uielements/uie-support-for-runtime-ui/uie-render-runtime-ui/uie-runtime-event-system).

## 30-language release readiness (new)

- [ ] First launch honors device language including `vi-VN`, `ja-JP`, `zh-Hant-TW`, `es-MX`, `pt-BR`.
- [ ] In-game language list shows all 30 **native names** legibly; auto mode and manual override survive restart.
- [ ] Changing language mid-puzzle, after choosing a tree and at the ending **never changes event history or solved state**.
- [ ] Translation coverage tests pass for **all canonical runtime keys** across all 29 JSON catalogs, including placeholders (currently 81; the static validator is authoritative).
- [ ] Validate language width and dynamic text on a compact device; no clipped buttons or title collisions.
- [ ] Native-script glyph coverage: Chinese, Japanese, Korean, Thai, Vietnamese.
- [ ] **RTL blocker:** Arabic, Persian, Hebrew and Urdu must be tested with a correct shaping/bidi engine. A right-aligned legacy uGUI Text alone is not enough.
- [ ] **Indic blocker:** Hindi, Bengali, Tamil and Telugu need proper conjunct/shaping and multi-codepoint font tests.
- [ ] Native speakers review phrasing, gender, punctuation, numerals and culture, then record sign-off per locale.
- [ ] Confirm 30-locale shipping build contains all Resources catalogs and remains offline; no per-language network calls.
