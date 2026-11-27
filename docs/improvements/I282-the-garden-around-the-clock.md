# I282 — The garden around the clock

## PM decision
- Tours now run on a fixed clock (13:00), so every shot compares build to build
  whatever time the tour actually runs.
- At the end, the same garden is shot at 06:00, 18:00, 20:00 and 23:00, and Fen
  is asked at night.

## Dev
- `QaSetClock(hour)`; shots `20-light-06`, `20-light-18`, `20-light-20`,
  `20-light-23` and `20-fen-at-night`.

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
