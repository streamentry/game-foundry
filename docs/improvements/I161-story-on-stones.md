# I161 — The garden's story stands on the stones you walked

Date: 2026-11-25. Branch: feat/mosselora-i127-plus.

## Player (Haiku art-director critique of b167)

"Screen 7, 'Your garden's story': **the 12-icon grid reads as an inventory tally.** Paint the bridge, tree and flowers the player built into the ending illustration…"

The ending's summary was twelve choice icons in a 6×2 grid, like an inventory screen. The idea behind it, the season you made, is good. The form was a list.

## PM decision

Keep the summary (it's the only place the whole year is seen at once) but put it **in the world's language**: each choice now **stands on the painted stepping stone of the stream it was made in**, in that stream's season (summer moss for 1–6, an amber leaf for 7–9, frost for 10–12), the same stones the player walked on the path. The grid reads as the walk through the year, with what grew at each step. Painting choices dynamically into the festival art is out of scope (a fixed Codex painting), so it's noted as a candidate.

## Dev

`EvergroveApp.ChoiceTimeline`: cells grow from 46 to 64pt tall. Each cell draws `PathStone(i, …)` across its lower third, then the choice picture above it from 20% of the height. It falls back to the old full-cell picture if the stone art is missing.

## Result (b171, iPhone 17 simulator, scripted tour `11-ending`)

`I161-story-on-stones.jpg` (gitignored), b170 compared with b171: each bridge, tree and flower bed now stands on its stone. The first row's stones are mossy and the second row's go from amber leaf to frost, so the seasons are visible in the summary itself. Tours pass (20 + 5). Verify passes.
