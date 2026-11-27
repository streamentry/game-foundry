# I280 — A switch for the light

## PM decision
- Settings has a sun token: "The garden's light follows your day". It's on by
  default.
- Off keeps the garden in clear daylight. That suits anyone who finds tinted
  scenes harder to see, or simply prefers noon.

## Dev
- `EvergroveApp.SettingsRow`, row 2: the `DayLightKey` preference
  (device-only, never in the save).

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
