# I404: Tap a visitor

Run 3, batch 14.

## PM decision

Visitors answer a tap, without words:

- the heron lifts off, stays away a few seconds, and glides back;
- the hedgehog curls into a ball, then uncurls;
- the owl blinks twice and hoots;
- the kingfisher darts down to the water.

The heron and kingfisher make a soft splash. It's discovery and delight, the small detail a large studio would give its world. With reduced motion, the change is a still pose (away, curled) and never a dart.

## Dev

- `GardenCreature` implements `IPointerClickHandler`. It has a 24-point invisible tap area on the body only, so the tree or flower beneath stays easy to tap. Taps within 2 seconds are ignored.
- `GardenCreature.Tapped` is a static hook; the app gives each visitor its voice in `OnVisitorTapped`, respecting the sound switch, with a haptic tick when gentle motion is on.

## Also in this batch: pollination follows the fruit cap

After a parallel economy session flagged it: the bees' extra fruit (I378) is now granted only when the solve itself picked fruit. Any limit on a tree's own fruit, such as a daily replay cap, therefore governs pollination too, so replays can't farm it.
