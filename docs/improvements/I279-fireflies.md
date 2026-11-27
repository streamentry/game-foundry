# I279 — Fireflies

## PM decision
- At dusk and night, a dozen soft points of light wander slowly over the
  garden, pulsing gently. Never in winter.
- With reduced motion they stay put but still glow.
- The same evening every time (fixed seed), so it's calm, not noisy.

## Dev
- `GardenMapView.FireflyDrift` runs in the viewport above the land;
  `Fireflies` and `FirefliesMove` come from `DayLight.Fireflies(phase, season)`
  and gentle motion.

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
