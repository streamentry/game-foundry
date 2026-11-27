# I248 — Four seasons, by progress

## Player (founder question)
- "We should have 4 seasons features, right?" Today the game has three looks
  (summer, autumn, winter), only during the story. After the story the garden
  is summer forever, which is exactly where daily play happens.

## PM decision
- Four seasons, by progress and never by the clock. The vision says seasons
  "arrive through the player's progress", and a garden left alone waits
  unchanged.
  - The story walks spring, summer, autumn, winter: three streams a season.
  - The festival that ends it is summer.
  - After it, every four first solves (the same drops on the mist that open
    new river, I224) turn the year on. It keeps turning past the river's
    ceiling.
- Only the look and the neighbours' words change. Seeds, fruit and puzzles are
  the same in every season: nothing is gated, exclusive or missable.

## Dev
- Test-first (`SeasonTests`, red on the missing type): `Season` and
  `GardenSeason.ForStory(phase)`, `AfterStory(solved)` and `For(world)`.
  - Replays don't turn the year.
  - The cycle is steady over 400 solves and still turns past the ceiling.
  - Puzzle-only mode stays summer.

## Result
- Core tests: 343,488 assertions pass.
- b283 tours complete in portrait, landscape and 150% text (72 shots each).
  - `01`: the story opens in spring.
  - `19-*-home`: home in summer, autumn, winter and spring.
  - `17y`: summer melting into the coming autumn under the mist.
- Found in b282 and fixed in b283: a hard line where this season met the next
  under the mist. A painting that fades out at its foot (`FadeDownRawImage`)
  now melts one into the other over the same ground.
- Not yet seen by players aged 40+.
