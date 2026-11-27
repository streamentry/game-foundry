# I278 — The light on the garden

## Player (founder question)
- "Should we support day vs night, morning, sunset?" The model exists (I272);
  the garden still looks like noon at 9 p.m.

## PM decision
- The garden's ground wears the hour's light from the I272 model:
  - rosy at dawn;
  - clear by day;
  - amber at golden hour;
  - violet at dusk;
  - blue at night.
- The wash sits over the painting but under every thing, neighbour and play
  mark, so those keep full contrast at night. Cards, buttons and text are never
  tinted.
- Home, the story page's map, the sideways view and visits all follow your
  hour. A visit shows your hour in their season.

## Dev
- `GardenMapView.Tint` adds a "Day light" wash on the land, after the painting
  and before the things.
- `EvergroveApp.ApplyDayLight(garden)` runs before each map is built.
- `Now()` honours the tour's fixed clock.

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
