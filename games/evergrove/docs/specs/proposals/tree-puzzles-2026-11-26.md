# Tree puzzles: every tree holds its own puzzle

**Status:** proposal; the puzzle engine (I203) is built, everything else awaits the founder.
**Source:** an idea from the founder's daughter (2026-11-26, in Vietnamese, worked out in a chat), fine-tuned here against the vision.

## Her idea, as she settled it

- Trees cost coins: 50, then 100, then 200. You must own the 50-coin tree before the 100-coin one unlocks (and so on).
- Buying a new tree keeps the old one. Each tree has its **own puzzle**: tap a tree to play it and earn that tree's reward.
- Dearer trees have harder puzzles: the 100-coin tree is about 5% harder than the 50, and the 200-coin tree about 10% harder than the 100.
- A special 250-coin tree: as hard as the 200-coin tree, pays 200 coins per solve, can only be bought at weekends but is used all week.
- Every tree has its own shape and colour; the dearer, the more beautiful. Buying a tree gives you both a new puzzle to play and a prettier garden to look at.

## What we keep (it fits Mosselora very well)

1. **Your garden is where you choose what to play.** Tap anything you've grown and play its puzzle. This makes the open world the menu, and it gives endless, calm play with no new art per level: the strongest answer yet to "something adults 40+ play every day" that doesn't break the vision's "no streaks, no clock".
2. **A ladder of gently harder puzzles**, unlocked in order. Her "5%" and "10%" become concrete steps a player can feel: everyday 3×3; a little harder 4×4 with some tiles already right; harder 4×4 with every tile turned.
3. **Nicer things as the reward.** Each tier's plants look more special: a different shape and colour, then a bloom or glow on the ones you've solved.
4. **Something saved up for.** Solving tree puzzles earns something you spend on new kinds of plants.

## What we change, and why

| Her rule | Change | Why |
|---|---|---|
| Coins | **Seeds** (or water drops), earned only by playing, never sold | The vision rejects real-money economies and pressure; seeds fit the garden and stay calm |
| Exact 5% / 10% harder | Concrete steps (board size, tiles already right) | A percentage can't be felt or tested; these can, and the tests check every board has exactly one answer |
| Weekend-only special tree | **Unlocked by progress or given by family** (e.g. when a grandchild joins your garden, or after finishing a stretch of river) | Weekend-only is a wall-clock gate and fake urgency, both banned by the vision (§ principles 6) |
| Special tree pays 200 per solve | Keep a "best" tree, but don't let one tree become the only one worth playing | Otherwise players replay one board forever; better to reward a fresh tree's first solve more |

## Plan

- **I203 (done):** `Core/TreePuzzles`: makes a puzzle from a seed and a tier; the same seed gives the same board on every device; tested over 360 boards for exactly one answer, never starting solved, and harder at the top tier.
- **I204:** tap something you've grown at home, then choose to play its puzzle. Each item's seed comes from its place in your story; its tier comes from the stream it was grown in. Solving it makes the thing bloom. It's practice only and never changes your story.
- **Later, after the founder decides:** seeds as an earned reward, a nursery in the basket with tiers that unlock in order, prettier art per tier, and the family/special tree.

## Open questions for the founder

1. Seeds as a currency at all, or only "solve it and it blooms"?
2. The special tree: a family gift (fits the family goal, vision §1a) or a progress reward?
3. Name the feature for players: "Tree puzzles", "Garden puzzles", or no name, just a play button on each tree?
