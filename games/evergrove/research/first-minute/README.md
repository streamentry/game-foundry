# Evergrove · First Minute

**A playable browser research slice, not an exported Unity game.**

Open [index.html](index.html) directly in a modern browser. The whole first stream works offline; no server, build step, assets, account, analytics, or internet connection is required.

This is the **smallest honest experiment** for the product claim: *one water puzzle → observable restoration → one village choice → an illustrated consequence.* We should test this before manufacturing more levels, AI villagers, meta-economies or elaborate settings.

## Exactly what is implemented

- The **identical** Unity chapter-one authored fixture (`First Bend`): layout `LIEIELLIL`, exact initial and solution orientations, clockwise N/E/S/W ports, full graph reachability, leak detection and completion rules.
- Tap or keyboard-activate any channel. **The board comes before the village illustration in document, visual and keyboard order** to avoid burying the first move beneath a hero on compact phones. Only genuinely spring-connected channels become teal; disconnected pipes stay earthen. The upstream creek extends and retracts based on actual graph connectivity.
- Undo, Start over, and three-stage gentle help: first the row, then the tile, then rotate one tile to its intended orientation. No hints are sold or imposed.
- After the leak-free route is complete, reveal the restored village with a deliberate (reduced-motion-aware) scroll and keyboard focus, then three choices. The illustration changes **only after** the player chooses a tree, bridge or flowers; focus returns to the illustrated world so the visual change comes before explanatory text.
- A one-click **optional** local session note appears only after the choice. It collects no identity and copies only if clicked. The first-tap/solve times and rotation/hint/undo/restart totals include **all attempts before Replay**, not only the last successful retry. No remote requests, local storage, cookies or server are used.

### Important limitations

- **One puzzle, not 12 streams.** The first choice is illustrated but is **not persisted or used to generate the next board**; that remains the Unity implementation's responsibility.
- English research copy only. It does not certify any of the 29 other draft locales or claim a real screen-reader audit.
- Web layout and C# Unity UI are separate implementations. This prototype **cannot** satisfy BLD-001..007, SAVE-001/002, M1/M2 device execution or M5/M6 store readiness.
- Browser first-minute observations do **not** establish D1/D7 retention, willingness to pay or the incremental impact of NPC/world systems. They are formative comprehension signals only. CSS/DOM order tests are **not** a pixel-level 320×568 mobile-browser certificate.

## Run research, not a product demo theater

Invite **8–12 relevant adults (including 45+)** to play without instruction other than “see what you think.” Do not describe the tap-to-rotate mechanism, where the garden is, what the art should mean or which choice to select. Ask **after** they finish:

1. What did the water do when you turned a pipe? Why?
2. What changed in the illustration after solving? Did you notice without prompting?
3. What changed when you chose a tree, bridge or flowers?
4. Would you voluntarily play the next stream? What, specifically, would make you return?

Record **each** observation separately: device/browser, whether the first tile was visible without scrolling at 320×568/375×667/430×932 equivalent viewports, seconds to first deliberate tap, hints requested, **restarts and abandoned attempts**, whether they solved, whether the automatic jump to restored creek made sense, whether they noticed the *final* restoration unprompted, and what confused them. Document noncompleters, not just the happy path. Get consent for any recording; do not capture names in the repository.

**Kill/simplify signal:** If several players confuse the initial partially wet inlet with an already finished creek, cannot infer rotation, or never notice the world change, revise the first screen **before adding more puzzles**. No arbitrary percentage alone is proof for 8–12 sessions.

## Verification

```bash
node --test games/evergrove/research/first-minute/game.test.cjs
```

Run from the repository root with Node 22. Unit tests compare the authored fixture directly against Unity C# source and exercise water propagation, undo, hint stages, completion, choices, layout source order and no-telemetry contracts.

**Chromium viewport smoke (separate from Unity):**

```bash
python -m pip install playwright==1.56.0
python -m playwright install chromium
python tools/verify_evergrove_browser_viewports.py
```

This executable check renders the actual HTML/CSS/JS in an **offline headless Chromium browser** at 320×568, 375×667 and 430×932 CSS viewports. It verifies that a usable first tile is in the **initial viewport** without scrolling, the screen has no horizontal overflow, 4→5→4 water/Undo, six-turn solution, creek reveal, tree choice/focus, and honest abandoned-attempt notes. It saves screenshots under ignored `artifacts/evergrove-first-minute/` for a human to inspect. GitHub CI retains screenshots for 7 days. This is stronger than a DOM order assertion, but it is still **automated browser evidence**.

**Still unverified:** physical iPhone Safari/Android Chrome, screen-reader semantics, contrast and actual human response. Test real devices, zoom/large-font and reduced-motion modes before treating the browser slice as suitable for broad user research.

Authoring intent: *make the first meaningful action delightful, not make the introduction longer.*
