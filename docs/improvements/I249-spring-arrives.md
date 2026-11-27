# I249 — Spring arrives

## Player
- There's no spring. A new garden starts in high summer, which feels like
  arriving halfway through the year.

## PM decision
- A spring grade of the garden map, banners and stones:
  - cooler, fresher mint greens and a soft morning haze, so it reads apart from
    the yellow summer at a glance;
  - on the map, a light scatter of pink and white blossom petals.
- A new garden now begins in spring.
- The story's journey stones wear spring mint (3.8:1 on paper).
- A relative's garden, visited, shows its own season, so families see each
  other's year.
- Why a grade and not a new painting: the garden map's land and water mask
  must line up exactly with the painting. A grade keeps every pixel of the
  river where it is.

## Dev
- Art pipeline:
  - `spring_grade(image, blossom)` produces `garden-map-spring` (with petals),
    `banner-spring-dry`, `banner-spring-flowing` and `stone-spring`;
  - the petal scatter uses a fixed seed and skips water.
- `PaintedVillage`:
  - `ArtSeason(world)` uses `GardenSeason`: 0 summer, 1 autumn, 2 winter,
    3 spring;
  - `MapFor` and `BannerFor` pick the art;
  - `Build` takes spring.
- Home map, story strip, creek behind the board, visits and path stones all use
  it.

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
