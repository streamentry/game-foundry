# BLD-007 evidence packet

test_id: BLD-007
source_commit_sha: 79d19ec09be5323db91077517b4f8325409c402a
build_artifact_sha256: 77c1d0621408f4bf2c36150f6b845551b90c3e0117780fa0dd604b503c24dbb3
result: PASS
reviewed_at_utc: 2026-11-23T06:04:57Z
reviewer_role: AI agent (Claude Opus 5.5) build, strings scan and simulator screenshot review; not independent human verification

**Passing criteria (verification matrix):** Nondevelopment player has no experiment modes, visual comparison or playtest export controls.

**Artifact:** SHA-256 of `Mosselora.app/Frameworks/UnityFramework.framework/UnityFramework` from a release-candidate iOS Simulator build (arm64, Xcode Release configuration, unsigned). `global-metadata.dat` sha256 `df0c41d169acebbab5400792ed68771cc1096d788bb8742b5ca391a8147113d8`.

**How produced:** `EVERGROVE_BUILD_CHANNEL=release-candidate EVERGROVE_IOS_SDK=simulator` with `EvergroveBuild.Build` (`BuildOptions.None`, product "Mosselora", id `com.streamentry.evergrove`), then `xcodebuild -configuration Release -sdk iphonesimulator CODE_SIGNING_ALLOWED=NO ARCHS=arm64`, from a clean tree at the source SHA above.

**Source guard audit:** [release-candidate-audit-2026-11-23.md](../../../qa/release-candidate-audit-2026-11-23.md), plus the `tools/verify_evergrove_project.py` check "release-candidate dev-control source guard: OK (no unguarded dev-only identifiers)".

**Shipped-metadata scan:** `grep -c -a` on IL2CPP `global-metadata.dat` found 0 hits each for `DevQaTour`, `DevScreenshot`, `TextSpike`, `mosseloraQaTour` and "matched control". Positive controls: `EvergroveApp` 2, `GardenTransfer` 2, `BoardSize` 3, `TextScale` 2.

**Screenshots (iOS Simulator, iPhone 17 Pro, iOS 26.5):** [top](../../../qa/release-candidate-2026-11-23/01-rc-chapter1-top.png) and [settings area](../../../qa/release-candidate-2026-11-23/02-rc-settings-bottom.png). Visible controls: board, Undo, Guide me, Restart, Aa text size, board size, neighbors (Mira, Orin, Fen), Copy/Restore garden file, Gentle chime, and "Offline · No timer · No ads · No account". There is no play-mode switch, matched control, 2D/2.5D visual comparison, miniature motion, playtest export or language picker (only English is allowed in shipping builds).

**Does not prove:** a signed device build, App Store review readiness, or the residual inert research strings noted in the audit.
