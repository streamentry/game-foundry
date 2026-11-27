# v1 batch 10: neighbours, items 82, 83, 84, 85, 87, 90 (ticked)

Date: 2026-11-27. Branch `feat/mosselora-v1-batch-10`, stacked on #137. Three Haiku agents worked in parallel on separate files, and the coordinator reviewed and fixed their work.

## What changed

- **82 They notice your habits, gently.** `Companion.Habit` now needs 5 visits at the same hour; it used to need 6 visits with a 60% share. The share rule let visits at other hours hide a real morning habit. The wording stays non-judging. Tested: 4 visits give no line, 5 give the line, and other hours don't count.
- **83 Each neighbour has a life arc.** `Core/NeighbourArcs`: Orin builds a boat and Fen mends the mill, in 6 steps each. The arcs advance with puzzles solved, never the clock. Each ends in a final state that stays: "The boat is in the water." and "The mill is turning again." A new step is said once, as a remark.
- **84 Neighbours ask for help.** A sweep over 3 gardens, 3 choice policies and every chapter replays public actions and proves each wish on offer can be granted from that garden. None was unsatisfiable; Fen's flower wish needs seeds from daily replays, which the game allows.
- **85 Neighbours can be quiet.** A "Speak less" setting sits next to Gentle motion (`evergrove:speakLess`) and persists at once. `Core/QuietRemarks` keeps half of remark opportunities, deterministically (49.9% over 10,000). Letters, gifts and essential notices aren't affected. The coordinator added one fix: a silenced remark no longer lights the news badge.
- **87 They mark real dates you choose.** "Dates to remember" stores up to 6 dates (month, day, kind, an optional label, no year, so no age) only in device PlayerPrefs. On the day, a neighbour marks it once, for example "Mira tied a ribbon on the gate for Nan." A privacy test proves the dates never reach the garden file, parcels or postcards. A source scan confirms the key appears only in its two files, and that the dates UI never calls share, clipboard, network or notification APIs. Caveat for the privacy notes: OS device backups may include app data.
- **90 Goodbyes that matter.** After 90 days or more away, Mira's return line is a short three-line letter that names this garden, or the last thing grown in it, with no guilt words. The coordinator wired it into the live path (`CompanionFacts.WhileAway` → `CompanionSpeech`) and added a test that 120 days away produces the letter and 30 days doesn't.

## Verification

- `.NET` suite, run as CI does: 1,795,737 assertions pass.
- Unity 6.3 runtime compile: 0 errors with both the release and dev define sets.
- `verify_evergrove_project.py` and `check_evergrove_docs.py`: OK.
- **Not run:** editor tour and device. "Speak less" uses the `IconChat` sprite, which hasn't been checked in the editor. New copy is English only.
