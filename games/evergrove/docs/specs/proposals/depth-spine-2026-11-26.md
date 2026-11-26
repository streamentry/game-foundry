# The depth spine: one loop, many layers

**Status:** proposal (2026-11-26) from the I189–I205 iteration run, written against the founder's goal of a game that is easy to play, deep to master, played every day by adults 40+, alone or with family. It ties every next iteration to one loop, so features add up instead of piling up. Vision rules still win: no streaks, no energy, no clock gates, no paid outcomes, the core stays complete offline.

## The loop (one sentence a player can feel)

> **Restore water → grow something → it holds a puzzle → solving earns seeds → seeds grow new things → new things hold new puzzles, and the river grows downstream.**

Every layer below either feeds this loop or makes it more personal. Anything that doesn't is cut.

## Layer 1 — the river (story, built)
- Twelve authored streams per garden; your choice at each changes the next board and is remembered.
- The world is one endless vertical river (I192): a new stretch opens every three streams, in mist until reached. The format already allows up to five stretches, so the river can grow further later (new gardens, new rivers) without breaking any save.

## Layer 2 — things hold puzzles (built: I203, I204)
- Every grown thing holds its own seeded puzzle, the same on every device, with exactly one answer.
- **Puzzle kind follows the family of the thing.** That gives variety without menus, and the rule is logical: water for trees (built); then gentle picture puzzles for the others:
  - **Flowers → Pairs:** turn over painted cards and find matching pairs from your own garden. Memory, never a timer.
  - **Buildings → Mosaic:** a neighbour's memory painting, cut into tiles; swap tiles to restore the picture (picture-swap jigsaw).
  - **Later candidates:** spot-what-changed between two paintings of your garden; arranging stepping stones.
  - All are familiar, image-based (no language needed), and calm. They're validated by playtests, not by assumption.
- **Difficulty tiers** come from the stream a thing grew in (early, middle, late), and per kind from concrete knobs: board size, pairs count, tiles already right.

## Layer 3 — seeds and the nursery (seeds built in I204; nursery next)
- **Earn:** first solve 10/20/40 by tier, replay 2/4/8. Nothing else gives seeds. No purchase, no clock.
- **Spend: the nursery** (in the basket). New plant kinds unlock in order: you must own a kind from one row before the next row opens (her 50 → 100 → 200 idea).
  - Row 1, about 50 seeds: everyday plants, everyday puzzles.
  - Row 2, about 100 seeds: "a little harder" puzzles, more beautiful plants.
  - Row 3, about 200 seeds: "harder" puzzles, the most beautiful plants.
- **Each bought plant is a new thing with its own puzzle.** So spending creates new play, and new play earns seeds. Prices stay simple round numbers; a price never changes once seen.
- Art comes from the existing catalogue of the later gardens, so it's prettier with no new painting needed at first.
- **Sink, not grind:** a first solve always pays more than any replay, so the best move is always "grow something new", never "farm one tree".

## Layer 4 — neighbours who notice (the companion, offline first)
- The distinctive promise is *the place remembers you truthfully*. The companion is that promise made warm, and **subtle**:
  - Neighbours notice what you actually did: which things you tend, what you solved, how long since your last visit (without blame), and what you planted near their homes.
  - They speak rarely, one line, by their portrait on the map, and only from true facts in your save. Never fake intimacy, never guilt, never "I missed you".
  - Offline and deterministic: a rules table of facts → lines, testable like everything else.
- **A real AI (language model) companion is a separate founder decision:** it needs a network or an on-device model, a privacy and cost review, safety for child co-players, and the store's child-audience declaration. Until then, the noticing neighbours are the companion.

## Layer 5 — family (opt-in, no server first)
- **Visit:** open a relative's garden file (the existing garden file) to walk their river read-only.
- **Gifts:** leave a seed parcel, a cutting of one of your plants, or a short letter on their map. These travel back inside a small, checksummed "parcel" file sent by any messaging app.
- **Help:** solve one of their things' puzzles while visiting; it blooms in their garden when they open the parcel.
- The opt-in live family circle (backend plus sign-up, decided 2026-11-25, not built) later carries the same parcels automatically. Same rules, different delivery.

## Simple economics, checked by tests
- One currency (seeds). Fixed prices. Rewards from play only.
- Invariants to test: seeds never go negative; a purchase is atomic; first solve > replay at every tier; every row is reachable by first solves alone; gifts move seeds and never create them.

## Order of the next iterations
1. Nursery: buy a plant with seeds; the rows unlock in order; bought plants hold puzzles. (Core TDD, then UI.)
2. Pairs puzzle for flowers.
3. Mosaic puzzle for buildings.
4. Neighbours notice (first five truthful lines).
5. Golden-ring and bloom polish; nursery art per row.
6. Family visit and parcel (file-based).

## Open decisions for the founder
1. A real language-model companion: yes/no and where it runs (Layer 4).
2. Family circle backend: provider, sign-up, and data retention (vision §1a).
3. Nursery prices (50/100/200 proposed) and whether bought plants can be given to family.
