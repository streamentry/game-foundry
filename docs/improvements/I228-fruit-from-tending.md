# I228 — Fruit from tending: food you can share

## Player
- The founder's list says family should be able to share food. My garden has
  apple and pear trees, but nothing to pick, and nothing small to bring a
  neighbour or send my daughter.
- Replaying a tree's puzzle pays only a few seeds. I like replaying the ones I'm
  fond of, but it feels like it doesn't matter.

## PM decision
- Fruit comes from play, never from time. The vision bans clock gates, streaks
  and energy, so there's no daily harvest and nothing rots.
- Every solve of a tree's puzzle picks 1 fruit, on the first solve and on
  replays, for grown and nursery trees alike. Buildings and flowers give none.
  The basket holds at most 99.
- Fruit is for giving, never for buying. Seeds and fruit never convert, so the
  seed economy (I206) is unchanged. Gifts move fruit, never create it.
- New food art: `icons-food.png`, generated with the Codex image tool in the
  `icons-small` style. Prompt in `art/prompts/food.txt`; provenance in
  `art/README.md`.

## Dev
- Rules and tests by a Sonnet subagent, reviewed by me (`Tests/FruitTests.cs`,
  test-first).
  - `WorldModel.Fruit`, `MaxFruit = 99`.
  - Fruit is picked in `SolveTree` and `SolveBought` for the tree family.
  - Snapshot fields `fruit`, `fruitGiven` and `lastFruitTo`, validated
    fail-closed.
  - Garden-file keys with a `matchesPreFruit` checksum matcher. Older files
    import with no fruit, and fruit can't ride under an older checksum.
- Art pipeline: `FOOD_ICONS` gives `food-pear`, `food-basket`, `food-jam` and
  `food-apple`.

## Result
- Core tests: 71,964 assertions pass at this step (216 new).
