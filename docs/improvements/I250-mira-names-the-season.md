# I250 — Mira names the season

## Player
- After the story, the map turned gold one day. Why?

## PM decision
- When tending opens new bank (I225's line), Mira names the season it brought,
  truly:
  - "The river runs further than it ever did. Autumn came down with it. The
    new bank is gold already."
  - Spring: "...all blossom."
  - Winter: "...frosted."
  - Summer: "...waiting for roots."
- The cause becomes legible in one sentence: tending opens river, and river
  turns the year.

## Dev
- Test-first (`SeasonTests`, red): `NeighbourNotice.SeasonCame` uses
  `GardenSeason.AfterStory`.

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
