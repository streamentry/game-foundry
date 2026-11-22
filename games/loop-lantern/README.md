# Loop Lantern — original, independent logic playtest

**Status: research prototype (Game Foundry #11, GF-008), not a mobile release or validated business opportunity.** Built with Godot **4.7.2 stable**. No backend, ad SDK, account, analytics, purchases, or timed pressure.

## Play

Draw a single closed nonbranching Slitherlink loop using cell clues that specify how many surrounding edges are selected.

**Controls:** Select a square by tapping, then press one of four large Top/Right/Bottom/Left edge buttons; Undo, Restart and Next are available. There are **six reproducible small boards**, with local-only saved progress and level unlocking on a rules-verified completion. A shared browser game can open a specific puzzle with `?level=2`; only genuine HTTPS hosting makes those links usable outside the developer's computer.

## Run and test

Open `games/loop-lantern/project.godot` in the Godot 4.7.2 standard editor and press **F5**. Or run:

```bash
godot --headless --path games/loop-lantern --editor --import --quit
godot --headless --path games/loop-lantern --script res://tests/test_rules.gd
godot --headless --path games/loop-lantern --script res://tests/test_progress.gd
godot --headless --path games/loop-lantern --script res://tests/test_scene.gd
```

**Correctness checked:** Unique single-loop solution per clue board, exact local counts, degree 0/2 vertices, one connected component. Tests also attempt gameplay via the actual scene, save/resume and malformed-save rejection. Puzzle reference answers are for testing only, not player-validated ratings of difficulty.

The [Loop Lantern Web Playtest workflow](../../.github/workflows/loop-lantern-preview.yml) exports a **downloadable Godot single-threaded WebGL2 ZIP**, *not* public hosting. Download the complete artifact from its successful Actions run, extract the files, and run `python3 serve.py` from inside the extracted directory. Visit **http://127.0.0.1:8000/** on the same computer. `python3 serve.py --smoke` verifies the four assets over HTTP. Direct opening through `file://` can cause NetworkError.

## Validation boundaries

**Testable hypothesis:** Can explicit side controls remove precision-drawing friction while retaining satisfying deduction? **Strongest objection:** The discrete side controls may feel slower than direct tracing, and classic competitors already exist. Neither source code nor a successful web export proves an advantage versus direct incumbents, official Apple Ads keyword popularity, adult-45+ retention, paid conversion or global adoption. Real iOS/Android device QA, VoiceOver/TalkBack, 200% text, contrast checks, and actual user studies are pending.

See [independent visual identity](DESIGN.md) and [comparison/stop rules](../../docs/research/next-five-game-experiments-2026-11-22.md).
