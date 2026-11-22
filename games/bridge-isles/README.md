# Bridge Isles — original, independent logic playtest

**Status: research prototype (Game Foundry #10, GF-007), not a mobile release or validated business opportunity.** Built with Godot **4.7.2 stable**. No backend, ad SDK, account, analytics, purchases, or timed pressure.

## Play

Connect visible neighboring numbered islands with zero, one or two straight bridges, without crossings, and keep the network connected.

**Controls:** Tap one island and another to cycle bridge strength; use large island-letter controls as a nonprecision alternative. There are **six reproducible small boards**, with local-only saved progress and level unlocking on a rules-verified completion. A shared browser game can open a specific puzzle with `?level=2`; only genuine HTTPS hosting makes those links usable outside the developer's computer.

## Run and test

Open `games/bridge-isles/project.godot` in the Godot 4.7.2 standard editor and press **F5**. Or run:

```bash
godot --headless --path games/bridge-isles --editor --import --quit
godot --headless --path games/bridge-isles --script res://tests/test_rules.gd
godot --headless --path games/bridge-isles --script res://tests/test_progress.gd
godot --headless --path games/bridge-isles --script res://tests/test_scene.gd
```

**Correctness checked:** Exact island degrees, no crossings, global connectivity, each authored board has one bridge-strength solution. Tests also attempt gameplay via the actual scene, save/resume and malformed-save rejection. Puzzle reference answers are for testing only, not player-validated ratings of difficulty.

The [Bridge Isles Web Playtest workflow](../../.github/workflows/bridge-isles-preview.yml) exports a **downloadable Godot single-threaded WebGL2 ZIP**, *not* public hosting. Download the complete artifact from its successful Actions run, extract the files, and run `python3 serve.py` from inside the extracted directory. Visit **http://127.0.0.1:8000/** on the same computer. `python3 serve.py --smoke` verifies the four assets over HTTP. Direct opening through `file://` can cause NetworkError.

## Validation boundaries

**Testable hypothesis:** Do visual bridge lines and direct letters make Hashi navigation easier on smaller phones? **Strongest objection:** Hashi is mature, and rendering bridges is not a defensible distribution advantage. Neither source code nor a successful web export proves an advantage versus direct incumbents, official Apple Ads keyword popularity, adult-45+ retention, paid conversion or global adoption. Real iOS/Android device QA, VoiceOver/TalkBack, 200% text, contrast checks, and actual user studies are pending.

See [independent visual identity](DESIGN.md) and [comparison/stop rules](../../docs/research/next-five-game-experiments-2026-11-22.md).
