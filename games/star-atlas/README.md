# Star Atlas — original, independent logic playtest

**Status: research prototype (Game Foundry #9, GF-005), not a mobile release or validated business opportunity.** Built with Godot **4.7.2 stable**. No backend, ad SDK, account, analytics, purchases, or timed pressure.

## Play

Place exactly one star in each row, column and labeled connected region, without allowing stars to touch even diagonally.

**Controls:** Tap a square to cycle Star → Dot → Blank; Undo and Restart allow mistake recovery. There are **six (three original connected-region layouts plus three rotated variations) reproducible small boards**, with local-only saved progress and level unlocking on a rules-verified completion. A shared browser game can open a specific puzzle with `?level=2`; only genuine HTTPS hosting makes those links usable outside the developer's computer.

## Run and test

Open `games/star-atlas/project.godot` in the Godot 4.7.2 standard editor and press **F5**. Or run:

```bash
godot --headless --path games/star-atlas --editor --import --quit
godot --headless --path games/star-atlas --script res://tests/test_rules.gd
godot --headless --path games/star-atlas --script res://tests/test_progress.gd
godot --headless --path games/star-atlas --script res://tests/test_scene.gd
```

**Correctness checked:** All three independent regions are connected; each variant has one legal constellation; no diagonal contact. Tests also attempt gameplay via the actual scene, save/resume and malformed-save rejection. Puzzle reference answers are for testing only, not player-validated ratings of difficulty.

The [Star Atlas Web Playtest workflow](../../.github/workflows/star-atlas-preview.yml) exports a **downloadable Godot single-threaded WebGL2 ZIP**, *not* public hosting. Download the complete artifact from its successful Actions run, extract the files, and run `python3 serve.py` from inside the extracted directory. Visit **http://127.0.0.1:8000/** on the same computer. `python3 serve.py --smoke` verifies the four assets over HTTP. Direct opening through `file://` can cause NetworkError.

## Validation boundaries

**Testable hypothesis:** Can letter-labeled regions and untimed no-contact logic produce legible, satisfying short play? **Strongest objection:** Rotations are equivalent layouts, and a three-base-board catalog is too small for sustained play. Neither source code nor a successful web export proves an advantage versus direct incumbents, official Apple Ads keyword popularity, adult-45+ retention, paid conversion or global adoption. Real iOS/Android device QA, VoiceOver/TalkBack, 200% text, contrast checks, and actual user studies are pending.

See [independent visual identity](DESIGN.md) and [comparison/stop rules](../../docs/research/next-five-game-experiments-2026-11-22.md).
