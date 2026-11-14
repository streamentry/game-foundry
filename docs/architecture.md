# Architecture — game portfolio, not template spam

## Decision record: 2026-11-15

- **Engine:** Godot 4.7.2 stable, standard GDScript distribution.
- **Projects:** `games/<slug>/project.godot` is the unit of builds, app signing, assets and release cadence.
- **Rendering:** 2D Godot Controls for grid/word puzzles; use CanvasItem/Node2D for drag-heavy or animated play.
- **Storage:** offline-first. Start with versioned local JSON in `user://`, atomic writes and migrations; no cloud by default.
- **Backend:** add only for an explicit cross-device, UGC, leaderboard or live-content requirement.
- **Monetization:** initially interface-only; don't choose AdMob/RevenueCat/Firebase plugins until native iOS and Android compatibility and privacy reviews pass.
- **Analytics:** write a privacy-aware event schema before integrating an SDK. At minimum: first_play, puzzle_start, puzzle_complete, hint_used, session_returned and crash; only collect what is necessary.
- **iOS:** requires macOS/Xcode, app ID, provisioning profile, signing and export templates. Linux CI validates import and logic, **not App Store readiness**.

## Two dependency directions

```text
game-specific scenes and UX -> pure game rules -> validated content and test fixtures
mobile wrappers and SDK adapters -> independently versioned ports
```

Keep gameplay, hints, generation, difficulty and scoring independent of engine scene state wherever practical. Don't expose a false promise of painless engine switching: Godot node APIs and renderer still create migration cost.

## Reuse policy

Do **not** force a common scene hierarchy or one design theme on all titles. Extract shared code only when two independently shipped game projects need the same behavior. Then vendor a versioned `addons/foundry_*` package **inside each project**, record its hash/version and maintain backwards-compatible interfaces. Independent builds prevent one plugin upgrade from breaking 30 apps.

## Test strategy

1. Unit: deterministic rules, constraints, unique solutions, edge cases.
2. Content: replay every generated seed; classify difficulty only after verified solver work.
3. Integration: import scene, run game, persistence migrations, plugin smoke tests.
4. Device: tap targets, notch, text scale, pauses, low memory, airplane mode, OS upgrade.
5. Business: App Store conversion, day-1/day-7 retention by organic cohort, organic impressions and revenue **measured**, never inferred from ratings.

## Security and policy

Treat downloaded data and generated assets as untrusted. Never let agents push production builds, buy ads or publish apps without explicit authorization. Avoid cloning proprietary levels or brand identifiers. Apple App Review 4.3 prohibits indistinguishable mass-produced variants.

Sources:
- https://godotengine.org/download/archive/
- https://docs.godotengine.org/en/stable/tutorials/editor/command_line_tutorial.html
- https://developer.apple.com/app-store/review/guidelines/
