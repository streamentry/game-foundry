# I272 — Light that follows the day: the model

## Player (founder question)
- "Should we support day vs night, morning, sunset?" Seasons turn with tending
  (I248), but the light never changes. A garden at 9 p.m. looks like noon.

## PM decision
- Yes, as cosmetic light only, following the player's own local time:
  - rosy dawn;
  - a faint warmth in the morning;
  - clear day;
  - amber golden hour;
  - violet dusk;
  - blue night.
- The vision's "no clock" rule is about progress. This gates nothing, earns
  nothing and is never saved, and a garden left alone still waits unchanged.
- Smooth: it blends minute by minute with no jumps. Night is the deepest wash
  but stays under a ceiling (36%) so the garden always reads. Cards and
  buttons are never tinted (I273).
- Fireflies at dusk and night, but not in winter.

## Dev
- Test-first (`DayLightTests`, red on the missing type):
  - `DayPhase`, `DayTint`, `DayLight.PhaseAt`, `TintAt` (keyframes, linear
    blend) and `Fireflies(phase, season)`;
  - tested every minute of the day for continuity and the ceiling;
  - night is blue, golden hour amber, dawn rosy;
  - bad input reads as midday.
- The visuals (tint, fireflies, the Settings switch, a neighbour's word) come
  after the notification batch the founder asked for next.

## Result
- Core tests pass (347,061 assertions at this step).

## Result (b294)
- Tours complete in portrait, landscape and 150% text (84 shots each).
  - The river opens (the log shows "solves before 3 (wants 3)").
  - Mira's note waits sealed on the letterbox and is listed on the family
    page (`17u3`).
  - Settings shows the garden-notes switch (`17d`).
  - The soft ask (`17d2`) has legible hours. In b293 the hour labels were
    white on paper; fixed.
  - The tour logs the note that would be scheduled, for example "Orin: Your
    wisteria would deal you a fresh board of garden plots, if you fancy one."
- Found and fixed on the way:
  - the tour had stopped reaching the river step (b290 and b291);
  - the b292 Xcode link failure (see I275).
- Core tests: 347,183 assertions pass.
- Not verified: delivery of a real notification on a device, and the iOS
  permission prompt. Not yet tried by players aged 40+.
