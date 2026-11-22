# Quiet Camp — a Tents & Trees logic playtest

**Status:** an original independent **Godot 4.7.2** prototype. This tests the existing **GF-006** search-research hypothesis; no production decision, market fit or App Store search demand has been verified.

## How to play

1. Open `games/quiet-camp/project.godot` in the Godot 4.7.2 standard editor and press **F5**.
2. Put **exactly one tent next to each fixed pine tree** (up, down, left or right). Tents must not touch each other in any of the eight directions, **including diagonals**.
3. The top numbers give the number of tents in each **column**; the right-side numbers give the tents in each **row**. Both must be satisfied. A tent may be adjacent to more than one tree, provided there is a valid one-to-one pairing.
4. Tap any blank cell to cycle **tent → grass mark → blank**. Trees cannot be edited. Use **Undo** and **Restart** freely; no timer, score penalty, account, tracking or ads.
5. Solve a board to unlock the next of **eight uniquely-solvable, original 5×5 / 6×6 clearings**. Your current markings and unlocked progress are saved locally.

The game validates actual row/column constraints, non-touching and a perfect one-to-one tree/tent matching; it does **not** compare your board to a pre-scripted image. The backtracking solver counts **distinct tent placements**, not multiple equivalent tree matchings.

## Run checks

```bash
godot --headless --path games/quiet-camp --editor --import --quit
godot --headless --path games/quiet-camp --script res://tests/test_rules.gd
godot --headless --path games/quiet-camp --script res://tests/test_progress.gd
godot --headless --path games/quiet-camp --script res://tests/test_scene.gd
```

Tests cover 8 unique authored boards, valid pairing with multiple-adjacent trees, a false-positive arrangement that matches all line totals but has **no** valid tree pairing, diagonal contact, saved progress and actual tapping, undo, win and next-screen behavior.

[Quiet Camp Web Playtest](../../.github/workflows/quiet-camp-preview.yml) exports a downloadable ZIP, **not a public website**. Extract and run `python3 serve.py`; visit `http://127.0.0.1:8000/` on the same computer. The ZIP includes `serve.py --smoke` to check complete binary HTTP delivery. Hosted browser deployments can use `?level=3` links; no production domain has been deployed.

## Limits

This is a known genre, so mechanic familiarity is not a novelty or demand advantage. Real-life puzzle difficulty, first-play comprehension, target 45+ relevance, iOS/Android accessibility and screen-reader support, store-specific organic demand, voluntary D1/D7 return and paid conversion are **unmeasured**. Compare against an incumbent on actual player devices before scaling content.

See the [independent design spec](DESIGN.md), [next-two decision brief](../../docs/research/next-game-experiments-2026-11-22.md), and [playtest protocol](../../docs/playtest.md).
