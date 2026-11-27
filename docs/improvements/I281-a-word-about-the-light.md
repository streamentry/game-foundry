# I281 — A word about the light

## PM decision
- When a neighbour has nothing new and true to tell, they may notice the light.
  It never says "you're up late" and is never a reason to stay or go. For
  example:
  - Mira at dawn: "First light on the water. It's the quietest hour on the
    river."
  - Orin at golden hour: "This light makes the old stones look like honey."
  - Fen at night: "The fireflies are out. Mind the steps by the creek." In
    winter: "Not a firefly in sight in this cold. The stars make up for it."
- Plain day and morning: nothing to remark.
- Priority sits below every truthful notice (I210 on).

## Dev
- Test-first (`DayLightTests`): `DayLight.Remark(villager, phase, season)`.
  - Banned words include "late", "sleep", "bed", "should" and "!".
  - Never claims fireflies in winter.
  - Nothing by plain day.
- `EvergroveApp.HourRemark` is used in `NeighborSays` after `NeighbourNotice`.

## Result
- b296 tours complete in portrait, landscape and 150% text (89 shots each).
- b297 (two colour values changed) was re-checked in portrait only.
- Tuned on the way:
  - b295: a colour wash alone read as a muted day. The model gained a darkness
    channel (`DayTint.Dark`, up to 48% at night, tested smooth) laid under the
    wash. Night now reads as night, and things, neighbours and buttons keep
    full contrast.
  - b296: dawn read olive, too close to golden hour. Dawn is now rosy pink and
    golden hour deep amber.
- Core tests: 349,144 assertions pass.
- Not yet seen by players aged 40+ in a real evening.
