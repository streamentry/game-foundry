# Quiet Shot — second Game Foundry playtest

An original, **playable** Godot 4.7.2 project exploring one-shot spatial prediction. Eight authored board layouts; no timer, account, ads, SDK, or backend. This is **not** an approved commercial release.

## Play

1. Open `games/quiet-shot/project.godot` in standard Godot **4.7.2** and press **F5**.
2. Touch the board to choose your initial direction (or press large ±5° / ±1° adjustment buttons).
3. Hit **Fire once**. The ray begins at the marked dot, passes through round targets, reflects off fixed blue mirror bars, and stops when all targets are cleared, it leaves the board, or the four-reflection budget is exhausted.
4. A hit fills a target with a checkmark. The actual trail and **X of Y targets** feedback appear. Tap **Adjust & retry** without a penalty timer. Completing a board unlocks the next; eight small boards are provided. Previous boards remain playable.

Progress saves locally in versioned `user://` JSON, guarded against malformed values. A hosted web build accepts `?level=4` to open a specific board. The **Copy challenge** button includes that link when the build is hosted via HTTP(S). A localhost or downloaded artifact URL is not publicly shareable.

## Verify

```bash
godot --headless --path games/quiet-shot --editor --import --quit
godot --headless --path games/quiet-shot --script res://tests/test_ray.gd
godot --headless --path games/quiet-shot --script res://tests/test_progress.gd
godot --headless --path games/quiet-shot --script res://tests/test_scene.gd
```

Physics tests check actual ray–segment collisions, hit/miss cases, board solvability, deterministic generation and small angle perturbations; the scene smoke test validates interaction and persistence. All eight authored trajectories are independent of a scripted result. The project is pinned to the repo's separate Godot 4.7.2/GDScript standard and can be run offline.

A [GitHub Actions browser-playtest workflow](../../.github/workflows/quiet-shot-preview.yml) exports a WebGL2 ZIP artifact, **not** a public site. Download the resulting `quiet-shot-web-playtest` artifact, extract and serve via `python3 -m http.server 8000`; visit `http://localhost:8000/` in a WebGL2-capable browser (not a local `file://` URL). Mobile native export and physical-device QA are not complete.

See [the ranked strategy, competitor caveats and stop conditions](../../docs/research/global-45plus-top-three-games-2026-11-21.md) and [this game's design](DESIGN.md). This does **not** constitute measured 45+ interest, app-store demand or retention.


**New artifacts contain `serve.py`:** Extract the entire ZIP, open a terminal in the same directory as `index.html`, run `python3 serve.py`, and visit `http://127.0.0.1:8000/`. Do not open the HTML file directly with `file://`. If loading fails, run `python3 serve.py --smoke` and follow [the browser NetworkError guide](../../docs/web-networkerror.md). Actions artifacts are downloads, not live websites.
