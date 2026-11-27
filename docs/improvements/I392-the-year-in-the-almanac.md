# I392: The year in the almanac

Run 3, batch 11.

## PM decision

The almanac gets four rows, one per season, each with its season stone and what that season gives and why. The current season is in ink and the others are faded. Players can plan ahead ("next autumn I want an orchard") without guessing, and there's no hidden mechanic.

## Dev

- `YearRows(page)` after the good-neighbour rows.
- The text comes from `SeasonalYields.Why`, so the almanac and the rules can't drift apart.
