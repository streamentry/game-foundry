# I319 — The lookout

Ideas 23 and 15 of the [fifty](../../games/evergrove/docs/specs/proposals/world-games-50-ideas-2026-11-27.md), after the lookouts of small-island hiking games that reward the climb, and the block-building sandbox's sense of always knowing your way home.

## Player

"My river's long now. I'd love to see all of it at once, and know where the cottage is."

## PM decision

The cottage card (I299) gains one more round button, to look out over the whole river. It shows the whole garden in one view, cottage at the top and the newest stretch at the foot, with everything else stepped aside as in the quiet garden (I294). A tap anywhere returns to where you were. One feature answers both ideas, the lookout and the way home, without a new button on home.

## Dev

- `GardenMapView.LookOut(on)` asks for the least width. The map's own width rule (`max(window width × scale, window height ÷ river height)`) then fits the river's whole height. Turning it off restores the scale exactly.
- `EnterQuiet(wholeRiver)` and `ExitQuiet` turn it on and off. Tour shot 17j6.

## Result

The whole river at a glance. With many stretches, things become small at that scale. It's meant for looking, not tapping, so taps return instead.
