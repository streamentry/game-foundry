# I107 — Hub recalls a different past memory each visit

Date: 2026-11-24. Branch: feat/mosselora-painted-ui (PR #76).

## Aim: daily appeal (the weakest AI-review score, 6)

A Haiku sub-agent read `games/evergrove/vision.md` and proposed three "come back tomorrow" ideas. Each had to be allowed by the vision, with no art and no server.

| Haiku idea | Decision |
|---|---|
| 1. Echo line: a neighbour recalls one of your choices on the hub | It already existed (I30, I73), but only ever showed the **latest** memory, so the hub said the same thing on every visit. **Built the missing part.** |
| 2. "Mira and Fen both remember the willow…" | **Rejected.** Each landmark has exactly one keeper (tree→Mira, bridge→Orin, flowers→Fen), so this would invent a memory. The vision forbids that (§4.3: lines "come from real save state and are never invented"). |
| 3. Replay a past stream from the journal | **Deferred.** It is real scope: core rules and the M4 Living-world vs Puzzle-only comparison would need it kept fully separate. |

## Change (b91)

- **Core `HubRecall.Next(memoryCount, lastCount, lastIndex)`:** the newest memory is recalled first. After that, each launch steps back one memory and wraps to the newest. It is driven by launches and saved choices only: no clock, no streak, no "you were away" (vision §4.2, "a garden left alone waits unchanged"; §4.3, they "don't plead for attention").
- **Hub:** the memory painting and the keeper line follow the recalled memory, using `LocalizedVillage.MemoryAt` (so the second bridge gets its own I90 line). "Count:index" is stored once per launch in PlayerPrefs, locally. The next-stream row keeps showing the **latest** landmark, because that is what shapes the next puzzle.
- The in-game "remembered choice" line is unchanged (latest).
- No new text, keys or art.

## Tests (written first, failed, then passed)

`HubRecallTests` checks:

- No memory → none.
- First → newest.
- A new memory since the last launch → newest.
- Stepping back and wrapping around.
- A single memory stays.
- A stale stored index → newest.
- Five launches visit all five memories.

The C# tests pass with 40,822 assertions.

## Evidence (live, iOS Simulator)

I made a three-memory garden (tree, bridge, flowers) from the core rules and loaded it through the game's own "Restore from garden file", allowing the iOS paste prompt and confirming the replace. Then I relaunched four times. `I107-hub-recalls-the-past.png`:

1. **Launch 1:** Fen and the wildflowers (newest).
2. **Launch 2:** Orin and the bridge.
3. **Launch 3:** Mira and the tree.
4. **Launch 4:** back to Fen.

On every launch, the painting and line agree and "🌼 Flowing Home" stays.

The verifier passes, and the b91 QA tour captured all 20 screens.

## Still unverified

- Whether recalled memories actually bring players aged 40+ back is exactly what m3-session-kit measures; no players yet.
- Codex is still out of credits.
