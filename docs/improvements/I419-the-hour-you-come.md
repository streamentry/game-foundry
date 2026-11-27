# I419: The hour you come

Run 3, batch 17.

## PM decision

A companion notices how you come, gently. Each day's first return is counted by its hour (dawn, morning, day, golden, dusk, night) **on this device only, never in the garden file**, because that file travels to family and an hour habit is nobody else's business. Once there are at least 6 returns and one hour has at least 60% of them, the keeper who would notice that hour says so, one day in seven:

- **Mira**, mornings: "Mornings suit you. You catch the dew before it goes."
- **Orin**, evenings: "Evenings suit you, I think. I've left the lamp lit."
- **Fen**, nights: "A night visitor, like our owl. It's peaceful, isn't it, with everyone asleep."

There are no streaks, no counts shown, no guilt, and no wellbeing claims.

## Dev

- `Core/Companion.cs`: `Habit(counts)`, `Noticer(habit)` and `HabitLine(counts, villager, day)`.
- The app counts in PlayerPrefs (`evergrove:hours:<garden>`, capped at 999 each) in `NoteReturn`. `NeighborSays` offers the line before the hour remark (I281).
- `CompanionTests`: thresholds; only the noticer speaks; exactly 10 days in 70; calm wording.
