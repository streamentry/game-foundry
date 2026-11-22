# Kakuro Compass — guided crossing sums

**Status:** original Godot **4.7.2** Kakuro prototype; not an App Store release or validated search opportunity. This is the [GF-001 search-led proof-of-play candidate](../../docs/research/first-game-decision-2026-11-21.md), **not** a production greenlight.

## Play

1. Open `games/kakuro-compass/project.godot` in Godot 4.7.2 stable and press **F5**.
2. Tap a light square. Enter digits **1–9** from the large keypad (or a physical keyboard). Each arrow sum must match its adjacent uninterrupted run; numbers **cannot repeat within that run**.
3. The **Compass inspector** stays visible below the grid: the selected cell's **across** target and possible digits; its **down** target and possible digits; and the local intersection of both. These are **locally feasible values**, not a solved puzzle or a claim of global logical necessity.
4. **Undo**, **Erase**, and **Restart** recover mistakes. A conflict is explained; a mistaken digit isn't silently overwritten. Finish all crossing sums to unlock the next of **eight authored 5×5 puzzles**.
5. Current entries and unlocked progress save locally in versioned, validated `user://` JSON.

If the exported game is hosted over HTTP(S), `?level=4` opens that specific board and **Copy this challenge** prepares its URL. A downloadable ZIP and a localhost URL are **not** a public playable site.

## Test

```bash
godot --headless --path games/kakuro-compass --editor --import --quit
godot --headless --path games/kakuro-compass --script res://tests/test_rules.gd
godot --headless --path games/kakuro-compass --script res://tests/test_progress.gd
godot --headless --path games/kakuro-compass --script res://tests/test_scene.gd
```

Each board is checked for **exactly one solution**, clue/run validity, across/down constraints and inspector truth. Scene smoke tests check keypad, undo, progress and victory with a real fixture solution. The independent [Web Playtest workflow](../../.github/workflows/kakuro-compass-preview.yml) exports a single-threaded WebGL2 ZIP. Once extracted, run `python3 serve.py` and open `http://127.0.0.1:8000/`. `python3 serve.py --smoke` confirms the four browser assets fetch correctly. No public site or native signed build is created.

## Validation boundary

The inspector tests a specific *visible intersection* hypothesis. Existing competitors have established hints and beginner experiences; no preference advantage is established. No physical iOS/Android QA, accessibility sign-off, player-45+ retention, App Store keyword evidence, pricing or organic distribution has been measured. Preserve the [existing Kakuro decision gates](../../docs/research/first-game-decision-2026-11-21.md).

See [independent design specification](DESIGN.md).
