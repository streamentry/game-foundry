# One Path — playable Euler-trail experiment

**Status: independent Godot 4.7.2 prototype for user research, not a commercial release.** Eight original graph puzzles, persistent offline progress, no login/ads/backend.

## Play

1. Open `games/one-path/project.godot` in Godot **4.7.2 stable** and press **F5**.
2. **Use each visible connection exactly once.** You may revisit the same dot, but a line can be used only once. Where drawn lines cross without a **dot**, you cannot change direction.
3. Tap a dot, drag along connections, or press a large **Dot A–H** button. These letter buttons provide an accessible alternative to precise gestures.
4. Open trails highlight valid starting endpoints. **Undo** removes one selected step; **Restart** begins again. A dead end is recoverable, without a timer.
5. Finish each board to unlock the next one. Earlier boards can be revisited. Your current route and unlocked progress save locally and resume on restart.

When the exported Web build is hosted on an actual HTTP(S) site, `?level=4` opens puzzle four directly for a challenge; the **Copy challenge** control prepares that link. A localhost address or GitHub Actions ZIP is not a public play link.

## Verify

```bash
godot --headless --path games/one-path --editor --import --quit
godot --headless --path games/one-path --script res://tests/test_rules.gd
godot --headless --path games/one-path --script res://tests/test_progress.gd
godot --headless --path games/one-path --script res://tests/test_scene.gd
```

Graph checks validate Eulerian connectivity and degrees, every original level's known solution, no repeated edges, forbidden starts, safe crossings and undo. Persistence checks verify partial and full resumes with corrupt JSON recovery. Scene smoke checks exercise accessible controls and real win-based unlocking.

[One Path Web Playtest](../../.github/workflows/one-path-preview.yml) creates a **downloadable** Godot 4.7.2 single-threaded WebGL2 artifact. Extract `one-path-web-playtest`, serve its directory using `python3 -m http.server 8000` and open `http://localhost:8000` in a WebGL2-capable browser. Avoid `file://`, which cannot load the WASM export correctly. The workflow does not host publicly or sign Android/iOS builds.

See [independent visual design](DESIGN.md), [global three-game research strategy](../../docs/research/global-45plus-top-three-games-2026-11-21.md) and the [playtest protocol](../../docs/playtest.md). The game still requires physical 45+ usability sessions and an acquisition/retention experiment before any shipping decision.
