# Takuzu Lab — verified gameplay engineering reference

**Not a production release or validated market opportunity.**

This Godot 4.7.2 project now includes:

- Offline, seed-reproducible **4×4 and 6×6** Takuzu puzzles, with every generated puzzle checked to have **exactly one solution**.
- A local **daily 4×4** puzzle based on the device's calendar date. No account, server, or synchronization; the device timezone defines the day.
- Free-play 4×4 and 6×6 puzzles, with contradiction detection, hints, Undo, Redo and Restart.
- Local progress saved after each move; saves are versioned and validated before resume. No ads, account or tracking SDK.
- A clean reference interaction design. Visual polish, real-device accessibility and performance **still require human/device tests**.

## Run

```bash
godot --headless --path games/takuzu-lab --editor --import --quit
godot --headless --path games/takuzu-lab --script res://tests/test_rules.gd
godot --headless --path games/takuzu-lab --script res://tests/test_factory.gd
godot --headless --path games/takuzu-lab --script res://tests/test_progress.gd
```

Open `games/takuzu-lab/project.godot` in Godot and press **F5** for the playable application.

## Integrity and limitations

`scripts/puzzle_factory.gd` draws full solutions using a seeded generator, removes clues only when the puzzle retains a single solution, and checks the final board again. Its row-pattern solver validates every completed row and column: balanced counts, no triples, and no duplicated completed rows or columns. CI runs independently across multiple seeds and both supported sizes. **Logical uniqueness does not establish enjoyable difficulty**; difficulty must be separately measured with players.

`scripts/progress_store.gd` uses local JSON via an atomic temporary-file rename. It refuses unsupported versions, invalid cells, changed clues and invalid daily seeds. This provides a reliable local-first baseline but does **not** prove crash recovery on every filesystem or OS; device tests are still needed.

The generator handles small 4×4 and 6×6 grids, and no difficulty claims, published daily puzzle service, iOS/Android signed binaries, monetization, analytics, or app-store listing are implied. **Do not launch this engineering reference as dozens of near-identical games.**

## Share with early testers

Use [the browser playtest workflow](../../.github/workflows/web-preview.yml) to produce a private downloadable WebGL2 artifact, then follow [the session protocol](../../docs/playtest.md). The build does not require Godot on the tester's device, but it does require a local HTTP server or an explicitly approved static host.



**New artifacts contain `serve.py`:** Extract the entire ZIP, open a terminal in the same directory as `index.html`, run `python3 serve.py`, and visit `http://127.0.0.1:8000/`. Do not open the HTML file directly with `file://`. If loading fails, run `python3 serve.py --smoke` and follow [the browser NetworkError guide](../../docs/web-networkerror.md). Actions artifacts are downloads, not live websites.

## Next real validation

1. Test on a physical iPhone and Android device, including safe areas, screen readers, low-memory and suspended-app recovery.
2. Test level difficulty, clarity and frustration with at least five real players.
3. Measure App Store demand and competition before deciding whether Takuzu merits a standalone product.
