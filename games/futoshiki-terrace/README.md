# Futoshiki Terrace — original, independent logic playtest

**Status: research prototype (Game Foundry #8, GF-004), not a mobile release or validated business opportunity.** Built with Godot **4.7.2 stable**. No backend, ad SDK, account, analytics, purchases, or timed pressure.

## Play

Fill a 4×4 Latin grid using 1–4 exactly once in each row and column, while satisfying directional inequalities.

**Controls:** Tap an editable number cell, then one of four large digit buttons. Undo, Erase, Restart and Next are visible. There are **six reproducible small boards**, with local-only saved progress and level unlocking on a rules-verified completion. A shared browser game can open a specific puzzle with `?level=2`; only genuine HTTPS hosting makes those links usable outside the developer's computer.

## Run and test

Open `games/futoshiki-terrace/project.godot` in the Godot 4.7.2 standard editor and press **F5**. Or run:

```bash
godot --headless --path games/futoshiki-terrace --editor --import --quit
godot --headless --path games/futoshiki-terrace --script res://tests/test_rules.gd
godot --headless --path games/futoshiki-terrace --script res://tests/test_progress.gd
godot --headless --path games/futoshiki-terrace --script res://tests/test_scene.gd
```

**Correctness checked:** One valid solution per authored 4×4 board; no row/column duplicates; all inequalities true. Tests also attempt gameplay via the actual scene, save/resume and malformed-save rejection. Puzzle reference answers are for testing only, not player-validated ratings of difficulty.

The [Futoshiki Terrace Web Playtest workflow](../../.github/workflows/futoshiki-terrace-preview.yml) exports a **downloadable Godot single-threaded WebGL2 ZIP**, *not* public hosting. Download the complete artifact from its successful Actions run, extract the files, and run `python3 serve.py` from inside the extracted directory. Visit **http://127.0.0.1:8000/** on the same computer. `python3 serve.py --smoke` verifies the four assets over HTTP. Direct opening through `file://` can cause NetworkError.

## Validation boundaries

**Testable hypothesis:** Do adults who enjoy logic puzzles understand inequalities faster with persistent local relationship explanations? **Strongest objection:** Incumbent Futoshiki apps already cover the same rules; an inspector could be unnecessary. Neither source code nor a successful web export proves an advantage versus direct incumbents, official Apple Ads keyword popularity, adult-45+ retention, paid conversion or global adoption. Real iOS/Android device QA, VoiceOver/TalkBack, 200% text, contrast checks, and actual user studies are pending.

See [independent visual identity](DESIGN.md) and [comparison/stop rules](../../docs/research/next-five-game-experiments-2026-11-22.md).
