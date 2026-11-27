# I252 — The coming season, in the mist

## Player
- The drops on the mist tell me how close more river is (I224), but not what it
  will bring.

## PM decision
- After the story, the stretch waiting in the mist is painted in the season it
  will bring, so you see autumn gold under the mist while it's still summer.
- The same principle as the journey stones: the year ahead is visible, never
  timed.

## Dev
- `GardenMapView.Build(..., teaserPlot)`.
- `EvergroveApp.NextSeasonPlot()` uses
  `GardenSeason.AfterStory(solved + drops left)`.

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
