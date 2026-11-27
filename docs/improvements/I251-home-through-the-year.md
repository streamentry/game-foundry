# I251 — Home through the year

## Player
- I want to see my garden in every season without playing a hundred puzzles to
  check.

## PM decision
- The tour builds four finished gardens with 0, 4, 8 and 12 first solves and
  shoots home in each: summer, autumn, winter, spring. Every build now checks
  the whole year on screen.

## Dev
- `DevQaTour`: shots `19-summer-home`, `19-autumn-home`, `19-winter-home` and
  `19-spring-home`, built from real saves through `QaReplaceWorld`.

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
